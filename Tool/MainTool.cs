extern alias UnifedUILib;

using ColossalFramework.UI;
using UnityEngine;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using PathController.Util;
using PathController.UI;
using System.Collections.Generic;
using System.Linq;
using KianCommons;
using PathController.UI.Data;
using UnifedUILib::UnifiedUI.Helpers;
using UnityEngine.UI;
using PathController.CustomData;
using PathController.UI.Marker;
using PathController.LifeCycle;
using PathController.Manager;

namespace PathController.Tool {
    public class PathControllerTool : ToolBase
    {
        public static readonly SavedInputKey ActivationShortcut = new SavedInputKey("ActivationShortcut", nameof(PathControllerMod), SavedInputKey.Encode(KeyCode.P, true, false, false), true);

        public static bool CtrlIsPressed => Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        public static bool ShiftIsPressed => Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        public static Ray MouseRay { get; private set; }
        public static float MouseRayLength { get; private set; }
        public static bool MouseRayValid { get; private set; }
        public static Vector3 MousePosition { get; private set; }
        public static Vector3 MouseWorldPosition { get; private set; }

        public static Camera Camera;

        public BaseTool CurrentTool { get; private set; }
        private Dictionary<ToolType, BaseTool> Tools { get; set; } = new Dictionary<ToolType, BaseTool>();

        public bool ToolEnabled => enabled;

        public BezierMarker BezierMarker { get; private set; }

        #region selected segments
        public SegmentDTO SegmentInstance {
            get;
            private set;
        }

        private CustomLane laneInstance_;
        public CustomLane LaneInstance {
            get => laneInstance_;
            private set {
                Log.Called(value);
                laneInstance_ = value;
                if (value != null) {
                    BezierMarker = new BezierMarker(value.LaneIdAndIndex);
                    Log.Debug("BezierMarker created for " + value.LaneIdAndIndex);
                } else {
                    BezierMarker = null;
                    Log.Debug("BezierMarker  = null");
                }
            }
        }

        private HashSet<ushort> selectedSegmentIds_ = new();
        public IEnumerable<ushort> SelectedSegmentIds => selectedSegmentIds_;
        public int ActiveLaneIndex => LaneInstance?.Index ?? -1;
        public ushort ActiveSegmentId {
            get => SegmentInstance.SegmentId;
            set {
                selectedSegmentIds_.Clear();
                if (value != 0) selectedSegmentIds_.Add(value);
                SetSegment(value); // also allocates custom lanes
            }
        }

        public bool IsSegmentSelected(ushort segmentId) =>
            selectedSegmentIds_.Contains(segmentId);

        public void SelectSegment(ushort segmnetId) {
            if (selectedSegmentIds_.Count == 0) {
                ActiveSegmentId = segmnetId;
            } else {
                selectedSegmentIds_.Add(segmnetId);
                PathControllerManager.Instance.GetOrCreateLanes(segmnetId); // allocate custom lanes
            }
        }

        public void ToggleSelectedSegment(ushort segmnetId) {
            if (IsSegmentSelected(segmnetId)) {
                DeselectSegment(segmnetId);
            } else {
                SelectSegment(segmnetId);
            }
        }

        public void DeselectSegment(ushort segmentId) {
            if (segmentId == ActiveSegmentId) {
                // set Active segment to another selected segment, or 0 otherwise.
                ushort newActiveSegmentId = selectedSegmentIds_.
                    FirstOrDefault(segmentId => segmentId != ActiveSegmentId);
                SetSegment(newActiveSegmentId);
            }
            selectedSegmentIds_.Remove(segmentId);
        }
        private void SetSegment(ushort segmentId) {
            int laneIndex0 = LaneInstance?.Index ?? -1;
            SegmentInstance = new SegmentDTO(segmentId);
            if (segmentId == 0) {
                SetLane(-1);
            } else {
                SetLane(laneIndex0);
            }
        }

        public void SetLane(int laneIndex) {
            Log.Called(laneIndex);
            LaneInstance = SegmentInstance.Lanes.ElementAtOrDefault(laneIndex);
        }

        private void SetLane(LaneIdAndIndex laneIdAndIndex) {
            Log.Called(laneIdAndIndex);
            SetSegment(laneIdAndIndex.SegmentId);
            SetLane(laneIdAndIndex.LaneIndex);
        }
        #endregion


        private UIComponent UUIButton;

        PathControllerExtendedPanel Panel => PathControllerExtendedPanel.Instance;

