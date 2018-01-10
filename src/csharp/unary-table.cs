using System;
using System.Collections.Generic;


namespace CellLang {
  class UnaryTable {
    public struct Iter {
      uint index;
      UnaryTable table;

      public Iter(uint index, UnaryTable table) {
        this.index = index;
        this.table = table;
      }

      public uint Get() {
        return index;
      }

      public bool Done() {
        return index >= 64 * table.bitmap.Length;
      }

      public void Next() {
        int size = 64 * table.bitmap.Length;
        do {
          index++;
        } while (index < size && !table.Contains(index));
      }
    }


    const int InitSize = 4;

    ulong[] bitmap = new ulong[InitSize];
    uint count = 0;

    public ValueStore store;

    public UnaryTable(ValueStore store) {
      this.store = store;
    }

    public bool Contains(uint surr) {
      uint widx = surr / 64;
      return widx < bitmap.Length && ((bitmap[widx] >> (int) (surr % 64) & 1) != 0);
    }

    public Iter GetIter() {
      return new Iter(0, this);
    }

    uint LiveCount() {
      uint liveCount = 0;
      for (int i=0 ; i < bitmap.Length ; i++) {
        ulong mask = bitmap[i];
        for (int j=0 ; j < 64 ; j++)
          if (((mask >> j) & 1) != 0)
            liveCount++;
      }
      return liveCount;
    }

    public void Insert(uint surr) {
      Miscellanea.Assert(surr < 64 * bitmap.Length);

      uint widx = surr / 64;
      int bidx = (int) (surr % 64);
      ulong mask = bitmap[widx];
      if (((mask >> bidx) & 1) == 0) {
        bitmap[widx] = mask | (1UL << bidx);
        count++;
      }
      Miscellanea.Assert(count == LiveCount());
    }

    public void Delete(uint surr) {
      Miscellanea.Assert(surr < 64 * bitmap.Length);

      uint widx = surr / 64;
      if (widx < bitmap.Length) {
        ulong mask = bitmap[widx];
        int bidx = (int) surr % 64;
        if (((mask >> bidx) & 1) == 1) {
          bitmap[widx] = mask & (ulong) ~(1 << bidx);
          count--;
        }
      }
      Miscellanea.Assert(count == LiveCount());
    }

    public Obj Copy() {
      if (count == 0)
        return EmptyRelObj.Singleton();
      Obj[] objs = new Obj[count];
      int next = 0;
      for (uint i=0 ; i < bitmap.Length ; i++) {
        ulong mask = bitmap[i];
        for (uint j=0 ; j < 64 ; j++)
          if (((mask >> (int) j) & 1) != 0)
            objs[next++] = store.GetValue(j + 64 * i);
      }
      Miscellanea.Assert(next == count);
      return Builder.CreateSet(objs, objs.Length);
    }

//    public static string IntToBinaryString(int number) {
//      string binStr = "";
//      while (number != 0) {
//        binStr = (number & 1) + binStr;
//        number = number >> 1;
//      }
//      if (binStr == "")
//        binStr = "0";
//      return binStr;
//    }
//
//    public static string IntToBinaryString(ulong number) {
//      string binStr = "";
//      while (number > 0) {
//        binStr = (number & 1) + binStr;
//        number = number >> 1;
//      }
//      if (binStr == "")
//        binStr = "0";
//      return binStr;
//    }
  }


  class UnaryTableUpdater {
    List<uint> deleteList = new List<uint>();
    List<uint> insertList = new List<uint>();

    UnaryTable table;
    ValueStoreUpdater store;

    public UnaryTableUpdater(UnaryTable table, ValueStoreUpdater store) {
      this.table = table;
      this.store = store;
    }

    public void Clear() {
      deleteList.Clear();
      UnaryTable.Iter it = table.GetIter();
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public void Set(Obj value) {
      Clear();
      Miscellanea.Assert(insertList.Count == 0);
      SeqOrSetIter it = value.GetSeqOrSetIter();
      while (!it.Done()) {
        Obj val = it.Get();
        int surr = store.LookupValueEx(val);
        if (surr == -1)
          surr = store.Insert(val);
        insertList.Add((uint) surr);
        it.Next();
      }
    }

    public void Delete(long value) {
      if (table.Contains((uint) value))
        deleteList.Add((uint) value);
    }

    public void Insert(long value) {
      insertList.Add((uint) value);
    }

    public void Apply() {
      for (int i=0 ; i < deleteList.Count ; i++) {
        uint surr = deleteList[i];
        if (table.Contains(surr))
          table.Delete(surr);
        else
          deleteList[i] = 0xFFFFFFFF;
      }

      var it = insertList.GetEnumerator();
      while (it.MoveNext()) {
        uint surr = it.Current;
        if (!table.Contains(surr)) {
          table.Insert(surr);
          table.store.AddRef(surr);
        }
      }
    }

    public void Finish() {
      var it = deleteList.GetEnumerator();
      while (it.MoveNext()) {
        uint surr = it.Current;
        if (surr != 0xFFFFFFFF)
          table.store.Release(surr);
      }
    }

    public void Reset() {
      deleteList.Clear();
      insertList.Clear();
    }
  }
}
