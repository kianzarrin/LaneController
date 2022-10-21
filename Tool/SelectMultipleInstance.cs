using PathController.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathController.Tool
{
    class SelectMultipleInstancesTool : SelectInstanceTool
    {
        public List<ushort> SelectedSegmentIDs = new List<ushort> { };

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            for (int i = 0; i < SelectedSegmentIDs.Count; i++)
            {
                RenderUtil.RenderAutoCutSegmentOverlay(cameraInfo, SelectedSegmentIDs[i], Color.magenta, true);
            }
        }

        public override void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public override void OnPrimaryMouseClicked(Event e)
        {
            if (HoveredSegmentId != 0)
            {
                if (SelectedSegmentIDs.Contains(HoveredSegmentId))
                    SelectedSegmentIDs.Remove(HoveredSegmentId);
                else
                    SelectedSegmentIDs.Add(HoveredSegmentId);
            }
        }

        public override void OnSecondaryMouseClicked()
        {
            SelectedSegmentIDs.Clear();
            Tool.DisableTool();
        }
    }
}
