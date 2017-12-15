using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


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
      return SymbObj.Get(SymbTable.NothingSymbId);
    }

    static ConditionalWeakTable<Obj, Obj> cachedSourceFileLocation = new ConditionalWeakTable<Obj, Obj>();

    static public void SetSourceFileLocation(Obj ast, Obj value) {
      cachedSourceFileLocation.Add(ast, value);
    }

    static public Obj GetSourceFileLocation(Obj ast) {
      Obj value;
      if (cachedSourceFileLocation.TryGetValue(ast, out value))
        return value;
      else
        return null;
    }
  }
}