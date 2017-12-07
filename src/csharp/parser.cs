using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public static class Parser {
    public static Obj ParseSymb(Obj obj) {
      string str = obj.GetString();
      int id = SymbTable.StrToIdx(str);
      return new SymbObj(id);
    }

    public static Obj Parse(Obj text) {
      throw new NotImplementedException();
    }
  }
}
