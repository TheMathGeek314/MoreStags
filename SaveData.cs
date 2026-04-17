using System.Collections.Generic;
using MenuChanger.Attributes;

namespace MoreStags {
    public class GlobalSettings {
        public bool Enabled = false;

        [MenuIgnore]
        public int Quantity = 11;
        [DynamicBound(nameof(Maximum), true)]
        [MenuRange(11, 115)]
        public int Minimum = 11;
        [DynamicBound(nameof(Minimum), false)]
        [MenuRange(11, 115)]
        public int Maximum = 115;
        
        public StagSelection Distribution = StagSelection.Balanced;
        public StagNestThreshold StagNestThreshold = StagNestThreshold.All;
        public bool PreferNonVanilla = false;
        public bool RemoveCursedLocations = false;
    }

    public class LocalData {
        public bool enabled = false;
        public Dictionary<string, bool> opened = new();
        public List<StagData> activeStags = new();
        public bool preserveStagLevers = false;
        public bool plandoOverride = false;
        public bool tramActive = false;
        public int tramBlueLakePosition = 1;
        public bool openedTram = false;
        public int threshold = 8;
    }

    public enum StagSelection {
        Balanced,
        Random
    }

    public enum StagNestThreshold {
        Half,
        Many,
        Most,
        All
    }

    public static class TramData {
        public static bool enteringTram = false;
        public static bool insideTram = false;
    }
}
