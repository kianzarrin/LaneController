namespace LaneConroller.Tool;
using System;
using System.Linq;
using KianCommons;
using LaneConroller.Util;
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
                LaneConrollerTool.MouseWorldPosition, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All,
                out var hitPos, out uint laneId, out var laneIndex, out _)) {
            HoveredLaneIdAndIndex = new(laneId, laneIndex);
        } else {
            HoveredLaneIdAndIndex = default;
        }
    }

    public override string OnToolInfo() {
        State state = CalculateState();
        switch (state) {
            case State.SelectLane:
                return $"Click => edit lane[{HoveredLaneIdAndIndex.LaneIndex}]\n" +
                    $"Ctrl+Click => Deselect segment #{HoveredSegmentId}";
            case State.Deselect:
                return $"Deselect Segment #{HoveredSegmentId}";

            case State.SetActiveSegment:
                return $"Segment #{HoveredSegmentId}\n" +
                "Click => Select this segment\n" +
                "Shift+Click => Select until intersection\n"+
                "Ctrl+Click => Add to selection\n" +
                "Shift+Click => Add to selection until intersection";
            case State.SetActiveSegmentMulti:
                return "Select until intersection.\n";
            case State.Illigal:
                return "Cannot add mismatching segment to selection.\n" + "Segment must match selection.";
            case State.SelectSegment:
                return $"Add Segment #{HoveredSegmentId} to selection";
            case State.SelectSegmentMutli:
                return "Add to selection until intersection";

            case State.None:
            default:
                return "Select a lane to edit\n" + "Or select more segments";
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

        
        Color? color = state switch {
            State.SetActiveSegment => Colors.GameBlue,
            State.SetActiveSegmentMulti => Colors.GameBlue,
            State.SelectSegment => Color.green,
            State.SelectSegmentMutli => Color.green,
            State.Deselect => Colors.OrangeWeb,
            State.Illigal => Color.red,
            _ => null,
        };

        bool multi = state is State.SelectSegmentMutli or State.SetActiveSegmentMulti;
        if (color.HasValue) {
            RenderUtil.RenderSegmentOverlay(cameraInfo, HoveredSegmentId, color.Value);
            if (multi) {
                foreach (ushort segmentId in TraverseUtil.GetSimilarSegmentsBetweenJunctions(HoveredSegmentId)) {
                    RenderUtil.RenderSegmentOverlay(cameraInfo, segmentId, color.Value);
                }
            }
        }

        if (state is State.SelectSegment or State.SelectSegmentMutli or State.Deselect or State.Illigal) {
            foreach (ushort segmentId in Tool.SelectedSegmentIds) {
                if (segmentId != HoveredSegmentId) {
                    RenderUtil.RenderSegmentOverlay(cameraInfo, segmentId, Colors.GameBlue);
                }
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
