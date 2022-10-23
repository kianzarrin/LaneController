using ColossalFramework;
using ColossalFramework.UI;
using KianCommons;
using PathController.Util;
using System;
using System.Linq;
using UnityEngine;

namespace ModsCommon.UI;
public abstract class HeaderButton : UIButton {
    internal const string BG_NORMAL = "header_bg_normal";
    internal const string BG_HOVERED = "header_bg_hovered";
    internal const string BG_PRESSED = "header_bg_pressed";
    internal const string BG_DISABLED = "header_bg_disabled";

    internal abstract string SpriteName { get; }

    public string AtlasName => $"{GetType().Name}_rev" + typeof(HeaderButton).VersionOf();
    public const int SIZE = 25;

    public virtual string Tooltip => null;

    public void RefreshToolTip() {
        try {
            if (Tooltip != null)
                tooltip = Tooltip;
            if (Hotkey == null || tooltip.Contains('('))
                return;
            tooltip += $" ( {Hotkey} )";
        } catch (Exception ex) { ex.Log(this.name); }
    }

    public SavedInputKey Hotkey;

    public override void Awake() {
        try {
            base.Awake();
            size = new Vector2(SIZE, SIZE);
            canFocus = false;
            name = GetType().Name;
            playAudioEvents = true;
            
        } catch (Exception ex) { Log.Exception(ex); }
    }

    public override void Start() {
        try {
            base.Start();
            tooltip = Tooltip;
            disabledFgSprite = pressedFgSprite = hoveredFgSprite = normalFgSprite = SpriteName;
            pressedBgSprite = BG_PRESSED;
            disabledBgSprite = BG_DISABLED;

            string[] spriteNames = new string[] {
                BG_NORMAL,
                BG_HOVERED,
                BG_PRESSED,
                SpriteName,
            };

            var atlas = TextureUtil.GetAtlas(AtlasName);
            if (atlas == UIView.GetAView().defaultAtlas) {
                atlas = TextureUtil.CreateTextureAtlas($"{SpriteName}.png", AtlasName, SIZE, SIZE, spriteNames);
            }

            this.atlas = atlas;
            RefreshToolTip();
        } catch (Exception ex) { Log.Exception(ex); }
    }


    public override void OnDestroy() {
        base.OnDestroy();
        this.SetAllDeclaredFieldsToNull();
    }



    public override void Start() {
        base.Start();
        Log.Info("GoToButton.Start() is called.");

        playAudioEvents = true;
        tooltip = "Go to network";

        string[] spriteNames = new string[]
        {
            GoToButtonBg,
            GoToButtonBgHovered,
            GoToButtonBgPressed,
            GotoIcon,
        };

        var atlas = TextureUtil.GetAtlas(AtlasName);
        if (atlas == UIView.GetAView().defaultAtlas) {
            atlas = TextureUtil.CreateTextureAtlas("goto.png", AtlasName, SIZE, SIZE, spriteNames);
        }

        this.atlas = atlas;

        hoveredBgSprite = GoToButtonBgHovered;
        pressedBgSprite = GoToButtonBgPressed;
        normalBgSprite = focusedBgSprite = disabledBgSprite = GoToButtonBg;
        normalFgSprite = focusedFgSprite = disabledFgSprite = hoveredFgSprite = pressedFgSprite = GotoIcon;

        Show();
        Unfocus();
        Invalidate();
        Log.Info("GoToButton created sucessfully.");
    }

    protected override void OnClick(UIMouseEventParameter p) {
        Log.Debug("ON CLICK CALLED");
        base.OnClick(p);
        GoToPanel.GoToPanel.Instance.Open(0);

    }


}