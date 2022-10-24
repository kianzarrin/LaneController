extern alias UnifedUILib;

using ColossalFramework.UI;
using UnityEngine;
using System;
using ColossalFramework;
using ColossalFramework.Math;
using LaneConroller.Util;
using LaneConroller.UI;
using System.Collections.Generic;
using System.Linq;
using KianCommons;
using UnifedUILib::UnifiedUI.Helpers;
using UnityEngine.UI;
using LaneConroller.CustomData;
using LaneConroller.UI.Marker;
using LaneConroller.LifeCycle;
using LaneConroller.Manager;
using static RenderManager;
using LaneConroller.UI.Editors;
using static NetInfo;

namespace LaneConroller.Tool {
    public class LaneConrollerTool : ToolBase
    {
        public static readonly SavedInputKey ActivationShortcut = new SavedInputKey("ActivationShortcut", nameof(LaneConrollerMod), SavedInputKey.Encode(KeyCode.P, true, false, false), true);

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
        private SegmentDTO segmentInstance_;
        public SegmentDTO SegmentInstance {
            get => segmentInstance_;
            private set {
                segmentInstance_ = value;
                UpdateMode();
            }
        }

        private CustomLane laneInstance_;
        public CustomLane LaneInstance {
            get => laneInstance_;
            private set {
                Log.Called(value);
                laneInstance_ = value;
                if (value != null) {
                    BezierMarker = new BezierMarker(value.LaneIdAndIndex);
                } else {
                    BezierMarker = null;
                }
                if (Panel?.CurrentEditor is LaneEditor laneEditor && laneEditor.EditObject != value) {
                    laneEditor.UpdateEditor(value);
                }
                UpdateMode();
            }
        }

        private HashSet<ushort> selectedSegmentIds_ = new();
        public IEnumerable<ushort> SelectedSegmentIds => selectedSegmentIds_;
        public int ActiveLaneIndex => LaneInstance?.Index ?? -1;
        public ushort ActiveSegmentId {
            get => SegmentInstance.SegmentId;
            set {
                Log.Called(value);
                foreach(ushort segmentId in selectedSegmentIds_) {
                    if (segmentId != value) {
                        LaneConrollerManager.Instance.TrimSegment(segmentId);
                    }
                }
                selectedSegmentIds_.Clear();
                if (value != 0) selectedSegmentIds_.Add(value);
                SetSegment(value); // also allocates custom lanes
            }
        }

        public bool IsSegmentSelected(ushort segmentId) =>
            selectedSegmentIds_.Contains(segmentId);

