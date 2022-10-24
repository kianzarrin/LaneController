namespace LaneConroller.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using LaneConroller.Tool;

public class PanelHeader : HeaderMoveablePanel<PanelHeaderContent> {
    public bool Available { set => Content.SetAvailable(value); }

    private HeaderButtonInfo<HeaderButton> PasteButton { get; }
    private HeaderButtonInfo<HeaderButton> BeetwenIntersectionsButton { get; }
    private HeaderButtonInfo<HeaderButton> WholeStreetButton { get; }

    public PanelHeader() {
        Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, LaneConrollerTextures.Atlas, LaneConrollerTextures.CopyHeaderButton, "Copy", LaneConrollerTool.Copy));

        PasteButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, LaneConrollerTextures.Atlas, LaneConrollerTextures.PasteHeaderButton, "Paste", LaneConrollerTool.Paste);
        Content.AddButton(PasteButton);

        Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, LaneConrollerTextures.Atlas, LaneConrollerTextures.ResetHeaderButton, "Reset", LaneConrollerTool.DeleteAll));

        Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Additional, LaneConrollerTextures.Atlas, LaneConrollerTextures.ResetControlPointsHeaderButton, "Reset control points", LaneConrollerTool.ResetControlPoints));


        BeetwenIntersectionsButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Additional, LaneConrollerTextures.Atlas, LaneConrollerTextures.BeetwenIntersectionsHeaderButton, "Apply between intersections", LaneConrollerTool.ApplyBetweenIntersections);
        Content.AddButton(BeetwenIntersectionsButton);

        WholeStreetButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Additional, LaneConrollerTextures.Atlas, LaneConrollerTextures.WholeStreetHeaderButton, "Apply to whole street", LaneConrollerTool.ApplyWholeStreet);
        Content.AddButton(WholeStreetButton);
    }

    public void Init(float height) => base.Init(height);

    public override void Refresh() {
        PasteButton.Enable = false; // !SingletonTool<NodeMarkupTool>.Instance.IsMarkupBufferEmpty;
        base.Refresh();
    }
}

