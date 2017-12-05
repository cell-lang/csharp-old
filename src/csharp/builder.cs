using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public static class Builder {
    public static Obj CreateSeq(List<Obj> objs) {
      return new MasterSeqObj(objs.ToArray());
    }

    public static Obj CreateSeq(Obj[] objs, long count) {
      Debug.Assert(objs != null && objs.Length == count);
      return new MasterSeqObj(objs);
    }

    public static Obj CreateSet(List<Obj> objs) {
      return CreateSet(objs.ToArray(), objs.Count);
    }

    public static Obj CreateSet(Obj[] objs, long count) {
      Debug.Assert(objs.Length == count);
      if (count != 0) {
        Obj[] norm_objs = Algs.SortUnique(objs);
        return new NeSetObj(norm_objs);
      }
      else
        return EmptyRelObj.Singleton();
    }

    public static Obj CreateMap(List<Obj> keys, List<Obj> vals) {
      Debug.Assert(keys.Count == vals.Count);
      return CreateMap(keys.ToArray(), vals.ToArray(), keys.Count);
    }

    public static Obj CreateMap(Obj[] keys, Obj[] vals, int count) {
      Debug.Assert(keys.Length == count && vals.Length == count);
      Obj binRel = CreateBinRel(keys, vals, count);
      if (!binRel.IsEmptyRel() && !binRel.IsNeMap())
        throw new Exception();
      return binRel;
    }

    public static Obj CreateBinRel(List<Obj> col1, List<Obj> col2) {
      Debug.Assert(col1.Count == col2.Count);
      return CreateBinRel(col1.ToArray(), col2.ToArray(), col1.Count);
    }

    public static Obj CreateBinRel(Obj[] col1, Obj[] col2, int count) {
      Debug.Assert(col1.Length == col2.Length && col1.Length == count);
      if (count != 0) {
        Obj[] norm_col_1, norm_col_2;
        Algs.SortUnique(col1, col2, out norm_col_1, out norm_col_2);
        return new NeBinRelObj(norm_col_1, norm_col_2, Algs.SortedArrayHasDuplicates(norm_col_1));
      }
      else
        return EmptyRelObj.Singleton();
    }

    public static Obj CreateTernRel(List<Obj> col1, List<Obj> col2, List<Obj> col3) {
      Debug.Assert(col1.Count == col2.Count && col1.Count == col3.Count);
      return CreateTernRel(col1.ToArray(), col2.ToArray(), col3.ToArray());
    }

    public static Obj CreateTernRel(Obj[] col1, Obj[] col2, Obj[] col3) {
      Debug.Assert(col1.Length == col2.Length && col1.Length == col3.Length);
      if (col1.Length != 0) {
        Obj[] norm_col_1, norm_col_2, norm_col_3;
        Algs.SortUnique(col1, col2, col3, out norm_col_1, out norm_col_2, out norm_col_3);
        return new NeTernRelObj(norm_col_1, norm_col_2, norm_col_3);
      }
      else {
        return EmptyRelObj.Singleton();
      }
    }

    public static Obj BuildConstIntSeq(byte[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = new IntObj(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(ushort[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = new IntObj(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(uint[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = new IntObj(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(sbyte[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = new IntObj(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(short[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = new IntObj(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(int[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = new IntObj(vals[i]);
      return new MasterSeqObj(objs);
    }
  }
}
