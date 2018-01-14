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
  }
}