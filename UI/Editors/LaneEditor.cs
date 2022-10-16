using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathController.Tool;
using PathController.Util;
using ColossalFramework.Math;
using UnityEngine;

namespace PathController.UI.Editors
{
    public class LaneEditor : BaseEditor<LaneItem, LaneData, LaneIcons>
    {

        public override string Name => "Lane Editor";
        public override string SelectionMessage => "Select a lane to edit it.";

        private FloatPropertyPanel PositionField { get; set; }

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
                        HoverItem.Object,
                        (SelectItem?.Object.LaneID != HoverItem.Object.LaneID) ? Color.yellow : Colors.Add(Color.red, 0.15f),
                        true);
                } else {
                    RenderUtil.RenderLaneOverlay(cameraInfo, HoverItem.Object, Color.yellow, true);
                }
            }

            if (IsSelectItem)
            {
                if (HoverItem?.Object.LaneID != SelectItem.Object.LaneID)
                    RenderUtil.RenderLaneOverlay(cameraInfo, SelectItem.Object, Color.magenta, true);
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
            PositionField.Text = "Position";
            PositionField.UseWheel = true;
            PositionField.WheelStep = 0.1f;
            PositionField.Init();
            PositionField.Value = EditObject.Position;
            PositionField.OnValueChanged += PositionChanged;
        }
        protected override void OnObjectUpdate()
        {
            Log.Debug("LaneEditor.OnObjectUpdate()");
            PositionField.OnValueChanged -= PositionChanged;
            PositionField.Value = EditObject.Position;
            PositionField.OnValueChanged += PositionChanged;
        }
        private void PositionChanged(float value)
        {
            EditObject.Position = value;
            NetUtil.UpdateLanePosition(ToolInstance.SegmentInstance.SegmentID, EditObject, ref ToolInstance.SegmentInstance.Segment);
        }
    }

    public class LaneItem : DynamicButton<LaneData, LaneIcons>
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

            switch (Object.LaneInfo().m_laneType)
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
                    switch (Object.LaneInfo().m_vehicleType)
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
                            if (MathUtil.OnesCount32(Convert.ToUInt32(Object.LaneInfo().m_vehicleType)) > 1)
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

            Icon.DirectionType = Object.LaneInfo().m_finalDirection;
            Icon.SetDirectionToolTip(Util.Settings.ShowToolTip ? Object.LaneInfo().m_finalDirection.ToString() : string.Empty);
            Icon.SetLaneToolTip(Util.Settings.ShowToolTip ? Object.LaneInfo().m_vehicleType.ToString() : string.Empty);
        }
    }
}
