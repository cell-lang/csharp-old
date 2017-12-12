using System;
using System.Collections.Generic;


namespace CellLang {
  static class Hacks {

    static List<Obj> targets = new List<Obj>();
    static List<Obj> attachments = new List<Obj>();

    static public void Attach(Obj target, Obj attachment) {
      targets.Add(target);
      attachments.Add(attachment);
    }

    static public Obj Fetch(Obj target) {
      for (int i=0 ; i < targets.Count ; i++)
        if (targets[i] == target)
          return new TaggedObj(SymbTable.JustSymbId, attachments[i]);
      return new SymbObj(SymbTable.NothingSymbId);
    }
  }
}