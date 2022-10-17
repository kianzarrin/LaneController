namespace PathController.UI.Data {
    using KianCommons;
    using System.Linq;

    public struct SegmentData {
        public ushort SegmentID;
        public LaneData[] Lanes;

        public bool IsEmpty => SegmentID == 0;
        public ref NetSegment Segment => ref SegmentID.ToSegment();

        public SegmentData(ushort segmentID) {
            SegmentID = segmentID;
            if (segmentID == 0) {
                Lanes = new LaneData[0];
            } else {
                Lanes = new LaneIterator(segmentID)
                    .Select(item => new LaneData(item))
                    .ToArray().LogRet("lanes");
            }
        }

        public void Empty() {
            SegmentID = 0;
            Lanes = new LaneData[0];
        }
    }
}
