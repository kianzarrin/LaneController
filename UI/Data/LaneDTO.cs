namespace PathController.UI.Data {
    using KianCommons;
    using PathController.CustomData;
    using UnityEngine;

    public class LaneDTO : CustomLane {
        public LaneDTO() : base(default) { }
        public LaneDTO(LaneIdAndIndex laneIdAndIndex) : base(laneIdAndIndex) { }

        public bool IsEmpty => LaneID == 0 || SegmentID == 0;
        public ushort SegmentID => LaneIdAndIndex.SegmentID;
        public int Index => LaneIdAndIndex.LaneIndex;
        public uint LaneID => LaneIdAndIndex.LaneID;

        public override string ToString() => $"Lane {Index}";
    }
}
