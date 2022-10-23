using ColossalFramework.UI;
using ModsCommon.Utilities;
using System.Collections.Generic;
using UnityEngine;

namespace PathController.UI
{
    public static class PathControllerTextures
    {
        public static UITextureAtlas Atlas;
        public static Texture2D Texture => Atlas.texture;

        public static string CopyHeaderButton => nameof(CopyHeaderButton);
        public static string PasteHeaderButton => nameof(PasteHeaderButton);
        public static string ResetHeaderButton => nameof(ResetHeaderButton);
        public static string ResetControlPointsHeaderButton => nameof(ResetControlPointsHeaderButton);
        public static string BeetwenIntersectionsHeaderButton => nameof(BeetwenIntersectionsHeaderButton);
        public static string WholeStreetHeaderButton => nameof(WholeStreetHeaderButton);


        static PathControllerTextures()
        {
            var spriteParams = new Dictionary<string, RectOffset>();

            //HeaderButtons
            spriteParams[CopyHeaderButton] = new RectOffset();
            spriteParams[PasteHeaderButton] = new RectOffset();
            spriteParams[ResetHeaderButton] = new RectOffset();
            spriteParams[ResetControlPointsHeaderButton] = new RectOffset();
            spriteParams[BeetwenIntersectionsHeaderButton] = new RectOffset();
            spriteParams[WholeStreetHeaderButton] = new RectOffset();
            Atlas = TextureHelper.CreateAtlas(nameof(NodeMarkup), spriteParams);
        }
    }
}
