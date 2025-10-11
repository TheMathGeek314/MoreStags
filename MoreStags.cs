using Modding;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker;
using HutongGames.PlayMaker.Actions;
using Satchel;
using MoreStags.Rando;

namespace MoreStags {
    public class MoreStags: Mod, ILocalSettings<LocalData>, IGlobalSettings<GlobalSettings> {
        new public string GetName() => "MoreStags";
        public override string GetVersion() => "1.0.0.0";

        public static GlobalSettings Settings { get; set; } = new();
        public void OnLoadGlobal(GlobalSettings s) => Settings = s;
        public GlobalSettings OnSaveGlobal() => Settings;

        public static LocalData localData { get; set; } = new();
        public void OnLoadLocal(LocalData d) => localData = d;
        public LocalData OnSaveLocal() => localData;

        internal static MoreStags instance;

        public GameObject stagPrefabLeft;
        public GameObject stagPrefabRight;
        public GameObject bellPrefab;
        public GameObject transitionPrefabLeft;
        public GameObject transitionPrefabRight;
        public GameObject uiPrefab;

        public static Dictionary<string, FsmOwnerDefault> uiGameobjectDict = new();

        public MoreStags() : base() {
            instance = this;
        }

        public override void Initialize(Dictionary<string, Dictionary<string, GameObject>> preloadedObjects) {
            UnityEngine.SceneManagement.SceneManager.activeSceneChanged += earlySceneChange;
            On.GameManager.OnNextLevelReady += lateSceneChange;
            On.PlayMakerFSM.OnEnable += editFsm;

            stagPrefabLeft = preloadedObjects["Crossroads_47"]["Stag"];
            stagPrefabRight = preloadedObjects["Ruins2_08"]["Stag"];
            bellPrefab = preloadedObjects["Crossroads_47"]["_Scenery/Station Bell"];
            transitionPrefabLeft = preloadedObjects["Crossroads_47"]["_Transition Gates/door_stagExit"];
            transitionPrefabRight = preloadedObjects["Ruins2_08"]["door_stagExit"];

            Assembly assembly = Assembly.GetExecutingAssembly();
            string stagDataJson = assembly.GetManifestResourceNames().Single(str => str.EndsWith("StagData.json"));
            using Stream stagStream = assembly.GetManifestResourceStream(stagDataJson);
            foreach(StagData data in new ParseJson(stagStream).parseFile<StagData>()) {
                data.translate();
            }
            simulateGeneration(localData);

            foreach(GameObject go in Resources.FindObjectsOfTypeAll<GameObject>()) {
                if(!go.scene.IsValid() && go.name == "Stag Map") {
                    uiPrefab = go.FindGameObjectInChildren("UI List Stag").FindGameObjectInChildren("Crossroads");
                    break;
                }
            }

            RandoInterop.Hook();
            if(ModHooks.GetMod("DebugMod") is Mod) {
                DebugInterop.HookDebug();
            }
        }

        public override List<(string, string)> GetPreloadNames() {
            return new List<(string, string)> {
                ("Crossroads_47", "_Scenery/Station Bell"),
                ("Crossroads_47", "Stag"),
                ("Crossroads_47", "_Transition Gates/door_stagExit"),
                ("Ruins2_08", "Stag"),
                ("Ruins2_08", "door_stagExit")
            };
        }

        //temporary until I get real generation (only works on new save files)
        private void simulateGeneration(LocalData ld) {
            foreach(StagData data in StagData.allStags) {
                ld.activeStags.Add(data);//all of 'em
                ld.opened.Add(data.name, false);
                //vanilla stags don't use this list, otherwise ["Dirtmouth"]=true
            }
            ld.activeStags.Sort((a, b) => a.positionNumber.CompareTo(b.positionNumber));
        }

