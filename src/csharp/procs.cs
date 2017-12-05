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
        return new SymbObj(SymbTable.NothingSymbId);
      }
    }

    public static Obj FileWrite_P(Obj fname, Obj data, object env) {
      string fnameStr = fname.GetString();
      byte[] bytes = data.GetByteArray();
      try {
        File.WriteAllBytes(fnameStr, bytes);
        return new SymbObj(SymbTable.TrueSymbId);
      }
      catch {
        return new SymbObj(SymbTable.FalseSymbId);
      }
    }

    public static void Print_P(Obj str, object env) {
      Console.Write(str.GetString());
    }

    public static Obj GetChar_P(object env) {
      int ch = Console.Read();
      if (ch != -1)
        return new TaggedObj(SymbTable.JustSymbId, new IntObj(ch));
      else
        return new SymbObj(SymbTable.NothingSymbId);
    }
  }
}
