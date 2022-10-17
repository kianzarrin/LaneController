namespace PathController.UI.Data {
    using KianCommons;
    using System.Linq;

    public struct SegmentDTO {
        public ushort SegmentID;
        public LaneDTO[] Lanes;

        public bool IsEmpty => SegmentID == 0;
        public ref NetSegment Segment => ref SegmentID.ToSegment();

        public SegmentDTO(ushort segmentID) {
            SegmentID = segmentID;
            if (segmentID == 0) {
                Lanes = new LaneDTO[0];
            } else {
                Lanes = new LaneIterator(segmentID)
                    .Select(item => new LaneDTO(item))
                    .ToArray().LogRet("lanes");
            }
        }

        public void Empty() {
            SegmentID = 0;
            Lanes = new LaneDTO[0];
        }
    }
}
