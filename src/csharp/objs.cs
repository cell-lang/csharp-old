using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public abstract class Obj : IComparable<Obj> {
    // public virtual bool IsBlankObj()                              {return false;}
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
    public virtual bool IsTernRel()                               {return false;}
    public virtual bool IsNeTernRel()                             {return false;}
    public virtual bool IsTagged()                                {return false;}

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
    //public virtual long   GetLong()                               {throw new NotImplementedException();}
    public virtual double GetFloat()                              {throw new NotImplementedException();}
    public virtual int    GetSize()                               {throw new NotImplementedException();}
    public virtual Obj    GetItem(long i)                         {throw new NotImplementedException();}
    public virtual int    GetTagId()                              {throw new NotImplementedException();}
    public virtual Obj    GetTag()                                {throw new NotImplementedException();}
    public virtual Obj    GetInnerObj()                           {throw new NotImplementedException();}

    public virtual SeqOrSetIter GetSeqOrSetIter()                 {throw new NotImplementedException();}
    public virtual BinRelIter   GetBinRelIter()                   {throw new NotImplementedException();}
    public virtual TernRelIter  GetTernRelIter()                  {throw new NotImplementedException();}

    // public virtual string ToString()                              {throw new NotImplementedException();}

    public void Print() {
      Console.WriteLine(ToString());
    }

    public Obj Printed() {
      return Miscellanea.StrToObj(ToString());
    }

    //## IMPLEMENT
    // Copy-on-write update
    public virtual Obj UpdateAt(long i, Obj v)                    {throw new NotImplementedException();}

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
    // public virtual Obj LookupField(int id)                        {throw new NotImplementedException();}

    public virtual Obj Append(Obj obj)                            {throw new NotImplementedException();}
    public virtual Obj Concat(Obj seq)                            {throw new NotImplementedException();}
    public virtual Obj ConcatMany()                               {throw new NotImplementedException();}

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

    public virtual int CmpSeq(Obj[] es, int o, int l)             {throw new NotImplementedException();}
    public virtual int CmpNeSet(Obj[] es)                         {throw new NotImplementedException();}
    public virtual int CmpNeBinRel(Obj[] c1, Obj[] c2)            {throw new NotImplementedException();}
    public virtual int CmpNeTernRel(Obj[] c1, Obj[] c2, Obj[] c3) {throw new NotImplementedException();}
    public virtual int CmpTaggedObj(int tag, Obj obj)             {throw new NotImplementedException();}

    protected abstract int TypeId();
    protected abstract int InternalCmp(Obj o);



    public virtual long GetLong() {
      Console.WriteLine(ToString());
      throw new NotImplementedException();
    }

    public virtual Obj LookupField(int id) {
      Console.WriteLine(this);
      throw new NotImplementedException();
    }

  }


