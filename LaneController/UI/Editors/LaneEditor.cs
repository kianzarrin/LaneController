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
using System.Security.AccessControl;
using ModsCommon.UI;

namespace LaneConroller.UI.Editors
{
    public class LaneEditor : BaseEditor<LaneItem, CustomLane, LaneIcons>
    {
        public override string Name => "Lane Editor";
        public override string SelectionMessage => "Select lane to edit it.";

        private FloatPropertyPanel ShiftField , HeightField/*, StartField , EndField*/;

        private Vector3PropertyPanel A, B, C, D;

        private Vector3PropertyPanel GetDeltaControlPoint(int i) {
            return i switch {
                0 => A,
                1 => B,
                2 => C,
                3 => D,
                _ => throw new ArgumentException("i:" +i),
            };
        }

        private Vector3PropertyPanel[] DeltaControlPoints(int i) => new Vector3PropertyPanel[] { A, B, C, D };

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

                A = SettingsPanel.AddUIComponent<Vector3PropertyPanel>();
                A.Init("Start Position");
                B = SettingsPanel.AddUIComponent<Vector3PropertyPanel>();
                B.Init("Control Point 1");
                C = SettingsPanel.AddUIComponent<Vector3PropertyPanel>();
                C.Init("Control Point 2");
                D = SettingsPanel.AddUIComponent<Vector3PropertyPanel>();
                D.Init("End Position");

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
            ShiftField. OnResetValue += PositionField_OnResetValue;
            HeightField.OnResetValue += HeightField_OnResetValue;

            A.OnValueChanged += DeltaControlPoints_OnValueChanged;
            B.OnValueChanged += DeltaControlPoints_OnValueChanged;
            C.OnValueChanged += DeltaControlPoints_OnValueChanged;
            D.OnValueChanged += DeltaControlPoints_OnValueChanged;
            A.OnResetValue += DeltaControlPoints_OnResetValue;
            B.OnResetValue += DeltaControlPoints_OnResetValue;
            C.OnResetValue += DeltaControlPoints_OnResetValue;
            D.OnResetValue += DeltaControlPoints_OnResetValue;
        }

        public void RemoveEvents() {
            ShiftField.OnValueChanged -= PositionField_OnValueChanged;
            HeightField.OnValueChanged -= HeightField_OnValueChanged;
            ShiftField.OnResetValue -= PositionField_OnResetValue;
            HeightField.OnResetValue -= HeightField_OnResetValue;

            A.OnValueChanged -= DeltaControlPoints_OnValueChanged;
            B.OnValueChanged -= DeltaControlPoints_OnValueChanged;
            C.OnValueChanged -= DeltaControlPoints_OnValueChanged;
            D.OnValueChanged -= DeltaControlPoints_OnValueChanged;
            A.OnResetValue -= DeltaControlPoints_OnResetValue;
            B.OnResetValue -= DeltaControlPoints_OnResetValue;
            C.OnResetValue -= DeltaControlPoints_OnResetValue;
            D.OnResetValue -= DeltaControlPoints_OnResetValue;
        }

        public void PullValues() {
            ShiftField.Value = EditObject.Shift;
            HeightField.Value = EditObject.VShift;

            A.Value = EditObject.DeltaControlPoints.a;
            B.Value = EditObject.DeltaControlPoints.b;
            C.Value = EditObject.DeltaControlPoints.c;
            D.Value = EditObject.DeltaControlPoints.d;
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

        private void DeltaControlPoints_OnValueChanged(Vector3 value) {
            Log.Called();
            foreach (var customLane in IterateSelectedLanes()) {
                for(int i = 0; i < 4; ++i) {
                    customLane.DeltaControlPoints.ControlPoint(i) = GetDeltaControlPoint(i).Value;
                }
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

        private void DeltaControlPoints_OnResetValue() {
            Log.Called();
            RemoveEvents();
            for (int i = 0; i < 4; ++i) {
                EditObject.DeltaControlPoints.ControlPoint(i) = GetDeltaControlPoint(i).Value = default; 
            }
            AddEvents();
        }

        public override void OnDestroy() {
            base.OnDestroy();
            RemoveEvents();
        }
    }

}
