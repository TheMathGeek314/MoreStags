using System;
using System.Reflection;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using UnityEngine;

namespace MoreStags {
    public static class PatchGodhome {
        private static MethodInfo setUnlocked;
        public static void Setup() {
            setUnlocked = typeof(BossSequenceBindingsDoor).GetMethod("SetUnlocked", BindingFlags.NonPublic | BindingFlags.Instance);
            On.BossSequenceBindingsDoor.Start += blueDoorStart;
            IL.BossStatueCompletionStates.Start += bossStatueStart;
            On.BossStatueCompletionStates.Start += bossCageStart;
        }

        private static void blueDoorStart(On.BossSequenceBindingsDoor.orig_Start orig, BossSequenceBindingsDoor self) {
            if(MoreStags.IsRandoSave() && MoreStags.localData.enabled && StagData.dataByRoom["GG_Blue_Room"].isActive(MoreStags.localData)) {
                if(MoreStags.localData.opened["Blue Room"]) {
                    setUnlocked.Invoke(self, [true, false]);
                    return;
                }
            }
            orig(self);
        }

        private static void bossStatueStart(ILContext il) {
            ILCursor cursor = new ILCursor(il).Goto(0);
            cursor.GotoNext(i => i.MatchCall<BossStatueCompletionStates.State>("SetActive"),
                            i => i.MatchRet());
            cursor.GotoNext(i => i.MatchRet());
            cursor.Emit(OpCodes.Ldarg_0);
            cursor.EmitDelegate<Action<BossStatueCompletionStates>>(self => {
                if(self.gameObject.name == "Statue") {
                    foreach(Transform child in self.gameObject.transform) {
                        child.gameObject.SetActive(false);
                    }
                }
            });
        }

        private static void bossCageStart(On.BossStatueCompletionStates.orig_Start orig, BossStatueCompletionStates self) {
            if(MoreStags.IsRandoSave() && MoreStags.localData.enabled && StagData.dataByRoom["GG_Workshop"].isActive(MoreStags.localData)) {
                if(self.gameObject.name == "Knight Statue Cage" && MoreStags.localData.opened["Hall of Gods"]) {
                    self.gameObject.SetActive(false);
                    return;
                }
            }
            orig(self);
        }
    }
}
