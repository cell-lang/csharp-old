using System;
using System.Collections.Generic;


namespace CellLang {
  public static class SymbTable {
    static string[] defaultSymbols = {
      "false",
      "true",
      "void",
      "string",
      "nothing",
      "just",
      "success",
      "failure"
    };

    public static int FalseSymbId   = 0;
    public static int TrueSymbId    = 1;
    public static int VoidSymbId    = 2;
    public static int StringSymbId  = 3;
    public static int NothingSymbId = 4;
    public static int JustSymbId    = 5;
    public static int SuccessSymbId = 6;
    public static int FailureSymbId = 7;

    static List<String> symbTable = new List<String>();
    static Dictionary<String, int> symbMap = new Dictionary<String, int>();

    static SymbTable() {
      int len = defaultSymbols.Length;
      for (int i=0 ; i < len ; i++) {
        string str = defaultSymbols[i];
        symbTable.Add(str);
        symbMap.Add(str, i);
      }
    }

    public static int StrToIdx(string str) {
      int idx;
      if (symbMap.TryGetValue(str, out idx))
        return idx;
      int count = symbTable.Count;
      if (count < 65535) {
        idx = count;
        symbTable.Add(str);
        symbMap.Add(str, idx);
        return idx;
      }
      throw new InvalidOperationException();
    }

    public static string IdxToStr(int idx) {
      return symbTable[idx];
    }
  }
}