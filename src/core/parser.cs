using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public static class Parser {
    public static Obj ParseSymb(Obj obj) {
      string str = obj.GetString();
      int id = SymbTable.StrToIdx(str);
      return SymbObj.Get(id);
    }

    public static Obj Parse(Obj text) {
      byte[] bytes = text.GetInnerObj().GetByteArray();
      Obj obj;
      long errorOffset;
      bool ok = Parse(bytes, out obj, out errorOffset);
      if (ok)
        return new TaggedObj(SymbTable.SuccessSymbId, obj);
      else
        return new TaggedObj(SymbTable.FailureSymbId, IntObj.Get(errorOffset));
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

    static long ReadNat(byte[] text, uint length, ref long offset) {
      long startOffset = offset;
      long endOffset = startOffset;
      long value = 0;
      byte ch;
      while (endOffset < length && Char.IsDigit((char)(ch = text[endOffset]))) {
        value = 10 * value + (ch - '0');
        endOffset++;
      }
      Miscellanea.Assert(endOffset > startOffset);
      long count = endOffset - startOffset;
      if (count > 19) {
        offset = -startOffset - 1;
        return -1;
      }
      else if (count == 19) {
        const string MAX = "9223372036854775807";
        for (int i=0 ; i < 19 ; i++) {
          ch = text[startOffset + i];
          byte maxCh = (byte) MAX[i];
          if (ch > maxCh) {
            offset = -startOffset - 1;
            return -1;
          }
          else if (ch < maxCh)
            break;
        }
      }
      offset = endOffset;
      return value;
    }


    static long ReadNumber(byte[] text, uint length, long offset, Token token, bool negate) {
      byte ch;

      long i = offset;

      long intValue = ReadNat(text, length, ref i);
      if (i < 0)
        return i;

      bool isInt;
      if (i == length) {
        isInt = true;
        ch = 0; // Shutting up the compiler
      }
      else {
        ch = text[i];
        isInt = ch != '.' & !Char.IsLower((char)ch);
        Miscellanea.Assert(!Char.IsDigit((char)ch));
      }

      if (isInt) {
        if (token != null) {
          token.offset = offset;
          token.length = i - offset;
          token.type = TokenType.Int;
          token.value = negate ? -intValue : intValue;
        }
        return i;
      }

      double floatValue = intValue;
      if (ch == '.') {
        long start = ++i;
        long DecIntValue = ReadNat(text, length, ref i);
        if (i < 0)
          return i;
        floatValue += ((double) DecIntValue) / Math.Pow(10, i - start);
      }

      if (i < length) {
        ch = text[i];
        if (ch == 'e') {
          if (++i == length)
            return -i - 1;
          ch = text[i];

          bool negExp = false;
          if (ch == '-') {
            if (++i == length)
              return -i - 1;
            ch = text[i];
            negExp = true;
          }

          if (!Char.IsDigit((char)ch))
            return -i - 1;

          long expValue = ReadNat(text, length, ref i);
          if (i < 0)
            return i;

          floatValue *= Math.Pow(10, negExp ? -expValue : expValue);
        }

        if (Char.IsLower((char)ch))
          return -i - 1;
      }

      if (token != null) {
        token.offset = offset;
        token.length = i - offset;
        token.type = TokenType.Float;
        token.value = negate ? -floatValue : floatValue;
      }
      return i;
    }


    static long ReadSymbol(byte[] text, uint length, long offset, Token token) {
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
        token.value = SymbObj.Get(SymbTable.StrToIdx(new string(chars)));
      }

      return i;
    }


    static long ReadString(byte[] text, uint length, long offset, Token token) {
      uint strLen = 0;
      for (long i=offset+1 ; i < length ; i++) {
        byte ch = text[i];

        if (ch < ' ' | ch > '~')
          return -offset - 1;

        if (ch == '"') {
          if (token != null) {
            char[] chars = new char[strLen];
            int nextIdx = 0;
            for (int j=0 ; j < i-offset-1 ; j++) {
              char currChar = (char) text[offset+j+1];
              if (currChar == '\\') {
                j++;
                currChar = (char) text[offset + j + 1];
                if (currChar == '\\' | currChar == '"') {
                  // Nothing to do here
                }
                else if (currChar == 'n') {
                  currChar = '\n';
                }
                else if (currChar == 't') {
                  currChar = '\t';
                }
                else {
                  currChar = (char) (
                    4092 * Miscellanea.HexDigitValue(ch) +
                     256 * Miscellanea.HexDigitValue(text[j+1]) +
                      16 * Miscellanea.HexDigitValue(text[j+2]) +
                           Miscellanea.HexDigitValue(text[j+3])
                  );
                  j += 3;
                }
              }
              chars[nextIdx++] = currChar;
            }
            Miscellanea.Assert(nextIdx == strLen);

            token.offset = offset;
            token.length = i + 1 - offset;
            token.type = TokenType.String;
            token.value = new string(chars);
          }
          return i + 1;
        }

        strLen++;

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


    static long Tokenize(byte[] text, uint length, Token[] tokens) {
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
          offset = ReadNumber(text, length, offset, token, negate);
          if (offset < 0)
            return offset;
          else
            continue;
        }

        // Symbols
        if (ch >= 'a' && ch <= 'z') {
          offset = ReadSymbol(text, length, offset, token);
          if (offset < 0)
            return offset;
          else
            continue;
        }

        // Strings
        if (ch == '"') {
          offset = ReadString(text, length, offset, token);
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

      public void Store(Obj obj) {
        if (cols.Length != 1)
          Console.WriteLine("cols.Length = " + cols.Length.ToString());
        Miscellanea.Assert(cols.Length == 1);
        cols[0].Add(obj);
      }

      public void Store(Obj[] objs, uint size) {
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

    static long ReadList(Token[] tokens, long offset, TokenType sep, TokenType term, TokenParser parseElem, State state) {
      int length = tokens.Length;

      // Empty list
      if (offset < length && tokens[offset].type == term)
        return offset + 1;

      for ( ; ; ) {
        offset = parseElem(tokens, offset, state);

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

    static long ParseEntry(Token[] tokens, long offset, uint count, TokenType sep, Obj[] vars) {
      int length = tokens.Length;

      uint read = 0;

      for (read = 0 ; read < count ; read++) {
        if (read > 0)
          if (offset < length && tokens[offset].type == sep)
            offset++;
          else
            break;
        offset = ParseObj(tokens, offset, out vars[read]);
        if (offset < 0)
          break;
      }

      if (read == count & offset < length)
        return offset;

      return offset < 0 ? offset : -offset - 1;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long ReadObj(Token[] tokens, long offset, State state) {
      Obj obj;
      offset = ParseObj(tokens, offset, out obj);
      if (offset >= 0)
        state.Store(obj);
      return offset;
    }

    static long ReadEntry(Token[] tokens, long offset, State state, uint size, TokenType sep) {
      Obj[] entry = new Obj[3];
      offset = ParseEntry(tokens, offset, size, sep, entry);
      if (offset >= 0)
        state.Store(entry, size);
      return offset;
    }

    static long ReadMapEntry(Token[] tokens, long offset, State state) {
      return ReadEntry(tokens, offset, state, 2, TokenType.Arrow);
    }

    static long ReadBinRelEntry(Token[] tokens, long offset, State state) {
      return ReadEntry(tokens, offset, state, 2, TokenType.Comma);
    }

    static long ReadTernRelEntry(Token[] tokens, long offset, State state) {
      return ReadEntry(tokens, offset, state, 3, TokenType.Comma);
    }

    static long ReadRecEntry(Token[] tokens, long offset, State state) {
      int length = tokens.Length;
      if (offset >= length || tokens[offset].type != TokenType.Symbol)
        return -offset - 1;
      SymbObj label = (SymbObj) tokens[offset++].value;
      if (offset >= length || tokens[offset].type != TokenType.Colon)
        return -offset - 1;
      Obj[] entry = new Obj[2];
      offset = ParseObj(tokens, offset+1, out entry[1]);
      if (offset < 0)
        return offset;
      entry[0] = label;
      state.Store(entry, 2);
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long ParseSeq(Token[] tokens, long offset, out Obj var) {
      State state = new State(1);
      offset = ReadList(tokens, offset+1, TokenType.Comma, TokenType.ClosePar, ReadObj, state);
      if (offset >= 0)
        var = Builder.CreateSeq(state.cols[0]);
      else
        var = null;
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static bool IsRecord(Token[] tokens, long offset) {
      return offset + 2 < tokens.Length && tokens[offset+1].type == TokenType.Symbol && tokens[offset+2].type == TokenType.Colon;
    }

    static long ParseRec(Token[] tokens, long offset, out Obj var) {
      State state = new State(2);
      offset = ReadList(tokens, offset+1, TokenType.Comma, TokenType.ClosePar, ReadRecEntry, state);
      if (offset >= 0)
        var = Builder.CreateBinRel(state.cols[0], state.cols[1]);
      else
        var = null;
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long ParseInnerObjOrTuple(Token[] tokens, long offset, out Obj var) {
      State state = new State(1);
      offset = ReadList(tokens, offset+1, TokenType.Comma, TokenType.ClosePar, ReadObj, state);
      bool ok = offset >= 0 & state.Count() > 0;
      if (ok)
        var = state.Count() == 1 ? state.cols[0][0] : Builder.CreateSeq(state.cols[0]);
      else
        var = null;
      return offset;
    }

    static long ParseSymbOrTaggedObj(Token[] tokens, long offset, out Obj var) {
      int length = tokens.Length;
      SymbObj symbObj = (SymbObj) tokens[offset].value;
      int symbIdx = symbObj.GetSymbId();
      if (++offset < length) {
        if (tokens[offset].type == TokenType.OpenPar) {
          Obj innerObj;
          if (IsRecord(tokens, offset))
            offset = ParseRec(tokens, offset, out innerObj);
          else
            offset = ParseInnerObjOrTuple(tokens, offset, out innerObj);
          if (offset >= 0)
            var = new TaggedObj(symbIdx, innerObj);
          else
            var = null;
          return offset;
        }
      }
      var = symbObj;
      return offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    static long ParseRelTail(Token[] tokens, long offset, uint size, Obj[] firstEntry, bool isMap, out Obj var) {
      State state = new State(size);
      state.Store(firstEntry, size);

      TokenParser entryParser;
      if (size == 2)
        if (isMap)
          entryParser = ReadMapEntry;
        else
          entryParser = ReadBinRelEntry;
      else
        entryParser = ReadTernRelEntry;

      offset = ReadList(tokens, offset, isMap ? TokenType.Comma : TokenType.Semicolon, TokenType.CloseBracket, entryParser, state);

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

    static long ParseUnordColl(Token[] tokens, long offset, out Obj var) {
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

      offset = ReadList(tokens, offset, TokenType.Comma, TokenType.Whatever, ReadObj, state);
      if (offset < 0) {
        var = null;
        return offset;
      }

      TokenType type = tokens[offset++].type;

      int count = state.Count();
      bool isMap = type == TokenType.Arrow & count == 1;
      bool isRel = type == TokenType.Semicolon & (count == 2 | count == 3);

      if (isMap) {
        offset = ReadObj(tokens, offset, state);
        if (offset >= length)
          offset = -offset - 1;
        if (offset < 0) {
          var = null;
          return offset;
        }
        type = tokens[offset++].type;
      }
      else if (isRel && offset < length && tokens[offset].type == TokenType.CloseBracket) {
        type = TokenType.CloseBracket;
        offset++;
      }

      if (type == TokenType.CloseBracket) {
        count = state.Count();
        if (isMap | (isRel & count == 2))
          var = Builder.CreateBinRel(state.cols[0][0], state.cols[0][1]);
        else if (isRel)
          var = Builder.CreateTernRel(state.cols[0][0], state.cols[0][1], state.cols[0][2]);
        else
          var = Builder.CreateSet(state.cols[0]);
        return offset;
      }

      if (isMap | isRel) {
        Obj[] entry = new Obj[3];
        for (int i=0 ; i < state.Count() ; i++)
          entry[i] = state.cols[0][i];
        return ParseRelTail(tokens, offset, (uint) state.Count(), entry, isMap, out var);
      }

      var = null;
      return -offset;
    }

    ////////////////////////////////////////////////////////////////////////////////

    // If the function is successfull, it returns the index of the next token to consume
    // If it fails, it returns the location/index of the error, negated and decremented by one
    static long ParseObj(Token[] tokens, long offset, out Obj var) {
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
          var = IntObj.Get((long) token.value);
          return offset + 1;

        case TokenType.Float:
          var = new FloatObj((double) token.value);
          return offset + 1;

        case TokenType.Symbol:
          return ParseSymbOrTaggedObj(tokens, offset, out var);

        case TokenType.OpenPar:
          if (IsRecord(tokens, offset))
            return ParseRec(tokens, offset, out var);
          else
            return ParseSeq(tokens, offset, out var);

        case TokenType.OpenBracket:
          return ParseUnordColl(tokens, offset, out var);

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

    public static bool Parse(byte[] text, out Obj var, out long errorOffset) {
      uint size = (uint) text.Length;

      long count = Tokenize(text, size, null);
      if (count <= 0) {
        var = null;
        errorOffset = count < 0 ? -count - 1 : size;
        return false;
      }

      Token[] tokens = new Token[count];
      for (int i=0 ; i < count ; i++)
        tokens[i] = new Token();
      Tokenize(text, size, tokens);

      long res = ParseObj(tokens, 0, out var);
      if (res < 0 | res < count) {
        errorOffset = res < 0 ? tokens[-res-1].offset : size;
        return false;
      }
      else {
        errorOffset = 0;
        return true;
      }
    }
  }
}
