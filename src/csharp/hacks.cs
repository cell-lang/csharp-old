using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;


namespace CellLang {
  static class Hacks {

    static ConditionalWeakTable<Obj, Obj> attachments = new ConditionalWeakTable<Obj, Obj>();

    static public void Attach(Obj target, Obj attachment) {
      attachments.Remove(target);
      attachments.Add(target, attachment);
    }

    static public Obj Fetch(Obj target) {
      Obj attachment;
      if (attachments.TryGetValue(target, out attachment))
        return new TaggedObj(SymbTable.JustSymbId, attachment);
      else
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