        private void earlySceneChange(Scene arg0, Scene arg1) {
            if(StagData.dataByRoom.TryGetValue(arg1.name, out StagData data)) {
                if(data.isActive(localData) && !data.isVanilla) {
                    GameObject stag = GameObject.Instantiate(data.leftSide ? stagPrefabLeft : stagPrefabRight, data.stagPosition, Quaternion.identity);
                    stag.name = "Stag";
                    stag.FindGameObjectInChildren("Prompt Marker Trv").transform.position = new Vector3(data.transitionPosition.x, data.transitionPosition.y + 3.8f, 0);
                    stag.SetActive(true);
                    stag.FindGameObjectInChildren("Travel Range").GetComponent<BoxCollider2D>().offset = new Vector2((data.transitionPosition.x - data.stagPosition.x) * (data.leftSide ? 1 : -1), data.transitionPosition.y - data.stagPosition.y - 0.8f);
                    GameObject.Instantiate(bellPrefab, data.bellPosition, Quaternion.identity).SetActive(true);
                    GameObject tp = GameObject.Instantiate(data.leftSide ? transitionPrefabLeft : transitionPrefabRight, data.transitionPosition, Quaternion.identity);
                    tp.name = "door_stagExit";
                    tp.SetActive(true);
                }
                if(!data.isActive(localData) && data.isVanilla) {
                    foreach((string scene, string objectName) in new (string, string)[] {
                        ("Room_Town_Stag_Station", "Station Bell"),
                        ("RestingGrounds_09", "Ruins Lever")
                    }) {
                        if(data.name == scene) {
                            GameObject.Find(objectName).SetActive(false);//make sure lever rando stag levers are accounted for
                        }
                    }
                }
            }
        }

        private void lateSceneChange(On.GameManager.orig_OnNextLevelReady orig, GameManager self) {
            orig(self);
            if(StagData.dataByRoom.TryGetValue(self.sceneName, out StagData data)) {
                if(data.isActive(localData)) {
                    foreach(string toDelete in data.objectsToRemove) {
                        GameObject.Find(toDelete).GetComponent<SpriteRenderer>().enabled = false;//make sure it's sprite not mesh?
                    }
                    //so far every test has been around 0.6-0.7 ish so 1.5 should be plenty, but watch out for custom stags near doorways
                    if((HeroController.instance.transform.position - GameObject.Find("door_stagExit").transform.position).magnitude < 1.5f) {
                        foreach(string toDelete in data.enemiesToRemove) {
                            GameObject.Find(toDelete).SetActive(false);
                        }
                    }
                }
            }
        }

