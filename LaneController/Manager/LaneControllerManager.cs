namespace LaneConroller.Manager;

using ColossalFramework;
using KianCommons;
using LaneConroller.CustomData;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using KianCommons.Serialization;
using System.Xml.Serialization;

public class LaneConrollerManager {
    #region lifecycle
    public static LaneConrollerManager Instance { get; private set; }
    public static bool Exists() => Instance != null;
    public static LaneConrollerManager Create() => Instance = new LaneConrollerManager();
    public static LaneConrollerManager Ensure() => Instance ??= Create();
    public static LaneConrollerManager Release() => Instance = null;
    #endregion

    #region serialization
    internal static Version LoadingVersion;
    [XmlAttribute("Version")]
    public string SavedVersion {
        get => this.VersionOf().ToString();
        set => LoadingVersion = new Version(value);
    }

    [XmlArray("Lanes"), XmlArrayItem("Lane")]
    public CustomLane[] SavedLanes {
        get {
            return Lanes.Values.
                Where(customLane => !customLane.IsDefault()).
                ToArray().LogRet();
        }
        set {
            Log.Called(value);
            Lanes.Clear();
            foreach(var customLane in value)
                Lanes[customLane.LaneId] = customLane;
        }
    }

    public byte[] Serialize() {
        Log.Called();
        if (SavedLanes.Any()) {
            var xmlData = XMLSerializerUtil.Serialize(this);
            Log.Debug(xmlData);
            return Encoding.ASCII.GetBytes(xmlData);
        } else {
            return null;
        }
    }

    public static LaneConrollerManager Deserialize(byte[] data) {
        try {
            Log.Called();
            if (data == null)
                return Create();
            string xmlData = Encoding.ASCII.GetString(data);
            Log.Debug(xmlData);
            Instance = XMLSerializerUtil.Deserialize<LaneConrollerManager>(xmlData);
            Instance.UpdateAllLanes();
            return Instance;
        }catch(Exception ex) {
            ex.Log();
            return Create();
        }
    }

    public void UpdateAllLanes() {
        var segmentIds = Lanes.Values.
            Select(lane => lane.LaneIdAndIndex.SegmentId)
            .Distinct();
        Log.Called("segments:"+segmentIds);
        foreach (ushort segmentId in segmentIds) {
            segmentId.ToSegment().UpdateLanes(segmentId, true);
        }
    }

    #endregion

    [XmlIgnore]
    internal Dictionary<uint, CustomLane> Lanes = new(1000);

    public CustomLane GetLane(uint laneId) => Lanes.GetorDefault(laneId);

    /// <summary>
    /// only return custom lanes if they exists, otherwise returns empty array.
    /// </summary>
    public CustomLane[] GetLanes(ushort segmentId) {
        if (segmentId == 0) return new CustomLane[0];
        return new LaneIterator(segmentId).
            Select(laneIdAndIndex => GetLane(laneIdAndIndex.LaneId)).
            Where(lane => lane != null).
            ToArray();
    }


    public CustomLane[] GetOrCreateLanes(ushort segmentId) {
        if(segmentId == 0) return new CustomLane[0];
        return new LaneIterator(segmentId).
            Select(laneIdAndIndex => GetOrCreateLane(laneIdAndIndex)).
            ToArray();
    }

    public CustomLane GetOrCreateLane(LaneIdAndIndex laneIdAndIndex) {
        uint laneId = laneIdAndIndex.LaneId;
        if (Lanes.TryGetValue(laneId, out var ret)) {
            return ret;
        } else {
            return Lanes[laneId] = new CustomLane(laneIdAndIndex);
        }
    }

    public void TrimLane(uint laneId) {
        if (Lanes.TryGetValue(laneId, out var lane)
            && lane.IsDefault()) {
            Lanes.Remove(laneId);
        }
    }

    public void TrimSegment(ushort segmentId) {
        foreach(var laneIdAndIndex in new LaneIterator(segmentId)) {
            TrimLane(laneIdAndIndex.LaneId);
        }
    }


    public void UpateLanes(ushort segmentId) {
        float len = 0;
        int count = 0;
        foreach(var laneIdAndIndex in new LaneIterator(segmentId)) {
            GetLane(laneIdAndIndex.LaneId)?.PostfixLaneBezier();
            len += laneIdAndIndex.Lane.m_length;
            count++;
        }

        if(count > 0) 
            segmentId.ToSegment().m_averageLength = len / count;
    }
}
