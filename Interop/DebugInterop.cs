using System.Linq;
using System.Reflection;
using MonoMod.RuntimeDetour;

namespace MoreStags {
    public class DebugInterop {
        private static Hook stagHook;

        public static void HookDebug() {
            MethodInfo methodToHook = typeof(DebugMod.BindableFunctions).GetMethod(nameof(DebugMod.BindableFunctions.AllStags));
            stagHook = new Hook(methodToHook, doAllStags);
        }

        public static void doAllStags() {
            PlayerData pd = PlayerData.instance;
            foreach(string pdBool in new string[] { "openedTownBuilding", "gladeDoorOpened", "troupeInTown"}) {
                pd.SetBool(pdBool, true);
            }

            if(MoreStags.localData.enabled) {
                foreach(StagData data in MoreStags.localData.activeStags) {
                    if(data.isVanilla)
                        pd.SetBool(Consts.OpenedNames[data.name], true);
                    else
                        MoreStags.localData.opened[data.name] = true;
                }
                DebugMod.Console.AddLine("Unlocked all active stags");
            }
            else {
                Consts.OpenedNames.Values.ToList().ForEach(s => pd.SetBool(s, true));
                DebugMod.Console.AddLine("Unlocked all stags");
            }
        }
    }
}
