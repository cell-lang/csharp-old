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
}

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
