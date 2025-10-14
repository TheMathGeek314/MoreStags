using System.IO;
using System.Linq;
using System.Reflection;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace MoreStags {
    public static class LogicAdder {
        public static void Hook() {
            RCData.RuntimeLogicOverride.Subscribe(50, ApplyLogic);
            RCData.RuntimeLogicOverride.Subscribe(1000.3f, TryMakeSubstitutions);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb) {
            if(!MoreStags.Settings.Enabled)
                return;
            JsonLogicFormat fmt = new();
            Assembly assembly = typeof(LogicAdder).Assembly;

            using Stream s = assembly.GetManifestResourceStream("MoreStags.Resources.logic.json");
            lmb.DeserializeFile(LogicFileType.Locations, fmt, s);

            using Stream st = assembly.GetManifestResourceStream("MoreStags.Resources.logicOverrides.json");
            lmb.DeserializeFile(LogicFileType.LogicEdit, fmt, st);

            /*using Stream str = assembly.GetManifestResourceStream("MoreStags.Resources.logicSubstitutions.json");
            lmb.DeserializeFile(LogicFileType.LogicSubst, fmt, str);*/

            DefineTermsAndItems(lmb, fmt);
        }

        private static void DefineTermsAndItems(LogicManagerBuilder lmb, JsonLogicFormat fmt) {
            using Stream t = typeof(LogicAdder).Assembly.GetManifestResourceStream("MoreStags.Resources.terms.json");
            lmb.DeserializeFile(LogicFileType.Terms, fmt, t);

            foreach(string stagName in StagData.allStags.Where(stag => !stag.isVanilla).Select(stag => RandoInterop.nameToLocation(stag.name))) {
                lmb.AddItem(new SingleItem(stagName, new RandomizerCore.TermValue(lmb.GetTerm(stagName), 1)));
            }
        }

        private static void TryMakeSubstitutions(GenerationSettings gs, LogicManagerBuilder lmb) {
            if(!MoreStags.Settings.Enabled)
                return;
            JsonLogicFormat fmt = new();
            using Stream s = typeof(LogicAdder).Assembly.GetManifestResourceStream("MoreStags.Resources.logicSubstitutions.json");
            lmb.DeserializeFile(LogicFileType.LogicSubst, fmt, s);
        }
    }
}
