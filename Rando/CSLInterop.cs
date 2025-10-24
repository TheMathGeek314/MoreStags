using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using MonoMod.RuntimeDetour;
using RandomizerMod.Logging;
using CSL = CondensedSpoilerLogger.Loggers.CondensedSpoilerLog;
using Category = CondensedSpoilerLogger.Loggers.CondensedSpoilerLog.Category;

namespace MoreStags {
    public class CSLInterop {
        public static void Hook() {
            PatchStagCategory();
            LogManager.AddLogger(new StagLogger());
        }

        private static void PatchStagCategory() {
            MethodInfo methodToHook = typeof(CSL).GetMethod("GetCategories", BindingFlags.NonPublic | BindingFlags.Static);
            FieldInfo defaultCategories = typeof(CSL).GetField("DefaultCategories", BindingFlags.NonPublic | BindingFlags.Static);
            new Hook(methodToHook, (Func<IEnumerable<Category>> orig) => {
                List<Category> defCat = defaultCategories.GetValue(null) as List<Category>;
                for(int i = 0; i < defCat.Count; i++) {
                    if(defCat[i].Name == "Stag Stations") {
                        var test = defCat[i].Test;
                        List<string> items = StagData.allStags.Select(stag => stag.isVanilla ? Consts.LocationNames[stag.name] : RandoInterop.nameToLocation(stag.name)).ToList();
                        defCat.RemoveAt(i);
                        defCat.Insert(i, new("Stag Stations", test, items));
                    }
                }
                return orig();
            });
        }
    }

    public class StagLogger: RandoLogger {
        public override void Log(LogArguments args) {
            if(!MoreStags.Settings.Enabled)
                return;
            StringBuilder sb = new();
            List<StagData> activeStags = MoreStags.localData.activeStags;

            sb.AppendLine($"Stags active for seed: {args.gs.Seed}");
            sb.AppendLine();

            sb.AppendLine("------Vanilla Stags------");
            foreach(StagData stag in activeStags.Where(stag => stag.isVanilla)) {
                sb.AppendLine(Consts.LocationNames[stag.name]);
            }
            sb.AppendLine();

            sb.AppendLine("------More Stags------");
            foreach(StagData stag in activeStags.Where(stag => !stag.isVanilla)) {
                sb.AppendLine(RandoInterop.nameToLocation(stag.name));
            }

            LogManager.Write(sb.ToString(), "MoreStagsActiveSpoiler.txt");
        }
    }
}