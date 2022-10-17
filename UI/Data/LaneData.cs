namespace PathController.UI.Data {
    using KianCommons;
    using UnityEngine;

    public class LaneData {
        public LaneData() { }
        public LaneData(LaneIdAndIndex laneIdAndIndex) {
            LaneIdAndIndex = laneIdAndIndex;
        }
        public LaneIdAndIndex LaneIdAndIndex;
        public float Shift, VShift;
        public float DeltaStart, DeltaEnd;
        public Vector3 A, B, C, D;

        public float Position {
            get => LaneInfo.m_position + Shift;
            set => Shift = value - LaneInfo.m_position;
        }

        public float Height {
            get => LaneInfo.m_verticalOffset + VShift;
            set => VShift= value - LaneInfo.m_verticalOffset;
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
