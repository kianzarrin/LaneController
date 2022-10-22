namespace PathController.UI.Marker;
using ColossalFramework.Math;
using PathController.Tool;
using PathController.Util;
using UnityEngine;


/// <summary>
/// code revived from the old Traffic++ mod:
/// https://github.com/joaofarias/csl-traffic/blob/a4c5609e030c5bde91811796b9836aad60ddde20/CSL-Traffic/Tools/RoadCustomizerTool.cs
/// </summary>
public class LaneMarker {
    internal LaneMarker(Bezier3 bezier) {
        Update(bezier);
    }

    private Bezier3 bezier_;

    /// <summary>Bezier size when drawing (thickness).</summary>
    internal float Size = 1.1f;

    private Bounds[] bounds_;

    private bool isUnderground_;

    public void Update(Bezier3 bezier) {
        if (!bezier_.EqualsTo(bezier)) {
            bezier_ = bezier;
            isUnderground_ = PathControllerTool.CheckIsUnderground(bezier.a) ||
                            PathControllerTool.CheckIsUnderground(bezier.d);
            CalculateBounds();
        }
    }

    /// <summary>Intersects mouse ray with lane bounds.</summary>
    internal bool IntersectRay() {
        foreach (Bounds eachBound in bounds_) {
            if (eachBound.IntersectRay(PathControllerTool.MouseRay)) {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Forces render height.
    /// </summary>
    /// <param name="height">New height</param>
    internal void ForceBezierHeight(float height) {
        bezier_ = bezier_.ForceHeight(height);
    }

    /// <summary>
    /// Initializes/recalculates bezier bounds.
    /// </summary>
    private void CalculateBounds() {
        Bezier3 bezier0 = bezier_;

        // split bezier in 10 parts to correctly raycast curves
        int n = 10;
        bounds_ = new Bounds[n];
        float size = 1f / n;
        for (int i = 0; i < n; i++) {
            Bezier3 bezier = bezier0.Cut(i * size, (i + 1) * size);
            Bounds bounds = bezier.GetBounds();
            bounds.Expand(1f);
            this.bounds_[i] = bounds;
        }
    }

    /// <summary>Renders lane overlay.</summary>
    internal void RenderOverlay(
        RenderManager.CameraInfo cameraInfo,
        Color color,
        bool enlarge = false,
        bool overDraw = false,
        bool alphaBlend = false,
        bool cutStart = false,
        bool cutEnd = false) {

        overDraw |= isUnderground_;
        float overdrawHeight = overDraw ? 0f : 2f;
        Bounds bounds = bezier_.GetBounds();
        float minY = bounds.min.y - overdrawHeight;
        float maxY = bounds.max.y + overdrawHeight;

        float size = enlarge ? Size * 1.41f : Size;
        ColossalFramework.Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
        RenderManager.instance.OverlayEffect.DrawBezier(
            cameraInfo: cameraInfo,
            color: color,
            bezier: bezier_,
            size: size,
            cutStart: cutStart ? size * 0.5f : 0,
            cutEnd: cutEnd ? size * 0.5f : 0,
            minY: minY,
            maxY: maxY,
            renderLimits: overDraw,
            alphaBlend: alphaBlend);
    }
}
