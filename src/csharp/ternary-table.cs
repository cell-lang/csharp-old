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
    }


    public struct Iter {
      public enum Type {F123, F12, F13, F23, F1, F2, F3};

      uint index;
      Type type;

      TernaryTable table;

      public Iter(uint index, Type type, TernaryTable table) {
        this.index = index;
        this.type = type;
        this.table = table;
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
            index = table.index12.Next(index);
            break;

          case Type.F13:
            index = table.index13.Next(index);
            break;

          case Type.F23:
            index = table.index23.Next(index);
            break;

          case Type.F1:
            index = table.index1.Next(index);
            break;

          case Type.F2:
            index = table.index2.Next(index);
            break;

          case Type.F3:
            index = table.index3.Next(index);
            break;
        }
      }
    }


    const int MinSize = 256;

    Tuple[] tuples = new Tuple[MinSize];
    uint count = 0;
    uint firstFree = 0;

    Index index123, index12, index13, index23, index1, index2, index3;

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
      index12.Insert(index, hashcode);
      if (!index13.IsBlank())
        index13.Insert(index, hashcode);
      if (!index23.IsBlank())
        index23.Insert(index, hashcode);
      if (!index1.IsBlank())
        index1.Insert(index, hashcode);
      if (!index2.IsBlank())
        index2.Insert(index, hashcode);
      if (!index3.IsBlank())
        index3.Insert(index, hashcode);

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

    public bool Contains(uint field1, uint field2, uint field3) {
      uint hashcode = Miscellanea.Hashcode(field1, field2, field3);
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
      return new Iter(0, Iter.Type.F123, this);
    }

    public Iter GetIter12(uint field1, uint field2) {
      uint hashcode = Miscellanea.Hashcode(field1, field2);
      return new Iter(index12.Head(hashcode), Iter.Type.F12, this);
    }

    public Iter GetIter13(uint field1, uint field3) {
      if (index13.IsBlank())
        BuildIndex13();
      uint hashcode = Miscellanea.Hashcode(field1, field3);
      return new Iter(index13.Head(hashcode), Iter.Type.F13, this);
    }

    public Iter GetIter23(uint field2, uint field3) {
      if (index23.IsBlank())
        BuildIndex23();
      uint hashcode = Miscellanea.Hashcode(field2, field3);
      return new Iter(index23.Head(hashcode), Iter.Type.F23, this);
    }

    public Iter GetIter1(uint field1) {
      if (index1.IsBlank())
        BuildIndex1();
      uint hashcode = Miscellanea.Hashcode(field1);
      return new Iter(index1.Head(hashcode), Iter.Type.F1, this);
    }

    public Iter GetIter2(uint field2) {
      if (index2.IsBlank())
        BuildIndex2();
      uint hashcode = Miscellanea.Hashcode(field2);
      return new Iter(index2.Head(hashcode), Iter.Type.F2, this);
    }

    public Iter GetIter3(uint field3) {
      if (index3.IsBlank())
        BuildIndex3();
      uint hashcode = Miscellanea.Hashcode(field3);
      return new Iter(index3.Head(hashcode), Iter.Type.F3, this);
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

      return Builder.CreateTernRel(
        idx1 == 0 ? objs1 : (idx1 == 1 ? objs2 : objs3),
        idx2 == 0 ? objs1 : (idx2 == 1 ? objs2 : objs3),
        idx3 == 0 ? objs1 : (idx3 == 1 ? objs2 : objs3),
        count
      );
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
      index12.Delete(index, hashcode);
      if (!index13.IsBlank())
        index13.Delete(index, hashcode);
      if (!index23.IsBlank())
        index23.Delete(index, hashcode);
      if (!index1.IsBlank())
        index1.Delete(index, hashcode);
      if (!index2.IsBlank())
        index2.Delete(index, hashcode);
      if (!index3.IsBlank())
        index3.Delete(index, hashcode);

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
        if (tuple.field2OrEmptyMarker != Tuple.Empty)
          index23.Insert(i, Miscellanea.Hashcode(tuple.field2OrEmptyMarker, tuple.field3));
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
      throw new NotImplementedException();

//      Comparison<TernaryTable.Tuple> cmp = delegate(TernaryTable.Tuple t1, TernaryTable.Tuple t2) {
//        if (t1.field1 != t2.field1)
//          return t2.field1 - t1.field1;
//        else if (t1.field2 != t2.field2)
//          return t2.field2 - t1.field2;
//        else
//          return t2.field3 - t1.field3;
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
    }

    public bool CheckUpdates_12_3() {
      throw new NotImplementedException();
    }

    public bool CheckUpdates_12_23() {
      throw new NotImplementedException();
    }

    public bool CheckUpdates_12_23_31() {
      throw new NotImplementedException();
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


    static bool ContainsField1(List<TernaryTable.Tuple> tuples, uint field1) {
      int low = 0;
      int high = tuples.Count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        uint midField1 = tuples[mid].field1OrNext;
        if (midField1 > field1)
          high = mid - 1;
        else if (midField1 < field1)
          low = mid + 1;
        else
          return true;
      }

      return false;
    }

    static bool ContainsField2(List<TernaryTable.Tuple> tuples, uint field2) {
      int low = 0;
      int high = tuples.Count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        uint midField2 = tuples[mid].field2OrEmptyMarker;
        if (midField2 > field2)
          high = mid - 1;
        else if (midField2 < field2)
          low = mid + 1;
        else
          return true;
      }

      return false;
    }

    static bool ContainsField3(List<TernaryTable.Tuple> tuples, uint field3) {
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