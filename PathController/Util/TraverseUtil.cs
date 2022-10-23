namespace PathController.Util;

using KianCommons;
using System.Collections.Generic;

internal class TraverseUtil {
    /// <summary>
    /// returns a list of segments
    /// that have same NetInfo
    /// between two junctions
    /// starting with the given segmentId
    /// </summary>
    public static IEnumerable<ushort> GetSimilarSegmentsBetweenJunctions(ushort segmentId) {
        var ret = new List<ushort>();
        ret.Add(segmentId);
        ret.AddRange(TraverseSegments(segmentId, segmentId.ToSegment().m_startNode));
        ret.AddRange(TraverseSegments(segmentId, segmentId.ToSegment().m_endNode));
        return ret;
    }

    private static HashSet<ushort> hashset = new();
    static IEnumerable<ushort> TraverseSegments(ushort startSegmentId, ushort nodeId) {
        hashset.Clear();
        NetInfo startInfo = startSegmentId.ToSegment().Info;
        ushort nextSegmentId = startSegmentId;
        int watchdog = 0;
        while (nodeId != 0 && nodeId.ToNode().m_flags.IsFlagSet(NetNode.Flags.Middle | NetNode.Flags.Bend)) {
            nextSegmentId = nodeId.ToNode().GetAnotherSegment(nextSegmentId);

            if (nextSegmentId.ToSegment().Info != startInfo)
                break;
            if (nextSegmentId == startSegmentId)
                break; // circled around
            if (hashset.Contains(nextSegmentId)) {
                Log.Error("unexpected Loop detected. send screenshot of networks to kian.");
                break;
            }
            if (watchdog++ > 10000) {
                Log.Error("watchdog limit exceeded. send screenshot of networks to kian.");
                break;
            }

            hashset.Add(nextSegmentId);
            nodeId = nextSegmentId.ToSegment().GetOtherNode(nodeId);
        }
        return hashset;
    }
}
