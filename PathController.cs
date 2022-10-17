using CitiesHarmony.API;
using HarmonyLib;
using ICities;
using System.Reflection;
using PathController.Util;
using UnityEngine;
using System;
using KianCommons;

namespace PathController
{
    public class PathController : IUserMod
    {

        public static string StaticName { get; } = "Path Manager Extended";
#if DEBUG
        public static string StaticFullName => $"{StaticName} Beta {Version}";
#else
        public static string StaticFullName => $"{StaticName} {Version}";
#endif

        public static Version Version => Assembly.GetExecutingAssembly().GetName().Version;
        public string Name => StaticFullName;
        public string Description => "Adjust lane paths to your liking.";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => Patcher.PatchAll());
        }

        public void OnDisabled()
        {
            LoadTool.Release();
            if (HarmonyHelper.IsHarmonyInstalled) Patcher.UnpatchAll();
        }

        public void OnSettingsUI(UIHelperBase helper)
        {
            Util.Settings.OnSettingsUI(helper);
        }
    }

    public static class Patcher
    {
        private const string HarmonyId = "pathmanager.Harmony2";

        private static bool patched = false;

        public static void PatchAll()
        {
            if (patched) return;

            Log.Debug("[PathManager] Harmony 2: Patching...");

            patched = true;

            // Apply your patches here!
            // Harmony.DEBUG = true;
            var harmony = new Harmony("pathmanager.Harmony2");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public static void UnpatchAll()
        {
            if (!patched) return;

            var harmony = new Harmony(HarmonyId);
            harmony.UnpatchAll(HarmonyId);

            patched = false;

            Log.Debug("[PathManager] Harmony 2: Unpatching...");
        }
    }

    public class LoadingExtension : LoadingExtensionBase
    {

        public override void OnLevelLoaded(LoadMode mode)
        {
            LoadTool.Load();
        }

        public override void OnLevelUnloading()
        {
            LoadTool.Release();
        }
    }

    public static class LoadTool
    {
        public static void Load()
        {
            Tool.PathManagerExtendedTool.Create();
            ToolsModifierControl.SetTool<DefaultTool>(); // disable tool.
        }
        public static void Release()
        {
            Tool.PathManagerExtendedTool.Remove();
        }
    }
}

