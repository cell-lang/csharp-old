using System;
using System.Collections.Generic;


namespace CellLang {
  class TernaryTable {
    struct Tuple {
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

    const int MinSize = 256;

    Tuple[] tuples = new Tuple[MinSize];
    uint count = 0;
    uint firstFree = 0;

    Index index123, index12, index13, index23, index1, index2, index3;

    ValueStore store1, store2, store3;

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

    public void Delete12(uint field1, uint field2) {
      uint hashcode = Miscellanea.Hashcode(field1, field2);
      for (uint idx = index12.Head(hashcode) ; idx != Tuple.Empty ; idx = index12.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1 & tuple.field2OrEmptyMarker == field2)
          DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
      }
    }

    public void Delete13(uint field1, uint field3) {
      uint hashcode = Miscellanea.Hashcode(field1, field3);
      if (index13.IsBlank())
        BuildIndex13();
      for (uint idx = index13.Head(hashcode) ; idx != Tuple.Empty ; idx = index13.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1 & tuple.field3 == field3)
          DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
      }
    }

    public void Delete23(uint field2, uint field3) {
      uint hashcode = Miscellanea.Hashcode(field2, field3);
      if (index23.IsBlank())
        BuildIndex23();
      for (uint idx = index23.Head(hashcode) ; idx != Tuple.Empty ; idx = index23.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field2OrEmptyMarker == field2 & tuple.field3 == field3)
          DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
      }
    }

    public void Delete1(uint field1) {
      uint hashcode = Miscellanea.Hashcode(field1);
      if (index1.IsBlank())
        BuildIndex1();
      for (uint idx = index1.Head(hashcode) ; idx != Tuple.Empty ; idx = index1.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field1OrNext == field1)
          DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
      }
    }

    public void Delete2(uint field2) {
      uint hashcode = Miscellanea.Hashcode(field2);
      if (index2.IsBlank())
        BuildIndex2();
      for (uint idx = index2.Head(hashcode) ; idx != Tuple.Empty ; idx = index2.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field2OrEmptyMarker == field2)
          DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
      }
    }

    public void Delete3(uint field3) {
      uint hashcode = Miscellanea.Hashcode(field3);
      if (index3.IsBlank())
        BuildIndex3();
      for (uint idx = index3.Head(hashcode) ; idx != Tuple.Empty ; idx = index3.Next(idx)) {
        Tuple tuple = tuples[idx];
        if (tuple.field3 == field3)
          DeleteAt(idx, Miscellanea.Hashcode(tuple.field1OrNext, tuple.field2OrEmptyMarker, tuple.field3));
      }
    }

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
}