        public void SelectSegment(ushort segmnetId) {
            if (segmnetId == 0) return;
            if (selectedSegmentIds_.Count == 0) {
                ActiveSegmentId = segmnetId;
            } else {
                selectedSegmentIds_.Add(segmnetId);
                LaneConrollerManager.Instance.GetOrCreateLanes(segmnetId); // allocate custom lanes
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
            if (segmentId == 0) return;
            if (segmentId == ActiveSegmentId) {
                // set Active segment to another selected segment, or 0 otherwise.
                ushort newActiveSegmentId = selectedSegmentIds_.
                    FirstOrDefault(segmentId => segmentId != ActiveSegmentId);
                SetSegment(newActiveSegmentId);
            }
            selectedSegmentIds_.Remove(segmentId);
            LaneConrollerManager.Instance.TrimSegment(segmentId);
        }

        /// <summary>
        /// sets segment to the given segment id.
        /// selected lane index does not change if any.
        /// if segment id == 0 then all segments/lanes are deselected.
        /// </summary>
        /// <param name="segmentId"></param>
        private void SetSegment(ushort segmentId) {
            Log.Called(segmentId);
            if (segmentId == 0) {
                SegmentInstance = null;
                SetLane(-1);
            } else {
                int laneIndex0 = LaneInstance?.Index ?? -1;
                SegmentInstance = new SegmentDTO(segmentId);
                SetLane(laneIndex: laneIndex0);
            }
        }

        public void SetLane(int laneIndex) {
            try {
                Log.Called(laneIndex);
                if (laneIndex < 0)
                    LaneInstance = null;
                else
                    LaneInstance = SegmentInstance.Lanes.ElementAtOrDefault(laneIndex);
            }catch(Exception ex) {
                ex.Log();
            }
        }

        private void SetLane(LaneIdAndIndex laneIdAndIndex) {
            Log.Called(laneIdAndIndex);
            SetSegment(laneIdAndIndex.SegmentId);
            SetLane(laneIdAndIndex.LaneIndex);
        }
        #endregion

        #region Action Shortcut
        public SegmentDTO Cache;

        public static void Copy() => Instance.Cache = Instance.SegmentInstance.Clone();

        public static void Paste() {
            foreach (ushort segmentId in Instance.SelectedSegmentIds)
                Instance.Cache.CopyTo(segmentId);
        }

        public static void DeleteAll() {
            foreach(ushort segmentId in Instance.SelectedSegmentIds) {
                foreach(var lane in new LaneIterator(segmentId) ){
                    LaneConrollerManager.Instance.GetLane(lane.LaneId)?.Reset();
                }
            }
        }
        public static void ResetControlPoints() {
            foreach (ushort segmentId in Instance.SelectedSegmentIds) {
                Instance.SegmentInstance.CopyTo(segmentId);
            }
        }
        public static void ApplyBetweenIntersections() {
            var sourceLanes = Instance.SegmentInstance.Lanes;
            foreach (ushort segmentId in TraverseUtil.GetSimilarSegmentsBetweenJunctions(Instance.ActiveSegmentId)) {
                var targetLanes = LaneConrollerManager.Instance.GetOrCreateLanes(segmentId);
                for (int laneIndex = 0; laneIndex < sourceLanes.Length; ++laneIndex) {
                    targetLanes[laneIndex].CopyFrom(sourceLanes[laneIndex]);
                }
            }
        }

        public static void ApplyWholeStreet() => throw new NotImplementedException();
        #endregion

        private UIComponent UUIButton;

        LaneConrollerPanel Panel => LaneConrollerPanel.Instance;

        public static LaneConrollerTool Instance { get; set; }

        #region Base Functions
        protected override void Awake()
        {
            Log.Info("LaneManagerTool.Awake()");
            Instance = this;
            base.Awake();
            Camera = UIView.GetAView().uiCamera;

            Tools = new() {
                { ToolType.SelectSegment, new SelectSegmentTool() },
                { ToolType.SelectLane, new SelectLaneTool() },
                { ToolType.ModifyLane, new ModifyLaneTool() },
            };

            LaneConrollerPanel.CreatePanel();
            string iconPath = UUIHelpers.GetFullPath<LaneConrollerMod>("uui_movelanes.png");
            UUIButton = UUIHelpers.RegisterToolButton(
                name: "LaneConroller",
                groupName: null, // default group
                tooltip: "Lane Controller",
                tool: this,
                icon: UUIHelpers.LoadTexture(iconPath),
                hotkeys: new UUIHotKeys { ActivationKey = ActivationShortcut });

            enabled = false;
        }

        public static LaneConrollerTool Create()
        {
            Log.Called();
            GameObject toolModControl = ToolsModifierControl.toolController.gameObject;
            Instance = toolModControl.AddComponent<LaneConrollerTool>();
            Log.Info($"Tool created");
            ToolsModifierControl.SetTool<DefaultTool>();
            return Instance;
        }

        public static void Remove()
        {
            Log.Called();
            if (Instance != null)
            {
                Destroy(Instance);
                Instance = null;
                Log.Info($"Tool removed");
            }
        }

        protected override void OnDestroy()
        {
            Log.Called();
            base.OnDestroy();

            LaneConrollerPanel.RemovePanel();
            UUIButton?.Destroy();
            enabled = false;
        }

        protected override void OnEnable() {
            try {

                base.OnEnable();
                Reset();

                Singleton<InfoManager>.instance.SetCurrentMode(InfoManager.InfoMode.None, InfoManager.SubInfoMode.Default);
            } catch (Exception ex) { ex.Log(); }
        }

        protected override void OnDisable() {
            try {
                base.OnDisable();
                Reset();
                //LaneManagerPanel.Instance?.Close();
                ToolsModifierControl.SetTool<DefaultTool>();
            } catch (Exception ex) { ex.Log(); }
        }

        public void Reset() {
            Panel.Hide();
            SetDefaultMode();
            LaneInstance = null;
            selectedSegmentIds_.Clear();
            SegmentInstance = null;
        }

        public void SetDefaultMode() => SetMode(ToolType.Initial);
        public void SetMode(ToolType mode) => SetMode(Tools[mode]);
        public void SetMode(BaseTool subtool) {
            Log.Called(subtool);
            CurrentTool?.DeInit();
            CurrentTool = subtool;
            CurrentTool?.Init();

            if (CurrentTool?.ShowPanel == true)
                Panel.SetSegment(ActiveSegmentId);
            else
                Panel.Hide();
        }

        public void UpdateMode() {
            ToolType type;
            if (selectedSegmentIds_.Count == 0)
                type = ToolType.SelectSegment;
            else if (laneInstance_ == null) {
                type = ToolType.SelectLane;
            } else {
                type = ToolType.ModifyLane;
            }

            if (CurrentTool?.Type != type) {
                SetMode(type);
            }
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
                CurrentTool.SimulationStep();
            } catch (Exception ex) { ex.Log(); }
        }
        #endregion

        #region Render Overlay
        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            try {
                base.RenderOverlay(cameraInfo);
                CurrentTool?.RenderOverlay(cameraInfo);
                if (CurrentTool.ShowPanel) {
                    Panel?.Render(cameraInfo);
                }

            } catch (Exception ex) { ex.Log(); }
        }

        public void RenderLanesOverlay(RenderManager.CameraInfo cameraInfo, int laneIndex, Color color) {
            if (laneIndex < 0)
                return;
            foreach (ushort segmentId in SelectedSegmentIds) {
                uint laneId = NetUtil.GetLaneId(segmentId, laneIndex);
                LaneIdAndIndex laneIdAndIndex = new(laneId, laneIndex);
                RenderUtil.RenderLaneOverlay(cameraInfo, laneIdAndIndex, color, alphaBlend: true);
            }
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
