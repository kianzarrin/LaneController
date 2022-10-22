namespace PathController.Tool;
using System;
using ColossalFramework;
using KianCommons;
using PathController.Util;
using UnityEngine;
using static PathControllerTool;
using static ToolBase;

public class SelectSegmentTool : BaseTool {
    public override ToolType Type => ToolType.SelectSegment;
    public override bool ShowPanel => false;

    /// <summary>
    /// hover is on node and is closer to node than segment.
    /// </summary>
    protected bool PreferNodeHover { get; private set; }
    public ushort HoveredNodeId { get; private set; } = 0;
    public ushort HoveredSegmentId { get; private set; } = 0;
    public bool HoveredStartNode => HoveredSegmentId.ToSegment().IsStartNode(HoveredNodeId);
    public Vector3 HitPos { get; private set; }
    protected bool HoverValid => MouseRayValid && HoveredSegmentId != 0;

    protected override void Reset() {
        base.Reset();
        PreferNodeHover = default;
        HoveredNodeId = default;
        HoveredSegmentId = default;
    }

    static InfoManager InfoMan => Singleton<InfoManager>.instance;
    public static void SetUnderGroundView() => InfoMan.SetCurrentMode(
        InfoManager.InfoMode.Underground, InfoManager.SubInfoMode.UndergroundTunnels);
    public static void SetOverGroundView() => InfoMan.SetCurrentMode(
        InfoManager.InfoMode.None, InfoManager.SubInfoMode.None);

    protected virtual void OnPageDown() {
        Log.Info("KianToolBase.OnPageDown()");
        if (PathControllerTool.MouseRayValid)
            SetUnderGroundView();
    }

    protected virtual void OnPageUp() {
        if (PathControllerTool.MouseRayValid)
            SetOverGroundView();
    }

    public override void OnKeyUp(Event e) {
        base.OnKeyUp(e);
        switch (e.keyCode) {
            case KeyCode.PageDown:
                OnPageDown();
                break;
            case KeyCode.PageUp:
                OnPageUp();
                break;
        }
    }

    private bool HoverMatchesSelection() {
        return Tool.ActiveSegmentId != 0 &&
            Tool.ActiveSegmentId.ToSegment().Info != HoveredSegmentId.ToSegment().Info;
    }

    /// <summary>
    /// click => select new segment*
    /// control click => add segment to selection** or deselect segment
    /// shift click => select all segments between junctions.
    /// control + shift click => add all segments between junctions to selection
    /// * first selected segment is active segment
    /// ** to add segment to selection NetInfo must match.
    /// </summary>
    enum State {
        None, // invalid hover
        NOP, // Click on already selected

        SetActive, // Click
        SetActiveMulti, // shift+Click

        Select, // Ctrl+Click on not selected with same info
        Illigal, // Ctrl+Click on not selected with different info
        Deselect, // Ctrl+Click on selected
        SelectMutli, // Ctrl+Shift+Click
    }

    State CalculateState() {
        if (!HoverValid)
            return State.None;
        bool hoverSelected = Tool.IsSegmentSelected(HoveredSegmentId);
        if (!Helpers.ControlIsPressed) {
            if (ShiftIsPressed)
                return State.SetActiveMulti;
            else if (!hoverSelected)
                return State.SetActive;
            else
                return State.NOP;
        } else if (HoverMatchesSelection()) {
            // don't restart selection.
            if (Helpers.ShiftIsPressed)
                return State.SelectMutli;
            else if (!hoverSelected)
                return State.Select;
            else
                return State.Deselect;
        } else {
            return State.Illigal;
        }
    }

    public override void OnPrimaryMouseClicked(Event e) {
        State state = CalculateState();
        switch (state) {
            case State.SetActive:
                Tool.ActiveSegmentId = HoveredSegmentId;
                break;
            case State.SetActiveMulti:
                Tool.ActiveSegmentId = HoveredSegmentId;
                SelectMulti();
                break;
            case State.Select:
                Tool.SelectSegment(HoveredSegmentId);
                break;
            case State.SelectMutli:
                SelectMulti();
                break;
            case State.Deselect:
                Tool.DeselectSegment(HoveredSegmentId);
                break;
        }

        void SelectMulti() {
            foreach (ushort segmentId in TraverseUtil.GetSimilarSegmentsBetweenJunctions(HoveredSegmentId)) {
                Tool.SelectSegment(segmentId);
            }
        }
    }

