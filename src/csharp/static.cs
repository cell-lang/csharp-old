using System;
using System.Collections.Generic;


namespace CellLang {
  public static class Static {
    public static void Fail() {
      throw new InvalidOperationException();
    }

    public static Obj CreateSet(List<Obj> os) {
      return null;
    }

    public static Obj CreateSet(Obj[] os, long n) {
      return null;
    }

    public static Obj CreateSeq(List<Obj> os) {
      return null;
    }

    public static Obj CreateSeq(Obj[] os, long n) {
      return null;
    }

    public static Obj CreateMap(List<Obj> ks, List<Obj> vs) {
      return null;
    }

    public static Obj CreateMap(Obj[] ks, Obj[] vs, int n) {
      return null;
    }

    public static Obj CreateBinRel(List<Obj> c1, List<Obj> c2) {
      return null;
    }

    public static Obj CreateBinRel(Obj[] c1, Obj[] c2, int n) {
      return null;
    }

    public static Obj CreateTernRel(List<Obj> c1, List<Obj> c2, List<Obj> c3) {
      return null;
    }

    public static Obj CreateTernRel(Obj[] c1, Obj[] c2, Obj[] c3) {
      return null;
    }

    public static Obj Parse(Obj text) {
      return null;
    }

    public static Obj BuildConstIntSeq(byte[] vals) {
      return null;
    }

    public static Obj BuildConstIntSeq(ushort[] vals) {
      return null;
    }

    public static Obj BuildConstIntSeq(uint[] vals) {
      return null;
    }

    public static Obj BuildConstIntSeq(sbyte[] vals) {
      return null;
    }

    public static Obj BuildConstIntSeq(short[] vals) {
      return null;
    }

    public static Obj BuildConstIntSeq(int[] vals) {
      return null;
    }

    public static Obj StrToObj(string str) {
      return null;
    }

    public static Obj ParseSymb(Obj obj) {
      return null;
    }





  // [ProcSymbol -> (NeType*, Maybe[NeType])] builtin_procedures_signatures = [
  //   proc_symbol(:file_read)    -> ((type_string), just(type_maybe(type_seq(type_byte)))),
  //   proc_symbol(:file_write)   -> ((type_string, type_seq(type_nat)), just(type_bool)),
  //   proc_symbol(:print)        -> ((type_string), nothing),
  //   proc_symbol(:get_char)     -> ((), just(type_maybe(type_nat)))
  // ];

  }
}

