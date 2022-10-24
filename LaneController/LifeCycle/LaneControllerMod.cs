using CitiesHarmony.API;
using KianCommons;
using LaneConroller.Manager;

namespace LaneConroller.LifeCycle {
    public class LaneConrollerMod : LifeCycleBase {
        public const string HARMONY_ID = "cs.lanecontroller";

        public override string ModName => "Lane Controller";
        public override string Description => "Adjust lane paths.";

        public override void Start() {
            HarmonyHelper.DoOnHarmonyReady(() =>
                HarmonyUtil.InstallHarmony(HARMONY_ID));
        }

        public override void OnSettingsUI(UIHelper helper) => LaneControllerSettings.OnSettingsUI(helper);

        public override void Load() {
            LaneConrollerManager.Ensure();
            Tool.LaneConrollerTool.Create();
        }

        public override void HotReload() {
            SerializableDataExtension.Load();
            Load();
        }

        public override void UnLoad() {
            Tool.LaneConrollerTool.Remove();
            LaneConrollerManager.Release();
        }

        public override void End() {
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
        }
    }
}

