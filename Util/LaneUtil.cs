namespace PathController.Util {
    using ColossalFramework.Math;
    using KianCommons;
    using PathController.UI.Data;
    using UnityEngine;

    public static class LaneUtil
    {
        public static void UpdateLaneBezier(LaneData laneData)
        {
            Log.Called(laneData.LaneIdAndIndex);
            ref NetLane lane = ref laneData.LaneIdAndIndex.Lane;
            ushort segmentID = lane.m_segment;
            ref NetSegment segment = ref segmentID.ToSegment();
            NetInfo.Lane laneInfo = laneData.LaneInfo;

            segment.CalculateCorner(segmentID, true, true, true, out Vector3 cornerStartLeft, out Vector3 dirStartLeft, out bool smoothStart);
            segment.CalculateCorner(segmentID, true, false, true, out Vector3 cornerEndLeft, out Vector3 dirEndLeft, out bool smoothEnd);
            segment.CalculateCorner(segmentID, true, true, false, out Vector3 cornerStartRight, out Vector3 dirStartRight, out smoothStart);
            segment.CalculateCorner(segmentID, true, false, false, out Vector3 cornerEndRight, out Vector3 dirEndRight, out smoothEnd);

            if (segment.IsInvert())
            {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerStartRight.z - cornerStartLeft.z, cornerStartRight.x - cornerStartLeft.x) * 40.7436638f) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerEndLeft.z - cornerEndRight.z, cornerEndLeft.x - cornerEndRight.x) * 40.7436638f) & 255);
            }
            else
            {
                segment.m_cornerAngleStart = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerStartLeft.z - cornerStartRight.z, cornerStartLeft.x - cornerStartRight.x) * 40.7436638f) & 255);
                segment.m_cornerAngleEnd = (byte)(Mathf.RoundToInt(Mathf.Atan2(cornerEndRight.z - cornerEndLeft.z, cornerEndRight.x - cornerEndLeft.x) * 40.7436638f) & 255);
            }

            float normalizedPos = laneData.Position / (segment.Info.m_halfWidth * 2f) + 0.5f;
            if ((segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None)
            {
                normalizedPos = 1f - normalizedPos;
            }
            
            Vector3 a = cornerStartLeft + (cornerStartRight - cornerStartLeft) * normalizedPos;
            Vector3 d = cornerEndRight + (cornerEndLeft - cornerEndRight) * normalizedPos;
            Vector3 dira = Vector3.Lerp(dirStartLeft, dirStartRight, normalizedPos);
            Vector3 dird = Vector3.Lerp(dirEndRight, dirEndLeft, normalizedPos);
            a.y += laneData.Height;
            d.y += laneData.Height;
            NetSegment.CalculateMiddlePoints(a, dira, d, dird, smoothStart, smoothEnd, out Vector3 b, out Vector3 c);

            lane.m_bezier = new Bezier3(a, b, c, d);
            lane.m_bezier = BezierUtil.MathLine(segment.m_startDirection, segment.m_endDirection, lane.m_bezier, normalizedPos);
            lane.m_segment = segmentID;
            lane.m_firstTarget = 0;
            lane.m_lastTarget = byte.MaxValue;

            lane.UpdateLength();
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
}
