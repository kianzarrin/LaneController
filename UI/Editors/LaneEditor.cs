using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathController.Tool;
using PathController.Util;
using ColossalFramework.Math;
using UnityEngine;
using KianCommons;
using PathController.UI.Data;

namespace PathController.UI.Editors
{
    public class LaneEditor : BaseEditor<LaneItem, LaneDTO, LaneIcons>
    {

        public override string Name => "Lane Editor";
        public override string SelectionMessage => "Select a lane to edit it.";

        private FloatPropertyPanel PositionField , HeightField, StartField , EndField;
        private FloatPropertyPanel[] FloatFields = new FloatPropertyPanel[0];

        protected override void FillItems()
        {
            Log.Debug("LaneEditor.FillItems() called");
            foreach (var lane in ToolInstance.SegmentInstance.Lanes)
                AddItem(lane);
        }

        public override void Render(RenderManager.CameraInfo cameraInfo)
        {
            if (IsHoverItem) {
                if (IsSelectItem) {
                    RenderUtil.RenderLaneOverlay(
                        cameraInfo,
                        HoverItem.Object.LaneIdAndIndex,
                        (SelectItem?.Object.LaneID != HoverItem.Object.LaneID) ? Color.yellow : Colors.Add(Color.red, 0.15f),
                        true);
                } else {
                    RenderUtil.RenderLaneOverlay(cameraInfo, HoverItem.Object.LaneIdAndIndex, Color.yellow, true);
                }
            }

            if (IsSelectItem)
            {
                if (HoverItem?.Object.LaneID != SelectItem.Object.LaneID)
                    RenderUtil.RenderLaneOverlay(cameraInfo, SelectItem.Object.LaneIdAndIndex, Color.magenta, true);
            }
        }

        protected override void OnObjectSelect()
        {
            Log.Debug("LaneEditor.OnObjectSelect()");
            //ToolInstance.OnLaneUISelect(SelectItem.Object.Index);
            HideEmptySelected();
            var componets = SettingsPanel.components.ToArray();
            foreach (var item in componets)
                DeleteUIComponent(item);

            PositionField = SettingsPanel.AddUIComponent<FloatPropertyPanel>();
            PositionField.Init("Position");
            HeightField = SettingsPanel.AddUIComponent<FloatPropertyPanel>();
            HeightField.Init( "Height");
            FloatFields = new[] { PositionField, HeightField};

            PullValues();
            AddEvents();
        }


        protected override void OnObjectUpdate()
        {
            Log.Debug("LaneEditor.OnObjectUpdate()");
            RemoveEvents();
            PullValues();
            AddEvents();
        }

        public void AddEvents() {
            RemoveEvents();
            PositionField.OnValueChanged += PositionField_OnValueChanged;
            HeightField.OnValueChanged += HeightField_OnValueChanged;
            PositionField.OnResetValue += PositionField_OnResetValue;
            HeightField.OnResetValue += HeightField_OnResetValue;
        }
        public void RemoveEvents() {
            PositionField.OnValueChanged -= PositionField_OnValueChanged;
            HeightField.OnValueChanged -= HeightField_OnValueChanged;
            PositionField.OnResetValue -= PositionField_OnResetValue;
            HeightField.OnResetValue -= HeightField_OnResetValue;
        }

        private void PullValues() {
            PositionField.Value = EditObject.Position;
            HeightField.Value = EditObject.Height;
        }

        private void PositionField_OnValueChanged(float value) {
            Log.Called();
            EditObject.Position = value;
            EditObject.UpdateLaneBezier();
        }
        private void PositionField_OnResetValue() {
            Log.Called();
            EditObject.Shift = 0;
            PositionField.Value = EditObject.Position;
        }

        private void HeightField_OnValueChanged(float value) {
            Log.Called();
            EditObject.Height = value;
        }

        private void HeightField_OnResetValue() {
            Log.Called();
            EditObject.VShift = 0;
            HeightField.Value = EditObject.Height;
            EditObject.UpdateLaneBezier();
        }

        public override void OnDestroy() {
            base.OnDestroy();
            RemoveEvents();
        }
    }

    public class LaneItem : DynamicButton<LaneDTO, LaneIcons>
    {
        public override string DeleteCaptionDescription => "Delete";
        public override string DeleteMessageDescription => "Delete2";
        public override void Init() => Init(true, false);

        protected override void OnObjectSet() => SetIcon();
        public override void Refresh()
        {
            base.Refresh();
            SetIcon();
        }

        private void SetIcon()
        {
            if (!ShowIcon)
                return;

            switch (Object.LaneInfo.m_laneType)
            {
                case NetInfo.LaneType.Pedestrian:
                    Icon.LaneType = BaseEditor.LaneType.Pedestrian;
                    break;
                case NetInfo.LaneType.Parking:
                    Icon.LaneType = BaseEditor.LaneType.Parking;
                    break;
                case NetInfo.LaneType.None:
                    Icon.LaneType = BaseEditor.LaneType.None;
                    break;
                case NetInfo.LaneType.Vehicle:
                    switch (Object.LaneInfo.m_vehicleType)
                    {
                        case VehicleInfo.VehicleType.Bicycle:
                            Icon.LaneType = BaseEditor.LaneType.Bicycle;
                            break;
                        case VehicleInfo.VehicleType.Car:
                            Icon.LaneType = BaseEditor.LaneType.Car;
                            break;
                        case VehicleInfo.VehicleType.Tram:
                            Icon.LaneType = BaseEditor.LaneType.Tram;
                            break;
                        case VehicleInfo.VehicleType.CableCar:
                            Icon.LaneType = BaseEditor.LaneType.Cablecar;
                            break;
                        case VehicleInfo.VehicleType.Monorail:
                            Icon.LaneType = BaseEditor.LaneType.Monorail;
                            break;
                        case VehicleInfo.VehicleType.Metro:
                            Icon.LaneType = BaseEditor.LaneType.Metro;
                            break;
                        case VehicleInfo.VehicleType.Trolleybus:
                            Icon.LaneType = BaseEditor.LaneType.Trolleybus;
                            break;
                        case VehicleInfo.VehicleType.Train:
                            Icon.LaneType = BaseEditor.LaneType.Train;
                            break;
                        default:
                            if (MathUtil.OnesCount32(Convert.ToUInt32(Object.LaneInfo.m_vehicleType)) > 1)
                            {
                                Icon.LaneType = BaseEditor.LaneType.Multiple;
                            }
                            else
                            {
                                Icon.LaneType = BaseEditor.LaneType.None;
                            }
                            break;
                    }
                    break;
                default:
                    Icon.LaneType = BaseEditor.LaneType.None;
                    break;
            }

            Icon.DirectionType = Object.LaneInfo.m_finalDirection;
            Icon.SetDirectionToolTip(Util.Settings.ShowToolTip ? Object.LaneInfo.m_finalDirection.ToString() : string.Empty);
            Icon.SetLaneToolTip(Util.Settings.ShowToolTip ? Object.LaneInfo.m_vehicleType.ToString() : string.Empty);
        }
    }
}
