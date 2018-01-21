using System;
using System.Collections.Generic;


namespace CellLang {
  struct OverflowTable {
    const int MinSize = 32;

    uint[] slots;
    uint head2;
    uint head4;
    uint head8;
    uint head16;

    public void Init() {
      slots = new uint[MinSize];
      uint idxLast = MinSize - 16;
      for (uint i=0 ; i < idxLast ; i += 16)
        slots[i] = i + 16;
      slots[idxLast] = 0xFFFFFFFFU;
      head2 = head4 = head8 = 0xFFFFFFFFU;
      head16 = 0;
    }

    public uint Insert(uint handle, uint value) {
      uint tag = handle >> 29;
      uint payload = handle & 0x1FFFFFFFU;

      switch (tag) {
        case 0:
          // The entry was single-valued, and inlined.
          // The payload contains the existing value.
          Miscellanea.Assert(handle == payload);
          return Insert2Block(handle, value);

        // From now on the payload is the index of the block

        case 1: // 2-slots block
          return InsertWith2Block(payload, value, handle);

        case 2: // 4-slot block
          return InsertWith4Block(payload, value, handle);

        case 3: // 8-slot block
          return InsertWith8Block(payload, value, handle);

        case 4: // Non-hashed 16-slot block
          return InsertWithNonHashed16Block(payload, value, handle);

        case 5:
          InsertIntoHashedBlock(payload, value, Hashcode(value));
          return handle;

        default:
          Miscellanea.Assert(false);
          throw new NotImplementedException();
      }
    }

    public uint Delete(uint handle, uint value) {
      throw new NotImplementedException();
    }

    ////////////////////////////////////////////////////////////////////////////

    uint Insert2Block(uint value0, uint value1) {
      // The newly inserted 2-block is not ordered
      // The returned handle is the address of the two-block,
      // tagged with the 2-values tag (= 1)

      // Checking first that the new value is not the same as the old one
      if (value0 == value1)
        // When there's only a single value, the value and the handle are the same
        return value0;

      uint blockIdx = Alloc2Block();
      slots[blockIdx]   = value0;
      slots[blockIdx+1] = value1;

      return blockIdx | 0x10000000U;
    }

    uint InsertWith2Block(uint block2Idx, uint value, uint handle) {
      // Going from a 2-block to a 4-block
      // Values are not sorted
      // The last slot is set to 0xFFFFFFFFU
      // The return value is the address of the 4-block,
      // tagged with the 4-values tag (= 2)

      uint value0 = slots[block2Idx];
      uint value1 = slots[block2Idx+1];

      if (value == value0 | value == value1)
        return handle;

      Release2Block(block2Idx);

      uint block4Idx = Alloc4Block();
      slots[block4Idx]   = value0;
      slots[block4Idx+1] = value1;
      slots[block4Idx+2] = value;
      slots[block4Idx+3] = 0xFFFFFFFFU;

      return block4Idx | 0x20000000U;
    }

    uint InsertWith4Block(uint block4Idx, uint value, uint handle) {
      // The entry contains either three or four values already
      // If it's three, the last one is set to 0xFFFFFFFFU
      // The payload contains the index of the 4-block

      uint value0 = slots[block4Idx];
      uint value1 = slots[block4Idx+1];
      uint value2 = slots[block4Idx+2];
      uint value3 = slots[block4Idx+3];

      if (value == value0 | value == value1 | value == value2 | value == value3)
        return handle;

      if (value3 == 0xFFFFFFFFU) {
        // Easy case: the last slot is available
        // We store the new value there, and return the same handle
        slots[block4Idx+3] = value;
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
        slots[block8Idx+5] = 0xFFFFFFFFU;
        slots[block8Idx+6] = 0xFFFFFFFFU;
        slots[block8Idx+7] = 0xFFFFFFFFU;

        return block8Idx | 0x30000000U;
      }
    }

    uint InsertWith8Block(uint block8Idx, uint value, uint handle) {
      // The block contains between 5 and 8 values already
      // The unused ones are at the end, and they are set to 0xFFFFFFFFU
      // and the payload contains the index of the block

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
      if (isDuplicate)
        return handle;

      if (value5 == 0xFFFFFFFFU) {
        slots[block8Idx+5] = value;
        return handle;
      }

      if (value6 == 0xFFFFFFFFU) {
        slots[block8Idx+6] = value;
        return handle;
      }

      if (value7 == 0xFFFFFFFFU) {
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
        slots[block16Idx+i] = 0xFFFFFFFFU;

      return block16Idx | 0x40000000U;
    }

    uint InsertWithNonHashed16Block(uint blockIdx, uint value, uint handle) {
      // a 16-slot standard block, which can contain between 9 and 16 entries
      uint value15 = slots[blockIdx+15];
      if (value15 == 0xFFFFFFFFU) {
        // The slot still contains some empty space
        for (int i=0 ; i < 16 ; i++) {
          uint valueI = slots[blockIdx+i];
          if (value == valueI)
            return handle;
          if (valueI == 0xFFFFFFFFU) {
            slots[blockIdx+i] = value;
            return handle;
          }
        }
        Miscellanea.Assert(false); //## CONTROL FLOW CAN NEVER MAKE IT HERE...
      }

      // The block is full, if the new value is not a duplicate
      // we need to turn this block into a hashed one
      if (value == value15)
        return handle;
      for (int i=0 ; i < 15 ; i++)
        if (value == slots[blockIdx+i])
          return handle;

      // Allocating and initializing the hashed block
      uint hashedBlockIdx = Alloc16Block();
      for (int i=0 ; i < 16 ; i++)
        slots[hashedBlockIdx] = 0xFFFFFFFFU;

      // Transferring the existing values
      for (int i=0 ; i < 16 ; i++) {
        uint content = slots[blockIdx+i];
        InsertIntoHashedBlock(hashedBlockIdx, content, Hashcode(content));
      }

      // Releasing the old block
      Release16Block(blockIdx);

      // Adding the new value
      InsertIntoHashedBlock(hashedBlockIdx, value, Hashcode(value));

      // Returning the tagged index of the block
      return hashedBlockIdx | 0x50000000U;
    }

    void InsertIntoHashedBlock(uint blockIdx, uint value, uint hashcode) {
      uint slotIdx = blockIdx + hashcode % 16;
      uint content = slots[slotIdx];
      if (content == 0xFFFFFFFFU)
        slots[slotIdx] = value;
      uint tag = content >> 29;
      Miscellanea.Assert(tag <= 5);
      if (tag < 5)
        slots[slotIdx] = Insert(content, value);
      else
        InsertIntoHashedBlock(content & 0x1FFFFFFFU, value, hashcode / 16);
    }

    ////////////////////////////////////////////////////////////////////////////

    uint Hashcode(uint value) {
      return value;
    }

    ////////////////////////////////////////////////////////////////////////////

    uint Alloc2Block() {
      throw new NotImplementedException();
    }

    void Release2Block(uint blockIdx) {
      throw new NotImplementedException();
    }

    uint Alloc4Block() {
      throw new NotImplementedException();
    }

    void Release4Block(uint blockIdx) {
      throw new NotImplementedException();
    }

    uint Alloc8Block() {
      throw new NotImplementedException();
    }

    void Release8Block(uint blockIdx) {
      throw new NotImplementedException();
    }

    uint Alloc16Block() {
      throw new NotImplementedException();
    }

    void Release16Block(uint blockIdx) {
      throw new NotImplementedException();
    }
  }
}
