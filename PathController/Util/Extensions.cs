namespace PathController.Util {
    using ColossalFramework.Math;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using UnityEngine;

    internal static class Extensions {
        public static bool IsDefault(this Bezier3 bezier) {
            return bezier.a == default &&
                bezier.b == default &&
                bezier.c == default &&
                bezier.d == default;
        }
        public static ref Vector3 ControlPoint(ref this Bezier3 bezier, int i) {
            switch (i) {
                case 0: return ref bezier.a;
                case 1: return ref bezier.b;
                case 2: return ref bezier.c;
                case 3: return ref bezier.d;
                default: throw new IndexOutOfRangeException("i=" + i);
            }
        }
    }
}
