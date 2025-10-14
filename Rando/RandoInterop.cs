using Modding;
using System.IO;
using System.Linq;
using System.Reflection;
using ItemChanger;
using ItemChanger.Tags;

namespace MoreStags {
    internal static class RandoInterop {
        public static void Hook() {
            RandoMenuPage.Hook();
            RequestModifier.Hook();
            LogicAdder.Hook();

            DefineLocations();
            DefineItems();

            //if(ModHooks.GetMod("CondensedSpoilerLogger") is Mod)
            //  todo

            if(ModHooks.GetMod("RandoSettingsManager") is Mod)
              RSMInterop.Hook();
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
                tag.Properties["WorldMapLocation"] = (data.scene, data.bellPosition.x, data.bellPosition.y);
                Finder.DefineCustomLocation(stagLoc);
            }
        }

        public static void DefineItems() {
            foreach(StagData data in StagData.allStags.Where(stag => !stag.isVanilla)) {
                MStagItem stagItem = new(data.name);
                Finder.DefineCustomItem(stagItem);
            }
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
