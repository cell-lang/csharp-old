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
      long error_offset;
      bool ok = Parser.parse(bytes, out obj, out error_offset);
      if (!ok) {
        Console.WriteLine(text);
        throw new Exception("Syntax error at offset " + error_offset.ToString());
      }
      return obj;
    }

    public static Value ExportAsValue(Obj obj) {
      return obj.GetValue();
    }

    public static Obj StringToObj(string str) {
      throw new NotImplementedException();
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

    public static string[] ToSymbArray(Obj obj) {
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
