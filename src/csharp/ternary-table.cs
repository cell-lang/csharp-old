using System;
using System.Collections.Generic;


namespace CellLang {
  class TernaryTable {
    public struct Tuple {
      public const uint Empty = 0xFFFFFFFF;

      public uint field1OrNext;
      public uint field2OrEmptyMarker;
      public uint field3;

      public Tuple(uint field1, uint field2, uint field3) {
        this.field1OrNext = field1;
        this.field2OrEmptyMarker = field2;
        this.field3 = field3;
      }

      override public string ToString() {
        return "(" + field1OrNext.ToString() + ", " + field2OrEmptyMarker.ToString() + ", " + field3.ToString() + ")";
      }
    }


    public struct Iter {
      public enum Type {F123, F12, F13, F23, F1, F2, F3};

      uint field1, field2, field3;

      uint index;
      Type type;

      TernaryTable table;

      public Iter(uint field1, uint field2, uint field3, uint index, Type type, TernaryTable table) {
        this.field1 = field1;
        this.field2 = field2;
        this.field3 = field3;
        this.index = index;
        this.type = type;
        this.table = table;
        if (index != Tuple.Empty) {
          Tuple tuple = table.tuples[index];
          bool ok1 = field1 == Tuple.Empty | tuple.field1OrNext == field1;
          bool ok2 = field2 == Tuple.Empty | tuple.field2OrEmptyMarker == field2;
          bool ok3 = field3 == Tuple.Empty | tuple.field3 == field3;
          if ((type == Type.F123 & tuple.field2OrEmptyMarker == Tuple.Empty) | !ok1 | !ok2 | !ok3) {
            Next();
          }
        }
      }

      public bool Done() {
        return index == Tuple.Empty;
      }

      public Tuple Get() {
        Miscellanea.Assert(index != Tuple.Empty);
        return table.tuples[index];
      }

      public uint GetField1() {
        Miscellanea.Assert(index != Tuple.Empty);
        return table.tuples[index].field1OrNext;
      }

      public uint GetField2() {
        Miscellanea.Assert(index != Tuple.Empty);
        return table.tuples[index].field2OrEmptyMarker;
      }

      public uint GetField3() {
        Miscellanea.Assert(index != Tuple.Empty);
        return table.tuples[index].field3;
      }

      public void Next() {
        Miscellanea.Assert(index != Tuple.Empty);
        switch (type) {
          case Type.F123:
            int len = table.tuples.Length;
            do {
              index++;
              if (index == len) {
                index = Tuple.Empty;
                return;
              }
            } while (table.tuples[index].field2OrEmptyMarker == Tuple.Empty);
            break;

          case Type.F12:
            for ( ; ; ) {
              index = table.index12.Next(index);
              if (index == Tuple.Empty)
                return;
              Tuple tuple = table.tuples[index];
              if (tuple.field1OrNext == field1 & tuple.field2OrEmptyMarker == field2)
                return;
            }
            break;

          case Type.F13:
            for ( ; ; ) {
              index = table.index13.Next(index);
              if (index == Tuple.Empty)
                return;
              Tuple tuple = table.tuples[index];
              if (tuple.field1OrNext == field1 & tuple.field3 == field3)
                return;
            }
            break;

          case Type.F23:
            for ( ; ; ) {
              index = table.index23.Next(index);
              if (index == Tuple.Empty)
                return;
              Tuple tuple = table.tuples[index];
              if (tuple.field2OrEmptyMarker == field2 & tuple.field3 == field3)
                return;
            }
            break;

          case Type.F1:
            do {
              index = table.index1.Next(index);
            } while (index != Tuple.Empty && table.tuples[index].field1OrNext != field1);
            break;

          case Type.F2:
            do {
              index = table.index2.Next(index);
            } while (index != Tuple.Empty && table.tuples[index].field2OrEmptyMarker != field2);
            break;

          case Type.F3:
            do {
              index = table.index3.Next(index);
            } while (index != Tuple.Empty && table.tuples[index].field3 != field3);
            break;
        }
      }

      public void Dump() {
        Console.WriteLine("fields = ({0}, {1}, {2})", field1, field2, field3);
        Console.WriteLine("index  = {0}", index);
        Console.WriteLine("type   = {0}", type);
        Console.WriteLine("Done() = {0}", Done());
      }
    }


    const int MinSize = 32;

    Tuple[] tuples = new Tuple[MinSize];
    uint count = 0;
    uint firstFree = 0;

    public Index index123, index12, index13, index23, index1, index2, index3;

    public ValueStore store1, store2, store3;

    public TernaryTable(ValueStore store1, ValueStore store2, ValueStore store3) {
      this.store1 = store1;
      this.store2 = store2;
      this.store3 = store3;

      for (uint i=0 ; i < MinSize ; i++) {
        tuples[i].field1OrNext = i + 1;
        tuples[i].field2OrEmptyMarker = Tuple.Empty;
      }

      for (uint i=0 ; i < MinSize ; i++) {
        Miscellanea.Assert(tuples[i].field1OrNext == i + 1);
        Miscellanea.Assert(tuples[i].field2OrEmptyMarker == Tuple.Empty);
      }

      index123.Init(MinSize);
      index12.Init(MinSize);
    }

