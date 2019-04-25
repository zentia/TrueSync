namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class LineTools
    {
        public static FP DistanceBetweenPointAndLineSegment(ref TSVector2 point, ref TSVector2 start, ref TSVector2 end)
        {
            if (start == end)
            {
                return TSVector2.Distance(point, start);
            }
            TSVector2 vector = TSVector2.Subtract(end, start);
            FP fp = TSVector2.Dot(TSVector2.Subtract(point, start), vector);
            if (fp <= 0)
            {
                return TSVector2.Distance(point, start);
            }
            FP fp2 = TSVector2.Dot(vector, vector);
            if (fp2 <= fp)
            {
                return TSVector2.Distance(point, end);
            }
            FP scaleFactor = fp / fp2;
            TSVector2 vector3 = TSVector2.Add(start, TSVector2.Multiply(vector, scaleFactor));
            return TSVector2.Distance(point, vector3);
        }

        public static TSVector2 LineIntersect(TSVector2 p1, TSVector2 p2, TSVector2 q1, TSVector2 q2)
        {
            TSVector2 zero = TSVector2.zero;
            FP fp = p2.y - p1.y;
            FP fp2 = p1.x - p2.x;
            FP fp3 = (fp * p1.x) + (fp2 * p1.y);
            FP fp4 = q2.y - q1.y;
            FP fp5 = q1.x - q2.x;
            FP fp6 = (fp4 * q1.x) + (fp5 * q1.y);
            FP fp7 = (fp * fp5) - (fp4 * fp2);
            if (!MathUtils.FPEquals(fp7, 0))
            {
                zero.x = ((fp5 * fp3) - (fp2 * fp6)) / fp7;
                zero.y = ((fp * fp6) - (fp4 * fp3)) / fp7;
            }
            return zero;
        }

        public static bool LineIntersect(ref TSVector2 point1, ref TSVector2 point2, ref TSVector2 point3, ref TSVector2 point4, out TSVector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        }

        public static bool LineIntersect(TSVector2 point1, TSVector2 point2, TSVector2 point3, TSVector2 point4, out TSVector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, true, true, out intersectionPoint);
        }

        public static bool LineIntersect(ref TSVector2 point1, ref TSVector2 point2, ref TSVector2 point3, ref TSVector2 point4, bool firstIsSegment, bool secondIsSegment, out TSVector2 point)
        {
            point = new TSVector2();
            FP fp = point4.y - point3.y;
            FP fp2 = point2.x - point1.x;
            FP fp3 = point4.x - point3.x;
            FP fp4 = point2.y - point1.y;
            FP fp5 = (fp * fp2) - (fp3 * fp4);
            if ((fp5 < -Settings.Epsilon) || (fp5 > Settings.Epsilon))
            {
                FP fp6 = point1.y - point3.y;
                FP fp7 = point1.x - point3.x;
                FP fp8 = 1f / fp5;
                FP fp9 = (fp3 * fp6) - (fp * fp7);
                fp9 *= fp8;
                if (!firstIsSegment || ((fp9 >= 0f) && (fp9 <= 1f)))
                {
                    FP fp10 = (fp2 * fp6) - (fp4 * fp7);
                    fp10 *= fp8;
                    if ((!secondIsSegment || ((fp10 >= 0f) && (fp10 <= 1f))) && ((fp9 != 0f) || (fp10 != 0f)))
                    {
                        point.x = point1.x + (fp9 * fp2);
                        point.y = point1.y + (fp9 * fp4);
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool LineIntersect(TSVector2 point1, TSVector2 point2, TSVector2 point3, TSVector2 point4, bool firstIsSegment, bool secondIsSegment, out TSVector2 intersectionPoint)
        {
            return LineIntersect(ref point1, ref point2, ref point3, ref point4, firstIsSegment, secondIsSegment, out intersectionPoint);
        }

        public static bool LineIntersect2(ref TSVector2 a0, ref TSVector2 a1, ref TSVector2 b0, ref TSVector2 b1, out TSVector2 intersectionPoint)
        {
            intersectionPoint = TSVector2.zero;
            if ((((a0 != b0) && (a0 != b1)) && (a1 != b0)) && (a1 != b1))
            {
                FP x = a0.x;
                FP y = a0.y;
                FP fp3 = a1.x;
                FP fp4 = a1.y;
                FP fp5 = b0.x;
                FP fp6 = b0.y;
                FP fp7 = b1.x;
                FP fp8 = b1.y;
                if ((TSMath.Max(x, fp3) < TSMath.Min(fp5, fp7)) || (TSMath.Max(fp5, fp7) < TSMath.Min(x, fp3)))
                {
                    return false;
                }
                if ((TSMath.Max(y, fp4) < TSMath.Min(fp6, fp8)) || (TSMath.Max(fp6, fp8) < TSMath.Min(y, fp4)))
                {
                    return false;
                }
                FP fp9 = ((fp7 - fp5) * (y - fp6)) - ((fp8 - fp6) * (x - fp5));
                FP fp10 = ((fp3 - x) * (y - fp6)) - ((fp4 - y) * (x - fp5));
                FP fp11 = ((fp8 - fp6) * (fp3 - x)) - ((fp7 - fp5) * (fp4 - y));
                if (FP.Abs(fp11) < Settings.Epsilon)
                {
                    return false;
                }
                fp9 /= fp11;
                fp10 /= fp11;
                if ((((0 < fp9) && (fp9 < 1)) && (0 < fp10)) && (fp10 < 1))
                {
                    intersectionPoint.x = x + (fp9 * (fp3 - x));
                    intersectionPoint.y = y + (fp9 * (fp4 - y));
                    return true;
                }
            }
            return false;
        }

        public static Vertices LineSegmentAABBIntersect(ref TSVector2 point1, ref TSVector2 point2, AABB aabb)
        {
            return LineSegmentVerticesIntersect(ref point1, ref point2, aabb.Vertices);
        }

        public static Vertices LineSegmentVerticesIntersect(ref TSVector2 point1, ref TSVector2 point2, Vertices vertices)
        {
            Vertices vertices2 = new Vertices();
            for (int i = 0; i < vertices.Count; i++)
            {
                TSVector2 vector;
                if (LineIntersect(vertices[i], vertices[vertices.NextIndex(i)], point1, point2, true, true, out vector))
                {
                    vertices2.Add(vector);
                }
            }
            return vertices2;
        }
    }
}

