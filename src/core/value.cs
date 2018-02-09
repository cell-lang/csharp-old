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

    void Print(TextWriter writer);
  }


  public abstract class ValueBase : Value {
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

    public void Print(TextWriter writer) {
      Obj obj = AsObj();
      obj.Print(writer, 90, true, 0);
    }

    override public string ToString() {
      StringWriter writer = new StringWriter();
      Print(writer);
      return writer.ToString();
    }

    public abstract Obj AsObj();
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

    override public Obj AsObj() {
      return SymbObj.Get(id);
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

    override public Obj AsObj() {
      return IntObj.Get(value);
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

    override public Obj AsObj() {
      return new FloatObj(value);
    }
  }


  class SeqValue : ValueBase {
    ValueBase[] values;

    public SeqValue(ValueBase[] values) {
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

    override public Obj AsObj() {
      int len = values.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = values[i].AsObj();
      return new MasterSeqObj(objs);
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

    override public Obj AsObj() {
      return EmptyRelObj.Singleton();
    }
  }


  class NeSetValue : ValueBase {
    ValueBase[] values;

    public NeSetValue(ValueBase[] values) {
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

    override public Obj AsObj() {
      int len = values.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = values[i].AsObj();
      return new NeSetObj(objs);
    }
  }


  class NeBinRelValue : ValueBase {
    ValueBase[,] values;
    bool isMap;

    public NeBinRelValue(ValueBase[,] values, bool isMap) {
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

    override public Obj AsObj() {
      int len = values.GetLength(0);
      Obj[] col1 = new Obj[len];
      Obj[] col2 = new Obj[len];
      for (int i=0 ; i < len ; i++) {
        col1[i] = values[i, 0].AsObj();
        col2[i] = values[i, 1].AsObj();
      }
      return new NeBinRelObj(col1, col2, isMap);
    }
  }


  class NeTernRelValue : ValueBase {
    ValueBase[,] values;

    public NeTernRelValue(ValueBase[,] values) {
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

    override public Obj AsObj() {
      int len = values.GetLength(0);
      Obj[] col1 = new Obj[len];
      Obj[] col2 = new Obj[len];
      Obj[] col3 = new Obj[len];
      for (int i=0 ; i < len ; i++) {
        col1[i] = values[i, 0].AsObj();
        col2[i] = values[i, 1].AsObj();
        col3[i] = values[i, 2].AsObj();
      }
      return new NeTernRelObj(col1, col2, col3);
    }
  }


  class TaggedValue : ValueBase {
    int tagId;
    ValueBase value;

    public TaggedValue(int tagId, ValueBase value) {
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

    override public Value Lookup(string field) {
      return value.Lookup(field);
    }

    override public Obj AsObj() {
      return new TaggedObj(tagId, value.AsObj());
    }
  }
}
