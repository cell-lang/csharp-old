using System;


namespace CellLang {
  class ValueStoreBase {
    protected Obj[] slots;
    protected int[] hashcodes;
    protected int[] hashtable;
    protected int[] chains;
    protected int   count = 0;

    protected ValueStoreBase() {

    }

    protected ValueStoreBase(int initSize) {
      slots     = new Obj[initSize];
      hashcodes = new int[initSize];
      hashtable = new int[initSize];
      chains    = new int[initSize];

      for (int i=0 ; i < initSize ; i++)
        hashtable[i] = -1;
    }

    protected void Insert(Obj value, int slotIdx) {
      int hashcode = value.Hashcode();

      slots[slotIdx] = value;
      hashcodes[slotIdx] = hashcode;

      //## DOES IT MAKE ANY DIFFERENCE HERE TO USE int INSTEAD OF uint?
      int hashtableIdx = hashcode % slots.Length;
      int head = hashtable[hashtableIdx];
      hashtable[hashtableIdx] = slotIdx;
      chains[slotIdx] = head;

      count++;
    }

    protected void Resize() {
      if (slots != null) {
        Miscellanea.Assert(slots.Length == count);

        int   currCapacity  = slots.Length;
        Obj[] currSlots     = slots;
        int[] currHashcodes = hashcodes;

        int newCapacity = 2 * currCapacity;
        slots     = new Obj[newCapacity];
        hashcodes = new int[newCapacity];
        hashtable = new int[newCapacity];
        chains    = new int[newCapacity];

        Array.Copy(currSlots, slots, currCapacity);
        Array.Copy(currHashcodes, hashcodes, currCapacity);

        for (int i=0 ; i < newCapacity ; i++)
          hashtable[i] = -1;

        for (int i=0 ; i < currCapacity ; i++) {
          int slotIdx = hashcodes[i] % newCapacity;
          int head = hashtable[slotIdx];
          hashtable[slotIdx] = i;
          chains[i] = head;
        }
      }
      else {
        const int MinCapacity = 32;

        slots     = new Obj[MinCapacity];
        hashcodes = new int[MinCapacity];
        hashtable = new int[MinCapacity];
        chains    = new int[MinCapacity];

        for (int i=0 ; i < MinCapacity ; i++)
          hashtable[i] = -1;
      }
    }
  }


  class ValueStore : ValueStoreBase {
    const int InitSize = 256;

    int[] refCounts = new int[InitSize];
    int[] nextFree  = new int[InitSize];
    int   firstFree = 0;

    public ValueStore() : base(InitSize) {
      for (int i=0 ; i < InitSize ; i++)
        nextFree[i] = i + 1;
    }

    public int LookupValue(Obj value) {
      throw new NotImplementedException();
    }

    public int NextSurrogate(int surrogate) {
      return nextFree[surrogate];
    }
  }


  class ValueStoreUpdater : ValueStoreBase {
    int[] surrogates;
    int   lastSurrogate = -1;

    ValueStore store;

    public ValueStoreUpdater(ValueStore store) {
      this.store = store;
    }

    public int Insert(Obj value) {
      int capacity = slots != null ? slots.Length : 0;
      Miscellanea.Assert(count <= capacity);

      if (count == capacity) {
        Resize();
        int[] currSurrogates = surrogates;
        surrogates = new int[slots.Length];
        Array.Copy(currSurrogates, surrogates, count);
      }

      lastSurrogate = store.NextSurrogate(lastSurrogate);
      surrogates[count] = lastSurrogate;
      Insert(value, count);
      return lastSurrogate;
    }

    public void Apply() {

    }

    public void Finish() {

    }

    public long LookupValueEx(Obj value) {
      throw new Exception();
    }
  }
}
