namespace LaneConroller.CustomData {
    using KianCommons;
    using LaneConroller.Manager;
    using System;
    using System.Linq;
    using JetBrains.Annotations;
    public class SegmentDTO {
        public ushort SegmentId;
        public CustomLane[] Lanes;

        [UsedImplicitly, Obsolete("XML only", error:true)]
        public SegmentDTO() { }

        public SegmentDTO(ushort segmentId) {
            SegmentId = segmentId;
            Lanes = LaneConrollerManager.Instance.GetOrCreateLanes(segmentId);
        }
        public SegmentDTO Clone() {
            var ret = new SegmentDTO(SegmentId);
            ret.Lanes = ret.Lanes.Select(lane => lane.Clone()).ToArray();
            return ret;
        }

        public void CopyTo(ushort segmentId) {
            var targetLanes = LaneConrollerManager.Instance.GetOrCreateLanes(segmentId);
            for (int laneIndex = 0; laneIndex < targetLanes.Length && laneIndex < Lanes.Length; laneIndex++) {
                targetLanes[laneIndex].CopyFrom(Lanes[laneIndex]);
            }
        }
    }
}
