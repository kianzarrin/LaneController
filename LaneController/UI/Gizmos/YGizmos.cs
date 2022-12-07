namespace LaneController.UI.Gizmos;
using ColossalFramework.UI;
using KianCommons;
using System;
using UnityEngine;

public class YGizmo {
    public struct GizmoAxisT {
        public GameObject Gizmo, Head;
        public LineRenderer Renderer;
        public BoxCollider Collider => Gizmo?.GetComponent<BoxCollider>();
    }

    public YGizmo() => Cam = Camera.main;
    ~YGizmo() => Destroy();


    public static float opacity = 1;
    public static Color GizmoRed = new Color(1f, 0f, 0f, opacity);
    public static Color GizmoBlue = new Color(0f, 0f, 1f, opacity);
    public static Color GizmoGreen = new Color(0f, 1f, 0f, opacity);
    public static Color GizmoYellow = new Color(1f, 0.92f, 0.016f, opacity);

    public static float GizmoSize = 0.5f;

    public static Camera Cam;

    public GameObject CenterCube;
    public GizmoAxisT GizmoAxis;
    public static Shader shader = Shader.Find("GUI/Text Shader");

    public void Destroy() {
        if (GizmoAxis.Gizmo) UnityEngine.Object.Destroy(GizmoAxis.Gizmo);
        if (CenterCube) UnityEngine.Object.Destroy(CenterCube);
        if (GizmoAxis.Head) UnityEngine.Object.Destroy(GizmoAxis.Head);
    }

    public static YGizmo CreatePositionGizmo(Vector3 position) {
        var ret = new YGizmo();
        ret.Init(position);
        return ret;
    }

    private void Init(Vector3 position) {
        try {
            GameObject gizmo = GizmoAxis.Gizmo = new GameObject("LCAxis_Y");
            var xCollid = gizmo.AddComponent<BoxCollider>();
            xCollid.size = new Vector3(2, 2, 2);
            LineRenderer lineRenderer = GizmoAxis.Renderer = gizmo.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(shader);
            lineRenderer.startColor = GizmoGreen;
            lineRenderer.endColor = GizmoGreen;
            lineRenderer.widthMultiplier = 1;
            Vector3[] xPos = new Vector3[2];
            xPos[1] = xPos[0] = position;
            xPos[1][(int)Axis.Y] += 20;

            lineRenderer.SetPositions(xPos);
            gizmo.transform.position = position;
            gizmo.transform.localScale = new Vector3(0.5f, 20f, 0.5f);

            //if(false)
            {
                Material yellow = new Material(shader);
                yellow.color = GizmoYellow;
                CenterCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                CenterCube.transform.position = position;
                CenterCube.GetComponent<MeshRenderer>().material = yellow;
                GameObject.Destroy(CenterCube.GetComponent<MeshCollider>());

                //CenterCube.transform.SetParent(GizmoAxis.Gizmo.transform, true);
            }

            //if(false)
            {
                Material green = new Material(shader);
                green.color = GizmoGreen;
                var head = GizmoAxis.Head = Cone.Create(green);
                head.transform.position = xPos[1];

                //head.transform.SetParent(GizmoAxis.Gizmo.transform, true);
            }
        } catch (Exception ex) { ex.Log(); }
    }

    public static Plane GetCollisionPlane(Vector3 axisHitPoint) {
        Vector3 forward = Cam.transform.forward;
        Vector3 normal = new Vector3(forward.x, 0, forward.z).normalized;
        return new Plane(normal, axisHitPoint);
    }

    public void UpdatePosition(Vector3 position) {
        if (AxisClicked) {
            // while dragging, position is already updated
            // besides simulation thread might be behind.
            return;
        }
        UpdatePositionImpl(position);
    }

