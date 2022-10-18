using CitiesHarmony.API;
using HarmonyLib;
using ICities;
using System.Reflection;
using System;
using KianCommons;

namespace PathController.Manager {
    public class PathControllerMod : IUserMod {

        public static string StaticName { get; } = "Path Controller";
#if DEBUG
        public static string StaticFullName => $"{StaticName} Beta {Version}";
#else
        public static string StaticFullName => $"{StaticName} {Version}";
#endif

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Name => StaticFullName;
        public string Description => "Adjust lane paths.";

        public void OnEnabled() {
            HarmonyHelper.DoOnHarmonyReady(Patcher.PatchAll);
            if (LoadingManager.instance.m_loadingComplete) {
                LoadTool.Load();
            }
        }

        public void OnDisabled() {
            LoadTool.Release();
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public void OnSettingsUI(UIHelperBase helper) {
            Util.Settings.OnSettingsUI(helper);
        }
    }

    public static class Patcher {
        private const string HarmonyId = "pathcontroller.Harmony2";

        private static bool patched = false;

        public static void PatchAll() {
            if (patched) return;

            Log.Debug("Harmony 2: Patching...");

            patched = true;

            // Apply your patches here!
            // Harmony.DEBUG = true;
            var harmony = new Harmony(HarmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void UnpatchAll() {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);

            patched = false;

            Log.Debug("Harmony 2: Unpatching...");
        }
    }

    public class LoadingExtension : LoadingExtensionBase {

        public override void OnLevelLoaded(LoadMode mode) {
            LoadTool.Load();
        }

        public override void OnLevelUnloading() {
            LoadTool.Release();
        }
    }

    public static class LoadTool {
        public static void Load() {
            Tool.PathControllerExtendedTool.Create();
            ToolsModifierControl.SetTool<DefaultTool>(); // disable tool.
        }
        public static void Release() {
            Tool.PathControllerExtendedTool.Remove();
        }
    }
}
