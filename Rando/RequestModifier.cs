using System.Collections.Generic;
using System.Linq;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace MoreStags {
    public class RequestModifier {
        public static void Hook() {
            RequestBuilder.OnUpdate.Subscribe(3, ApplyStagDef);
            RequestBuilder.OnUpdate.Subscribe(4, SetupItems);
        }

        private static void ApplyStagDef(RequestBuilder rb) {
            SelectStags(rb);
            AddAndRemoveLocations(rb);
        }

        private static void SelectStags(RequestBuilder rb) {
            List<StagData> stagsToActivate = new();
            stagsToActivate.Add(StagData.dataByRoom["Room_Town_Stag_Station"]);
            int iterations = 10;
            if(!rb.gs.SkipSettings.EnemyPogos) {
                stagsToActivate.Add(StagData.dataByRoom["Cliffs_03"]);
                iterations = 9;
            }
            switch(MoreStags.Settings.Selection) {
                case StagSelection.Balanced:
                    List<string> regions = new(Consts.Regions);
                    if(stagsToActivate.Count == 2)
                        regions.Remove("Cliffs");
                    for(int i = 0; i < iterations; i++) {
                        string region = regions[rb.rng.Next(regions.Count)];
                        regions.Remove(region);
                        List<StagData> regionalCandidates = StagData.allStags.Where(stag => stag.region == region).ToList();
                        filterBySettings(regionalCandidates);
                        StagData chosenCandi = regionalCandidates[rb.rng.Next(regionalCandidates.Count)];
                        stagsToActivate.Add(chosenCandi);
                    }
                    break;
                case StagSelection.Random:
                    List<StagData> candidates = new(StagData.allStags.Where(stag => stag.name != "Dirtmouth"));
                    if(stagsToActivate.Count == 2)
                        candidates.RemoveAll(stag => stag.name == "Stag Nest");
                    filterBySettings(candidates);
                    for(int i = 0; i < iterations; i++) {
                        StagData chosenCandi = candidates[rb.rng.Next(candidates.Count)];
                        candidates.Remove(chosenCandi);
                        stagsToActivate.Add(chosenCandi);
                    }   
                    break;
                case StagSelection.All:
                    stagsToActivate = new(StagData.allStags);
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
                    Modding.Logger.Log("[MoreStags] - Removing " + nameToRemove);
                    rb.RemoveLocationByName(nameToRemove);
                    rb.RemoveItemByName(nameToRemove);
                }
                foreach(StagData data in ld.activeStags.Where(stag => !stag.isVanilla)) {
                    string stagLocation = RandoInterop.nameToLocation(data.name);
                    Modding.Logger.Log("[MoreStags] - Adding location " + stagLocation);
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
                    Modding.Logger.Log("[MoreStags] - Adding item " + stagLocation);
                    rb.AddItemByName(stagLocation);
                }
                else {
                    rb.AddToVanilla(stagLocation, stagLocation);
                }
            }
        }

        private static void filterBySettings(List<StagData> data) {
            if(MoreStags.Settings.PreferNonVanilla)
                data.RemoveAll(stag => stag.isVanilla);
            if(MoreStags.Settings.RemoveCursedLocations)
                data.RemoveAll(stag => stag.isCursed);
        }
    }
}
