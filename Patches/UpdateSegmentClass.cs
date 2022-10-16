using HarmonyLib;
using ColossalFramework;

namespace PathController.Patches
{
    [HarmonyPatch(typeof(RoadAI), "UpdateSegmentFlags")]
    public static class RoadAIUpdateSegmentFlags
    {
        public static void Postfix(ushort segmentID)
        {
            UnityEngine.Debug.Log("UpdateSegmentFlags Postfix");
            NetManager instance = Singleton<NetManager>.instance;
            if ((instance.m_segments.m_buffer[segmentID].m_flags & NetSegment.Flags.StopRight) != 0)
            {
                UnityEngine.Debug.Log(
                    "UpdateSegmentFlags Postfix: Segment has 'StopRight' flag.\n" +
                    "segmentID = " + segmentID.ToString() + ";"
                );
                NetInfo info = instance.m_segments.m_buffer[segmentID].Info;
                int j = 0;
                int k = 0;
                int pedLaneCount = 0;
                for (int i = 0; i < info.m_lanes.Length; i++)
                {
                    float mostRight = 0;
                    float mostRightPed = 0;
                    if (((info.m_lanes[i].m_laneType & NetInfo.LaneType.Vehicle) != 0) && ((info.m_lanes[i].m_vehicleType & VehicleInfo.VehicleType.Bicycle) == 0) && (info.m_lanes[i].m_position > mostRight))
                    {
                        mostRight = info.m_lanes[i].m_position;
                        j = i;
                    }

                    if ((info.m_lanes[i].m_laneType & NetInfo.LaneType.Pedestrian) != 0)
                    {
                        pedLaneCount += (info.m_lanes[i].m_position < 0) ? 1 : -1;
                        if (info.m_lanes[i].m_position > mostRightPed)
                        {
                            mostRightPed = info.m_lanes[i].m_position;
                            k = i;
                        }
                    }
                }
                UnityEngine.Debug.Log("UpdateSegmentFlags Postfix:\n\tint j = " + j.ToString() + ";\n\tint k = " + k.ToString() + ";\n\tint pedLaneCount = " + pedLaneCount.ToString() + ";");

                if (pedLaneCount != 0)
                {
                    return;
                }

                NetInfo.Lane carLane = info.m_lanes[j];
                NetInfo.Lane pedLane = info.m_lanes[k];

                UnityEngine.Debug.Log(
                    "pedLane.m_position   = " + pedLane.m_position.ToString() + "\n" +
                    "pedLane.m_width      = " + pedLane.m_width.ToString() + "\n" +
                    "carLane.m_position   = " + carLane.m_position.ToString() + "\n" +
                    "carLane.m_stopOffset = " + carLane.m_stopOffset.ToString() + "\n" +
                    "carLane.m_width      = " + carLane.m_width.ToString()
                );

                float pedLaneLeftPoint = (pedLane.m_position - (pedLane.m_width / 2));
                float carLaneRightPoint = (carLane.m_position + carLane.m_stopOffset + (carLane.m_width / 2));
                float delta = 0;

                UnityEngine.Debug.Log("UpdateSegmentFlags Postfix:\n\tfloat pedLaneLeftPoint = " + pedLaneLeftPoint.ToString() + ";\n\tfloat carLaneRightPoint = " + carLaneRightPoint.ToString() + ";");

                if (carLaneRightPoint > pedLaneLeftPoint)
                {
                    delta = carLaneRightPoint - pedLaneLeftPoint;
                }

                UnityEngine.Debug.Log("UpdateSegmentFlags Postfix:\n\tfloat delta = " + delta.ToString() + ";");

                instance.m_segments.m_buffer[segmentID].Info.m_lanes[k].m_width = pedLane.m_width - delta;
                instance.m_segments.m_buffer[segmentID].Info.m_lanes[k].m_position = pedLane.m_position + (delta / 2);
            }
            UnityEngine.Debug.Log("UpdateSegmentFlags Postfix: Segment does not have 'StopRight' flag.");
        } // End PostFix
    }
}
