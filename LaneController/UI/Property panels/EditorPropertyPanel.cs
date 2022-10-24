namespace LaneConroller.UI; 
using ColossalFramework.UI;
using LaneConroller.UI.Editors;
using System.Drawing;
using UnityEngine;

public abstract class EditorPropertyPanel : EditorItem {
    private UILabel Label { get; set; }
    protected UIPanel Control { get; set; }

    public string Text {
        get => Label.text;
        set => Label.text = value;
    }

    public EditorPropertyPanel() {
        Label = AddUIComponent<UILabel>();
        Label.textScale = 0.8f;

        Control = AddUIComponent<UIPanel>();
        //Control.autoLayout = true;
        Control.autoLayoutDirection = LayoutDirection.Horizontal;
        Control.autoLayoutStart = LayoutStart.TopRight;
        Control.autoLayoutPadding = new RectOffset(5, 0, 0, 0);

        Control.eventSizeChanged += ControlSizeChanged;
    }

    protected override void OnSizeChanged() {
        base.OnSizeChanged();

        Label.relativePosition = new Vector2(0, (height - Label.height) / 2);
        Control.size = size;
    }

    private void ControlSizeChanged(UIComponent component, Vector2 value) => RefreshContent();
    protected void RefreshContent() {
        Control.autoLayout = true;
        Control.autoLayout = false;

        foreach (var item in Control.components)
            item.relativePosition = new Vector2(item.relativePosition.x, (Control.size.y - item.size.y) / 2);
    }
}
