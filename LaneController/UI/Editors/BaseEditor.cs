using ColossalFramework;
using ColossalFramework.UI;
using LaneConroller.Tool;
using LaneConroller.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using KianCommons;
using KianCommons.UI;

namespace LaneConroller.UI.Editors
{
    public abstract class BaseEditor : UIPanel
    {
        public enum LaneType
        {
            [Description("Bicycle")]
            Bicycle,

            [Description("Car")]
            Car,

            [Description("Tram")]
            Tram,

            [Description("Pedestrian")]
            Pedestrian,

            [Description("Cablecar")]
            Cablecar,

            [Description("Monorail")]
            Monorail,

            [Description("Metro")]
            Metro,

            [Description("TrolleyBus")]
            Trolleybus,

            [Description("Train")]
            Train,

            [Description("Parking")]
            Parking,

            [Description("None")]
            None,

            [Description("Multiple")]
            Multiple,
        }

        protected LaneConrollerTool ToolInstance => LaneConrollerTool.Instance;

        public static Dictionary<LaneType, string> LaneSpriteNames { get; set; }
        public static Dictionary<NetInfo.Direction, string> DirectionSpriteNames { get; set; }
        public static UITextureAtlas LaneAtlas { get; } = GetLaneIcons();
        public static UITextureAtlas DirectionAtlas { get; } = GetDirectionIcons();

        #region Icons
        private static UITextureAtlas GetLaneIcons()
        {
            Log.Debug("BaseEditor.GetLaneIcons() called");
            LaneSpriteNames = new Dictionary<LaneType, string>()
            {
                {LaneType.Bicycle,    nameof(LaneType.Bicycle) },
                {LaneType.Car,        nameof(LaneType.Car) },
                {LaneType.Tram,       nameof(LaneType.Tram) },
                {LaneType.Pedestrian, nameof(LaneType.Pedestrian) },
                {LaneType.Cablecar,   nameof(LaneType.Cablecar) },
                {LaneType.Monorail,   nameof(LaneType.Monorail) },
                {LaneType.Metro,      nameof(LaneType.Metro) },
                {LaneType.Trolleybus, nameof(LaneType.Trolleybus) },
                {LaneType.Train,      nameof(LaneType.Train) },
                {LaneType.Parking,    nameof(LaneType.Parking) },
                {LaneType.None,       nameof(LaneType.None) },
                {LaneType.Multiple,   nameof(LaneType.Multiple) },
            };

            return TextureUtil.GetAtlasOrNull(nameof(LaneAtlas)) ??
                TextureUtil.CreateTextureAtlas("laneTypes.png", nameof(LaneAtlas), 64, 64, LaneSpriteNames.Values.ToArray());
        }

        private static UITextureAtlas GetDirectionIcons()
        {
            Log.Debug("BaseEditor.GetDirectionIcons() called");
            DirectionSpriteNames = new Dictionary<NetInfo.Direction, string>()
            {
                {NetInfo.Direction.None,          "DirectionNone" },
                {NetInfo.Direction.Forward,       "DirectionForward" },
                {NetInfo.Direction.Backward,      "DirectionBackward" },
                {NetInfo.Direction.Both,          "DirectionBoth" },
                {NetInfo.Direction.Avoid,         "DirectionAvoid" },
                {NetInfo.Direction.AvoidForward,  "DirectionAvoidForward" },
                {NetInfo.Direction.AvoidBackward, "DirectionAvoidBackward" },
                {NetInfo.Direction.AvoidBoth,     "DirectionAvoidBoth" },
            };

            return TextureUtil.GetAtlasOrNull(nameof(DirectionAtlas)) ??
                TextureUtil.CreateTextureAtlas("directions.png", nameof(DirectionAtlas), 64, 64, DirectionSpriteNames.Values.ToArray());
        }
        #endregion

        public LaneConrollerPanel LaneManagerPanel { get; private set; }

        protected UIScrollablePanel ItemsPanel { get; set; }
        protected UIScrollablePanel SettingsPanel { get; set; }

        protected UILabel SelectionLabel { get; set; }

        public abstract string Name { get; }
        public abstract string SelectionMessage { get; }

        public BaseEditor()
        {
            clipChildren = true;
            atlas = TextureUtil.InGameAtlas;
            backgroundSprite = "UnlockingItemBackground";

            AddItemsPanel();
            AddSettingPanel();
            AddSelectionLabel();
        }

