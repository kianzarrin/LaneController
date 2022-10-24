using ColossalFramework;
using KianCommons;
using KianCommons.UI;
using UnityEngine;

namespace LaneConroller.LifeCycle {
    public static class LaneControllerSettings {
        public static string SettingsFile => "LaneConrollerMod";

        static LaneControllerSettings() {
            if (GameSettings.FindSettingsFileByName(SettingsFile) == null)
                GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = SettingsFile } });
        }

        public static SavedBool ShowToolTips =
            new SavedBool("ShowToolTips", SettingsFile, true, true);

        public static readonly SavedInputKey ActivationShortcut = new SavedInputKey(
            name: "ActivationShortcut",
            fileName: SettingsFile,
            def: SavedInputKey.Encode(KeyCode.P, control: true, shift: false, alt: false),
            autoUpdate: true);


        public static void OnSettingsUI(UIHelper helper) {
            helper.AddSavedToggle("Show Tooltips", ShowToolTips);

            var keymappingsPanel = helper.AddKeymappingsPanel();
            keymappingsPanel.AddKeymapping("Hotkey", ActivationShortcut);
        }
    }
}
