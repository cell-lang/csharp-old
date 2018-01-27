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
        chars[count++] = IntObj.Get(ch);
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

    public static Obj SoftFail(int val) {
      throw new InvalidOperationException();
    }

    public static void PrintAssertionFailedMsg(string file, int line, string text) {
      if (text == null)
        Console.WriteLine("\nAssertion failed. File: {0}, line: {1}\n\n", file, line);
      else
        Console.WriteLine("\nAssertion failed: {0}\nFile: {1}, line: {2}\n\n", text, file, line);
    }

    public static void DumpVar(string name, Obj val) {
      Console.WriteLine("{0} = {1}\n", name, val.ToString());
    }

    static Random random = new Random(0);

    public static long RandNat(long max) {
      return random.Next((int) max);
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
        StackTrace st = new StackTrace(true);
        Console.WriteLine(st.ToString());
        Environment.Exit(1);
      }
    }

    public static void Assert(bool cond, string message) {
      if (!cond) {
        Console.WriteLine("Assertion failed: " + message);
        StackTrace st = new StackTrace(true);
        Console.WriteLine(st.ToString());
        Environment.Exit(1);
      }
    }

    public static void Trace(bool cond, string message) {
      if (!cond) {
        Console.WriteLine("*** TRACE: " + message);
      }
    }

    static int      stackDepth = 0;
    static string[] fnNamesStack = new String[100];
    static Obj[][]  argsStack = new Obj[100][];

    public static void PushCallInfo(string fnName, Obj[] args) {
      if (stackDepth < 100) {
        fnNamesStack[stackDepth] = fnName;
        argsStack[stackDepth]    = args;
      }
      stackDepth++;
    }

    public static void PopCallInfo() {
      stackDepth--;
    }

    public static bool IsHexDigit(byte b) {
      char ch = (char) b;
      return ('0' <= ch & ch <= '9') | ('a' <= ch & ch <= 'f') | ('A' <= ch & ch <= 'F');
    }

    public static uint Hashcode(uint n) {
      return n;
    }

    public static uint Hashcode(uint n1, uint n2) {
      return n1 ^ n2;
    }

    public static uint Hashcode(uint n1, uint n2, uint n3) {
      return n1 ^ n2 ^ n3;
    }

    public static int[] CodePoints(string str) {
      int len = str.Length;
      List<int> cps = new List<int>(len);
      for (int i=0 ; i < len ; i++) {
        cps.Add(Char.ConvertToUtf32(str, i));
        if (Char.IsHighSurrogate(str[i]))
          i++;
      }
      return cps.ToArray();
    }

    public static bool debugFlag = false;
  }
}
