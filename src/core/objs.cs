using System;
using System.Collections.Generic;
using System.Text;
using System.IO;


namespace CellLang {
  public abstract class Obj : IComparable<Obj> {
    public virtual bool IsBlankObj()                              {return false;}
    public virtual bool IsNullObj()                               {return false;}
    public virtual bool IsSymb()                                  {return false;}
    // public virtual bool IsBool()                                  {return false;}
    public virtual bool IsInt()                                   {return false;}
    public virtual bool IsFloat()                                 {return false;}
    public virtual bool IsSeq()                                   {return false;}
    public virtual bool IsEmptySeq()                              {return false;}
    public virtual bool IsNeSeq()                                 {return false;}
    public virtual bool IsEmptyRel()                              {return false;}
    public virtual bool IsSet()                                   {return false;}
    public virtual bool IsNeSet()                                 {return false;}
    public virtual bool IsBinRel()                                {return false;}
    public virtual bool IsNeBinRel()                              {return false;}
    public virtual bool IsNeMap()                                 {return false;}
    public virtual bool IsNeRecord()                              {return false;}
    public virtual bool IsTernRel()                               {return false;}
    public virtual bool IsNeTernRel()                             {return false;}
    public virtual bool IsTagged()                                {return false;}
    public virtual bool IsSyntacticSugaredString()                {return false;}

    public virtual bool IsSymb(int id)                            {return false;}
    public virtual bool IsInt(long n)                             {return false;}
    public virtual bool IsFloat(double x)                         {return false;}

    public virtual bool HasElem(Obj o)                            {throw new NotImplementedException();}
    public virtual bool HasKey(Obj o)                             {throw new NotImplementedException();}
    public virtual bool HasField(int id)                          {throw new NotImplementedException();}
    public virtual bool HasPair(Obj o1, Obj o2)                   {throw new NotImplementedException();}
    public virtual bool HasTriple(Obj o1, Obj o2, Obj o3)         {throw new NotImplementedException();}

    public virtual int    GetSymbId()                             {throw new NotImplementedException();}
    public virtual bool   GetBool()                               {throw new NotImplementedException();}
    public virtual long   GetLong()                               {throw new NotImplementedException();}
    public virtual double GetDouble()                             {throw new NotImplementedException();}
    public virtual int    GetSize()                               {throw new NotImplementedException();}
    public virtual Obj    GetItem(long i)                         {throw new NotImplementedException();}
    public virtual int    GetTagId()                              {throw new NotImplementedException();}
    public virtual Obj    GetTag()                                {throw new NotImplementedException();}
    public virtual Obj    GetInnerObj()                           {throw new NotImplementedException();}

    public virtual SeqOrSetIter GetSeqOrSetIter()                 {throw new NotImplementedException();}
    public virtual BinRelIter   GetBinRelIter()                   {throw new NotImplementedException();}
    public virtual TernRelIter  GetTernRelIter()                  {throw new NotImplementedException();}

    // Copy-on-write update
    public virtual Obj UpdatedAt(long i, Obj v)                   {throw new NotImplementedException();}

    public virtual BinRelIter GetBinRelIterByCol1(Obj obj)        {throw new NotImplementedException();}
    public virtual BinRelIter GetBinRelIterByCol2(Obj obj)        {throw new NotImplementedException();}

    public virtual TernRelIter GetTernRelIterByCol1(Obj val)      {throw new NotImplementedException();}
    public virtual TernRelIter GetTernRelIterByCol2(Obj val)      {throw new NotImplementedException();}
    public virtual TernRelIter GetTernRelIterByCol3(Obj val)      {throw new NotImplementedException();}

    public virtual TernRelIter GetTernRelIterByCol12(Obj val1, Obj val2)  {throw new NotImplementedException();}
    public virtual TernRelIter GetTernRelIterByCol13(Obj val1, Obj val3)  {throw new NotImplementedException();}
    public virtual TernRelIter GetTernRelIterByCol23(Obj val2, Obj val3)  {throw new NotImplementedException();}

    public virtual long Mantissa()                                {throw new NotImplementedException();}
    public virtual long DecExp()                                  {throw new NotImplementedException();}

    public virtual Obj Negate()                                   {throw new NotImplementedException();}
    public virtual Obj Reverse()                                  {throw new NotImplementedException();}
    public virtual void InitAt(long i, Obj v)                     {throw new NotImplementedException();}

    public virtual Obj InternalSort()                             {throw new NotImplementedException();}
    public virtual Obj GetSlice(long first, long len)             {throw new NotImplementedException();}

    public virtual long[] GetLongArray()                          {throw new NotImplementedException();}
    public virtual byte[] GetByteArray()                          {throw new NotImplementedException();}
    public virtual string GetString()                             {throw new NotImplementedException();}

