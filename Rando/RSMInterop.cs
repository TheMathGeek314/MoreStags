using RandoSettingsManager;
using RandoSettingsManager.SettingsManagement;
using RandoSettingsManager.SettingsManagement.Versioning;

namespace MoreStags {
    internal static class RSMInterop {
        public static void Hook() {
            RandoSettingsManagerMod.Instance.RegisterConnection(new MoreStagsSettingsProxy());
        }
    }

    internal class MoreStagsSettingsProxy: RandoSettingsProxy<GlobalSettings, string> {
        public override string ModKey => MoreStags.instance.GetName();

        public override VersioningPolicy<string> VersioningPolicy { get; } = new EqualityVersioningPolicy<string>(MoreStags.instance.GetVersion());

        public override void ReceiveSettings(GlobalSettings settings) {
            settings ??= new();
            RandoMenuPage.Instance.ResetMenu(settings);
        }

        public override bool TryProvideSettings(out GlobalSettings settings) {
            settings = MoreStags.Settings;
            return settings.Enabled;
        }
    }
}
