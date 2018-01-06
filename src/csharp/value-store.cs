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

      for (int i=0 ; i < initSize ; i++) {
        hashtable[i] = -1;
        buckets[i] = -1;
      }
    }

    public virtual void Reset() {
      if (slots != null) {
        int size = slots.Length;
        for (int i=0 ; i < size ; i++) {
          slots[i] = null;
          hashcodes[i] = 0; //## IS THIS NECESSARY?
          hashtable[i] = -1;
          buckets[i] = -1;
        }
        count = 0;
      }
    }

    public int Count() {
      return count;
    }

    public int Capacity() {
      return slots != null ? slots.Length : 0;
    }

    public int LookupValue(Obj value) {
      if (count == 0)
        return -1;
      int hashcode = value.Hashcode();
      int idx = hashtable[hashcode % hashtable.Length];
      while (idx != -1) {
        Miscellanea.Assert(slots[idx] != null);
        if (hashcodes[idx] == hashcode && value.IsEq(slots[idx]))
          return idx;
        idx = buckets[idx];
      }
      return -1;
    }

    //## IS THIS A DUPLICATE OF GetValue()?
    public Obj LookupSurrogate(uint index) {
      return slots[index];
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

    protected void Delete(int index) {
      Miscellanea.Assert(slots != null && index < slots.Length);
      Miscellanea.Assert(slots[index] != null);

      int hashcode = hashcodes[index];

      slots[index] = null;
      hashcodes[index] = 0; //## NOT STRICTLY NECESSARY...
      count--;

      int hashtableIdx = hashcode % slots.Length;
      int idx = hashtable[hashtableIdx];
      Miscellanea.Assert(idx != -1);

      if (idx == index) {
        hashtable[hashtableIdx] = buckets[idx];
        buckets[idx] = -1;
        return;
      }

      int prevIdx = idx;
      idx = buckets[idx];
      while (idx != index) {
        prevIdx = idx;
        idx = buckets[idx];
        Miscellanea.Assert(idx != -1);
      }

      buckets[prevIdx] = buckets[idx];
      buckets[idx] = -1;
    }

    public virtual void Resize(int minCapacity) {
      if (slots != null) {
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

    public virtual void Dump() {
      Console.WriteLine("");
      Console.WriteLine("count = " + count.ToString());
      WriteObjs("slots", slots);
      WriteInts("hashcodes", hashcodes);
      WriteInts("hashtable", hashtable);
      WriteInts("buckets", buckets);
    }

    protected void WriteObjs(string name, Obj[] objs) {
      Console.WriteLine(name + " = ");
      if (objs != null) {
        Console.Write("[");
        for (int i=0 ; i < objs.Length ; i++) {
          if (i > 0)
            Console.Write(", ");
          Obj obj = objs[i];
          Console.Write(obj != null ? obj.ToString() : "null");
        }
        Console.WriteLine("]");
      }
      else
        Console.WriteLine("null");
    }

    protected void WriteInts(string name, int[] ints) {
      Console.Write(name + " = ");
      if (ints != null) {
        Console.Write("[");
        for (int i=0 ; i < ints.Length ; i++) {
          if (i > 0)
            Console.Write(", ");
          Console.Write(ints[i].ToString());
        }
        Console.WriteLine("]");
      }
      else
        Console.WriteLine("null");
    }

  }


  class ValueStore : ValueStoreBase {
    const int InitSize = 16;

    int[] refCounts     = new int[InitSize];
    int[] nextFreeIdx   = new int[InitSize];
    int   firstFreeIdx  = 0;

    public ValueStore() : base(InitSize) {
      for (int i=0 ; i < InitSize ; i++)
        nextFreeIdx[i] = i + 1;
    }

    public void AddRef(uint index) {
      refCounts[index] = refCounts[index] + 1;
    }

    public void Release(uint index) {
      int refCount = refCounts[index];
      Miscellanea.Assert(refCount > 0);
      refCounts[index] = refCount - 1;
      if (refCount == 1) {
        Delete((int) index);
        nextFreeIdx[index] = firstFreeIdx;
        firstFreeIdx = (int) index;
      }
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
        nextFreeIdx[i] = i + 1;
    }

    public int NextFreeIdx(int index) {
      Miscellanea.Assert(index == -1 || (slots[index] == null & nextFreeIdx[index] != -1));
      return index != -1 ? nextFreeIdx[index] : firstFreeIdx;
    }

    override public void Dump() {
      base.Dump();
      WriteInts("refCounts", refCounts);
      WriteInts("nextFreeIdx", nextFreeIdx);
      Console.WriteLine("firstFreeIdx = " + firstFreeIdx.ToString());
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
      base.Resize(count+1);
      int[] currSurrogates = surrogates;
      surrogates = new int[slots.Length];
      if (count > 0)
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

      Reset();
    }

    override public void Reset() {
      base.Reset();
      lastSurrogate = -1;
      //## IS THIS NECESSARY?
      if (surrogates != null) {
        int len = surrogates.Length;
        for (int i=0 ; i < len ; i++)
          surrogates[i] = 0;
      }
    }

    public int LookupValueEx(Obj value) {
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
