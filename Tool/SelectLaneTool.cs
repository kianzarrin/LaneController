using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ColossalFramework.Math;
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

        protected bool HoverValid => PathControllerTool.MouseRayValid && IsHover;
        public int HoveredLaneIndex { get; private set; } = 0;

        protected override void Reset()
        {
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            RaycastInput segmentInput = new RaycastInput(PathControllerTool.MouseRay, PathControllerTool.MouseRayLength)
            {
                m_ignoreTerrain = true,
                m_ignoreSegmentFlags = NetSegment.Flags.None,
                m_ignoreNodeFlags = NetNode.Flags.All
            };
            segmentInput.m_netService.m_itemLayers = (ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels);
            segmentInput.m_netService.m_service = ItemClass.Service.Road;

            if (PathControllerTool.RayCast(segmentInput, out RaycastOutput segmentOutput))
            {
                HoveredSegmentId = segmentOutput.m_netSegment;
            } else {
                HoveredSegmentId = 0;
            }

            if (Tool.SegmentInstance.Segment.
                GetClosestLanePosition(
                PathControllerTool.MouseWorldPosition, NetInfo.LaneType.All, VehicleInfo.VehicleType.All, VehicleInfo.VehicleCategory.All,
                out var hitPos, out uint laneID, out var laneIndex, out _))
            {
                HoveredLaneIndex = laneIndex;
            }
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            if (HoverValid && HoveredLaneIndex >=0) {
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