        public static PathControllerTool Instance { get; set; }

        #region Base Functions
        protected override void Awake()
        {
            Log.Info("LaneManagerTool.Awake()");
            base.Awake();
            Camera = UIView.GetAView().uiCamera;

            Tools = new Dictionary<ToolType, BaseTool>()
            {
                { ToolType.SelectSegment, new SelectSegmentTool() },
                { ToolType.SelectLane, new SelectLaneTool() },
                // More here...
            };

            PathControllerExtendedPanel.CreatePanel();
            string iconPath = UUIHelpers.GetFullPath<PathControllerMod>("uui_movelanes.png");
            UUIButton = UUIHelpers.RegisterToolButton(
                name: "PathController",
                groupName: null, // default group
                tooltip: "Path Controller",
                tool: this,
                icon: UUIHelpers.LoadTexture(iconPath),
                hotkeys: new UUIHotKeys { ActivationKey = ActivationShortcut });

            DisableTool();
        }

        public static PathControllerTool Create()
        {
            Log.Debug("PathControllerExtendedTool.Create()");
            GameObject toolModControl = ToolsModifierControl.toolController.gameObject;
            Instance = toolModControl.AddComponent<PathControllerTool>();
            Log.Info($"Tool created");
            ToolsModifierControl.SetTool<DefaultTool>();
            return Instance;
        }

        public static void Remove()
        {
            Log.Debug("PathControllerExtendedTool.Remove()");
            if (Instance != null)
            {
                Destroy(Instance);
                Instance = null;
                Log.Info($"Tool removed");
            }
        }

        protected override void OnDestroy()
        {
            Log.Debug("PathControllerExtendedTool.OnDestroy()");
            base.OnDestroy();

            PathControllerExtendedPanel.RemovePanel();
            UUIButton?.Destroy();
            DisableTool();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            Reset();

            Singleton<InfoManager>.instance.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            Reset();
            //LaneManagerPanel.Instance?.Close();
            ToolsModifierControl.SetTool<DefaultTool>();
        }

        public void Reset()
        {
            Panel.Hide();
            SetMode(ToolType.SelectSegment);
            LaneInstance = null;
            SegmentInstance = null;
            selectedSegmentIds_.Clear();
        }
        public void SetDefaultMode() => SetMode(ToolType.Initial);
        public void SetMode(ToolType mode) => SetMode(Tools[mode]);
        public void SetMode(BaseTool mode)
        {
            CurrentTool?.DeInit();
            CurrentTool = mode;
            CurrentTool?.Init();

            if (CurrentTool?.ShowPanel == true)
                Panel.Show();
            else
                Panel.Hide();
        }

        public void EnableTool()
        {
            Log.Debug("PathControllerExtendedTool.EnableTool()");
            enabled = true;
        }

        public void DisableTool()
        {
            Log.Debug("PathControllerExtendedTool.DisableTool()");
            enabled = false;
        }

        public void ToggleTool()
        {
            Log.Debug("PathControllerExtendedTool.ToggleTool()");
            enabled = !enabled;
        }
        #endregion

        #region Tool Update
        protected override void OnToolUpdate()
        {
            MousePosition = Input.mousePosition;
            MouseRay = Camera.main.ScreenPointToRay(MousePosition);
            MouseRayLength = Camera.main.farClipPlane;
            MouseRayValid = !UIView.IsInsideUI() && Cursor.visible;
            RaycastInput input = new RaycastInput(MouseRay, MouseRayLength);
            RayCast(input, out RaycastOutput output);
            MouseWorldPosition = output.m_hitPos;

            CurrentTool.OnUpdate();

            base.OnToolUpdate();
        }

        public override void SimulationStep() {
            try {
                base.SimulationStep();
                if (BezierMarker != null) {
                    RaycastInput input = new(MouseRay, MouseRayLength) { m_ignoreTerrain = false };
                    RayCast(input, out RaycastOutput output);
                    BezierMarker.Drag(output.m_hitPos);
                }
            } catch (Exception ex) { ex.Log(); }
        }
        #endregion

        #region Render Overlay
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            try {
                base.RenderOverlay(cameraInfo);
                CurrentTool?.RenderOverlay(cameraInfo);
                Panel?.Render(cameraInfo);
                BezierMarker?.RenderOverlay(cameraInfo, Color.green);
                base.RenderOverlay(cameraInfo);
            } catch (Exception ex) { ex.Log(); }
        }

        #endregion

