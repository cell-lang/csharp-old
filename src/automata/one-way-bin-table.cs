using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  struct OverflowTable {
    public struct Iter {
      uint[] values;
      uint   next;
      uint   end;

      public Iter(uint[] values, uint first, uint count) {
        this.values = values;
        this.next   = first;
        this.end    = first + count;
      }

      public uint Get() {
        Miscellanea.Assert(!Done());
        return values[next];
      }

      public bool Done() {
        return next >= end;
      }

      public void Next() {
        Miscellanea.Assert(!Done());
        next++;
      }
    }


    const int MinSize = 32;

    public const uint EmptyMarker  = 0xFFFFFFFFU;

    const uint EndLowerMarker   = 0xDFFFFFFFU;
    const uint End2UpperMarker  = 0x3FFFFFFFU;
    const uint End4UpperMarker  = 0x5FFFFFFFU;
    const uint End8UpperMarker  = 0x7FFFFFFFU;
    const uint End16UpperMarker = 0x9FFFFFFFU;

    public const uint PayloadMask  = 0x1FFFFFFFU;

    const uint InlineTag            = 0;
    const uint Block2Tag            = 1;
    const uint Block4Tag            = 2;
    const uint Block8Tag            = 3;
    const uint Block16Tag           = 4;
    const uint HashedBlockTag       = 5;
    const uint AvailableTag         = 6;
    const uint Unused               = 7;

    uint[] slots;
    uint head2;
    uint head4;
    uint head8;
    uint head16;

    public void Check(uint[] column, int count) {
      int len = slots.Length;
      bool[] slotOK = new bool[len];
      for (int i=0 ; i < len ; i++)
        Miscellanea.Assert(!slotOK[i]);

      if (head2 != EmptyMarker) {
        uint curr = head2;
        Check(slots[curr] == EndLowerMarker, "slots[curr] == EndLowerMarker (2), curr = " + curr.ToString());
        for ( ; ; ) {
          Check(curr < len, "curr < len");
          uint slot1 = slots[curr + 1];
          uint tag = slot1 >> 29;
          uint payload = slot1 & PayloadMask;
          Check(slot1 == End2UpperMarker | (tag == Block2Tag & payload < len), "slot1 == End2UpperMarker | (tag == Block2Tag & payload < len)");
          Check(!slotOK[curr] & !slotOK[curr+1], "!slotOK[curr] & !slotOK[curr+1]");
          slotOK[curr] = slotOK[curr+1] = true;
          if (slot1 == End2UpperMarker)
            break;
          uint nextSlot0 = slots[payload];
          Check(nextSlot0 >> 29 == AvailableTag, "nextSlot0 >> 29 == AvailableTag");
          Check((nextSlot0 & PayloadMask) == curr, "(nextSlot0 & PayloadMask) == curr");
          curr = payload;
        }
      }

      if (head4 != EmptyMarker) {
        uint curr = head4;
        Check(slots[curr] == EndLowerMarker, "slots[curr] == EndLowerMarker (4), curr = " + curr.ToString());
        for ( ; ; ) {
          Check(curr < len, "curr < len");
          uint slot1 = slots[curr + 1];
          uint tag = slot1 >> 29;
          uint payload = slot1 & PayloadMask;
          Check(slot1 == End4UpperMarker | (tag == Block4Tag & payload < len), "slot1 == End4UpperMarker | (tag == Block4Tag & payload < len)");
          for (int i=0 ; i < 4 ; i++) {
            Check(!slotOK[curr+i], "!slotOK[curr+i]");
            slotOK[curr+i] = true;
          }
          if (slot1 == End4UpperMarker)
            break;
          uint nextSlot0 = slots[payload];
          Check(nextSlot0 >> 29 == AvailableTag, "nextSlot0 >> 29 == AvailableTag");
          Check((nextSlot0 & PayloadMask) == curr, "(nextSlot0 & PayloadMask) == curr");
          curr = payload;
        }
      }

      if (head8 != EmptyMarker) {
        uint curr = head8;
        Check(slots[curr] == EndLowerMarker, "slots[curr] == EndLowerMarker (8), curr = " + curr.ToString());
        for ( ; ; ) {
          Check(curr < len, "curr < len");
          uint slot1 = slots[curr + 1];
          uint tag = slot1 >> 29;
          uint payload = slot1 & PayloadMask;
          Check(slot1 == End8UpperMarker | (tag == Block8Tag & payload < len), "slot1 == End8UpperMarker | (tag == Block8Tag & payload < len)");
          for (int i=0 ; i < 8 ; i++) {
            Check(!slotOK[curr+i], "!slotOK[curr+i]");
            slotOK[curr+i] = true;
          }
          if (slot1 == End8UpperMarker)
            break;
          uint nextSlot0 = slots[payload];
          Check(nextSlot0 >> 29 == AvailableTag, "nextSlot0 >> 29 == AvailableTag");
          Check((nextSlot0 & PayloadMask) == curr, "(nextSlot0 & PayloadMask) == curr");
          curr = payload;
        }
      }

      if (head16 != EmptyMarker) {
        uint curr = head16;
        Check(slots[curr] == EndLowerMarker, "slots[curr] == EndLowerMarker (16), curr = " + curr.ToString());
        for ( ; ; ) {
          Check(curr < len, "curr < len");
          uint slot1 = slots[curr + 1];
          uint tag = slot1 >> 29;
          uint payload = slot1 & PayloadMask;
          Check(slot1 == End16UpperMarker | (tag == Block16Tag & payload < len), "slot1 == End16UpperMarker | (tag == Block16Tag & payload < len)");
          for (int i=0 ; i < 16 ; i++) {
            Check(!slotOK[curr+i], "!slotOK[curr+i]");
            slotOK[curr+i] = true;
          }
          if (slot1 == End16UpperMarker)
            break;
          uint nextSlot0 = slots[payload];
          Check(nextSlot0 >> 29 == AvailableTag, "nextSlot0 >> 29 == AvailableTag");
          Check((nextSlot0 & PayloadMask) == curr, "(nextSlot0 & PayloadMask) == curr");
          curr = payload;
        }
      }

      uint actualCount = 0;
      for (int i=0 ; i < column.Length ; i++) {
        uint content = column[i];
        if (content == EmptyMarker)
          continue;
        uint tag = content >> 29;
        uint payload = content & PayloadMask;
        if (tag == 0) {
          Check(payload < 1000, "payload < 1000");
          actualCount++;
        }
        else {
          CheckGroup(tag, payload, slotOK, ref actualCount);
        }
      }

      Check(count == actualCount, "count == actualCount");

      for (int i=0 ; i < slotOK.Length ; i++) {
        if (!slotOK[i]) {
          for (int j=0 ; j < slotOK.Length ; j++) {
            if (j != 0 & j % 256 == 0)
              Console.WriteLine();
            if (j != 0 & j % 128 == 0)
              Console.WriteLine();
            if (j % 16 == 0)
              Console.Write("\n  ");
            Console.Write("{0} ", slotOK[j] ? 1 : 0);
          }
          Console.WriteLine();
          Console.WriteLine();
          Console.WriteLine("i = {0}", i);
          Console.WriteLine();
        }
        Check(slotOK[i], "slotOK[i]");
      }
    }

    void CheckGroup(uint tag, uint blockIdx, bool[] slotOK, ref uint totalCount) {
      Check(tag >= Block2Tag, "tag >= Block2Tag");
      Check(tag <= HashedBlockTag, "tag <= HashedBlockTag");

      int capacity, minUsage;
      if (tag == Block2Tag) {
        capacity = 2;
        minUsage = 2;
      }
      else if (tag == Block4Tag) {
        capacity = 4;
        minUsage = 2;
      }
      else if (tag == Block8Tag) {
        capacity = 8;
        minUsage = 4;
      }
      else if (tag == Block16Tag) {
        capacity = 16;
        minUsage = 7;
      }
      else {
        Miscellanea.Assert(tag == HashedBlockTag);
        capacity = 16;
        minUsage = 7; // Unused
      }

      for (int i=0 ; i < capacity ; i++) {
        Check(!slotOK[blockIdx+i], "!slotOK[blockIdx+i]");
        slotOK[blockIdx+i] = true;
      }

      if (tag != HashedBlockTag) {
        for (int i=0 ; i < capacity ; i++) {
          uint slot = slots[blockIdx + i];
          if (i < minUsage)
            Check(slot != EmptyMarker, "slot != EmptyMarker");
          if (slot == EmptyMarker) {
            for (int j=i+1 ; j < capacity ; j++)
              Check(slots[blockIdx+j] == EmptyMarker, "slots[blockIdx+j] == EmptyMarker");
            break;
          }
          Check(slot >> 29 == 0, "slot >> 29 == 0");
          Check((slot & PayloadMask) < 1000, "(slot & PayloadMask) < 1000");
          totalCount++;
        }
      }
      else {
        uint blockCount = slots[blockIdx];
        uint actualBlockCount = 0;
        for (int i=1 ; i < 16 ; i++) {
          uint slot = slots[blockIdx + i];
          if (slot != EmptyMarker) {
            uint slotTag = slot >> 29;
            uint slotPayload = slot & PayloadMask;
            if (slotTag == 0) {
              Check(slotPayload < 1000, "slotPayload < 1000");
              actualBlockCount++;
            }
            else
              CheckGroup(slotTag, slotPayload, slotOK, ref actualBlockCount);
          }
        }
        Check(blockCount == actualBlockCount, "blockCount == actualBlockCount");
        totalCount += blockCount;
      }
    }

    void Check(bool cond, string msg) {
      if (!cond) {
        Console.WriteLine(msg);
        Console.WriteLine("");
        Dump();
        Console.WriteLine("");
        Miscellanea.Assert(false);
      }
    }

    public void Dump() {
      Console.Write("  slots:");
      for (int i=0 ; i < slots.Length ; i++) {
        if (i != 0 & i % 256 == 0)
          Console.WriteLine();
        if (i != 0 & i % 128 == 0)
          Console.WriteLine();
        if (i % 16 == 0)
          Console.Write("\n   ");
        else if (i % 8 == 0)
          Console.Write("  ");
        uint slot = slots[i];
        uint payload = slot & PayloadMask;
        Console.Write("  {0}:{1,3}", slot >> 29, payload == 0x1FFFFFFFU ? "-" : payload.ToString());
      }
      Console.WriteLine();
      Console.WriteLine();
      Console.WriteLine(
        "  heads: 2 = {0}, 4 = {1}, 8 = {2}, 16 = {3}",
        head2 != EmptyMarker ? head2.ToString() : "-",
        head4 != EmptyMarker ? head4.ToString() : "-",
        head8 != EmptyMarker ? head8.ToString() : "-",
        head16 != EmptyMarker ? head16.ToString() : "-"
      );
    }

    ////////////////////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////////////////////

    public void Init() {
      slots = new uint[MinSize];
      for (uint i=0 ; i < MinSize ; i += 16) {
        slots[i]   = (i - 16) | AvailableTag << 29;
        slots[i+1] = (i + 16) | Block16Tag << 29;
      }
      slots[0] = EndLowerMarker;
      slots[MinSize-16+1] = End16UpperMarker;
      head2 = head4 = head8 = EmptyMarker;
      head16 = 0;
    }

    public uint Insert(uint handle, uint value, out bool inserted) {
      uint tag = handle >> 29;
      uint payload = handle & PayloadMask;
      Miscellanea.Assert(((tag << 29) | payload) == handle);

      switch (tag) {
        case 0:
          // The entry was single-valued, and inlined.
          // The payload contains the existing value.
          Miscellanea.Assert(handle == payload);
          Miscellanea.Assert(payload != value);
          return Insert2Block(handle, value, out inserted);

        // From now on the payload is the index of the block

        case 1: // 2-slots block
          return InsertWith2Block(payload, value, handle, out inserted);

        case 2: // 4-slot block
          return InsertWith4Block(payload, value, handle, out inserted);

        case 3: // 8-slot block
          return InsertWith8Block(payload, value, handle, out inserted);

        case 4: // 16-slot block
          return InsertWith16Block(payload, value, handle, out inserted);

        case 5: // Hashed block
          InsertIntoHashedBlock(payload, value, Hashcode(value), out inserted);
          return handle;

        default:
          Miscellanea.InternalFail("Invalid control flow");
          throw new NotImplementedException(); // Control flow cannot get here
      }
    }

    public uint Delete(uint handle, uint value, out bool deleted) {
      uint tag = handle >> 29;
      uint blockIdx = handle & PayloadMask;
      Miscellanea.Assert(((tag << 29) | blockIdx) == handle);

      switch (tag) {
        case 1: // 2-slots block
          return DeleteFrom2Block(blockIdx, value, handle, out deleted);

        case 2: // 4-slot block
          return DeleteFrom4Block(blockIdx, value, handle, out deleted);

        case 3: // 8-slot block
          return DeleteFrom8Block(blockIdx, value, handle, out deleted);

        case 4: // 16-slot block
          return DeleteFrom16Block(blockIdx, value, handle, out deleted);

        case 5: // Hashed block
          return DeleteFromHashedBlock(blockIdx, value, handle, Hashcode(value), out deleted);

        default:
          Miscellanea.InternalFail("Invalid control flow");
          throw new NotImplementedException(); // Control flow cannot get here
      }
    }

    public bool In(uint value, uint handle) {
      uint tag = handle >> 29;
      uint blockIdx = handle & PayloadMask;
      Miscellanea.Assert(((tag << 29) | blockIdx) == handle);

      switch (tag) {
        case 1: // 2-block slot
          return In2Block(value, blockIdx);

        case 2: // 4-block slot
          return InBlock(value, blockIdx, 4);

        case 3: // 8-block slot
          return InBlock(value, blockIdx, 8);

        case 4: // 16-slot block
          return InBlock(value, blockIdx, 16);

        case 5: // Hashed block
          return InHashedBlock(value, blockIdx, Hashcode(value));

        default:
          Miscellanea.InternalFail("Invalid control flow");
          throw new NotImplementedException(); // Control flow cannot get here
      }
    }

    public uint Count(uint handle) {
      uint tag = handle >> 29;
      uint blockIdx = handle & PayloadMask;
      Miscellanea.Assert(((tag << 29) | blockIdx) == handle);

      switch (tag) {
        case 0: // Inline
          return 1;

        case 1: // 2-block slot
          return 2;

        case 2: // 4-block slot
          return 2 + CountFrom(blockIdx + 2, 2);

        case 3: // 8-block slot
          return 4 + CountFrom(blockIdx + 4, 4);

        case 4: // 16-slot block
          return 7 + CountFrom(blockIdx + 7, 9);

        case 5: // Hashed block
          return slots[blockIdx];

        default:
          Miscellanea.InternalFail("Invalid control flow");
          throw new NotImplementedException(); // Control flow cannot get here
      }
    }

    public Iter GetIter(uint handle) {
      uint tag = handle >> 29;
      uint blockIdx = handle & PayloadMask;
      Miscellanea.Assert(((tag << 29) | blockIdx) == handle);

      switch (tag) {
        // case 0: // Inline
        //  return 1;

        case 1: // 2-block slot
          return new Iter(slots, blockIdx, 2);

        case 2: // 4-block slot
        case 3: // 8-block slot
        case 4: // 16-slot block
          return new Iter(slots, blockIdx, Count(handle));

        case 5: // Hashed block
          return HashedBlockIter(blockIdx);

        default:
          Miscellanea.InternalFail("Invalid control flow");
          throw new NotImplementedException(); // Control flow cannot get here
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    bool In2Block(uint value, uint blockIdx) {
      return value == slots[blockIdx] || value == slots[blockIdx+1];
    }

    bool InBlock(uint value, uint blockIdx, int blockSize) {
      for (int i=0 ; i < blockSize ; i++) {
        uint content = slots[blockIdx+i];
        if (content == value)
          return true;
        if (content == EmptyMarker)
          return false;
      }
      return false;
    }

    bool InHashedBlock(uint value, uint blockIdx, uint hashcode) {
      uint slotIdx = blockIdx + hashcode % 15 + 1;
      uint content = slots[slotIdx];
      if (content == value)
        return true;
      if (content == EmptyMarker)
        return false;
      uint tag = content >> 29;
      Miscellanea.Assert(tag <= 5);
      if (tag == 0)
        return false;
      else if (tag < 5)
        return In(value, content);
      else
        return InHashedBlock(value, content & PayloadMask, hashcode / 15);
    }

    ////////////////////////////////////////////////////////////////////////////

    uint CountFrom(uint offset, uint max) {
      for (uint i=0 ; i < max ; i++)
        if (slots[offset+i] == EmptyMarker)
          return i;
      return max;
    }

    ////////////////////////////////////////////////////////////////////////////

    Iter HashedBlockIter(uint blockIdx) {
      uint count = slots[blockIdx];
      uint[] values = new uint[count];
      int next = 0;
      CopyHashedBlock(blockIdx, values, ref next);
      Miscellanea.Assert(next == count);
      return new Iter(values, 0, count);
    }

    void CopyHashedBlock(uint blockIdx, uint[] dest, ref int next) {
      for (int i=1 ; i < 16 ; i++) {
        uint content = slots[blockIdx+i];
        if (content != EmptyMarker) {
          uint tag = content >> 29;
          if (tag == 0)
            dest[next++] = content;
          else
            Copy(content, dest, ref next);
        }
      }
    }

    void Copy(uint handle, uint[] dest, ref int next) {
      uint tag = handle >> 29;
      uint blockIdx = handle & PayloadMask;
      Miscellanea.Assert(((tag << 29) | blockIdx) == handle, "((tag << 29) | blockIdx) == handle");

      switch (tag) {
        // case 0: // Inline
        //   dest[next++] = handle;
        //   return;

        case 1: // 2-block slot
          dest[next++] = slots[blockIdx];
          dest[next++] = slots[blockIdx + 1];
          return;

        case 2: // 4-block slot
          CopyNonEmpty(blockIdx, 4, dest, ref next);
          return;

        case 3: // 8-block slot
          CopyNonEmpty(blockIdx, 8, dest, ref next);
          return;

        case 4: // 16-slot block
          CopyNonEmpty(blockIdx, 16, dest, ref next);
          return;

        case 5: // Hashed block
          CopyHashedBlock(blockIdx, dest, ref next);
          return;

        default:
          Miscellanea.InternalFail("Invalid control flow");
          throw new NotImplementedException(); // Control flow cannot get here
      }
    }

    void CopyNonEmpty(uint offset, uint max, uint[] dest, ref int next) {
      for (int i=0 ; i < max ; i++) {
        uint content = slots[offset + i];
        if (content == EmptyMarker)
          return;
        dest[next++] = content;
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    uint Insert2Block(uint value0, uint value1, out bool inserted) {
      // The newly inserted 2-block is not ordered
      // The returned handle is the address of the two-block,
      // tagged with the 2-values tag (= 1)

      // Checking first that the new value is not the same as the old one
      if (value0 == value1) {
        // When there's only a single value, the value and the handle are the same
        inserted = false;
        return value0;
      }

      uint blockIdx = Alloc2Block();
      slots[blockIdx]   = value0;
      slots[blockIdx+1] = value1;

      inserted = true;
      return blockIdx | (Block2Tag << 29);
    }

    uint DeleteFrom2Block(uint blockIdx, uint value, uint handle, out bool deleted) {
      uint value0 = slots[blockIdx];
      uint value1 = slots[blockIdx+1];

      if (value != value0 & value != value1) {
        deleted = false;
        return handle;
      }

      Release2Block(blockIdx);
      deleted = true;
      return value == value0 ? value1 : value0;
    }

    uint InsertWith2Block(uint block2Idx, uint value, uint handle, out bool inserted) {
      // Going from a 2-block to a 4-block
      // Values are not sorted
      // The last slot is set to 0xFFFFFFFFU
      // The return value is the address of the 4-block,
      // tagged with the 4-values tag (= 2)

      uint value0 = slots[block2Idx];
      uint value1 = slots[block2Idx+1];

      if (value == value0 | value == value1) {
        inserted = false;
        return handle;
      }

      Release2Block(block2Idx);

      uint block4Idx = Alloc4Block();
      slots[block4Idx]   = value0;
      slots[block4Idx+1] = value1;
      slots[block4Idx+2] = value;
      slots[block4Idx+3] = EmptyMarker;

      inserted = true;
      return block4Idx | (Block4Tag << 29);
    }

    uint DeleteFrom4Block(uint blockIdx, uint value, uint handle, out bool deleted) {
      uint value0 = slots[blockIdx];
      uint value1 = slots[blockIdx+1];
      uint value2 = slots[blockIdx+2];
      uint value3 = slots[blockIdx+3];

      if (value == value3) {
        deleted = true;
        slots[blockIdx+3] = EmptyMarker;
      }
      else if (value == value2) {
        deleted = true;
        if (value3 == EmptyMarker) {
          slots[blockIdx+2] = EmptyMarker;
        }
        else {
          slots[blockIdx+2] = value3;
          slots[blockIdx+3] = EmptyMarker;
        }
      }
      else if (value == value1) {
        deleted = true;
        if (value2 == EmptyMarker) {
          Release4Block(blockIdx);
          return value0;
        }
        else if (value3 == EmptyMarker) {
          slots[blockIdx+1] = value2;
          slots[blockIdx+2] = EmptyMarker;
        }
        else {
          slots[blockIdx+1] = value3;
          slots[blockIdx+3] = EmptyMarker;
        }
      }
      else if (value == value0) {
        deleted = true;
        if (value2 == EmptyMarker) {
          Release4Block(blockIdx);
          return value1;
        }
        else if (value3 == EmptyMarker) {
          slots[blockIdx]   = value2;
          slots[blockIdx+2] = EmptyMarker;
        }
        else {
          slots[blockIdx]   = value3;
          slots[blockIdx+3] = EmptyMarker;
        }
      }
      else
        deleted = false;

      return handle;
    }

    uint InsertWith4Block(uint block4Idx, uint value, uint handle, out bool inserted) {
      // The entry contains between two and four values already
      // The unused slots are at the end, and they are set to 0xFFFFFFFFU

      uint value0 = slots[block4Idx];
      uint value1 = slots[block4Idx+1];
      uint value2 = slots[block4Idx+2];
      uint value3 = slots[block4Idx+3];

      if (value == value0 | value == value1 | value == value2 | value == value3) {
        inserted = false;
        return handle;
      }

      inserted = true;
      if (value3 == EmptyMarker) {
        // Easy case: the last slot is available
        // We store the new value there, and return the same handle
        slots[block4Idx+3] = value;
        return handle;
      }
      else if (value2 == EmptyMarker) {
        // Another easy case: the last but one slot is available
        slots[block4Idx+2] = value;
        return handle;
      }
      else {
        // The block is already full, we need to allocate an 8-block now
        // We store the values in the first five slots, and set the rest
        // to 0xFFFFFFFFU. The return value is the index of the block,
        // tagged with the 8-value tag
        Release4Block(block4Idx);

        uint block8Idx = Alloc8Block();
        slots[block8Idx]   = value0;
        slots[block8Idx+1] = value1;
        slots[block8Idx+2] = value2;
        slots[block8Idx+3] = value3;
        slots[block8Idx+4] = value;
        slots[block8Idx+5] = EmptyMarker;
        slots[block8Idx+6] = EmptyMarker;
        slots[block8Idx+7] = EmptyMarker;

        return block8Idx | (Block8Tag << 29);
      }
    }

    //## BAD BAD: THE IMPLEMENTATION IS ALMOST THE SAME AS THAT OF DeleteFrom16Block()
    uint DeleteFrom8Block(uint blockIdx, uint value, uint handle, out bool deleted) {
      uint lastValue = EmptyMarker;
      int targetIdx = -1;

      int idx = 0;
      while (idx < 8) {
        uint valueI = slots[blockIdx + idx];
        if (valueI == value)
          targetIdx = idx;
        else if (valueI == EmptyMarker)
          break;
        else
          lastValue = valueI;
        idx++;
      }

      // <idx> is now the index of the first free block,
      // or <8> if the slot is full. It's also the number
      // of values in the block before the deletion
      Miscellanea.Assert(idx >= 4);

      deleted = targetIdx != -1;

      if (targetIdx == -1)
        return handle;

      if (targetIdx != idx)
        slots[blockIdx + targetIdx] = lastValue;
      slots[blockIdx + idx - 1] = EmptyMarker;

      if (idx == 4) {
        // We are down to 3 elements, so we release the upper half of the block
        Release8BlockUpperHalf(blockIdx);
        return blockIdx | (Block4Tag << 29);
      }

      return handle;
    }

    uint InsertWith8Block(uint block8Idx, uint value, uint handle, out bool inserted) {
      // The block contains between 4 and 8 values already
      // The unused ones are at the end, and they are set to 0xFFFFFFFFU

      uint value0 = slots[block8Idx];
      uint value1 = slots[block8Idx+1];
      uint value2 = slots[block8Idx+2];
      uint value3 = slots[block8Idx+3];
      uint value4 = slots[block8Idx+4];
      uint value5 = slots[block8Idx+5];
      uint value6 = slots[block8Idx+6];
      uint value7 = slots[block8Idx+7];

      bool isDuplicate = (value == value0 | value == value1 | value == value2 | value == value3) ||
                         (value == value4 | value == value5 | value == value6 | value == value7);
      inserted = !isDuplicate;

      if (isDuplicate)
        return handle;

      if (value4 == EmptyMarker) {
        slots[block8Idx+4] = value;
        return handle;
      }

      if (value5 == EmptyMarker) {
        slots[block8Idx+5] = value;
        return handle;
      }

      if (value6 == EmptyMarker) {
        slots[block8Idx+6] = value;
        return handle;
      }

      if (value7 == EmptyMarker) {
        slots[block8Idx+7] = value;
        return handle;
      }

      Release8Block(block8Idx);

      uint block16Idx = Alloc16Block();
      slots[block16Idx]   = value0;
      slots[block16Idx+1] = value1;
      slots[block16Idx+2] = value2;
      slots[block16Idx+3] = value3;
      slots[block16Idx+4] = value4;
      slots[block16Idx+5] = value5;
      slots[block16Idx+6] = value6;
      slots[block16Idx+7] = value7;
      slots[block16Idx+8] = value;
      for (int i=9 ; i < 16 ; i++)
        slots[block16Idx+i] = EmptyMarker;

      return block16Idx | (Block16Tag << 29);
    }

    //## BAD BAD: THE IMPLEMENTATION IS ALMOST THE SAME AS THAT OF DeleteFrom8Block()
    uint DeleteFrom16Block(uint blockIdx, uint value, uint handle, out bool deleted) {
      uint lastValue = EmptyMarker;
      int targetIdx = -1;

      int idx = 0;
      while (idx < 16) {
        uint valueI = slots[blockIdx + idx];
        if (valueI == value)
          targetIdx = idx;
        else if (valueI == EmptyMarker)
          break;
        else
          lastValue = valueI;
        idx++;
      }

      // <idx> is now the index of the first free block,
      // or <16> if the slot is full. It's also the number
      // of values in the block before the deletion
      Miscellanea.Assert(idx >= 7);

      deleted = targetIdx != -1;

      if (targetIdx == -1)
        return handle;

      if (targetIdx != idx)
        slots[blockIdx + targetIdx] = lastValue;
      slots[blockIdx + idx - 1] = EmptyMarker;

      if (idx == 7) {
        // We are down to 6 elements, so we release the upper half of the block
        Release16BlockUpperHalf(blockIdx);
        return blockIdx | (Block8Tag << 29);
      }

      return handle;
    }

    uint InsertWith16Block(uint blockIdx, uint value, uint handle, out bool inserted) {
      // a 16-slot standard block, which can contain between 7 and 16 entries
      uint value15 = slots[blockIdx+15];
      if (value15 == EmptyMarker) {
        // The slot still contains some empty space
        for (int i=0 ; i < 16 ; i++) {
          uint valueI = slots[blockIdx+i];
          if (value == valueI) {
            inserted = false;
            return handle;
          }
          if (valueI == EmptyMarker) {
            slots[blockIdx+i] = value;
            inserted = true;
            return handle;
          }
        }
        Miscellanea.Assert(false); //## CONTROL FLOW CAN NEVER MAKE IT HERE...
      }

      // The block is full, if the new value is not a duplicate
      // we need to turn this block into a hashed one
      for (int i=0 ; i < 16 ; i++)
        if (value == slots[blockIdx+i]) {
          inserted = false;
          return handle;
        }

      // Allocating and initializing the hashed block
      uint hashedBlockIdx = Alloc16Block();
      slots[hashedBlockIdx] = 0;
      for (int i=1 ; i < 16 ; i++)
        slots[hashedBlockIdx + i] = EmptyMarker;

      // Transferring the existing values
      for (int i=0 ; i < 16 ; i++) {
        uint content = slots[blockIdx+i];
        InsertIntoHashedBlock(hashedBlockIdx, content, Hashcode(content), out inserted);
        Miscellanea.Assert(inserted);
      }

      // Releasing the old block
      Release16Block(blockIdx);

      // Adding the new value
      InsertIntoHashedBlock(hashedBlockIdx, value, Hashcode(value), out inserted);
      Miscellanea.Assert(inserted);

      // Returning the tagged index of the block
      return hashedBlockIdx | (HashedBlockTag << 29);
    }

    uint DeleteFromHashedBlock(uint blockIdx, uint value, uint handle, uint hashcode, out bool deleted) {
      uint slotIdx = blockIdx + hashcode % 15 + 1;
      uint content = slots[slotIdx];
      if (content == EmptyMarker) {
        deleted = false;
        return handle;
      }
      uint tag = content >> 29;
      Miscellanea.Assert(tag <= 5);
      if (tag == 0) {
        if (content == value) {
          deleted = true;
          slots[slotIdx] = EmptyMarker;
        }
        else {
          deleted = false;
          return handle;
        }
      }
      else if (tag < 5) {
        uint newHandle = Delete(content, value, out deleted);
        slots[slotIdx] = newHandle;
      }
      else {
        uint nestedBlockIdx = content & PayloadMask;
        uint newHandle = DeleteFromHashedBlock(nestedBlockIdx, value, content, hashcode / 15, out deleted);
        slots[slotIdx] = newHandle;
      }

      if (deleted) {
        uint count = slots[blockIdx] - 1;
        Miscellanea.Assert(count >= 6);
        if (count == 6)
          return ShrinkHashedBlock(blockIdx);
        slots[blockIdx] = count;
      }

      return handle;
    }

    uint ShrinkHashedBlock(uint blockIdx) {
      uint slot1  = slots[blockIdx + 1];
      uint slot2  = slots[blockIdx + 2];
      uint slot3  = slots[blockIdx + 3];
      uint slot4  = slots[blockIdx + 4];
      uint slot5  = slots[blockIdx + 5];

      int nextIdx = CopyAndReleaseBlock(slot1, (int) blockIdx);
      nextIdx = CopyAndReleaseBlock(slot2, nextIdx);
      nextIdx = CopyAndReleaseBlock(slot3, nextIdx);
      nextIdx = CopyAndReleaseBlock(slot4, nextIdx);
      nextIdx = CopyAndReleaseBlock(slot5, nextIdx);

      uint endIdx = blockIdx + 6;
      for (int i=6 ; nextIdx < endIdx ; i++) {
        Miscellanea.Assert(i < 16);
        nextIdx = CopyAndReleaseBlock(slots[blockIdx + i], nextIdx);
      }

      slots[blockIdx + 6] = EmptyMarker;
      slots[blockIdx + 7] = EmptyMarker;

      Release16BlockUpperHalf(blockIdx);
      return blockIdx | (Block8Tag << 29);
    }

    int CopyAndReleaseBlock(uint handle, int nextIdx) {
      if (handle != EmptyMarker) {
        uint tag = handle >> 29;
        uint blockIdx = handle & PayloadMask;
        Miscellanea.Assert(((tag << 29) | blockIdx) == handle, "((tag << 29) | blockIdx) == handle");

        switch (tag) {
          case 0: // Inline
            slots[nextIdx++] = handle;
            break;

          case 1: // 2-block slot
            slots[nextIdx++] = slots[blockIdx];
            slots[nextIdx++] = slots[blockIdx + 1];
            Release2Block(blockIdx);
            break;

          case 2: // 4-block slot
            CopyNonEmpty(blockIdx, 4, slots, ref nextIdx);
            Release4Block(blockIdx);
            break;

          case 3: // 8-block slot
            CopyNonEmpty(blockIdx, 8, slots, ref nextIdx);
            Release8Block(blockIdx);
            break;

          // case 4: // 16-slot block
          //   CopyNonEmpty(blockIdx, 16, slots, ref nextIdx);
          //   break;

          // case 5: // Hashed block
          //   CopyHashedBlock(blockIdx, slots, ref nextIdx);
          //   break;

          default:
            Miscellanea.InternalFail("Invalid control flow");
            throw new NotImplementedException(); // Control flow cannot get here
        }
      }

      return nextIdx;
    }


    void InsertIntoHashedBlock(uint blockIdx, uint value, uint hashcode, out bool inserted) {
      uint slotIdx = blockIdx + hashcode % 15 + 1;
      uint content = slots[slotIdx];
      if (content == EmptyMarker) {
        slots[slotIdx] = value;
        slots[blockIdx]++;
        inserted = true;
      }
      else {
        uint tag = content >> 29;
        Miscellanea.Assert(tag <= 5);
        if (tag < 5) {
          uint newHandle = Insert(content, value, out inserted);
          slots[slotIdx] = newHandle;
        }
        else
          InsertIntoHashedBlock(content & PayloadMask, value, hashcode / 15, out inserted);
        if (inserted)
          slots[blockIdx]++;
      }
    }

    ////////////////////////////////////////////////////////////////////////////

    uint Hashcode(uint value) {
      return value;
    }

    ////////////////////////////////////////////////////////////////////////////

    uint Alloc2Block() {
      if (head2 != EmptyMarker) {
        Miscellanea.Assert(slots[head2] == EndLowerMarker);
        Miscellanea.Assert(slots[head2+1] == End2UpperMarker || slots[head2+1] >> 29 == Block2Tag);

        uint blockIdx = head2;
        RemoveBlockFromChain(blockIdx, EndLowerMarker, End2UpperMarker, ref head2);
        return blockIdx;
      }
      else {
        uint block4Idx = Alloc4Block();
        AddBlockToChain(block4Idx, Block2Tag, End2UpperMarker, ref head2);
        return block4Idx + 2;
      }
    }

    void Release2Block(uint blockIdx) {
      Miscellanea.Assert((blockIdx & 1) == 0);

      bool isFirst = (blockIdx & 3) == 0;
      uint otherBlockIdx = (uint) (blockIdx + (isFirst ? 2 : -2));
      uint otherBlockSlot0 = slots[otherBlockIdx];

      if (otherBlockSlot0 >> 29 == AvailableTag) {
        Miscellanea.Assert(slots[otherBlockIdx+1] >> 29 == Block2Tag);

        // The matching block is available, so we release both at once as a 4-slot block
        // But first we have to remove the matching block from the 2-slot block chain
        RemoveBlockFromChain(otherBlockIdx, otherBlockSlot0, End2UpperMarker, ref head2);
        Release4Block(isFirst ? blockIdx : otherBlockIdx);
      }
      else {
        // The matching block is not available, so we
        // just add the new one to the 2-slot block chain
        AddBlockToChain(blockIdx, Block2Tag, End2UpperMarker, ref head2);
      }
    }

    uint Alloc4Block() {
      if (head4 != EmptyMarker) {
        Miscellanea.Assert(slots[head4] == EndLowerMarker);
        Miscellanea.Assert(slots[head4+1] == End4UpperMarker | slots[head4+1] >> 29 == Block4Tag);

        uint blockIdx = head4;
        RemoveBlockFromChain(blockIdx, EndLowerMarker, End4UpperMarker, ref head4);
        return blockIdx;
      }
      else {
        uint block8Idx = Alloc8Block();
        AddBlockToChain(block8Idx, Block4Tag, End4UpperMarker, ref head4);
        return block8Idx + 4;
      }
    }

    void Release4Block(uint blockIdx) {
      Miscellanea.Assert((blockIdx & 3) == 0);

      bool isFirst = (blockIdx & 7) == 0;
      uint otherBlockIdx = (uint) (blockIdx + (isFirst ? 4 : -4));
      uint otherBlockSlot0 = slots[otherBlockIdx];
      uint otherBlockSlot1 = slots[otherBlockIdx+1];

      if (otherBlockSlot0 >> 29 == AvailableTag & otherBlockSlot1 >> 29 == Block4Tag) {
        RemoveBlockFromChain(otherBlockIdx, otherBlockSlot0, End4UpperMarker, ref head4);
        Release8Block(isFirst ? blockIdx : otherBlockIdx);
      }
      else
        AddBlockToChain(blockIdx, Block4Tag, End4UpperMarker, ref head4);
    }

    uint Alloc8Block() {
      if (head8 != EmptyMarker) {
        Miscellanea.Assert(slots[head8] == EndLowerMarker);
        Miscellanea.Assert(slots[head8+1] == End8UpperMarker | slots[head8+1] >> 29 == Block8Tag);

        uint blockIdx = head8;
        RemoveBlockFromChain(blockIdx, EndLowerMarker, End8UpperMarker, ref head8);
        return blockIdx;
      }
      else {
        uint block16Idx = Alloc16Block();
        Miscellanea.Assert(slots[block16Idx] == EndLowerMarker);
        Miscellanea.Assert(slots[block16Idx+1] == End16UpperMarker | slots[block16Idx+1] >> 29 == Block16Tag);
        AddBlockToChain(block16Idx, Block8Tag, End8UpperMarker, ref head8);
        return block16Idx + 8;
      }
    }

    void Release8Block(uint blockIdx) {
      Miscellanea.Assert((blockIdx & 7) == 0);

      bool isFirst = (blockIdx & 15) == 0;
      uint otherBlockIdx = (uint) (blockIdx + (isFirst ? 8 : -8));
      uint otherBlockSlot0 = slots[otherBlockIdx];
      uint otherBlockSlot1 = slots[otherBlockIdx+1];

      if (otherBlockSlot0 >> 29 == AvailableTag & otherBlockSlot1 >> 29 == Block8Tag) {
        RemoveBlockFromChain(otherBlockIdx, otherBlockSlot0, End8UpperMarker, ref head8);
        Release16Block(isFirst ? blockIdx : otherBlockIdx);
      }
      else
        AddBlockToChain(blockIdx, Block8Tag, End8UpperMarker, ref head8);
    }

    void Release8BlockUpperHalf(uint blockIdx) {
      AddBlockToChain(blockIdx+4, Block4Tag, End4UpperMarker, ref head4);
    }

    uint Alloc16Block() {
      if (head16 == EmptyMarker) {
        uint len = (uint) slots.Length;
        uint[] newSlots = new uint[2*len];
        Array.Copy(slots, newSlots, len);
        for (uint i=len ; i < 2 * len ; i += 16) {
          newSlots[i]   = (i - 16) | AvailableTag << 29;
          newSlots[i+1] = (i + 16) | Block16Tag << 29;
        }
        newSlots[len] = EndLowerMarker;
        newSlots[2*len - 16 + 1] = End16UpperMarker;
        slots = newSlots;
        head16 = len;
      }

      Miscellanea.Assert(slots[head16] == EndLowerMarker);
      Miscellanea.Assert(slots[head16+1] == End16UpperMarker | slots[head16+1] >> 29 == Block16Tag);

      uint blockIdx = head16;
      RemoveBlockFromChain(blockIdx, EndLowerMarker, End16UpperMarker, ref head16);
      return blockIdx;
    }

    void Release16Block(uint blockIdx) {
      AddBlockToChain(blockIdx, Block16Tag, End16UpperMarker, ref head16);
    }

    void Release16BlockUpperHalf(uint blockIdx) {
      AddBlockToChain(blockIdx+8, Block8Tag, End8UpperMarker, ref head8);
    }

    ////////////////////////////////////////////////////////////////////////////

    void RemoveBlockFromChain(uint blockIdx, uint slot0, uint endUpperMarker, ref uint head) {
      uint slot1 = slots[blockIdx + 1];

      if (slot0 != EndLowerMarker) {
        // Not the first block in the chain
        Miscellanea.Assert(head != blockIdx);
        uint prevBlockIdx = slot0 & PayloadMask;

        if (slot1 != endUpperMarker) {
          // The block is in the middle of the chain
          // The previous and next blocks must be repointed to each other
          uint nextBlockIdx = slot1 & PayloadMask;
          slots[prevBlockIdx+1] = slot1;
          slots[nextBlockIdx]   = slot0;
        }
        else {
          // Last block in a chain with multiple blocks
          // The 'next' field of the previous block must be cleared
          slots[prevBlockIdx+1] = endUpperMarker;
        }
      }
      else {
        // First slot in the chain, must be the one pointed to by head
        Miscellanea.Assert(head == blockIdx);

        if (slot1 != endUpperMarker) {
          // The head must be repointed at the next block,
          // whose 'previous' field must now be cleared
          uint nextBlockIdx = slot1 & PayloadMask;
          head = nextBlockIdx;
          slots[nextBlockIdx] = EndLowerMarker;
        }
        else {
          // No 'previous' nor 'next' slots, it must be the only one
          // Just resetting the head of the 2-slot block chain
          head = EmptyMarker;
        }
      }
    }

    void AddBlockToChain(uint blockIdx, uint sizeTag, uint endUpperMarker, ref uint head) {
      // The 'previous' field of the newly released block must be cleared
      slots[blockIdx] = EndLowerMarker;
      if (head != EmptyMarker) {
        // If the list of blocks is not empty, we link the first two blocks
        slots[blockIdx+1] = head | sizeTag << 29;
        slots[head] = blockIdx | AvailableTag << 29;
      }
      else {
        // Otherwise we just clear then 'next' field of the newly released block
        slots[blockIdx+1] = endUpperMarker;
      }
      // The new block becomes the head one
      head = blockIdx;
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////////////

  struct OneWayBinTable {
    const int MinCapacity = 16;

    static uint[] emptyArray = new uint[0];

    public uint[] column;
    public OverflowTable overflowTable;
    public int count;

    public void Check() {
      overflowTable.Check(column, count);
    }

    public void Dump() {
      Console.WriteLine("count = " + count.ToString());
      Console.Write("column = [");
      for (int i=0 ; i < column.Length ; i++)
        Console.Write("{0}{1:X}", (i > 0 ? " " : ""), column[i]);
      Console.WriteLine("]");
      overflowTable.Dump();
    }

    public void Init() {
      column = emptyArray;
      overflowTable.Init();
      count = 0;
    }

    public void InitReverse(ref OneWayBinTable source) {
      Miscellanea.Assert(count == 0);

      uint[] srcCol = source.column;
      int len = srcCol.Length;

      for (uint i=0 ; i < len ; i++) {
        uint code = srcCol[i];
        if (code != OverflowTable.EmptyMarker)
          if (code >> 29 == 0) {
            Insert(code, i);
          }
          else {
            OverflowTable.Iter it = source.overflowTable.GetIter(code);
            while (!it.Done()) {
              Insert(it.Get(), i);
              it.Next();
            }
          }
      }
    }

    public bool Contains(uint surr1, uint surr2) {
      if (surr1 >= column.Length)
        return false;
      uint code = column[surr1];
      if (code == OverflowTable.EmptyMarker)
        return false;
      if (code >> 29 == 0)
        return code == surr2;
      return overflowTable.In(surr2, code);
    }

    public bool ContainsKey(uint surr1) {
      return surr1 < column.Length && column[surr1] != OverflowTable.EmptyMarker;
    }

    public uint[] Lookup(uint surr) {
      if (surr >= column.Length)
        return emptyArray;
      uint code = column[surr];
      if (code == OverflowTable.EmptyMarker)
        return emptyArray;
      if (code >> 29 == 0)
        return new uint[] {code};

      uint count = overflowTable.Count(code);
      OverflowTable.Iter it = overflowTable.GetIter(code);
      uint[] surrs = new uint[count];
      int next = 0;
      while (!it.Done()) {
        surrs[next++] = it.Get();
        it.Next();
      }
      Miscellanea.Assert(next == count);
      return surrs;
    }

    public void Insert(uint surr1, uint surr2) {
      int size = column.Length;
      if (surr1 >= size) {
        int newSize = size == 0 ? MinCapacity : 2 * size;
        while (surr1 >= newSize)
          newSize *= 2;
        uint[] newColumn = new uint[newSize];
        Array.Copy(column, newColumn, size);
        for (int i=size ; i < newSize ; i++)
          newColumn[i] = OverflowTable.EmptyMarker;
        column = newColumn;
      }

      uint code = column[surr1];
      if (code == OverflowTable.EmptyMarker) {
        column[surr1] = surr2;
        count++;
      }
      else {
        bool inserted;
        column[surr1] = overflowTable.Insert(code, surr2, out inserted);
        if (inserted)
          count++;
      }
    }

    public void Delete(uint surr1, uint surr2) {
      uint code = column[surr1];
      if (code == OverflowTable.EmptyMarker)
        return;
      if (code == surr2) {
        column[surr1] = OverflowTable.EmptyMarker;
        count--;
      }
      else if (code >> 29 != 0) {
        bool deleted;
        column[surr1] = overflowTable.Delete(code, surr2, out deleted);
        if (deleted)
          count--;
      }
    }

    public uint[,] Copy() {
      uint[,] res = new uint[count, 2];
      int next = 0;
      for (uint i=0 ; i < column.Length ; i++) {
        uint code = column[i];
        if (code != OverflowTable.EmptyMarker) {
          if (code >> 29 == 0) {
            res[next, 0] = i;
            res[next++, 1] = code;
          }
          else {
            OverflowTable.Iter it = overflowTable.GetIter(code);
            while (!it.Done()) {
              res[next, 0] = i;
              res[next++, 1] = it.Get();
              it.Next();
            }
          }
        }
      }
      Miscellanea.Assert(next == count);
      return res;
    }
  }
}
