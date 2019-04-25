namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    internal static class EarclipDecomposer
    {
        public static List<Vertices> ConvexPartition(Vertices vertices, FP tolerance)
        {
            Debug.Assert(vertices.Count > 3);
            Debug.Assert(!vertices.IsCounterClockWise());
            return TriangulatePolygon(vertices, tolerance);
        }

        private static bool IsEar(int i, FP[] xv, FP[] yv, int xvLength)
        {
            FP fp;
            FP fp2;
            FP fp3;
            FP fp4;
            if (((i >= xvLength) || (i < 0)) || (xvLength < 3))
            {
                return false;
            }
            int index = i + 1;
            int num2 = i - 1;
            if (i == 0)
            {
                fp = xv[0] - xv[xvLength - 1];
                fp2 = yv[0] - yv[xvLength - 1];
                fp3 = xv[1] - xv[0];
                fp4 = yv[1] - yv[0];
                num2 = xvLength - 1;
            }
            else if (i == (xvLength - 1))
            {
                fp = xv[i] - xv[i - 1];
                fp2 = yv[i] - yv[i - 1];
                fp3 = xv[0] - xv[i];
                fp4 = yv[0] - yv[i];
                index = 0;
            }
            else
            {
                fp = xv[i] - xv[i - 1];
                fp2 = yv[i] - yv[i - 1];
                fp3 = xv[i + 1] - xv[i];
                fp4 = yv[i + 1] - yv[i];
            }
            FP fp5 = (fp * fp4) - (fp3 * fp2);
            if (fp5 > 0)
            {
                return false;
            }
            Triangle triangle = new Triangle(xv[i], yv[i], xv[index], yv[index], xv[num2], yv[num2]);
            for (int j = 0; j < xvLength; j++)
            {
                if ((((j != i) && (j != num2)) && (j != index)) && triangle.IsInside(xv[j], yv[j]))
                {
                    return false;
                }
            }
            return true;
        }

        private static int Remainder(int x, int modulus)
        {
            int num = x % modulus;
            while (num < 0)
            {
                num += modulus;
            }
            return num;
        }

        private static bool ResolvePinchPoint(Vertices pin, out Vertices poutA, out Vertices poutB, FP tolerance)
        {
            poutA = new Vertices();
            poutB = new Vertices();
            if (pin.Count < 3)
            {
                return false;
            }
            bool flag = false;
            int num = -1;
            int num2 = -1;
            for (int i = 0; i < pin.Count; i++)
            {
                for (int j = i + 1; j < pin.Count; j++)
                {
                    if (((FP.Abs(pin[i].x - pin[j].x) < tolerance) && (FP.Abs(pin[i].y - pin[j].y) < tolerance)) && (j != (i + 1)))
                    {
                        num = i;
                        num2 = j;
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            if (flag)
            {
                int num5 = num2 - num;
                if (num5 == pin.Count)
                {
                    return false;
                }
                for (int k = 0; k < num5; k++)
                {
                    int num8 = Remainder(num + k, pin.Count);
                    poutA.Add(pin[num8]);
                }
                int num6 = pin.Count - num5;
                for (int m = 0; m < num6; m++)
                {
                    int num10 = Remainder(num2 + m, pin.Count);
                    poutB.Add(pin[num10]);
                }
            }
            return flag;
        }

        private static List<Vertices> TriangulatePolygon(Vertices vertices, FP tolerance)
        {
            Vertices vertices2;
            Vertices vertices3;
            if (vertices.Count < 3)
            {
                return new List<Vertices>();
            }
            List<Vertices> list = new List<Vertices>();
            Vertices pin = new Vertices(vertices);
            if (ResolvePinchPoint(pin, out vertices2, out vertices3, tolerance))
            {
                List<Vertices> list3 = TriangulatePolygon(vertices2, tolerance);
                List<Vertices> list4 = TriangulatePolygon(vertices3, tolerance);
                if ((list3.Count == -1) || (list4.Count == -1))
                {
                    throw new Exception("Can't triangulate your polygon.");
                }
                for (int k = 0; k < list3.Count; k++)
                {
                    list.Add(new Vertices(list3[k]));
                }
                for (int m = 0; m < list4.Count; m++)
                {
                    list.Add(new Vertices(list4[m]));
                }
                return list;
            }
            Vertices[] verticesArray = new Vertices[vertices.Count - 2];
            int index = 0;
            FP[] xv = new FP[vertices.Count];
            FP[] yv = new FP[vertices.Count];
            for (int i = 0; i < vertices.Count; i++)
            {
                xv[i] = vertices[i].x;
                yv[i] = vertices[i].y;
            }
            int count = vertices.Count;
            while (count > 3)
            {
                int num6 = -1;
                FP fp = -10f;
                for (int n = 0; n < count; n++)
                {
                    if (IsEar(n, xv, yv, count))
                    {
                        FP fp2;
                        FP fp3;
                        FP fp4;
                        int num11 = Remainder(n - 1, count);
                        int num12 = Remainder(n + 1, count);
                        TSVector2 a = new TSVector2(xv[num12] - xv[n], yv[num12] - yv[n]);
                        TSVector2 b = new TSVector2(xv[n] - xv[num11], yv[n] - yv[num11]);
                        TSVector2 vector3 = new TSVector2(xv[num11] - xv[num12], yv[num11] - yv[num12]);
                        a.Normalize();
                        b.Normalize();
                        vector3.Normalize();
                        MathUtils.Cross(ref a, ref b, out fp2);
                        fp2 = FP.Abs(fp2);
                        MathUtils.Cross(ref b, ref vector3, out fp3);
                        fp3 = FP.Abs(fp3);
                        MathUtils.Cross(ref vector3, ref a, out fp4);
                        fp4 = FP.Abs(fp4);
                        FP fp5 = TSMath.Min(fp2, TSMath.Min(fp3, fp4));
                        if (fp5 > fp)
                        {
                            num6 = n;
                            fp = fp5;
                        }
                    }
                }
                if (num6 == -1)
                {
                    for (int num13 = 0; num13 < index; num13++)
                    {
                        list.Add(verticesArray[num13]);
                    }
                    return list;
                }
                count--;
                FP[] fpArray3 = new FP[count];
                FP[] fpArray4 = new FP[count];
                int num7 = 0;
                for (int num14 = 0; num14 < count; num14++)
                {
                    if (num7 == num6)
                    {
                        num7++;
                    }
                    fpArray3[num14] = xv[num7];
                    fpArray4[num14] = yv[num7];
                    num7++;
                }
                int num8 = (num6 == 0) ? count : (num6 - 1);
                int num9 = (num6 == count) ? 0 : (num6 + 1);
                verticesArray[index] = new Triangle(xv[num6], yv[num6], xv[num9], yv[num9], xv[num8], yv[num8]);
                index++;
                xv = fpArray3;
                yv = fpArray4;
            }
            verticesArray[index] = new Triangle(xv[1], yv[1], xv[2], yv[2], xv[0], yv[0]);
            index++;
            for (int j = 0; j < index; j++)
            {
                list.Add(new Vertices(verticesArray[j]));
            }
            return list;
        }

        private class Triangle : Vertices
        {
            public Triangle(FP x1, FP y1, FP x2, FP y2, FP x3, FP y3)
            {
                FP fp = ((x2 - x1) * (y3 - y1)) - ((x3 - x1) * (y2 - y1));
                if (fp > 0)
                {
                    base.Add(new TSVector2(x1, y1));
                    base.Add(new TSVector2(x2, y2));
                    base.Add(new TSVector2(x3, y3));
                }
                else
                {
                    base.Add(new TSVector2(x1, y1));
                    base.Add(new TSVector2(x3, y3));
                    base.Add(new TSVector2(x2, y2));
                }
            }

            public bool IsInside(FP x, FP y)
            {
                TSVector2 vector = base[0];
                TSVector2 vector2 = base[1];
                TSVector2 vector3 = base[2];
                if (((x < vector.x) && (x < vector2.x)) && (x < vector3.x))
                {
                    return false;
                }
                if (((x > vector.x) && (x > vector2.x)) && (x > vector3.x))
                {
                    return false;
                }
                if (((y < vector.y) && (y < vector2.y)) && (y < vector3.y))
                {
                    return false;
                }
                if (((y > vector.y) && (y > vector2.y)) && (y > vector3.y))
                {
                    return false;
                }
                FP fp = x - vector.x;
                FP fp2 = y - vector.y;
                FP fp3 = vector2.x - vector.x;
                FP fp4 = vector2.y - vector.y;
                FP fp5 = vector3.x - vector.x;
                FP fp6 = vector3.y - vector.y;
                FP fp7 = (fp5 * fp5) + (fp6 * fp6);
                FP fp8 = (fp5 * fp3) + (fp6 * fp4);
                FP fp9 = (fp5 * fp) + (fp6 * fp2);
                FP fp10 = (fp3 * fp3) + (fp4 * fp4);
                FP fp11 = (fp3 * fp) + (fp4 * fp2);
                FP fp12 = 1f / ((fp7 * fp10) - (fp8 * fp8));
                FP fp13 = ((fp10 * fp9) - (fp8 * fp11)) * fp12;
                FP fp14 = ((fp7 * fp11) - (fp8 * fp9)) * fp12;
                return (((fp13 > 0) && (fp14 > 0)) && ((fp13 + fp14) < 1));
            }
        }
    }
}

