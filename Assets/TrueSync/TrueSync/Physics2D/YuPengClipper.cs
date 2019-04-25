namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class YuPengClipper
    {
        private static readonly FP ClipperEpsilonSquared = 1.192093E-07f;

        private static PolyClipError BuildPolygonsFromChain(List<Edge> simplicies, out List<Vertices> result)
        {
            result = new List<Vertices>();
            PolyClipError none = PolyClipError.None;
            while (simplicies.Count > 0)
            {
                Vertices item = new Vertices();
                item.Add(simplicies[0].EdgeStart);
                item.Add(simplicies[0].EdgeEnd);
                simplicies.RemoveAt(0);
                bool flag = false;
                int index = 0;
                int count = simplicies.Count;
                while (!flag && (simplicies.Count > 0))
                {
                    if (VectorEqual(item[item.Count - 1], simplicies[index].EdgeStart))
                    {
                        if (VectorEqual(simplicies[index].EdgeEnd, item[0]))
                        {
                            flag = true;
                        }
                        else
                        {
                            item.Add(simplicies[index].EdgeEnd);
                        }
                        simplicies.RemoveAt(index);
                        index--;
                    }
                    else if (VectorEqual(item[item.Count - 1], simplicies[index].EdgeEnd))
                    {
                        if (VectorEqual(simplicies[index].EdgeStart, item[0]))
                        {
                            flag = true;
                        }
                        else
                        {
                            item.Add(simplicies[index].EdgeStart);
                        }
                        simplicies.RemoveAt(index);
                        index--;
                    }
                    if (!flag && (++index == simplicies.Count))
                    {
                        if (count == simplicies.Count)
                        {
                            result = new List<Vertices>();
                            Debug.WriteLine("Undefined error while building result polygon(s).");
                            return PolyClipError.BrokenResult;
                        }
                        index = 0;
                        count = simplicies.Count;
                    }
                }
                if (item.Count < 3)
                {
                    none = PolyClipError.DegeneratedOutput;
                    Debug.WriteLine("Degenerated output polygon produced (vertices < 3).");
                }
                result.Add(item);
            }
            return none;
        }

        private static FP CalculateBeta(TSVector2 point, Edge e, FP coefficient)
        {
            FP fp = 0f;
            if (PointInSimplex(point, e))
            {
                fp = coefficient;
            }
            if (PointOnLineSegment(TSVector2.zero, e.EdgeStart, point) || PointOnLineSegment(TSVector2.zero, e.EdgeEnd, point))
            {
                fp = 0.5f * coefficient;
            }
            return fp;
        }

        private static void CalculateIntersections(Vertices polygon1, Vertices polygon2, out Vertices slicedPoly1, out Vertices slicedPoly2)
        {
            TSVector2 vector6;
            slicedPoly1 = new Vertices(polygon1);
            slicedPoly2 = new Vertices(polygon2);
            for (int i = 0; i < polygon1.Count; i++)
            {
                TSVector2 vector = polygon1[i];
                TSVector2 vector2 = polygon1[polygon1.NextIndex(i)];
                for (int m = 0; m < polygon2.Count; m++)
                {
                    TSVector2 vector5;
                    TSVector2 vector3 = polygon2[m];
                    TSVector2 vector4 = polygon2[polygon2.NextIndex(m)];
                    if (LineTools.LineIntersect(vector, vector2, vector3, vector4, out vector5))
                    {
                        FP fp = GetAlpha(vector, vector2, vector5);
                        if ((fp > 0f) && (fp < 1f))
                        {
                            int index = slicedPoly1.IndexOf(vector) + 1;
                            while ((index < slicedPoly1.Count) && (GetAlpha(vector, vector2, slicedPoly1[index]) <= fp))
                            {
                                index++;
                            }
                            slicedPoly1.Insert(index, vector5);
                        }
                        fp = GetAlpha(vector3, vector4, vector5);
                        if ((fp > 0f) && (fp < 1f))
                        {
                            int num4 = slicedPoly2.IndexOf(vector3) + 1;
                            while ((num4 < slicedPoly2.Count) && (GetAlpha(vector3, vector4, slicedPoly2[num4]) <= fp))
                            {
                                num4++;
                            }
                            slicedPoly2.Insert(num4, vector5);
                        }
                    }
                }
            }
            for (int j = 0; j < slicedPoly1.Count; j++)
            {
                int num6 = slicedPoly1.NextIndex(j);
                vector6 = slicedPoly1[num6] - slicedPoly1[j];
                if (vector6.LengthSquared() <= ClipperEpsilonSquared)
                {
                    slicedPoly1.RemoveAt(j);
                    j--;
                }
            }
            for (int k = 0; k < slicedPoly2.Count; k++)
            {
                int num8 = slicedPoly2.NextIndex(k);
                vector6 = slicedPoly2[num8] - slicedPoly2[k];
                if (vector6.LengthSquared() <= ClipperEpsilonSquared)
                {
                    slicedPoly2.RemoveAt(k);
                    k--;
                }
            }
        }

        private static void CalculateResultChain(List<FP> poly1Coeff, List<Edge> poly1Simplicies, List<FP> poly2Coeff, List<Edge> poly2Simplicies, PolyClipType clipType, out List<Edge> resultSimplices)
        {
            resultSimplices = new List<Edge>();
            for (int i = 0; i < poly1Simplicies.Count; i++)
            {
                FP fp = 0;
                if (poly2Simplicies.Contains(poly1Simplicies[i]))
                {
                    fp = 1f;
                }
                else if (poly2Simplicies.Contains(-poly1Simplicies[i]) && (clipType == PolyClipType.Union))
                {
                    fp = 1f;
                }
                else
                {
                    for (int k = 0; k < poly2Simplicies.Count; k++)
                    {
                        if (!poly2Simplicies.Contains(-poly1Simplicies[i]))
                        {
                            fp += CalculateBeta(poly1Simplicies[i].GetCenter(), poly2Simplicies[k], poly2Coeff[k]);
                        }
                    }
                }
                if (clipType == PolyClipType.Intersect)
                {
                    if (fp == 1f)
                    {
                        resultSimplices.Add(poly1Simplicies[i]);
                    }
                }
                else if (fp == 0f)
                {
                    resultSimplices.Add(poly1Simplicies[i]);
                }
            }
            for (int j = 0; j < poly2Simplicies.Count; j++)
            {
                FP fp2 = 0f;
                if (!resultSimplices.Contains(poly2Simplicies[j]) && !resultSimplices.Contains(-poly2Simplicies[j]))
                {
                    if (poly1Simplicies.Contains(-poly2Simplicies[j]) && (clipType == PolyClipType.Union))
                    {
                        fp2 = 1f;
                    }
                    else
                    {
                        fp2 = 0f;
                        for (int m = 0; m < poly1Simplicies.Count; m++)
                        {
                            if (!poly1Simplicies.Contains(poly2Simplicies[j]) && !poly1Simplicies.Contains(-poly2Simplicies[j]))
                            {
                                fp2 += CalculateBeta(poly2Simplicies[j].GetCenter(), poly1Simplicies[m], poly1Coeff[m]);
                            }
                        }
                        if ((clipType == PolyClipType.Intersect) || (clipType == PolyClipType.Difference))
                        {
                            if (fp2 == 1f)
                            {
                                resultSimplices.Add(-poly2Simplicies[j]);
                            }
                        }
                        else if (fp2 == 0f)
                        {
                            resultSimplices.Add(poly2Simplicies[j]);
                        }
                    }
                }
            }
        }

        private static FP CalculateSimplexCoefficient(TSVector2 a, TSVector2 b, TSVector2 c)
        {
            FP fp = MathUtils.Area(ref a, ref b, ref c);
            if (fp < 0f)
            {
                return -1f;
            }
            if (fp > 0f)
            {
                return 1f;
            }
            return 0f;
        }

        private static void CalculateSimplicalChain(Vertices poly, out List<FP> coeff, out List<Edge> simplicies)
        {
            simplicies = new List<Edge>();
            coeff = new List<FP>();
            for (int i = 0; i < poly.Count; i++)
            {
                simplicies.Add(new Edge(poly[i], poly[poly.NextIndex(i)]));
                coeff.Add(CalculateSimplexCoefficient(TSVector2.zero, poly[i], poly[poly.NextIndex(i)]));
            }
        }

        public static List<Vertices> Difference(Vertices polygon1, Vertices polygon2, out PolyClipError error)
        {
            return Execute(polygon1, polygon2, PolyClipType.Difference, out error);
        }

        private static List<Vertices> Execute(Vertices subject, Vertices clip, PolyClipType clipType, out PolyClipError error)
        {
            Vertices vertices;
            Vertices vertices2;
            TSVector2 vector3;
            List<Edge> list;
            List<FP> list2;
            List<Edge> list3;
            List<FP> list4;
            List<Edge> list5;
            List<Vertices> list6;
            Debug.Assert(subject.IsSimple() && clip.IsSimple(), "Non simple input!", "Input polygons must be simple (cannot intersect themselves).");
            CalculateIntersections(subject, clip, out vertices, out vertices2);
            TSVector2 lowerBound = subject.GetAABB().LowerBound;
            TSVector2 vector2 = clip.GetAABB().LowerBound;
            TSVector2.Min(ref lowerBound, ref vector2, out vector3);
            vector3 = TSVector2.one - vector3;
            if (vector3 != TSVector2.zero)
            {
                vertices.Translate(ref vector3);
                vertices2.Translate(ref vector3);
            }
            vertices.ForceCounterClockWise();
            vertices2.ForceCounterClockWise();
            CalculateSimplicalChain(vertices, out list2, out list);
            CalculateSimplicalChain(vertices2, out list4, out list3);
            CalculateResultChain(list2, list, list4, list3, clipType, out list5);
            error = BuildPolygonsFromChain(list5, out list6);
            vector3 = (TSVector2) (vector3 * -1f);
            for (int i = 0; i < list6.Count; i++)
            {
                list6[i].Translate(ref vector3);
                SimplifyTools.CollinearSimplify(list6[i], FP.Zero);
            }
            return list6;
        }

        private static FP GetAlpha(TSVector2 start, TSVector2 end, TSVector2 point)
        {
            TSVector2 vector = point - start;
            vector = end - start;
            FP introduced2 = vector.LengthSquared();
            return (introduced2 / vector.LengthSquared());
        }

        public static List<Vertices> Intersect(Vertices polygon1, Vertices polygon2, out PolyClipError error)
        {
            return Execute(polygon1, polygon2, PolyClipType.Intersect, out error);
        }

        private static bool PointInSimplex(TSVector2 point, Edge edge)
        {
            return (new Vertices { TSVector2.zero, edge.EdgeStart, edge.EdgeEnd }.PointInPolygon(ref point) == 1);
        }

        private static bool PointOnLineSegment(TSVector2 start, TSVector2 end, TSVector2 point)
        {
            TSVector2 vector = end - start;
            return (((MathUtils.Area(ref start, ref end, ref point) == 0f) && (TSVector2.Dot(point - start, vector) >= 0f)) && (TSVector2.Dot(point - end, vector) <= 0f));
        }

        public static List<Vertices> Union(Vertices polygon1, Vertices polygon2, out PolyClipError error)
        {
            return Execute(polygon1, polygon2, PolyClipType.Union, out error);
        }

        private static bool VectorEqual(TSVector2 vec1, TSVector2 vec2)
        {
            TSVector2 vector = vec2 - vec1;
            return (vector.LengthSquared() <= ClipperEpsilonSquared);
        }

        private sealed class Edge
        {
            [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private TSVector2 <EdgeEnd>k__BackingField;
            [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private TSVector2 <EdgeStart>k__BackingField;

            public Edge(TSVector2 edgeStart, TSVector2 edgeEnd)
            {
                this.EdgeStart = edgeStart;
                this.EdgeEnd = edgeEnd;
            }

            public override bool Equals(object obj)
            {
                if (obj == null)
                {
                    return false;
                }
                return this.Equals(obj as YuPengClipper.Edge);
            }

            public bool Equals(YuPengClipper.Edge e)
            {
                if (e == null)
                {
                    return false;
                }
                return (YuPengClipper.VectorEqual(this.EdgeStart, e.EdgeStart) && YuPengClipper.VectorEqual(this.EdgeEnd, e.EdgeEnd));
            }

            public TSVector2 GetCenter()
            {
                return (TSVector2) ((this.EdgeStart + this.EdgeEnd) / 2f);
            }

            public override int GetHashCode()
            {
                return (this.EdgeStart.GetHashCode() ^ this.EdgeEnd.GetHashCode());
            }

            public static YuPengClipper.Edge operator -(YuPengClipper.Edge e)
            {
                return new YuPengClipper.Edge(e.EdgeEnd, e.EdgeStart);
            }

            public TSVector2 EdgeEnd { get; private set; }

            public TSVector2 EdgeStart { get; private set; }
        }
    }
}

