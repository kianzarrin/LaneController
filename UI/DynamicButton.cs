using ColossalFramework.UI;
using PathController.Util;
using PathController.UI.Editors;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KianCommons.UI;

namespace PathController.UI
{
    public abstract class DynamicButton : UIButton
    {
        public static UITextureAtlas ItemAtlas { get; } = GetStyleIcons();
        private static UITextureAtlas GetStyleIcons()
        {
            var spriteNames = new string[] { "Item" };

            return TextureUtil.GetAtlasOrNull(nameof(ItemAtlas))??
                TextureUtil.CreateTextureAtlas("ListItem.png", nameof(ItemAtlas), 21, 26, spriteNames, new RectOffset(1, 1, 1, 1));
        }

        public virtual Color32 NormalColor => new Color32(29, 58, 77, 255);
        public virtual Color32 HoveredColor => new Color32(44, 87, 112, 255);
        public virtual Color32 PressedColor => new Color32(51, 100, 132, 255);
        public virtual Color32 FocusColor => new Color32(171, 185, 196, 255);
        public virtual Color32 TextColor => Color.white;

        private bool _isSelect;
        public bool IsSelect
        {
            get => _isSelect;
            set
            {
                if (_isSelect != value)
                {
                    _isSelect = value;
                    OnSelectChanged();
                }
            }
        }

        protected UILabel Label { get; set; }
        public string Text
        {
            get => Label.text;
            set => Label.text = value;
        }

        public DynamicButton()
        {
            AddLabel();

            atlas = ItemAtlas;
            normalBgSprite = "Item";
            height = 25;

            OnSelectChanged();
        }

        private void AddLabel()
        {
            Label = AddUIComponent<UILabel>();
            Label.textAlignment = UIHorizontalAlignment.Left;
            Label.verticalAlignment = UIVerticalAlignment.Middle;
            Label.autoSize = false;
            Label.autoHeight = false;
            Label.textScale = 0.55f;
            Label.padding = new RectOffset(50, 0, 7, 0);
            Label.autoHeight = true;
            Label.wordWrap = true;
        }

        protected virtual void OnSelectChanged()
        {
            color = NormalColor;
            hoveredColor = HoveredColor;
            pressedColor = PressedColor;
            focusedColor = FocusColor;

            Label.textColor = TextColor;
        }
    }
    public abstract class DynamicButton<DynamicObject, IconType> : DynamicButton
        where IconType : UIComponent
        where DynamicObject : class
    {
        public event Action<DynamicButton<DynamicObject, IconType>> OnDelete;

        DynamicObject _object;
        private bool Inited { get; set; } = false;
        public abstract string DeleteCaptionDescription { get; }
        public abstract string DeleteMessageDescription { get; }
        public DynamicObject Object
        {
            get => _object;
            set
            {
                _object = value;
                Refresh();
                OnObjectSet();
            }
        }
        protected IconType Icon { get; set; }
        private UIButton DeleteButton { get; set; }

        public bool ShowIcon { get; set; }
        public bool ShowDelete { get; set; }

        public DynamicButton()
        {
            Label.eventSizeChanged += LabelSizeChanged;
        }

        public abstract void Init();
        public void Init(bool showIcon, bool showDelete)
        {
            if (Inited)
                return;

            ShowIcon = showIcon;
            ShowDelete = showDelete;

            if (ShowIcon)
                AddIcon();
            if (ShowDelete)
                AddDeleteButton();

            OnSizeChanged();

            Inited = true;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            var labelWidth = size.x;
            if (ShowIcon)
            {
                //Icon.size = new Vector2(size.x /2, size.y);
                Icon.relativePosition = new Vector2(0, 0);
                labelWidth -= 50;
            }

            if (ShowDelete)
            {
                DeleteButton.size = new Vector2(size.x - 6, size.y - 6);
                DeleteButton.relativePosition = new Vector2(size.x - (size.y - 3), 3);
                labelWidth -= 19;
            }

            Label.size = new Vector2(ShowIcon ? labelWidth : labelWidth - 3, size.y);
            //Label.relativePosition = new Vector3(ShowIcon ? size.y : 3, (size.y - Label.height) / 2);
        }
        private void LabelSizeChanged(UIComponent component, Vector2 value) => Label.relativePosition = new Vector3(ShowIcon ? size.y : 3, (size.y - Label.height) / 2);

        private void AddIcon()
        {
            Icon = AddUIComponent<IconType>();
            Icon.size = new Vector2(50, 25);
        }

        private void AddDeleteButton()
        {
            DeleteButton = AddUIComponent<UIButton>();
            DeleteButton.atlas = TextureUtil.InGameAtlas;
            DeleteButton.normalBgSprite = "buttonclose";
            DeleteButton.hoveredBgSprite = "buttonclosehover";
            DeleteButton.pressedBgSprite = "buttonclosepressed";
            DeleteButton.size = new Vector2(20, 20);
            DeleteButton.isEnabled = ShowDelete;
            DeleteButton.eventClick += DeleteClick;
        }
        protected override void OnSelectChanged()
        {
            if (IsSelect)
            {
                color = FocusColor;
                hoveredColor = FocusColor;
                pressedColor = FocusColor;

                Label.textColor = TextColor;
            }
            else
                base.OnSelectChanged();
        }
        private void DeleteClick(UIComponent component, UIMouseEventParameter eventParam) => OnDelete?.Invoke(this);
        protected virtual void OnObjectSet() { }

        public virtual void Refresh() => Text = Object.ToString();
    }

    public class BaseIcons : UIButton
    {
        private static float Border => 1f;
        public Color32 BorderColor { set => color = value; }
        public BaseIcons()
        {
            isInteractive = false;
            color = new Color32(29, 58, 77, 255);
            relativePosition = new Vector3(0, 0);
            width = 50;
            height = 25;
        }
    }
    public class LaneIcons : BaseIcons
    {
        protected UIButton Lane { get; set; }
        protected UIButton Direction { get; set; }

        public void SetLaneToolTip(string toolTip)
        {
            Lane.tooltip = toolTip;
        }

        public void SetDirectionToolTip(string toolTip)
        {
            Direction.tooltip = toolTip;
        }

        public BaseEditor.LaneType LaneType
        {
            set
            {
                if (!BaseEditor.LaneSpriteNames.TryGetValue(value, out string sprite))
                    sprite = "None";

                Lane.normalBgSprite = Lane.normalFgSprite = sprite;
            }
        }

        public NetInfo.Direction DirectionType
        {
            set
            {
                if (!BaseEditor.DirectionSpriteNames.TryGetValue(value, out string sprite))
                    sprite = "None";

                Direction.normalBgSprite = Direction.normalFgSprite = sprite;
            }
        }

        public void OnTooltipEnter(UIComponent component, UIMouseEventParameter eventParam)
        {
            base.OnTooltipEnter(eventParam);
        }

        public LaneIcons()
        {
            Lane = AddUIComponent<UIButton>();
            Lane.atlas = BaseEditor.LaneAtlas;
            Lane.relativePosition = new Vector3(0, 0);
            Lane.size = new Vector2(25, 25);
            Lane.isInteractive = true;
            Direction = AddUIComponent<UIButton>();
            Direction.atlas = BaseEditor.DirectionAtlas;
            Direction.relativePosition = new Vector3(25, 0);
            Direction.size = new Vector2(25, 25);
            Direction.isInteractive = true;
            Direction.eventTooltipEnter += OnTooltipEnter;
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();
            if (Lane != null)
                Lane.size = new Vector2(size.x /2, size.y);
            if (Direction != null)
                Direction.size = new Vector2(size.x / 2, size.y);
        }
    }
}
