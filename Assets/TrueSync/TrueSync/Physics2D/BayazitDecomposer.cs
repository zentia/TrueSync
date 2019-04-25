namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    internal static class BayazitDecomposer
    {
        private static TSVector2 At(int i, Vertices vertices)
        {
            int count = vertices.Count;
            return vertices[(i < 0) ? ((count - 1) - ((-i - 1) % count)) : (i % count)];
        }

        private static bool CanSee(int i, int j, Vertices vertices)
        {
            if (Reflex(i, vertices))
            {
                if (LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)) && RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)))
                {
                    return false;
                }
            }
            else if (RightOn(At(i, vertices), At(i + 1, vertices), At(j, vertices)) || LeftOn(At(i, vertices), At(i - 1, vertices), At(j, vertices)))
            {
                return false;
            }
            if (Reflex(j, vertices))
            {
                if (LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)) && RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)))
                {
                    return false;
                }
            }
            else if (RightOn(At(j, vertices), At(j + 1, vertices), At(i, vertices)) || LeftOn(At(j, vertices), At(j - 1, vertices), At(i, vertices)))
            {
                return false;
            }
            for (int k = 0; k < vertices.Count; k++)
            {
                TSVector2 vector;
                if (((((((k + 1) % vertices.Count) != i) && (k != i)) && (((k + 1) % vertices.Count) != j)) && (k != j)) && LineTools.LineIntersect(At(i, vertices), At(j, vertices), At(k, vertices), At(k + 1, vertices), out vector))
                {
                    return false;
                }
            }
            return true;
        }

        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            Debug.Assert(vertices.Count > 3);
            Debug.Assert(vertices.IsCounterClockWise());
            return TriangulatePolygon(vertices);
        }

        private static Vertices Copy(int i, int j, Vertices vertices)
        {
            while (j < i)
            {
                j += vertices.Count;
            }
            Vertices vertices2 = new Vertices(j);
            while (i <= j)
            {
                vertices2.Add(At(i, vertices));
                i++;
            }
            return vertices2;
        }

        private static bool Left(TSVector2 a, TSVector2 b, TSVector2 c)
        {
            return (MathUtils.Area(ref a, ref b, ref c) > 0);
        }

        private static bool LeftOn(TSVector2 a, TSVector2 b, TSVector2 c)
        {
            return (MathUtils.Area(ref a, ref b, ref c) >= 0);
        }

        private static bool Reflex(int i, Vertices vertices)
        {
            return Right(i, vertices);
        }

        private static bool Right(int i, Vertices vertices)
        {
            return Right(At(i - 1, vertices), At(i, vertices), At(i + 1, vertices));
        }

        private static bool Right(TSVector2 a, TSVector2 b, TSVector2 c)
        {
            return (MathUtils.Area(ref a, ref b, ref c) < 0);
        }

        private static bool RightOn(TSVector2 a, TSVector2 b, TSVector2 c)
        {
            return (MathUtils.Area(ref a, ref b, ref c) <= 0);
        }

        private static FP SquareDist(TSVector2 a, TSVector2 b)
        {
            FP fp = b.x - a.x;
            FP fp2 = b.y - a.y;
            return ((fp * fp) + (fp2 * fp2));
        }

        private static List<Vertices> TriangulatePolygon(Vertices vertices)
        {
            Vertices vertices2;
            Vertices vertices3;
            List<Vertices> list = new List<Vertices>();
            TSVector2 vector = new TSVector2();
            TSVector2 vector2 = new TSVector2();
            int i = 0;
            int j = 0;
            for (int k = 0; k < vertices.Count; k++)
            {
                if (Reflex(k, vertices))
                {
                    FP fp;
                    FP fp2 = fp = FP.MaxValue;
                    for (int m = 0; m < vertices.Count; m++)
                    {
                        FP fp3;
                        TSVector2 vector3;
                        if (Left(At(k - 1, vertices), At(k, vertices), At(m, vertices)) && RightOn(At(k - 1, vertices), At(k, vertices), At(m - 1, vertices)))
                        {
                            vector3 = LineTools.LineIntersect(At(k - 1, vertices), At(k, vertices), At(m, vertices), At(m - 1, vertices));
                            if (Right(At(k + 1, vertices), At(k, vertices), vector3))
                            {
                                fp3 = SquareDist(At(k, vertices), vector3);
                                if (fp3 < fp2)
                                {
                                    fp2 = fp3;
                                    vector = vector3;
                                    i = m;
                                }
                            }
                        }
                        if (Left(At(k + 1, vertices), At(k, vertices), At(m + 1, vertices)) && RightOn(At(k + 1, vertices), At(k, vertices), At(m, vertices)))
                        {
                            vector3 = LineTools.LineIntersect(At(k + 1, vertices), At(k, vertices), At(m, vertices), At(m + 1, vertices));
                            if (Left(At(k - 1, vertices), At(k, vertices), vector3))
                            {
                                fp3 = SquareDist(At(k, vertices), vector3);
                                if (fp3 < fp)
                                {
                                    fp = fp3;
                                    j = m;
                                    vector2 = vector3;
                                }
                            }
                        }
                    }
                    if (i == ((j + 1) % vertices.Count))
                    {
                        TSVector2 item = (vector + vector2) / 2;
                        vertices2 = Copy(k, j, vertices);
                        vertices2.Add(item);
                        vertices3 = Copy(i, k, vertices);
                        vertices3.Add(item);
                    }
                    else
                    {
                        FP fp4 = 0;
                        FP fp5 = i;
                        while (j < i)
                        {
                            j += vertices.Count;
                        }
                        for (int n = i; n <= j; n++)
                        {
                            if (CanSee(k, n, vertices))
                            {
                                FP fp6 = 1 / (SquareDist(At(k, vertices), At(n, vertices)) + 1);
                                if (Reflex(n, vertices))
                                {
                                    if (RightOn(At(n - 1, vertices), At(n, vertices), At(k, vertices)) && LeftOn(At(n + 1, vertices), At(n, vertices), At(k, vertices)))
                                    {
                                        fp6 += 3;
                                    }
                                    else
                                    {
                                        fp6 += 2;
                                    }
                                }
                                else
                                {
                                    fp6 += 1;
                                }
                                if (fp6 > fp4)
                                {
                                    fp5 = n;
                                    fp4 = fp6;
                                }
                            }
                        }
                        vertices2 = Copy(k, (int) ((long) fp5), vertices);
                        vertices3 = Copy((int) ((long) fp5), k, vertices);
                    }
                    list.AddRange(TriangulatePolygon(vertices2));
                    list.AddRange(TriangulatePolygon(vertices3));
                    return list;
                }
            }
            if (vertices.Count > Settings.MaxPolygonVertices)
            {
                vertices2 = Copy(0, vertices.Count / 2, vertices);
                vertices3 = Copy(vertices.Count / 2, 0, vertices);
                list.AddRange(TriangulatePolygon(vertices2));
                list.AddRange(TriangulatePolygon(vertices3));
            }
            else
            {
                list.Add(vertices);
            }
            return list;
        }
    }
}