    public virtual Obj Lookup(Obj key)                            {throw new NotImplementedException();}
    public virtual Obj LookupField(int id)                        {throw new NotImplementedException();}

    public virtual Obj Append(Obj obj)                            {throw new NotImplementedException();}
    public virtual Obj Concat(Obj seq)                            {throw new NotImplementedException();}
    public virtual Obj ConcatMany()                               {throw new NotImplementedException();}

    public virtual void CopyItems(Obj[] items, int offset)        {throw new NotImplementedException();}

    public virtual int CmpSeq(Obj[] es, int o, int l)             {throw new NotImplementedException();}
    public virtual int CmpNeSet(Obj[] es)                         {throw new NotImplementedException();}
    public virtual int CmpNeBinRel(Obj[] c1, Obj[] c2)            {throw new NotImplementedException();}
    public virtual int CmpNeTernRel(Obj[] c1, Obj[] c2, Obj[] c3) {throw new NotImplementedException();}
    public virtual int CmpTaggedObj(int tag, Obj obj)             {throw new NotImplementedException();}

    public virtual ValueBase GetValue()                           {throw new NotImplementedException();}

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public abstract uint Hashcode();
    public abstract void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel);
    public abstract int MinPrintedSize();

    protected abstract int TypeId();
    protected abstract int InternalCmp(Obj o);

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public virtual Obj RandElem() {throw new NotImplementedException();}

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public virtual bool IsEq(Obj o) {
      return Cmp(o) == 0;
    }

    public int CompareTo(Obj other) {
      return -Cmp(other);
    }

    public int Cmp(Obj o) {
      if (this == o)
        return 0;
      int id1 = TypeId();
      int id2 = o.TypeId();
      if (id1 == id2)
        return InternalCmp(o);
      return id1 < id2 ? 1 : -1;
    }

    override public string ToString() {
      StringWriter writer = new StringWriter();
      Print(writer, 90, true, 0);
      return writer.ToString();
    }

    public void Print() {
      Print(Console.Out, 90, true, 0);
      Console.WriteLine();
    }

