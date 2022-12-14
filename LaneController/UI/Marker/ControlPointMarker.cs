namespace LaneConroller.UI.Marker;
using ColossalFramework;
using KianCommons;
using LaneConroller.Tool;
using LaneController.UI.Gizmos;
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

    internal YGizmo Gizmo;

    public ControlPointMarker(Vector3 pos, int i) {
        this.i = i;
        UpdatePosition(pos);
        Gizmo = YGizmo.CreatePositionGizmo(pos);
    }

    public bool GizmoMode {
        get {
            if (Gizmo == null) return false;
            if(Gizmo.AxisClicked) return true;
            return Helpers.ControlIsPressed;
        }
    }

    public void OnUpdate() => Gizmo?.OnUpdate();

    public void Destroy() {
        Gizmo?.Destroy();
        Gizmo = null;
    }

    public void UpdatePosition(Vector3 pos) {
        if (!Selected && (Gizmo == null || !Gizmo.AxisClicked)) {
            // don't update in the middle of dragging.
            // the position is already updated and simulation thread might be behind.
            UpdatePositionImpl(pos);
        }
    }

    private void UpdatePositionImpl(Vector3 pos) {
        Position = pos;
        UnderGround = false;
        TerrainPosition = pos;
        TerrainPosition.y = Singleton<TerrainManager>.instance.SampleDetailHeightSmooth(pos);
        Gizmo?.UpdatePosition(pos);
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

        if (!GizmoMode) {
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

        if (Event.current.type == EventType.Repaint && Gizmo != null) {
            Gizmo.IsVisible = GizmoMode;
        }
    }

    public bool Drag(Vector3 hitPos) {
        if (GizmoMode) {
            if (Gizmo != null && Gizmo.Drag()) {
                UpdatePositionImpl(Gizmo.Origin);
                return true;
            }
        } else if (Selected) {
            hitPos.y = Position.y;
            var delta = hitPos - Position;
            if (delta.sqrMagnitude > 1e-04) {
                UpdatePositionImpl(hitPos);
                return true;
            }
        }
        return false;
    }
}

