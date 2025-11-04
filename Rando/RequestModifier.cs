using System.Collections.Generic;
using System.Linq;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace MoreStags {
    public class RequestModifier {
        public static void Hook() {
            RequestBuilder.OnUpdate.Subscribe(3, ApplyStagDef);
            RequestBuilder.OnUpdate.Subscribe(4, SetupItems);
            RequestBuilder.OnUpdate.Subscribe(0, CopyGlobalToLocal);
        }

        private static void ApplyStagDef(RequestBuilder rb) {
            SelectStags(rb);
            AddAndRemoveLocations(rb);
            PreserveLevers(rb);
            ConsiderTram(rb);
        }

        private static void PreserveLevers(RequestBuilder rb) {
            bool areLeversOn = rb.GetItemGroupFor("Lever-Resting_Grounds_Stag").Items.GetCount("Lever-Resting_Grounds_Stag") > 0;
            LocalData ld = MoreStags.localData;
            ld.preserveStagLevers = areLeversOn;
        }

        private static void SelectStags(RequestBuilder rb) {
            GlobalSettings ms = MoreStags.Settings;
            if(!ms.Enabled)
                return;
            List<StagData> stagsToActivate = [StagData.dataByRoom["Room_Town_Stag_Station"]];
            bool isRoomRando = rb.gs.TransitionSettings.Mode == RandomizerMod.Settings.TransitionSettings.TransitionMode.RoomRandomizer;
            if(!rb.gs.SkipSettings.EnemyPogos && !isRoomRando) {
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
                        string region = regions[rb.rng.Next(regions.Count)];
                        regions.Remove(region);
                        List<StagData> regionalCandidates = new(StagData.allStags.Where(stag => stag.region == region && !stagsToActivate.Contains(stag)));
                        filterBySettings(regionalCandidates);
                        if(regionalCandidates.Count == 0) {
                            masterRegionList.Remove(region);
                            continue;
                        }
                        StagData chosenCandi = regionalCandidates[rb.rng.Next(regionalCandidates.Count)];
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
                        StagData chosenCandi = candidates[rb.rng.Next(candidates.Count)];
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
        }

        private static void AddAndRemoveLocations(RequestBuilder rb) {
            if(rb.gs.PoolSettings.Stags) {
                LocalData ld = MoreStags.localData;
                foreach(StagData data in StagData.allStags.Where(stag => stag.isVanilla && !stag.isActive(ld))) {
                    string nameToRemove = Consts.LocationNames[data.name];
                    rb.RemoveLocationByName(nameToRemove);
                    rb.RemoveItemByName(nameToRemove);
                }
                foreach(StagData data in ld.activeStags.Where(stag => !stag.isVanilla)) {
                    string stagLocation = RandoInterop.nameToLocation(data.name);
                    rb.AddLocationByName(stagLocation);
                    rb.EditLocationRequest(stagLocation, info => {
                        info.getLocationDef = () => new() {
                            Name = stagLocation,
                            SceneName = data.scene,
                            FlexibleCount = false,
                            AdditionalProgressionPenalty = false
                        };
                        info.onRandoLocationCreation += (factory, rl) => {
                            rl.AddCost(new LogicGeoCost(factory.lm, data.cost));
                        };
                    });
                }
            }
        }

        private static void SetupItems(RequestBuilder rb) {
            if(!MoreStags.Settings.Enabled)
                return;

            foreach(string stagLocation in MoreStags.localData.activeStags.Where(stag => !stag.isVanilla).Select(stag => RandoInterop.nameToLocation(stag.name))) {
                rb.EditItemRequest(stagLocation, info => {
                    info.getItemDef = () => new ItemDef() {
                        Name = stagLocation,
                        Pool = "Stag",
                        PriceCap = 500,
                        MajorItem = false
                    };
                });
                if(rb.gs.PoolSettings.Stags) {
                    rb.AddItemByName(stagLocation);
                }
                else {
                    rb.AddToVanilla(stagLocation, stagLocation);
                }
            }

            if(!rb.gs.PoolSettings.Stags) {
                foreach(string vanillaLocation in StagData.allStags.Where(stag => stag.isVanilla && !stag.isActive(MoreStags.localData)).Select(stag => Consts.LocationNames[stag.name])) {
                    rb.RemoveFromVanilla(vanillaLocation);
                }
            }
        }

        private static void ConsiderTram(RequestBuilder rb) {
            if(!MoreStags.Settings.Enabled)
                return;
            if(rb.ctx.GenerationSettings.Seed % 10 == 3) {
                LocalData ld = MoreStags.localData;
                ld.tramActive = true;
                ld.tramBlueLakePosition = 1;
                ld.openedTram = false;
            }
        }

        private static void filterBySettings(List<StagData> data) {
            if(MoreStags.Settings.PreferNonVanilla)
                data.RemoveAll(stag => stag.isVanilla);
            if(MoreStags.Settings.RemoveCursedLocations)
                data.RemoveAll(stag => stag.isCursed);
        }

        private static void CopyGlobalToLocal(RequestBuilder rb) {
            MoreStags.localData.enabled = MoreStags.Settings.Enabled;
        }
    }
}
