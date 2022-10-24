namespace LaneConroller.CustomData;
using ColossalFramework.Math;
using KianCommons;
using KianCommons.Serialization;
using LaneConroller.Util;
using System;
using System.Security.AccessControl;
using System.Xml.Serialization;
using UnityEngine;

public interface ICustomPath {
    bool IsDefault();
    void Reset();
}

public class CustomLane : ICustomPath {
    public CustomLane(LaneIdAndIndex laneIdAndIndex) {
        LaneIdAndIndex = laneIdAndIndex;
        Beizer0 = FinalBezier;
    }

    #region Data
    public LaneIdAndIndex LaneIdAndIndex;

    public float Shift, VShift;
    [XmlElement("Displacement",typeof(Bezier3XML))]
    public Bezier3 DeltaControlPoints;
    [XmlElement("Bezier0", typeof(Bezier3XML))]
    private Bezier3 Beizer0;

    public float Position {
        get => LaneInfo.m_position + Shift;
        set => Shift = value - LaneInfo.m_position;
    }

    public float Height {
        get => LaneInfo.m_verticalOffset + VShift;
        set => VShift = value - LaneInfo.m_verticalOffset;
    }

    public void UpdateControlPoint(int i, Vector3 newPos) {
        DeltaControlPoints.ControlPoint(i) = newPos - Beizer0.ControlPoint(i);
        FinalBezier.ControlPoint(i) = newPos;
        QueueUpdate();
    }
    #endregion

    #region shortcuts
    public NetInfo.Lane LaneInfo => LaneIdAndIndex.LaneInfo;
    public int Index => LaneIdAndIndex.LaneIndex;
    public uint LaneId => LaneIdAndIndex.LaneId;
    public Vector3 GetControlPoint(int i) => FinalBezier.ControlPoint(i);
    public ref Bezier3 FinalBezier => ref LaneIdAndIndex.Lane.m_bezier; 
    #endregion

    public void CopyFrom(CustomLane lane) {
        Shift = lane.Shift;
        VShift = lane.VShift;
        DeltaControlPoints = lane.DeltaControlPoints;
    }

    public CustomLane Clone() => MemberwiseClone() as CustomLane;

    public bool IsDefault() {
        return
            Shift == default &&
            VShift == default &&
            DeltaControlPoints.IsDefault();
    }

    public void Reset() {
        Shift = default;
        VShift = default;
        DeltaControlPoints = default;
    }

    public void QueueUpdate() => NetManager.instance.UpdateSegment(LaneIdAndIndex.SegmentId);

    public void RecalculateLaneBezier() {
        Log.Called(LaneIdAndIndex);
        ref NetLane lane = ref LaneIdAndIndex.Lane;
        ushort segmentId = lane.m_segment;
        ref NetSegment segment = ref segmentId.ToSegment();
        NetInfo.Lane laneInfo = LaneInfo;

        segment.CalculateCorner(segmentId, true, true, true, out Vector3 cornerStartLeft, out Vector3 dirStartLeft, out bool smoothStart);
        segment.CalculateCorner(segmentId, true, false, true, out Vector3 cornerEndLeft, out Vector3 dirEndLeft, out bool smoothEnd);
        segment.CalculateCorner(segmentId, true, true, false, out Vector3 cornerStartRight, out Vector3 dirStartRight, out smoothStart);
        segment.CalculateCorner(segmentId, true, false, false, out Vector3 cornerEndRight, out Vector3 dirEndRight, out smoothEnd);

        float normalizedPos = Position / (segment.Info.m_halfWidth * 2f) + 0.5f;
        if ((segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None) {
            normalizedPos = 1f - normalizedPos;
        }

        Vector3 a = cornerStartLeft + (cornerStartRight - cornerStartLeft) * normalizedPos;
        Vector3 d = cornerEndRight + (cornerEndLeft - cornerEndRight) * normalizedPos;
        Vector3 dira = Vector3.Lerp(dirStartLeft, dirStartRight, normalizedPos);
        Vector3 dird = Vector3.Lerp(dirEndRight, dirEndLeft, normalizedPos);
        a.y += Height;
        d.y += Height;
        NetSegment.CalculateMiddlePoints(a, dira, d, dird, smoothStart, smoothEnd, out Vector3 b, out Vector3 c);

        Beizer0 = new Bezier3(a, b, c, d);
        lane.m_bezier = Beizer0.Add(DeltaControlPoints);
        lane.m_segment = segmentId;

        lane.UpdateLength();
        float lanesTotalLength = 0;
        if (segment.Info.m_lanes.Length != 0)
            foreach (var lane2 in new LaneIterator(segmentId))
                lanesTotalLength += lane2.Lane.m_length;

        if (segment.Info.m_lanes.Length != 0) {
            segment.m_averageLength = lanesTotalLength / segment.Info.m_lanes.Length;
        } else {
            segment.m_averageLength = 0f;
        }
    }

    public void CalculateBeizer0() {
        ref NetLane lane = ref LaneIdAndIndex.Lane;
        ushort segmentId = lane.m_segment;
        ref NetSegment segment = ref segmentId.ToSegment();

        segment.CalculateCorner(segmentId, true, true, true, out Vector3 cornerStartLeft, out Vector3 dirStartLeft, out bool smoothStart);
        segment.CalculateCorner(segmentId, true, false, true, out Vector3 cornerEndLeft, out Vector3 dirEndLeft, out bool smoothEnd);
        segment.CalculateCorner(segmentId, true, true, false, out Vector3 cornerStartRight, out Vector3 dirStartRight, out smoothStart);
        segment.CalculateCorner(segmentId, true, false, false, out Vector3 cornerEndRight, out Vector3 dirEndRight, out smoothEnd);

        float normalizedPos = Position / (segment.Info.m_halfWidth * 2f) + 0.5f;
        if ((segment.m_flags & NetSegment.Flags.Invert) != NetSegment.Flags.None) {
            normalizedPos = 1f - normalizedPos;
        }

        Vector3 a = cornerStartLeft + (cornerStartRight - cornerStartLeft) * normalizedPos;
        Vector3 d = cornerEndRight + (cornerEndLeft - cornerEndRight) * normalizedPos;
        Vector3 dira = Vector3.Lerp(dirStartLeft, dirStartRight, normalizedPos);
        Vector3 dird = Vector3.Lerp(dirEndRight, dirEndLeft, normalizedPos);
        a.y += Height;
        d.y += Height;
        NetSegment.CalculateMiddlePoints(a, dira, d, dird, smoothStart, smoothEnd, out Vector3 b, out Vector3 c);

        Beizer0 = new Bezier3(a, b, c, d);
    }

    public void PostfixLaneBezier() {
        try {
            ref NetLane lane = ref LaneIdAndIndex.Lane;
            Beizer0 = lane.m_bezier.Shift(Shift, VShift);
            lane.m_bezier = Beizer0.Add(DeltaControlPoints);
            lane.UpdateLength();
        } catch(Exception ex) { ex.Log(); }
    }

    public override string ToString() => $"Lane {Index}";
}

