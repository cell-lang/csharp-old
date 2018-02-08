using System;
using System.Diagnostics;


namespace CellLang {
  public class SeqOrSetIter {
    Obj[] objs;
    int next;
    int last;

    public SeqOrSetIter(Obj[] objs, int next, int last) {
      this.objs = objs;
      this.next = next;
      this.last = last;
    }

    public Obj Get() {
      Miscellanea.Assert(next <= last);
      return objs[next];
    }

    public void Next() {
      Miscellanea.Assert(next <= last);
      next++;
    }

    public bool Done() {
      return next > last;
    }
  }


  public class BinRelIter {
    Obj[] col1;
    Obj[] col2;
    int[] idxs;
    int next;
    int last;

    public BinRelIter(Obj[] col1, Obj[] col2, int[] idxs, int next, int last) {
      Miscellanea.Assert(col1.Length == col2.Length);
      Miscellanea.Assert(idxs == null || col1.Length == idxs.Length);
      Miscellanea.Assert(next >= 0);
      Miscellanea.Assert(last >= -1 & last < col1.Length);
      this.col1 = col1;
      this.col2 = col2;
      this.idxs = idxs;
      this.next = next;
      this.last = last;
    }

    public BinRelIter(Obj[] col1, Obj[] col2, int next, int last) : this(col1, col2, null, next, last) {

    }

    public BinRelIter(Obj[] col1, Obj[] col2) : this(col1, col2, null, 0, col1.Length-1) {

    }

    public Obj Get1() {
      return col1[idxs == null ? next : idxs[next]];
    }

    public Obj Get2() {
      return col2[idxs == null ? next : idxs[next]];
    }

    public void Next() {
      Miscellanea.Assert(next <= last);
      next++;
    }

    public bool Done() {
      return next > last;
    }
  }


  public class TernRelIter {
    Obj[] col1;
    Obj[] col2;
    Obj[] col3;
    int[] idxs;
    int next;
    int last;

    public TernRelIter(Obj[] col1, Obj[] col2, Obj[] col3, int[] idxs, int next, int last) {
      Miscellanea.Assert(col1.Length == col2.Length && col1.Length == col3.Length);
      Miscellanea.Assert(idxs == null || idxs.Length == col1.Length);
      Miscellanea.Assert(next >= 0);
      Miscellanea.Assert(last >= -1 && last < col1.Length);
      this.col1 = col1;
      this.col2 = col2;
      this.col3 = col3;
      this.idxs = idxs;
      this.next = next;
      this.last = last;
    }

    public TernRelIter(Obj[] col1, Obj[] col2, Obj[] col3) : this(col1, col2, col3, null, 0, col1.Length-1) {

    }

    public Obj Get1() {
      return col1[idxs == null ? next : idxs[next]];
    }

    public Obj Get2() {
      return col2[idxs == null ? next : idxs[next]];
    }

    public Obj Get3() {
      return col3[idxs == null ? next : idxs[next]];
    }

    public void Next() {
      Miscellanea.Assert(next <= last);
      next++;
    }

    public bool Done() {
      return next > last;
    }
  }
}