        private void editFsm(On.PlayMakerFSM.orig_OnEnable orig, PlayMakerFSM self) {
            orig(self);
            if(self.FsmName == "Stag Bell") {
                if(StagData.dataByRoom.TryGetValue(self.gameObject.scene.name, out StagData data)) {
                    if(data.isVanilla && !data.isActive(localData)) {
                        self.gameObject.SetActive(false);//double check this for stagnest
                    }
                    if(!data.isVanilla && data.isActive(localData)) {
                        self.GetValidState("Get Price").AddCustomAction(() => { self.FsmVariables.GetFsmInt("Toll Cost").Value = data.cost; });
                        FsmState init = self.GetValidState("Init");
                        init.RemoveAction(5);
                        init.AddAction(new StagOpenedBoolTest(data.name, "OPENED", "UNOPENED", localData));
                        FsmState yes = self.GetValidState("Yes");
                        yes.RemoveAction(0);
                        yes.InsertCustomAction(() => { localData.opened[data.name] = true; }, 0);
                    }
                }
            }
            else if(self.FsmName == "Stag Control") {
                if(StagData.dataByRoom.TryGetValue(self.gameObject.scene.name, out StagData data) && !data.isVanilla && data.isActive(localData)) {
                    if(ModHooks.GetMod("QoL") is Mod) {
                        QolStagArrive(self);
                    }
                    self.FsmVariables.GetFsmInt("Station Position Number").Value = data.positionNumber - 1;

                    FsmState init = self.GetValidState("Init");
                    init.Actions[3].Enabled = false;//Find Stag Grate

                    FsmState checkIfOpened = self.GetValidState("Check if Station Opened");
                    checkIfOpened.RemoveAction(0);
                    checkIfOpened.AddAction(new StagOpenedBoolTest(data.name, "OPENED", "CLOSED", localData));

                    FsmState openGrate = self.GetValidState("Open Grate");
                    openGrate.RemoveAction(0);
                    openGrate.InsertCustomAction(() => { localData.opened[data.name] = true; }, 0);
                    openGrate.GetFirstActionOfType<Tk2dPlayAnimationWithEvents>().Enabled = false;//Grate_disappear animation
                    openGrate.ChangeTransition("FINISHED", "Arrive Pause");//skip Remove Grate

                    FsmState stationOpened = self.GetValidState("Station Opened");
                    stationOpened.GetFirstActionOfType<DestroyObject>().Enabled = false;//Destroy Grate
                }

                //FsmState "Remember 3" hardcoded openedStagNest after >8 stags

                FsmState checkResult = self.GetValidState("Check Result");
                foreach(StagData activeStag in localData.activeStags.Where(s => !s.isVanilla)) {
                    string msName = $"MoreStags {activeStag.name}";
                    FsmState otherStag = self.AddState(msName);
                    otherStag.AddAction(new SetIntValue() {
                        intVariable = self.FsmVariables.GetFsmInt("To Position"),
                        intValue = new FsmInt() { Value = activeStag.positionNumber - 1 },
                        everyFrame = false
                    });
                    otherStag.AddAction(new SetStringValue() {
                        stringVariable = self.FsmVariables.GetFsmString("To Scene"),
                        stringValue = new FsmString() { Value = activeStag.scene },
                        everyFrame = false
                    });
                    otherStag.AddTransition("FINISHED", "Current Location Check");

                    FsmEvent otherStagEvent = new(msName);
                    checkResult.AddTransition(msName, msName);
                }
            }
            else if(self.FsmName == "UI List Stag") {
                if(!self.TryGetState("MoreStags Flag1", out _)) {
                    createUiObjects(self.gameObject);
                    self.AddState("MoreStags Flag1");
                    addCustomStagFsmEdits(self);
                }
            }
            else if(self.FsmName == "move_stagmap_marker") {
                StagData markPosData = StagData.allStags.Find(data => data.name == Consts.MarkerNames(self.Fsm.FsmComponent.gameObject.name));
                if(markPosData != null) {
                    self.FsmVariables.GetFsmVector3("Marker Pos").Value = markPosData.markerPosition;
                }
            }
            else if(self.FsmName == "stag_map_piece") {
                self.FsmVariables.GetFsmBool("Always Active").Value = true;
            }
            else if(self.FsmName == "Conversation Control" && self.gameObject.name == "Stag") {
                self.GetValidState("Convo Choice").AddTransition("FINISHED", "Exhausted");
            }
            else if(self.gameObject.name == "UI List Stag" && self.FsmName == "ui_list") {
                self.GetValidState("Activate").InsertCustomAction(() => scrollStagMenu(self, "Initial Item"), 1);
                self.GetValidState("Update").InsertCustomAction(() => scrollStagMenu(self, "Current Item"), 0);
            }
            else if(self.gameObject.name == "Stag Map(Clone)" && self.FsmName == "Control") {
                //likely not needed but it responds to UI SELECTION MADE and sends CONTINUE
            }
        }

        private void createUiObjects(GameObject uiList) {
            uiGameobjectDict.Clear();
            Vector3 sourcePosition = new Vector3(-5.07f, 4.3f, 0);
            foreach(StagData data in localData.activeStags.Where(s => !s.isVanilla)) {
                Vector3 yOffset = new Vector3(0, data.positionNumber, 0);
                GameObject newUI = GameObject.Instantiate(uiPrefab, Vector3.zero, Quaternion.identity, uiList.transform);
                newUI.transform.localPosition = sourcePosition - yOffset;
                newUI.name = data.name;
                newUI.GetComponent<TextMeshPro>().SetText(data.name);
                newUI.LocateMyFSM("ui_list_item").FsmVariables.GetFsmInt("Item Number").Value = data.positionNumber;
                newUI.LocateMyFSM("ui_list_item").FsmVariables.GetFsmString("Selection Name").Value = $"MoreStags {data.name}";
                uiGameobjectDict.Add(data.name, new FsmOwnerDefault { GameObject = new FsmGameObject { Value = newUI }, OwnerOption = OwnerDefaultOption.SpecifyGameObject });
            }
        }

