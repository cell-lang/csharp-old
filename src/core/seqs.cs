using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;


namespace CellLang {
  abstract class SeqObj : Obj {
    internal Obj[] items;
    internal int length;
    int minPrintedSize = -1;

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

    override public Obj UpdatedAt(long idx, Obj obj) {
      if (idx < 0 | idx >= length)
        Miscellanea.SoftFail("Invalid sequence index");

      int offset = Offset();
      Obj[] newItems = new Obj[length];
      for (int i=0 ; i < length ; i++)
        newItems[i] = i == idx ? obj : items[offset + i];

      return new MasterSeqObj(newItems);
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
        Obj seq = items[i+offset];
        seq.CopyItems(newItems, targetOffset);
        targetOffset += seq.GetSize();
      }
      Miscellanea.Assert(targetOffset == newLen);
      return new MasterSeqObj(newItems, newLen);
    }

    override public void CopyItems(Obj[] array, int offset) {
      Array.Copy(items, Offset(), array, offset, length);
    }

    protected Obj CopyOnWriteConcat(Obj seq) {
      int offset = Offset();
      int seqLen = seq.GetSize();
      int minLen = length + seqLen;
      Obj[] newItems = new Obj[Math.Max(4 * minLen, 32)];
      Array.Copy(items, offset, newItems, 0, length);
      SeqObj seqObj = (SeqObj) seq;
      Array.Copy(seqObj.items, seqObj.Offset(), newItems, length, seqObj.length);
      return new MasterSeqObj(newItems, minLen);
    }

    override public uint Hashcode() {
      int offset = Offset();
      uint hashcodesSum = 0;
      for (int i=0 ; i < length ; i++)
        hashcodesSum += items[offset+i].Hashcode();
      return hashcodesSum ^ (uint) length;
    }

    override public void Print(TextWriter writer, int maxLineLen, bool newLine, int indentLevel) {
      int offset = Offset();
      bool breakLine = MinPrintedSize() > maxLineLen;

      writer.Write('(');

      if (breakLine) {
        // If we are on a fresh line, we start writing the first element
        // after the opening bracket, with just a space in between
        // Otherwise we start on the next line
        if (newLine)
          writer.Write(' ');
        else
          writer.WriteIndentedNewLine(indentLevel + 1);
      }

      for (int i=0 ; i < length ; i++) {
        if (i > 0) {
          writer.Write(',');
          if (breakLine)
            writer.WriteIndentedNewLine(indentLevel + 1);
          else
            writer.Write(' ');
        }
        items[offset+i].Print(writer, maxLineLen, breakLine & !newLine, indentLevel + 1);
      }

      if (breakLine)
        writer.WriteIndentedNewLine(indentLevel);

      writer.Write(')');
    }

    override public int MinPrintedSize() {
      if (minPrintedSize == -1) {
        int offset = Offset();
        minPrintedSize = 2 * length;
        for (int i=0 ; i < length ; i++)
          minPrintedSize += items[offset+i].MinPrintedSize();
      }
      return minPrintedSize;
    }

    override public ValueBase GetValue() {
      ValueBase[] values = new ValueBase[length];
      int offset = Offset();
      for (int i=0 ; i < length ; i++)
        values[i] = items[offset+i].GetValue();
      return new SeqValue(values);
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
      // return new RopeObj(this, seq);
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
      // return new RopeObj(this, seq);
    }

    override protected int Offset() {
      return offset;
    }
  }

  //////////////////////////////////////////////////////////////////////////////
  //////////////////////////////////////////////////////////////////////////////

