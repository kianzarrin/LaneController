using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KianCommons;
using LaneConroller.UI;
using UnityEngine;
using static ToolBase;

namespace LaneConroller.Tool
{
    public abstract class BaseTool
    {
        public abstract ToolType Type { get; }
        public virtual bool ShowPanel => true;
        protected LaneConrollerTool Tool => LaneConrollerTool.Instance;
        protected LaneConrollerPanel Panel => LaneConrollerPanel.Instance;

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

        public virtual void OnSecondaryMouseClicked() {
            if (Type >= (ToolType)1) {
                Tool.SetMode(Type - 1);
            } else {
                Tool.enabled = false;
            }
        }


        public virtual void SimulationStep() { }
    }

    public enum ToolType {
        Initial       = 0x0,
        SelectSegment = 0x0,
        SelectLane    = 0x1,
        ModifyLane    = 0x2,
    }
}
