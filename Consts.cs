using ItemChanger;
using System.Collections.Generic;

namespace MoreStags {
    public static class Consts {
        public static readonly Dictionary<string, string> OpenedNames = new() {
            { "Dirtmouth", "openedTown" },
            { "Crossroads", "openedCrossroads" },
            { "Greenpath", "openedGreenpath" },
            { "Fungal Wastes", "openedFungalWastes" },
            { "Royal Gardens", "openedRoyalGardens" },
            { "City 1", "openedRuins1" },
            { "Kings Station", "openedRuins2" },
            { "Resting Grounds", "openedRestingGrounds" },
            { "Deepnest", "openedDeepnest" },
            { "Hidden Station", "openedHiddenStation" },
            { "Stag Nest", "openedStagNest" }
        };

        public static string UiStateNames(string area, bool initial) {
            return area switch {
                "Dirtmouth" => "Town",
                "Fungal Wastes" => "Fungus",
                "Royal Gardens" => "Royal G",
                "City 1" => initial ? "City 1" : "City",
                "Kings Station" => "City 2",
                "Resting Grounds" => "Resting G",
                "Hidden Station" => initial ? "Hidden S" : "Hidden Station",
                "Stag Nest" => initial ? "Stagnest" : "Stag Nest",
                _ => area
            } + (initial ? " Initial?" : "?");
        }

        public static string MarkerNames(string name) {
            return name == "City Storerooms" ? "City 1" : name;
        }

        public static readonly Dictionary<string, string> LocationNames = new() {
            { "Dirtmouth", ItemNames.Dirtmouth_Stag },
            { "Crossroads", ItemNames.Crossroads_Stag },
            { "Greenpath", ItemNames.Greenpath_Stag },
            { "Fungal Wastes", ItemNames.Queens_Station_Stag },
            { "Royal Gardens", ItemNames.Queens_Gardens_Stag },
            { "City 1", ItemNames.City_Storerooms_Stag },
            { "Kings Station", ItemNames.Kings_Station_Stag },
            { "Resting Grounds", ItemNames.Resting_Grounds_Stag },
            { "Deepnest", ItemNames.Distant_Village_Stag },
            { "Hidden Station", ItemNames.Hidden_Station_Stag },
            { "Stag Nest", ItemNames.Stag_Nest_Stag }
        };

        public static readonly List<string> Regions = new() {
            "Crossroads",
            "Upper Greenpath",
            "Lower Greenpath",
            "Canyon",
            "Upper Fungal",
            "Lower Fungal",
            "Gardens",
            "West City",
            "East City",
            "Waterways",
            "Peak",
            "Grounds",
            "Edge",
            "Hive",
            "Deepnest",
            "Basin",
            "Abyss",
            "Cliffs",
            "Palace",
            "Godhome"
        };
    }
}