//  class NullObj : Obj {
//    static NullObj singleton = new NullObj();
//
//    public static NullObj Singleton() {
//      return singleton;
//    }
//  }


  class SymbObj : Obj {
    int id;

    public SymbObj(int id) {
      this.id = id;
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
        return new SymbObj(SymbTable.TrueSymbId);
      if (id == SymbTable.TrueSymbId)
        return new SymbObj(SymbTable.FalseSymbId);
      throw new NotImplementedException();
    }

    override public string ToString() {
      return SymbTable.IdxToStr(id);
    }

    override protected int TypeId() {
      return 0;
    }

    override protected int InternalCmp(Obj other) {
      int other_id = other.GetSymbId();
      return id == other_id ? 0 : (id < other_id ? 1 : -1);
    }
  }


  class IntObj : Obj {
    long value;

    public IntObj(long value) {
      this.value = value;
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

    override public string ToString() {
      return value.ToString();
    }

    override protected int TypeId() {
      return 1;
    }

    override protected int InternalCmp(Obj obj) {
      long other_value = obj.GetLong();
      return value == other_value ? 0 : (value < other_value ? 1 : -1);
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

    override public double GetFloat() {
      return value;
    }

    override public bool IsEq(Obj obj) {
      return obj.IsFloat(value);
    }

    override public string ToString() {
      return value.ToString();
    }

    override protected int TypeId() {
      return 2;
    }

    override protected int InternalCmp(Obj other) {
      double other_value = other.GetFloat();
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

    override public string ToString() {
      return "[]";
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

    public NeSetObj(Obj[] elts) {
      Miscellanea.Assert(elts == null || elts.Length > 0);
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

    override public string ToString() {
      string[] reprs = new string[elts.Length];
      for (int i=0 ; i < elts.Length ; i++)
        reprs[i] = elts[i].ToString();
      return "[" + string.Join(", ", reprs) + "]";
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return new SeqOrSetIter(elts, 0, elts.Length-1);
    }

    override public Obj InternalSort() {
      return new MasterSeqObj(elts);
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
  }


  class NeBinRelObj : Obj {
    Obj[] col1;
    Obj[] col2;
    int[] revIdxs;
    bool isMap;

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

    override public string ToString() {
      string sep = isMap ? " -> " : ", ";
      string[] reprs = new string[col1.Length];
      for (int i=0 ; i < col1.Length ; i++)
        reprs[i] = col1[i].ToString() + sep + col2[i].ToString();
      return "[" + string.Join(isMap ? ", " : "; ", reprs) + (col1.Length == 1 ? ";]" : "]");
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
      int count = Algs.BinSearchRange(col2, 0, col2.Length, obj, out first);
      return new BinRelIter(col1, col2, first, first+count-1);
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
  }


  class NeTernRelObj : Obj {
    Obj[] col1;
    Obj[] col2;
    Obj[] col3;
    int[] idxs231;
    int[] idxs312;

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

    override public string ToString() {
      string[] reprs = new string[col1.Length];
      for (int i=0 ; i < col1.Length ; i++)
        reprs[i] = col1[i].ToString() + ", " + col2[i].ToString() + ", " + col3[i].ToString();
      return "[" + string.Join("; ", reprs) + (col1.Length == 1 ? ";]" : "]");
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
      return new TernRelIter(col1, col2, col3, idxs312, first, first+count+1);
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

    public TaggedObj(int tag, Obj obj) {
      Miscellanea.Assert(obj != null);
      if (tag == SymbTable.StringSymbId) {
        if (!obj.IsSeq()) {
          Console.WriteLine("NOT A SEQUENCE!");
          throw new Exception();
        }
        for (int i=0 ; i < obj.GetSize() ; i++) {
          Obj item = obj.GetItem(i);
          if (!item.IsInt()) {
            Console.WriteLine("NOT A CHARACTER!");
            Console.WriteLine(item.ToString());
            Console.WriteLine(obj.ToString());
            throw new Exception();
          }
        }
      }
      this.tag = tag;
      this.obj = obj;
    }

    public TaggedObj(Obj tag, Obj obj) : this(tag.GetSymbId(), obj) {

    }

    override public bool IsTagged() {
      return true;
    }

    override public bool HasField(int id) {
      return obj.HasField(id);
    }

    override public int GetTagId() {
      return tag;
    }

    override public Obj GetTag() {
      return new SymbObj(tag);
    }

    override public Obj GetInnerObj() {
      return obj;
    }

    override public Obj LookupField(int id) {
      return obj.LookupField(id);
    }

    override public string ToString() {
      if (IsString())
        return "\"" + GetString() + "\"";
      else
        return SymbTable.IdxToStr(tag) + "(" + obj.ToString() + ")";
    }

    bool IsString() {
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

    override protected int TypeId() {
      return 8;
    }

    override protected int InternalCmp(Obj other) {
      return other.CmpTaggedObj(tag, obj);
    }

    override public int CmpTaggedObj(int other_tag, Obj other_obj) {
      if (other_tag != tag)
        return other_tag < tag ? 1 : -1;
      else
        return other_obj.Cmp(obj);
    }
  }
}
