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
      Debug.Assert(next <= last);
      return objs[next];
    }

    public void Next() {
      Debug.Assert(next <= last);
      next++;
    }

    public bool Done() {
      return next > last;
    }
  }


  public class BinRelIter {
    Obj[] col1;
    Obj[] col2;
    int next;

    public BinRelIter(Obj[] col1, Obj[] col2) {
      this.col1 = col1;
      this.col2 = col2;
      this.next = 0;
    }

    public Obj Get1() {
      return col1[next];
    }

    public Obj Get2() {
      return col2[next];
    }

    public void Next() {
      Debug.Assert(next < col1.Length);
      next++;
    }

    public bool Done() {
      return next >= col1.Length;
    }
  }


  public class TernRelIter {
    Obj[] col1;
    Obj[] col2;
    Obj[] col3;
    int next;

    public TernRelIter(Obj[] col1, Obj[] col2, Obj[] col3) {
      this.col1 = col1;
      this.col2 = col2;
      this.col3 = col3;
      this.next = 0;
    }

    public Obj Get1() {
      return col1[next];
    }

    public Obj Get2() {
      return col2[next];
    }

    public Obj Get3() {
      return col3[next];
    }

    public void Next() {
      Debug.Assert(next < col1.Length);
      next++;
    }

    public bool Done() {
      return next >= col1.Length;
    }
  }
}