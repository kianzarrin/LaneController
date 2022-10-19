using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KianCommons;
using PathController.UI;
using UnityEngine;
using static ToolBase;

namespace PathController.Tool
{
    public abstract class BaseTool
    {
        public abstract ToolType Type { get; }
        public virtual bool ShowPanel => true;
        protected PathControllerTool Tool => PathControllerTool.Instance;
        protected PathControllerExtendedPanel Panel => PathControllerExtendedPanel.Instance;

        public virtual void Init() => Reset();
        public virtual void DeInit() { }
        protected virtual void Reset() { }

        public virtual void OnUpdate() { }
        public virtual void OnGUI(Event e) { }
        public virtual void RenderOverlay(RenderManager.CameraInfo cameraInfo) { }

        public virtual void OnMouseDown(Event e) { }
        public virtual void OnMouseDrag(Event e) { }

        public virtual void OnKeyUp(Event e) {
            Log.Called(e);
            var del = e.keyCode == KeyCode.Delete || e.keyCode == KeyCode.Backspace;
            if (del) {
                var control = Panel?.
                    GetComponentsInChildren<FieldPropertyPanel>()?.
                    FirstOrDefault(control => control.containsMouse);
                if (control != null) {
                    control.ResetValue();
                }
            }
        }
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
