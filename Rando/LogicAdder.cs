using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Json;
using RandomizerCore.Logic;
using RandomizerCore.LogicItems;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace MoreStags {
    public static class LogicAdder
    {
        public static void Hook()
        {
            RCData.RuntimeLogicOverride.Subscribe(50f, ApplyLogic);
            RCData.RuntimeLogicOverride.Subscribe(69420f, AlterConnectionLogic);
        }

        private static void ApplyLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!MoreStags.Settings.Enabled)
                return;
            JsonLogicFormat fmt = new();
            Assembly assembly = typeof(LogicAdder).Assembly;

            using Stream s = assembly.GetManifestResourceStream("MoreStags.Resources.logic.json");
            lmb.DeserializeFile(LogicFileType.Locations, fmt, s);

            using Stream st = assembly.GetManifestResourceStream("MoreStags.Resources.logicOverrides.json");
            lmb.DeserializeFile(LogicFileType.LogicEdit, fmt, st);

            using Stream str = assembly.GetManifestResourceStream("MoreStags.Resources.logicSubstitutions.json");
            lmb.DeserializeFile(LogicFileType.LogicSubst, fmt, str);

            using Stream stre = assembly.GetManifestResourceStream("MoreStags.Resources.waypoints.json");
            lmb.DeserializeFile(LogicFileType.Waypoints, fmt, stre);

            DefineTermsAndItems(lmb, fmt);
        }

        private static void DefineTermsAndItems(LogicManagerBuilder lmb, JsonLogicFormat fmt)
        {
            using Stream t = typeof(LogicAdder).Assembly.GetManifestResourceStream("MoreStags.Resources.terms.json");
            lmb.DeserializeFile(LogicFileType.Terms, fmt, t);

            foreach(string stagName in StagData.allStags.Where(stag => !stag.isVanilla).Select(stag => RandoInterop.nameToLocation(stag.name))) {
                lmb.AddItem(new MultiItem(stagName, [
                    new TermValue(lmb.GetTerm(stagName), 1),
                    new TermValue(lmb.GetTerm("STAGS"), 1)
                ]));
            }
        }

        private static void AlterConnectionLogic(GenerationSettings gs, LogicManagerBuilder lmb)
        {
            if (!MoreStags.Settings.Enabled)
                return;

            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() { TypeNameHandling = TypeNameHandling.Auto };

            using Stream stream = assembly.GetManifestResourceStream("MoreStags.Resources.connectionOverrides.json");
            StreamReader reader = new(stream);
            List<ConnectionLogicObject> objectList = jsonSerializer.Deserialize<List<ConnectionLogicObject>>(new JsonTextReader(reader));

            foreach (ConnectionLogicObject o in objectList)
            {
                foreach (var sub in o.logicSubstitutions)
                {
                    bool exists = lmb.LogicLookup.TryGetValue(o.name, out _);
                    if (exists)
                        lmb.DoSubst(new(o.name, sub.Key, sub.Value));
                }

                if (o.logicOverride != "")
                {
                    bool exists = lmb.LogicLookup.TryGetValue(o.name, out _);
                    if (exists)
                        lmb.DoLogicEdit(new(o.name, o.logicOverride));
                }
            }
        }
    }
    
    public class ConnectionLogicObject
    {
        public string name;
        public string logicOverride;
        public Dictionary<string, string> logicSubstitutions;
    }
}
