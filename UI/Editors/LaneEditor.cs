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
using PathController.CustomData;

namespace PathController.UI.Editors
{
    public class LaneEditor : BaseEditor<LaneItem, CustomLane, LaneIcons>
    {
        public override string Name => "Lane Editor";
        public override string SelectionMessage => "Select a lane to edit it.";

        private FloatPropertyPanel ShiftField , HeightField/*, StartField , EndField*/;

        protected override void FillItems() {
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
            try {
                Log.Debug("LaneEditor.OnObjectSelect()");
                //ToolInstance.OnLaneUISelect(SelectItem.Object.Index);
                HideEmptySelected();
                var componets = SettingsPanel.components.ToArray();
                foreach (var item in componets)
                    DeleteUIComponent(item);

                ShiftField = SettingsPanel.AddUIComponent<FloatPropertyPanel>();
                ShiftField.Init("Horizontal Shift");
                HeightField = SettingsPanel.AddUIComponent<FloatPropertyPanel>();
                HeightField.Init("Vertical Shift");

                PullValues();
                AddEvents();
                ToolInstance.SetLane(EditObject.Index);
            } catch (Exception ex) { ex.Log(); }
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
            ShiftField.OnValueChanged += PositionField_OnValueChanged;
            HeightField.OnValueChanged += HeightField_OnValueChanged;
            ShiftField.OnResetValue += PositionField_OnResetValue;
            HeightField.OnResetValue += HeightField_OnResetValue;
        }
        public void RemoveEvents() {
            ShiftField.OnValueChanged -= PositionField_OnValueChanged;
            HeightField.OnValueChanged -= HeightField_OnValueChanged;
            ShiftField.OnResetValue -= PositionField_OnResetValue;
            HeightField.OnResetValue -= HeightField_OnResetValue;
        }

        private void PullValues() {
            ShiftField.Value = EditObject.Shift;
            HeightField.Value = EditObject.VShift;
        }

        private void PositionField_OnValueChanged(float value) {
            Log.Called();
            EditObject.Shift = value;
            EditObject.QueueUpdate();
        }

        private void HeightField_OnValueChanged(float value) {
            Log.Called();
            EditObject.VShift = value;
            EditObject.QueueUpdate();
        }

        private void PositionField_OnResetValue() {
            Log.Called();
            ShiftField.Value = EditObject.Shift = 0;
        }

        private void HeightField_OnResetValue() {
            Log.Called();
            HeightField.Value = EditObject.VShift = 0;
        }

        public override void OnDestroy() {
            base.OnDestroy();
            RemoveEvents();
        }
    }

}
