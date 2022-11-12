namespace LaneConroller.Tool;

using KianCommons;
using LaneConroller.CustomData;
using LaneConroller.UI.Editors;
using System.Linq;
using UnityEngine;
using static LaneConrollerTool;
using static ToolBase;

public class ModifyLaneTool : BaseTool {
    public override ToolType Type => ToolType.ModifyLane;
    public override bool ShowPanel => true;

    public override void SimulationStep() {
        base.SimulationStep();
        if (Tool.BezierMarker != null) {
            RaycastInput input = new(MouseRay, MouseRayLength) { m_ignoreTerrain = false };
            RayCast(input, out RaycastOutput output);
            if (Tool.BezierMarker.Drag(output.m_hitPos)) {
                if(Panel?.CurrentEditor is LaneEditor laneEditor) {
                    laneEditor.PullValues();
                }
            }
        }
    }

    public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
        base.RenderOverlay(cameraInfo);
        Tool.BezierMarker?.RenderOverlay(cameraInfo, Color.green);
    }
}
