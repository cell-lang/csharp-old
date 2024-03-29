using System;
using System.Collections.Generic;


namespace CellLang {
  public static class Algs {
    static void CheckIsOrdered(Obj[] objs) {
      for (int i=1 ; i < objs.Length ; i++) {
        int cmp = objs[i-1].Cmp(objs[i]);
        if (cmp != 1) {
          Console.WriteLine("*****************************************");
          Console.WriteLine(objs[i-1].ToString());
          Console.WriteLine(objs[i].ToString());
          Console.WriteLine(cmp.ToString());
          throw new Exception();
        }
      }
    }

    public static int BinSearch(Obj[] objs, Obj obj) {
      return BinSearch(objs, 0, objs.Length, obj);
    }

    public static int BinSearch(Obj[] objs, int first, int count, Obj obj) {
      int low = first;
      int high = first + count - 1;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[mid].Cmp(obj)) {
          case -1:
            // objs[mid] > obj
            high = mid - 1;
            break;

          case 0:
            return mid;

          case 1:
            // objs[mid] < obj
            low = mid + 1;
            break;
        }
      }

      return -1;
    }

    public static int BinSearchRange(Obj[] objs, int offset, int length, Obj obj, out int first) {
      int low = offset;
      int high = offset + length - 1;
      int lower_bound = low;
      int upper_bound = high;


      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[mid].Cmp(obj)) {
          case -1:
            // objs[mid] > obj
            upper_bound = high = mid - 1;
            break;

          case 0:
            if (mid == offset || !objs[mid-1].IsEq(obj)) {
              first = mid;
              low = lower_bound;
              high = upper_bound;
              goto Next;
            }
            else
              high = mid - 1;
            break;

          case 1:
            // objs[mid] < obj
            lower_bound = low = mid + 1;
            break;
        }
      }

      first = 0;
      return 0;

    Next:
      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[mid].Cmp(obj)) {
          case -1:
            // objs[mid] > obj
            high = mid - 1;
            break;

          case 0:
            if (mid == upper_bound || !objs[mid+1].IsEq(obj)) {
              return mid - first + 1;
            }
            else
              low = mid + 1;
            break;

          case 1:
            // objs[mid] < obj
            low = mid + 1;
            break;
        }
      }

      // We're not supposed to ever get here.
      throw new InvalidOperationException();
    }


    public static int BinSearchRange(int[] idxs, Obj[] objs, Obj obj, out int first) {
      Miscellanea.Assert(idxs.Length == objs.Length);

      int offset = 0;
      int length = idxs.Length;

      int low = offset;
      int high = offset + length - 1;
      int lower_bound = low;
      int upper_bound = high;


      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[idxs[mid]].Cmp(obj)) {
          case -1:
            // objs[idxs[mid]] > obj
            upper_bound = high = mid - 1;
            break;

          case 0:
            if (mid == offset || !objs[idxs[mid-1]].IsEq(obj)) {
              first = mid;
              low = lower_bound;
              high = upper_bound;
              goto Next;
            }
            else
              high = mid - 1;
            break;

          case 1:
            // objs[idxs[mid]] < obj
            lower_bound = low = mid + 1;
            break;
        }
      }

      first = 0;
      return 0;

    Next:
      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        switch (objs[idxs[mid]].Cmp(obj)) {
          case -1:
            // objs[idxs[mid]] > obj
            high = mid - 1;
            break;

          case 0:
            if (mid == upper_bound || !objs[idxs[mid+1]].IsEq(obj)) {
              return mid - first + 1;
            }
            else
              low = mid + 1;
            break;

          case 1:
            // objs[idxs[mid]] < obj
            low = mid + 1;
            break;
        }
      }

      // We're not supposed to ever get here.
      throw new InvalidOperationException();
    }


    public static int BinSearchRange(Obj[] major, Obj[] minor, Obj majorVal, Obj minorVal, out int first) {
      int offset = 0;
      int length = major.Length;

      int low = offset;
      int high = offset + length - 1;
      int lower_bound = low;
      int upper_bound = high;


      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int res = major[mid].Cmp(majorVal);
        if (res == 0)
          res = minor[mid].Cmp(minorVal);
        switch (res) {
          case -1:
            // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
            upper_bound = high = mid - 1;
            break;

          case 0:
            if (mid == offset || (!major[mid-1].IsEq(majorVal) || !minor[mid-1].IsEq(minorVal))) {
              first = mid;
              low = lower_bound;
              high = upper_bound;
              goto Next;
            }
            else
              high = mid - 1;
            break;

          case 1:
            // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
            lower_bound = low = mid + 1;
            break;
        }
      }

      first = 0;
      return 0;

    Next:
      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int res = major[mid].Cmp(majorVal);
        if (res == 0)
          res = minor[mid].Cmp(minorVal);
        switch (res) {
          case -1:
            // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
            high = mid - 1;
            break;

          case 0:
            if (mid == upper_bound || (!major[mid+1].IsEq(majorVal) || !minor[mid+1].IsEq(minorVal))) {
              return mid - first + 1;
            }
            else
              low = mid + 1;
            break;

          case 1:
            // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
            low = mid + 1;
            break;
        }
      }

      // We're not supposed to ever get here.
      throw new InvalidOperationException();
    }


    public static int BinSearchRange(int[] idxs, Obj[] major, Obj[] minor, Obj majorVal, Obj minorVal, out int first) {
      int offset = 0;
      int length = major.Length;

      int low = offset;
      int high = offset + length - 1;
      int lower_bound = low;
      int upper_bound = high;

      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int midIdx = idxs[mid];
        int res = major[midIdx].Cmp(majorVal);
        if (res == 0) {
          res = minor[midIdx].Cmp(minorVal);
        }
        switch (res) {
          case -1:
            // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
            upper_bound = high = mid - 1;
            break;

          case 0:
            bool isFirst = mid == offset;
            if (!isFirst) {
              int prevIdx = idxs[mid-1];
              isFirst = !major[prevIdx].IsEq(majorVal) || !minor[prevIdx].IsEq(minorVal);
            }
            if (isFirst) {
              first = mid;
              low = lower_bound;
              high = upper_bound;
              goto Next;
            }
            else
              high = mid - 1;
            break;

          case 1:
            // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
            lower_bound = low = mid + 1;
            break;
        }
      }

      first = 0;
      return 0;

    Next:
      while (low <= high) {
        int mid = (int) (((long) low + (long) high) / 2);
        int midIdx = idxs[mid];
        int res = major[midIdx].Cmp(majorVal);
        if (res == 0)
          res = minor[midIdx].Cmp(minorVal);
        switch (res) {
          case -1:
            // major[mid] > majorVal | (major[mid] == majorVal & minor[mid] > minorVal)
            high = mid - 1;
            break;

          case 0:
            bool isLast = mid == upper_bound;
            if (!isLast) {
              int nextIdx = idxs[mid+1];
              isLast = !major[nextIdx].IsEq(majorVal) || !minor[nextIdx].IsEq(minorVal);
            }
            if (isLast) {
              return mid - first + 1;
            }
            else
              low = mid + 1;
            break;

          case 1:
            // major[mid] < majorVal | (major[mid] == majorVal) & minor[mid] < minorVal)
            low = mid + 1;
            break;
        }
      }

      // We're not supposed to ever get here.
      throw new InvalidOperationException();
    }


    public static Obj[] SortUnique(Obj[] objs, int count) {
      Miscellanea.Assert(count > 0);
      Array.Sort(objs, 0, count);
      int prev = 0;
      for (int i=1 ; i < count ; i++)
        if (!objs[prev].IsEq(objs[i]))
          if (i != ++prev)
            objs[prev] = objs[i];
      int len = prev + 1;
      Obj[] norm_objs = new Obj[len];
      Array.Copy(objs, norm_objs, len);
      return norm_objs;
    }


    public static void SortUnique(Obj[] col1, Obj[] col2, int count, out Obj[] norm_col_1, out Obj[] norm_col_2) {
      Miscellanea.Assert(count > 0);

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      Comparison<int> cmp = delegate(int i, int j) {
        int res = col1[i].CompareTo(col1[j]);
        if (res != 0)
          return res;
        return col2[i].CompareTo(col2[j]);
      };

      Array.Sort(idxs, cmp);

      int prev = 0;
      for (int i=1 ; i < count ; i++) {
        int j = idxs[i];
        int k = idxs[i-1];
        if (!col1[j].IsEq(col1[k]) || !col2[j].IsEq(col2[k]))
          if (i != ++prev)
            idxs[prev] = idxs[i];
      }

      int size = prev + 1;
      norm_col_1 = new Obj[size];
      norm_col_2 = new Obj[size];

      for (int i=0 ; i < size ; i++) {
        int j = idxs[i];
        norm_col_1[i] = col1[j];
        norm_col_2[i] = col2[j];
      }
    }

    public static void SortUnique(Obj[] col1, Obj[] col2, Obj[] col3, int count, out Obj[] norm_col_1, out Obj[] norm_col_2, out Obj[] norm_col_3) {
      Miscellanea.Assert(count > 0);

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      Comparison<int> cmp = delegate(int i, int j) {
        int res = col1[i].CompareTo(col1[j]);
        if (res != 0)
          return res;
        res = col2[i].CompareTo(col2[j]);
        if (res != 0)
          return res;
        return col3[i].CompareTo(col3[j]);
      };

      Array.Sort(idxs, cmp);

      int prev = 0;
      for (int i=1 ; i < count ; i++) {
        int j = idxs[i];
        int k = idxs[i-1];
        if (!col1[j].IsEq(col1[k]) || !col2[j].IsEq(col2[k]) || !col3[j].IsEq(col3[k]))
          if (i != ++prev)
            idxs[prev] = idxs[i];
      }

      int size = prev + 1;
      norm_col_1 = new Obj[size];
      norm_col_2 = new Obj[size];
      norm_col_3 = new Obj[size];

      for (int i=0 ; i < size ; i++) {
        int j = idxs[i];
        norm_col_1[i] = col1[j];
        norm_col_2[i] = col2[j];
        norm_col_3[i] = col3[j];
      }
    }

    public static bool SortedArrayHasDuplicates(Obj[] objs) {
      for (int i=1 ; i < objs.Length ; i++)
        if (objs[i].IsEq(objs[i-1]))
          return true;
      return false;
    }

    public static int[] SortedIndexes(Obj[] major, Obj[] minor) {
      Miscellanea.Assert(major.Length == minor.Length);

      int count = major.Length;

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      Comparison<int> cmp = delegate(int i, int j) {
        int res = major[i].CompareTo(major[j]);
        if (res != 0)
          return res;
        return minor[i].CompareTo(minor[j]);
      };

      Array.Sort(idxs, cmp);
      return idxs;
    }

    public static int[] SortedIndexes(Obj[] col1, Obj[] col2, Obj[] col3) {
      Miscellanea.Assert(col1.Length == col2.Length && col1.Length == col3.Length);

      int count = col1.Length;

      int[] idxs = new int[count];
      for (int i=0 ; i < count ; i++)
        idxs[i] = i;

      Comparison<int> cmp = delegate(int i, int j) {
        int res = col1[i].CompareTo(col1[j]);
        if (res != 0)
          return res;
        res = col2[i].CompareTo(col2[j]);
        if (res != 0)
          return res;
        return col3[i].CompareTo(col3[j]);
      };

      Array.Sort(idxs, cmp);
      return idxs;
    }
  }
}