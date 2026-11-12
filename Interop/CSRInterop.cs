using ConnectionSettingsRando;

namespace MoreStags {
    internal static class CSRInterop {
        public static void Hook() {
            CSR.Register(
                MoreStags.instance.GetName(),
                () => MoreStags.Settings,
                s => SettingsRandomizer.CopyTo(s, MoreStags.Settings)
            );
        }
    }
}