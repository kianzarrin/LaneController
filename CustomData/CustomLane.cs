namespace PathController.CustomData {
    using ColossalFramework.Math;
    using KianCommons;
    using PathController.Util;

    using UnityEngine;

    public interface ICustomPath {
        bool IsDefault();
        void Reset();
    }

    public class CustomLane: ICustomPath {
        public CustomLane(LaneIdAndIndex laneIdAndIndex) {
            LaneIdAndIndex = laneIdAndIndex;
        }

        #region Data
        public readonly LaneIdAndIndex LaneIdAndIndex;
        public float Shift, VShift;
        public float DeltaStart, DeltaEnd;
        public Bezier3 DeltaPoints;

        public float Position {
            get => LaneInfo.m_position + Shift;
            set => Shift = value - LaneInfo.m_position;
        }

        public float Height {
            get => LaneInfo.m_verticalOffset + VShift;
            set => VShift = value - LaneInfo.m_verticalOffset;
        }
        #endregion

        #region shortcuts
        public NetInfo.Lane LaneInfo => LaneIdAndIndex.LaneInfo;
        public int Index => LaneIdAndIndex.LaneIndex;
        public uint LaneID => LaneIdAndIndex.LaneID;
        #endregion

        public bool IsDefault() {
            return
                Shift == default &&
                VShift == default &&
                DeltaStart == default &&
                DeltaEnd == default &&
                DeltaPoints.IsDefault();
        }

        public void Reset() {
            Shift = default;
            VShift = default;
            DeltaStart = default;
            DeltaEnd = default;
            DeltaPoints = default;
        }

        public void QueueUpdate() {
            NetManager.instance.UpdateSegment(LaneIdAndIndex.SegmentID);
        }

        public void RecalculateLaneBezier() {
            Log.Called(LaneIdAndIndex);
            ref NetLane lane = ref LaneIdAndIndex.Lane;
            ushort segmentID = lane.m_segment;
            ref NetSegment segment = ref segmentID.ToSegment();
            NetInfo.Lane laneInfo = LaneInfo;

            segment.CalculateCorner(segmentID, true, true, true, out Vector3 cornerStartLeft, out Vector3 dirStartLeft, out bool smoothStart);
            segment.CalculateCorner(segmentID, true, false, true, out Vector3 cornerEndLeft, out Vector3 dirEndLeft, out bool smoothEnd);
            segment.CalculateCorner(segmentID, true, true, false, out Vector3 cornerStartRight, out Vector3 dirStartRight, out smoothStart);
            segment.CalculateCorner(segmentID, true, false, false, out Vector3 cornerEndRight, out Vector3 dirEndRight, out smoothEnd);

            float normalizedPos = Position / (segment.Info.m_halfWidth * 2f) + 0.5f;
            if ((segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None) {
                normalizedPos = 1f - normalizedPos;
            }

            Vector3 a = cornerStartLeft + (cornerStartRight - cornerStartLeft) * normalizedPos;
            Vector3 d = cornerEndRight + (cornerEndLeft - cornerEndRight) * normalizedPos;
            Vector3 dira = Vector3.Lerp(dirStartLeft, dirStartRight, normalizedPos);
            Vector3 dird = Vector3.Lerp(dirEndRight, dirEndLeft, normalizedPos);
            a.y += Height;
            d.y += Height;
            NetSegment.CalculateMiddlePoints(a, dira, d, dird, smoothStart, smoothEnd, out Vector3 b, out Vector3 c);

            lane.m_bezier = new Bezier3(a, b, c, d);
            lane.m_bezier = BezierUtil.MathLine(segment.m_startDirection, segment.m_endDirection, lane.m_bezier, normalizedPos);
            lane.m_segment = segmentID;

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

        public void PostfixLaneBezier() {
            Log.Called(LaneIdAndIndex);
            ref NetLane lane = ref LaneIdAndIndex.Lane;
            ushort segmentID = lane.m_segment;
            ref NetSegment segment = ref segmentID.ToSegment();

            bool smootha = segment.m_startNode.ToNode().m_flags.IsFlagSet(NetNode.Flags.Middle);
            bool smoothd = segment.m_endNode.ToNode().m_flags.IsFlagSet(NetNode.Flags.Middle);
            Bezier3 bezier = lane.m_bezier;

            bezier = bezier.Shift(Shift, VShift, smootha, smoothd);

            lane.m_bezier = bezier;
            lane.UpdateLength();
        }

        public override string ToString() => $"Lane {Index}";
    }
}
