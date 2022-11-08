using ColossalFramework.Math;
using UnityEngine;

namespace LaneConroller.Util
{
    public static class BezierUtil
    {
        public static Bezier3 Add(this Bezier3 lhs, Bezier3 rhs) {
            return new Bezier3(
                lhs.a + rhs.a,
                lhs.b + rhs.b,
                lhs.c + rhs.c,
                lhs.d + rhs.d);
        }

        public static bool EqualsTo(this Bezier3 lhs, Bezier3 rhs) {
            return lhs.a == rhs.a && lhs.b == rhs.b && lhs.c == rhs.c && lhs.d == rhs.d;
        }

        public static float middleT1 = 0.1f;
        public static float middleT2 = 0.9f;
        public static Bezier3 MathLine(Vector3 startDir, Vector3 endDir, Bezier3 basic, float shift){
            Vector3 shiftedStartPos = CalcShift(basic.a, startDir, ShiftAtOffset(shift, 0));
            Vector3 shiftedStartMiddlePos = CalcShift(basic, middleT1, ShiftAtOffset(shift, middleT1 / 2));
            Vector3 shiftedMiddleEndPos = CalcShift(basic, middleT2, ShiftAtOffset(shift, 0.5f + middleT2 / 2));
            Vector3 shiftedEndPos = CalcShift(basic.d, -endDir, ShiftAtOffset(shift, 1));

            var bezier = CalcPerfict(
                shiftedStartPos, shiftedEndPos,
                shiftedStartMiddlePos, shiftedMiddleEndPos,
                middleT1, middleT2);

            return bezier;
        }

        #region shift
        public static float ShiftAtOffset(float shift, float t) {
            return shift * (t - 0.5f);
        }

        public static Vector3 CalcShift(Bezier3 basic, float t, float shift) {
            var pos = basic.Position(t);
            var dir = basic.Tangent(t);
            var shiftPos = CalcShift(pos, dir, shift);
            return shiftPos;
        }
        public static Vector3 Turn90(this Vector3 v, bool isClockWise) => isClockWise
            ? new Vector3(v.z, v.y, -v.x)
            : new Vector3(-v.z, v.y, v.x);

        public static Vector3 CalcShift(Vector3 pos, Vector3 dir, float shift) => pos + dir.Turn90(true).normalized * shift;

        public static Bezier3 Shift(this Bezier3 bezier, float shift, float vshift) {
            float len0 = (bezier.d - bezier.a).magnitude;
            Vector3 dira = bezier.b - bezier.a;
            bezier.a = CalcShift(bezier.a, dira, shift);
            bezier.a.y += vshift;

            Vector3 dird = bezier.c - bezier.d;
            bezier.d = CalcShift(bezier.d, -dird, shift);
            bezier.d.y += vshift;

            float len = (bezier.d - bezier.a).magnitude;
            float r = len / len0;
            bezier.b = bezier.a + dira * r;
            bezier.c = bezier.d + dird * r;

            return bezier;
        }
        #endregion 

        public static Bezier3 CalcPerfict(Vector3 start, Vector3 end, Vector3 point1, Vector3 point2, float t1, float t2)
        {
            CalcCoef(t1, out float a1, out float b1, out float c1, out float d1);
            CalcCoef(t2, out float a2, out float b2, out float c2, out float d2);

            Vector3 u1 = CalcU(start, end, point1, a1, d1);
            Vector3 u2 = CalcU(start, end, point2, a2, d2);

            CalcMiddlePoints(b1, c1, u1, b2, c2, u2, out Vector3 middle1, out Vector3 middle2);

            var bezier = new Bezier3()
            {
                a = start,
                b = middle1,
                c = middle2,
                d = end
            };

            return bezier;
        }

        public static void CalcCoef(float t, out float a, out float b, out float c, out float d) {
            var mt = 1 - t;
            a = mt * mt * mt;
            b = 3 * t * mt * mt;
            c = 3 * t * t * mt;
            d = t * t * t;
        }

        public static float CalcU(float start, float end, float point, float a, float d) => point - (a * start) - (d * end);

        public static void CalcMiddlePoints(float b1, float c1, float u1, float b2, float c2, float u2, out float m1, out float m2)
        {
            m2 = (u2 - (b2 / b1 * u1)) / (c2 - (b2 / b1 * c1));
            m1 = (u1 - c1 * m2) / b1;
        }

        public static Vector3 CalcU(Vector3 start, Vector3 end, Vector3 point, float a, float d) {
            return new Vector3(
                CalcU(start.x, end.x, point.x, a, d),
                CalcU(start.y, end.y, point.y, a, d),
                CalcU(start.z, end.z, point.z, a, d));
        }
        public static void CalcMiddlePoints(
            float b1, float c1, Vector3 u1,
            float b2, float c2, Vector3 u2,
            out Vector3 middle1, out Vector3 middle2) {
            CalcMiddlePoints(b1, c1, u1.x, b2, c2, u2.x, out middle1.x, out middle2.x);
            CalcMiddlePoints(b1, c1, u1.y, b2, c2, u2.y, out middle1.y, out middle2.y);
            CalcMiddlePoints(b1, c1, u1.z, b2, c2, u2.z, out middle1.z, out middle2.z);
        }
    }
}
