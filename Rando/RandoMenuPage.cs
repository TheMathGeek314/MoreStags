using MenuChanger;
using MenuChanger.Extensions;
using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using RandomizerMod.Menu;
using static RandomizerMod.Localization;

namespace MoreStags {
    public class RandoMenuPage {
        internal MenuPage MsRandoPage;
        internal MenuElementFactory<GlobalSettings> msMEF;
        internal VerticalItemPanel msVIP;

        internal SmallButton JumpToMsButton;

        internal static RandoMenuPage Instance { get; private set; }

        public static void OnExitMenu() {
            Instance = null;
        }

        public static void Hook() {
            RandomizerMenuAPI.AddMenuPage(ConstructMenu, HandleButton);
            MenuChangerMod.OnExitMainMenu += OnExitMenu;
        }

        private static bool HandleButton(MenuPage landingPage, out SmallButton button) {
            button = Instance.JumpToMsButton;
            return true;
        }

        private void SetTopLevelButtonColor() {
            if(JumpToMsButton != null) {
                JumpToMsButton.Text.color = MoreStags.Settings.Enabled ? Colors.TRUE_COLOR : Colors.DEFAULT_COLOR;
            }
        }

        private static void ConstructMenu(MenuPage landingPage) => Instance = new(landingPage);

        private RandoMenuPage(MenuPage landingPage) {
            MsRandoPage = new MenuPage(Localize("MoreStags"), landingPage);
            msMEF = new(MsRandoPage, MoreStags.Settings);
            msVIP = new(MsRandoPage, new(0, 300), 75f, true, msMEF.Elements);
            Localize(msMEF);
            foreach(IValueElement e in msMEF.Elements) {
                e.SelfChanged += obj => SetTopLevelButtonColor();
            }

            JumpToMsButton = new(landingPage, Localize("MoreStags"));
            JumpToMsButton.AddHideAndShowEvent(landingPage, MsRandoPage);
            SetTopLevelButtonColor();
        }

        internal void ResetMenu(GlobalSettings settings) {
            msMEF.SetMenuValues(settings);
            SetTopLevelButtonColor();
        }
    }
}
