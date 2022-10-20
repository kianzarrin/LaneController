using ColossalFramework.Math;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace PathController.Util
{
    public static class BezierUtil
    {
        public static float middleT1 = 0.1f;
        public static float middleT2 = 0.9f;

        public static Vector3 Turn90(this Vector3 v, bool isClockWise) => isClockWise ? new Vector3(v.z, v.y, -v.x) : new Vector3(-v.z, v.y, v.x);
        public static Bezier3 MathLine(Vector3 startDir, Vector3 endDir, Bezier3 basic, float shift)
        {
            var shiftStartPos = CalcShift(basic.a, startDir, ShiftAtPoint(shift, 0));
            var shiftStartMiddlePos = CalcShift(basic, middleT1, ShiftAtPoint(shift, middleT1 / 2));
            var shiftMiddleEndPos = CalcShift(basic, middleT2, ShiftAtPoint(shift, 0.5f + middleT2 / 2));
            var shiftEndPos = CalcShift(basic.d, endDir, -ShiftAtPoint(shift, 1));

            var bezier = CalcPerfict(shiftStartPos, shiftEndPos, shiftStartMiddlePos, shiftMiddleEndPos, middleT1, middleT2);

            return bezier;
        }
        public static float ShiftAtPoint(float shift, float point)
        {
            if (point < 0.5f)
                return -shift * (0.5f - point);
            else
                return shift * (point - 0.5f);
        }
        public static Vector3 CalcShift(Bezier3 basic, float t, float shift)
        {
            var pos = basic.Position(t);
            var dir = basic.Tangent(t);
            var shiftPos = CalcShift(pos, dir, shift);
            return shiftPos;
        }
        public static Vector3 CalcShift(Vector3 pos, Vector3 dir, float shift) => pos + dir.Turn90(true).normalized * shift;

        public static Bezier3 Shift(this Bezier3 bezier, float shift, float vshift, bool smootha, bool smoothb) {
            Vector3 dira = bezier.b - bezier.a;
            bezier.a = CalcShift(bezier.a, dira, shift);
            bezier.a.y += vshift;

            Vector3 dird = bezier.c - bezier.d;
            bezier.d = CalcShift(bezier.d, -dird, shift);
            bezier.d.y += vshift;

            NetSegment.CalculateMiddlePoints(
                startPos: bezier.a, startDir: dira, smoothStart: smootha, middlePos1: out bezier.b,
                endPos: bezier.d, endDir: dird,smoothEnd: smoothb, middlePos2: out bezier.c);
            return bezier;
        }

        public static Bezier3 Add(this Bezier3 lhs, Bezier3 rhs) {
            return new Bezier3(
                lhs.a + rhs.a,
                lhs.b + rhs.b,
                lhs.c + rhs.c,
                lhs.d + rhs.d);
        }

        public static Bezier3 CalcPerfict(Vector3 start, Vector3 end, Vector3 point1, Vector3 point2, float t1, float t2)
        {
            CalcCoef(t1, out float a1, out float b1, out float c1, out float d1);
            CalcCoef(t2, out float a2, out float b2, out float c2, out float d2);

            CalcU(start, end, point1, a1, d1, out float ux1, out float uy1, out float uz1);
            CalcU(start, end, point2, a2, d2, out float ux2, out float uy2, out float uz2);

            CalcCoordinate(b1, c1, ux1, b2, c2, ux2, out float m1x, out float m2x);
            CalcCoordinate(b1, c1, uy1, b2, c2, uy2, out float m1y, out float m2y);
            CalcCoordinate(b1, c1, uz1, b2, c2, uz2, out float m1z, out float m2z);

            var middle1 = new Vector3(m1x, m1y, m1z);
            var middle2 = new Vector3(m2x, m2y, m2z);

            var bezier = new Bezier3()
            {
                a = start,
                b = middle1,
                c = middle2,
                d = end
            };

            return bezier;
        }
        public static void CalcCoef(float t, out float a, out float b, out float c, out float d)
        {
            var mt = 1 - t;
            a = mt * mt * mt;
            b = 3 * t * mt * mt;
            c = 3 * t * t * mt;
            d = t * t * t;
        }
        public static void CalcU(Vector3 start, Vector3 end, Vector3 point, float a, float d, out float ux, out float uy, out float uz)
        {
            ux = CalcU(start.x, end.x, point.x, a, d);
            uy = CalcU(start.y, end.y, point.y, a, d);
            uz = CalcU(start.z, end.z, point.z, a, d);
        }
        public static float CalcU(float start, float end, float point, float a, float d) => point - (a * start) - (d * end);
        public static void CalcCoordinate(float b1, float c1, float u1, float b2, float c2, float u2, out float coordinate1, out float coordinate2)
        {
            coordinate2 = (u2 - (b2 / b1 * u1)) / (c2 - (b2 / b1 * c1));
            coordinate1 = (u1 - c1 * coordinate2) / b1;
        }
    }
}
