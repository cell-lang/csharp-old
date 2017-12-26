using System;
using System.Collections.Generic;


namespace CellLang {
  class BinaryTable {
    const uint EmptySlot      = 0xFFFFFFFF;
    const uint MultiValueSlot = 0xFFFFFFFE;
    //const uint MaxSurrId = 0x1FFFFFFF;

    uint[] col1;
    uint[] col2;
    Dictionary<uint, HashSet<uint>> multimap1;
    Dictionary<uint, HashSet<uint>> multimap2;
    int count = 0;

    ValueStore store1;
    ValueStore store2;

    public BinaryTable(ValueStore store1, ValueStore store2) {
      this.store1 = store1;
      this.store2 = store2;
    }


    public Obj Copy(bool flipped) {
      if (count == 0)
        return EmptyRelObj.Singleton();

      Obj[] objs1 = new Obj[count];
      Obj[] objs2 = new Obj[count];

      int next = 0;
      for (uint i=0 ; i < col1.Length ; i++) {
        uint code = col1[i];
        if (code != EmptySlot) {
          Obj val1 = store1.GetValue(i);
          if (code != MultiValueSlot) {
            objs1[next] = val1;
            objs2[next++] = store2.GetValue(code);
          }
          else {
            foreach (uint surr2 in multimap1[i]) {
              objs1[next] = val1;
              objs2[next++] = store2.GetValue(surr2);
            }
          }
        }
      }

      Miscellanea.Assert(next == count);
      return Builder.CreateBinRel(objs1, objs2, count); //## THIS COULD BE MADE MORE EFFICIENT
    }
  }


  class BinaryTableUpdater {
    BinaryTable table;

    public BinaryTableUpdater(BinaryTable table) {
      this.table = table;
    }

    public void Clear() {

    }

    public void Set(Obj value, bool flipped) {

    }

    public void Delete(long value1, long value2) {
      throw new NotImplementedException();
    }

    public void DeleteByCol1(long value) {
      throw new NotImplementedException();
    }

    public void DeleteByCol2(long value) {
      throw new NotImplementedException();
    }

    public void Insert(long value1, long value2) {
      throw new NotImplementedException();
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

    }

    public void Finish() {

    }
  }
}
