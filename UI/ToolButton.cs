using System;
using ColossalFramework.UI;
using UnityEngine;
using PathController.Util;
using PathController.Tool;
using KianCommons;

namespace PathController.UI
{
    public class PathManagerExtendedButton : UIButton
    {
        const string CONTAINING_PANEL_NAME = "RoadsOptionPanel";
        const string ButtonBg = "PathManagerButtonBg";
        const string ButtonBgActive = "PathManagerButtonBgActive";
        const string ButtonBgHovered = "PathManagerButtonBgHovered";
        const string Icon = "PathManagerIcon";
        const string IconActive = "PathManagerIconActive";
        const string IconHovered = "PathManagerIconHovered";
        const int buttonSize = 31;
        readonly static Vector2 buttonPosition = new Vector3(124, 38);
        public static string AtlasName = nameof(PathManagerExtendedButton);
        public static PathManagerExtendedButton Instance { get; private set; }

        static UIComponent GetContainingPanel()
        {
            var ret = UIUtils.FindComponent<UIComponent>(CONTAINING_PANEL_NAME, null, UIUtils.FindOptions.NameContains);
            return ret ?? throw new Exception($"Could not find {CONTAINING_PANEL_NAME}");
        }

        public override void Start()
        {
            Log.Debug("PathManagerExtendedButton.Start()");

            base.Start();
            name = nameof(PathManagerExtendedButton);
            playAudioEvents = true;

            if (!(UIUtils.FindComponent<UITabstrip>("ToolMode", GetContainingPanel(), UIUtils.FindOptions.None) is UITabstrip builtinTabstrip))
                return;

            string[] spriteNames = new string[]
            {
                ButtonBg,
                ButtonBgActive,
                ButtonBgHovered,
                Icon,
                IconActive,
                IconHovered
            };

            atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas)
            {
                atlas = TextureUtil.CreateTextureAtlas("sprites.png", AtlasName, buttonSize, buttonSize, spriteNames);
            }

            Deactivate();
            hoveredBgSprite = ButtonBgHovered;
            hoveredFgSprite = IconHovered;

            relativePosition = buttonPosition;
            size = new Vector2(buttonSize, buttonSize);
            Show();
            Unfocus();
            Invalidate();

            Instance = this;
        }

        public void Activate()
        {
            Log.Debug("PathManagerExtendedButton.Activate()");

            focusedBgSprite = ButtonBgActive;
            normalBgSprite = ButtonBgActive;
            pressedBgSprite = ButtonBgActive;
            disabledBgSprite = ButtonBgActive;
            normalFgSprite = IconActive;
            focusedFgSprite = IconActive;
            Invalidate();
        }
        public void Deactivate()
        {
            Log.Debug("PathManagerExtendedButton.Deactivate()");

            focusedBgSprite = ButtonBg;
            normalBgSprite = ButtonBg;
            pressedBgSprite = ButtonBg;
            disabledBgSprite = ButtonBg;
            normalFgSprite = Icon;
            focusedFgSprite = Icon;
            Invalidate();
        }

        public static PathManagerExtendedButton CreateButton()
        {
            Log.Debug("PathManagerExtendedButton.CreateButton()");
            return GetContainingPanel().AddUIComponent<PathManagerExtendedButton>();
        }
        public static void RemoveButton()
        {
            Log.Debug("PathManagerExtendedButton.RemoveButton()");

            if (Instance != null)
            {
                GetContainingPanel().RemoveUIComponent(Instance);
                Destroy(Instance);
                Instance = null;
            }
        }

        protected override void OnClick(UIMouseEventParameter p)
        {
            Log.Debug("PathManagerExtendedButton.OnClick()");

            base.OnClick(p);
            PathManagerExtendedTool.Instance.ToggleTool();
        }
        protected override void OnTooltipEnter(UIMouseEventParameter p)
        {
            tooltip = $"{PathControllerMod.StaticFullName} ({PathManagerExtendedTool.ActivationShortcut})";
            base.OnTooltipEnter(p);
        }
    }
}
