namespace PathController.Manager;

using ColossalFramework;
using KianCommons;
using PathController.CustomData;
using System.Collections.Generic;

internal class CustomManager : Singleton<CustomManager> {

    public Dictionary<uint, CustomLane> Lanes = new(1000);

    public CustomLane GetLane(uint laneID) => Lanes.GetorDefault(laneID);

    //public CustomLane GetOrCreateLane(uint laneID) {
    //    if (Lanes.TryGetValue(laneID, out var ret)) {
    //        return ret;
    //    } else {
    //        int laneIndex = NetUtil.GetLaneIndex(laneID);
    //        return Lanes[laneID] = new CustomLane(new(laneID, laneIndex));
    //    }
    //}
    public CustomLane GetOrCreateLane(LaneIdAndIndex laneIdAndIndex) {
        uint laneID = laneIdAndIndex.LaneID;
        if (Lanes.TryGetValue(laneID, out var ret)) {
            return ret;
        } else {
            return Lanes[laneID] = new CustomLane(laneIdAndIndex);
        }
    }

    public void Trim(uint laneID) {
        if (Lanes.TryGetValue(laneID, out var lane)
            && lane.IsDefault()) {
            Lanes.Remove(laneID);
        }
    }

    public void UpateLanes(ushort segmentID) {
        float len = 0;
        int count = 0;
        foreach(var laneIdAndIndex in new LaneIterator(segmentID)) {
            GetLane(laneIdAndIndex.LaneID)?.PostfixLaneBezier();
            len += laneIdAndIndex.Lane.m_length;
            count++;
        }

        if(count > 0) 
            segmentID.ToSegment().m_averageLength = len / count;
    }
}
