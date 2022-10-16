using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using UnityEngine;

namespace PathController.Util
{
    public static class NetUtil
    {
        public static NetManager netMan => NetManager.instance;
        public static NetTool netTool => Singleton<NetTool>.instance;
        internal static ref NetNode ToNode(this ushort id) => ref netMan.m_nodes.m_buffer[id];
        internal static ref NetSegment ToSegment(this ushort id) => ref netMan.m_segments.m_buffer[id];
        internal static ref NetLane ToLane(this uint id) => ref netMan.m_lanes.m_buffer[id];

        internal static NetLane.Flags Flags(this ref NetLane lane) => (NetLane.Flags)lane.m_flags;

        internal static SegmentData GetSegmentData(ushort segmentID)
        {
            if (segmentID == 0)
            {
                return new SegmentData
                {
                    Lanes = { },
                };
            }
            var seg = new SegmentData
            {
                SegmentID = segmentID,
                StartNode = (segmentID.ToSegment().Info.m_lanes[0].m_finalDirection == NetInfo.Direction.Forward),
                Lanes = new List<LaneData> { },
            };
            foreach (var laneData in IterateSegmentLanes(seg))
            {
                seg.Lanes.Add(laneData);
            }
            return seg;
            throw new Exception("Unreachable code");
        }

        public static IEnumerable<LaneData> IterateSegmentLanes(SegmentData segData)
        {
            int idx = 0;
            if (segData.Segment.Info == null)
            {
                Log.Error("null info: potentially caused by missing assets");
                yield break;
            }
            int n = segData.Segment.Info.m_lanes.Length;
            for (uint laneID = segData.Segment.m_lanes;
                laneID != 0 && idx < n;
                laneID = laneID.ToLane().m_nextLane, idx++)
            {
                var ret = new LaneData(segData.SegmentID)
                {
                    LaneID = laneID,
                    Index = idx,
                    Position = segData.SegmentID.ToSegment().Info.m_lanes[idx].m_position,
                    Width = segData.SegmentID.ToSegment().Info.m_lanes[idx].m_width,
                };
                yield return ret;
            }
        }

        internal static float GetNodeRadius(ushort nodeID)
        {
            float r = 0;
            int n = 0;
            foreach (ushort segmentId in GetSegmentIDsAtNode(nodeID))
            {
                r += segmentId.ToSegment().Info.m_halfWidth;
                n++;
            }
            return r / n;
        }

        public static IEnumerable<ushort> GetSegmentIDsAtNode(ushort nodeID)
        {
            for (int i = 0; i < 8; ++i)
            {
                ushort segmentID = nodeID.ToNode().GetSegment(i);
                if (segmentID != 0)
                {
                    yield return segmentID;
                }
            }
        }