    public Obj Printed() {
      return Miscellanea.StrToObj(ToString());
    }
  }


  class BlankObj : Obj {
    override public bool IsBlankObj() {
      return true;
    }

    override public uint Hashcode() {
      throw new NotImplementedException();
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      writer.Write("Blank");
    }

    override public int MinPrintedSize() {
      return "Blank".Length;
    }

    override protected int TypeId() {
      return -2;
    }

    override protected int InternalCmp(Obj o) {
      throw new NotImplementedException();
    }

    static BlankObj singleton = new BlankObj();

    public static BlankObj Singleton() {
      return singleton;
    }
  }


  class NullObj : Obj {
    override public bool IsNullObj() {
      return true;
    }

    override public uint Hashcode() {
      throw new NotImplementedException();
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      writer.Write("Null");
    }

    override public int MinPrintedSize() {
      return "Null".Length;
    }

    override protected int TypeId() {
      return -1;
    }

    override protected int InternalCmp(Obj o) {
      throw new NotImplementedException();
    }

    static NullObj singleton = new NullObj();

    public static NullObj Singleton() {
      return singleton;
    }
  }


  class SymbObj : Obj {
    int id;

    public SymbObj(int id) {
      this.id = id;
    }

    public static SymbObj Get(int id) {
      return SymbTable.Get(id);
    }

    public static SymbObj Get(bool b) {
      return SymbTable.Get(b ? SymbTable.TrueSymbId : SymbTable.FalseSymbId);
    }

    override public bool IsSymb() {
      return true;
    }

    override public bool IsSymb(int id) {
      return this.id == id;
    }

    override public int GetSymbId() {
      return id;
    }

    override public bool GetBool() {
      if (id == SymbTable.FalseSymbId)
        return false;
      if (id == SymbTable.TrueSymbId)
        return true;
      throw new NotImplementedException();
    }

    override public bool IsEq(Obj obj) {
      return obj.IsSymb(id);
    }

    override public Obj Negate() {
      if (id == SymbTable.FalseSymbId)
        return SymbObj.Get(SymbTable.TrueSymbId);
      if (id == SymbTable.TrueSymbId)
        return SymbObj.Get(SymbTable.FalseSymbId);
      throw new NotImplementedException();
    }

    override public uint Hashcode() {
      return (uint) id; //## BAD HASHCODE, IT'S NOT STABLE
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      writer.Write(SymbTable.IdxToStr(id));
    }

    override public int MinPrintedSize() {
      return SymbTable.IdxToStr(id).Length;
    }

    override public ValueBase GetValue() {
      return new SymbValue(id);
    }

    override protected int TypeId() {
      return 0;
    }

    override protected int InternalCmp(Obj other) {
      return SymbTable.CompSymbs(id, other.GetSymbId());
    }
  }


  class IntObj : Obj {
    long value;

    IntObj(long value) {
      this.value = value;
    }

    public static IntObj Get(long value) {
      if (value >= 0 & value < 256)
        return byteObjs[value];
      return new IntObj(value);
    }

    override public bool IsInt() {
      return true;
    }

    override public bool IsInt(long value) {
      return this.value == value;
    }

    override public long GetLong() {
      return value;
    }

    override public bool IsEq(Obj obj) {
      return obj.IsInt(value);
    }

    override public uint Hashcode() {
      return ((uint) (value >> 32)) ^ ((uint) value);
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      writer.Write(value);
    }

    override public int MinPrintedSize() {
      return value.ToString().Length;
    }

    override public ValueBase GetValue() {
      return new IntValue(value);
    }

    override protected int TypeId() {
      return 1;
    }

    override protected int InternalCmp(Obj obj) {
      long other_value = obj.GetLong();
      return value == other_value ? 0 : (value < other_value ? 1 : -1);
    }

    static IntObj[] byteObjs = new IntObj[256];

    static IntObj() {
      for (int i=0 ; i < byteObjs.Length ; i++)
        byteObjs[i] = new IntObj(i);
    }
  }


  class FloatObj : Obj {
    double value;

    public FloatObj(double value) {
      this.value = value;
    }

    override public bool IsFloat() {
      return true;
    }

    override public bool IsFloat(double value) {
      return this.value == value;
    }

    override public double GetDouble() {
      return value;
    }

    override public bool IsEq(Obj obj) {
      return obj.IsFloat(value);
    }

    override public uint Hashcode() {
      long longVal = BitConverter.DoubleToInt64Bits(value);
      return ((uint) (longVal >> 32)) ^ ((uint) longVal);
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      writer.Write(value);
    }

    override public int MinPrintedSize() {
      return value.ToString().Length;
    }

    override public ValueBase GetValue() {
      return new FloatValue(value);
    }

    override protected int TypeId() {
      return 2;
    }

    override protected int InternalCmp(Obj other) {
      double other_value = other.GetDouble();
      return value == other_value ? 0 : (value < other_value ? 1 : -1);
    }
  }


  class EmptyRelObj : Obj {
    EmptyRelObj() {

    }

    override public bool IsEmptyRel() {
      return true;
    }

    override public bool IsSet() {
      return true;
    }

    override public bool IsBinRel() {
      return true;
    }

    override public bool IsTernRel() {
      return true;
    }

    override public bool IsEq(Obj obj) {
      return obj.IsEmptyRel();
    }

    override public bool HasElem(Obj obj) {
      return false;
    }

    override public bool HasKey(Obj key) {
      return false;
    }

    override public bool HasField(int id) {
      return false;
    }

    override public bool HasPair(Obj obj1, Obj obj2) {
      return false;
    }

    override public bool HasTriple(Obj obj1, Obj obj2, Obj obj3) {
      return false;
    }

    override public int GetSize() {
      return 0;
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return iter1;
    }

    override public BinRelIter GetBinRelIter() {
      return iter2;
    }

    override public BinRelIter GetBinRelIterByCol1(Obj obj) {
      return iter2;
    }

    override public BinRelIter GetBinRelIterByCol2(Obj obj) {
      return iter2;
    }

    override public TernRelIter GetTernRelIter() {
      return iter3;
    }

    override public TernRelIter GetTernRelIterByCol1(Obj val) {
      return iter3;
    }

    override public TernRelIter GetTernRelIterByCol2(Obj val) {
      return iter3;
    }

    override public TernRelIter GetTernRelIterByCol3(Obj val) {
      return iter3;
    }

    override public TernRelIter GetTernRelIterByCol12(Obj val1, Obj val2) {
      return iter3;
    }

    override public TernRelIter GetTernRelIterByCol13(Obj val1, Obj val3) {
      return iter3;
    }

    override public TernRelIter GetTernRelIterByCol23(Obj val2, Obj val3) {
      return iter3;
    }

    override public Obj InternalSort() {
      return SeqObj.Empty();
    }

    override public uint Hashcode() {
      return 0; //## FIND BETTER VALUE
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      writer.Write("[]");
    }

    override public int MinPrintedSize() {
      return 2;
    }

    override public ValueBase GetValue() {
      return new EmptyRelValue();
    }

    override protected int TypeId() {
      return 4;
    }

    override protected int InternalCmp(Obj other) {
      return 0;
    }

    static SeqOrSetIter iter1 = new SeqOrSetIter(new Obj[0], 0, -1);
    static BinRelIter   iter2 = new BinRelIter(new Obj[0], new Obj[0]);
    static TernRelIter  iter3 = new TernRelIter(new Obj[0], new Obj[0], new Obj[0]);

    static EmptyRelObj singleton = new EmptyRelObj();

    public static EmptyRelObj Singleton() {
      return singleton;
    }
  }


  class NeSetObj : Obj {
    Obj[] elts;
    int minPrintedSize = -1;

    public NeSetObj(Obj[] elts) {
      Miscellanea.Assert(elts.Length > 0);
      this.elts = elts;
    }

    override public bool IsSet() {
      return true;
    }

    override public bool IsNeSet() {
      return true;
    }

    override public bool HasElem(Obj obj) {
      return Algs.BinSearch(elts, obj) != -1;
    }

    override public int GetSize() {
      return elts.Length;
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return new SeqOrSetIter(elts, 0, elts.Length-1);
    }

    override public Obj InternalSort() {
      return new MasterSeqObj(elts);
    }

    override public uint Hashcode() {
      uint hashcodesSum = 0;
      for (int i=0 ; i < elts.Length ; i++)
        hashcodesSum += elts[i].Hashcode();
      return hashcodesSum ^ (uint) elts.Length;
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      int len = elts.Length;
      bool breakLine = MinPrintedSize() > maxLineLen;

      writer.Write('[');

      if (breakLine) {
        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (newLine)
          writer.Write(' ');
        else
          writer.WriteIndentedNewLine(indentLevel + 1);
      }

      for (int i=0 ; i < len ; i++) {
        if (i > 0) {
          writer.Write(',');
          if (breakLine)
            writer.WriteIndentedNewLine(indentLevel + 1);
          else
            writer.Write(' ');
        }
        elts[i].Print(writer, maxLineLen, breakLine & !newLine, indentLevel + 1);
      }

      if (breakLine)
        writer.WriteIndentedNewLine(indentLevel);

      writer.Write(']');
    }

    override public int MinPrintedSize() {
      if (minPrintedSize == -1) {
        int len = elts.Length;
        minPrintedSize = 2 * len;
        for (int i=0 ; i < len ; i++)
          minPrintedSize += elts[i].MinPrintedSize();
      }
      return minPrintedSize;
    }

    override public ValueBase GetValue() {
      int size = elts.Length;
      ValueBase[] values = new ValueBase[size];
      for (int i=0 ; i < size ; i++)
        values[i] = elts[i].GetValue();
      return new NeSetValue(values);
    }

    override protected int TypeId() {
      return 5;
    }

    override protected int InternalCmp(Obj other) {
      return other.CmpNeSet(elts);
    }

    override public int CmpNeSet(Obj[] other_elts) {
      int len = elts.Length;
      int other_len = other_elts.Length;
      if (other_len != len)
        return other_len < len ? 1 : -1;
      for (int i=0 ; i < len ; i++) {
        int res = other_elts[i].Cmp(elts[i]);
        if (res != 0)
          return res;
      }
      return 0;
    }

    override public Obj RandElem() {
      return elts[0];
    }
  }


  class NeBinRelObj : Obj {
    Obj[] col1;
    Obj[] col2;
    int[] revIdxs;
    bool isMap;
    int minPrintedSize = -1;

    public NeBinRelObj(Obj[] col1, Obj[] col2, bool isMap) {
      Miscellanea.Assert(col1 != null && col2 != null);
      Miscellanea.Assert(col1.Length > 0);
      Miscellanea.Assert(col1.Length == col2.Length);
      this.col1 = col1;
      this.col2 = col2;
      this.isMap = isMap;
    }

    override public bool IsBinRel() {
      return true;
    }

    override public bool IsNeBinRel() {
      return true;
    }

    override public bool IsNeMap() {
      return isMap;
    }

    override public bool HasKey(Obj obj) {
      Miscellanea.Assert(isMap);
      return Algs.BinSearch(col1, obj) != -1;
    }

    override public bool HasField(int symb_id) {
      int len = col1.Length;
      for (int i=0 ; i < len ; i++)
        if (col1[i].IsSymb(symb_id))
          return true;
      return false;
    }

    override public bool HasPair(Obj obj1, Obj obj2) {
      if (isMap) {
        int idx = Algs.BinSearch(col1, obj1);
        return idx != -1 && col2[idx].IsEq(obj2);
      }
      else {
        int first;
        int count = Algs.BinSearchRange(col1, 0, col1.Length, obj1, out first);
        if (count == 0)
          return false;
        int idx = Algs.BinSearch(col2, first, count, obj2);
        return idx != -1;
      }
    }

    override public int GetSize() {
      return col1.Length;
    }

    override public BinRelIter GetBinRelIter() {
      return new BinRelIter(col1, col2);
    }

    override public BinRelIter GetBinRelIterByCol1(Obj obj) {
      int first;
      int count = Algs.BinSearchRange(col1, 0, col1.Length, obj, out first);
      return new BinRelIter(col1, col2, first, first+count-1);
    }

    override public BinRelIter GetBinRelIterByCol2(Obj obj) {
      if (revIdxs == null)
        revIdxs = Algs.SortedIndexes(col2, col1);
      int first;
      int count = Algs.BinSearchRange(revIdxs, col2, obj, out first);
      return new BinRelIter(col1, col2, revIdxs, first, first+count-1);
    }

    override public Obj Lookup(Obj key) {
      int idx = Algs.BinSearch(col1, key);
      if (idx == -1)
        throw new Exception();
      if (!isMap)
        if ((idx > 0 && col1[idx-1].IsEq(key)) || (idx+1 < col1.Length && col1[idx+1].IsEq(key)))
          throw new Exception();
      return col2[idx];
    }

    override public Obj LookupField(int symb_id) {
      int len = col1.Length;
      for (int i=0 ; i < len ; i++)
        if (col1[i].IsSymb(symb_id))
          return col2[i];
      // We should never get here. The typechecker should prevent it.
      throw new InvalidOperationException();
    }

    override public uint Hashcode() {
      uint hashcodesSum = 0;
      for (int i=0 ; i < col1.Length ; i++)
        hashcodesSum += col1[i].Hashcode() + col2[i].Hashcode();
      return hashcodesSum ^ (uint) col1.Length;
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      int len = col1.Length;
      bool isRec = IsNeRecord();
      bool breakLine = MinPrintedSize() > maxLineLen;
      string argSep = isMap ? (isRec ? ":" : " ->") : ",";
      string entrySep = isMap ? "," : ";";

      writer.Write(isRec ? '(' : '[');

      if (breakLine) {
        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (newLine)
          writer.Write(' ');
        else
          writer.WriteIndentedNewLine(indentLevel + 1);
      }

      for (int i=0 ; i < len ; i++) {
        Obj arg1 = col1[i];
        Obj arg2 = col2[i];

        // Writing the first argument, followed by the separator
        arg1.Print(writer, maxLineLen, newLine | (i > 0), indentLevel + 1);
        writer.Write(argSep);

        int arg1Len = arg1.MinPrintedSize();
        int arg2Len = arg2.MinPrintedSize();

        if (arg1Len + arg2Len + argSep.Length <= maxLineLen) {
          // The entire entry fits into one line
          // We just insert a space and start printing the second argument
          writer.Write(' ');
          arg2.Print(writer, maxLineLen, false, indentLevel);
        }
        else if (arg1Len <= maxLineLen) {
          // The first argument fits into one line, but the whole entry doesn't.
          if ((arg2.IsTagged() & !arg2.IsSyntacticSugaredString()) | arg2Len <= maxLineLen) {
            // If the second argument fits into one line (and therefore cannot break itself)
            // or if it's an unsugared tagged object, we break the line.
            writer.WriteIndentedNewLine(indentLevel + 2);
            arg2.Print(writer, maxLineLen, false, indentLevel + 2);
          }
          else {
            // Otherwise we keep going on the same line, and let the second argument break itself
            writer.Write(' ');
            arg2.Print(writer, maxLineLen, false, indentLevel + 1);
          }
        }
        else if (arg2.IsTagged() & !arg2.IsSyntacticSugaredString() & arg2Len > maxLineLen) {
          // The first argument does not fit into a line, and the second one
          // is a multiline unsugared tagged object, so we break the line
          writer.WriteIndentedNewLine(indentLevel + 1);
          arg2.Print(writer, maxLineLen, true, indentLevel + 1);
        }
        else {
          // The first argument doesn't fit into a line, and the second
          // one is not special, so we just keep going on the same line
          // and let the second argument break itself is need be
          writer.Write(' ');
          arg2.Print(writer, maxLineLen, true, indentLevel + 1);
        }

        // We print the entry separator/terminator when appropriate
        bool lastLine = i == len - 1;
        if (!lastLine | (!isMap & (len == 1)))
          writer.Write(entrySep);

        // Either we break the line, or insert a space if this is not the last entry
        if (breakLine)
          writer.WriteIndentedNewLine(indentLevel + (lastLine ? 0 : 1));
        else if (!lastLine)
          writer.Write(' ');
      }

      writer.Write(isRec ? ')' : ']');
    }

    override public int MinPrintedSize() {
      if (minPrintedSize == -1) {
        int len = col1.Length;
        bool isRec = IsNeRecord();
        minPrintedSize = (2 + (isMap & !isRec ? 4 : 2)) * len + ((!isMap & len == 1) ? 1 : 0);
        for (int i=0 ; i < len ; i++)
          minPrintedSize += col1[i].MinPrintedSize() + col2[i].MinPrintedSize();
      }
      return minPrintedSize;
    }

    override public ValueBase GetValue() {
      int size = col1.Length;
      ValueBase[,] values = new ValueBase[size, 2];
      for (int i=0 ; i < size ; i++) {
        values[i, 0] = col1[i].GetValue();
        values[i, 1] = col2[i].GetValue();
      }
      return new NeBinRelValue(values, isMap);
    }

    override protected int TypeId() {
      return 6;
    }

    override protected int InternalCmp(Obj other) {
      return other.CmpNeBinRel(col1, col2);
    }

    override public int CmpNeBinRel(Obj[] other_col_1, Obj[] other_col_2) {
      int len = col1.Length;
      int other_len = other_col_1.Length;
      if (other_len != len)
        return other_len < len ? 1 : -1;
      for (int i=0 ; i < len ; i++) {
        int res = other_col_1[i].Cmp(col1[i]);
        if (res != 0)
          return res;
      }
      for (int i=0 ; i < len ; i++) {
        int res = other_col_2[i].Cmp(col2[i]);
        if (res != 0)
          return res;
      }
      return 0;
    }

    override public bool IsNeRecord() {
      if (!isMap)
        return false;
      int len = col1.Length;
      for (int i=0 ; i < len ; i++)
        if (!col1[i].IsSymb())
          return false;
      return true;
    }
  }


  class NeTernRelObj : Obj {
    Obj[] col1;
    Obj[] col2;
    Obj[] col3;
    int[] idxs231;
    int[] idxs312;
    int minPrintedSize = -1;

    public void Dump() {
      if (idxs231 == null)
        idxs231 = Algs.SortedIndexes(col2, col3, col1);
      if (idxs312 == null)
        idxs312 = Algs.SortedIndexes(col3, col1, col2);

      Console.WriteLine("");

      for (int i=0 ; i < col1.Length ; i++)
        Console.WriteLine("({0}, {1}, {2}", col1[i], col2[i], col3[i]);
      Console.WriteLine("");

      for (int i=0 ; i < idxs231.Length ; i++) {
        int idx = idxs231[i];
        Console.WriteLine("({0}, {1}, {2})", col1[idx], col2[idx], col3[idx]);
      }
      Console.WriteLine("");

      for (int i=0 ; i < idxs312.Length ; i++) {
        int idx = idxs312[i];
        Console.WriteLine("({0}, {1}, {2})", col1[idx], col2[idx], col3[idx]);
      }
    }

    public NeTernRelObj(Obj[] col1, Obj[] col2, Obj[] col3) {
      Miscellanea.Assert(col1 != null && col2 != null && col3 != null);
      Miscellanea.Assert(col1.Length == col2.Length && col1.Length == col3.Length);
      Miscellanea.Assert(col1.Length > 0);
      this.col1 = col1;
      this.col2 = col2;
      this.col3 = col3;
    }

    override public bool IsTernRel() {
      return true;
    }

    override public bool IsNeTernRel() {
      return true;
    }

    override public bool HasTriple(Obj obj1, Obj obj2, Obj obj3) {
      int first;

      int count = Algs.BinSearchRange(col1, 0, col1.Length, obj1, out first);
      if (count == 0)
        return false;

      count = Algs.BinSearchRange(col2, first, count, obj2, out first);
      if (count == 0)
        return false;

      int idx = Algs.BinSearch(col3, first, count, obj3);
      return idx != -1;
    }

    override public int GetSize() {
      return col1.Length;
    }

    override public TernRelIter GetTernRelIter() {
      return new TernRelIter(col1, col2, col3);
    }

    override public TernRelIter GetTernRelIterByCol1(Obj val) {
      int first;
      int count = Algs.BinSearchRange(col1, 0, col1.Length, val, out first);
      return new TernRelIter(col1, col2, col3, null, first, first+count-1);
    }

    override public TernRelIter GetTernRelIterByCol2(Obj val) {
      if (idxs231 == null)
        idxs231 = Algs.SortedIndexes(col2, col3, col1);
      int first;
      int count = Algs.BinSearchRange(idxs231, col2, val, out first);
      return new TernRelIter(col1, col2, col3, idxs231, first, first+count-1);
    }

    override public TernRelIter GetTernRelIterByCol3(Obj val) {
      if (idxs312 == null)
        idxs312 = Algs.SortedIndexes(col3, col1, col2);
      int first;
      int count = Algs.BinSearchRange(idxs312, col3, val, out first);
      return new TernRelIter(col1, col2, col3, idxs312, first, first+count-1);
    }

    override public TernRelIter GetTernRelIterByCol12(Obj val1, Obj val2) {
      int first;
      int count = Algs.BinSearchRange(col1, col2, val1, val2, out first);
      return new TernRelIter(col1, col2, col3, null, first, first+count-1);
    }

    override public TernRelIter GetTernRelIterByCol13(Obj val1, Obj val3) {
      if (idxs312 == null)
        idxs312 = Algs.SortedIndexes(col3, col1, col2);
      int first;
      int count = Algs.BinSearchRange(idxs312, col3, col1, val3, val1, out first);
      return new TernRelIter(col1, col2, col3, idxs312, first, first+count-1);
    }

    override public TernRelIter GetTernRelIterByCol23(Obj val2, Obj val3) {
      if (idxs231 == null)
        idxs231 = Algs.SortedIndexes(col2, col3, col1);
      int first;
      int count = Algs.BinSearchRange(idxs231, col2, col3, val2, val3, out first);
      return new TernRelIter(col1, col2, col3, idxs231, first, first+count-1);
    }

    override public uint Hashcode() {
      uint hashcodesSum = 0;
      for (int i=0 ; i < col1.Length ; i++)
        hashcodesSum += col1[i].Hashcode() + col2[i].Hashcode() + col3[i].Hashcode();
      return hashcodesSum ^ (uint) col1.Length;
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      int len = col1.Length;
      bool breakLine = MinPrintedSize() > maxLineLen;

      writer.Write('[');

      if (breakLine) {
        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (newLine)
          writer.Write(" ");
        else
          writer.WriteIndentedNewLine(indentLevel + 1);
      }

      for (int i=0 ; i < len ; i++) {
        int arg1Len = col1[i].MinPrintedSize();
        int arg2Len = col2[i].MinPrintedSize();
        int arg3Len = col3[i].MinPrintedSize();
        int entryLen = 4 + arg1Len + arg2Len + arg3Len;

        bool eachArgFits = arg1Len <= maxLineLen & arg2Len <= maxLineLen & arg3Len <= maxLineLen;
        bool breakLineBetweenArgs = eachArgFits & entryLen > maxLineLen;

        // Writing the first argument, followed by the separator
        col1[i].Print(writer, maxLineLen, newLine | (i > 0), indentLevel + 2);

        if (breakLineBetweenArgs) {
          // If each argument fits into the maximum line length, but
          // the whole entry doesn't, then we break the line before
          // we start printing each of the following arguments
          writer.WriteIndentedNewLine(",", indentLevel);
          col2[i].Print(writer, maxLineLen, true, indentLevel);
          writer.WriteIndentedNewLine(",", indentLevel);
          col3[i].Print(writer, maxLineLen, true, indentLevel);
        }
        else {
          // Otherwise we just insert a space and start printing the second argument
          writer.Write(", ");
          col2[i].Print(writer, maxLineLen, false, indentLevel);
          writer.Write(", ");
          col3[i].Print(writer, maxLineLen, false, indentLevel);
        }

        // We print the entry separator/terminator when appropriate
        bool lastLine = i == len - 1;
        if (!lastLine | len == 1)
          writer.Write(';');

        // Either we break the line, or insert a space if this is not the last entry
        if (breakLine)
          writer.WriteIndentedNewLine(indentLevel + (lastLine ? 0 : 1));
        else if (!lastLine)
          writer.Write(' ');
      }

      writer.Write(']');
    }

    override public int MinPrintedSize() {
      if (minPrintedSize == -1) {
        int len = col1.Length;
        minPrintedSize = 6 * len + (len == 1 ? 1 : 0);
        for (int i=0 ; i < len ; i++)
          minPrintedSize += col1[i].MinPrintedSize() + col2[i].MinPrintedSize() + col3[i].MinPrintedSize();
      }
      return minPrintedSize;
    }

    override public ValueBase GetValue() {
      int size = col1.Length;
      ValueBase[,] values = new ValueBase[size, 3];
      for (int i=0 ; i < size ; i++) {
        values[i, 0] = col1[i].GetValue();
        values[i, 1] = col2[i].GetValue();
        values[i, 2] = col3[i].GetValue();
      }
      return new NeTernRelValue(values);
    }

    override protected int TypeId() {
      return 7;
    }

    override protected int InternalCmp(Obj other) {
      return other.CmpNeTernRel(col1, col2, col3);
    }

    override public int CmpNeTernRel(Obj[] other_col_1, Obj[] other_col_2, Obj[] other_col_3) {
      int len = col1.Length;
      int other_len = other_col_1.Length;
      if (other_len != len)
        return other_len < len ? 1 : -1;
      for (int i=0 ; i < len ; i++) {
        int res = other_col_1[i].Cmp(col1[i]);
        if (res != 0)
          return res;
      }
      for (int i=0 ; i < len ; i++) {
        int res = other_col_2[i].Cmp(col2[i]);
        if (res != 0)
          return res;
      }
      for (int i=0 ; i < len ; i++) {
        int res = other_col_3[i].Cmp(col3[i]);
        if (res != 0)
          return res;
      }
      return 0;
    }
  }


  class TaggedObj : Obj {
    int tag;
    Obj obj;
    int minPrintedSize = -1;

    public TaggedObj(int tag, Obj obj) {
      Miscellanea.Assert(obj != null);
      this.tag = tag;
      this.obj = obj;
    }

    public TaggedObj(Obj tag, Obj obj) : this(tag.GetSymbId(), obj) {

    }

    override public bool IsTagged() {
      return true;
    }

    override public bool IsSyntacticSugaredString() {
      if (tag != SymbTable.StringSymbId | !obj.IsSeq())
        return false;
      int len = obj.GetSize();
      for (int i=0 ; i < len ; i++) {
        Obj item = obj.GetItem(i);
        if (!item.IsInt())
          return false;
        long value = item.GetLong();
        if (value < 0 | value > 65535)
          return false;
      }
      return true;
    }

    override public bool HasField(int id) {
      return obj.HasField(id);
    }

    override public int GetTagId() {
      return tag;
    }

    override public Obj GetTag() {
      return SymbObj.Get(tag);
    }

    override public Obj GetInnerObj() {
      return obj;
    }

    override public Obj LookupField(int id) {
      return obj.LookupField(id);
    }

    override public string GetString() {
      if (tag != SymbTable.StringSymbId)
        throw new NotImplementedException();
      long[] codes = obj.GetLongArray();
      char[] chars = new char[codes.Length];
      for (int i=0 ; i < codes.Length ; i++) {
        long code = codes[i];
        if (code < 0 | code > 65535)
          // Char.ConvertFromUtf32
          throw new NotImplementedException();
        chars[i] = (char) code;
      }
      return new string(chars);
    }

    override public uint Hashcode() {
      return ((uint) tag) ^ obj.Hashcode();
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      if (IsSyntacticSugaredString()) {
        long[] codes = obj.GetLongArray();
        int len = codes.Length;
        writer.Write('"');
        for (int i=0 ; i < len ; i++) {
          int code = (char) codes[i];
          if (code == '\n')
            writer.Write("\\n");
          else if (code == '\t')
            writer.Write("\\t");
          else if (code == '\\')
            writer.Write("\\\\");
          else if (code == '"')
            writer.Write("\\\"");
          else if (code >= 32 & code <= 126)
            writer.Write((char) code);
          else {
            writer.Write('\\');
            for (int j=0 ; j < 4 ; j++) {
              int hexDigit = (code >> (12 - 4 * j)) % 16;
              char ch = (char) ((hexDigit < 10 ? '0' : 'A') + hexDigit);
              writer.Write(ch);
            }
          }
        }
        writer.Write('"');
        return;
      }

      string tagStr = SymbTable.IdxToStr(tag);
      writer.Write(tagStr);

      if (obj.IsNeRecord() | (obj.IsNeSeq() && obj.GetSize() > 1)) {
        obj.Print(writer, maxLineLen, false, indentLevel);
        return;
      }

      bool breakLine = MinPrintedSize() > maxLineLen;
      if (breakLine)
        breakLine = (obj.IsTagged() & !obj.IsSyntacticSugaredString()) | obj.MinPrintedSize() <= maxLineLen;

      writer.Write('(');
      if (breakLine) {
        writer.WriteIndentedNewLine(indentLevel + 1);
        obj.Print(writer, maxLineLen, breakLine, indentLevel + 1);
        writer.WriteIndentedNewLine(indentLevel);
      }
      else
        obj.Print(writer, maxLineLen, breakLine, indentLevel);
      writer.Write(')');
    }

    override public int MinPrintedSize() {
      if (minPrintedSize == -1)
        if (!IsSyntacticSugaredString()) {
          bool skipPars = obj.IsNeRecord() | obj.IsNeSeq();
          minPrintedSize = SymbTable.IdxToStr(tag).Length + obj.MinPrintedSize() + (skipPars ? 0 : 2);
        }
        else {
          long[] codes = obj.GetLongArray();
          int len = codes.Length;
          minPrintedSize = 2;
          for (int i=0 ; i < len ; i++) {
            int code = (char) codes[i];
            if (code == '"' | code == '\n' | code == '\t')
              minPrintedSize += 2;
            else if (code == '\\')
              minPrintedSize += 4;
            else if (code < 32 | code > 126)
              minPrintedSize += 5;
            else
              minPrintedSize++;
          }
        }
      return minPrintedSize;
    }

    override public ValueBase GetValue() {
      return new TaggedValue(tag, obj.GetValue());
    }

    override protected int TypeId() {
      return 8;
    }

    override protected int InternalCmp(Obj other) {
      return other.CmpTaggedObj(tag, obj);
    }

    override public int CmpTaggedObj(int other_tag, Obj other_obj) {
      if (other_tag != tag)
        return SymbTable.CompSymbs(other_tag, tag);
      else
        return other_obj.Cmp(obj);
    }
  }
}
