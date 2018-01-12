using System;
using System.IO;


namespace CellLang {
  interface Value {
    bool IsSymb();
    bool IsInt();
    bool IsFloat();
    bool IsSeq();
    bool IsSet();
    bool IsBinRel();
    bool IsTernRel();
    bool IsTagged();

    string AsSymb();
    long   AsInt();
    double AsFloat();

    int Size();
    Value Item(int index);
    void Entry(int index, out Value field1, out Value field2);
    void Entry(int index, out Value field1, out Value field2, out Value field3);

    string Tag();
    Value Untagged();

    bool IsString();
    bool IsRecord();

    string AsString();
    Value Lookup(string field);

    string Printed();
    void Print(Stream stream);
  }
}