        private void addCustomStagFsmEdits(PlayMakerFSM self) {
            self.GetValidState("Init").InsertCustomAction(() => { self.FsmVariables.GetFsmInt("Items").Value = 11 + localData.activeStags.Where(stag => !stag.isVanilla && stag.isActive(localData)).Count(); }, 14);

            //edit vanilla translations
            FsmFloat spaceToMoveUp = self.FsmVariables.GetFsmFloat("Space to move up");
            FsmString decrementListNumber = new FsmString { Value = "DECREMENT LIST NUMBER" };
            foreach(FsmState vState in StagData.allStags.Where(s => s.isVanilla && s.name != "Dirtmouth").Select(st => self.GetValidState(Consts.UiStateNames(st.name, false)))) {
                int translateIndex = vState.Actions.OfType<Translate>().Count() + 2;
                foreach(StagData data in localData.activeStags.Where(s => !s.isVanilla)) {
                    vState.InsertAction(new Translate {
                        gameObject = uiGameobjectDict[data.name],
                        vector = new FsmVector3 { Value = Vector3.zero },
                        x = new FsmFloat { Value = 0 },
                        y = spaceToMoveUp,
                        z = new FsmFloat { Value = 0 },
                        space = Space.Self,
                        perSecond = false,
                        everyFrame = false,
                        lateUpdate = false,
                        fixedUpdate = false
                    }, translateIndex++);
                    vState.InsertAction(new SendEventByName {
                        eventTarget = new FsmEventTarget { target = FsmEventTarget.EventTarget.GameObject, gameObject = uiGameobjectDict[data.name] },
                        sendEvent = decrementListNumber,
                        delay = new FsmFloat { Value = 0 },
                        everyFrame = false
                    }, vState.Actions.Length - 1);
                }
            }

            //add new states
            bool firstCustom = true;
            FsmState lastInitial = self.GetValidState("Stagnest Initial?");
            foreach(StagData data in localData.activeStags.Where(s => !s.isVanilla)) {
                FsmState nameState = self.AddState($"MoreStags {data.name}?");
                FsmState initialState = self.AddState($"MoreStags {data.name} Initial?");
                nameState.AddTransition("FINISHED", initialState.Name);
                if(firstCustom) {
                    lastInitial.ChangeTransition("FINISHED", nameState.Name);
                    firstCustom = false;
                }
                else {
                    lastInitial.AddTransition("FINISHED", nameState.Name);
                }
                lastInitial = initialState;

                nameState.AddAction(new StagOpenedBoolTest(data.name, "FINISHED", "", localData));
                nameState.AddAction(new ActivateGameObject {
                    gameObject = uiGameobjectDict[data.name],
                    activate = new FsmBool { Value = false },
                    recursive = new FsmBool { Value = false },
                    resetOnExit = false,
                    everyFrame = false
                });
                bool foundSelf = false;
                foreach(StagData stagToTranslate in localData.activeStags.Where(s => !s.isVanilla)) {
                    if(stagToTranslate.name == data.name) {
                        foundSelf = true;
                        continue;
                    }
                    if(!foundSelf)
                        continue;
                    nameState.AddAction(new Translate {
                        gameObject = uiGameobjectDict[stagToTranslate.name],
                        vector = new FsmVector3 { Value = Vector3.zero },
                        x = new FsmFloat { Value = 0 },
                        y = spaceToMoveUp,
                        z = new FsmFloat { Value = 0 },
                        space = Space.Self,
                        perSecond = false,
                        everyFrame = false,
                        lateUpdate = false,
                        fixedUpdate = false
                    });
                    nameState.AddAction(new SendEventByName {
                        eventTarget = new FsmEventTarget { target = FsmEventTarget.EventTarget.GameObject, gameObject = uiGameobjectDict[stagToTranslate.name] },
                        sendEvent = decrementListNumber,
                        delay = new FsmFloat { Value = 0 },
                        everyFrame = false
                    });
                }
                nameState.AddAction(new IntOperator {
                    integer1 = self.FsmVariables.GetFsmInt("Items"),
                    integer2 = new FsmInt { Value = 1 },
                    operation = IntOperator.Operation.Subtract,
                    storeResult = self.FsmVariables.GetFsmInt("Items"),
                    everyFrame = false
                });

                initialState.AddAction(new IntCompare {
                    integer1 = self.FsmVariables.GetFsmInt("Stag Position"),
                    integer2 = new FsmInt { Value = data.positionNumber },
                    lessThan = FsmEvent.GetFsmEvent("FINISHED"),
                    greaterThan = FsmEvent.GetFsmEvent("FINISHED"),
                    everyFrame = false
                });
                initialState.AddAction(new GetFsmInt {
                    gameObject = uiGameobjectDict[data.name],
                    fsmName = new FsmString { Value = "ui_list_item" },
                    variableName = new FsmString { Value = "Item Number" },
                    storeValue = self.FsmVariables.GetFsmInt("Initial Item"),
                    everyFrame = false
                });
            }
            lastInitial.AddTransition("FINISHED", "Set Initial Item");
        }

