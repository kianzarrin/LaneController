namespace PathController.UI;
using ColossalFramework.UI;
using PathController.UI.Editors;
using System;
using System.Collections.Generic;
using UnityEngine;
using KianCommons;
using KianCommons.UI;

public class PathControllerPanel : UIPanel
{
    public static PathControllerPanel Instance { get; private set; }
    private PanelHeader Header { get; set; }
    private UILabel Caption { get; set; }
    private CustomUITabstrip TabStrip { get; set; }
    private UIPanel SizeChanger { get; set; }
    public List<BaseEditor> Editors { get; } = new List<BaseEditor>();
    public BaseEditor CurrentEditor { get; set; }

    private Vector2 EditorSize => size - new Vector2(0, Header.height + TabStrip.height);
    private Vector2 EditorPosition => new Vector2(0, TabStrip.relativePosition.y + TabStrip.height);

    private static float TabStripHeight => 20;

    public static UITextureAtlas ResizeAtlas { get; } = GetResizeIcon();
    private static UITextureAtlas GetResizeIcon()
    {
        return TextureUtil.GetAtlasOrNull(nameof(ResizeAtlas)) ??
            TextureUtil.CreateTextureAtlas("resize.png", nameof(ResizeAtlas), 9, 9, new string[] { "resize" }, space: 2);
    }

    public static PathControllerPanel CreatePanel()
    {
        var uiView = UIView.GetAView();
        Instance = uiView.AddUIComponent(typeof(PathControllerPanel)) as PathControllerPanel;
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

        CreateHeader();
        CreateTabStrip();
        CreateEditors();
        CreateSizeChanger();

        size = new Vector2(550, Header.height + TabStrip.height + 400);
        minimumSize = new Vector2(500, Header.height + TabStrip.height + 200);
    }

    public void UpdatePanel() => CurrentEditor?.UpdateEditor();
    public void Render(RenderManager.CameraInfo cameraInfo) => CurrentEditor?.Render(cameraInfo);
    private void CreateHeader()
    {
        Header = AddUIComponent<PanelHeader>();
        Header.relativePosition = new Vector2(0, 0);
        Header.Target = parent;
        Header.Init(500);
    }

    private void CreateTabStrip()
    {
        TabStrip = AddUIComponent<CustomUITabstrip>();
        TabStrip.relativePosition = new Vector3(0, Header.height);
        TabStrip.eventSelectedIndexChanged += TabStripSelectedIndexChanged;
        TabStrip.selectedIndex = -1;
    }

    private void CreateEditors()
    {
        Log.Debug("LaneManagerPanel.CreateEditors() called");
        CreateEditor<LaneEditor>();
        //CreateEditor<TemplateEditor>();
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

    public void SetSegment(ushort segmentId)
    {
        Log.Debug("LaneManagerPanel.SetSegment(" + segmentId.ToString() + ") called");
        Show();
        Caption.text = "Segment " + segmentId.ToString();
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
        if (Header != null)
            Header.size = new Vector2(size.x, Header.height);
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
