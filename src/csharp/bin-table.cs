using System;
using System.Collections.Generic;


namespace CellLang {
  struct OneWayBinTable {
    const int MinCapacity = 256;

    public const uint EmptySlot      = 0xFFFFFFFF;
    public const uint MultiValueSlot = 0xFFFFFFFE;
    //const uint MaxSurrId = 0x1FFFFFFF;

    static uint[] emptyArray = new uint[0];

    public uint[] column;
    public Dictionary<uint, HashSet<uint>> multimap;
    public int count;

    public void Init() {
      column = emptyArray;
      multimap = new Dictionary<uint, HashSet<uint>>();
      count = 0;
    }

    public void InitReverse(OneWayBinTable source) {
      Miscellanea.Assert(count == 0);

      uint[] srcCol = source.column;
      Dictionary<uint, HashSet<uint>> srcMultimap = source.multimap;

      for (uint i=0 ; i < column.Length ; i++) {
        uint code = srcCol[i];
        if (code == MultiValueSlot) {
          HashSet<uint>.Enumerator it = srcMultimap[i].GetEnumerator();
          while (it.MoveNext())
            Insert(it.Current, i);
        }
        else if (code != EmptySlot)
          Insert(code, i);
      }
    }

    public bool Contains(uint surr1, uint surr2) {
      if (surr1 >= column.Length)
        return false;
      uint code = column[surr1];
      if (code == EmptySlot)
        return false;
      if (code == MultiValueSlot)
        return multimap[surr1].Contains(surr2);
      return code == surr2;
    }

    public uint[] Lookup(uint surr) {
      if (surr >= column.Length)
        return emptyArray;
      uint code = column[surr];
      if (code == EmptySlot)
        return emptyArray;
      if (code != MultiValueSlot)
        return new uint[] {code};

      HashSet<uint> surrSet = multimap[surr];
      int count = surrSet.Count;
      uint[] surrs = new uint[count];
      HashSet<uint>.Enumerator it = surrSet.GetEnumerator();
      for (int i=0 ; i < count ; i++) {
        surrs[i] = it.Current;
        it.MoveNext();
      }
      return surrs;
    }

    public void Insert(uint surr1, uint surr2) {
      int size = column.Length;
      if (surr1 >= size) {
        int newSize = size == 0 ? MinCapacity : 2 * size;
        while (surr1 >= newSize)
          newSize *= 2;
        uint[] newColumn = new uint[newSize];
        Array.Copy(column, newColumn, size);
        for (int i=size ; i < newSize ; i++)
          newColumn[i] = EmptySlot;
        column = newColumn;
      }

      uint code = column[surr1];
      if (code == EmptySlot) {
        column[surr1] = surr2;
        count++;
      }
      else if (code == MultiValueSlot) {
        HashSet<uint> surrs = multimap[surr1];
        if (surrs.Add(surr2))
          count++;
      }
      else if (code != surr2) {
        column[surr1] = MultiValueSlot;
        HashSet<uint> surrs = new HashSet<uint>();
        surrs.Add(surr2);
        multimap[surr1] = surrs;
        count++;
      }
    }

    public void Clear() {
      column = emptyArray;
      multimap.Clear();
      count = 0;
    }

    public void Delete(uint surr1, uint surr2) {
      uint code = column[surr1];
      if (code == surr2) {
        column[surr1] = EmptySlot;
        count--;
      }
      else if (code == MultiValueSlot) {
        HashSet<uint> surrs = multimap[surr1];
        if (surrs.Remove(surr2)) {
          count--;
          if (surrs.Count == 1) {
            column[surr1] = surrs.GetEnumerator().Current;
            multimap.Remove(surr1);
          }
        }
      }
    }
  }


  class BinaryTable {
    OneWayBinTable table1;
    OneWayBinTable table2;

    public ValueStore store1;
    public ValueStore store2;

    public BinaryTable(ValueStore store1, ValueStore store2) {
      table1.Init();
      table2.Init();
      this.store1 = store1;
      this.store2 = store2;
    }

    public bool Contains(uint surr1, uint surr2) {
      return table1.Contains(surr1, surr2);
    }

