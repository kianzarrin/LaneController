namespace PathController.Data {
    using KianCommons;
    using UnityEngine;

    public class LaneData {
        public LaneData() { }
        public LaneData(LaneIdAndIndex laneIdAndIndex) {
            LaneIdAndIndex = laneIdAndIndex;
            Shift = 0;
        }
        public LaneIdAndIndex LaneIdAndIndex;
        public float Shift, VShift;
        public float DeltaStart, DeltaEnd;
        public Vector3 A, B, C, D;

        public float Position {
            get => LaneInfo.m_position + Shift;
            set => LaneInfo.m_position = value - Shift;
        }

        public float Height {
            get => LaneInfo.m_verticalOffset + VShift;
            set => LaneInfo.m_verticalOffset = value - VShift;
        }

        public bool IsEmpty => LaneID == 0 || SegmentID == 0;
        public ushort SegmentID => LaneIdAndIndex.SegmentID;
        public int Index => LaneIdAndIndex.LaneIndex;
        public uint LaneID => LaneIdAndIndex.LaneID;
        public float Width => LaneIdAndIndex.LaneInfo.m_width;
        public NetInfo.Lane LaneInfo => LaneIdAndIndex.LaneInfo;
        public ref NetLane Lane => ref LaneIdAndIndex.Lane;


        public override string ToString() => $"Lane {Index}";
    }
}
