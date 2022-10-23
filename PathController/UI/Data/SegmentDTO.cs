namespace PathController.UI.Data {
    using KianCommons;
    using PathController.CustomData;
    using PathController.Manager;
    using System.Linq;
    public class SegmentDTO {
        public ushort SegmentId;
        public CustomLane[] Lanes;
        public SegmentDTO(ushort segmentId) {
            SegmentId = segmentId;
            Lanes = PathControllerManager.Instance.GetOrCreateLanes(segmentId);
        }
    }
}