        #region ItemsPanel
        private void AddItemsPanel()
        {
            Log.Debug("BaseEditor.AddItemsPanel() called");
            ItemsPanel = AddUIComponent<UIScrollablePanel>();
            ItemsPanel.autoLayout = true;
            ItemsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            ItemsPanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            ItemsPanel.scrollWheelDirection = UIOrientation.Vertical;
            ItemsPanel.builtinKeyNavigation = true;
            ItemsPanel.clipChildren = true;
            ItemsPanel.eventSizeChanged += ItemsPanelSizeChanged;
            ItemsPanel.atlas = TextureUtil.InGameAtlas;
            ItemsPanel.backgroundSprite = "ScrollbarTrack";

            UIUtils.AddScrollbar(this, ItemsPanel);

            ItemsPanel.verticalScrollbar.eventVisibilityChanged += ItemsScrollbarVisibilityChanged;
        }

        private void ItemsPanelSizeChanged(UIComponent component, Vector2 value)
        {
            Log.Debug("BaseEditor.ItemsPanelSizeChanged() called.");
            foreach (var item in ItemsPanel.components)
                item.width = ItemsPanel.width;
        }

        private void ItemsScrollbarVisibilityChanged(UIComponent component, bool value)
        {
            Log.Debug("BaseEditor.ItemsScrollbarVisibilityChanged() called.");
            ItemsPanel.width = size.x / 10 * 3 - (ItemsPanel.verticalScrollbar.isVisible ? ItemsPanel.verticalScrollbar.width : 0);
            ItemsPanel.verticalScrollbar.relativePosition = ItemsPanel.relativePosition + new Vector3(ItemsPanel.width, 0);
        }
        #endregion

        #region SettingsPanel
        private void AddSettingPanel()
        {
            SettingsPanel = AddUIComponent<UIScrollablePanel>();
            SettingsPanel.autoLayout = true;
            SettingsPanel.autoLayoutDirection = LayoutDirection.Vertical;
            SettingsPanel.autoLayoutPadding = new RectOffset(10, 10, 10, 10);
            SettingsPanel.scrollWheelDirection = UIOrientation.Vertical;
            SettingsPanel.builtinKeyNavigation = true;
            SettingsPanel.clipChildren = true;
            SettingsPanel.atlas = TextureUtil.InGameAtlas;
            SettingsPanel.backgroundSprite = "UnlockingItemBackground";
            SettingsPanel.eventSizeChanged += SettingsPanelSizeChanged;

            UIUtils.AddScrollbar(this, SettingsPanel);

            SettingsPanel.verticalScrollbar.eventVisibilityChanged += SettingsScrollbarVisibilityChanged;
        }
        private void SettingsPanelSizeChanged(UIComponent component, Vector2 value)
        {
            foreach (var item in SettingsPanel.components)
                item.width = SettingsPanel.width - SettingsPanel.autoLayoutPadding.horizontal;
        }
        private void SettingsScrollbarVisibilityChanged(UIComponent component, bool value)
        {
            SettingsPanel.width = size.x / 10 * 7 - (value ? SettingsPanel.verticalScrollbar.width : 0);
            SettingsPanel.verticalScrollbar.relativePosition = SettingsPanel.relativePosition + new Vector3(SettingsPanel.width, 0);
        }
        #endregion

        #region SelectionLabel
        private void AddSelectionLabel()
        {
            SelectionLabel = AddUIComponent<UILabel>();
            SelectionLabel.textAlignment = UIHorizontalAlignment.Center;
            SelectionLabel.verticalAlignment = UIVerticalAlignment.Middle;
            SelectionLabel.padding = new RectOffset(10, 10, 0, 0);
            SelectionLabel.wordWrap = true;
            SelectionLabel.autoSize = false;

            SwitchEmptySelected();
        }
        protected void SwitchEmptySelected()
        {
            Log.Debug("BaseEditor.SwitchEmptySelected() called.");
            if (ItemsPanel.components.Any())
                SelectionLabel.isVisible = false;
            else
            {
                SelectionLabel.isVisible = true;
                SelectionLabel.text = SelectionMessage;
            }
        }

        protected void ShowEmptySelected()
        {
            Log.Debug("BaseEditor.ShowEmptySelected() called.");
            SelectionLabel.text = SelectionMessage;
            SelectionLabel.isVisible = true;
        }

        protected void HideEmptySelected()
        {
            Log.Debug("BaseEditor.HideEmptySelected() called.");
            SelectionLabel.isVisible = false;
        }
        #endregion

        public virtual void Init(LaneConrollerPanel panel)
        {
            Log.Debug("BaseEditor.Init() called.");
            LaneManagerPanel = panel;
        }


