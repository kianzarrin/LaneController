namespace PathController.UI;
using ModsCommon.UI;
using ModsCommon.Utilities;
using PathController.Tool;

public class PanelHeader : HeaderMoveablePanel<PanelHeaderContent> {
    public bool Available { set => Content.SetAvailable(value); }

    private HeaderButtonInfo<HeaderButton> PasteButton { get; }
    private HeaderButtonInfo<HeaderButton> BeetwenIntersectionsButton { get; }
    private HeaderButtonInfo<HeaderButton> WholeStreetButton { get; }

    public PanelHeader() {
        Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, PathControllerTextures.Atlas, PathControllerTextures.CopyHeaderButton, "Copy", PathControllerTool.Copy));

        PasteButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, PathControllerTextures.Atlas, PathControllerTextures.PasteHeaderButton, "Paste", PathControllerTool.Paste);
        Content.AddButton(PasteButton);

        Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Main, PathControllerTextures.Atlas, PathControllerTextures.ResetHeaderButton, "Reset", PathControllerTool.DeleteAll));

        Content.AddButton(new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Additional, PathControllerTextures.Atlas, PathControllerTextures.ResetControlPointsHeaderButton, "Reset control points", PathControllerTool.ResetControlPoints));


        BeetwenIntersectionsButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Additional, PathControllerTextures.Atlas, PathControllerTextures.BeetwenIntersectionsHeaderButton, "Apply between intersections", PathControllerTool.ApplyBetweenIntersections);
        Content.AddButton(BeetwenIntersectionsButton);

        WholeStreetButton = new HeaderButtonInfo<HeaderButton>(HeaderButtonState.Additional, PathControllerTextures.Atlas, PathControllerTextures.WholeStreetHeaderButton, "Apply to whole street", PathControllerTool.ApplyWholeStreet);
        Content.AddButton(WholeStreetButton);
    }

    public void Init(float height) => base.Init(height);

    public override void Refresh() {
        PasteButton.Enable = false; // !SingletonTool<NodeMarkupTool>.Instance.IsMarkupBufferEmpty;
        base.Refresh();
    }
}