    public void Insert(uint field1, uint field2, uint field3) {
      uint hashcode = Miscellanea.Hashcode(field1, field2, field3);

      // Making sure the tuple has not been inserted yet
      //## CAN'T I JUST CALL Contains() HERE?
      for (uint idx = index123.Head(hashcode) ; idx != Tuple.Empty ; idx = index123.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1 & tuple.field2OrEmptyMarker == field2 & tuple.field3 == field3)
          return;
      }

      // Increasing the size of the table if need be
      if (firstFree >= tuples.Length) {
        uint size = (uint) tuples.Length;
        Miscellanea.Assert(count == size);
        Tuple[] newTuples = new Tuple[2*size];
        Array.Copy(tuples, newTuples, size);
        for (uint i=size ; i < 2 * size ; i++) {
          newTuples[i].field1OrNext = i + 1;
          newTuples[i].field2OrEmptyMarker = Tuple.Empty;
          Miscellanea.Assert(newTuples[i].field1OrNext == i + 1);
          Miscellanea.Assert(newTuples[i].field2OrEmptyMarker == Tuple.Empty);
        }
        tuples = newTuples;
      }

      // Inserting the new tuple
      uint index = firstFree;
      firstFree = tuples[firstFree].field1OrNext;
      tuples[index] = new Tuple(field1, field2, field3);
      count++;

      // Updating the indexes
      index123.Insert(index, hashcode);
      index12.Insert(index, Miscellanea.Hashcode(field1, field2));
      if (!index13.IsBlank())
        index13.Insert(index, Miscellanea.Hashcode(field1, field3));
      if (!index23.IsBlank())
        index23.Insert(index, Miscellanea.Hashcode(field2, field3));
      if (!index1.IsBlank())
        index1.Insert(index, Miscellanea.Hashcode(field1));
      if (!index2.IsBlank())
        index2.Insert(index, Miscellanea.Hashcode(field2));
      if (!index3.IsBlank())
        index3.Insert(index, Miscellanea.Hashcode(field3));

      // Updating the reference count in the value stores
      store1.AddRef(field1);
      store2.AddRef(field2);
      store3.AddRef(field3);
    }

    public void Clear() {
      count = 0;
      firstFree = 0;

      int size = tuples.Length;
      for (uint i=0 ; i < size ; i++) {
        tuples[i].field1OrNext = i + 1;
        tuples[i].field2OrEmptyMarker = Tuple.Empty;
      }

      index123.Clear();
      index12.Clear();
      index13.Clear();
      index23.Clear();
      index1.Clear();
      index2.Clear();
      index3.Clear();
    }

