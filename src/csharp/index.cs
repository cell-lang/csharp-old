using System;
using System.Collections.Generic;


namespace CellLang {
  struct Index {
    const uint Empty = 0xFFFFFFFF;

    public uint[] hashtable;
    public uint[] buckets;

    public void Init(uint size) {
      hashtable = new uint[size];
      buckets   = new uint[size];
      for (int i=0 ; i < size ; i++) {
        hashtable[i] = Empty;
        buckets[i] = Empty;
      }
    }

    public void Clear() {
      if (hashtable != null) {
        int size = hashtable.Length;
        for (int i=0 ; i < size ; i++) {
          hashtable[i] = Empty;
          buckets[i] = Empty;
        }
      }
    }

    public void Insert(uint index, uint hashcode) {
      Miscellanea.Assert(buckets[index] == Empty);
      Miscellanea.Assert(index < hashtable.Length);

      uint hashIdx = hashcode % (uint) hashtable.Length;
      uint head = hashtable[hashIdx];
      hashtable[hashIdx] = index;
      buckets[index] = head;
    }

    public void Delete(uint index, uint hashcode) {
      uint hashIdx = hashcode % (uint) hashtable.Length;
      uint head = hashtable[hashIdx];
      Miscellanea.Assert(head != Empty);

      if (head == index) {
        hashtable[hashIdx] = buckets[index];
        buckets[index] = Empty;
        return;
      }

      uint curr = head;
      for ( ; ; ) {
        uint next = buckets[curr];
        Miscellanea.Assert(next != Empty);
        if (next == index) {
          buckets[curr] = buckets[next];
          buckets[next] = Empty;
          return;
        }
        curr = next;
      }
    }

    public bool IsBlank() {
      return hashtable == null;
    }

    public uint Head(uint hashcode) {
      return hashtable[hashcode % hashtable.Length];
    }

    public uint Next(uint index) {
      return buckets[index];
    }

    public void Dump() {
      Console.Write("hashtable =");
      if (hashtable != null)
        for (int i=0 ; i < hashtable.Length ; i++)
          Console.Write(" " + (hashtable[i] == Empty ? "-" : hashtable[i].ToString()));
      else
        Console.Write(" null");
      Console.WriteLine("");

      Console.Write("buckets   =");
      if (hashtable != null)
        for (int i=0 ; i < buckets.Length ; i++)
          Console.Write(" " + (buckets[i] == Empty ? "-" : buckets[i].ToString()));
      else
        Console.Write(" null");
      Console.WriteLine("");
      Console.WriteLine("");
    }
  }
}