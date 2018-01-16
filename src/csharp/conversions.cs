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


    public static bool[] ToBoolArray(Obj obj, bool stripTag) {
      throw new NotImplementedException();
    }

    public static long[] ToLongArray(Obj obj, bool stripTag) {
      throw new NotImplementedException();
    }

    public static double[] ToDoubleArray(Obj obj, bool stripTag) {
      throw new NotImplementedException();
    }

    public static string[] ToSymbArray(Obj obj, bool stripTag) {
      throw new NotImplementedException();
    }

    public static string[] ToStringArray(Obj obj, bool stripTag) {
      throw new NotImplementedException();
    }

    public static Value[] ToValueArray(Obj obj) {
      throw new NotImplementedException();
    }
  }
}