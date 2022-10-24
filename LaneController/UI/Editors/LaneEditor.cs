using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LaneConroller.Tool;
using LaneConroller.Util;
using ColossalFramework.Math;
using UnityEngine;
using KianCommons;
using LaneConroller.CustomData;
using LaneConroller.Manager;

namespace LaneConroller.UI.Editors
{
    public class LaneEditor : BaseEditor<LaneItem, CustomLane, LaneIcons>
    {
        public override string Name => "Lane Editor";
        public override string SelectionMessage => "Select lane to edit it.";

        private FloatPropertyPanel ShiftField , HeightField/*, StartField , EndField*/;



        public IEnumerable<CustomLane> IterateOtherSelectedLanes() {
            if(EditObject == null)yield break;
            foreach(ushort segmentId in ToolInstance.SelectedSegmentIds) {
                int laneIndex = EditObject.Index;
                if (segmentId != EditObject.LaneIdAndIndex.SegmentId) {
                    uint laneId = NetUtil.GetLaneId(segmentId, laneIndex);
                    yield return LaneConrollerManager.Instance.GetOrCreateLane(new(laneId, laneIndex));
                }
            }
        }

        public IEnumerable<CustomLane> IterateSelectedLanes() {
            if (EditObject == null) yield break;
            foreach (ushort segmentId in ToolInstance.SelectedSegmentIds) {
                int laneIndex = EditObject.Index;
                uint laneId = NetUtil.GetLaneId(segmentId, laneIndex);
                yield return LaneConrollerManager.Instance.GetOrCreateLane(new(laneId, laneIndex));
            }
        }

        protected override void FillItems() {
            Log.Debug("LaneEditor.FillItems() called");
            foreach (var lane in ToolInstance.SegmentInstance.Lanes)
                AddItem(lane);
        }

        public override void Render(RenderManager.CameraInfo cameraInfo)
        {
            if (IsHoverItem) {
                ToolInstance.RenderLanesOverlay(cameraInfo, HoverItem.Object.Index, Color.yellow);
            }

            if (IsSelectItem) {
                ToolInstance.RenderLanesOverlay(cameraInfo, SelectItem.Object.Index, Color.magenta);
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
            foreach (var customLane in IterateSelectedLanes()) {
                customLane.Shift = value;
                customLane.QueueUpdate();
            }
        }

        private void HeightField_OnValueChanged(float value) {
            Log.Called();
            foreach (var customLane in IterateSelectedLanes()) {
                customLane.VShift = value;
                customLane.QueueUpdate();
            }
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
