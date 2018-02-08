using System;


namespace CellLang {
  static class WrappingUtils {
    public static bool TableContains(UnaryTable table, Obj elem) {
      int surr = table.store.LookupValue(elem);
      if (surr == -1)
        return false;
      return table.Contains((uint) surr);
    }

    public static bool TableContains(BinaryTable table, Obj field1, Obj field2) {
      int surr1 = table.store1.LookupValue(field1);
      if (surr1 == -1)
        return false;
      int surr2 = table.store2.LookupValue(field2);
      if (surr2 == -1)
        return false;
      return table.Contains((uint) surr1, (uint) surr2);
    }

    public static bool TableContains(TernaryTable table, Obj field1, Obj field2, Obj field3) {
      int surr1 = table.store1.LookupValue(field1);
      if (surr1 == -1)
        return false;
      int surr2 = table.store2.LookupValue(field2);
      if (surr2 == -1)
        return false;
      int surr3 = table.store3.LookupValue(field3);
      if (surr3 == -1)
        return false;
      return table.Contains((uint) surr1, (uint) surr2, (uint) surr3);
    }
  }
}