namespace PathController.Manager;

using ColossalFramework;
using KianCommons;
using PathController.CustomData;
using System;
using System.Collections.Generic;
using System.Linq;
using KianCommons.Serialization;
using System.Xml.Serialization;

public class PathControllerManager {
    #region lifecycle
    public static PathControllerManager Instance { get; private set; }
    public static bool Exists() => Instance != null;
    public static PathControllerManager Create() => Instance = new PathControllerManager();
    public static PathControllerManager Ensure() => Instance ??= Create();
    public static PathControllerManager Release() => Instance = null;
    #endregion

    #region serialization
    internal static Version LoadingVersion;
    [XmlAttribute("Version")]
    public string SavedVersion {
        get => this.VersionOf().ToString();
        set => LoadingVersion = new Version(value);
    }

    public List<CustomLane> SavedLanes {
        get {
            return Lanes.Values.
                Where(customLane => !customLane.IsDefault()).
                ToList();
                
        }
        set {
            Lanes.Clear();
            foreach(var customLane in value)
                Lanes[customLane.LaneId] = customLane;
        }
    }

    public byte[] Serialize() {
        if(Lanes.Values.Any(customLane => !customLane.IsDefault())) {
            var xmlData = XMLSerializerUtil.Serialize(this);
            return Convert.FromBase64String(xmlData);
        } else {
            return null;
        }

    }

    public static PathControllerManager Deserialize(byte[] data) {
        if (data == null)
            return Create();

        string xmlData = Convert.ToBase64String(data);
        return Instance = XMLSerializerUtil.Deserialize<PathControllerManager>(xmlData);
    }

    #endregion

    internal Dictionary<uint, CustomLane> Lanes = new(1000);

    public CustomLane GetLane(uint laneId) => Lanes.GetorDefault(laneId);

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

    public void Trim(uint laneId) {
        if (Lanes.TryGetValue(laneId, out var lane)
            && lane.IsDefault()) {
            Lanes.Remove(laneId);
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
