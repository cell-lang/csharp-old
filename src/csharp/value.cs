using System;
using System.IO;
using System.Text;
using System.Collections.Generic;


namespace CellLang {
  public interface Value {
    bool IsSymb();
    bool IsInt();
    bool IsFloat();
    bool IsSeq();
    bool IsSet();
    bool IsBinRel();
    bool IsTernRel();
    bool IsTagged();

    string AsSymb();
    long   AsLong();
    double AsDouble();

    int Size();
    Value Item(int index);
    void Entry(int index, out Value field1, out Value field2);
    void Entry(int index, out Value field1, out Value field2, out Value field3);

    string Tag();
    Value Untagged();

    bool IsString();
    bool IsRecord();

    string AsString();
    Value Lookup(string field);

    string Printed();
    void Print(StreamWriter writer);
  }


  class ValueBase : Value {
    public virtual bool IsSymb() {
      return false;
    }

    public virtual bool IsInt() {
      return false;
    }

    public virtual bool IsFloat() {
      return false;
    }

    public virtual bool IsSeq() {
      return false;
    }

    public virtual bool IsSet() {
      return false;
    }

    public virtual bool IsBinRel() {
      return false;
    }

    public virtual bool IsTernRel() {
      return false;
    }

    public virtual bool IsTagged() {
      return false;
    }

    public virtual string AsSymb() {
      throw new NotImplementedException();
    }

    public virtual long AsLong() {
      throw new NotImplementedException();
    }

    public virtual double AsDouble() {
      throw new NotImplementedException();
    }

    public virtual int Size() {
      throw new NotImplementedException();
    }

    public virtual Value Item(int index) {
      throw new NotImplementedException();
    }

    public virtual void Entry(int index, out Value field1, out Value field2) {
      throw new NotImplementedException();
    }

    public virtual void Entry(int index, out Value field1, out Value field2, out Value field3) {
      throw new NotImplementedException();
    }

    public virtual string Tag() {
      throw new NotImplementedException();
    }

    public virtual Value Untagged() {
      throw new NotImplementedException();
    }

    public virtual bool IsString() {
      return false;
    }

    public virtual bool IsRecord() {
      return false;
    }

    public virtual string AsString() {
      throw new NotImplementedException();
    }

    public virtual Value Lookup(string field) {
      throw new NotImplementedException();
    }

    public virtual string Printed() {
      throw new NotImplementedException();
    }

    public virtual void Print(StreamWriter writer) {
      writer.Write(Printed());
    }

    override public string ToString() {
      return Printed();
    }
  }


  class SymbValue : ValueBase {
    int id;

    public SymbValue(int id) {
      this.id = id;
    }

    override public bool IsSymb() {
      return true;
    }

    override public string AsSymb() {
      return SymbTable.IdxToStr(id);
    }

    override public string Printed() {
      return AsSymb();
    }
  }


  class IntValue : ValueBase {
    long value;

    public IntValue(long value) {
      this.value = value;
    }

    override public bool IsInt() {
      return true;
    }

    override public long AsLong() {
      return value;
    }

    override public string Printed() {
      return value.ToString();
    }
  }


  class FloatValue : ValueBase {
    double value;

    public FloatValue(double value) {
      this.value = value;
    }

    override public bool IsFloat() {
      return true;
    }

    override public double AsDouble() {
      return value;
    }

    override public string Printed() {
      return value.ToString();
    }
  }


  class SeqValue : ValueBase {
    Value[] values;

    public SeqValue(Value[] values) {
      this.values = values;
    }

    override public bool IsSeq() {
      return true;
    }

    override public int Size() {
      return values.Length;
    }

    override public Value Item(int index) {
      return values[index];
    }

    override public string Printed() {
      StringBuilder builder = new StringBuilder();
      builder.Append("(");
      for (int i=0 ; i < values.Length ; i++) {
        if (i > 0)
          builder.Append(", ");
        builder.Append(values[i].Printed());
      }
      builder.Append(")");
      return builder.ToString();
    }
  }


  class EmptyRelValue : ValueBase {
    override public bool IsSet() {
      return true;
    }

    override public bool IsBinRel() {
      return true;
    }

    override public bool IsTernRel() {
      return true;
    }

    override public int Size() {
      return 0;
    }

    override public Value Item(int index) {
      throw new IndexOutOfRangeException();
    }

    override public void Entry(int index, out Value field1, out Value field2) {
      throw new IndexOutOfRangeException();
    }

    override public void Entry(int index, out Value field1, out Value field2, out Value field3) {
      throw new IndexOutOfRangeException();
    }

    override public bool IsRecord() {
      return true;
    }

    override public string Printed() {
      return "[]";
    }
  }


  class NeSetValue : ValueBase {
    Value[] values;

    public NeSetValue(Value[] values) {
      this.values = values;
    }

    override public bool IsSet() {
      return true;
    }

    override public int Size() {
      return values.Length;
    }

    override public Value Item(int index) {
      return values[index];
    }

