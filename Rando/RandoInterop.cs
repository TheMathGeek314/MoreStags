using Modding;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ItemChanger;
using ItemChanger.Internal;
using ItemChanger.Tags;
using RandomizerMod.Logging;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace MoreStags {
    internal static class RandoInterop {
        internal static bool UseAdditionalTimelines;

        public static void Hook() {
            RandoMenuPage.Hook();
            RequestModifier.Hook();
            LogicAdder.Hook();

            DefineLocations();
            DefineItems();

            RandoController.OnExportCompleted += RemovePlatforms;
            SettingsLog.AfterLogSettings += LogRandoSettings;

            if(ModHooks.GetMod("CondensedSpoilerLogger") is Mod)
                CSLInterop.Hook();

            if(ModHooks.GetMod("RandoSettingsManager") is Mod)
              RSMInterop.Hook();

            if(ModHooks.GetMod("AdditionalTimelines") is Mod) {
                UseAdditionalTimelines = true;
                SetupTimeline();
            }
            else {
                UseAdditionalTimelines = false;
            }
        }

        public static void DefineLocations() {
            Assembly assembly = Assembly.GetExecutingAssembly();
            string stagDataJson = assembly.GetManifestResourceNames().Single(str => str.EndsWith("StagData.json"));
            using Stream stagStream = assembly.GetManifestResourceStream(stagDataJson);
            foreach(StagData data in new ParseJson(stagStream).parseFile<StagData>()) {
                data.translate();
            }

            foreach(StagData data in StagData.allStags.Where(stag => !stag.isVanilla)) {
                MStagLocation stagLoc = new() {
                    name = nameToLocation(data.name),
                    sceneName = data.scene,
                    rawName = data.name,
                    cost = data.cost,
                    x = data.bellPosition.x,
                    y = data.bellPosition.y,
                    elevation = 0f,
                    managed = false,
                    forceShiny = false,
                    flingType = FlingType.Everywhere
                };
                InteropTag tag = AddTag(stagLoc);
                tag.Properties["PinSprite"] = new EmbeddedSprite("stagpin");
                tag.Properties["WorldMapLocation"] = data.pinInfo;

                Finder.DefineCustomLocation(stagLoc);
            }
        }

        public static void DefineItems() {
            foreach(StagData data in StagData.allStags.Where(stag => !stag.isVanilla)) {
                MStagItem stagItem = new(data.name);
                Finder.DefineCustomItem(stagItem);
            }
        }

        private static void RemovePlatforms(RandoController rc) {
            if(rc.ctx.itemPlacements.Any(placement => placement.Item.Name == "Stag-Lower_Tramway")) {
                List<IDeployer> deployers = Ref.Settings.Deployers;
                foreach(IDeployer deployer in deployers) {
                    if(ShouldRemoveDeployer(deployer)) {
                        Events.RemoveSceneChangeEdit(deployer.SceneName, deployer.OnSceneChange);
                    }
                }
                deployers.RemoveAll(ShouldRemoveDeployer);
            }
        }

        private static void LogRandoSettings(LogArguments args, TextWriter w) {
            w.WriteLine("Logging MoreStags settings:");
            w.WriteLine(JsonUtil.Serialize(MoreStags.Settings));
        }

        private static bool ShouldRemoveDeployer(IDeployer deployer) {
            return deployer is SmallPlatform platform && platform.SceneName == SceneNames.Abyss_17;
        }

        private static void SetupTimeline() {
            FStats.API.OnGenerateFile += gen => gen(new MStagTimeline());
        }

        internal static InteropTag AddTag(TaggableObject obj) {
            InteropTag tag = obj.GetOrAddTag<InteropTag>();
            tag.Message = "RandoSupplementalMetadata";
            tag.Properties["ModSource"] = MoreStags.instance.Name;
            return tag;
        }

        public static string clean(string name) {
            return name.Replace("_", " ").Replace("-", " - ");
        }

        public static string nameToLocation(string name) {
            return "Stag-" + name.Replace(" ", "_");
        }
    }
}
