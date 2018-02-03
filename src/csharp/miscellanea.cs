using System;
using System.IO;
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
      PrintCallStack();
      Environment.Exit(1);
      return null;
    }

    public static Obj SoftFail() {
      PrintCallStack();
      throw new InvalidOperationException();
    }

    public static Obj HardFail() {
      PrintCallStack();
      Environment.Exit(1);
      return null;
    }

    public static void ImplFail(string msg) {
      if (msg != null)
        Console.Error.WriteLine(msg + "\n");
      PrintCallStack();
      Environment.Exit(1);
    }

    public static void InternalFail() {
      Console.Error.WriteLine("Internal error!\n");
      PrintCallStack();
      Environment.Exit(1);
    }

    public static void PrintAssertionFailedMsg(string file, int line, string text) {
      if (text == null)
        Console.WriteLine("\nAssertion failed. File: {0}, line: {1}\n\n", file, line);
      else
        Console.WriteLine("\nAssertion failed: {0}\nFile: {1}, line: {2}\n\n", text, file, line);
    }

    public static void DumpVar(string name, Obj obj) {
      string str = PrintedObjOrFilename(obj, true);
      Console.WriteLine("{0} = {1}\n", name, str);
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

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

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

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

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

    static void PrintCallStack() {
      Console.WriteLine("Call stack:\n");
      int size = stackDepth <= fnNamesStack.Length ? stackDepth : fnNamesStack.Length;
      for (int i=0 ; i < size ; i++)
        Console.WriteLine("  {0}", fnNamesStack[i]);
      string outFnName = "debug" + Path.DirectorySeparatorChar + "stack-trace.txt";
      Console.Error.WriteLine("\nNow trying to write a full dump of the stack to " + outFnName);
      Console.Error.Flush();
      try {
        using (StreamWriter file = new StreamWriter(outFnName))
          for (int i=0 ; i < size ; i++)
            PrintStackFrame(i, file);
        Console.WriteLine("");
      }
      catch {
        Console.Error.WriteLine("Could not write a dump of the stack to {0}. Did you create the \"debug\" directory?", outFnName);
      }
    }

    static void PrintStackFrame(int frameIdx, TextWriter writer) {
      Obj[] args = argsStack[frameIdx];
      writer.Write("{0}(", fnNamesStack[frameIdx]);
      if (args != null) {
        writer.WriteLine("");
        for (int i=0 ; i < args.Length ; i++)
          PrintIndentedArg(args[i], i == args.Length - 1, writer);
      }
      writer.WriteLine(")\n");
    }

    static void PrintIndentedArg(Obj arg, bool isLast, TextWriter writer) {
      string str = arg.IsBlankObj() ? "<closure>" : PrintedObjOrFilename(arg, false);
      for (int i=0 ; i < str.Length ; i++) {
        if (i == 0 || str[i] == '\n')
          writer.Write("  ");
        writer.Write(str[i]);
      }
      if (!isLast)
        writer.Write(',');
      writer.WriteLine("");
      writer.Flush();
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    static List<Obj> filedObjs = new List<Obj>();

    static string PrintedObjOrFilename(Obj obj, bool addPath) {
      string path = addPath ? "debug" + Path.DirectorySeparatorChar : "";

      for (int i=0 ; i < filedObjs.Count ; i++)
        if (filedObjs[i].IsEq(obj))
          return String.Format("<{0}obj-{1}.txt>", path, i);

      string str = obj.ToString();
      if (str.Length <= 50)
        return str;

      string outFnName = String.Format("debug{0}obj-{1}.txt", Path.DirectorySeparatorChar, filedObjs.Count);
      File.WriteAllText(outFnName, str);
      filedObjs.Add(obj);
      return String.Format("<{0}obj-{1}.txt>", path, filedObjs.Count-1);
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static bool IsHexDigit(byte b) {
      char ch = (char) b;
      return ('0' <= ch & ch <= '9') | ('a' <= ch & ch <= 'f') | ('A' <= ch & ch <= 'F');
    }

    public static int HexDigitValue(byte b) {
      char ch = (char) b;
      return ch - (ch >= '0' & ch <= '9' ? '0' : (ch >= 'a' & ch <= 'f' ? 'a' : 'A'));
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

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public static void WriteIndentedNewLine(this TextWriter writer, int level) {
      WriteIndentedNewLine(writer, "", level);
    }

    public static void WriteIndentedNewLine(this TextWriter writer, string str, int level) {
      Console.WriteLine(str);
      for (int i=0 ; i < level ; i++)
        writer.Write("  ");
    }
  }
}
