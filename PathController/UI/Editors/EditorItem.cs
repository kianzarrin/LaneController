using ColossalFramework.UI;
using KianCommons.UI;
using PathController.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathController.UI.Editors {
    public abstract class EditorItem : UIPanel {
        protected const float defaultHeight = 30f;

        public static UITextureAtlas EditorItemAtlas { get; } = GetAtlas();
        private static UITextureAtlas GetAtlas() {
            var spriteNames = new string[]
            {
                "TextFieldPanel",
                "TextFieldPanelHovered",
                "TextFieldPanelFocus",
                "EmptySprite"
            };

            return TextureUtil.GetAtlasOrNull(nameof(EditorItemAtlas)) ??
                TextureUtil.CreateTextureAtlas("TextFieldPanel.png", nameof(EditorItemAtlas), 32, 32, spriteNames, new RectOffset(4, 4, 4, 4), 2);
        }
        public virtual void Init() => Init(defaultHeight);
        public void Init(float height) {
            if (parent is UIScrollablePanel scrollablePanel)
                width = scrollablePanel.width - scrollablePanel.autoLayoutPadding.horizontal;
            else if (parent is UIPanel panel)
                width = panel.width - panel.autoLayoutPadding.horizontal;
            else
                width = parent.width;

            this.height = height;
        }

        protected UIButton AddButton(UIComponent parent) {
            var button = parent.AddUIComponent<UIButton>();
            button.SetDefaultStyle();
            return button;
        }
    }
}