    public void Delete(uint field1, uint field2, uint field3) {
      uint hashcode = Miscellanea.Hashcode(field1, field2, field3);
      for (uint idx = index123.Head(hashcode) ; idx != Tuple.Empty ; idx = index123.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1 & tuple.field2OrEmptyMarker == field2 & tuple.field3 == field3) {
          DeleteAt(idx, hashcode);
          return;
        }
      }
    }

    // public void Delete12(uint field1, uint field2) {
    //   uint hashcode = Miscellanea.Hashcode(field1, field2);
    //   for (uint idx = index12.Head(hashcode) ; idx != Tuple.Empty ; idx = index12.Next(idx)) {
    //     Tuple tuple = tuples[idx];
    //     if (tuple.field1OrNext == field1 & tuple.field2OrEmptyMarker == field2)
    //       DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
    //   }
    // }

    // public void Delete13(uint field1, uint field3) {
    //   uint hashcode = Miscellanea.Hashcode(field1, field3);
    //   if (index13.IsBlank())
    //     BuildIndex13();
    //   for (uint idx = index13.Head(hashcode) ; idx != Tuple.Empty ; idx = index13.Next(idx)) {
    //     Tuple tuple = tuples[idx];
    //     if (tuple.field1OrNext == field1 & tuple.field3 == field3)
    //       DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
    //  }
    //}

    // public void Delete23(uint field2, uint field3) {
    //   uint hashcode = Miscellanea.Hashcode(field2, field3);
    //   if (index23.IsBlank())
    //     BuildIndex23();
    //   for (uint idx = index23.Head(hashcode) ; idx != Tuple.Empty ; idx = index23.Next(idx)) {
    //     Tuple tuple = tuples[idx];
    //     if (tuple.field2OrEmptyMarker == field2 & tuple.field3 == field3)
    //       DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
    //   }
    // }

    // public void Delete1(uint field1) {
    //   uint hashcode = Miscellanea.Hashcode(field1);
    //   if (index1.IsBlank())
    //     BuildIndex1();
    //   for (uint idx = index1.Head(hashcode) ; idx != Tuple.Empty ; idx = index1.Next(idx)) {
    //     Tuple tuple = tuples[idx];
    //     if (tuple.field1OrNext == field1)
    //       DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
    //   }
    // }

    // public void Delete2(uint field2) {
    //   uint hashcode = Miscellanea.Hashcode(field2);
    //   if (index2.IsBlank())
    //     BuildIndex2();
    //   for (uint idx = index2.Head(hashcode) ; idx != Tuple.Empty ; idx = index2.Next(idx)) {
    //     Tuple tuple = tuples[idx];
    //     if (tuple.field2OrEmptyMarker == field2)
    //       DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
    //   }
    // }

    // public void Delete3(uint field3) {
    //   uint hashcode = Miscellanea.Hashcode(field3);
    //   if (index3.IsBlank())
    //     BuildIndex3();
    //   for (uint idx = index3.Head(hashcode) ; idx != Tuple.Empty ; idx = index3.Next(idx)) {
    //     Tuple tuple = tuples[idx];
    //     if (tuple.field3 == field3)
    //       DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
    //   }
    // }

    public bool Contains(long field1, long field2, long field3) {
      uint hashcode = Miscellanea.Hashcode((uint) field1, (uint) field2, (uint) field3);
      for (uint idx = index123.Head(hashcode) ; idx != Tuple.Empty ; idx = index123.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1 & tuple.field2OrEmptyMarker == field2 & tuple.field3 == field3)
          return true;
      }
      return false;
    }

    public bool Contains12(uint field1, uint field2) {
      uint hashcode = Miscellanea.Hashcode(field1, field2);
      for (uint idx = index12.Head(hashcode) ; idx != Tuple.Empty ; idx = index12.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1 & tuple.field2OrEmptyMarker == field2)
          return true;
      }
      return false;
    }

    public bool Contains13(uint field1, uint field3) {
      if (index13.IsBlank())
        BuildIndex13();
      uint hashcode = Miscellanea.Hashcode(field1, field3);
      for (uint idx = index13.Head(hashcode) ; idx != Tuple.Empty ; idx = index13.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1 & tuple.field3 == field3)
          return true;
      }
      return false;
    }

    public bool Contains23(uint field2, uint field3) {
      if (index23.IsBlank())
        BuildIndex23();
      uint hashcode = Miscellanea.Hashcode(field2, field3);
      for (uint idx = index23.Head(hashcode) ; idx != Tuple.Empty ; idx = index23.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field2OrEmptyMarker == field2 & tuple.field3 == field3)
          return true;
      }
      return false;
    }

    public bool Contains1(uint field1) {
      if (index1.IsBlank())
        BuildIndex1();
      uint hashcode = Miscellanea.Hashcode(field1);
      for (uint idx = index1.Head(hashcode) ; idx != Tuple.Empty ; idx = index1.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1)
          return true;
      }
      return false;
    }

    public bool Contains2(uint field2) {
      if (index2.IsBlank())
        BuildIndex2();
      uint hashcode = Miscellanea.Hashcode(field2);
      for (uint idx = index2.Head(hashcode) ; idx != Tuple.Empty ; idx = index2.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field2OrEmptyMarker == field2)
          return true;
      }
      return false;
    }

    public bool Contains3(uint field3) {
      if (index3.IsBlank())
        BuildIndex3();
      uint hashcode = Miscellanea.Hashcode(field3);
      for (uint idx = index3.Head(hashcode) ; idx != Tuple.Empty ; idx = index3.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field3 == field3)
          return true;
      }
      return false;
    }

    public Iter GetIter() {
      return new Iter(Tuple.Empty, Tuple.Empty, Tuple.Empty, 0, Iter.Type.F123, this);
    }

    public Iter GetIter12(long field1, long field2) {
      uint hashcode = Miscellanea.Hashcode((uint) field1, (uint) field2);
      return new Iter((uint) field1, (uint) field2, Tuple.Empty, index12.Head(hashcode), Iter.Type.F12, this);
    }

    public Iter GetIter13(long field1, long field3) {
      if (index13.IsBlank())
        BuildIndex13();
      uint hashcode = Miscellanea.Hashcode((uint) field1, (uint) field3);
      return new Iter((uint) field1, Tuple.Empty, (uint) field3, index13.Head(hashcode), Iter.Type.F13, this);
    }

    public Iter GetIter23(long field2, long field3) {
      if (index23.IsBlank())
        BuildIndex23();
      uint hashcode = Miscellanea.Hashcode((uint) field2, (uint) field3);
      return new Iter(Tuple.Empty, (uint) field2, (uint) field3, index23.Head(hashcode), Iter.Type.F23, this);
    }

    public Iter GetIter1(long field1) {
      if (index1.IsBlank())
        BuildIndex1();
      uint hashcode = Miscellanea.Hashcode((uint) field1);
      return new Iter((uint) field1, Tuple.Empty, Tuple.Empty, index1.Head(hashcode), Iter.Type.F1, this);
    }

    public Iter GetIter2(long field2) {
      if (index2.IsBlank())
        BuildIndex2();
      uint hashcode = Miscellanea.Hashcode((uint) field2);
      return new Iter(Tuple.Empty, (uint) field2, Tuple.Empty, index2.Head(hashcode), Iter.Type.F2, this);
    }

    public Iter GetIter3(long field3) {
      if (index3.IsBlank())
        BuildIndex3();
      uint hashcode = Miscellanea.Hashcode((uint) field3);
      return new Iter(Tuple.Empty, Tuple.Empty, (uint) field3, index3.Head(hashcode), Iter.Type.F3, this);
    }

    public Obj Copy(int idx1, int idx2, int idx3) {
      if (count == 0)
        return EmptyRelObj.Singleton();

      Obj[] objs1 = new Obj[count];
      Obj[] objs2 = new Obj[count];
      Obj[] objs3 = new Obj[count];

      int len = tuples.Length;
      int next = 0;
      for (uint i=0 ; i < len ; i++) {
        Tuple tuple = tuples[i];
        if (tuple.field2OrEmptyMarker != Tuple.Empty) {
          objs1[next] = store1.GetValue(tuple.field1OrNext);
          objs2[next] = store2.GetValue(tuple.field2OrEmptyMarker);
          objs3[next] = store3.GetValue(tuple.field3);
          next++;
        }
      }
      Miscellanea.Assert(next == count);

      Obj[][] cols = new Obj[3][];
      cols[idx1] = objs1;
      cols[idx2] = objs2;
      cols[idx3] = objs3;

      return Builder.CreateTernRel(cols[0], cols[1], cols[2], count);
    }

    ////////////////////////////////////////////////////////////////////////////

    void DeleteAt(uint index, uint hashcode) {
      Tuple tuple = tuples[index];
      Miscellanea.Assert(tuple.field2OrEmptyMarker != Tuple.Empty);

      // Removing the tuple
      tuples[index].field1OrNext = firstFree;
      tuples[index].field2OrEmptyMarker = Tuple.Empty;
      Miscellanea.Assert(tuples[index].field1OrNext == firstFree);
      Miscellanea.Assert(tuples[index].field2OrEmptyMarker == Tuple.Empty);
      firstFree = index;
      count--;

      // Updating the indexes
      index123.Delete(index, hashcode);
      index12.Delete(index, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker));
      if (!index13.IsBlank())
        index13.Delete(index, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field3));
      if (!index23.IsBlank())
        index23.Delete(index, Miscellanea.Hashcode(tuple.field2OrEmptyMarker, tuple.field3));
      if (!index1.IsBlank())
        index1.Delete(index, Miscellanea.Hashcode(tuple.field1OrNext));
      if (!index2.IsBlank())
        index2.Delete(index, Miscellanea.Hashcode(tuple.field2OrEmptyMarker));
      if (!index3.IsBlank())
        index3.Delete(index, Miscellanea.Hashcode(tuple.field3));

      // Updating the reference count in the value stores
      store1.Release(tuple.field1OrNext);
      store2.Release(tuple.field2OrEmptyMarker);
      store3.Release(tuple.field3);
    }

    void BuildIndex13() {
      index13.Init(IndexInitSize());
      uint len = (uint) tuples.Length;
      for (uint i=0 ; i < len ; i++) {
        Tuple tuple = tuples[i];
        if (tuple.field2OrEmptyMarker != Tuple.Empty)
          index13.Insert(i, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field3));
      }
    }

    void BuildIndex23() {
      index23.Init(IndexInitSize());
      uint len = (uint) tuples.Length;
      for (uint i=0 ; i < len ; i++) {
        Tuple tuple = tuples[i];
        if (tuple.field2OrEmptyMarker != Tuple.Empty) {
          uint hashcode = Miscellanea.Hashcode(tuple.field2OrEmptyMarker, tuple.field3);
          index23.Insert(i, hashcode);
        }
      }
    }

    void BuildIndex1() {
      index1.Init(IndexInitSize());
      uint len = (uint) tuples.Length;
      for (uint i=0 ; i < len ; i++) {
        Tuple tuple = tuples[i];
        if (tuple.field2OrEmptyMarker != Tuple.Empty)
          index1.Insert(i, Miscellanea.Hashcode(tuple.field1OrNext));
      }
    }

    void BuildIndex2() {
      index2.Init(IndexInitSize());
      uint len = (uint) tuples.Length;
      for (uint i=0 ; i < len ; i++) {
        Tuple tuple = tuples[i];
        if (tuple.field2OrEmptyMarker != Tuple.Empty)
          index2.Insert(i, Miscellanea.Hashcode(tuple.field2OrEmptyMarker));
      }
    }

    void BuildIndex3() {
      index3.Init(IndexInitSize());
      uint len = (uint) tuples.Length;
      for (uint i=0 ; i < len ; i++) {
        Tuple tuple = tuples[i];
        if (tuple.field2OrEmptyMarker != Tuple.Empty)
          index3.Insert(i, Miscellanea.Hashcode(tuple.field3));
      }
    }

    uint IndexInitSize() {
      uint size = MinSize;
      while (size < tuples.Length)
        size *= 2;
      return size;
    }
  }


  class TernaryTableUpdater {
    List<TernaryTable.Tuple> deleteList = new List<TernaryTable.Tuple>();
    List<TernaryTable.Tuple> insertList = new List<TernaryTable.Tuple>();

    TernaryTable table;
    ValueStoreUpdater store1, store2, store3;

    public TernaryTableUpdater(TernaryTable table, ValueStoreUpdater store1, ValueStoreUpdater store2, ValueStoreUpdater store3) {
      this.table = table;
      this.store1 = store1;
      this.store2 = store2;
      this.store3 = store3;
    }

    public void Clear() {
      deleteList.Clear();
      TernaryTable.Iter it = table.GetIter();
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public void Set(Obj value, int idx1, int idx2, int idx3) {
      Miscellanea.Assert(deleteList.Count == 0);
      Miscellanea.Assert(insertList.Count == 0);

      Clear();
      TernRelIter it = value.GetTernRelIter();
      while (!it.Done()) {
        Obj val1 = it.Get1();
        Obj val2 = it.Get2();
        Obj val3 = it.Get3();
        int surr1 = store1.LookupValueEx(val1);
        if (surr1 == -1)
          surr1 = store1.Insert(val1);
        int surr2 = store2.LookupValueEx(val2);
        if (surr2 == -1)
          surr2 = store2.Insert(val2);
        int surr3 = store3.LookupValueEx(val3);
        if (surr3 == -1)
          surr3 = store3.Insert(val3);
        insertList.Add(new TernaryTable.Tuple((uint) surr1, (uint) surr2, (uint) surr3));
        it.Next();
      }
    }

    public void Insert(long value1, long value2, long value3) {
      insertList.Add(new TernaryTable.Tuple((uint) value1, (uint) value2, (uint) value3));
    }

    public void Delete(long value1, long value2, long value3) {
      if (table.Contains((uint) value1, (uint) value2, (uint) value3))
        deleteList.Add(new TernaryTable.Tuple((uint) value1, (uint) value2, (uint) value3));
    }

    public void Delete12(long value1, long value2) {
      TernaryTable.Iter it = table.GetIter12((uint) value1, (uint) value2);
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public void Delete13(long value1, long value3) {
      TernaryTable.Iter it = table.GetIter13((uint) value1, (uint) value3);
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public void Delete23(long value2, long value3) {
      TernaryTable.Iter it = table.GetIter23((uint) value2, (uint) value3);
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public void Delete1(long value1) {
      TernaryTable.Iter it = table.GetIter1((uint) value1);
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public void Delete2(long value2) {
      TernaryTable.Iter it = table.GetIter2((uint) value2);
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public void Delete3(long value3) {
      TernaryTable.Iter it = table.GetIter3((uint) value3);
      while (!it.Done()) {
        deleteList.Add(it.Get());
        it.Next();
      }
    }

    public bool CheckUpdates_12() {
      deleteList.Sort(compare123);
      insertList.Sort(compare123);

      int count = insertList.Count;
      if (count == 0)
        return true;

      TernaryTable.Tuple prev = insertList[0];
      if (!Contains12(deleteList, prev.field1OrNext, prev.field2OrEmptyMarker))
        if (table.Contains12(prev.field1OrNext, prev.field2OrEmptyMarker))
          return false;

      for (int i=1 ; i < count ; i++) {
        TernaryTable.Tuple curr = insertList[i];
        if ( curr.field1OrNext == prev.field1OrNext &
             curr.field2OrEmptyMarker == prev.field2OrEmptyMarker &
             curr.field3 != prev.field3
           )
          return false;
        if (!Contains12(deleteList, curr.field1OrNext, curr.field2OrEmptyMarker))
          if (table.Contains12(curr.field1OrNext, curr.field2OrEmptyMarker))
            return false;
        prev = curr;
      }

      return true;
    }

    public bool CheckUpdates_12_3() {
      if (!CheckUpdates_12())
        return false;

      deleteList.Sort(compare312);
      insertList.Sort(compare312);

      int count = insertList.Count;
      if (count == 0)
        return true;

      TernaryTable.Tuple prev = insertList[0];
      if (!Contains3(deleteList, prev.field3))
        if (table.Contains3(prev.field3))
          return false;

      for (int i=1 ; i < count ; i++) {
        TernaryTable.Tuple curr = insertList[i];
        if ( curr.field3 == prev.field3 &
             (curr.field1OrNext != prev.field1OrNext | curr.field2OrEmptyMarker != prev.field2OrEmptyMarker)
           )
          return false;
        if (!Contains3(deleteList, prev.field3))
          if (table.Contains3(prev.field3))
        prev = curr;
      }

      return true;
    }

    public bool CheckUpdates_12_23() {
      if (!CheckUpdates_12())
        return false;

      deleteList.Sort(compare231);
      insertList.Sort(compare231);

      int count = insertList.Count;
      if (count == 0)
        return true;

      TernaryTable.Tuple prev = insertList[0];
      if (!Contains23(deleteList, prev.field2OrEmptyMarker, prev.field3))
        if (table.Contains23(prev.field2OrEmptyMarker, prev.field3))
          return false;

      for (int i=1 ; i < count ; i++) {
        TernaryTable.Tuple curr = insertList[i];
        if ( curr.field2OrEmptyMarker == prev.field2OrEmptyMarker &
             curr.field3 == prev.field3 &
             curr.field1OrNext != prev.field1OrNext
           )
          return false;
        if (!Contains23(deleteList, curr.field2OrEmptyMarker, curr.field3))
          if (table.Contains23(curr.field2OrEmptyMarker, curr.field3))
            return false;
        prev = curr;
      }

      return true;
    }

    public bool CheckUpdates_12_23_31() {
      if (!CheckUpdates_12_23())
        return false;

      deleteList.Sort(compare312);
      insertList.Sort(compare312);

      int count = insertList.Count;
      if (count == 0)
        return true;

      TernaryTable.Tuple prev = insertList[0];
      if (!Contains31(deleteList, prev.field3, prev.field1OrNext))
        if (table.Contains13(prev.field1OrNext, prev.field3))
          return false;

      for (int i=1 ; i < count ; i++) {
        TernaryTable.Tuple curr = insertList[i];
        if ( curr.field3 == prev.field3 &
             curr.field1OrNext == prev.field1OrNext &
             curr.field2OrEmptyMarker != prev.field2OrEmptyMarker
           )
          return false;
        if (!Contains31(deleteList, curr.field3, curr.field1OrNext))
          if (table.Contains13(curr.field1OrNext, curr.field3))
            return false;
        prev = curr;
      }

      return true;
    }

    public void Apply() {
      for (int i=0 ; i < deleteList.Count ; i++) {
        var tuple = deleteList[i];
        if (table.Contains(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3))
          table.Delete(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3);
        else
          deleteList[i] = new TernaryTable.Tuple(0xFFFFFFFF, 0xFFFFFFFF, 0xFFFFFFFF);
      }

      var it = insertList.GetEnumerator();
      while (it.MoveNext()) {
        var curr = it.Current;
        if (!table.Contains(curr.field1OrNext, curr.field2OrEmptyMarker, curr.field3)) {
          table.Insert(curr.field1OrNext, curr.field2OrEmptyMarker, curr.field3);
          table.store1.AddRef(curr.field1OrNext);
          table.store2.AddRef(curr.field2OrEmptyMarker);
          table.store3.AddRef(curr.field3);
        }
      }
    }

    public void Finish() {
      var it = deleteList.GetEnumerator();
      while (it.MoveNext()) {
        var tuple = it.Current;
        if (tuple.field1OrNext != 0xFFFFFFFF) {
          table.store1.Release(tuple.field1OrNext);
          table.store2.Release(tuple.field2OrEmptyMarker);
          table.store3.Release(tuple.field3);
        }
      }
    }

    public void Reset() {
      deleteList.Clear();
      insertList.Clear();
    }


    static bool Contains12(List<TernaryTable.Tuple> tuples, uint field1, uint field2) {
      int low = 0;
      int high = tuples.Count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        TernaryTable.Tuple tuple = tuples[mid];
        if (tuple.field1OrNext > field1)
          high = mid - 1;
        else if (tuple.field1OrNext < field1)
          low = mid + 1;
        else if (tuple.field2OrEmptyMarker > field2)
          high = mid - 1;
        else if (tuple.field2OrEmptyMarker < field2)
          low = mid + 1;
        else
          return true;
      }

      return false;
    }

    static bool Contains23(List<TernaryTable.Tuple> tuples, uint field2, uint field3) {
      int low = 0;
      int high = tuples.Count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        TernaryTable.Tuple tuple = tuples[mid];
        if (tuple.field2OrEmptyMarker > field2)
          high = mid - 1;
        else if (tuple.field2OrEmptyMarker < field2)
          low = mid + 1;
        else if (tuple.field3 > field3)
          high = mid - 1;
        else if (tuple.field3 < field3)
          low = mid + 1;
        else
          return true;
      }

      return false;
    }

    static bool Contains31(List<TernaryTable.Tuple> tuples, uint field3, uint field1) {
      int low = 0;
      int high = tuples.Count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        TernaryTable.Tuple tuple = tuples[mid];
        if (tuple.field3 > field3)
          high = mid - 1;
        else if (tuple.field3 < field3)
          low = mid + 1;
        else if (tuple.field1OrNext > field1)
          high = mid - 1;
        else if (tuple.field1OrNext < field1)
          low = mid + 1;
        else
          return true;
      }

      return false;
    }

//    static bool ContainsField1(List<TernaryTable.Tuple> tuples, uint field1) {
//      int low = 0;
//      int high = tuples.Count - 1;
//
//      while (low <= high) {
//        int mid = (int) (((long) low + (long) high) / 2);
//        uint midField1 = tuples[mid].field1OrNext;
//        if (midField1 > field1)
//          high = mid - 1;
//        else if (midField1 < field1)
//          low = mid + 1;
//        else
//          return true;
//      }
//
//      return false;
//    }

//    static bool ContainsField2(List<TernaryTable.Tuple> tuples, uint field2) {
//      int low = 0;
//      int high = tuples.Count - 1;
//
//      while (low <= high) {
//        int mid = (int) (((long) low + (long) high) / 2);
//        uint midField2 = tuples[mid].field2OrEmptyMarker;
//        if (midField2 > field2)
//          high = mid - 1;
//        else if (midField2 < field2)
//          low = mid + 1;
//        else
//          return true;
//      }
//
//      return false;
//    }

    static bool Contains3(List<TernaryTable.Tuple> tuples, uint field3) {
      int low = 0;
      int high = tuples.Count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        uint midField3 = tuples[mid].field3;
        if (midField3 > field3)
          high = mid - 1;
        else if (midField3 < field3)
          low = mid + 1;
        else
          return true;
      }

      return false;
    }

    static Comparison<TernaryTable.Tuple> compare123 = delegate(TernaryTable.Tuple t1, TernaryTable.Tuple t2) {
      if (t1.field1OrNext != t2.field1OrNext)
        return (int) (t1.field1OrNext - t2.field1OrNext);
      else if (t1.field2OrEmptyMarker != t2.field2OrEmptyMarker)
        return (int) (t1.field2OrEmptyMarker - t2.field2OrEmptyMarker);
      else
        return (int) (t1.field3 - t2.field3);
    };

    static Comparison<TernaryTable.Tuple> compare231 = delegate(TernaryTable.Tuple t1, TernaryTable.Tuple t2) {
      if (t1.field2OrEmptyMarker != t2.field2OrEmptyMarker)
        return (int) (t1.field2OrEmptyMarker - t2.field2OrEmptyMarker);
      else if (t1.field3 != t2.field3)
        return (int) (t1.field3 - t2.field3);
      else
        return (int) (t1.field1OrNext - t2.field1OrNext);
    };

    static Comparison<TernaryTable.Tuple> compare312 = delegate(TernaryTable.Tuple t1, TernaryTable.Tuple t2) {
      if (t1.field3 != t2.field3)
        return (int) (t1.field3 - t2.field3);
      if (t1.field1OrNext != t2.field1OrNext)
        return (int) (t1.field1OrNext - t2.field1OrNext);
      else
        return (int) (t1.field2OrEmptyMarker - t2.field2OrEmptyMarker);
    };
  }
}

//    public void Delete(long value1, long value2) {
//      if (table.Contains((uint) value1, (uint) value2))
//        deleteList.Add(new Tuple((uint) value1, (uint) value2));
//    }
//
//    public void DeleteByCol1(long value) {
//      uint[] assocs = table.LookupByCol1((uint) value);
//      for (int i=0 ; i < assocs.Length ; i++)
//        deleteList.Add(new Tuple((uint) value, assocs[i]));
//    }
//
//    public void DeleteByCol2(long value) {
//      uint[] assocs = table.LookupByCol2((uint) value);
//      for (int i=0 ; i < assocs.Length ; i++)
//        deleteList.Add(new Tuple(assocs[i], (uint) value));
//    }
//
//    public void Insert(long value1, long value2) {
//      insertList.Add(new Tuple((uint) value1, (uint) value2));
//    }
//
//    public bool CheckUpdates_1() {
//      deleteList.Sort();
//      insertList.Sort();
//
//      int count = insertList.Count;
//      if (count == 0)
//        return true;
//
//      Tuple prev = insertList[0];
//      if (!ContainsField1(deleteList, prev.field1))
//        if (table.ContainsField1(prev.field1))
//          return false;
//
//      for (int i=1 ; i < count ; i++) {
//        Tuple curr = insertList[i];
//        if (curr.field1 == prev.field1 & curr.field2 != prev.field2)
//          return false;
//        if (!ContainsField1(deleteList, curr.field1))
//          if (table.ContainsField1(curr.field1))
//            return false;
//        prev = curr;
//      }
//
//      return true;
//    }
//
//    public bool CheckUpdates_1_2() {
//      if (!CheckUpdates_1())
//        return false;
//
//      Comparison<Tuple> cmp = delegate(Tuple t1, Tuple t2) {
//        return (int) (t1.field2 != t2.field2 ? t2.field2 - t1.field2 : t2.field1 - t1.field1);
//      };
//
//      deleteList.Sort(cmp);
//      insertList.Sort(cmp);
//
//      int count = insertList.Count;
//      if (count == 0)
//        return true;
//
//      Tuple prev = insertList[0];
//      if (!ContainsField2(deleteList, prev.field2))
//        if (table.ContainsField2(prev.field2))
//          return false;
//
//      for (int i=1 ; i < count ; i++) {
//        Tuple curr = insertList[i];
//        if (curr.field2 == prev.field2 & curr.field1 != prev.field1)
//          return false;
//        if (!ContainsField2(deleteList, curr.field2))
//          if (table.ContainsField2(curr.field2))
//            return false;
//        prev = curr;
//      }
//
//      return true;
//    }
//
//    public void Apply() {
//      for (int i=0 ; i < deleteList.Count ; i++) {
//        Tuple tuple = deleteList[i];
//        if (table.Contains(tuple.field1, tuple.field2))
//          table.Delete(tuple.field1, tuple.field2);
//        else
//          deleteList[i] = new Tuple(0xFFFFFFFF, 0xFFFFFFFF);
//      }
//
//      var it = insertList.GetEnumerator();
//      while (it.MoveNext()) {
//        var curr = it.Current;
//        if (!table.Contains(curr.field1, curr.field2)) {
//          table.Insert(curr.field1, curr.field2);
//          table.store1.AddRef(curr.field1);
//          table.store2.AddRef(curr.field2);
//        }
//      }
//    }
//
//    public void Finish() {
//      var it = deleteList.GetEnumerator();
//      while (it.MoveNext()) {
//        var tuple = it.Current;
//        if (tuple.field1 != 0xFFFFFFFF) {
//          table.store1.Release(tuple.field1);
//          table.store2.Release(tuple.field2);
//        }
//      }
//    }
//
//    public void Reset() {
//      deleteList.Clear();
//      insertList.Clear();
//    }
//
//    static bool ContainsField1(List<Tuple> tuples, uint field1) {
//      int low = 0;
//      int high = tuples.Count - 1;
//
//      while (low <= high) {
//        int mid = (int) (((long) low + (long) high) / 2);
//        uint midField1 = tuples[mid].field1;
//        if (midField1 > field1)
//          high = mid - 1;
//        else if (midField1 < field1)
//          low = mid + 1;
//        else
//          return true;
//      }
//
//      return false;
//    }
//
//    static bool ContainsField2(List<Tuple> tuples, uint field2) {
//      int low = 0;
//      int high = tuples.Count - 1;
//
//      while (low <= high) {
//        int mid = (int) (((long) low + (long) high) / 2);
//        uint midField2 = tuples[mid].field2;
//        if (midField2 > field2)
//          high = mid - 1;
//        else if (midField2 < field2)
//          low = mid + 1;
//        else
//          return true;
//      }
//
//      return false;
//    }
//  }


// bool Contains(uint field1, uint field2, uint field3)
// bool Contains12(uint field1, uint field2)
// bool Contains13(uint field1, uint field3)
// bool Contains23(uint field2, uint field3)
// bool Contains1(uint field1)
// bool Contains2(uint field2)
// bool Contains3(uint field3)
// void Insert(uint field1, uint field2, uint field3)
// void Clear()
// void Delete(uint field1, uint field2, uint field3)
// void Delete12(uint field1, uint field2)
// void Delete13(uint field1, uint field3)
// void Delete23(uint field2, uint field3)
// void Delete1(uint field1)
// void Delete2(uint field2)
// void Delete3(uint field3)
// Obj Copy(int idx1, int idx2, int idx3) {

// bool Contains(uint surr1, uint surr2)
// bool ContainsField1(uint surr1)
// bool ContainsField2(uint surr2)
// uint[] LookupByCol1(uint surr)
// uint[] LookupByCol2(uint surr)
// void Insert(uint surr1, uint surr2)
// void Clear()
// void Delete(uint surr1, uint surr2)
// Obj Copy(bool flipped)

// void ternary_table_init(TERNARY_TABLE *table);
// void ternary_table_cleanup(TERNARY_TABLE *table);
//
// void ternary_table_updates_init(TERNARY_TABLE_UPDATES *updates);
// void ternary_table_updates_cleanup(TERNARY_TABLE_UPDATES *updates);
//
// bool ternary_table_contains(TERNARY_TABLE *table, uint32 left_val, uint32 middle_val, uint32 right_val);
//
// void ternary_table_delete(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, uint32 left_val, uint32 middle_val, uint32 right_val);
// void ternary_table_delete_by_cols_01(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, uint32 value0, uint32 value1);
// void ternary_table_delete_by_cols_02(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, uint32 value0, uint32 value2);
// void ternary_table_delete_by_cols_12(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, uint32 value1, uint32 value2);
// void ternary_table_delete_by_col_0(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, uint32 value);
// void ternary_table_delete_by_col_1(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, uint32 value);
// void ternary_table_delete_by_col_2(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, uint32 value);
// void ternary_table_clear(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates);
//
// void ternary_table_insert(TERNARY_TABLE_UPDATES *updates, uint32 left_val, uint32 middle_val, uint32 right_val);
//
// bool ternary_table_updates_check_01(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates);
// bool ternary_table_updates_check_01_2(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates);
// bool ternary_table_updates_check_01_12(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates);
// bool ternary_table_updates_check_01_12_20(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates);
//
// void ternary_table_updates_apply(TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates, VALUE_STORE *vs0, VALUE_STORE *vs1, VALUE_STORE *vs2);
// void ternary_table_updates_finish(TERNARY_TABLE_UPDATES *updates, VALUE_STORE *vs0, VALUE_STORE *vs1, VALUE_STORE *vs2);
//
// void ternary_table_get_iter_by_cols_01(TERNARY_TABLE *table, TERNARY_TABLE_ITER *iter, uint32 value0, uint32 value1);
// void ternary_table_get_iter_by_cols_02(TERNARY_TABLE *table, TERNARY_TABLE_ITER *iter, uint32 value0, uint32 value2);
// void ternary_table_get_iter_by_cols_12(TERNARY_TABLE *table, TERNARY_TABLE_ITER *iter, uint32 value1, uint32 value2);
// void ternary_table_get_iter_by_col_0(TERNARY_TABLE *table, TERNARY_TABLE_ITER *iter, uint32 value);
// void ternary_table_get_iter_by_col_1(TERNARY_TABLE *table, TERNARY_TABLE_ITER *iter, uint32 value);
// void ternary_table_get_iter_by_col_2(TERNARY_TABLE *table, TERNARY_TABLE_ITER *iter, uint32 value);
// void ternary_table_get_iter(TERNARY_TABLE *table, TERNARY_TABLE_ITER *iter);
//
// bool ternary_table_iter_is_out_of_range(TERNARY_TABLE_ITER *iter);
//
// uint32 ternary_table_iter_get_left_field(TERNARY_TABLE_ITER *iter);
// uint32 ternary_table_iter_get_middle_field(TERNARY_TABLE_ITER *iter);
// uint32 ternary_table_iter_get_right_field(TERNARY_TABLE_ITER *iter);
//
// void ternary_table_iter_next(TERNARY_TABLE_ITER *iter);
//
// OBJ copy_ternary_table(TERNARY_TABLE *table, VALUE_STORE *vs1, VALUE_STORE *vs2, VALUE_STORE *vs3, int idx1, int idx2, int idx3);
//
// void set_ternary_table(
//   TERNARY_TABLE *table, TERNARY_TABLE_UPDATES *updates,
//   VALUE_STORE *vs1, VALUE_STORE *vs2, VALUE_STORE *vs3,
//   VALUE_STORE_UPDATES *vsu1, VALUE_STORE_UPDATES *vsu2, VALUE_STORE_UPDATES *vsu3,
//   OBJ rel, int idx1, int idx2, int idx3
// );
