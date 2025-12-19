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
using UnityEngine;

namespace MoreStags {
    public static class LogicAdder {
        public static void Hook() {
            RCData.RuntimeLogicOverride.Subscribe(50f, ApplyLogic);
            RCData.RuntimeLogicOverride.Subscribe(69420f, AlterConnectionLogic);
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

            using Stream str = assembly.GetManifestResourceStream("MoreStags.Resources.logicSubstitutions.json");
            lmb.DeserializeFile(LogicFileType.LogicSubst, fmt, str);

            using Stream stre = assembly.GetManifestResourceStream("MoreStags.Resources.waypoints.json");
            lmb.DeserializeFile(LogicFileType.Waypoints, fmt, stre);

            if(!MoreStags.Settings.RemoveCursedLocations) {
                using Stream strea = assembly.GetManifestResourceStream("MoreStags.Resources.macros.json");
                lmb.DeserializeFile(LogicFileType.Macros, fmt, strea);

                using Stream stream = assembly.GetManifestResourceStream("MoreStags.Resources.GodhomeWaypoints.json");
                lmb.DeserializeFile(LogicFileType.Waypoints, fmt, stream);

                using Stream streamz = assembly.GetManifestResourceStream("MoreStags.Resources.transitions.json");
                lmb.DeserializeFile(LogicFileType.Transitions, fmt, streamz);

                using Stream streamzy = assembly.GetManifestResourceStream("MoreStags.Resources.GodhomeStags.json");
                lmb.DeserializeFile(LogicFileType.Locations, fmt, streamzy);
            }

            int stagMax = 115;
            if(MoreStags.Settings.RemoveCursedLocations)
                stagMax -= 3;
            int requireStagNest = gs.TransitionSettings.Mode != TransitionSettings.TransitionMode.RoomRandomizer && !gs.SkipSettings.EnemyPogos ? 1 : 0;
            if(MoreStags.Settings.PreferNonVanilla)
                stagMax -= 10 - requireStagNest;
            int stagCount = Mathf.Min(MoreStags.Settings.Quantity, stagMax) - 2;
            if(MoreStags.Settings.StagNestThreshold == StagNestThreshold.Half)
                stagCount /= 2;
            if(MoreStags.Settings.StagNestThreshold == StagNestThreshold.Many) {
                stagCount *= 3;
                stagCount /= 4;
            }
            if(MoreStags.Settings.StagNestThreshold == StagNestThreshold.Most) {
                stagCount *= 9;
                stagCount /= 10;
            }
            lmb.DoMacroEdit(new("ALLSTAGS", $"STAGS>{stagCount - 1}"));

            DefineTermsAndItems(lmb, fmt);
        }

        private static void DefineTermsAndItems(LogicManagerBuilder lmb, JsonLogicFormat fmt) {
            using Stream t = typeof(LogicAdder).Assembly.GetManifestResourceStream("MoreStags.Resources.terms.json");
            lmb.DeserializeFile(LogicFileType.Terms, fmt, t);

            foreach(string stagName in StagData.allStags.Where(stag => !stag.isVanilla).Select(stag => RandoInterop.nameToLocation(stag.name))) {
                lmb.AddItem(new MultiItem(stagName, [
                    new TermValue(lmb.GetTerm(stagName), 1),
                    new TermValue(lmb.GetTerm("STAGS"), 1)
                ]));
            }
        }

        private static void AlterConnectionLogic(GenerationSettings gs, LogicManagerBuilder lmb) {
            if(!MoreStags.Settings.Enabled)
                return;

            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() { TypeNameHandling = TypeNameHandling.Auto };

            using Stream stream = assembly.GetManifestResourceStream("MoreStags.Resources.connectionOverrides.json");
            StreamReader reader = new(stream);
            List<ConnectionLogicObject> objectList = jsonSerializer.Deserialize<List<ConnectionLogicObject>>(new JsonTextReader(reader));

            foreach(ConnectionLogicObject o in objectList) {
                foreach(var sub in o.logicSubstitutions) {
                    bool exists = lmb.LogicLookup.TryGetValue(o.name, out _);
                    if(exists)
                        lmb.DoSubst(new(o.name, sub.Key, sub.Value));
                }

                if(o.logicOverride != "") {
                    bool exists = lmb.LogicLookup.TryGetValue(o.name, out _);
                    if(exists)
                        lmb.DoLogicEdit(new(o.name, o.logicOverride));
                }
            }

            System.Random rng = new(gs.Seed + 115);
            SelectStags(gs, rng);
            List<StagData> activeStags = MoreStags.localData.activeStags;
            if (MoreStags.Settings.RemoveCursedLocations)
                activeStags = activeStags.Where(s => !s.isCursed).ToList();
            string clause = string.Join(" | ", activeStags.Select(s => $"*{(s.isVanilla ? Consts.LocationNames[s.name] : RandoInterop.nameToLocation(s.name))}"));
            lmb.DoLogicEdit(new("Can_Stag", clause));
        }