    override public string Printed() {
      StringBuilder builder = new StringBuilder();
      builder.Append("[");
      for (int i=0 ; i < values.Length ; i++) {
        if (i > 0)
          builder.Append(", ");
        builder.Append(values[i].Printed());
      }
      builder.Append("]");
      return builder.ToString();
    }
  }


  class NeBinRelValue : ValueBase {
    Value[,] values;
    bool isMap;

    public NeBinRelValue(Value[,] values, bool isMap) {
      this.values = values;
      this.isMap = isMap;
    }

    override public bool IsBinRel() {
      return true;
    }

    override public int Size() {
      return values.GetLength(0);
    }

    override public void Entry(int index, out Value field1, out Value field2) {
      field1 = values[index, 0];
      field2 = values[index, 1];
    }

    override public bool IsRecord() {
      if (!isMap)
        return false;
      int len = values.GetLength(0);
      for (int i=0 ; i < len ; i++)
        if (!values[i, 0].IsSymb())
          return false;
      return isMap;
    }

    override public Value Lookup(string field) {
      int len = values.GetLength(0);
      for (int i=0 ; i < len ; i++)
        if (values[i, 0].AsSymb() == field)
          return values[i, 1];
      throw new KeyNotFoundException();
    }

    override public string Printed() {
      bool isRec = IsRecord();
      StringBuilder builder = new StringBuilder();
      builder.Append(isRec ? "(" : "[");
      int len = values.GetLength(0);
      for (int i=0 ; i < len ; i++) {
        if (i > 0)
          builder.Append(isMap ? ", " : "; ");
        builder.Append(values[i, 0].Printed());
        builder.Append(isMap ? (isRec ? ": " : " -> ") : ", ");
        builder.Append(values[i, 1].Printed());
      }
      builder.Append(isRec ? ")" : "]");
      return builder.ToString();
    }
  }


  class NeTernRelValue : ValueBase {
    Value[,] values;

    public NeTernRelValue(Value[,] values) {
      this.values = values;
    }

    override public bool IsTernRel() {
      return true;
    }

    override public int Size() {
      return values.GetLength(0);
    }

    override public void Entry(int index, out Value field1, out Value field2, out Value field3) {
      field1 = values[index, 0];
      field2 = values[index, 1];
      field3 = values[index, 2];
    }

    override public string Printed() {
      StringBuilder builder = new StringBuilder();
      builder.Append("[");
      int len = values.GetLength(0);
      for (int i=0 ; i < len ; i++) {
        if (i > 0)
          builder.Append("; ");
        builder.Append(values[i, 0].Printed());
        builder.Append(", ");
        builder.Append(values[i, 1].Printed());
        builder.Append(", ");
        builder.Append(values[i, 2].Printed());
      }
      if (len == 1)
        builder.Append(";");
      builder.Append("]");
      return builder.ToString();
    }
  }


  class TaggedValue : ValueBase {
    int tagId;
    Value value;

    public TaggedValue(int tagId, Value value) {
      this.tagId = tagId;
      this.value = value;
    }

    override public bool IsTagged() {
      return true;
    }

    override public string Tag() {
      return SymbTable.IdxToStr(tagId);
    }

    override public Value Untagged() {
      return value;
    }

    override public bool IsString() {
      if (tagId != SymbTable.StringSymbId)
        return false;
      if (!value.IsSeq())
        return false;
      int len = value.Size();
      for (int i=0 ; i < len ; i++) {
        Value item = value.Item(i);
        if (!item.IsInt() || item.AsLong() > 65535)
          return false;
      }
      return true;
    }

    override public string AsString() {
      if (!IsString())
        throw new NotImplementedException();
      int len = value.Size();
      char[] chars = new char[len];
      for (int i=0 ; i < len ; i++) {
        long code = value.Item(i).AsLong();
        // if (code > 65535)
        //  throw new NotImplementedException(); // Char.ConvertFromUtf32
        chars[i] = (char) code;
      }
      return new string(chars);
    }

    override public string Printed() {
      if (IsString()) {
        StringBuilder builder = new StringBuilder();
        builder.Append('"');
        int len = value.Size();
        for (int i=0 ; i < len ; i++) {
          int code = (char) value.Item(i).AsLong();
          if (code == '\n')
            builder.Append("\\n");
          else if (code == '\\')
            builder.Append("\\\\");
          else if (code == '"')
            builder.Append("\\\"");
          else if (code >= 32 & code <= 126)
            builder.Append((char) code);
          else {
            builder.Append('\\');
            for (int j=0 ; j < 4 ; j++) {
              int hexDigit = (code >> (12 - 4 * j)) % 16;
              char ch = (char) ((hexDigit < 10 ? '0' : 'A') + hexDigit);
              builder.Append(ch);
            }
          }
        }
        builder.Append('"');
        return builder.ToString();
      }

      bool skipPars = value.IsRecord() || (value.IsSeq() && value.Size() != 0);
      string str = value.Printed();
      if (!skipPars)
        str = "(" + str + ")";
      return SymbTable.IdxToStr(tagId) + str;
    }
  }
}
