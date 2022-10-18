namespace PathController.Manager;

using ColossalFramework;
using KianCommons;
using PathController.CustomData;
using System;
using System.Collections.Generic;
using System.Linq;
using KianCommons.Serialization;
using System.Xml.Serialization;

internal class CustomManager {
    #region lifecycle
    public static CustomManager Instance { get; private set; }
    public static bool Exists() => Instance != null;
    public static CustomManager Create() => Instance = new CustomManager();
    public static CustomManager Ensure() => Instance ??= Create();
    public static CustomManager Release() => Instance = null;
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
                Lanes[customLane.LaneID] = customLane;
        }
    }

    public byte[] Serialize() {
        var xmlData = XMLSerializerUtil.Serialize(this);
        return Convert.FromBase64String(xmlData);
    }

    public static CustomManager Deserialize(byte[] data) {
        if (data == null)
            return Create();

        string xmlData = Convert.ToBase64String(data);
        return Instance = XMLSerializerUtil.Deserialize<CustomManager>(xmlData);
    }

    #endregion

    internal Dictionary<uint, CustomLane> Lanes = new(1000);

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
