using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  abstract class SeqObj : Obj {
    internal Obj[] items;
    internal int length;

    protected SeqObj(int length) {
      this.items = new Obj[length];
      this.length = length;
    }

    protected SeqObj(Obj[] items, int length) {
      Miscellanea.Assert(items != null && length >= 0 && length <= items.Length);
      this.items = items;
      this.length = length;
    }

    override public bool IsSeq() {
      return true;
    }

    override public bool IsEmptySeq() {
      return length == 0;
    }

    override public bool IsNeSeq() {
      return length != 0;
    }

    override public int GetSize() {
      return length;
    }

    override public Obj GetItem(long idx) {
      if (idx < length)
        return items[Offset()+idx];
      else
        throw new Exception();
    }

    override public Obj Reverse() {
      int offset = Offset();
      int last = offset + length - 1;
      Obj[] revItems = new Obj[length];
      for (int i=0 ; i < length ; i++)
        revItems[i] = items[last-i];
      return new MasterSeqObj(revItems);
    }

    override public long[] GetLongArray() {
      long[] longs = new long[length];
      int offset = Offset();
      for (int i=0 ; i < length ; i++)
        longs[i] = items[offset+i].GetLong();
      return longs;
    }

    override public byte[] GetByteArray() {
      byte[] bytes = new byte[length];
      int offset = Offset();
      for (int i=0 ; i < length ; i++) {
        long val = items[offset+i].GetLong();
        if (val < 0 | val > 255)
          throw new NotImplementedException();
        bytes[i] = (byte) val;
      }
      return bytes;
    }

    override public string ToString() {
      int offset = Offset();
      string[] reprs = new string[length];
      for (int i=0 ; i < length ; i++)
        reprs[i] = items[offset+i].ToString();
      return "(" + string.Join(", ", reprs) + ")";
    }

    override public Obj ConcatMany() {
      int offset = Offset();
      int newLen = 0;
      for (int i=0 ; i < length ; i++)
        newLen += items[i+offset].GetSize();
      Obj[] newItems = new Obj[newLen];
      int targetOffset = 0;
      for (int i=0 ; i < length ; i++) {
        SeqObj seq = (SeqObj) items[i+offset];
        Array.Copy(seq.items, seq.Offset(), newItems, targetOffset, seq.length);
        targetOffset += seq.length;
      }
      Miscellanea.Assert(targetOffset == newLen);
      return new MasterSeqObj(newItems, newLen);
    }

    protected Obj CopyOnWriteConcat(Obj seq) {
      int offset = Offset();
      int seqLen = seq.GetSize();
      int minLen = length + seqLen;
      Obj[] newItems = new Obj[Math.Max(4 * minLen, 32)];
      Array.Copy(items, offset, newItems, 0, length);
      SeqObj seqObj = (SeqObj) seq;
      Array.Copy(seqObj.items, seqObj.Offset(), newItems, length, seqObj.length);
//      for (int i=0 ; i < length ; i++)
//        newItems[i] = items[i+offset];
//      for (int i=0 ; i < seqLen ; i++)
//        newItems[i+length] = seq.GetItem(i);
      return new MasterSeqObj(newItems, minLen);

      // Obj[] newItems = new Obj[newLen <= 16 ? 32 : (3 * newLen) / 2];
      // for (int i=0 ; i < length ; i++)
      //   newItems[i] = items[i];
      // for (int i=0 ; i < seqLen ; i++)
      //   newItems[length+i] = seq.GetItem(i);
      // return new MasterSeqObj(newItems, newLen);

      // newLen = length + seqLen;
      // Obj[] newItems = new Obj[newLen <= 16 ? 32 : (3 * newLen) / 2];
      // for (int i=0 ; i < length ; i++)
      //   newItems[i] = items[offset+i];
      // for (int i=0 ; i < seqLen ; i++)
      //   newItems[length+i] = seq.GetItem(i);
      // return new MasterSeqObj(newItems, newLen);
    }

    override protected int TypeId() {
      return 3;
    }

    override protected int InternalCmp(Obj other) {
      return other.CmpSeq(items, Offset(), length);
    }

    override public int CmpSeq(Obj[] other_items, int other_offset, int other_length) {
      int offset = Offset();
      if (other_length != length)
        return other_length < length ? 1 : -1;
      for (int i=0 ; i < length ; i++) {
        int res = other_items[other_offset+i].Cmp(items[offset+i]);
        if (res != 0)
          return res;
      }
      return 0;
    }

    protected abstract int Offset();

    static Obj emptySeq = new MasterSeqObj(new Obj[] {});

    public static Obj Empty() {
      return emptySeq;
    }
  }


  class MasterSeqObj : SeqObj {
    internal int used;

    public MasterSeqObj(Obj[] items, int length) : base(items, length) {
      for (int i=0 ; i < length ; i++)
        Miscellanea.Assert(items[i] != null);
      this.used = length;
    }

    public MasterSeqObj(Obj[] items) : this(items, items.Length) {

    }

    public MasterSeqObj(long length) : base(length > 0 ? (int) length : 0) {

    }

    override public Obj GetItem(long idx) {
      if (idx < length)
        return items[idx];
      else
        throw new Exception();
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return new SeqOrSetIter(items, 0, length-1);
    }

    override public void InitAt(long idx, Obj value) {
      Miscellanea.Assert(idx >= 0 & idx < length);
      Miscellanea.Assert(items[idx] == null);
      items[idx] = value;
    }

    override public Obj GetSlice(long first, long len) {
      if (first + len > length)
        throw new Exception(); //## FIND BETTER EXCEPTION
      return new SliceObj(this, (int) first, (int) len);
    }

    override public Obj Append(Obj obj) {
      if (used == length && length + 1 < items.Length) {
        items[length] = obj;
        return new SliceObj(this, 0, length+1);
      }
      else {
        Obj[] newItems = new Obj[length < 16 ? 32 : (3 * length) / 2];
        for (int i=0 ; i < length ; i++)
          newItems[i] = items[i];
        newItems[length] = obj;
        return new MasterSeqObj(newItems, length+1);
      }
    }

    override public Obj Concat(Obj seq) {
      Miscellanea.Assert(seq != null);

      int seqLen = seq.GetSize();
      int newLen = length + seqLen;

      if (used == length && newLen < items.Length) {
//        SeqObj seqObj = (SeqObj) seq;
//        Array.Copy(seqObj.items, seqObj.Offset(), items, length, seqObj.length);
        for (int i=0; i < seqLen ; i++)
          items[length+i] = seq.GetItem(i);
        return new SliceObj(this, 0, newLen);
      }

      return CopyOnWriteConcat(seq);
    }

    override protected int Offset() {
      return 0;
    }
  }


  class SliceObj : SeqObj {
    MasterSeqObj master;
    int offset;

    public SliceObj(MasterSeqObj master, int offset, int length) : base(master.items, length) {
      for (int i=0 ; i < offset+length ; i++)
        Miscellanea.Assert(master.items[i] != null);
      this.master = master;
      this.offset = offset;
    }

    override public SeqOrSetIter GetSeqOrSetIter() {
      return new SeqOrSetIter(items, offset, offset+length-1);
    }

    override public Obj GetSlice(long first, long len) {
      if (first + len > length)
        throw new Exception(); //## FIND BETTER EXCEPTION
      return new SliceObj(master, offset + (int) first, (int) len);
    }

    override public Obj Append(Obj obj) {
      int used = offset + length;
      if (master.used == used && used + 1 < master.items.Length) {
        master.items[used] = obj;
        return new SliceObj(master, offset, length+1);
      }
      else {
        Obj[] newItems = new Obj[length < 16 ? 32 : (3 * length) / 2];
        for (int i=0 ; i < length ; i++)
          newItems[i] = items[i];
        newItems[length] = obj;
        return new MasterSeqObj(newItems, length+1);

      }
    }

    override public Obj Concat(Obj seq) {
      int seqLen = seq.GetSize();
      int used = offset + length;
      int newLen = used + seqLen;

      if (master.used == used && newLen <= master.items.Length) {
        for (int i=0 ; i < seqLen ; i++)
          master.items[used+i] = seq.GetItem(i);
        return new SliceObj(master, offset, newLen);
      }

      return CopyOnWriteConcat(seq);
    }

    override protected int Offset() {
      return offset;
    }
  }
}