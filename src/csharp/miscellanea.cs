using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public static class Miscellanea {
    public static Obj StrToObj(string str) {
      int len = str.Length;
      Obj[] chars = new Obj[len];
      int count = 0;
      int i = 0;
      while (i < len) {
        int ch = Char.ConvertToUtf32(str, i);
        chars[count++] = new IntObj(ch);
        i += Char.IsSurrogatePair(str, i) ? 2 : 1;
      }
      return new TaggedObj(SymbTable.StringSymbId, new MasterSeqObj(chars, count));
    }

    public static string ObjToStr(Obj str) {
      return null;
    }

    public static void Fail() {
      throw new InvalidOperationException();
    }

    public static Obj RandNat(Obj max) {
      return null;
    }

    public static Obj UniqueNat() {
      return null;
    }

    public static Obj GetTickCount() {
      return null;
    }

    public static void Assert(bool cond) {
      if (!cond) {
        Console.WriteLine("Assertion failed");
        throw new Exception();
      }
    }

    public static void Assert(bool cond, string message) {
      if (!cond) {
        Console.WriteLine("Assertion failed: " + message);
        throw new Exception();
      }
    }
  }
}
