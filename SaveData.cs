using System.Collections.Generic;

namespace MoreStags {
    public class GlobalSettings {
        public bool Enabled = false;
        public StagSelection Selection = StagSelection.Balanced;
        public bool PreferNonVanilla = false;
        public bool RemoveCursedLocations = false;
    }

    public class LocalData {
        public Dictionary<string, bool> opened = new();
        public List<StagData> activeStags = new();
        public bool preserveStagLevers = false;
    }

    public enum StagSelection {
        Balanced,
        Random,
        All
    }
}
