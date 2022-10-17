namespace PathController.UI.Data {
    using KianCommons;
    using PathController.CustomData;
    using PathController.Manager;
    using System.Collections.Generic;
    using System.Linq;
    public struct SegmentDTO {
        public ushort SegmentID;
        public CustomData.CustomLane[] Lanes;
        
        public bool IsEmpty => SegmentID == 0;
        public ref NetSegment Segment => ref SegmentID.ToSegment();

        public SegmentDTO(ushort segmentID) {
            SegmentID = segmentID;
            if (segmentID == 0) {
                Lanes = new CustomData.CustomLane[0];
            } else {
                Lanes = new LaneIterator(segmentID)
                    .Select(CustomManager.instance.GetOrCreateLane)
                    .ToArray().LogRet("lanes");
            }
        }

        public void Empty() {
            SegmentID = 0;
            Lanes = new CustomData.CustomLane[0];
        }
    }
}
