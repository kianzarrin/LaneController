using PathController.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static ToolBase;

namespace PathController.Tool
{
    public class SelectInstanceTool : BaseTool
    {
        public override ToolType Type => ToolType.SelectInstance;
        public override bool ShowPanel => false;

        public ushort HoveredNodeId { get; private set; } = 0;
        public ushort HoveredSegmentId { get; private set; } = 0;
        public List<ushort> SelectedSegmentIDs = new List<ushort> { };
        public List<ushort> SelectedNodeIDs = new List<ushort> { };
        protected bool IsHover => (HoveredSegmentId != 0 || HoveredNodeId != 0);

        protected bool HoverValid => PathManagerExtendedTool.MouseRayValid && IsHover;


        protected override void Reset()
        {
            HoveredNodeId = 0;
            HoveredSegmentId = 0;
        }

        public override void OnUpdate()
        {
            RaycastInput nodeInput = new RaycastInput(PathManagerExtendedTool.MouseRay, PathManagerExtendedTool.MouseRayLength)
            {
                m_ignoreTerrain = true,
                m_ignoreNodeFlags = NetNode.Flags.None,
                m_ignoreSegmentFlags = NetSegment.Flags.All
            };
            nodeInput.m_netService.m_itemLayers = (ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels);
            nodeInput.m_netService.m_service = ItemClass.Service.Road;

            if (PathManagerExtendedTool.RayCast(nodeInput, out RaycastOutput nodeOutput))
            {
                HoveredNodeId = nodeOutput.m_netNode;
                HoveredSegmentId = 0;
                return;
            }

            HoveredNodeId = 0;

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
                return;
            }

            HoveredSegmentId = 0;
        }

        public override void OnKeyUp(Event e)
        {
            base.OnKeyUp(e);
            switch (e.keyCode)
            {
                case KeyCode.Return:
                    break;
                default:
                    break;
            }
        }

        public override void OnMouseUp(Event e) => OnPrimaryMouseClicked(e);
        public override void OnPrimaryMouseClicked(Event e)
        {
            if (HoverValid)
            {
                if (HoveredNodeId != 0)
                {
                    if (PathManagerExtendedTool.ShiftIsPressed)
                    {
                        if (SelectedNodeIDs.Contains(HoveredNodeId))
                            SelectedNodeIDs.Remove(HoveredNodeId);
                        else
                            SelectedNodeIDs.Add(HoveredNodeId);
                    }
                    else
                    {
                        //Tool.SetNode(HoveredNodeId);
                        //Tool.SetMode(ToolType.EditNote);
                        return;
                    }
                }
                if (HoveredSegmentId != 0)
                {
                    if (PathManagerExtendedTool.ShiftIsPressed)
                    {
                        if (SelectedSegmentIDs.Contains(HoveredSegmentId))
                            SelectedSegmentIDs.Remove(HoveredSegmentId);
                        else
                            SelectedSegmentIDs.Add(HoveredSegmentId);
                    } else
                    {
                        Tool.SetSegment(HoveredSegmentId);
                        Tool.SetMode(ToolType.SelectLane);
                        return;
                    }
                }
            }
        }
        public override void OnSecondaryMouseClicked()
        {
            Log.Debug("SelectInstanceTool.OnSecondaryMouseClicked()");
            if (SelectedNodeIDs.Count > 0 || SelectedSegmentIDs.Count > 0)
            {
                SelectedNodeIDs.Clear();
                SelectedSegmentIDs.Clear();
            }
            else
            {
                Tool.DisableTool();
            }
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo)
        {
            for (int i = 0; i < SelectedSegmentIDs.Count; i++)
            {
                if (HoveredSegmentId == 0 || HoveredSegmentId != SelectedSegmentIDs[i])
                    RenderUtil.RenderRawSegmentOverlay(cameraInfo, SelectedSegmentIDs[i], (SelectedSegmentIDs[i].ToSegment().Info.m_halfWidth - 2f), Colors.GameGreen, 0f, 0f, true);
            }

            for (int i = 0; i < SelectedNodeIDs.Count; i++)
            {
                if (HoveredNodeId == 0 || HoveredNodeId != SelectedNodeIDs[i])
                    RenderUtil.DrawNodeCircle(cameraInfo, Colors.GameGreen, SelectedNodeIDs[i], true);
            }

            if (HoverValid)
            {
                if (HoveredSegmentId != 0)
                {
                    RenderUtil.RenderRawSegmentOverlay(
                        cameraInfo,
                        HoveredSegmentId,
                        (HoveredSegmentId.ToSegment().Info.m_halfWidth - 2f),
                        SelectedSegmentIDs.Contains(HoveredSegmentId) ? (PathManagerExtendedTool.ShiftIsPressed ? Colors.OrangeWeb : Colors.Add(Colors.GameGreen, 0.15f)) : Colors.GameBlue,
                        0f,
                        0f,
                        true
                    );
                }

                if (HoveredNodeId != 0)
                {
                    RenderUtil.DrawNodeCircle(
                        cameraInfo,
                        SelectedNodeIDs.Contains(HoveredNodeId) ? (PathManagerExtendedTool.ShiftIsPressed ? Colors.OrangeWeb : Colors.Add(Colors.GameGreen, 0.15f)) : Colors.GameBlue,
                        HoveredNodeId,
                        true
                    );
                }
            }
        }
    }
}
