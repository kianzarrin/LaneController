namespace PathController.Tool;
using UnityEngine;
using static PathControllerTool;
using static ToolBase;

public class ModifyLaneTool : BaseTool {
    public override ToolType Type => ToolType.ModifyLane;
    public override bool ShowPanel => true;

    public override void SimulationStep() {
        base.SimulationStep();
        if (Tool.BezierMarker != null) {
            RaycastInput input = new(MouseRay, MouseRayLength) { m_ignoreTerrain = false };
            RayCast(input, out RaycastOutput output);
            Tool.BezierMarker.Drag(output.m_hitPos);
        }
    }

    public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
        base.RenderOverlay(cameraInfo);
        Tool.BezierMarker?.RenderOverlay(cameraInfo, Color.green);
    }
}
