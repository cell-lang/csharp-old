using System;
using System.Collections.Generic;
using System.Diagnostics;


namespace CellLang {
  public static class Builder {
    public static Obj CreateSeq(List<Obj> objs) {
      return new MasterSeqObj(objs.ToArray());
    }

    public static Obj CreateSeq(Obj[] objs, long count) {
      Miscellanea.Assert(objs != null && count <= objs.Length);
      for (int i=0 ; i < count ; i++)
        Miscellanea.Assert(objs[i] != null);

      Obj[] objs_copy = new Obj[count];
      for (int i=0 ; i < count ; i++)
        objs_copy[i] = objs[i];
      return new MasterSeqObj(objs_copy);
    }

    public static Obj CreateSet(List<Obj> objs) {
      return CreateSet(objs.ToArray(), objs.Count);
    }

    public static Obj CreateSet(Obj[] objs, long count) {
      Miscellanea.Assert(objs.Length >= count);
      if (count != 0) {
        Obj[] norm_objs = Algs.SortUnique(objs, (int) count);
        return new NeSetObj(norm_objs);
      }
      else
        return EmptyRelObj.Singleton();
    }

    public static Obj CreateMap(List<Obj> keys, List<Obj> vals) {
      Miscellanea.Assert(keys.Count == vals.Count);
      return CreateMap(keys.ToArray(), vals.ToArray(), keys.Count);
    }

    public static Obj CreateMap(Obj[] keys, Obj[] vals, long count) {
      Obj binRel = CreateBinRel(keys, vals, count);
      if (!binRel.IsEmptyRel() && !binRel.IsNeMap()) {
        BinRelIter iter = binRel.GetBinRelIter();
        //## REMOVE WHEN DONE
        while (!iter.Done()) {
          Console.WriteLine(iter.Get1().ToString());
          iter.Next();
        }
        throw new Exception();
      }
      return binRel;
    }

    public static Obj CreateBinRel(List<Obj> col1, List<Obj> col2) {
      Miscellanea.Assert(col1.Count == col2.Count);
      return CreateBinRel(col1.ToArray(), col2.ToArray(), col1.Count);
    }

    public static Obj CreateBinRel(Obj[] col1, Obj[] col2, long count) {
      Miscellanea.Assert(count <= col1.Length & count <= col2.Length);
      if (count != 0) {
        Obj[] norm_col_1, norm_col_2;
        Algs.SortUnique(col1, col2, (int) count, out norm_col_1, out norm_col_2);
        return new NeBinRelObj(norm_col_1, norm_col_2, !Algs.SortedArrayHasDuplicates(norm_col_1));
      }
      else
        return EmptyRelObj.Singleton();
    }

    public static Obj CreateBinRel(Obj obj1, Obj obj2) {
      Obj[] col1 = new Obj[1];
      Obj[] col2 = new Obj[1];
      col1[0] = obj1;
      col2[0] = obj2;
      return new NeBinRelObj(col1, col2, true);
    }

    public static Obj CreateTernRel(List<Obj> col1, List<Obj> col2, List<Obj> col3) {
      Miscellanea.Assert(col1.Count == col2.Count && col1.Count == col3.Count);
      return CreateTernRel(col1.ToArray(), col2.ToArray(), col3.ToArray(), col1.Count);
    }

    public static Obj CreateTernRel(Obj[] col1, Obj[] col2, Obj[] col3, long count) {
      Miscellanea.Assert(count <= col1.Length && count <= col2.Length && count <= col3.Length);
      if (col1.Length != 0) {
        Obj[] norm_col_1, norm_col_2, norm_col_3;
        Algs.SortUnique(col1, col2, col3, (int) count, out norm_col_1, out norm_col_2, out norm_col_3);
        return new NeTernRelObj(norm_col_1, norm_col_2, norm_col_3);
      }
      else {
        return EmptyRelObj.Singleton();
      }
    }

    public static Obj CreateTernRel(Obj obj1, Obj obj2, Obj obj3) {
      Obj[] col1 = new Obj[1];
      Obj[] col2 = new Obj[1];
      Obj[] col3 = new Obj[1];
      col1[0] = obj1;
      col2[0] = obj2;
      col3[0] = obj3;
      return new NeTernRelObj(col1, col2, col3);
    }

    public static Obj BuildConstIntSeq(byte[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = IntObj.Get(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(ushort[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = IntObj.Get(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(uint[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = IntObj.Get(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(sbyte[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = IntObj.Get(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(short[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = IntObj.Get(vals[i]);
      return new MasterSeqObj(objs);
    }

    public static Obj BuildConstIntSeq(int[] vals) {
      int len = vals.Length;
      Obj[] objs = new Obj[len];
      for (int i=0 ; i < len ; i++)
        objs[i] = IntObj.Get(vals[i]);
      return new MasterSeqObj(objs);
    }
  }
}
