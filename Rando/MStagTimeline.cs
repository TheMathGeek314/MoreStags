using Modding;
using System.Collections.Generic;
using System.Linq;
using FStats;
using FStats.StatControllers;
using FStats.Util;

namespace MoreStags {
    public class MStagTimeline: StatController {
        public static MStagTimeline instance;
        public Dictionary<string, float> MStagObtainTimeline = new();

        public MStagTimeline() {
            instance = this;
        }

        internal static readonly Dictionary<string, string> BoolNames = new() {
            [nameof(PlayerData.openedTown)] = "Dirtmouth",
            [nameof(PlayerData.openedCrossroads)] = "Forgotten Crossroads",
            [nameof(PlayerData.openedGreenpath)] = "Greenpath",
            [nameof(PlayerData.openedFungalWastes)] = "Queen's Station",
            [nameof(PlayerData.openedRoyalGardens)] = "Queen's Gardens",
            [nameof(PlayerData.openedRuins1)] = "City Storerooms",
            [nameof(PlayerData.openedRuins2)] = "King's Station",
            [nameof(PlayerData.openedRestingGrounds)] = "Resting Grounds",
            [nameof(PlayerData.openedDeepnest)] = "Distant Village",
            [nameof(PlayerData.openedHiddenStation)] = "Hidden Station",
            [nameof(PlayerData.openedStagNest)] = "Stag Nest",
        };

        public override void Initialize() {
            ModHooks.SetPlayerBoolHook += RecordPdBool;
        }

        private bool RecordPdBool(string name, bool orig) {
            if(orig && BoolNames.ContainsKey(name) && !MStagObtainTimeline.ContainsKey(name))
                MStagObtainTimeline[name] = FStatsMod.LS.Get<Common>().CountedTime;
            return orig;
        }

        internal void Record(string stag) {
            if(!MStagObtainTimeline.ContainsKey(stag)) {
                MStagObtainTimeline[stag] = FStatsMod.LS.Get<Common>().CountedTime;
            }
        }

        public override IEnumerable<DisplayInfo> GetDisplayInfos() {
            List<string> Lines = MStagObtainTimeline.OrderBy(kvp => MStagObtainTimeline[kvp.Key])
                 .Select(kvp => $"{(BoolNames.ContainsKey(kvp.Key) ? BoolNames[kvp.Key] : kvp.Key)}: {kvp.Value.PlaytimeHHMMSS()}")
                 .ToList();
            yield return new() {
                Title = "More Stags Timeline",
                MainStat = FStatsMod.LS.Get<Common>().TotalTimeString,
                StatColumns = Columnize(Lines),
                Priority = -47_999
            };
        }

        private List<string> Columnize(List<string> rows) {
            int columnCount = (rows.Count + 9) / 10;
            List<string> list = [];
            for(int i = 0; i < columnCount; i++)
                list.Add(string.Join("\n", rows.Slice(i, columnCount)));
            return list;
        }
    }
}
