namespace ModsCommon.UI;
using ColossalFramework.UI;
using System;
using UnityEngine;

public abstract class HeaderButton : ButtonBase {
    public override int ButtonSize => 25;
}

[Flags]
public enum HeaderButtonState {
    Main = 1,
    Additional = 2,
    //Auto = Main | Additional,
}
public interface IHeaderButtonInfo {
    public event MouseEventHandler ClickedEvent;

    public HeaderButton Button { get; }

    public void AddButton(UIComponent parent);
    public void RemoveButton();
    public HeaderButtonState State { get; }
    public bool Visible { get; set; }
}
public class HeaderButtonInfo<TypeButton> : IHeaderButtonInfo
    where TypeButton : HeaderButton {
    public event MouseEventHandler ClickedEvent {
        add => Button.eventClicked += value;
        remove => Button.eventClicked -= value;
    }

    HeaderButton IHeaderButtonInfo.Button => Button;
    public TypeButton Button { get; }

    public HeaderButtonState State { get; }
    private Action OnClick { get; }

    public bool Visible { get; set; } = true;
    public bool Enable {
        get => Button.isEnabled;
        set => Button.isEnabled = value;
    }
    private HeaderButtonInfo(HeaderButtonState state) {
        State = state;
        Button = new GameObject(typeof(TypeButton).Name).AddComponent<TypeButton>();
    }

    public void AddButton(UIComponent parent) {
        RemoveButton();
        parent.AttachUIComponent(Button.gameObject);
        Button.transform.parent = parent.cachedTransform;
    }

    public void RemoveButton() {
        Button.parent?.RemoveUIComponent(Button);
        Button.transform.parent = null;
    }
}