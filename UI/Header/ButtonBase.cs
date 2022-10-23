using ColossalFramework;
using ColossalFramework.UI;
using KianCommons;
using KianCommons.UI;
using System;
using UnityEngine;

namespace ModsCommon.UI;
public abstract class ButtonBase : MultyAtlasUIButton {
    internal const string BG_NORMAL = "header_bg_normal";
    internal const string BG_HOVERED = "header_bg_hovered";
    internal const string BG_PRESSED = "header_bg_pressed";
    public string AtlasName => $"{GetType().Name}_rev" + typeof(HeaderButton).VersionOf();

    public SavedInputKey Hotkey;

    public abstract string SpriteName { get; }
    public abstract int ButtonSize { get; }
    public abstract string Title { get; }
    public abstract bool ShowText { get; } 

    public string GetText() {
        string ret = "" + Title;
        if (Hotkey != null) ret += $" ( {Hotkey} )";
        return ret;
    }

    public void RefreshText() {
        if (ShowText) {
            tooltip = GetText();
        } else {
            text = GetText();
        }
    }
    protected override void OnVisibilityChanged() => RefreshText();

    public override void Awake() {
        try {
            base.Awake();
            size = new Vector2(ButtonSize, ButtonSize);
            canFocus = false;
            name = GetType().Name;
            playAudioEvents = true;
        } catch (Exception ex) { Log.Exception(ex); }
    }

    public override void Start() {
        try {
            base.Start();
            disabledFgSprite = pressedFgSprite = hoveredFgSprite = normalFgSprite = SpriteName;
            pressedBgSprite = BG_PRESSED;
            string[] spriteNames = {BG_NORMAL,BG_HOVERED,BG_PRESSED,SpriteName};
            atlasBackground = atlas = TextureUtil.GetAtlasOrNull(AtlasName) ??
                TextureUtil.CreateTextureAtlas(AtlasName, spriteNames);
        } catch (Exception ex) { Log.Exception(ex); }
    }

    public override void OnDestroy() {
        base.OnDestroy();
        this.SetAllDeclaredFieldsToNull();
    }
}