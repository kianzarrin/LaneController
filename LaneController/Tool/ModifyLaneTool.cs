namespace LaneConroller.Tool;

using KianCommons;
using LaneConroller.CustomData;
using LaneConroller.UI.Editors;
using LaneConroller.UI.Marker;
using LaneController.UI.Gizmos;
using System.Linq;
using UnityEngine;
using static LaneConrollerTool;
using static ToolBase;

public class ModifyLaneTool : BaseTool {
    public override ToolType Type => ToolType.ModifyLane;
    public override bool ShowPanel => true;

    public void Drag() {
        Assertion.InMainThread();
        if (Tool.BezierMarker != null) {
            RaycastInput input = new(MouseRay, MouseRayLength) { m_ignoreTerrain = false };
            RayCast(input, out RaycastOutput output);
            if (Tool.BezierMarker.Drag(output.m_hitPos)) {
                if (Panel?.CurrentEditor is LaneEditor laneEditor) {
                    laneEditor.PullValues();
                }
            }
        }
    }

    public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
        base.RenderOverlay(cameraInfo);
        int hoveredIndex = (Panel.CurrentEditor as LaneEditor)?.HoveredControlPointIndex ??  -1;
        Tool.BezierMarker?.RenderOverlay(cameraInfo, Color.green, hoverIndex: hoveredIndex);
    }

    public override void OnUpdate() {
        base.OnUpdate();
        Tool.BezierMarker?.OnUpdate();
        Drag();
    }

    public override string OnToolInfo() {
        var markers = Tool.BezierMarker?.controlMarkers;
        if (markers != null) {
            foreach (ControlPointMarker c in markers) {
                if (c?.Gizmo?.KeyTyping is KeyTyping keyTyping) {
                    if (!keyTyping.registeredString.IsNullorEmpty()) {
                        return keyTyping.registeredString;
                    } else if (c.Gizmo.distance != 0) {
                        return c.Gizmo.distance.ToString("R");
                    } else {
                        return "type meters to move";

                    }
                }
            }
            if(!Helpers.ControlIsPressed)
                return "Hold control for vertical shift.";
        }
        return null;
    }
}
