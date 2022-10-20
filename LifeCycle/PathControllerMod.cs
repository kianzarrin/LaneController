using CitiesHarmony.API;
using KianCommons;
using PathController.Manager;

namespace PathController.LifeCycle {
    public class PathControllerMod : LifeCycleBase {
        public const string HARMONY_ID = "cs.pathcontroller";

        public override string ModName => "Path Controller";
        public override string Description => "Adjust lane paths.";

        public override void Start() {
            HarmonyHelper.DoOnHarmonyReady(() =>
                HarmonyUtil.InstallHarmony(HARMONY_ID));
        }

        public override void OnSettingsUI(UIHelper helper) => Settings.OnSettingsUI(helper);

        public override void Load() {
            PathControllerManager.Ensure();
            Tool.PathControllerTool.Create();
        }

        public override void UnLoad() {
            Tool.PathControllerTool.Remove();
            PathControllerManager.Release();
        }

        public override void End() {
            HarmonyUtil.UninstallHarmony(HARMONY_ID);
        }
    }
}

