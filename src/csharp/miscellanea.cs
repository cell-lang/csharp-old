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

    public static Obj Fail() {
      throw new InvalidOperationException();
    }

    public static long RandNat(Obj max) {
      return 0;
    }

    static int nextUniqueNat = 0;
    public static long UniqueNat() {
      return nextUniqueNat++;
    }

    public static long GetTickCount() {
      return Environment.TickCount & Int32.MaxValue;
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

    public static void Trace(bool cond, string message) {
      if (!cond) {
        Console.WriteLine("*** TRACE: " + message);
      }
    }

    public static bool IsHexDigit(byte b) {
      char ch = (char) b;
      return ('0' <= ch & ch <= '9') | ('a' <= ch & ch <= 'f') | ('A' <= ch & ch <= 'F');
    }
  }
}
