using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public static class Parser {
    public static Obj ParseSymb(Obj obj) {
      string str = obj.GetString();
      int id = SymbTable.StrToIdx(str);
      return new SymbObj(id);
    }

    public static Obj Parse(Obj text) {
      byte[] bytes = text.GetInnerObj().GetByteArray();
      Obj obj;
      long error_offset;
      bool ok = parse(bytes, out obj, out error_offset);
      if (ok)
        return new TaggedObj(SymbTable.SuccessSymbId, obj);
      else
        return new TaggedObj(SymbTable.FailureSymbId, new IntObj(error_offset));
    }

    ////////////////////////////////////////////////////////////////////////////

    enum TokenType {
      Comma,
      Colon,
      Semicolon,
      Arrow,
      OpenPar,
      ClosePar,
      OpenBracket,
      CloseBracket,
      Int,
      Float,
      Symbol,
      String,
      Whatever
    };

    class Token {
      public long offset;
      public long length;
      public TokenType type;
      public object value; // long, double, int (symbol), string
    };

    ////////////////////////////////////////////////////////////////////////////////

    static long read_nat(byte[] text, uint length, ref long offset) {
      long start_offset = offset;
      long end_offset = start_offset;
      long value = 0;
      byte ch;
      while (end_offset < length && Char.IsDigit((char)(ch = text[end_offset]))) {
        value = 10 * value + (ch - '0');
        end_offset++;
      }
      Miscellanea.Assert(end_offset > start_offset);
      long count = end_offset - start_offset;
      if (count > 19) {
        offset = -start_offset - 1;
        return -1;
      }
      else if (count == 19) {
        const string MAX = "9223372036854775807";
        for (int i=0 ; i < 19 ; i++) {
          ch = text[start_offset + i];
          byte max_ch = (byte) MAX[i];
          if (ch > max_ch) {
            offset = -start_offset - 1;
            return -1;
          }
          else if (ch < max_ch)
            break;
        }
      }
      offset = end_offset;
      return value;
    }


    static long read_number(byte[] text, uint length, long offset, Token token, bool negate) {
      byte ch;

      long i = offset;

      long int_value = read_nat(text, length, ref i);
      if (i < 0)
        return i;

      bool is_int;
      if (i == length) {
        is_int = true;
        ch = 0; // Shutting up the compiler
      }
      else {
        ch = text[i];
        is_int = ch != '.' & !Char.IsLower((char)ch);
        Miscellanea.Assert(!Char.IsDigit((char)ch));
      }

      if (is_int) {
        if (token != null) {
          token.offset = offset;
          token.length = i - offset;
          token.type = TokenType.Int;
          token.value = negate ? -int_value : int_value;
        }
        return i;
      }

      double float_value = int_value;
      if (ch == '.') {
        long start = ++i;
        long dec_int_value = read_nat(text, length, ref i);
        if (i < 0)
          return i;
        float_value += ((double) dec_int_value) / Math.Pow(10, i - start);
      }

      if (i < length) {
        ch = text[i];
        if (ch == 'e') {
          if (++i == length)
            return -i - 1;
          ch = text[i];

          bool neg_exp = false;
          if (ch == '-') {
            if (++i == length)
              return -i - 1;
            ch = text[i];
            neg_exp = true;
          }

          if (!Char.IsDigit((char)ch))
            return -i - 1;

          long exp_value = read_nat(text, length, ref i);
          if (i < 0)
            return i;

          float_value *= Math.Pow(10, neg_exp ? -exp_value : exp_value);
        }

        if (Char.IsLower((char)ch))
          return -i - 1;
      }

      if (token != null) {
        token.offset = offset;
        token.length = i - offset;
        token.type = TokenType.Float;
        token.value = negate ? -float_value : float_value;
      }
      return i;
    }


    static long read_symbol(byte[] text, uint length, long offset, Token token) {
      long i = offset;
      while (++i < length) {
        byte ch = text[i];
        if (ch == '_') {
          if (++i == length)
            return -i - 1;
          ch = text[i];
          if (!Char.IsLower((char)ch) & !Char.IsDigit((char)ch))
            return -i - 1;
        }
        else if (!Char.IsLower((char)ch) & !Char.IsDigit((char)ch))
          break;
      }

      if (token != null) {
        long len = i - offset;
        char[] chars = new char[len];
        for (int j=0 ; j < len ; j++)
          chars[j] = (char) text[offset+j];

        token.offset = offset;
        token.length = len;
        token.type = TokenType.Symbol;
        token.value = new SymbObj(SymbTable.StrToIdx(new string(chars)));
      }

      return i;
    }


    static long read_string(byte[] text, uint length, long offset, Token token) {
      uint str_len = 0;
      for (long i=offset+1 ; i < length ; i++) {
        byte ch = text[i];

        if (ch < ' ' | ch > '~')
          return -offset - 1;

        if (ch == '"') {
          if (token != null) {
            char[] chars = new char[str_len];
            for (int j=0 ; j < str_len ; j++)
              chars[j] = (char) text[offset+j+1];

            token.offset = offset;
            token.length = i + 1 - offset;
            token.type = TokenType.String;
            token.value = new string(chars);
          }
          return i + 1;
        }

        str_len++;

        if (ch == '\\') {
          if (++i == length)
            return -i - 1;
          ch = text[i];
          if (Miscellanea.IsHexDigit(ch)) {
            if (i + 3 >= length || !(Miscellanea.IsHexDigit(text[i+1]) & Miscellanea.IsHexDigit(text[i+2]) & Miscellanea.IsHexDigit(text[i+3])))
              return -i;
            i += 3;
          }
          else if (ch != '\\' & ch != '"' & ch != 'n' & ch != 't')
            return -i;
        }
      }
      return -(length + 1);
    }


    static long tokenize(byte[] text, uint length, Token[] tokens) {
      uint index = 0;
      long offset = 0;

      while (offset < length) {
        byte ch = text[offset];

        if (Char.IsWhiteSpace((char) ch)) {
          offset++;
          continue;
        }

        Token token = tokens != null ? tokens[index] : null;
        index++;

        bool negate = false;
        if (ch == '-') {
          if (offset + 1 == length)
            return -offset - 1;

          offset++;
          ch = text[offset];

          // Arrow
          if (ch == '>') {
            if (token != null) {
              token.offset = offset - 1;
              token.length = 2;
              token.type = TokenType.Arrow;
            }
            offset++;
            continue;
          }

          if (!Char.IsDigit((char)ch))
            return -offset - 2;

          negate = true;
        }

        // Integer and floating point numbers
        if (ch >= '0' && ch <= '9') {
          offset = read_number(text, length, offset, token, negate);
          if (offset < 0)
            return offset;
          else
            continue;
        }

        // Symbols
        if (ch >= 'a' && ch <= 'z') {
          offset = read_symbol(text, length, offset, token);
          if (offset < 0)
            return offset;
          else
            continue;
        }

        // Strings
        if (ch == '"') {
          offset = read_string(text, length, offset, token);
          if (offset < 0)
            return offset;
          else
            continue;
        }

        // Single character tokens
        TokenType type;
        switch ((char) ch) {
          case ',':
            type = TokenType.Comma;
            break;

          case ':':
            type = TokenType.Colon;
            break;

          case ';':
            type = TokenType.Semicolon;
            break;

          case '(':
            type = TokenType.OpenPar;
            break;

          case ')':
            type = TokenType.ClosePar;
            break;

          case '[':
            type = TokenType.OpenBracket;
            break;

          case ']':
            type = TokenType.CloseBracket;
            break;

          default:
            return -offset - 1;
        }

        if (token != null) {
          token.offset = offset;
          token.length = 1;
          token.type = type;
        }

        offset++;
      }

      return index;
    }

    ////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////

    class State {
      public List<Obj>[] cols;

      public State(uint arity) {
        Miscellanea.Assert(arity >= 0 & arity <= 3);
        cols = new List<Obj>[arity];
        for (int i=0 ; i < arity ; i++)
          cols[i] = new List<Obj>();
      }

      public void store(Obj obj) {
        if (cols.Length != 1)
          Console.WriteLine("cols.Length = " + cols.Length.ToString());
        Miscellanea.Assert(cols.Length == 1);
        cols[0].Add(obj);
      }

      public void store(Obj[] objs, uint size) {
        Miscellanea.Assert(cols.Length == size);
        for (int i=0 ; i < size ; i++)
          cols[i].Add(objs[i]);
      }

      public int Count() {
        return cols[0].Count;
      }
    }

    ////////////////////////////////////////////////////////////////////////////////

    delegate long TokenParser(Token[] tokens, long offset, State state);

    static long read_list(Token[] tokens, long offset, TokenType sep, TokenType term, TokenParser parse_elem, State state) {
      int length = tokens.Length;

      // Empty list
      if (offset < length && tokens[offset].type == term)
        return offset + 1;

      for ( ; ; ) {
        offset = parse_elem(tokens, offset, state);

        // Unexpected EOF
        if (offset >= length)
          offset = -offset - 1;

        // Parsing failed
        if (offset < 0)
          return offset;

        TokenType type = tokens[offset++].type;

        // One more item
        if (type == sep)
          continue;

        // Done
        if (type == term)
          return offset;

        // Done
        if (term == TokenType.Whatever)
          return offset - 1;

        // Unexpected separator/terminator
        return -offset;
      }
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long parse_entry(Token[] tokens, long offset, uint count, TokenType sep, Obj[] vars) {
      int length = tokens.Length;

      uint read = 0;

      for (read = 0 ; read < count ; read++) {
        if (read > 0)
          if (offset < length && tokens[offset].type == sep)
            offset++;
          else
            break;
        offset = parse_obj(tokens, offset, out vars[read]);
        if (offset < 0)
          break;
      }

      if (read == count & offset < length)
        return offset;

      return offset < 0 ? offset : -offset - 1;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long read_obj(Token[] tokens, long offset, State state) {
      Obj obj;
      offset = parse_obj(tokens, offset, out obj);
      if (offset >= 0)
        state.store(obj);
      return offset;
    }

    static long read_entry(Token[] tokens, long offset, State state, uint size, TokenType sep) {
      Obj[] entry = new Obj[3];
      offset = parse_entry(tokens, offset, size, sep, entry);
      if (offset >= 0)
        state.store(entry, size);
      return offset;
    }

    static long read_map_entry(Token[] tokens, long offset, State state) {
      return read_entry(tokens, offset, state, 2, TokenType.Arrow);
    }

    static long read_bin_rel_entry(Token[] tokens, long offset, State state) {
      return read_entry(tokens, offset, state, 2, TokenType.Comma);
    }

    static long read_tern_rel_entry(Token[] tokens, long offset, State state) {
      return read_entry(tokens, offset, state, 3, TokenType.Comma);
    }

    static long read_rec_entry(Token[] tokens, long offset, State state) {
      int length = tokens.Length;
      if (offset >= length || tokens[offset].type != TokenType.Symbol)
        return -offset - 1;
      SymbObj label = (SymbObj) tokens[offset++].value;
      if (offset >= length || tokens[offset].type != TokenType.Colon)
        return -offset - 1;
      Obj[] entry = new Obj[2];
      offset = parse_obj(tokens, offset+1, out entry[1]);
      if (offset < 0)
        return offset;
      entry[0] = label;
      state.store(entry, 2);
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long parse_seq(Token[] tokens, long offset, out Obj var) {
      State state = new State(1);
      offset = read_list(tokens, offset+1, TokenType.Comma, TokenType.ClosePar, read_obj, state);
      if (offset >= 0)
        var = Builder.CreateSeq(state.cols[0]);
      else
        var = null;
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static bool is_record(Token[] tokens, long offset) {
      return offset + 2 < tokens.Length && tokens[offset+1].type == TokenType.Symbol && tokens[offset+2].type == TokenType.Colon;
    }

    static long parse_rec(Token[] tokens, long offset, out Obj var) {
      State state = new State(2);
      offset = read_list(tokens, offset+1, TokenType.Comma, TokenType.ClosePar, read_rec_entry, state);
      if (offset >= 0)
        var = Builder.CreateBinRel(state.cols[0], state.cols[1]);
      else
        var = null;
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long parse_inner_obj_or_tuple(Token[] tokens, long offset, out Obj var) {
      State state = new State(1);
      offset = read_list(tokens, offset+1, TokenType.Comma, TokenType.ClosePar, read_obj, state);
      bool ok = offset >= 0 & state.Count() > 0;
      if (ok)
        var = state.Count() == 1 ? state.cols[0][0] : Builder.CreateSeq(state.cols[0]);
      else
        var = null;
      return offset;
    }

    static long parse_symb_or_tagged_obj(Token[] tokens, long offset, out Obj var) {
      int length = tokens.Length;
      SymbObj symb_obj = (SymbObj) tokens[offset].value;
      int symb_idx = symb_obj.GetSymbId();
      if (++offset < length) {
        if (tokens[offset].type == TokenType.OpenPar) {
          Obj inner_obj;
          if (is_record(tokens, offset))
            offset = parse_rec(tokens, offset, out inner_obj);
          else
            offset = parse_inner_obj_or_tuple(tokens, offset, out inner_obj);
          if (offset >= 0)
            var = new TaggedObj(symb_idx, inner_obj);
          else
            var = null;
          return offset;
        }
      }
      var = symb_obj;
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long parse_rel_tail(Token[] tokens, long offset, uint size, Obj[] first_entry, bool is_map, out Obj var) {
      State state = new State(size);
      state.store(first_entry, size);

      TokenParser entry_parser;
      if (size == 2)
        if (is_map)
          entry_parser = read_map_entry;
        else
          entry_parser = read_bin_rel_entry;
      else
        entry_parser = read_tern_rel_entry;

      offset = read_list(tokens, offset, is_map ? TokenType.Comma : TokenType.Semicolon, TokenType.CloseBracket, entry_parser, state);

      if (offset >= 0)
        if (size == 2)
          var = Builder.CreateBinRel(state.cols[0], state.cols[1]);
        else
          var = Builder.CreateTernRel(state.cols[0], state.cols[1], state.cols[2]);
      else
        var = null;

      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long parse_unord_coll(Token[] tokens, long offset, out Obj var) {
      int length = tokens.Length;

      if (++offset >= length) {
        var = null;
        return -offset - 1;
      }

      if (tokens[offset].type == TokenType.CloseBracket) {
        var = EmptyRelObj.Singleton();
        return offset + 1;
      }

      State state = new State(1);

      offset = read_list(tokens, offset, TokenType.Comma, TokenType.Whatever, read_obj, state);
      if (offset < 0) {
        var = null;
        return offset;
      }

      TokenType type = tokens[offset++].type;

      int count = state.Count();
      bool is_map = type == TokenType.Arrow & count == 1;
      bool is_rel = type == TokenType.Semicolon & (count == 2 | count == 3);

      if (is_map) {
        offset = read_obj(tokens, offset, state);
        if (offset >= length)
          offset = -offset - 1;
        if (offset < 0) {
          var = null;
          return offset;
        }
        type = tokens[offset++].type;
      }
      else if (is_rel && offset < length && tokens[offset].type == TokenType.CloseBracket) {
        type = TokenType.CloseBracket;
        offset++;
      }

      if (type == TokenType.CloseBracket) {
        count = state.Count();
        if (is_map | (is_rel & count == 2))
          var = Builder.CreateBinRel(state.cols[0][0], state.cols[0][1]);
        else if (is_rel)
          var = Builder.CreateTernRel(state.cols[0][0], state.cols[0][1], state.cols[0][2]);
        else
          var = Builder.CreateSet(state.cols[0]);
        return offset;
      }

      if (is_map | is_rel) {
        Obj[] entry = new Obj[3];
        for (int i=0 ; i < state.Count() ; i++)
          entry[i] = state.cols[0][i];
        return parse_rel_tail(tokens, offset, (uint) state.Count(), entry, is_map, out var);
      }

      var = null;
      return -offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    // If the function is successfull, it returns the index of the next token to consume
    // If it fails, it returns the location/index of the error, negated and decremented by one
    static long parse_obj(Token[] tokens, long offset, out Obj var) {
      int length = tokens.Length;

      if (offset >= length) {
        var = null;
        return -offset - 1;
      }

      Token token = tokens[offset];

      switch (token.type) {
        case TokenType.Comma:
        case TokenType.Colon:
        case TokenType.Semicolon:
        case TokenType.Arrow:
        case TokenType.ClosePar:
        case TokenType.CloseBracket:
          var = null;
          return -offset - 1;

        case TokenType.Int:
          var = new IntObj((long) token.value);
          return offset + 1;

        case TokenType.Float:
          var = new FloatObj((double) token.value);
          return offset + 1;

        case TokenType.Symbol:
          return parse_symb_or_tagged_obj(tokens, offset, out var);

        case TokenType.OpenPar:
          if (is_record(tokens, offset))
            return parse_rec(tokens, offset, out var);
          else
            return parse_seq(tokens, offset, out var);

        case TokenType.OpenBracket:
          return parse_unord_coll(tokens, offset, out var);

        case TokenType.String:
          var = Miscellanea.StrToObj((string) token.value);
          return offset + 1;

        default:
          var = null;
          throw new InvalidOperationException(); // Unreachable code
      }
    }

    ////////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////////

    static bool parse(byte[] text, out Obj var, out long error_offset) {
      uint size = (uint) text.Length;

      long count = tokenize(text, size, null);
      if (count <= 0) {
        var = null;
        error_offset = count < 0 ? -count - 1 : size;
        return false;
      }

      Token[] tokens = new Token[count];
      for (int i=0 ; i < count ; i++)
        tokens[i] = new Token();
      tokenize(text, size, tokens);

      long res = parse_obj(tokens, 0, out var);
      if (res < 0 | res < count) {
        error_offset = res < 0 ? tokens[-res-1].offset : size;
        return false;
      }
      else {
        error_offset = 0;
        return true;
      }
    }
  }
}
