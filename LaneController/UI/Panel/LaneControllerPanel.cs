namespace LaneConroller.UI;
using ColossalFramework.UI;
using LaneConroller.UI.Editors;
using System;
using System.Collections.Generic;
using UnityEngine;
using KianCommons;
using KianCommons.UI;

public class LaneConrollerPanel : UIPanel
{
    public static LaneConrollerPanel Instance { get; private set; }
    private PanelHeader Header { get; set; }
    private CustomUITabstrip TabStrip { get; set; }
    private UIPanel SizeChanger { get; set; }
    public List<BaseEditor> Editors { get; } = new List<BaseEditor>();
    public BaseEditor CurrentEditor { get; set; }

    private Vector2 EditorSize => size - new Vector2(0, HeaderHeight + TabStrip.height);
    private Vector2 EditorPosition => new Vector2(0, TabStrip.relativePosition.y + TabStrip.height);

    private static float TabStripHeight => 20;
    private static float HeaderHeight => 42;

    public static UITextureAtlas ResizeAtlas { get; } = GetResizeIcon();
    private static UITextureAtlas GetResizeIcon()
    {
        return TextureUtil.GetAtlasOrNull(nameof(ResizeAtlas)) ??
            TextureUtil.CreateTextureAtlas("resize.png", nameof(ResizeAtlas), 9, 9, new string[] { "resize" }, space: 2);
    }

    public static LaneConrollerPanel CreatePanel()
    {
        var uiView = UIView.GetAView();
        Instance = uiView.AddUIComponent(typeof(LaneConrollerPanel)) as LaneConrollerPanel;
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

        size = new Vector2(550, HeaderHeight + TabStrip.height + 400);
        minimumSize = new Vector2(500, HeaderHeight + TabStrip.height + 200);
    }

    public void UpdatePanel() => CurrentEditor?.UpdateEditor();

    public void Render(RenderManager.CameraInfo cameraInfo) => CurrentEditor?.Render(cameraInfo);
    private void CreateHeader()
    {
        Header = AddUIComponent<PanelHeader>();
        Header.relativePosition = new Vector2(0, 0);
        Header.Target = parent;
        Header.Init(HeaderHeight);
    }

    private void CreateTabStrip()
    {
        TabStrip = AddUIComponent<CustomUITabstrip>();
        TabStrip.relativePosition = new Vector3(0, HeaderHeight);
        TabStrip.eventSelectedIndexChanged += TabStripSelectedIndexChanged;
        TabStrip.selectedIndex = -1;
    }

    private void CreateEditors()
    {
        Log.Called();
        CreateEditor<LaneEditor>();
        //CreateEditor<TemplateEditor>();
    }

    private void CreateEditor<EditorType>() where EditorType : BaseEditor
    {
        Log.Called();
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
        Log.Called(segmentId);
        Show();
        Header.Text = $"Segment #{segmentId}";
        TabStrip.selectedIndex = -1;
        SelectEditor<LaneEditor>();
    }

    private void TabStripSelectedIndexChanged(UIComponent component, int index)
    {
        Log.Called(component, index);
        CurrentEditor = SelectEditor(index);
        UpdatePanel();
    }

    private void CreateSizeChanger()
    {
        Log.Called(size);
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
        Log.Called(value);
        size = (Vector2)SizeChanger.relativePosition + SizeChanger.size;
        SizeChanger.relativePosition = size - SizeChanger.size;
    }

    protected override void OnSizeChanged()
    {
        Log.Called(size);
        base.OnSizeChanged();

        if (CurrentEditor != null)
            CurrentEditor.size = EditorSize;
        if (Header != null)
            Header.size = new Vector2(size.x, HeaderHeight);
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
        Log.Called(index);
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
        Log.Called();
        var editorIndex = GetEditor(typeof(EditorType));
        TabStrip.selectedIndex = editorIndex;
        return Editors[editorIndex] as EditorType;
    }
    private int GetEditor(Type editorType) => Editors.FindIndex((e) => e.GetType() == editorType);

}
