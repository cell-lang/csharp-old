using System;
using System.Collections.Generic;
using System.Diagnostics;


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
    public virtual bool IsTernRel()                               {return false;}
    public virtual bool IsNeTernRel()                             {return false;}
    public virtual bool IsTagged()                                {return false;}

    public virtual bool IsSymb(int id)                            {return false;}
    public virtual bool IsInt(long n)                             {return false;}
    public virtual bool IsFloat(double x)                         {return false;}

    public virtual bool HasElem(Obj o)                            {throw new InvalidOperationException();}
    public virtual bool HasKey(Obj o)                             {throw new InvalidOperationException();}
    public virtual bool HasField(int id)                          {throw new InvalidOperationException();}
    public virtual bool HasPair(Obj o1, Obj o2)                   {throw new InvalidOperationException();}
    public virtual bool HasTriple(Obj o1, Obj o2, Obj o3)         {throw new InvalidOperationException();}

    public virtual int    GetSymbId()                             {throw new InvalidOperationException();}
    public virtual bool   GetBool()                               {throw new InvalidOperationException();}
    public virtual long   GetInt()                                {throw new InvalidOperationException();}
    public virtual double GetFloat()                              {throw new InvalidOperationException();}
    public virtual int    GetSize()                               {throw new InvalidOperationException();}
    public virtual Obj    GetItem(long i)                         {throw new InvalidOperationException();}
    public virtual int    GetTagId()                              {throw new InvalidOperationException();}
    public virtual Obj    GetTag()                                {throw new InvalidOperationException();}
    public virtual Obj    GetInnerObj()                           {throw new InvalidOperationException();}

    public virtual SeqOrSetIter GetSeqOrSetIter()                 {throw new InvalidOperationException();}
    public virtual BinRelIter   GetBinRelIter()                   {throw new InvalidOperationException();}
    public virtual TernRelIter  GetTernRelIter()                  {throw new InvalidOperationException();}

    //## IMPLEMENT
    // Copy-on-write update
    public virtual Obj SetItem(long i, Obj v)                     {throw new InvalidOperationException();}

    public virtual Obj Negate()                                   {throw new InvalidOperationException();}
    public virtual Obj InternalSort()                             {throw new InvalidOperationException();}
    public virtual Obj GetSlice(long first, long len)             {throw new InvalidOperationException();}
    public virtual Obj Reverse()                                  {throw new InvalidOperationException();}
    public virtual Obj TextRepr()                                 {throw new InvalidOperationException();}

    public virtual BinRelIter GetBinRelIter0(Obj obj)             {throw new InvalidOperationException();}
    public virtual BinRelIter GetBinRelIter1(Obj obj)             {throw new InvalidOperationException();}


    public virtual Obj Lookup(Obj key)                            {throw new InvalidOperationException();}
    public virtual Obj LookupField(int id)                        {throw new InvalidOperationException();}

    public virtual Obj Append(Obj obj)                            {throw new InvalidOperationException();}
    public virtual Obj Append(Obj[] objs)                         {throw new InvalidOperationException();}

    public virtual bool IsEq(Obj o) {
      return Cmp(o) == 0;
    }

    public int CompareTo(Obj other) {
      return Cmp(other);
    }

    public int Cmp(Obj o) {
      int id1 = TypeId();
      int id2 = o.TypeId();
      if (id1 == id2)
        return InternalCmp(o);
      return id1 < id2 ? 1 : -1;
    }

    public virtual int CmpSeq(Obj[] es, int o, int l)             {throw new InvalidOperationException();}
    public virtual int CmpNeSet(Obj[] es)                         {throw new InvalidOperationException();}
    public virtual int CmpNeBinRel(Obj[] c1, Obj[] c2)            {throw new InvalidOperationException();}
    public virtual int CmpNeTernRel(Obj[] c1, Obj[] c2, Obj[] c3) {throw new InvalidOperationException();}
    public virtual int CmpTaggedObj(int tag, Obj obj)             {throw new InvalidOperationException();}

    protected abstract int TypeId();
    protected abstract int InternalCmp(Obj o);
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
      if (id == 0)
        return false;
      if (id == 1)
        return true;
      throw new InvalidOperationException();
    }

    override public bool IsEq(Obj obj) {
      return obj.IsSymb(id);
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

    override public long GetInt() {
      return value;
    }

    override public bool IsEq(Obj obj) {
      return obj.IsInt(value);
    }

    override protected int TypeId() {
      return 1;
    }

    override protected int InternalCmp(Obj obj) {
      long other_value = obj.GetInt();
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

    override protected int TypeId() {
      return 2;
    }

    override protected int InternalCmp(Obj other) {
      double other_value = other.GetFloat();
      return value == other_value ? 0 : (value < other_value ? 1 : -1);
    }
  }


  abstract class SeqObj : Obj {
    internal Obj[] items;
    internal int length;

    protected SeqObj(Obj[] items, int length) {
      this.items = items;
      this.length = length;
    }

    override public bool IsSeq() {
      return true;
    }

    override public bool IsEmptySeq() {
      return length == 0;
    }

    override public bool IsNeSeq() {
      return length != 0;
    }

    override public int GetSize() {
      return length;
    }

    override public Obj GetItem(long idx) {
      if (idx < length)
        return items[Offset()+idx];
      else
        throw new Exception();
    }

    override protected int TypeId() {
      return 3;
    }

    override protected int InternalCmp(Obj other) {
      return other.CmpSeq(items, Offset(), length);
    }

    override public int CmpSeq(Obj[] other_items, int other_offset, int other_length) {
      int offset = Offset();
      if (other_length != length)
        return other_length < length ? 1 : -1;
      for (int i=0 ; i < length ; i++) {
        int res = other_items[other_offset+i].Cmp(items[offset+i]);
        if (res != 0)
          return res;
      }
      return 0;
    }

    protected abstract int Offset();

    static Obj emptySeq = new MasterSeqObj(new Obj[] {});

    public static Obj Empty() {
      return emptySeq;
    }
  }


  class MasterSeqObj : SeqObj {
    internal int used;

    public MasterSeqObj(Obj[] items, int length) : base(items, length) {
      this.used = length;
    }

    public MasterSeqObj(Obj[] items) : this(items, items.Length) {

    }

    public MasterSeqObj(long length) : base(new Obj[length], (int)length) {

    }

    override public Obj GetItem(long idx) {
      if (idx < length)
        return items[idx];
      else
        throw new Exception();
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return new SeqOrSetIter(items, 0, length-1);
    }

    override public Obj Append(Obj obj) {
      if (used == length && length + 1 < items.Length) {
        items[length] = obj;
        return new SliceObj(this, 0, length+1);
      }
      else {
        Obj[] newItems = new Obj[length < 16 ? 32 : (3 * length) / 2];
        for (int i=0 ; i < length ; i++)
          newItems[i] = items[i];
        newItems[length] = obj;
        return new MasterSeqObj(newItems, length+1);
      }
    }

    override public Obj Append(Obj[] objs) {
      int newLen = length + objs.Length;
      if (used == length && newLen < items.Length) {
        for (int i=0; i < objs.Length ; i++)
          items[length+i] = objs[i];
        return new SliceObj(this, 0, newLen);
      }
      else {
        Obj[] newItems = new Obj[newLen <= 16 ? 32 : (3 * newLen) / 2];
        for (int i=0 ; i < length ; i++)
          newItems[i] = items[i];
        for (int i=0 ; i < objs.Length ; i++)
          newItems[length+i] = objs[i];
        return new MasterSeqObj(newItems, newLen);
      }
    }

    override protected int Offset() {
      return 0;
    }
  }


  class SliceObj : SeqObj {
    MasterSeqObj master;
    int offset;

    public SliceObj(MasterSeqObj master, int offset, int length) : base(master.items, length) {
      this.master = master;
      this.offset = offset;
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return new SeqOrSetIter(items, offset, offset+length-1);
    }

    override public Obj Append(Obj obj) {
      int used = offset + length;
      if (master.used == used && used + 1 < master.items.Length) {
        master.items[used] = obj;
        return new SliceObj(master, offset, length+1);
      }
      else {
        Obj[] newItems = new Obj[length < 16 ? 32 : (3 * length) / 2];
        for (int i=0 ; i < length ; i++)
          newItems[i] = items[i];
        newItems[length] = obj;
        return new MasterSeqObj(newItems, length+1);

      }
    }

    override public Obj Append(Obj[] objs) {
      int used = offset + length;
      int newLen = used + objs.Length;
      if (master.used == used && newLen <= master.items.Length) {
        for (int i=0 ; i < objs.Length ; i++)
          master.items[used+i] = objs[i];
        return new SliceObj(master, offset, newLen);
      }
      else {
        Obj[] newItems = new Obj[newLen <= 16 ? 32 : (3 * length) / 2];
        for (int i=0 ; i < length ; i++)
          newItems[i] = items[offset+i];
        for (int i=0 ; i < objs.Length ; i++)
          newItems[length+i] = objs[i];
        return new MasterSeqObj(newItems, newLen);
      }
    }

    override protected int Offset() {
      return offset;
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

    override public TernRelIter GetTernRelIter() {
      return iter3;
    }

    override protected int TypeId() {
      return 4;
    }

    override protected int InternalCmp(Obj other) {
      return 0;
    }

    static SeqOrSetIter iter1 = new SeqOrSetIter(new Obj[0], 0, 0);
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
      Debug.Assert(elts == null || elts.Length > 0);
      this.elts = elts;
    }

    override public bool IsSet() {
      return true;
    }

    override public bool IsNeSet() {
      return true;
    }

    override public bool HasElem(Obj obj) {
      return ObjUtils.BinSearch(elts, obj) != -1;
    }

    override public int GetSize() {
      return elts.Length;
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return new SeqOrSetIter(elts, 0, elts.Length-1);
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
      for (int i=0 ; i < 0 ; i++) {
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
    bool isMap;

    public NeBinRelObj(Obj[] col1, Obj[] col2, bool isMap) {
      Debug.Assert(col1 != null && col2 != null);
      Debug.Assert(col1.Length > 0);
      Debug.Assert(col1.Length == col2.Length);
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
      Debug.Assert(isMap);
      return ObjUtils.BinSearch(col1, obj) != -1;
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
        int idx = ObjUtils.BinSearch(col1, obj1);
        return idx != -1 && col2[idx].IsEq(obj2);
      }
      else {
        int first;
        int count = ObjUtils.BinSearchRange(col1, 0, col1.Length, obj1, out first);
        if (count == 0)
          return false;
        int idx = ObjUtils.BinSearch(col2, first, count, obj2);
        return idx != -1;
      }
    }

    override public int GetSize() {
      return col1.Length;
    }

    override public BinRelIter GetBinRelIter() {
      return new BinRelIter(col1, col2);
    }

    override public Obj Lookup(Obj key) {
      int idx = ObjUtils.BinSearch(col1, key);
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
      for (int i=0 ; i < 0 ; i++) {
        int res = other_col_1[i].Cmp(col1[i]);
        if (res != 0)
          return res;
      }
      for (int i=0 ; i < 0 ; i++) {
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

    public NeTernRelObj(Obj[] col1, Obj[] col2, Obj[] col3) {
      Debug.Assert(col1 != null && col2 != null && col3 != null);
      Debug.Assert(col1.Length == col2.Length && col1.Length == col3.Length);
      Debug.Assert(col1.Length > 0);
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

      int count = ObjUtils.BinSearchRange(col1, 0, col1.Length, obj1, out first);
      if (count == 0)
        return false;

      count = ObjUtils.BinSearchRange(col2, first, count, obj2, out first);
      if (count == 0)
        return false;

      int idx = ObjUtils.BinSearch(col3, first, count, obj3);
      return idx != -1;
    }

    override public int GetSize() {
      return col1.Length;
    }

    override public TernRelIter GetTernRelIter() {
      return new TernRelIter(col1, col2, col3);
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
      for (int i=0 ; i < 0 ; i++) {
        int res = other_col_1[i].Cmp(col1[i]);
        if (res != 0)
          return res;
      }
      for (int i=0 ; i < 0 ; i++) {
        int res = other_col_2[i].Cmp(col2[i]);
        if (res != 0)
          return res;
      }
      for (int i=0 ; i < 0 ; i++) {
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
      this.tag = tag;
      this.obj = obj;
    }

    public TaggedObj(Obj tag, Obj obj) {
      this.tag = tag.GetSymbId();
      this.obj = obj;
    }

    override public bool IsTagged() {
      return true;
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


  static class ObjUtils {
    public static int BinSearch(Obj[] objs, Obj obj) {
      return BinSearch(objs, 0, objs.Length, obj);
    }

    public static int BinSearch(Obj[] objs, int first, int count, Obj obj) {
      int low = first;
      int high = first + count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[mid].Cmp(obj)) {
          case -1:
            // objs[mid] > obj
            high = mid - 1;
            break;

          case 0:
            return mid;

          case 1:
            // objs[mid] < obj
            low = mid + 1;
            break;
        }
      }

      return -1;
    }

    public static int BinSearchRange(Obj[] objs, int offset, int length, Obj obj, out int first) {
      int low = offset;
      int high = offset + length - 1;
      int lower_bound = low;
      int upper_bound = high;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[mid].Cmp(obj)) {
          case -1:
            // objs[mid] > obj
            upper_bound = high = mid - 1;
            break;

          case 0:
            if (mid == offset || !objs[mid-1].IsEq(obj)) {
              first = mid;
              low = lower_bound;
              high = upper_bound;
              goto Next;
            }
            else
              high = mid - 1;
            break;

          case 1:
            // objs[mid] < obj
            lower_bound = low = mid + 1;
            break;
        }
      }

      first = -1; //## IS THIS NECESSARY?
      return 0;

    Next:
      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[mid].Cmp(obj)) {
          case -1:
            // objs[mid] > obj
            high = mid - 1;
            break;

          case 0:
            if (mid == upper_bound || !objs[mid+1].IsEq(obj)) {
              return mid - first + 1;
            }
            else
              low = mid + 1;
            break;

          case 1:
            // objs[mid] < obj
            low = mid + 1;
            break;
        }
      }

      // We're not supposed to ever get here.
      throw new InvalidOperationException();
    }
  }


  public static class SymbTable {
    static string[] defaultSymbols = {
      "false",
      "true",
      "void",
      "string",
      "nothing",
      "just",
      "success",
      "failure"
    };

    static List<String> symbTable = new List<String>();
    static Dictionary<String, int> symbMap = new Dictionary<String, int>();

    static SymbTable() {
      int len = defaultSymbols.Length;
      for (int i=0 ; i < len ; i++) {
        string str = defaultSymbols[i];
        symbTable.Add(str);
        symbMap.Add(str, i);
      }
    }

    public static int StrToIdx(string str) {
      int idx;
      if (symbMap.TryGetValue(str, out idx))
        return idx;
      int count = symbTable.Count;
      if (count < 65535) {
        idx = count;
        symbTable.Add(str);
        symbMap.Add(str, idx);
        return idx;
      }
      throw new InvalidOperationException();
    }

    public static string IdxToStr(int idx) {
      return symbTable[idx];
    }
  }
}
