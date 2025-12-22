using ItemChanger;
using ItemChanger.Tags;
using ItemChanger.UIDefs;

namespace MoreStags {
    public class MStagItem: AbstractItem {
        public string rawName;

        public MStagItem(string name) {
            rawName = name;
            this.name = RandoInterop.nameToLocation(name);
            InteropTag tag = RandoInterop.AddTag(this);
            tag.Properties["PinSprite"] = new EmbeddedSprite("stagpin");
            UIDef = new MsgUIDef {
                name = new BoxedString(RandoInterop.clean(this.name)),
                shopDesc = new LanguageString("UI", "ITEMCHANGER_STAG_DESC"),
                sprite = new ItemChangerSprite("ShopIcons.StagPin")
            };
            AddTag(CurseTag());
        }

        public override void GiveImmediate(GiveInfo info) {
            MoreStags.localData.opened[rawName] = true;
            PlayerData.instance.IncrementInt(nameof(PlayerData.stationsOpened));
        }

        private InteropTag CurseTag() {
            InteropTag tag = new();
            tag.Properties["CanMimic"] = new BoxedBool(true);
            tag.Properties["MimicNames"] = new string[] {
                "Stag - Grand Bellway",
                "Stag - Blasted Steps",
                "Stag - Bone Bottom",
                "Stag - Weavenest",
                "Stag - Shellwood",
                "Stag - Bilewater"
            };
            tag.Properties["Weight"] = 0.2f;
            tag.Message = "CurseData";
            return tag;
        }
    }
}
