namespace LaneConroller.Patches;
using HarmonyLib;
using LaneConroller.Manager;

[HarmonyPatch(typeof(NetSegment), nameof(NetSegment.UpdateLanes))]
public static class NetSegmentUpdateLanesPatch {
    public static void Postfix(ushort segmentID) {
        LaneConrollerManager.Instance?.UpateLanes(segmentID);
    }
}

