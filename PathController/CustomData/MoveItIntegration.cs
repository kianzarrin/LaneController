namespace PathController.CustomData; 
using System;
using System.Collections.Generic;
using MoveItIntegration;
using PathController.Manager;
using KianCommons.Serialization;
using KianCommons;
using System.Xml.Linq;

public class LCMoveItIntegrationFactory: IMoveItIntegrationFactory {
    public MoveItIntegrationBase GetInstance() => LCMoveItIntegration.Instance;
}

public class LCMoveItIntegration : MoveItIntegrationBase {
    public static LCMoveItIntegration Instance = new();
    static PathControllerManager Man => PathControllerManager.Instance;
    public override string ID { get; } = "LaneController";
    public override Version DataVersion { get; } = typeof(LCMoveItIntegration).VersionOf();

    public override object Copy(InstanceID sourceInstanceID) {
        if (sourceInstanceID.Type == InstanceType.NetSegment) {
            // clone lanes:
            return new SegmentDTO(sourceInstanceID.NetSegment).Clone();
        } else {
            return null;
        }
    }

    public override void Paste(InstanceID targetInstanceID, object record, Dictionary<InstanceID, InstanceID> map) {
        if (targetInstanceID.Type == InstanceType.NetSegment && record is SegmentDTO segmentDTO) {
            segmentDTO.CopyTo(targetInstanceID.NetSegment);
        }
    }

    public override string Encode64(object record) {
        if (record is SegmentDTO segmentDTO) {
            return XMLSerializerUtil.Serialize(segmentDTO);
        } else {
            return null;
        }
    }

    public override object Decode64(string base64Data, Version dataVersion) {
        if (new XDocument(base64Data).Root.Name == "SegmentDTO") {
            return XMLSerializerUtil.Deserialize<SegmentDTO>(base64Data);
        } else {
            throw new NotImplementedException(base64Data);
        }
    }
}