/*
using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public class ArrayBuilder {
    Obj[] objs;
    int count;

    public ArrayBuilder(int len) {
      objs = new Obj[len];
      count = 0;
    }

    public void Add(Obj obj) {
      objs[count++] = obj;
    }

    public Obj Create() {
      Debug.Assert(count == objs.Length);
      Obj obj = new MasterSeqObj(objs);
      objs = null; //## REMOVE ONCE THE CODE HAS BEEN TESTED AND DEBUGGED
      return obj;
    }
  }


  public class ListBuilder {
    List<Obj> list = new List<Obj>();

    public void Add(Obj obj) {
      list.Add(obj);
    }

    public Obj Create() {
      Obj obj = new MasterSeqObj(list.ToArray());
      list = null; //## REMOVE ONCE THE CODE HAS BEEN TESTED AND DEBUGGED
      return obj;
    }
  }


  public class SetBuilder {
    List<Obj> list = new List<Obj>();

    public void Add(Obj obj) {
      list.Add(obj);
    }

    public Obj Create() {
      int count = list.Count;
      if (count == 0)
        return EmptyRelObj.Singleton();
      list.Sort();
      Obj[] objs = list.ToArray();
      list = null; //## REMOVE ONCE THE CODE HAS BEEN TESTED AND DEBUGGED
      for (int i=1 ; i < count ; i++)
        if (objs[i-1].IsEq(objs[i])) {
          int n = i;
          for (int j=i+1 ; j < count ; j++)
            if (!objs[n-1].IsEq(objs[j]))
              objs[n++] = objs[j];
          Obj[] unique_objs = new Obj[n];
          Array.Copy(objs, unique_objs, n);
          return new NeSetObj(unique_objs);
        }
      return new NeSetObj(objs);
    }
  }


  public class BinRelBuilder {
    List<Obj> list1 = new List<Obj>();
    List<Obj> list2 = new List<Obj>();

    public void Add(Obj obj1, Obj obj2) {
      list1.Add(obj1);
      list2.Add(obj2);
    }

    public Obj Create() {
      int count = list1.Count;
      if (count == 0)
        return EmptyRelObj.Singleton();

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      Comparison<int> cmp = delegate(int i1, int i2) {
        int res = list1[i1].CompareTo(list1[i2]);
        if (res != 0)
          return res;
        return list2[i1].CompareTo(list2[i2]);
      };

      Array.Sort(idxs, cmp);

      int unique_count = count;
      for (int i=1 ; i < count ; i++) {
        int j = idxs[i];
        if (list1[j].IsEq(list1[j-1]) && list2[j].IsEq(list2[j-1])) {
          int n = i;
          for (int k=i+1 ; k < count ; k++) {
            j = idxs[k];
            if (!list1[j].IsEq(list1[j-1]) || !list2[j].IsEq(list2[j-1]))
              idxs[n++] = k;
          }
          unique_count = n;
          goto Next;
        }
      }

Next:
      Obj[] col1 = new Obj[unique_count];
      Obj[] col2 = new Obj[unique_count];

      for (int i=0 ; i < unique_count ; i++) {
        int j = idxs[i];
        col1[i] = list1[j];
        col2[i] = list2[j];
      }

      list2 = list1 = null; //## REMOVE ONCE THE CODE HAS BEEN TESTED AND DEBUGGED

      bool isMap = true;
      for (int i=1 ; i < unique_count ; i++)
        if (col1[i].IsEq(col1[i-1])) {
          isMap = false;
          break;
        }

      return new NeBinRelObj(col1, col2, isMap);
    }
  }


  public class TernRelBuilder {
    List<Obj> list1 = new List<Obj>();
    List<Obj> list2 = new List<Obj>();
    List<Obj> list3 = new List<Obj>();

    public void Add(Obj obj1, Obj obj2, Obj obj3) {
      list1.Add(obj1);
      list2.Add(obj2);
      list3.Add(obj3);
    }

    public Obj Create() {
      int count = list1.Count;
      if (count == 0)
        return EmptyRelObj.Singleton();

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      Comparison<int> cmp = delegate(int i1, int i2) {
        int res = list1[i1].CompareTo(list1[i2]);
        if (res != 0)
          return res;
        return list2[i1].CompareTo(list2[i2]);
      };

      Array.Sort(idxs, cmp);

      int unique_count = count;
      for (int i=1 ; i < count ; i++) {
        int j = idxs[i];
        if (list1[j].IsEq(list1[j-1]) && list2[j].IsEq(list2[j-1]) && list3[j].IsEq(list3[j-1])) {
          unique_count = i;
          for (int k=i+1 ; k < count ; k++) {
            j = idxs[k];
            if (!list1[j].IsEq(list1[j-1]) || !list2[j].IsEq(list2[j-1]) || !list3[j].IsEq(list3[j-1]))
              idxs[unique_count++] = k;
          }
          goto Next;
        }
      }

Next:
      Obj[] col1 = new Obj[unique_count];
      Obj[] col2 = new Obj[unique_count];
      Obj[] col3 = new Obj[unique_count];

      for (int i=0 ; i < unique_count ; i++) {
        int j = idxs[i];
        col1[i] = list1[j];
        col2[i] = list2[j];
        col3[i] = list3[j];
      }

      list3 = list2 = list1 = null; //## REMOVE ONCE THE CODE HAS BEEN TESTED AND DEBUGGED

      return new NeTernRelObj(col1, col2, col3);
    }
  }
}
*/