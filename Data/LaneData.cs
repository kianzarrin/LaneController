namespace PathController.Data {
    using KianCommons;

    public class LaneData {
        public LaneData() { }
        public LaneData(LaneIdAndIndex laneIdAndIndex) {
            LaneIdAndIndex = laneIdAndIndex;
            DeltaPos = 0;
        }
        public LaneIdAndIndex LaneIdAndIndex;
        public float DeltaPos;
        public float Position {
            get => LaneInfo.m_position + DeltaPos;
            set => LaneInfo.m_position = value - DeltaPos;
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
