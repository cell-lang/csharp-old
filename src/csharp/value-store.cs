using System;


namespace CellLang {
  class ValueStoreBase {
    protected Obj[] slots;
    protected int[] hashcodes;
    protected int[] hashtable;
    protected int[] buckets;
    protected int   count = 0;

    protected ValueStoreBase() {

    }

    protected ValueStoreBase(int initSize) {
      slots     = new Obj[initSize];
      hashcodes = new int[initSize];
      hashtable = new int[initSize];
      buckets   = new int[initSize];

      for (int i=0 ; i < initSize ; i++)
        hashtable[i] = -1;
    }

    public int Count() {
      return count;
    }

    public int Capacity() {
      return slots != null ? slots.Length : 0;
    }

    public int LookupValue(Obj value) {
      int hashcode = value.Hashcode();
      int idx = hashtable[hashcode % hashtable.Length];
      while (idx != -1) {
        if (hashcodes[idx] == hashcode && value.IsEq(slots[idx]))
          return idx;
        idx = buckets[idx];
      }
      return -1;
    }

    public Obj GetValue(uint index) {
      return slots[index];
    }

    public void Insert(Obj value, int slotIdx) {
      Insert(value, value.Hashcode(), slotIdx);
    }

    public virtual void Insert(Obj value, int hashcode, int slotIdx) {
      Miscellanea.Assert(slots != null && slotIdx < slots.Length);
      Miscellanea.Assert(slots[slotIdx] == null);
      Miscellanea.Assert(hashcode == value.Hashcode());

      slots[slotIdx] = value;
      hashcodes[slotIdx] = hashcode;

      //## DOES IT MAKE ANY DIFFERENCE HERE TO USE int INSTEAD OF uint?
      int hashtableIdx = hashcode % slots.Length;
      int head = hashtable[hashtableIdx];
      hashtable[hashtableIdx] = slotIdx;
      buckets[slotIdx] = head;

      count++;
    }

    public virtual void Resize(int minCapacity) {
      if (slots != null) {
        Miscellanea.Assert(slots.Length == count);

        int   currCapacity  = slots.Length;
        Obj[] currSlots     = slots;
        int[] currHashcodes = hashcodes;

        int newCapacity = 2 * currCapacity;
        while (newCapacity < minCapacity)
          newCapacity = 2 * newCapacity;

        slots     = new Obj[newCapacity];
        hashcodes = new int[newCapacity];
        hashtable = new int[newCapacity];
        buckets   = new int[newCapacity];

        Array.Copy(currSlots, slots, currCapacity);
        Array.Copy(currHashcodes, hashcodes, currCapacity);

        for (int i=0 ; i < newCapacity ; i++)
          hashtable[i] = -1;

        for (int i=0 ; i < currCapacity ; i++) {
          int slotIdx = hashcodes[i] % newCapacity;
          int head = hashtable[slotIdx];
          hashtable[slotIdx] = i;
          buckets[i] = head;
        }
      }
      else {
        const int MinCapacity = 32;

        slots     = new Obj[MinCapacity];
        hashcodes = new int[MinCapacity];
        hashtable = new int[MinCapacity];
        buckets   = new int[MinCapacity];

        for (int i=0 ; i < MinCapacity ; i++)
          hashtable[i] = -1;
      }
    }
  }


  class ValueStore : ValueStoreBase {
    const int InitSize = 256;

    int[] refCounts     = new int[InitSize];
    int[] nextFreeIdx   = new int[InitSize];
    int   firstFreeIdx  = 0;

    public ValueStore() : base(InitSize) {
      for (int i=0 ; i < InitSize ; i++)
        nextFreeIdx[i] = i + 1;
    }

    override public void Insert(Obj value, int hashcode, int index) {
      Miscellanea.Assert(firstFreeIdx == index);
      Miscellanea.Assert(nextFreeIdx[index] != -1);
      base.Insert(value, hashcode, index);
      firstFreeIdx = nextFreeIdx[index];
      nextFreeIdx[index] = -1; //## UNNECESSARY, BUT USEFUL FOR DEBUGGING
    }

    override public void Resize(int minCapacity) {
      base.Resize(minCapacity);
      int capacity = slots.Length;

      int[] currRefCounts   = refCounts;
      int[] currNextFreeIdx = nextFreeIdx;
      int currCapacity = currRefCounts.Length;

      refCounts   = new int[capacity];
      nextFreeIdx = new int[capacity];

      Array.Copy(currRefCounts, refCounts, currCapacity);
      Array.Copy(currNextFreeIdx, nextFreeIdx, currCapacity);

      for (int i=currCapacity ; i < capacity ; i++)
        currNextFreeIdx[i] = i + 1;
    }

    public int NextFreeIdx(int index) {
      Miscellanea.Assert(index == -1 || (slots[index] == null & nextFreeIdx[index] != -1));
      return index != -1 ? nextFreeIdx[index] : firstFreeIdx;
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

      if (count == capacity)
        Resize(count+1);

      lastSurrogate = store.NextFreeIdx(lastSurrogate);
      surrogates[count] = lastSurrogate;
      Insert(value, count);
      return lastSurrogate;
    }

    override public void Resize(int minCapacity) {
      Resize(count+1);
      int[] currSurrogates = surrogates;
      surrogates = new int[slots.Length];
      Array.Copy(currSurrogates, surrogates, count);
    }

    public void Apply() {
      if (count == 0)
        return;

      int storeCapacity = store.Capacity();
      int storeCount = store.Count();

      int reqCapacity = store.Count() + count;

      if (storeCapacity < reqCapacity)
        store.Resize(reqCapacity);

      for (int i=0 ; i < count ; i++)
        store.Insert(slots[i], hashcodes[i], surrogates[i]);

      count = 0;
    }

    public void Finish() {

    }

    public long LookupValueEx(Obj value) {
      int surrogate = store.LookupValue(value);
      if (surrogate != -1)
        return surrogate;
      int index = LookupValue(value);
      if (index == -1)
        return -1;
      return surrogates[index];
    }
  }
}
