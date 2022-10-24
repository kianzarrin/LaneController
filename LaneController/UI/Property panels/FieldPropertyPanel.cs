namespace LaneConroller.UI;
using ColossalFramework.UI;
using KianCommons;
using LaneConroller.Tool;
using LaneConroller.UI.Editors;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public abstract class FieldPropertyPanel : EditorPropertyPanel {
    public event Action OnResetValue;
    public virtual void ResetValue() => OnResetValue?.Invoke();
}

public abstract class FieldPropertyPanel<ValueType> : FieldPropertyPanel {
    protected UITextField Field { get; set; }

    public event Action<ValueType> OnValueChanged;
    public event Action OnHover;
    public event Action OnLeave;

    protected abstract bool CanUseWheel { get; }
    public virtual bool UseWheel { get; set; }
    public virtual ValueType WheelStep { get; set; }
    public float FieldWidth
    {
        get => Field.width;
        set => Field.width = value;
    }

    private bool ValueProgress { get; set; } = false;
    public virtual ValueType Value
    {
        get
        {
            try
            {
                return (ValueType)TypeDescriptor.GetConverter(typeof(ValueType)).ConvertFromString(Field.text);
            }
            catch
            {
                return default;
            }
        }
        set
        {
            if (!ValueProgress)
            {
                ValueProgress = true;
                Field.text = GetString(value);
                OnValueChanged?.Invoke(value);
                ValueProgress = false;
            }
        }
    }

    public FieldPropertyPanel()
    {
        Field = Control.AddUIComponent<UITextField>();
        Field.atlas = EditorItemAtlas;
        Field.normalBgSprite = "TextFieldPanel";
        Field.hoveredBgSprite = "TextFieldPanelHovered";
        Field.focusedBgSprite = "TextFieldPanel";
        Field.selectionSprite = "EmptySprite";
        Field.allowFloats = true;
        Field.isInteractive = true;
        Field.enabled = true;
        Field.readOnly = false;
        Field.builtinKeyNavigation = true;
        Field.cursorWidth = 1;
        Field.cursorBlinkTime = 0.45f;
        Field.selectOnFocus = true;

        List<string> hint = new();
        if (LifeCycle.LaneControllerSettings.ShowToolTips) {
            hint.Add("Press delete to reset value");
            if (CanUseWheel) {
                hint.Add("Scroll the wheel to change\n" + "shift X10, Ctrl X0.1");
            }
        }
        Field.tooltip = hint.JoinLines();

        Field.eventMouseWheel += FieldMouseWheel;
        Field.eventTextSubmitted += FieldTextSubmitted;
        Field.eventMouseHover += FieldHover;
        Field.eventMouseLeave += FieldLeave;
        Field.textScale = 0.7f;
        Field.verticalAlignment = UIVerticalAlignment.Middle;
        Field.padding = new RectOffset(0, 0, 6, 0);
    }

    protected virtual string GetString(ValueType value) => value.ToString();
    protected abstract ValueType Increment(ValueType value, ValueType step, WheelMode mode);
    protected abstract ValueType Decrement(ValueType value, ValueType step, WheelMode mode);

    protected virtual void FieldTextSubmitted(UIComponent component, string value) => Value = Value;
    private void FieldHover(UIComponent component, UIMouseEventParameter eventParam) => OnHover?.Invoke();
    private void FieldLeave(UIComponent component, UIMouseEventParameter eventParam) => OnLeave?.Invoke();

    protected virtual void FieldMouseWheel(UIComponent component, UIMouseEventParameter eventParam)
    {
        if (CanUseWheel && UseWheel)
        {
            var mode = LaneConrollerTool.ShiftIsPressed
                ? WheelMode.High
                : LaneConrollerTool.CtrlIsPressed
                ? WheelMode.Low
                : WheelMode.Normal;
            if (eventParam.wheelDelta < 0)
                Value = Decrement(Value, WheelStep, mode);
            else
                Value = Increment(Value, WheelStep, mode);
        }
    }
    public void Edit() => Field.Focus();

    protected enum WheelMode
    {
        Normal,
        Low,
        High
    }
}
public abstract class ComparableFieldPropertyPanel<ValueType> : FieldPropertyPanel<ValueType>
    where ValueType : IComparable<ValueType>
{
    public ValueType MinValue { get; set; } = default;
    public ValueType MaxValue { get; set; } = default;
    public bool CheckMax { get; set; } = false;
    public bool CheckMin { get; set; } = false;

    public override ValueType Value
    {
        get => base.Value;
        set
        {
            var newValue = value;

            if (CheckMin && newValue.CompareTo(MinValue) < 0)
                newValue = MinValue;

            if (CheckMax && newValue.CompareTo(MaxValue) > 0)
                newValue = MaxValue;

            base.Value = newValue;
        }
    }
}
public class FloatPropertyPanel : ComparableFieldPropertyPanel<float>
{
    protected override bool CanUseWheel => true;
    protected override float Decrement(float value, float step, WheelMode mode)
    {
        step = mode == WheelMode.Low ? step / 10 : mode == WheelMode.High ? step * 10 : step;
        return (value - step).RoundToNearest(step);
    }
    protected override float Increment(float value, float step, WheelMode mode)
    {
        step = mode == WheelMode.Low ? step / 10 : mode == WheelMode.High ? step * 10 : step;
        return (value + step).RoundToNearest(step);
    }
    protected override string GetString(float value) => value.ToString("0.###");

    public void Init(string label) {
        Text = label;
        UseWheel = true;
        WheelStep = 0.1f;
        base.Init();
    }
}
public class StringPropertyPanel : FieldPropertyPanel<string>
{
    protected override bool CanUseWheel => false;

    protected override string Decrement(string value, string step, WheelMode mode) => throw new NotSupportedException();
    protected override string Increment(string value, string step, WheelMode mode) => throw new NotSupportedException();
}
public class IntPropertyPanel : ComparableFieldPropertyPanel<int>
{
    protected override bool CanUseWheel => true;

    protected override int Decrement(int value, int step, WheelMode mode) => value - step;
    protected override int Increment(int value, int step, WheelMode mode) => value + step;
}