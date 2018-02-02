using System;


namespace CellLang {
  public static class BinTableUnitTests {
    public static void Run() {
      for (int i=1 ; i <= 200 ; i++) {
        Console.WriteLine(i.ToString());
        for (int j=0 ; j < 100 ; j++) {
          // Console.Write("{0} ", j);
          Run(i, false);
        }
        // Console.WriteLine();
      }
    }

    static Random random = new Random(0);

    public static void Run(int range, bool trace) {
      BinaryTable table = new BinaryTable(null, null);

      bool[,] bitMap = new bool[range, range];
      CheckTable(table, bitMap, range);

      // Random random = new Random(0);

      // Inserting until the table is full
      for (int i=0 ; i < range * range ; i++) {
        uint surr1 = (uint) random.Next(range);
        uint surr2 = (uint) random.Next(range);

        while (bitMap[surr1, surr2]) {
          surr2 = (surr2 + 1) % (uint) range;
          if (surr2 == 0)
            surr1 = (surr1 + 1) % (uint) range;
        }

        if (trace)
          Console.WriteLine("Inserting: ({0}, {1})", surr1, surr2);

        table.Insert(surr1, surr2);
        bitMap[surr1, surr2] = true;

        CheckTable(table, bitMap, range);
      }

      // Deleting until the table is empty
      for (int i=0 ; i < range * range ; i++) {
        uint surr1 = (uint) random.Next(range);
        uint surr2 = (uint) random.Next(range);

        while (!bitMap[surr1, surr2]) {
          surr2 = (surr2 + 1) % (uint) range;
          if (surr2 == 0)
            surr1 = (surr1 + 1) % (uint) range;
        }

        if (trace)
          Console.WriteLine("Deleting: ({0}, {1})", surr1, surr2);

        table.Delete(surr1, surr2);
        bitMap[surr1, surr2] = false;

        CheckTable(table, bitMap, range);
      }
    }

    static void CheckTable(BinaryTable table, bool[,] bitMap, int size) {
      table.Check();

      for (int i=0 ; i < size ; i++)
        for (int j=0 ; j < size ; j++)
          if (table.Contains(i, j) != bitMap[i, j]) {
            Console.Error.WriteLine("ERROR!\n");
            PrintDiffs(table, bitMap, size);
            //throw new Exception();
            Environment.Exit(1);
          }
    }

    static void PrintDiffs(BinaryTable table, bool[,] bitMap, int size) {
      for (int i=0 ; i < size ; i++) {
        for (int j=0 ; j < size ; j++) {
          int actual = table.Contains(i, j) ? 1 : 0;
          int expected = bitMap[i, j] ? 1 : 0;
          Console.Write("{0}/{1} ", actual, expected);
        }
        Console.WriteLine();
      }
    }
  }
}