    public uint[] LookupByCol1(uint surr) {
      return table1.Lookup(surr);
    }

    public uint[] LookupByCol2(uint surr) {
      if (table2.count == 0 & table1.count > 0)
        table2.InitReverse(table1);
      return table2.Lookup(surr);
    }

    public void Insert(uint surr1, uint surr2) {
      table1.Insert(surr1, surr2);
      if (table2.count > 0)
        table2.Insert(surr2, surr1);
    }

    public void Clear() {
      table1.Clear();
      table2.Clear();
    }

    public void Delete(uint surr1, uint surr2) {
      table1.Delete(surr1, surr2);
      if (table2.count > 0)
        table2.Delete(surr2, surr1);
    }

    public Obj Copy(bool flipped) {
      int count = table1.count;

      if (count == 0)
        return EmptyRelObj.Singleton();

      Obj[] objs1 = new Obj[count];
      Obj[] objs2 = new Obj[count];

      int next = 0;
      for (uint i=0 ; i < table1.column.Length ; i++) {
        uint code = table1.column[i];
        if (code != OneWayBinTable.EmptySlot) {
          Obj val1 = store1.GetValue(i);
          if (code != OneWayBinTable.MultiValueSlot) {
            objs1[next] = val1;
            objs2[next++] = store2.GetValue(code);
          }
          else {
            foreach (uint surr2 in table1.multimap[i]) {
              objs1[next] = val1;
              objs2[next++] = store2.GetValue(surr2);
            }
          }
        }
      }
      Miscellanea.Assert(next == count);

      return Builder.CreateBinRel(flipped ? objs1 : objs2, flipped ? objs2 : objs1, count); //## THIS COULD BE MADE MORE EFFICIENT
    }
  }


  class BinaryTableUpdater {
    struct Tuple {
      public uint field1;
      public uint field2;

      public Tuple(uint field1, uint field2) {
        this.field1 = field1;
        this.field2 = field2;
      }
    }

    List<Tuple> deleteList = new List<Tuple>();
    List<Tuple> insertList = new List<Tuple>();

    BinaryTable table;

    public BinaryTableUpdater(BinaryTable table) {
      this.table = table;
    }

    public void Clear() {
      throw new NotImplementedException();
    }

    public void Set(Obj value, bool flipped) {

    }

    public void Delete(long value1, long value2) {
      if (table.Contains((uint) value1, (uint) value2))
        deleteList.Add(new Tuple((uint) value1, (uint) value2));
    }

    public void DeleteByCol1(long value) {
      uint[] assocs = table.LookupByCol1((uint) value);
      for (int i=0 ; i < assocs.Length ; i++)
        deleteList.Add(new Tuple((uint) value, assocs[i]));
    }

    public void DeleteByCol2(long value) {
      uint[] assocs = table.LookupByCol2((uint) value);
      for (int i=0 ; i < assocs.Length ; i++)
        deleteList.Add(new Tuple(assocs[i], (uint) value));
    }

    public void Insert(long value1, long value2) {
      insertList.Add(new Tuple((uint) value1, (uint) value2));
    }

    public bool CheckUpdates_0() {
      throw new NotImplementedException();
    }

    public bool CheckUpdates_1() {
      throw new NotImplementedException();
    }

    public bool CheckUpdates_0_1() {
      throw new NotImplementedException();
    }

    public void Apply() {
      var it = deleteList.GetEnumerator();
      while (it.MoveNext()) {
        var curr = it.Current;
        table.Delete(curr.field1, curr.field2);
      }

      it = insertList.GetEnumerator();
      while (it.MoveNext()) {
        var curr = it.Current;
        if (!table.Contains(curr.field1, curr.field2)) {
          table.Insert(curr.field1, curr.field2);
          table.store1.AddRef(curr.field1);
          table.store2.AddRef(curr.field2);
        }
      }
    }

    public void Finish() {
      var it = deleteList.GetEnumerator();
      while (it.MoveNext()) {
        var curr = it.Current;
        table.store1.Release(curr.field1);
        table.store2.Release(curr.field2);
      }
    }

    public void Reset() {
      deleteList.Clear();
      insertList.Clear();
    }
  }
}
