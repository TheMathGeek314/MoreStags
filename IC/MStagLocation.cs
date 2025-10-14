using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Placements;
using UnityEngine;

namespace MoreStags {
    public class MStagLocation: CoordinateLocation {
        public string rawName;
        public int cost;

        protected override void OnLoad() {
            On.PlayMakerFSM.OnEnable += EditMStag;
        }

        protected override void OnUnload() {
            On.PlayMakerFSM.OnEnable -= EditMStag;
        }

        private void EditMStag(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);
            if(self.FsmName == "Stag Control") {
                FsmState openGrate = self.GetState("Open Grate");
                openGrate.Actions[0].Enabled = false;
                openGrate.Actions[1].Enabled = false;

                FsmBool cancelTravel = self.AddFsmBool("Cancel Travel", false);

                if(!MoreStags.localData.opened[rawName]) {
                    self.FsmVariables.GetFsmInt("Station Position Number").Value = 0;
                    self.GetState("Current Location Check").GetFirstActionOfType<IntCompare>().Enabled = false;
                    FsmState checkResult = self.GetState("Check Result");
                    checkResult.AddFirstAction(new Lambda(() => {
                        if(cancelTravel.Value) {
                            self.SendEvent("CANCEL");
                        }
                    }));
                    checkResult.AddTransition("CANCEL", "HUD Return");
                }

                self.GetState("HUD Return").AddFirstAction(new SetBoolValue {
                    boolVariable = cancelTravel,
                    boolValue = false
                });
            }
            else if(self.gameObject.name == "UI List Stag" && self.FsmName == "ui_list") {
                self.GetState("Selection Made Cancel").AddFirstAction(new Lambda(() => {
                    GameObject.Find("Stag").LocateMyFSM("Stag Control").FsmVariables.GetFsmBool("Cancel Travel").Value = true;
                }));
            }
            else if(self.FsmName == "Stag Bell") {
                FsmState init = self.GetState("Init");
                init.GetFirstActionOfType<StagOpenedBoolTest>().Enabled = false;
                init.AddTransition("FINISHED", "Opened");
            }
        }

        public override AbstractPlacement Wrap() {
            return new MutablePlacement(name) {
                Location = this,
                Cost = new GeoCost(cost)
            };
        }
    }
}
