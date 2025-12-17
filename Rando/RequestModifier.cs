using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using RandomizerCore.Extensions;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace MoreStags {
    public class RequestModifier {
        public static void Hook() {
            ProgressionInitializer.OnCreateProgressionInitializer += SetPI;
            RequestBuilder.OnUpdate.Subscribe(55f, ApplyStagDef);
            RequestBuilder.OnUpdate.Subscribe(60f, SetupItems);
            RequestBuilder.OnUpdate.Subscribe(75f, EditStartItems);
            RequestBuilder.OnUpdate.Subscribe(80f, WriteToLocal);
            RequestBuilder.OnUpdate.Subscribe(1100f, AddGodhomeTransitions);
        }

        private static void AddGodhomeTransitions(RequestBuilder rb) {
            if(!MoreStags.Settings.Enabled)
                return;
            if(MoreStags.Settings.RemoveCursedLocations)
                return;

            Assembly assembly = Assembly.GetExecutingAssembly();
            JsonSerializer jsonSerializer = new() { TypeNameHandling = TypeNameHandling.Auto };
            using Stream stream = assembly.GetManifestResourceStream("MoreStags.Resources.TransitionDefs.json");
            StreamReader reader = new(stream);
            List<TransitionDef> list = jsonSerializer.Deserialize<List<TransitionDef>>(new JsonTextReader(reader));
            int group = 1;
            foreach(TransitionDef def in list) {
                bool shouldBeIncluded = def.IsMapAreaTransition && (rb.gs.TransitionSettings.Mode >= TransitionSettings.TransitionMode.MapAreaRandomizer);
                shouldBeIncluded |= def.IsTitledAreaTransition && (rb.gs.TransitionSettings.Mode >= TransitionSettings.TransitionMode.FullAreaRandomizer);
                shouldBeIncluded |= rb.gs.TransitionSettings.Mode >= TransitionSettings.TransitionMode.RoomRandomizer;
                if(shouldBeIncluded) {
                    rb.EditTransitionRequest($"{def.SceneName}[{def.DoorName}]", info => info.getTransitionDef = () => def);
                    bool uncoupled = rb.gs.TransitionSettings.TransitionMatching == TransitionSettings.TransitionMatchingSetting.NonmatchingDirections;
                    if(uncoupled) {
                        SelfDualTransitionGroupBuilder tgb = rb.EnumerateTransitionGroups().First(x => x.label == RBConsts.TwoWayGroup) as SelfDualTransitionGroupBuilder;
                        tgb.Transitions.Add($"{def.SceneName}[{def.DoorName}]");
                    }
                    else {
                        SymmetricTransitionGroupBuilder stgb = rb.EnumerateTransitionGroups().First(x => x.label == RBConsts.InLeftOutRightGroup) as SymmetricTransitionGroupBuilder;
                        if(group == 1)
                            stgb.Group1.Add($"{def.SceneName}[{def.DoorName}]");
                        else
                            stgb.Group2.Add($"{def.SceneName}[{def.DoorName}]");
                    }
                    group = group == 1 ? 2 : 1;
                }
                else {
                    rb.EditTransitionRequest($"{def.SceneName}[{def.DoorName}]", info => info.getTransitionDef = () => def);
                    rb.EnsureVanillaSourceTransition($"{def.SceneName}[{def.DoorName}]");
                }
            }
        }

        private static void ApplyStagDef(RequestBuilder rb) {
            AddAndRemoveLocations(rb);
            PreserveLevers(rb);
            ConsiderTram(rb);
        }

        private static void PreserveLevers(RequestBuilder rb) {
            bool areLeversOn = rb.GetItemGroupFor("Lever-Resting_Grounds_Stag").Items.GetCount("Lever-Resting_Grounds_Stag") > 0;
            MoreStags.localData.preserveStagLevers = areLeversOn;
        }

        private static void AddAndRemoveLocations(RequestBuilder rb) {
            if(!MoreStags.Settings.Enabled)
                return;
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
                        Pool = PoolNames.Stag,
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

        private static void EditStartItems(RequestBuilder rb) {
            foreach(string vanillaStag in Data.GetPoolDef(PoolNames.Stag).IncludeItems.ToArray()) {
                if(rb.IsAtStart(vanillaStag))
                    rb.RemoveFromStart(vanillaStag);
            }
            List<StagData> startItems = new();

            void AddFrom(IList<StagData> items, double rate, int cap) {
                items = rb.rng.Permute(items);
                int toAdd = 0;
                for(int i = 0; i < items.Count; i++) {
                    if(rb.rng.NextDouble() < rate) {
                        toAdd = (toAdd + 1) % (cap + 1);
                    }
                }
                for(int i = 0; i < toAdd; i++) {
                    startItems.Add(items[i]);
                }
            }

            List<StagData> stags = new(MoreStags.localData.activeStags);

            switch(rb.gs.StartItemSettings.Stags) {
                case StartItemSettings.StartStagType.None:
                    break;
                case StartItemSettings.StartStagType.DirtmouthStag:
                    startItems.Add(StagData.dataByRoom["Room_Town_Stag_Station"]);
                    break;
                case StartItemSettings.StartStagType.OneRandomStag:
                    startItems.Add(rb.rng.Next(stags));
                    break;
                case StartItemSettings.StartStagType.ZeroOrMoreRandomStags:
                    AddFrom(stags, 1d / stags.Count, 3);
                    break;
                case StartItemSettings.StartStagType.ManyRandomStags:
                    AddFrom(stags, 0.4, stags.Count);
                    break;
                case StartItemSettings.StartStagType.AllStags:
                    startItems.AddRange(stags);
                    break;
            }

            foreach(StagData stag in startItems) {
                string name = stag.isVanilla ? Consts.LocationNames[stag.name] : RandoInterop.nameToLocation(stag.name);
                rb.AddToStart(name);
                rb.GetItemGroupFor(name).Items.Remove(name, 1);
            }
        }

        private static void WriteToLocal(RequestBuilder rb) {
            MoreStags.localData.enabled = MoreStags.Settings.Enabled;
        }

        private static void SetPI(LogicManager lm, GenerationSettings gs, ProgressionInitializer pi)
        {
            if (!MoreStags.Settings.Enabled)
                return;
            
            // Lake tram is only considered logical for traversing Crossroads_50 if this is enabled.
            // For the moment it considers either a full tram pass or any of the Split Tram halves.
            if (gs.Seed % 10 == 3)
                pi.Setters.Add(new(lm.GetTerm("LAKETRAM"), 1));
        }
    }
}
