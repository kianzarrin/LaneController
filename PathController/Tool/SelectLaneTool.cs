namespace PathController.Tool;
using System;
using System.Linq;
using KianCommons;
using PathController.Util;
using UnityEngine;

public class SelectLaneTool : SelectSegmentTool {
    public override ToolType Type => ToolType.SelectLane;
    public override bool ShowPanel => true;
    public LaneIdAndIndex HoveredLaneIdAndIndex { get; private set; }

    protected override void Reset() {
        Log.Called();
        PreferNodeHover = default;
        HoveredNodeId = default;
        HoveredSegmentId = default;
        Tool.SetLane(-1);
    }


    public override void OnUpdate() {
        base.OnUpdate();
        if (Tool.IsSegmentSelected(HoveredSegmentId) &&
            HoveredSegmentId.ToSegment().GetClosestLanePosition(
                PathControllerTool.MouseWorldPosition, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All,
                out var hitPos, out uint laneId, out var laneIndex, out _)) {
            HoveredLaneIdAndIndex = new(laneId, laneIndex);
        } else {
            HoveredLaneIdAndIndex = default;
        }
    }

    public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
        State state = CalculateState();
        if (state == State.None)
            return;

        if(state == State.SelectLane) {
            Tool.RenderLanesOverlay(cameraInfo, HoveredLaneIdAndIndex.LaneIndex, Color.yellow);
            return;
        }

        bool select = state is State.SelectSegment or State.SelectSegmentMutli;
        bool multi = state is State.SelectSegmentMutli or State.SetActiveSegmentMulti;

        Color? color = state switch {
            State.SetActiveSegment => Colors.GameBlue,
            State.SetActiveSegmentMulti => Colors.GameBlue,
            State.SelectSegment => Color.white,
            State.SelectSegmentMutli => Color.white,
            State.Deselect => Color.red,
            State.Illigal => Colors.OrangeWeb,
            _ => null,
        };

        if (color.HasValue) {
            RenderUtil.RenderSegmentOverlay(cameraInfo, HoveredSegmentId, color.Value);
            if (multi) {
                foreach (ushort segmentId in TraverseUtil.GetSimilarSegmentsBetweenJunctions(HoveredSegmentId)) {
                    RenderUtil.RenderSegmentOverlay(cameraInfo, segmentId, color.Value);
                }
            }
        }

        if (select) {
            foreach (ushort segmentId in Tool.SelectedSegmentIds) {
                RenderUtil.RenderSegmentOverlay(cameraInfo, segmentId, Colors.GameBlue);
            }
        }
    }

    public override void OnPrimaryMouseClicked(Event e) {
        Log.Called();
        State state = CalculateState();
        switch (state) {
            case State.None:
                return;
            case State.SelectLane:
                Tool.SetLane(HoveredLaneIdAndIndex.LaneIndex);
                return;
            case State.SetActiveSegment:
                Tool.ActiveSegmentId = HoveredSegmentId;
                return;
            case State.SetActiveSegmentMulti:
                Tool.ActiveSegmentId = HoveredSegmentId;
                SelectMulti();
                return;
            case State.SelectSegment:
                Tool.SelectSegment(HoveredSegmentId);
                return;
            case State.SelectSegmentMutli:
                SelectMulti();
                return;
            case State.Deselect:
                Tool.DeselectSegment(HoveredSegmentId);
                return;
        }

        void SelectMulti() {
            foreach (ushort segmentId in TraverseUtil.GetSimilarSegmentsBetweenJunctions(HoveredSegmentId)) {
                Tool.SelectSegment(segmentId);
            }
        }
    }

    public override void OnSecondaryMouseClicked() {
        Tool.ActiveSegmentId = 0;
        base.OnSecondaryMouseClicked();
    }

    /// <summary>
    /// click => select new segment*
    /// control click => add segment to selection** or deselect segment
    /// shift click => select all segments between junctions.
    /// control + shift click => add all segments between junctions to selection
    /// * first selected segment is active segment
    /// ** to add segment to selection NetInfo must match.
    /// </summary>
    private enum State {
        None, // invalid hover
        SelectLane, // Click on already selected

        SetActiveSegment, // Click
        SetActiveSegmentMulti, // shift+Click

        SelectSegment, // Ctrl+Click on not selected with same info
        Illigal, // Ctrl+Click on not selected with different info
        Deselect, // Ctrl+Click on selected
        SelectSegmentMutli, // Ctrl+Shift+Click
    }

    private State CalculateState() {
        if (!HoverValid)
            return State.None;
        bool hoverSelected = Tool.IsSegmentSelected(HoveredSegmentId);
        if (!Helpers.ControlIsPressed) {
            if (Helpers.ShiftIsPressed)
                return State.SetActiveSegmentMulti;
            else if (!hoverSelected)
                return State.SetActiveSegment;
            else
                return State.SelectLane;
        } else if (HoverMatchesSelection()) {
            // don't restart selection.
            if (Helpers.ShiftIsPressed)
                return State.SelectSegmentMutli;
            else if (!hoverSelected)
                return State.SelectSegment;
            else
                return State.Deselect;
        } else {
            return State.Illigal;
        }
    }

    private bool HoverMatchesSelection() {
        return Tool.ActiveSegmentId == 0 ||
            Tool.ActiveSegmentId.ToSegment().Info == HoveredSegmentId.ToSegment().Info;
    }
}
