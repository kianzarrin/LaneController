namespace LaneConroller.Util;

using KianCommons;
using System.Collections.Generic;
using System.Linq;

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
        ret.AddRange(TraverseSegmentsUntilJunction(segmentId, segmentId.ToSegment().m_startNode));
        ret.AddRange(TraverseSegmentsUntilJunction(segmentId, segmentId.ToSegment().m_endNode));
        return ret;
    }

    private static HashSet<ushort> traveresed_ = new();
    static IEnumerable<ushort> TraverseSegmentsUntilJunction(ushort startSegmentId, ushort nodeId) {
        traveresed_.Clear();
        NetInfo startInfo = startSegmentId.ToSegment().Info;
        ushort nextSegmentId = startSegmentId;
        int watchdog = 0;
        while (nodeId != 0 && nodeId.ToNode().m_flags.IsFlagSet(NetNode.Flags.Middle | NetNode.Flags.Bend)) {
            nextSegmentId = nodeId.ToNode().GetAnotherSegment(nextSegmentId);

            if (nextSegmentId.ToSegment().Info != startInfo)
                break;
            if (nextSegmentId == startSegmentId)
                break; // circled around
            if (traveresed_.Contains(nextSegmentId)) {
                Log.Error("unexpected Loop detected. send screenshot of networks to kian.");
                break;
            }
            if (watchdog++ > 10000) {
                Log.Error("watchdog limit exceeded. send screenshot of networks to kian.");
                break;
            }

            traveresed_.Add(nextSegmentId);
            nodeId = nextSegmentId.ToSegment().GetOtherNode(nodeId);
        }
        return traveresed_;
    }

    /// <summary>
    /// returns a list of segments
    /// that have same NetInfo
    /// and the same name
    /// starting with the given segmentId
    /// </summary>
    public static IEnumerable<ushort> GetSimilarSegmentsInRoad(ushort segmentId) {
        var ret = new List<ushort>();
        ret.Add(segmentId);
        ret.AddRange(TraverseRoad(segmentId, segmentId.ToSegment().m_startNode));
        ret.AddRange(TraverseRoad(segmentId, segmentId.ToSegment().m_endNode));
        return ret;
    }

    static IEnumerable<ushort> TraverseRoad(ushort startSegmentId, ushort nodeId) {
        traveresed_.Clear();
        NetInfo startInfo = startSegmentId.ToSegment().Info;
        ushort nextSegmentId = startSegmentId;
        var nameSeed = startSegmentId.ToSegment().m_nameSeed;
        int watchdog = 0;
        while (nodeId != 0) {
            ref NetNode node = ref nodeId.ToNode();
            if (node.m_flags.IsFlagSet(NetNode.Flags.Middle | NetNode.Flags.Bend)) {
                nextSegmentId = node.GetAnotherSegment(nextSegmentId);
                if (nextSegmentId.ToSegment().Info != startInfo)
                    break;
                if (nextSegmentId == startSegmentId || traveresed_.Contains(nextSegmentId))
                    break; // circled around
            } else if (node.m_flags.IsFlagSet(NetNode.Flags.Junction)) {
                bool found = false;
                foreach ( ushort segmentId in new NodeSegmentIterator(nodeId)) {
                    if (segmentId != nextSegmentId && // optimisation
                        segmentId.ToSegment().Info == startInfo &&
                        segmentId.ToSegment().m_nameSeed == nameSeed &&
                        !traveresed_.Contains(segmentId)) {
                        nextSegmentId = segmentId;
                        found = true;
                        break;
                    }
                }
                if (!found) break;
            } else {
                Log.Warning($"Unexpected node detected node{nodeId} flags:{node.m_flags}");
                break;
            }

            if (watchdog++ > 10000) {
                Log.Error("watchdog limit exceeded. send screenshot of networks to kian.");
                break;
            }

            traveresed_.Add(nextSegmentId);
            nodeId = nextSegmentId.ToSegment().GetOtherNode(nodeId);
        }

        return traveresed_;
    }
}
