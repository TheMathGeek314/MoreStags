namespace MoreStags {
    public class FStatsInterop {
        public static void Hook() {
            FStats.API.OnGenerateFile += gen => gen(new MStagTimeline());
        }
    }
}
