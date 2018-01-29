using System;


namespace CellLang {
  static class Conversions {
    public static Obj ConvertText(string text) {
      int len = text.Length;
      byte[] bytes = new byte[len];
      for (int i=0 ; i < len ; i++) {
        char ch = text[i];
        if (ch > 255)
          throw new Exception("Invalid character at offset " + i.ToString());
        bytes[i] = (byte) ch;
      }

      Obj obj;
      long errorOffset;
      bool ok = Parser.Parse(bytes, out obj, out errorOffset);
      if (!ok) {
        throw new Exception("Syntax error at offset " + errorOffset.ToString());
      }
      return obj;
    }

    public static Value ExportAsValue(Obj obj) {
      return obj.GetValue();
    }

    public static Obj StringToObj(string str) {
      int[] cps = Miscellanea.CodePoints(str);
      int len = cps.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = IntObj.Get(cps[i]);
      return new TaggedObj(SymbTable.StringSymbId, new MasterSeqObj(objs));
    }

    ////////////////////////////////////////////////////////////////////////////

    public static bool[] ToBoolArray(Obj obj) {
      int size = obj.GetSize();
      bool[] array = new bool[size];
      SeqOrSetIter it = obj.GetSeqOrSetIter();
      int idx = 0;
      while (!it.Done()) {
        array[idx++] = it.Get().GetBool();
        it.Next();
      }
      return array;
    }

    public static long[] ToLongArray(Obj obj) {
      int size = obj.GetSize();
      long[] array = new long[size];
      SeqOrSetIter it = obj.GetSeqOrSetIter();
      int idx = 0;
      while (!it.Done()) {
        array[idx++] = it.Get().GetLong();
        it.Next();
      }
      return array;
    }

    public static double[] ToDoubleArray(Obj obj) {
      int size = obj.GetSize();
      double[] array = new double[size];
      SeqOrSetIter it = obj.GetSeqOrSetIter();
      int idx = 0;
      while (!it.Done()) {
        array[idx++] = it.Get().GetDouble();
        it.Next();
      }
      return array;
    }

    public static string[] ToSymbolArray(Obj obj) {
      int size = obj.GetSize();
      string[] array = new string[size];
      SeqOrSetIter it = obj.GetSeqOrSetIter();
      int idx = 0;
      while (!it.Done()) {
        array[idx++] = it.Get().ToString();
        it.Next();
      }
      return array;
    }

    public static string[] ToStringArray(Obj obj) {
      int size = obj.GetSize();
      string[] array = new string[size];
      SeqOrSetIter it = obj.GetSeqOrSetIter();
      int idx = 0;
      while (!it.Done()) {
        array[idx++] = it.Get().GetString();
        it.Next();
      }
      return array;
    }

    public static Value[] ToValueArray(Obj obj) {
      int size = obj.GetSize();
      Value[] array = new Value[size];
      SeqOrSetIter it = obj.GetSeqOrSetIter();
      int idx = 0;
      while (!it.Done()) {
        array[idx++] = it.Get().GetValue();
        it.Next();
      }
      return array;
    }
  }
}
