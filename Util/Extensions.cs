namespace PathController.Util {
    using ColossalFramework.Math;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal static class Extensions {
        public static bool IsDefault(this Bezier3 bezier) {
            return bezier.a == default &&
                bezier.b == default &&
                bezier.c == default &&
                bezier.d == default;
        }
    }
}