        private static void SelectStags(GenerationSettings gs, System.Random rng) {
            GlobalSettings ms = MoreStags.Settings;
            if(!ms.Enabled)
                return;
            List<StagData> stagsToActivate = [StagData.dataByRoom["Room_Town_Stag_Station"]];
            bool isRoomRando = gs.TransitionSettings.Mode == TransitionSettings.TransitionMode.RoomRandomizer;
            if(!gs.SkipSettings.EnemyPogos && !isRoomRando) {
                stagsToActivate.Add(StagData.dataByRoom["Cliffs_03"]);
            }
            switch(ms.Selection) {
                case StagSelection.Balanced:
                    List<string> masterRegionList = new(Consts.Regions);
                    List<string> regions = new(masterRegionList);
                    if(stagsToActivate.Count == 2)
                        regions.Remove("Cliffs");
                    while(stagsToActivate.Count < ms.Quantity) {
                        if(regions.Count == 0) {
                            if(masterRegionList.Count == 0) {
                                break;
                            }
                            regions.AddRange(masterRegionList);
                        }
                        string region = regions[rng.Next(regions.Count)];
                        regions.Remove(region);
                        List<StagData> regionalCandidates = new(StagData.allStags.Where(stag => stag.region == region && !stagsToActivate.Contains(stag)));
                        filterBySettings(regionalCandidates);
                        if(regionalCandidates.Count == 0) {
                            masterRegionList.Remove(region);
                            continue;
                        }
                        StagData chosenCandi = regionalCandidates[rng.Next(regionalCandidates.Count)];
                        stagsToActivate.Add(chosenCandi);
                    }
                    break;
                case StagSelection.Random:
                    List<StagData> candidates = new(StagData.allStags.Where(stag => stag.name != "Dirtmouth"));
                    if(stagsToActivate.Count == 2)
                        candidates.RemoveAll(stag => stag.name == "Stag Nest");
                    filterBySettings(candidates);
                    while(stagsToActivate.Count < ms.Quantity) {
                        if(candidates.Count == 0)
                            break;
                        StagData chosenCandi = candidates[rng.Next(candidates.Count)];
                        candidates.Remove(chosenCandi);
                        stagsToActivate.Add(chosenCandi);
                    }
                    break;
            }
            LocalData ld = MoreStags.localData;
            ld.activeStags.Clear();
            ld.opened.Clear();
            foreach(StagData data in stagsToActivate) {
                ld.activeStags.Add(data);
                ld.opened.Add(data.name, false);
            }
            ld.activeStags.Sort((a, b) => a.positionNumber.CompareTo(b.positionNumber));
            ld.threshold = Mathf.FloorToInt(ms.StagNestThreshold switch {
                StagNestThreshold.Half => 0.5f,
                StagNestThreshold.Many => 0.75f,
                StagNestThreshold.Most => 0.9f,
                StagNestThreshold.All => 1f,
                _ => 1f
            } * stagsToActivate.Where(stag => stag.name != "Dirtmouth" && stag.name != "Stag Nest").Count()) - 1;
        }

        private static void filterBySettings(List<StagData> data) {
            if(MoreStags.Settings.PreferNonVanilla)
                data.RemoveAll(stag => stag.isVanilla);
            if(MoreStags.Settings.RemoveCursedLocations)
                data.RemoveAll(stag => stag.isCursed);
        }
    }

    public class ConnectionLogicObject {
        public string name;
        public string logicOverride;
        public Dictionary<string, string> logicSubstitutions;
    }
}
