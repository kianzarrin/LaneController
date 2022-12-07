namespace LaneConroller.Patches;
using HarmonyLib;
using KianCommons;
using LaneConroller.Manager;

[HarmonyPatch(typeof(NetSegment), nameof(NetSegment.UpdateLanes))]
public static class NetSegmentUpdateLanesPatch {
    public static void Postfix(ushort segmentID) {
        Assertion.InSimulationThread();
        LaneConrollerManager.Instance?.UpateLanes(segmentID);
    }
}

