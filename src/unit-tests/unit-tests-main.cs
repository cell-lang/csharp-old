using System;
using CellLang;


namespace CellLang {
  static class UnitTests {
    public static void Main(string[] args) {
      BinTableUnitTests.Run();
      Console.WriteLine("OK");
    }
  }

  static class Generated {
    public static string[] EmbeddedSymbols = {};
  }
}
