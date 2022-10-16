using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathController.UI;
using UnityEngine;

namespace PathController.Tool
{
    public abstract class BaseTool
    {
        public abstract ToolType Type { get; }
        public virtual bool ShowPanel => true;
        protected PathManagerExtendedTool Tool => PathManagerExtendedTool.Instance;
        protected PathManagerExtendedPanel Panel => PathManagerExtendedPanel.Instance;

        public virtual void Init() => Reset();
        public virtual void DeInit() { }
        protected virtual void Reset() { }

        public virtual void OnUpdate() { }
        public virtual void OnGUI(Event e) { }
        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo) { }

        public virtual void OnMouseDown(Event e) { }
        public virtual void OnMouseDrag(Event e) { }

        public virtual void OnKeyUp(Event e) { }
        public virtual void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public virtual void OnPrimaryMouseClicked(Event e) { }
        public virtual void OnSecondaryMouseClicked() { }
    }

    public enum ToolType
    {
        None                    = 0x0,
        SelectInstance          = 0x1,
        SelectLane              = 0x2,
        ModifyLane              = 0x4,
        DragBezierPoint         = 0x8,
        SelectMultipleInstances = 0x10,
    }
}
