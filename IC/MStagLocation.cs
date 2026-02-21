using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using UnityEngine;
using UnityEngine.SceneManagement;
using ItemChanger;
using ItemChanger.Extensions;
using ItemChanger.FsmStateActions;
using ItemChanger.Locations;
using ItemChanger.Placements;
using uManager = UnityEngine.SceneManagement.SceneManager;

namespace MoreStags {
    public class MStagLocation: CoordinateLocation {
        public string rawName;
        public int cost;

        protected override void OnLoad() {
            base.OnLoad();
            uManager.sceneLoaded += TryEditFsm;
        }

        protected override void OnUnload() {
            base.OnUnload();
            uManager.sceneLoaded -= TryEditFsm;
        }

        private void TryEditFsm(Scene arg0, LoadSceneMode arg1) {
            if(arg0.name == UnsafeSceneName)
                On.PlayMakerFSM.OnEnable += EditMStag;
            else if(!arg0.name.EndsWith("_boss") && !arg0.name.EndsWith("_boss_defeated"))
                On.PlayMakerFSM.OnEnable -= EditMStag;
        }

        private void EditMStag(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);
            if(self.FsmName == "Stag Control") {
                FsmState openGrate = self.GetState("Open Grate");
                for(int i = 0; i < Mathf.Min(2, openGrate.Actions.Length); i++) {
                    openGrate.Actions[i].Enabled = false;
                }

                FsmBool cancelTravel = self.AddFsmBool("Cancel Travel", false);

                if(!MoreStags.localData.opened[rawName]) {
                    self.FsmVariables.GetFsmInt("Station Position Number").Value = 0;
                    IntCompare curLocAction = self.GetState("Current Location Check").GetFirstActionOfType<IntCompare>();
                    if(curLocAction != null)
                        curLocAction.Enabled = false;
                    else
                        Modding.Logger.Log($"[MoreStags] - no IntCompare found in {UnsafeSceneName}");
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
                FsmState selectionMade = self.GetState("Selection Made Cancel");
                if(selectionMade.Actions[0] is not Lambda) {
                    selectionMade.AddFirstAction(new Lambda(() => {
                        GameObject.Find("Stag").LocateMyFSM("Stag Control").FsmVariables.GetFsmBool("Cancel Travel").Value = true;
                    }));
                }
            }
            else if(self.FsmName == "Stag Bell") {
                if(Satchel.FsmUtil.TryGetState(self, "Init", out FsmState init)) {
                    if(init.GetActionsOfType<StagOpenedBoolTest>().Length > 0) {
                        init.GetFirstActionOfType<StagOpenedBoolTest>().Enabled = false;
                    }
                    init.AddTransition("FINISHED", "Opened");
                }
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
