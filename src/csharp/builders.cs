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
