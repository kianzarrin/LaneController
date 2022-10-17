using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PathController.Util;
using UnityEngine;
using static ToolBase;

namespace PathController.Tool
{
    public class SelectLaneTool : BaseTool
    {
        public override ToolType Type => ToolType.SelectLane;

        public ushort HoveredSegmentId { get; private set; } = 0;

        protected bool IsHover => (HoveredSegmentId != 0);

        protected bool HoverValid => PathManagerExtendedTool.MouseRayValid && IsHover;
        public int HoveredLaneIndex { get; private set; } = 0;

        protected override void Reset()
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            RaycastInput segmentInput = new RaycastInput(PathManagerExtendedTool.MouseRay, PathManagerExtendedTool.MouseRayLength)
            {
                m_ignoreTerrain = true,
                m_ignoreSegmentFlags = NetSegment.Flags.None,
                m_ignoreNodeFlags = NetNode.Flags.All
            };
            segmentInput.m_netService.m_itemLayers = (ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels);
            segmentInput.m_netService.m_service = ItemClass.Service.Road;

            if (PathManagerExtendedTool.RayCast(segmentInput, out RaycastOutput segmentOutput))
            {
                HoveredSegmentId = segmentOutput.m_netSegment;
            } else {
                HoveredSegmentId = 0;
            }

            if (Tool.SegmentInstance.Segment.GetClosestLanePosition(PathManagerExtendedTool.MouseWorldPosition, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All, out _, out uint laneID, out _, out _))
            {
                for (int i = 0; i < Tool.SegmentInstance.Lanes.Length; i++)
                {
                    if (Tool.SegmentInstance.Lanes[i].LaneID == laneID)
                    {
                        HoveredLaneIndex = i;
                        break;
                    }
                }
            }
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (HoverValid)
            {
                RenderUtil.RenderLaneOverlay(cameraInfo, Tool.SegmentInstance.Lanes[HoveredLaneIndex].LaneIdAndIndex, Color.yellow, true);
            }
        }

        public override void OnSecondaryMouseClicked() => Tool.SetDefaultMode();

        public override void OnPrimaryMouseClicked(Event e)
        {
            //Tool.SetLane(HoveredLaneIndex);
            //Tool.SetMode(ToolType.ModifyLane);
            //return;
        }
    }
}
