using ColossalFramework.UI;
using PathController.UI;
using PathController.Util;
using PathController.UI.Editors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using KianCommons;

namespace PathController.UI
{
    public class PathManagerExtendedPanel : UIPanel
    {
        public static PathManagerExtendedPanel Instance { get; private set; }
        private UIDragHandle Handle { get; set; }
        private UILabel Caption { get; set; }
        private CustomUITabstrip TabStrip { get; set; }
        private UIPanel SizeChanger { get; set; }
        public List<BaseEditor> Editors { get; } = new List<BaseEditor>();
        public BaseEditor CurrentEditor { get; set; }

        private Vector2 EditorSize => size - new Vector2(0, Handle.height + TabStrip.height);
        private Vector2 EditorPosition => new Vector2(0, TabStrip.relativePosition.y + TabStrip.height);

        private static float TabStripHeight => 20;

        public static UITextureAtlas ResizeAtlas { get; } = GetResizeIcon();
        private static UITextureAtlas GetResizeIcon()
        {
            var atlas = TextureUtil.GetAtlas(nameof(ResizeAtlas));
            if (atlas == UIView.GetAView().defaultAtlas)
                atlas = TextureUtil.CreateTextureAtlas("resize.png", nameof(ResizeAtlas), 9, 9, new string[] { "resize" }, space: 2);

            return atlas;
        }

        public static PathManagerExtendedPanel CreatePanel()
        {
            var uiView = UIView.GetAView();
            Instance = uiView.AddUIComponent(typeof(PathManagerExtendedPanel)) as PathManagerExtendedPanel;
            Instance.Init();
            return Instance;
        }
        public static void RemovePanel()
        {
            if (Instance != null)
            {
                Instance.Hide();
                Destroy(Instance);
                Instance = null;
            }
        }
        public void Init()
        {
            atlas = TextureUtil.InGameAtlas;
            backgroundSprite = "MenuPanel2";
            absolutePosition = new Vector3(100, 100);
            name = "LaneManagerPanel";

            CreateHandle();
            CreateTabStrip();
            CreateEditors();
            CreateSizeChanger();

            size = new Vector2(550, Handle.height + TabStrip.height + 400);
            minimumSize = new Vector2(500, Handle.height + TabStrip.height + 200);
        }

        public void UpdatePanel() => CurrentEditor?.UpdateEditor();
        public void Render(RenderManager.CameraInfo cameraInfo) => CurrentEditor?.Render(cameraInfo);
        private void CreateHandle()
        {
            Handle = AddUIComponent<UIDragHandle>();
            Handle.size = new Vector2(500, 42);
            Handle.relativePosition = new Vector2(0, 0);
            Handle.target = parent;
            Handle.eventSizeChanged += ((component, size) =>
            {
                Caption.size = size;
                Caption.CenterToParent();
            });

            Caption = Handle.AddUIComponent<UILabel>();
            Caption.text = nameof(PathManagerExtendedPanel);
            Caption.textAlignment = UIHorizontalAlignment.Center;
            Caption.anchor = UIAnchorStyle.Top;

            Caption.eventTextChanged += ((component, text) => Caption.CenterToParent());
        }

        private void CreateTabStrip()
        {
            TabStrip = AddUIComponent<CustomUITabstrip>();
            TabStrip.relativePosition = new Vector3(0, Handle.height);
            TabStrip.eventSelectedIndexChanged += TabStripSelectedIndexChanged;
            TabStrip.selectedIndex = -1;
        }

        private void CreateEditors()
        {
            Log.Debug("LaneManagerPanel.CreateEditors() called");
            CreateEditor<LaneEditor>();
            CreateEditor<TemplateEditor>();
        }

        private void CreateEditor<EditorType>() where EditorType : BaseEditor
        {
            Log.Debug("LaneManagerPanel.CreateEditor<EditorType>() called");
            var editor = AddUIComponent<EditorType>();
            editor.Init(this);
            TabStrip.AddTab(editor.Name);

            editor.isVisible = false;
            editor.size = EditorSize;
            editor.relativePosition = EditorPosition;

            Editors.Add(editor);
        }

        public void SetSegment(ushort segmentID)
        {
            Log.Debug("LaneManagerPanel.SetSegment(" + segmentID.ToString() + ") called");
            Show();
            Caption.text = "Segment " + segmentID.ToString();
            TabStrip.selectedIndex = -1;
            SelectEditor<LaneEditor>();
        }

        private void TabStripSelectedIndexChanged(UIComponent component, int index)
        {
            Log.Debug("LaneManagerPanel.TabStripSelectedIndexChanged(UIComponent component, " + index.ToString() + ") called");
            CurrentEditor = SelectEditor(index);
            UpdatePanel();
        }

        private void CreateSizeChanger()
        {
            Log.Debug("LaneManagerPanel.CreateSizeChanger(): " + size.ToString());
            SizeChanger = AddUIComponent<UIPanel>();
            SizeChanger.size = new Vector2(9, 9);
            SizeChanger.atlas = ResizeAtlas;
            SizeChanger.backgroundSprite = "resize";
            SizeChanger.color = new Color32(255, 255, 255, 160);
            SizeChanger.eventPositionChanged += SizeChangerPositionChanged;

            var handle = SizeChanger.AddUIComponent<UIDragHandle>();
            handle.size = SizeChanger.size;
            handle.relativePosition = Vector2.zero;
            handle.target = SizeChanger;
        }

        private void SizeChangerPositionChanged(UIComponent component, Vector2 value)
        {
            Log.Debug("LaneManagerPanel.SizeChangerPositionChanged(): " + size.ToString());
            size = (Vector2)SizeChanger.relativePosition + SizeChanger.size;
            SizeChanger.relativePosition = size - SizeChanger.size;
        }

        protected override void OnSizeChanged()
        {
            Log.Debug("LaneManagerPanel.OnSizeChanged(): " + size.ToString());
            base.OnSizeChanged();

            if (CurrentEditor != null)
                CurrentEditor.size = EditorSize;
            if (Handle != null)
                Handle.size = new Vector2(size.x, Handle.height);
            if (SizeChanger != null)
                SizeChanger.relativePosition = size - SizeChanger.size;
        }

        protected override void OnVisibilityChanged()
        {
            base.OnVisibilityChanged();

            if (isVisible)
            {
                UpdatePanel();
                OnSizeChanged();
            }
        }

        private BaseEditor SelectEditor(int index)
        {
            Log.Debug("LaneManagerPanel.SelectEditor(" + index.ToString() + ") called");
            if (index >= 0 && Editors.Count > index)
            {
                foreach (var editor in Editors)
                {
                    editor.isVisible = false;
                }

                Editors[index].isVisible = true;
                return Editors[index];
            }
            else
                return null;
        }
        private EditorType SelectEditor<EditorType>() where EditorType : BaseEditor
        {
            Log.Debug("LaneManagerPanel.SelectEditor<EditorType>() called");
            var editorIndex = GetEditor(typeof(EditorType));
            TabStrip.selectedIndex = editorIndex;
            return Editors[editorIndex] as EditorType;
        }
        private int GetEditor(Type editorType) => Editors.FindIndex((e) => e.GetType() == editorType);

    }
}
