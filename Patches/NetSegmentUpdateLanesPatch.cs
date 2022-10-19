namespace PathController.Patches;
using HarmonyLib;
using PathController.Manager;

[HarmonyPatch(typeof(NetSegment), nameof(NetSegment.UpdateLanes))]
public static class NetSegmentUpdateLanesPatch {
    public static void Postfix(ushort segmentID) {
        PathControllerManager.Instance?.UpateLanes(segmentID);
    }
}