        private void scrollStagMenu(PlayMakerFSM self, string itemName) {
            int openedCount = self.FsmVariables.GetFsmInt("Items").Value;
            if(openedCount > 11) {
                int currentSelection = self.FsmVariables.GetFsmInt(itemName).Value;
                float toTranslate = (currentSelection - 1f) * (openedCount - 11f) / (openedCount - 1f);
                self.gameObject.transform.localPosition = new Vector3(-2, 0.52f + toTranslate * 0.7908f, -1);
            }
        }

        private void QolStagArrive(PlayMakerFSM self) {
            if(QoL.Modules.SkipCutscenes.StagArrive) {
                self.GetValidState("Arrive Pause").GetFirstActionOfType<Wait>().Enabled = false;
                self.GetValidState("Activate").GetFirstActionOfType<Wait>().Enabled = false;
            }
        }
    }

    public class StagOpenedBoolTest: FsmStateAction {
        private LocalData localData;
        private FsmEvent isTrue;
        private FsmEvent isFalse;
        private string name;
        private bool hasTrue = false;
        private bool hasFalse = false;
        public StagOpenedBoolTest(string stagName, string trueEvent, string falseEvent, LocalData localData) {
            name = stagName;
            if(!string.IsNullOrEmpty(trueEvent)) {
                isTrue = FsmEvent.GetFsmEvent(trueEvent);
                hasTrue = true;
            }
            if(!string.IsNullOrEmpty(falseEvent)) {
                isFalse = FsmEvent.GetFsmEvent(falseEvent);
                hasFalse = true;
            }
            this.localData = localData;
        }
        public override void OnEnter() {
            if(localData.opened[name]) {
                if(hasTrue) {
                    base.Fsm.Event(isTrue);
                }
            }
            else {
                if(hasFalse) {
                    base.Fsm.Event(isFalse);
                }
            }
            Finish();
        }
    }
}

//--bugs--
//none known at this point

//--todo--
//add all the rando stuff
//      the menu with settings
//      the generation options
//      item and location definitions
//      make sure the stag locations grant items and the stag items open the locations
//      logic definitions (or at least filler data)
//populate the json with every location

//stag settings options and notes:
//dirtmouth is always active, stagnest is always active unless enemy pogos are on probably
//if enabled, you can choose:
//      distributed (either 1-2 per region or exclusion logic),
//      random, (no balancing just pure rng)
//      all locations (if possible to implement)
//include settings to remove cursed stags (godhome roof) from the pool
//setting to avoid vanilla stag locations (excluding dirtmouth/stagnest)
//consider "vanilla" support
//      stags not rando'd items/locations but still a different active set

//--menu plans--
//   Enabled (with either a description or a clearer name)
//   Selection (balanced/distributed, random, all)
//   Avoid vanilla / prefer custom
//   Disable/prevent cursed locations

//--costs and order and grouping--
//   Crossroads - 50
//   East Greenpath - 130
//   West Greenpath - 140
//   Canyon - 250
//   Upper Fungal - 120
//   Lower Fungal - 130
//   Gardens - 200
//   West City - 200
//   East City - 300
//   Waterways - 300
//   Peak - 250
//   Grounds - 170
//   Edge - 350
//   Hive - 350
//   Deepnest - 250
//   Basin - 300
//   Abyss - 400
//   Cliffs - 170
//   Palace - 450
//   Godhome - 450