namespace LaneConroller.UI.Marker;

using KianCommons;
using LaneConroller.CustomData;
using LaneConroller.Manager;
using LaneConroller.Util;
using LaneController.UI.Gizmos;
using UnityEngine;

public class BezierMarker {
    public LaneIdAndIndex LaneIdAndIndex;
    public ControlPointMarker[] controlMarkers = new ControlPointMarker[4];

    internal bool Focused =>
        A.Hovered || A.Selected ||
        B.Hovered || B.Selected ||
        C.Hovered || C.Selected ||
        D.Hovered || D.Selected;

    public CustomLane CustomLane => LaneConrollerManager.Instance.GetOrCreateLane(LaneIdAndIndex);
    public ControlPointMarker A => controlMarkers[0];
    public ControlPointMarker B => controlMarkers[1];
    public ControlPointMarker C => controlMarkers[2];
    public ControlPointMarker D => controlMarkers[3];


    public BezierMarker(LaneIdAndIndex laneIdAndIndex) {
        LaneIdAndIndex = laneIdAndIndex;
        var bezier = laneIdAndIndex.Lane.m_bezier;
        for (int i = 0; i < 4; ++i)
            controlMarkers[i] = new ControlPointMarker(bezier.ControlPoint(i), i);
    }

    public void Destroy() {
        foreach (var c in controlMarkers)
            c?.Destroy();
    }

    public void OnUpdate() {
        foreach(var c in controlMarkers) {
            c?.OnUpdate();
        }
    }

    /// <param name="hoverIndex">index of the control point that contains mouse in Panel</param>
    public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color color, int hoverIndex = -1) {
        for(int i = 0; i < 4; ++i) {
            controlMarkers[i].RenderOverlay(cameraInfo, color, hoverIndex == i);
        }
    }

    /// <returns>true if position changed</returns>
    public bool Drag(Vector3 hitPos) {
        for (int i = 0; i < 4; ++i) {
            var controlMarker = controlMarkers[i];
            controlMarker.UpdatePosition(CustomLane.GetControlPoint(i));
        }

        for (int i = 0; i < 4; ++i) {
            var controlMarker = controlMarkers[i];
            if (controlMarker.Drag(hitPos)) {
                CustomLane.UpdateControlPoint(i, controlMarker.Position);
                return true;
            }
        }
        return false;
    }
}


