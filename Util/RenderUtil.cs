using ColossalFramework;
using ColossalFramework.Math;
using KianCommons;
using PathController.UI;
using PathController.UI.Editors;
using System;
using UnityEngine;

namespace PathController.Util
{
    public static class RenderUtil
    {
        public static void Render(this Bezier3 bezier, RenderManager.CameraInfo cameraInfo, Color color, float halfWidth, float cutStart, float cutEnd, bool alphaBlend = false)
        {
            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            RenderManager.instance.OverlayEffect.DrawBezier(
                cameraInfo,
                color,
                bezier,
                halfWidth * 2,
                cutStart,
                cutEnd,
                -1,
                1024,
                false,
                alphaBlend
            );
        }
        public static void Render(this Bezier3 bezier, RenderManager.CameraInfo cameraInfo, Color color, float hw, bool alphaBlend = false)
        {
            bezier.Render(cameraInfo, color, hw, hw, hw, alphaBlend);
        }
        public static void RenderSegmentOverlay(RenderManager.CameraInfo cameraInfo, ushort segmentId, Color color, bool alphaBlend = false)
        {
            ref NetSegment segment = ref segmentId.ToSegment();
            RenderUncutSegmentOverlay(cameraInfo, segmentId, color, segment.Info.m_halfWidth, segment.Info.m_halfWidth, alphaBlend);
        }

        public static void RenderAutoCutSegmentOverlay(RenderManager.CameraInfo cameraInfo, ushort segmentId, Color color, bool alphaBlend = false)
        {
            ref NetSegment segment = ref segmentId.ToSegment();

            RenderUncutSegmentOverlay(
                cameraInfo, segmentId, color,
                (segment.m_startNode.ToNode().CountSegments() > 1) ? segment.Info.m_halfWidth : 0,
                (segment.m_endNode.ToNode().CountSegments() > 1)   ? segment.Info.m_halfWidth : 0,
                alphaBlend
            );
        }

        public static void RenderUncutSegmentOverlay(RenderManager.CameraInfo cameraInfo, ushort segmentId, Color color, float cutStart, float cutEnd, bool alphaBlend = false)
        {
            if (segmentId == 0) return;

            RenderRawSegmentOverlay(cameraInfo, segmentId, segmentId.ToSegment().Info.m_halfWidth, color, cutStart, cutEnd, alphaBlend);
        }

        public static void RenderRawSegmentOverlay(RenderManager.CameraInfo cameraInfo, ushort segmentId, float width, Color color, float cutStart, float cutEnd, bool alphaBlend = false)
        {
            if (segmentId == 0) return;

            ref NetSegment segment = ref segmentId.ToSegment();

            NetNode[] nodeBuffer = Singleton<NetManager>.instance.m_nodes.m_buffer;
            bool IsMiddle(ushort nodeId) => (nodeBuffer[nodeId].m_flags & NetNode.Flags.Middle) != 0;

            Bezier3 bezier;
            bezier.a = segment.m_startNode.ToNode().m_position;
            bezier.d = segment.m_endNode.ToNode().m_position;

            NetSegment.CalculateMiddlePoints(
                bezier.a,
                segment.m_startDirection,
                bezier.d,
                segment.m_endDirection,
                IsMiddle(segment.m_startNode),
                IsMiddle(segment.m_endNode),
                out bezier.b,
                out bezier.c
            );

            bezier.Render(cameraInfo, color, width, cutStart, cutEnd, alphaBlend);
        }

        //public static void RenderLaneOverlay(RenderManager.CameraInfo cameraInfo, LaneData laneData, Color color, bool alphaBlend = false)
        //{
        //    RenderLaneOverlay(cameraInfo, laneData, color, alphaBlend, false);
        //}

        public static void RenderLaneOverlay(RenderManager.CameraInfo cameraInfo, LaneIdAndIndex laneData, Color color, bool alphaBlend = false)
        {
            float hw = laneData.LaneInfo.m_width * 0.5f;
            laneData.Lane.m_bezier.Render(cameraInfo, color, hw, 0f, 0f, alphaBlend);
        }

        public static void DrawNodeCircle(RenderManager.CameraInfo cameraInfo, Color color, ushort nodeId, bool alphaBlend = false)
        {
            DrawOverlayCircle(cameraInfo, color, nodeId.ToNode().m_position, nodeId.ToNode().Info.m_halfWidth, alphaBlend);
        }


        public static void DrawOverlayCircle(RenderManager.CameraInfo cameraInfo,
                               Color color,
                               Vector3 position,
                               float radius,
                               bool alphaBlend = false)
        {
            Singleton<ToolManager>.instance.m_drawCallData.m_overlayCalls++;
            Singleton<RenderManager>.instance.OverlayEffect.DrawCircle(
                cameraInfo,
                color,
                position,
                radius * 2,
                position.y - 100f,
                position.y + 100f,
                false,
                alphaBlend);
        }
    }

    public static class Colors
    {
        public static Color Add(Color a, float b)
        {
            return new Color(a.r + b, a.g + b, a.b + b, a.a + b);
        }
        public static Color White
        {
            get
            {
                return new Color(1f, 1f, 1f, 1f);
            }
        }
        public static Color OrangeWeb
        {
            get
            {
                return new Color(1f, 0.65f, 0f, 1f);
            }
        }
        public static Color NeonBlue
        {
            get
            {
                return new Color(0.27f, 0.4f, 1f, 1f);
            }
        }
        public static Color SteelPink
        {
            get
            {
                return new Color(0.8f, 0.2f, 0.8f, 1f);
            }
        }
        public static Color YellowGreen
        {
            get
            {
                return new Color(0.6f, 0.8f, 0.2f, 1f);
            }
        }
        public static Color GameBlue
        {
            get
            {
                return new Color(0.0f, 0.71f, 1f, 1f);
            }
        }
        public static Color GameGreen
        {
            get
            {
                return new Color(0.37f, 0.65f, 0f, 1f);
            }
        }
    }
}