    private void UpdatePositionImpl(Vector3 position) {
        try {
            Assertion.InMainThread();
            float distance = (Cam.transform.position - position).magnitude;
            float factor = (0.0070455f * distance + 0.0386363f) * GizmoSize;
            float factor20 = 20f * factor;
            float halfFactor = 0.5f * factor;

            Vector3 postion2 = position + new Vector3(0, factor20, 0);
            Vector3 scale = new Vector3(halfFactor, factor20, halfFactor);
            Vector3 scale2 = Vector3.one * factor;
            if (GizmoAxis.Gizmo is GameObject gizmo && gizmo) {
                gizmo.transform.position = position;
                gizmo.transform.localScale = scale;
            }

            if (CenterCube != null) {
                CenterCube.transform.position = position;
                CenterCube.transform.localScale = scale2;
            }

            if (GizmoAxis.Head is GameObject head && head) {
                head.transform.position = postion2;
                head.transform.localScale = scale2;
            }

            if (GizmoAxis.Renderer is LineRenderer renderer && renderer) {
                renderer.widthMultiplier = factor;
                renderer.SetPositions(new[] { position, postion2 });
            }
        } catch (Exception ex) { ex.Log(); }
    }


    #region Movement
    public bool AxisClicked = false;
    public Vector3 Origin => GizmoAxis.Gizmo.transform.position;
    public float distance => Origin.y - Origin0.y;
    Vector3 HitPos0;
    Vector3 Origin0;
    public KeyTyping KeyTyping;

    public bool IsVisible {
        get {
            if (GizmoAxis.Renderer is LineRenderer lineRenderer) {
                return lineRenderer.enabled;
            } else {
                return false;
            }
            
        }
        set {
            if (GizmoAxis.Renderer is LineRenderer lineRenderer && lineRenderer) {
                lineRenderer.enabled = value;
            }

            if (GizmoAxis.Collider is BoxCollider collider && collider) {
                collider.enabled = value;
            }

            if (CenterCube?.GetComponent<MeshRenderer>() is MeshRenderer cubeRenderer && cubeRenderer) {
                cubeRenderer.enabled = value;
            }

            if (GizmoAxis.Head?.GetComponent<MeshRenderer>() is MeshRenderer headRenderer && headRenderer) {
                headRenderer.enabled = value;
            }
        }
    }

    public void OnUpdate() {
        Assertion.InMainThread();
        KeyTyping?.Register();
    }

    public bool GetAxisHitPoint() {
        Ray ray = Cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit)) {
            if (hit.transform.gameObject == GizmoAxis.Gizmo) {
                //PlaySound(3);
                var plane = GetCollisionPlane(Origin);
                if (plane.Raycast(ray, out float enter))
                    HitPos0 = ray.GetPoint(distance: enter);
                else
                    HitPos0 = hit.point;
                return true;
            }
        }
        HitPos0 = default;
        return false;
    }

    public bool Movement(out Vector3 newPosition) {
        Ray mouseRay = Cam.ScreenPointToRay(Input.mousePosition);
        Plane plane = GetCollisionPlane(HitPos0);
        newPosition = Origin0;

        if (plane.Raycast(mouseRay, out float enter)) {
            float slowFactor = Helpers.AltIsPressed ? 0.2f : 1f;
            Vector3 hitPos = Vector3.Lerp(HitPos0, mouseRay.GetPoint(enter), slowFactor);
            if (KeyTyping != null && KeyTyping.registeredFloat != 0)
                newPosition += new Vector3(0, KeyTyping.registeredFloat);
            else
                newPosition.y += hitPos.y - HitPos0.y;
        }

        return (newPosition - Origin).sqrMagnitude > 0.001; // min movement is 1mm
    }

    public bool Drag() {
        try {
            Assertion.InMainThread();
            if (UIView.HasModalInput() || UIView.HasInputFocus()) return false;

            if (Input.GetMouseButtonDown(0)) {
                if (GetAxisHitPoint()) { // axis click
                    Origin0 = Origin;
                    AxisClicked = true;
                    KeyTyping = new(); // enable key typing
                }
            } else if (AxisClicked) {
                if (Input.GetMouseButton(0)) { // movement
                    if (Movement(out Vector3 newPosion)) {
                        UpdatePositionImpl(newPosion);
                        return true; // position updated.
                    }
                } else { // released
                    AxisClicked = false;
                    KeyTyping = null; //disable key typing
                    return true; // position updated.
                }
            }
        } catch (Exception ex) { ex.Log(); }
        return false;
    }
    #endregion
}
