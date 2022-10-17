using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KianCommons;
using PathController.CustomData;
using PathController.UI.Data;
using PathController.Util;

namespace PathController.UI.Editors
{
    public class TemplateEditor : BaseEditor<LaneItem, CustomData.CustomLane, LaneIcons>
    {
        public override string Name => "Template Editor";
        public override string SelectionMessage => "No Templates";

        public override void Render(RenderManager.CameraInfo cameraInfo)
        {

        }
    }
}
