namespace PathController.UI.Marker;

using KianCommons;
using PathController.Manager;
using PathController.Util;
using UnityEngine;

internal class BezierMarker {
    internal bool Focused =>
        A.Hovered || A.Selected ||
        B.Hovered || B.Selected ||
        C.Hovered || C.Selected ||
        D.Hovered || D.Selected;

    public LaneIdAndIndex LaneIdAndIndex;
    public ControlPointMarker[] ControlPoints = new ControlPointMarker[4];
    public ControlPointMarker A => ControlPoints[0];
    public ControlPointMarker B => ControlPoints[1];
    public ControlPointMarker C => ControlPoints[2];
    public ControlPointMarker D => ControlPoints[3];


    public BezierMarker(LaneIdAndIndex laneIdAndIndex) {
        LaneIdAndIndex = laneIdAndIndex;
        var bezier = laneIdAndIndex.Lane.m_bezier;
        for (int i = 0; i < 4; ++i)
            ControlPoints[i] = new ControlPointMarker(bezier.ControlPoint(i));
    }


    /// <param name="focus">number field contains mouses</param>
    public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color color, bool fieldHovered = false) {
        foreach (ControlPointMarker c in ControlPoints) 
            c.RenderOverlay(cameraInfo, color, fieldHovered);
    }

    public bool Drag(Vector3 hitPos) {
        for(int i = 0; i < 4; ++i) {
            var c = ControlPoints[i];
            if (c.Drag(hitPos)) {
                var customLane = PathControllerManager.Instance.GetOrCreateLane(LaneIdAndIndex);
                customLane.LaneIdAndIndex.Lane.m_bezier.ControlPoint(i) = c.Position;
                customLane.SetControlPoint(i, c.Position);
                customLane.QueueUpdate();
                return true;
            }
        }
        return false;
    }
}


