using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using LaneConroller.UI;
using System.Collections.Generic;
using UnityEngine;
using UIUtils = LaneConroller.UI.UIUtils;

namespace LaneConroller.LifeCycle {
    public static class Settings {
        public static string SettingsFile => $"{nameof(LaneConrollerMod)}";

        private static CustomUITabstrip TabStrip { get; set; }
        private static List<UIPanel> TabPanels { get; set; }

        public static SavedBool ShowToolTip { get; } = new SavedBool(nameof(ShowToolTip), SettingsFile, true, true);
        static Settings() {
            if (GameSettings.FindSettingsFileByName(SettingsFile) == null)
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = SettingsFile } });
        }

        public static void OnSettingsUI(UIHelperBase helper) {
            var mainPanel = (helper as UIHelper).self as UIScrollablePanel;
            mainPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 25);
            CreateTabStrip(mainPanel);

            var generalTab = CreateTab(mainPanel, "General");
            generalTab.AddGroup(LaneConrollerMod.Instance.ModName);
            //AddLanguage(generalTab);
            AddGeneral(generalTab);
            //AddGrouping(generalTab);
            //AddNotifications(generalTab);

            //var shortcutTab = CreateTab(mainPanel, Localize.Settings_ShortcutsAndModifiersTab);
            //AddKeyMapping(shortcutTab);

            //var backupTab = CreateTab(mainPanel, Localize.Settings_BackupTab);
            //if (SceneManager.GetActiveScene().name is string scene && (scene != "MainMenu" && scene != "IntroScreen"))
            //  AddBackupMarking(backupTab);
            //AddBackupTemplates(backupTab);

            //var supportTab = CreateTab(mainPanel, Localize.Settings_SupportTab);
            //AddSupport(supportTab);
            //AddAccess(supportTab);
        }

        private static void CreateTabStrip(UIScrollablePanel mainPanel) {
            TabPanels = new List<UIPanel>();

            TabStrip = mainPanel.AddUIComponent<CustomUITabstrip>();
            TabStrip.eventSelectedIndexChanged += TabStripSelectedIndexChanged;
            TabStrip.selectedIndex = -1;
        }

        private static void TabStripSelectedIndexChanged(UIComponent component, int index) {
            if (index >= 0 && TabPanels.Count > index) {
                foreach (var tab in TabPanels)
                    tab.isVisible = false;

                TabPanels[index].isVisible = true;
            }
        }

        private static UIHelper CreateTab(UIScrollablePanel mainPanel, string name) {
            TabStrip.AddTab(name, 1.25f);

            var tabPanel = mainPanel.AddUIComponent<UIPanel>();
            tabPanel.size = new Vector2(mainPanel.width - mainPanel.scrollPadding.horizontal, mainPanel.height - mainPanel.scrollPadding.vertical - 2 * mainPanel.autoLayoutPadding.vertical - TabStrip.height);
            tabPanel.isVisible = false;
            TabPanels.Add(tabPanel);

            var panel = tabPanel.AddUIComponent<UIScrollablePanel>();
            UIUtils.AddScrollbar(tabPanel, panel);
            panel.verticalScrollbar.eventVisibilityChanged += ScrollbarVisibilityChanged;

            panel.size = tabPanel.size;
            panel.relativePosition = Vector2.zero;
            panel.autoLayout = true;
            panel.autoLayoutDirection = LayoutDirection.Vertical;
            panel.clipChildren = true;
            panel.scrollWheelDirection = UIOrientation.Vertical;

            return new UIHelper(panel);

            void ScrollbarVisibilityChanged(UIComponent component, bool value) {
                panel.width = tabPanel.width - (panel.verticalScrollbar.isVisible ? panel.verticalScrollbar.width : 0);
            }
        }

        private static void AddGeneral(UIHelperBase helper) {
            UIHelper group = helper.AddGroup("Display and Usage") as UIHelper;

            //AddDistanceSetting(group);
            AddShowToolTipsSetting(group);
            //AddCheckboxPanel(group, Localize.Settings_ShowDeleteWarnings, DeleteWarnings, DeleteWarningsType, new string[] { Localize.Settings_ShowDeleteWarningsAlways, Localize.Settings_ShowDeleteWarningsOnlyDependences });
            //AddQuickRuleSetup(group);
        }

        private static void AddShowToolTipsSetting(UIHelper group) {
            var showCheckBox = group.AddCheckbox("Show Tooltips", ShowToolTip, OnShowToolTipsChanged) as UICheckBox;

            void OnShowToolTipsChanged(bool show) => ShowToolTip.value = show;
        }
    }
}
