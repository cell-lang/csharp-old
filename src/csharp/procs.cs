using System;
using System.IO;


namespace CellLang {
  public static class Procs {
    public static Obj FileRead_P(Obj fname, object env) {
      string fnameStr = fname.GetString();
      try {
        byte[] content = File.ReadAllBytes(fnameStr);
        Obj bytesObj = Builder.BuildConstIntSeq(content);
        return new TaggedObj(SymbTable.JustSymbId, bytesObj);
      }
      catch {
        return SymbObj.Get(SymbTable.NothingSymbId);
      }
    }

    public static Obj FileWrite_P(Obj fname, Obj data, object env) {
      string fnameStr = fname.GetString();
      byte[] bytes = data.GetByteArray();
      try {
        File.WriteAllBytes(fnameStr, bytes);
        return SymbObj.Get(SymbTable.TrueSymbId);
      }
      catch {
        return SymbObj.Get(SymbTable.FalseSymbId);
      }
    }

    public static void Print_P(Obj str, object env) {
      Console.Write(str.GetString());
    }

    public static Obj GetChar_P(object env) {
      int ch = Console.Read();
      if (ch != -1)
        return new TaggedObj(SymbTable.JustSymbId, IntObj.Get(ch));
      else
        return SymbObj.Get(SymbTable.NothingSymbId);
    }
  }
}