//  class RopeObj : Obj {
//    Obj left;
//    Obj right;
//    int length;
//    Obj[] array;
//
//    internal RopeObj(Obj left, Obj right) {
//      this.left = left;
//      this.right = right;
//      length = left.GetSize() + right.GetSize();
//    }
//
//    void BuildArray() {
//      if (array == null) {
//        array = new Obj[length];
//        left.CopyItems(array, 0);
//        right.CopyItems(array, left.GetSize());
//        left = right = null;
//      }
//    }
//
//    override public bool IsSeq() {
//      return true;
//    }
//
//    override public bool IsEmptySeq() {
//      return length == 0;
//    }
//
//    override public bool IsNeSeq() {
//      return length != 0;
//    }
//
//    override public int GetSize() {
//      return length;
//    }
//
//    override public Obj GetItem(long idx) {
//      BuildArray();
//      return array[idx];
//    }
//
//    override public Obj Reverse() {
//      BuildArray();
//      Obj[] revArray = new Obj[length];
//      for (int i=0 ; i < length ; i++)
//        revArray[i] = array[length-i-1];
//      return new MasterSeqObj(revArray);
//    }
//
//    override public long[] GetLongArray() {
//      BuildArray();
//      long[] longs = new long[length];
//      for (int i=0 ; i < length ; i++)
//        longs[i] = array[i].GetLong();
//      return longs;
//    }
//
//    override public byte[] GetByteArray() {
//      BuildArray();
//      byte[] bytes = new byte[length];
//      for (int i=0 ; i < length ; i++) {
//        long val = array[i].GetLong();
//        if (val < 0 | val > 255)
//          throw new NotImplementedException();
//        bytes[i] = (byte) val;
//      }
//      return bytes;
//    }
//
//    override public string ToString() {
//      BuildArray();
//      string[] reprs = new string[length];
//      for (int i=0 ; i < length ; i++)
//        reprs[i] = array[i].ToString();
//      return "(" + string.Join(", ", reprs) + ")";
//    }
//
////    override public Obj ConcatMany() {
////    }
//
//    override public uint Hashcode() {
//      BuildArray();
//      uint hashcodesSum = 0;
//      for (int i=0 ; i < length ; i++)
//        hashcodesSum += array[i].Hashcode();
//      return hashcodesSum ^ (uint) length;
//    }
//
//    override protected int TypeId() {
//      return 3;
//    }
//
//    override protected int InternalCmp(Obj other) {
//      BuildArray();
//      return other.CmpSeq(array, 0, length);
//
//    }
//
//    override public int CmpSeq(Obj[] other_items, int other_offset, int other_length) {
//      BuildArray();
//      if (other_length != length)
//        return other_length < length ? 1 : -1;
//      for (int i=0 ; i < length ; i++) {
//        int res = other_items[other_offset+i].Cmp(array[i]);
//        if (res != 0)
//          return res;
//      }
//      return 0;
//    }
//
//    override public SeqOrSetIter GetSeqOrSetIter() {
//      BuildArray();
//      return new SeqOrSetIter(array, 0, length-1);
//    }
//
////    override public void InitAt(long idx, Obj value) {
////    }
//
//    override public Obj GetSlice(long first, long len) {
//      if (array == null) {
//        long upperBound = first + len;
//        if (upperBound > length)
//          throw new Exception(); //## FIND BETTER EXCEPTION
//        int leftLength = left.GetSize();
//        if (upperBound <= leftLength)
//          return left.GetSlice(first, len);
//        if (first >= leftLength)
//          return right.GetSlice(first-leftLength, len);
//        Obj leftSlice = left.GetSlice(first, leftLength-first);
//        Obj rightSlice = right.GetSlice(0, len-leftLength);
//        return new RopeObj(leftSlice, rightSlice);
//      }
//      else {
//        //## BAD BAD BAD
//        MasterSeqObj master = new MasterSeqObj(array);
//        return master.GetSlice(first, len);
//      }
//    }
//
//    override public Obj Append(Obj obj) {
//      if (array == null) {
//        Obj newRight = right.Append(obj);
//        return new RopeObj(left, newRight);
//      }
//      else {
//        Obj[] newArray = new Obj[16];
//        newArray[0] = obj;
//        Obj newSeq = new MasterSeqObj(newArray, 1);
//        return new RopeObj(this, newSeq);
//      }
//    }
//
//    override public Obj Concat(Obj seq) {
//      return new RopeObj(this, seq);
//    }
//
//    override public void CopyItems(Obj[] destArray, int offset) {
//      if (array == null) {
//        left.CopyItems(destArray, offset);
//        right.CopyItems(destArray, offset+left.GetSize());
//      }
//      else
//        Array.Copy(array, 0, destArray, offset, length);
//    }
//  }
}