        public virtual void UpdateEditor() { }
        public virtual void Render(RenderManager.CameraInfo cameraInfo) { }
        protected virtual void ClearItems() { }
        protected virtual void ClearSettings() { }
        protected virtual void FillItems() { }
        public virtual void Select(int index) { }
        protected virtual void Deselect() { }
        protected abstract void ItemClick(UIComponent component, UIMouseEventParameter eventParam);
        protected abstract void ItemHover(UIComponent component, UIMouseEventParameter eventParam);
        protected abstract void ItemLeave(UIComponent component, UIMouseEventParameter eventParam);
    }

    public abstract class BaseEditor<DynamicButtonType, DynamicObject, ItemIcon> : BaseEditor
        where DynamicButtonType : DynamicButton<DynamicObject, ItemIcon>
        where ItemIcon : UIComponent
        where DynamicObject : class
    {
        DynamicButtonType _selectItem;

        protected DynamicButtonType HoverItem { get; set; }
        protected bool IsHoverItem => HoverItem != null;
        protected bool IsSelectItem => SelectItem != null;
        protected DynamicButtonType SelectItem
        {
            get => _selectItem;
            private set
            {
                if (_selectItem != null)
                    _selectItem.IsSelect = false;

                _selectItem = value;

                if (_selectItem != null)
                    _selectItem.IsSelect = true;
            }
        }
        public DynamicObject EditObject => SelectItem?.Object;

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            var itemsPanelWidth = size.x / 10 * 3 - (ItemsPanel.verticalScrollbar.isVisible ? ItemsPanel.verticalScrollbar.width : 0);
            ItemsPanel.size = new Vector2(itemsPanelWidth, size.y);
            ItemsPanel.relativePosition = new Vector2(0, 0);

            var settingsPanelWidth = size.x / 10 * 7 - (SettingsPanel.verticalScrollbar.isVisible ? SettingsPanel.verticalScrollbar.width : 0);
            SettingsPanel.size = new Vector2(settingsPanelWidth, size.y);
            SettingsPanel.relativePosition = new Vector2(size.x / 10 * 3, 0);

            SelectionLabel.size = new Vector2(size.x / 10 * 7, size.y / 2);
            SelectionLabel.relativePosition = SettingsPanel.relativePosition;
        }

        #region Item Functions
        protected virtual DynamicButtonType GetItem(DynamicObject editObject) => ItemsPanel.components.OfType<DynamicButtonType>().FirstOrDefault(c => ReferenceEquals(c.Object, editObject));
        public virtual DynamicButtonType AddItem(DynamicObject editableObject)
        {
            Log.Debug("BaseEditor<>.AddItem() called.");
            var item = NewItem();
            InitItem(item, editableObject);

            return item;
        }
        protected DynamicButtonType NewItem()
        {
            Log.Debug("BaseEditor<>.NewItem() called.");
            var newItem = ItemsPanel.AddUIComponent<DynamicButtonType>();
            newItem.width = ItemsPanel.width;

            return newItem;
        }
        protected void InitItem(DynamicButtonType item, DynamicObject editableObject)
        {
            Log.Debug("BaseEditor<>.InitItem() called.");
            item.Text = editableObject.ToString();
            item.Init();
            item.Object = editableObject;
            item.eventClick += ItemClick;
            item.eventMouseEnter += ItemHover;
            item.eventMouseLeave += ItemLeave;
            item.OnDelete += ItemDelete;
        }
        protected void ItemDelete(DynamicButton<DynamicObject, ItemIcon> deleteItem)
        {
            Log.Debug("BaseEditor<>.ItemDelete() called.");
            if (!(deleteItem is DynamicButtonType item))
                return;

            /*if (Settings.DeleteWarnings)
            {
                var messageBox = MessageBoxBase.ShowModal<YesNoMessageBox>();
                messageBox.CaprionText = string.Format(NodeMarkup.Localize.Editor_DeleteCaption, item.DeleteCaptionDescription);
                messageBox.MessageText = string.Format(NodeMarkup.Localize.Editor_DeleteMessage, item.DeleteMessageDescription, item.Object);
                messageBox.OnButton1Click = Delete;
            }
            else */
                 Delete();

            bool Delete()
            {
                OnObjectDelete(item.Object);
                var isSelect = item == SelectItem;
                DeleteItem(item);
                if (isSelect)
                {
                    //ClearSettings();
                    Select(0);
                }
                return true;
            }
        }
        protected virtual void DeleteItem(DynamicButtonType item)
        {
            Log.Debug("BaseEditor<>.DeleteItem() called.");
            DeInitItem(item);
            DeleteUIComponent(item);

            SwitchEmptySelected();
        }
        protected void DeInitItem(DynamicButtonType item)
        {
            Log.Debug("BaseEditor<>.DeInitItem() called.");
            item.eventClick -= ItemClick;
            item.eventMouseEnter -= ItemHover;
            item.eventMouseLeave -= ItemLeave;
        }
        #endregion