        #region access
        public new static bool RayCast(RaycastInput input, out RaycastOutput output) => ToolBase.RayCast(input, out output);
        public new string GetErrorString(ToolErrors e) => base.GetErrorString(e);

        private const string kCursorInfoErrorColor = "<color #ff7e00>";
        private const string kCursorInfoNormalColor = "<color #87d3ff>";
        private const string kCursorInfoCloseColorTag = "</color>";
        protected void ShowToolInfo2(string text) => ShowToolInfo2(true, text, Input.mousePosition);

        protected void ShowToolInfo2(bool show, string text, Vector2 pos) {
            if (ToolBase.cursorInfoLabel == null) {
                return;
            }
            ToolErrors errors = GetErrors();
            if ((errors & ToolErrors.VisibleErrors) != ToolErrors.None) {
                bool hasText = !string.IsNullOrEmpty(text);
                if (hasText) {
                    text += "\n";
                } else {
                    text = string.Empty;
                }
                text += kCursorInfoErrorColor;
                for (var toolErrors = ToolErrors.ObjectCollision;
                    toolErrors <= ToolErrors.AdministrationBuildingExists;
                    toolErrors = (ToolErrors)((int)toolErrors << 1)) {
                    if ((errors & toolErrors) != ToolErrors.None) {
                        if (hasText) {
                            text += "\n";
                        }
                        hasText = true;
                        text += GetErrorString(toolErrors);
                    }
                }
                text += kCursorInfoCloseColorTag;
            }
            if (!string.IsNullOrEmpty(text) && show) {
                text = kCursorInfoNormalColor + text + kCursorInfoCloseColorTag;
                ToolBase.cursorInfoLabel.isVisible = true;
                UIView uiview = ToolBase.cursorInfoLabel.GetUIView();
                Vector2 res = (!(ToolBase.fullscreenContainer != null)) ? uiview.GetScreenResolution() : ToolBase.fullscreenContainer.size;
                Vector2 startCorner = ToolBase.cursorInfoLabel.pivot.UpperLeftToTransform(ToolBase.cursorInfoLabel.size, ToolBase.cursorInfoLabel.arbitraryPivotOffset);
                Vector3 relativePosition = uiview.ScreenPointToGUI(pos / uiview.inputScale) + startCorner;
                ToolBase.cursorInfoLabel.text = text;
                if (relativePosition.x < 0f) {
                    relativePosition.x = 0f;
                }
                if (relativePosition.y < 0f) {
                    relativePosition.y = 0f;
                }
                if (relativePosition.x + ToolBase.cursorInfoLabel.width > res.x) {
                    relativePosition.x = res.x - ToolBase.cursorInfoLabel.width;
                }
                if (relativePosition.y + ToolBase.cursorInfoLabel.height > res.y) {
                    relativePosition.y = res.y - ToolBase.cursorInfoLabel.height;
                }
                ToolBase.cursorInfoLabel.relativePosition = relativePosition;
            } else {
                ToolBase.cursorInfoLabel.isVisible = false;
            }
        }
        #endregion

        /// <summary>Check whether the position is below the ground level.</summary>
        /// <param name="position">Point in the world.</param>
        /// <returns>True if the position is below the ground level</returns>
        public static bool CheckIsUnderground(Vector3 position) {
            float sampledHeight = TerrainManager.instance.SampleDetailHeightSmooth(position);
            return sampledHeight > position.y;
        }

        #region Tool GUI
        private bool IsMouseDown { get; set; }
        private bool IsMouseMove { get; set; }
        protected override void OnToolGUI(Event e)
        {
            CurrentTool.OnGUI(e);

            switch (e.type)
            {
                case EventType.MouseDown when MouseRayValid && e.button == 0:
                    IsMouseDown = true;
                    IsMouseMove = false;
                    CurrentTool.OnMouseDown(e);
                    break;
                case EventType.MouseDrag when MouseRayValid:
                    IsMouseMove = true;
                    CurrentTool.OnMouseDrag(e);
                    break;
                case EventType.MouseUp when MouseRayValid && e.button == 0:
                    if (IsMouseMove)
                        CurrentTool.OnMouseUp(e);
                    else
                        CurrentTool.OnPrimaryMouseClicked(e);
                    IsMouseDown = false;
                    break;
                case EventType.MouseUp when MouseRayValid && e.button == 1:
                    CurrentTool.OnSecondaryMouseClicked();
                    break;
                case EventType.KeyUp:
                    CurrentTool.OnKeyUp(e);
                    break;
            }
        }
        #endregion
    }
}
