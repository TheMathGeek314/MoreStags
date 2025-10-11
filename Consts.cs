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
    }
}