        protected void DeleteUIComponent(UIComponent item)
        {
            Log.Debug("BaseEditor<>.DeleteUIComponent() called.");
            ItemsPanel.RemoveUIComponent(item);
            Destroy(item.gameObject);
        }
        protected override void ClearItems()
        {
            Log.Debug("BaseEditor<>.ClearItems() called.");
            var componets = ItemsPanel.components.ToArray();
            foreach (var item in componets)
                DeleteUIComponent(item);
        }
        public virtual void UpdateEditor(DynamicObject selectObject = null)
        {
            Log.Debug("BaseEditor<>.UpdateEditor() called.");
            var editObject = EditObject;

            if (selectObject != null && selectObject == editObject)
            {
                OnObjectUpdate();
                return;
            }

            //if (ItemsPanel.components.Count > 0 && LaneConrollerTool.Instance.CurrentTool.Type < ToolType.SelectLane)
                ClearItems();
            if (ToolInstance.SegmentInstance != null)
                FillItems();

            if (ToolInstance.LaneInstance != null)
            {
                Select(ToolInstance.LaneInstance.Index);
                SwitchEmptySelected();
            } else
            {
                Deselect();
                ClearSettings();
                //ShowEmptySelected();
            }

            if (!IsSelectItem)
            {
                editObject = null;
            }

            if (selectObject != null && GetItem(selectObject) is DynamicButtonType selectItem)
                Select(selectItem);
            else if (editObject != null && GetItem(editObject) is DynamicButtonType editItem)
            {
                Log.Debug("BaseEditor<>.UpdateEditor() called.\n\t\teditObject check.");
                SelectItem = editItem;
                ScrollTo(SelectItem);
                OnObjectUpdate();
            }
            else
            {
                //SelectItem = null;
                //ClearSettings();
                //SelectSegment(0);
            }

            //SwitchSelected();
        }
        public override void UpdateEditor() => UpdateEditor(null);

        protected override void ClearSettings()
        {
            var componets = SettingsPanel.components.ToArray();
            foreach (var item in componets)
            {
                if (item == SelectionLabel)
                    continue;
                SettingsPanel.RemoveUIComponent(item);
                Destroy(item.gameObject);
            }
        }

        protected override void ItemClick(UIComponent component, UIMouseEventParameter eventParam) => ItemClick((DynamicButtonType)component);
        protected virtual void ItemClick(DynamicButtonType item)
        {
            Log.Debug("BaseEditor<>.ItemClick()");
            SettingsPanel.autoLayout = false;
            //ClearSettings();
            SelectItem = item;
            OnObjectSelect();
            Log.Debug($"BaseEditor<>.ItemClick(): SelectItem != null, {SelectItem != null}");
            SettingsPanel.autoLayout = true;
        }
        protected override void ItemHover(UIComponent component, UIMouseEventParameter eventParam)
        {
            HoverItem = component as DynamicButtonType;
        }
        protected override void ItemLeave(UIComponent component, UIMouseEventParameter eventParam) => HoverItem = null;
        protected virtual void OnObjectSelect() { }
        protected virtual void OnObjectDelete(DynamicObject editableObject) { }
        protected virtual void OnObjectUpdate() { }
        protected override void Deselect()
        {
            if (IsSelectItem)
            {
                Log.Debug("BaseEditor<>.Deselect() called.");
                SelectItem.Unfocus();
                ShowEmptySelected();
                SelectItem = null;
                ToolInstance.SetLane(-1);
            }
        }
        public override void Select(int index)
        {
            if (ItemsPanel.components.Count > index && ItemsPanel.components[index] is DynamicButtonType item)
                Select(item);
        }
        public virtual void Select(DynamicButtonType item)
        {
            Log.Debug("BaseEditor<>.SelectSegment(DynamicButtonType item)");
            item.SimulateClick();
            item.Focus();
            ScrollTo(item);
        }
        public virtual void ScrollTo(DynamicButtonType item)
        {
            ItemsPanel.ScrollToBottom();
            ItemsPanel.ScrollIntoView(item);
        }
        protected virtual void RefreshItems()
        {
            foreach (var item in ItemsPanel.components.OfType<DynamicButtonType>())
                item.Refresh();
        }
    }
}
