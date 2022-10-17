using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using ColossalFramework;
using ColossalFramework.Math;
using HarmonyLib;
using KianCommons;
using UnityEngine;

namespace PathController.Util
{
    public static class NetUtil2
    {
        internal static SegmentData GetSegmentData(ushort segmentID) => new SegmentData(segmentID);
        public static void UpdateLanePosition(LaneIdAndIndex lane)
        {
            ushort segmentID = lane.SegmentID;
            ref NetSegment segment = ref lane.Segment;
            segment.CalculateCorner(segmentID, true, true, true, out Vector3 vector, out Vector3 a, out bool smoothStart);
            segment.CalculateCorner(segmentID, true, false, true, out Vector3 a2, out Vector3 b, out bool smoothEnd);
            segment.CalculateCorner(segmentID, true, true, false, out Vector3 a3, out Vector3 b2, out smoothStart);
            segment.CalculateCorner(segmentID, true, false, false, out Vector3 vector2, out Vector3 a4, out smoothEnd);

            if ((segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
            {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(a3.z - vector.z, a3.x - vector.x) * 40.7436638f) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(a2.z - vector2.z, a2.x - vector2.x) * 40.7436638f) & 255);
            }
            else
            {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(vector.z - a3.z, vector.x - a3.x) * 40.7436638f) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(vector2.z - a2.z, vector2.x - a2.x) * 40.7436638f) & 255);
            }

            float num5 = lane.LaneInfo.m_position / (segment.Info.m_halfWidth * 2f) + 0.5f;
            if ((segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
            {
                num5 = 1f - num5;
            }

            Vector3 vector3 = vector + (a3 - vector) * num5;
            Vector3 startDir = Vector3.Lerp(a, b2, num5);
            Vector3 vector4 = vector2 + (a2 - vector2) * num5;
            Vector3 endDir = Vector3.Lerp(a4, b, num5);
            vector3.y += lane.LaneInfo.m_verticalOffset;
            vector4.y += lane.LaneInfo.m_verticalOffset;
            NetSegment.CalculateMiddlePoints(vector3, startDir, vector4, endDir, smoothStart, smoothEnd, out Vector3 b3, out Vector3 c);

            lane.Lane.m_bezier = new Bezier3(vector3, b3, c, vector4);
            //lane.Lane.m_bezier = BezierUtil.MathLine(segmentData.m_startDirection, segmentData.m_endDirection, lane.Lane.m_bezier, num5);
            lane.Lane.m_segment = segmentID;
            lane.Lane.m_firstTarget = 0;
            lane.Lane.m_lastTarget = byte.MaxValue;

            lane.Lane.UpdateLength();
            float lanesTotalLength = 0;
            if (segment.Info.m_lanes.Length != 0)
                foreach (var lane2 in new LaneIterator(segmentID))
                lanesTotalLength += lane2.Lane.m_length;

            if (segment.Info.m_lanes.Length != 0) {
                segment.m_averageLength = lanesTotalLength / segment.Info.m_lanes.Length;
            } else {
                segment.m_averageLength = 0f;
            }
        }
    }

    public struct SegmentData {
        public ushort SegmentID;
        public LaneData[] Lanes;

        public bool IsEmpty => SegmentID == 0;
        public ref NetSegment Segment => ref SegmentID.ToSegment();

        public SegmentData(ushort segmentID) {
            SegmentID = segmentID;
            if (segmentID == 0) {
                Lanes = new LaneData[0];
            } else {
                Lanes = new LaneIterator(segmentID).Select(item => new LaneData(item)).ToArray();
            }
        }

        public void Empty() {
            SegmentID = 0;
            Lanes = new LaneData[0];
        }
    }

    public class LaneData {
        public LaneData() { }
        public LaneData(LaneIdAndIndex laneIdAndIndex) {
            LaneIdAndIndex = laneIdAndIndex;
            Position = LaneInfo.m_position;
        }
        public LaneIdAndIndex LaneIdAndIndex;
        public float Position;
        public bool IsEmpty => LaneID == 0 || SegmentID == 0;
        public ushort SegmentID => LaneIdAndIndex.SegmentID;
        public int Index => LaneIdAndIndex.Index;
        public uint LaneID => LaneIdAndIndex.LaneID ;
        public float Width => LaneIdAndIndex.LaneInfo.m_width;
        public NetInfo.Lane LaneInfo => LaneIdAndIndex.LaneInfo;
    }
}
