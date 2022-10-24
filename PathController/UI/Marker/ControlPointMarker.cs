namespace LaneConroller.UI.Marker;
using ColossalFramework;
using LaneConroller.Tool;
using UnityEngine;

public class ControlPointMarker {
    internal bool UnderGround;
    internal Vector3 TerrainPosition; // projected on terrain
    internal Vector3 Position; // original height.
    internal static float Radius = 1.5f;
    internal const float MAX_ERROR = 2.5f;
    internal bool Hovered;
    internal bool Selected;
    internal int i;

    public ControlPointMarker(Vector3 pos, int i) {
        this.i = i;
        UpdatePosition(pos);
    }

    public void UpdatePosition(Vector3 pos) {
        Position = pos;
        UnderGround = false;
        TerrainPosition = pos;
        TerrainPosition.y = Singleton<TerrainManager>.instance.SampleDetailHeightSmooth(pos);
    }

    public void CalculateMode() {
        if (Input.GetMouseButtonDown(0)) {
            Selected = Hovered = IntersectRay();
        } else if (Input.GetMouseButtonUp(0)) {
            Selected = false;
            Hovered = IntersectRay();
        } else {
            Hovered = Selected || IntersectRay();
        }
    }

    /// <summary>
    ///  Intersects mouse ray with marker bounds.
    /// </summary>
    /// <returns><c>true</c>if mouse ray intersects with marker <c>false</c> otherwise</returns>
    private bool IntersectRay() {
        Vector3 pos = UnderGround ? TerrainPosition : Position;
        Bounds bounds = new Bounds(center: pos, size: Vector3.one * Radius);
        return bounds.IntersectRay(LaneConrollerTool.MouseRay);
    }

    /// <param name="focus">number field contains mouses</param>
    public void RenderOverlay(RenderManager.CameraInfo cameraInfo, Color color, bool fieldHovered = false) {
        CalculateMode();
        float magnification = Hovered || fieldHovered ? 2f : 1f;
        if (Selected) magnification = 2.5f;

        const float OVERDRAW = 100;// through all the geometry -100..100
        RenderManager.instance.OverlayEffect.DrawCircle(
            cameraInfo,
            color,
            TerrainPosition,
            Radius * magnification,
            TerrainPosition.y - OVERDRAW,
            TerrainPosition.y + OVERDRAW,
            false,
            true);

        RenderManager.instance.OverlayEffect.DrawCircle(
            cameraInfo,
            Selected ? Color.white : Color.black,
            TerrainPosition,
            Radius * 0.75f * magnification, // inner circle
            TerrainPosition.y - OVERDRAW,
            TerrainPosition.y + OVERDRAW,
            false,
            false);
    }

    public bool Drag(Vector3 hitPos) {
        if (Selected) {
            hitPos.y = Position.y;
            var delta = hitPos - Position;
            if (delta.sqrMagnitude > 1e-04) {
                Position = hitPos;
                return true;
            }
        }
        return false;
    }
}