    public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
        base.RenderOverlay(cameraInfo);
        State state = CalculateState();
        bool select = state is State.Select or State.SelectMutli;
        bool multi = state is State.SelectMutli or State.SetActiveMulti;

        Color? color = state switch {
            State.SetActive => Colors.GameGreen,
            State.SetActiveMulti => Colors.GameGreen,
            State.Select => Color.white,
            State.SelectMutli => Color.white,
            State.Deselect => Colors.OrangeWeb,
            State.Illigal => Color.red,
            _ => null,
        };

        if (color.HasValue) {
            RenderUtil.RenderSegmentOverlay(cameraInfo, HoveredSegmentId, color.Value);
            if (multi) {
                foreach (ushort segmentId in TraverseUtil.GetSimilarSegmentsBetweenJunctions(HoveredSegmentId)) {
                    RenderUtil.RenderSegmentOverlay(cameraInfo, segmentId, color.Value);
                }
            }
        }

        if (select) {
            foreach (ushort segmentId in Tool.SelectedSegmentIds) {
                RenderUtil.RenderSegmentOverlay(cameraInfo, segmentId, Colors.GameGreen);
            }
        }
    }

    public override void OnUpdate() {
        base.OnUpdate();
        DetermineHoveredElements();
    }

    private bool DetermineHoveredElements() {
        try {
            HoveredSegmentId = 0;
            HoveredNodeId = 0;
            HitPos = Vector3.zero;
            if (!PathControllerTool.MouseRayValid)
                return false;

            // find currently hovered node
            RaycastInput nodeInput = new RaycastInput(MouseRay, MouseRayLength) {
                m_netService = GetService(),
                m_ignoreTerrain = true,
                m_ignoreNodeFlags = NetNode.Flags.None
            };

            if (RayCast(nodeInput, out RaycastOutput nodeOutput)) {
                HoveredNodeId = nodeOutput.m_netNode;
                HitPos = nodeOutput.m_hitPos;
            }

            HoveredSegmentId = GetSegmentFromNode();

            if (HoveredSegmentId != 0) {
                Debug.Assert(HoveredNodeId != 0, "unexpected: HoveredNodeId == 0");
                return true;
            }

            // find currently hovered segment
            var segmentInput = new RaycastInput(MouseRay, MouseRayLength) {
                m_netService = GetService(),
                m_ignoreTerrain = true,
                m_ignoreSegmentFlags = NetSegment.Flags.None
            };

            if (RayCast(segmentInput, out RaycastOutput segmentOutput)) {
                HoveredSegmentId = segmentOutput.m_netSegment;
                HitPos = segmentOutput.m_hitPos;
            }

            if (HoveredNodeId <= 0 && HoveredSegmentId > 0) {
                // alternative way to get a node hit: check distance to start and end nodes
                // of the segment
                ushort startNodeId = HoveredSegmentId.ToSegment().m_startNode;
                ushort endNodeId = HoveredSegmentId.ToSegment().m_endNode;

                var vStart = segmentOutput.m_hitPos - startNodeId.ToNode().m_position;
                var vEnd = segmentOutput.m_hitPos - endNodeId.ToNode().m_position;

                float startDist = vStart.magnitude;
                float endDist = vEnd.magnitude;

                if (startDist < endDist && startDist < 75f) {
                    HoveredNodeId = startNodeId;
                } else if (endDist < startDist && endDist < 75f) {
                    HoveredNodeId = endNodeId;
                }
            }

            PreferNodeHover = nodeOutput.m_netNode != 0;
            if (PreferNodeHover) {
                Vector3 nodePos = HoveredNodeId.ToNode().m_position;
                Vector3 segmentPos = HoveredSegmentId.ToSegment().m_middlePosition;
                PreferNodeHover = (HitPos - nodePos).sqrMagnitude < (HitPos - segmentPos).sqrMagnitude;
            }

            return HoveredNodeId != 0 || HoveredSegmentId != 0;
        } catch (Exception ex) {
            ex.Log(false);
            return false;
        }
    }

    static float GetAngle(Vector3 v1, Vector3 v2) {
        float ret = Vector3.Angle(v1, v2);
        if (ret > 180) ret -= 180; //future proofing
        ret = System.Math.Abs(ret);
        return ret;
    }

    ushort GetSegmentFromNode() {
        bool considerSegmentLenght = false;
        ushort minSegId = 0;
        if (HoveredNodeId != 0) {
            NetNode node = HoveredNodeId.ToNode();
            Vector3 dir0 = node.m_position - MousePosition;
            float min_angle = float.MaxValue;
            for (int i = 0; i < 8; ++i) {
                ushort segmentId = node.GetSegment(i);
                if (segmentId == 0)
                    continue;
                NetSegment segment = segmentId.ToSegment();
                Vector3 dir;
                if (segment.m_startNode == HoveredNodeId) {
                    dir = segment.m_startDirection;

                } else {
                    dir = segment.m_endDirection;
                }
                float angle = GetAngle(-dir, dir0);
                if (considerSegmentLenght)
                    angle *= segment.m_averageLength;
                if (angle < min_angle) {
                    min_angle = angle;
                    minSegId = segmentId;
                }
            }
        }
        return minSegId;
    }

    //  copy and modified from DefaultTool.GetService()
    public virtual RaycastService GetService() {
        var currentMode = Singleton<InfoManager>.instance.CurrentMode;
        var currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
        ItemClass.Availability avaliblity = Singleton<ToolManager>.instance.m_properties.m_mode;
        if ((avaliblity & ItemClass.Availability.MapAndAsset) == ItemClass.Availability.None) {
            switch (currentMode) {
                case InfoManager.InfoMode.TrafficRoutes:
                case InfoManager.InfoMode.Tours:
                    break;
                case InfoManager.InfoMode.Underground:
                    if (currentSubMode == InfoManager.SubInfoMode.Default) {
                        return new RaycastService { m_itemLayers = ItemClass.Layer.MetroTunnels };
                    }
                    // ignore water pipes:
                    return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
                default:
                    if (currentMode != InfoManager.InfoMode.Water) {
                        if (currentMode == InfoManager.InfoMode.Transport) {
                            return new RaycastService(
                                ItemClass.Service.PublicTransport,
                                ItemClass.SubService.None,
                                ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels
                                /*| ItemClass.Layer.MetroTunnels | ItemClass.Layer.BlimpPaths | ItemClass.Layer.HelicopterPaths | ItemClass.Layer.FerryPaths*/
                                );
                        }
                        if (currentMode == InfoManager.InfoMode.Traffic) {
                            break;
                        }
                        if (currentMode != InfoManager.InfoMode.Heating) {
                            return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
                        }
                    }
                    // ignore water pipes:
                    //return new RaycastService(ItemClass.Service.Water, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.WaterPipes);
                    return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
                case InfoManager.InfoMode.Fishing:
                    // ignore fishing
                    //return new RaycastService(ItemClass.Service.Fishing, ItemClass.SubService.None, ItemClass.Layer.Default | ItemClass.Layer.FishingPaths);
                    return new RaycastService { m_itemLayers = ItemClass.Layer.Default };
            }
            return new RaycastService { m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels };
        }
        if (currentMode != InfoManager.InfoMode.Underground) {
            if (currentMode != InfoManager.InfoMode.Tours) {
                if (currentMode == InfoManager.InfoMode.Transport) {
                    return new RaycastService {
                        m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels
                        /*| ItemClass.Layer.AirplanePaths | ItemClass.Layer.ShipPaths | ItemClass.Layer.Markers*/
                    };
                }
                if (currentMode != InfoManager.InfoMode.Traffic) {
                    return new RaycastService { m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.Markers };
                }
            }
            return new RaycastService {
                m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels | ItemClass.Layer.Markers
            };
        }
        return new RaycastService { m_itemLayers = ItemClass.Layer.MetroTunnels };
    }


    private const string kCursorInfoErrorColor = "<color #ff7e00>";
    private const string kCursorInfoNormalColor = "<color #87d3ff>";
    private const string kCursorInfoCloseColorTag = "</color>";


}
