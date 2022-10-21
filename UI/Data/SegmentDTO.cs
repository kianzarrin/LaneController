namespace PathController.UI.Data {
    using KianCommons;
    using PathController.CustomData;
    using PathController.Manager;
    using System.Linq;
    public class SegmentDTO {
        public ushort SegmentID;
        public CustomLane[] Lanes;
        
        public ref NetSegment Segment => ref SegmentID.ToSegment();

        public SegmentDTO(ushort segmentID) {
            SegmentID = segmentID;
            if (segmentID == 0) {
                Lanes = new CustomLane[0];
            } else {
                Lanes = new LaneIterator(segmentID)
                    .Select(PathControllerManager.Instance.GetOrCreateLane)
                    .ToArray()/*.LogRet("lanes")*/;
            }
        }
    }
}