        public static void UpdateLanePosition(ushort segmentID, LaneData lane, ref NetSegment segmentData)
        {
            segmentData.CalculateCorner(segmentID, true, true, true, out Vector3 vector, out Vector3 a, out bool smoothStart);
            segmentData.CalculateCorner(segmentID, true, false, true, out Vector3 a2, out Vector3 b, out bool smoothEnd);
            segmentData.CalculateCorner(segmentID, true, true, false, out Vector3 a3, out Vector3 b2, out smoothStart);
            segmentData.CalculateCorner(segmentID, true, false, false, out Vector3 vector2, out Vector3 a4, out smoothEnd);

            if ((segmentData.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
            {
                segmentData.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(a3.z - vector.z, a3.x - vector.x) * 40.7436638f) & 255);
                segmentData.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(a2.z - vector2.z, a2.x - vector2.x) * 40.7436638f) & 255);
            }
            else
            {
                segmentData.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(vector.z - a3.z, vector.x - a3.x) * 40.7436638f) & 255);
                segmentData.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(vector2.z - a2.z, vector2.x - a2.x) * 40.7436638f) & 255);
            }

            float num5 = lane.Position / (segmentData.Info.m_halfWidth * 2f) + 0.5f;
            if ((segmentData.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
            {
                num5 = 1f - num5;
            }

            Vector3 vector3 = vector + (a3 - vector) * num5;
            Vector3 startDir = Vector3.Lerp(a, b2, num5);
            Vector3 vector4 = vector2 + (a2 - vector2) * num5;
            Vector3 endDir = Vector3.Lerp(a4, b, num5);
            vector3.y += lane.LaneInfo().m_verticalOffset;
            vector4.y += lane.LaneInfo().m_verticalOffset;
            NetSegment.CalculateMiddlePoints(vector3, startDir, vector4, endDir, smoothStart, smoothEnd, out Vector3 b3, out Vector3 c);

            netMan.m_lanes.m_buffer[lane.LaneID].m_bezier = new Bezier3(vector3, b3, c, vector4);
            //netMan.m_lanes.m_buffer[lane.LaneID].m_bezier = BezierUtil.MathLine(segmentData.m_startDirection, segmentData.m_endDirection, netMan.m_lanes.m_buffer[lane.LaneID].m_bezier, num5);
            netMan.m_lanes.m_buffer[lane.LaneID].m_segment = segmentID;
            netMan.m_lanes.m_buffer[lane.LaneID].m_firstTarget = 0;
            netMan.m_lanes.m_buffer[lane.LaneID].m_lastTarget = byte.MaxValue;

            uint currentLane = segmentData.m_lanes;
            float lanesTotalLength = netMan.m_lanes.m_buffer[lane.LaneID].UpdateLength();
            for (int i = 0; i < segmentData.Info.m_lanes.Length; i++)
            {
                if (i != lane.Index)
                {
                    lanesTotalLength += netMan.m_lanes.m_buffer[(int)(UIntPtr)currentLane].m_length;
                }
                currentLane = netMan.m_lanes.m_buffer[(int)(UIntPtr)currentLane].m_nextLane;
            }

            if (segmentData.Info.m_lanes.Length != 0)
            {
                segmentData.m_averageLength = lanesTotalLength / segmentData.Info.m_lanes.Length;
            }
            else
            {
                segmentData.m_averageLength = 0f;
            }
        }
    }

    public struct SegmentData
    {
        public bool IsEmpty
        {
            get
            {
                return SegmentID == 0;
            }
        }

        public void Empty()
        {
            SegmentID = 0;
            StartNode = false;
        }

        public bool HasLane(uint laneID)
        {
            for (int i = 0; i < Lanes.Count; i++)
            {
                if (Lanes[i].LaneID == laneID)
                    return true;
            }

            return false;
        }

        public bool StartNode;
        public ushort SegmentID;
        public ref NetSegment Segment => ref SegmentID.ToSegment();
        public ushort NodeID => StartNode ? Segment.m_startNode : Segment.m_endNode;

        public List<LaneData> Lanes;
    }

    public class LaneData
    {
        public bool IsEmpty
        {
            get
            {
                return LaneID == 0;
            }
        }

        public LaneData(ushort segmentID)
        {
            SegmentID = segmentID;
            Index = 0;
            LaneID = 0;
            Position = 0f;
            Width = 0f;
        }

        private readonly ushort SegmentID;

        public int Index;
        public uint LaneID;
        public float Position;
        public float Width;
        public ref NetInfo.Lane LaneInfo() {
            //if (SegmentID == 0)
            //{
            //    return;
            //}
            return ref SegmentID.ToSegment().Info.m_lanes[Index];
        }
        public Bezier3 Bezier => LaneID.ToLane().m_bezier;
        //public override string ToString() => $"Lane Data:[segment:{SegmentID} lane ID:{LaneID} {LaneInfo.m_laneType} {LaneInfo.m_vehicleType}]";
        public override string ToString() => $"Lane {Index}";
        public string ToString2()
        {
            return $"SegmentID {SegmentID}, LaneID {LaneID}, Lane {Index}";
        }
    }
}
