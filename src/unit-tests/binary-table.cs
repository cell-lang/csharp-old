using System;
using System.Collections.Generic;


namespace CellLang {
  public static class BinTableUnitTests {
    public static void Run() {
      for (int i=1 ; i < 50 ; i++) {
        Console.WriteLine(i.ToString());
        for (int j=0 ; j < 100 ; j++) {
        //  Console.Write("{0} ", j);
          Run(i, false);
        }
        //Console.WriteLine();
      }

      Console.WriteLine();

      for (int i=1 ; i < 240 ; i++) {
        Console.WriteLine(i.ToString());
        Run(i, false);
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
          Console.WriteLine("Inserting: ({0,2}, {1,2})", surr1, surr2);

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
            Console.Error.WriteLine("ERROR (1)!\n");
            PrintDiffs(table, bitMap, size);
            //throw new Exception();
            Environment.Exit(1);
          }

      for (uint i=0 ; i < size ; i++) {
        List<uint> list = new List<uint>();
        for (uint j=0 ; j < size ; j++)
          if (bitMap[i, j])
            list.Add(j);
        uint[] expValues = list.ToArray();

        if (table.ContainsField1(i) != (expValues.Length > 0)) {
          Console.Error.WriteLine("ERROR (2)!\n");
          for (int k=0 ; k < expValues.Length ; k++)
            Console.Error.Write("{0} ", expValues[k]);
          Console.Error.WriteLine();
          Console.Error.WriteLine("{0}", table.ContainsField1(i));
          Environment.Exit(1);
        }

        uint[] actualValues = table.LookupByCol1(i);
        Array.Sort(actualValues);
        if (!Eq(actualValues, expValues)) {
          Console.Error.WriteLine("ERROR (3)!\n");
          Environment.Exit(1);
        }

        list = new List<uint>();
        BinaryTable.Iter it = table.GetIter1(i);
        while (!it.Done()) {
          list.Add(it.GetField2());
          it.Next();
        }
        actualValues = list.ToArray();
        Array.Sort(actualValues);
        if (!Eq(actualValues, expValues)) {
          Console.Error.WriteLine("ERROR (4)!\n");
          Environment.Exit(1);
        }
      }

      for (uint j=0 ; j < size ; j++) {
        List<uint> list = new List<uint>();
        for (uint i=0 ; i < size ; i++)
          if (bitMap[i, j])
            list.Add(i);
        uint[] expValues = list.ToArray();

        if (table.ContainsField2(j) != (expValues.Length > 0)) {
          Console.Error.WriteLine("ERROR (5)!\n");
          Environment.Exit(1);
        }

        uint[] actualValues = table.LookupByCol2(j);
        Array.Sort(actualValues);
        if (!Eq(actualValues, expValues)) {
          Console.Error.WriteLine("ERROR (6)!\n");
          Environment.Exit(1);
        }

        list = new List<uint>();
        BinaryTable.Iter it = table.GetIter2(j);
        while (!it.Done()) {
          list.Add(it.GetField1());
          it.Next();
        }
        actualValues = list.ToArray();
        Array.Sort(actualValues);
        if (!Eq(actualValues, expValues)) {
          Console.Error.WriteLine("ERROR (4)!\n");
          Environment.Exit(1);
        }
      }
    }

    static bool Eq(uint[] a1, uint[] a2) {
      if (a1.Length != a2.Length)
        return false;
      for (int i=0 ; i < a1.Length ; i++)
        if (a1[i] != a2[i])
          return false;
      return true;
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