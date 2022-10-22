namespace PathController.Tool;
using KianCommons;
using PathController.Util;
using UnityEngine;

public class SelectLaneTool : SelectSegmentTool {
    public override ToolType Type => ToolType.SelectLane;
    public override bool ShowPanel => true;
    public LaneIdAndIndex HoveredLaneIdAndIndex { get; private set; }

    public override void OnUpdate() {
        base.OnUpdate();
        if (HoveredSegmentId == Tool.SegmentInstance.SegmentId &&
            HoveredSegmentId.ToSegment().GetClosestLanePosition(
                PathControllerTool.MouseWorldPosition, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All,
                out var hitPos, out uint laneId, out var laneIndex, out _)) {
            HoveredLaneIdAndIndex = new(laneId, laneIndex);
        } else {
            HoveredLaneIdAndIndex = default;
        }
    }

    public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
        if (Tool.IsSegmentSelected(HoveredSegmentId)) {
            RenderUtil.RenderLaneOverlay(cameraInfo, HoveredLaneIdAndIndex, Color.yellow, true);
        } else {
            base.RenderOverlay(cameraInfo);
        }
    }

    public override void OnPrimaryMouseClicked(Event e) {
        if (Tool.IsSegmentSelected(HoveredSegmentId)) {
            Tool.SetLane(HoveredLaneIdAndIndex.LaneIndex);
            Tool.SetMode(ToolType.ModifyLane);
        } else {
            base.OnPrimaryMouseClicked(e);
        }
    }
}
