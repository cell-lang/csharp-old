using System;


namespace CellLang {
  public static class BinTableUnitTests {
    public static void Run() {
      BinaryTable table = new BinaryTable(null, null);

      bool[,] bitMap = new bool[20, 20];
      CheckTable(table, bitMap, 20);

      Random random = new Random(0);

      // Inserting until the table is full
      for (int i=0 ; i < 400 ; i++) {
        uint surr1 = (uint) random.Next(20);
        uint surr2 = (uint) random.Next(20);

        while (bitMap[surr1, surr2]) {
          surr2 = (surr2 + 1) % 20;
          if (surr2 == 0)
            surr1 = (surr1 + 1) % 20;
        }

        table.Insert(surr1, surr2);
        bitMap[surr1, surr2] = true;

        CheckTable(table, bitMap, 20);
      }

      // Deleting until the table is empty
      for (int i=0 ; i < 400 ; i++) {
        uint surr1 = (uint) random.Next(20);
        uint surr2 = (uint) random.Next(20);

        while (!bitMap[surr1, surr2]) {
          surr2 = (surr2 + 1) % 20;
          if (surr2 == 0)
            surr1 = (surr1 + 1) % 20;
        }

        table.Delete(surr1, surr2);
        bitMap[surr1, surr2] = false;

        CheckTable(table, bitMap, 20);
      }
    }

    static void CheckTable(BinaryTable table, bool[,] bitMap, int size) {
      table.Check();

      for (int i=0 ; i < size ; i++)
        for (int j=0 ; j < size ; j++)
          if (table.Contains(i, j) != bitMap[i, j]) {
            Console.Error.WriteLine("ERROR!");
            Environment.Exit(1);
          }
    }
  }
}