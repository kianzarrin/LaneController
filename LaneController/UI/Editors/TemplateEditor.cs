using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KianCommons;
using LaneConroller.CustomData;
using LaneConroller.Util;

namespace LaneConroller.UI.Editors
{
    public class TemplateEditor : BaseEditor<LaneItem, CustomLane, LaneIcons>
    {
        public override string Name => "Template Editor";
        public override string SelectionMessage => "No Templates";

        public override void Render(RenderManager.CameraInfo cameraInfo)
        {

        }
    }
}
