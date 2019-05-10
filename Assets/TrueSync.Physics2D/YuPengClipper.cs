using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public static class YuPengClipper
	{
		private sealed class Edge
		{
			public TSVector2 EdgeStart
			{
				get;
				private set;
			}

			public TSVector2 EdgeEnd
			{
				get;
				private set;
			}

			public Edge(TSVector2 edgeStart, TSVector2 edgeEnd)
			{
				this.EdgeStart = edgeStart;
				this.EdgeEnd = edgeEnd;
			}

			public TSVector2 GetCenter()
			{
				return (this.EdgeStart + this.EdgeEnd) / 2f;
			}

			public static YuPengClipper.Edge operator -(YuPengClipper.Edge e)
			{
				return new YuPengClipper.Edge(e.EdgeEnd, e.EdgeStart);
			}

			public override bool Equals(object obj)
			{
				bool flag = obj == null;
				return !flag && this.Equals(obj as YuPengClipper.Edge);
			}

			public bool Equals(YuPengClipper.Edge e)
			{
				bool flag = e == null;
				return !flag && YuPengClipper.VectorEqual(this.EdgeStart, e.EdgeStart) && YuPengClipper.VectorEqual(this.EdgeEnd, e.EdgeEnd);
			}

			public override int GetHashCode()
			{
				return this.EdgeStart.GetHashCode() ^ this.EdgeEnd.GetHashCode();
			}
		}

		private static readonly FP ClipperEpsilonSquared = 1.1920929E-07f;

		public static List<Vertices> Union(Vertices polygon1, Vertices polygon2, out PolyClipError error)
		{
			return YuPengClipper.Execute(polygon1, polygon2, PolyClipType.Union, out error);
		}

		public static List<Vertices> Difference(Vertices polygon1, Vertices polygon2, out PolyClipError error)
		{
			return YuPengClipper.Execute(polygon1, polygon2, PolyClipType.Difference, out error);
		}

		public static List<Vertices> Intersect(Vertices polygon1, Vertices polygon2, out PolyClipError error)
		{
			return YuPengClipper.Execute(polygon1, polygon2, PolyClipType.Intersect, out error);
		}

		private static List<Vertices> Execute(Vertices subject, Vertices clip, PolyClipType clipType, out PolyClipError error)
		{
			Debug.Assert(subject.IsSimple() && clip.IsSimple(), "Non simple input!", "Input polygons must be simple (cannot intersect themselves).");
			Vertices vertices;
			Vertices vertices2;
			YuPengClipper.CalculateIntersections(subject, clip, out vertices, out vertices2);
			TSVector2 lowerBound = subject.GetAABB().LowerBound;
			TSVector2 lowerBound2 = clip.GetAABB().LowerBound;
			TSVector2 tSVector;
			TSVector2.Min(ref lowerBound, ref lowerBound2, out tSVector);
			tSVector = TSVector2.one - tSVector;
			bool flag = tSVector != TSVector2.zero;
			if (flag)
			{
				vertices.Translate(ref tSVector);
				vertices2.Translate(ref tSVector);
			}
			vertices.ForceCounterClockWise();
			vertices2.ForceCounterClockWise();
			List<FP> poly1Coeff;
			List<YuPengClipper.Edge> poly1Simplicies;
			YuPengClipper.CalculateSimplicalChain(vertices, out poly1Coeff, out poly1Simplicies);
			List<FP> poly2Coeff;
			List<YuPengClipper.Edge> poly2Simplicies;
			YuPengClipper.CalculateSimplicalChain(vertices2, out poly2Coeff, out poly2Simplicies);
			List<YuPengClipper.Edge> simplicies;
			YuPengClipper.CalculateResultChain(poly1Coeff, poly1Simplicies, poly2Coeff, poly2Simplicies, clipType, out simplicies);
			List<Vertices> list;
			error = YuPengClipper.BuildPolygonsFromChain(simplicies, out list);
			tSVector *= -1f;
			for (int i = 0; i < list.Count; i++)
			{
				list[i].Translate(ref tSVector);
				SimplifyTools.CollinearSimplify(list[i], FP.Zero);
			}
			return list;
		}

		private static void CalculateIntersections(Vertices polygon1, Vertices polygon2, out Vertices slicedPoly1, out Vertices slicedPoly2)
		{
			slicedPoly1 = new Vertices(polygon1);
			slicedPoly2 = new Vertices(polygon2);
			for (int i = 0; i < polygon1.Count; i++)
			{
				TSVector2 tSVector = polygon1[i];
				TSVector2 tSVector2 = polygon1[polygon1.NextIndex(i)];
				for (int j = 0; j < polygon2.Count; j++)
				{
					TSVector2 tSVector3 = polygon2[j];
					TSVector2 tSVector4 = polygon2[polygon2.NextIndex(j)];
					TSVector2 tSVector5;
					bool flag = LineTools.LineIntersect(tSVector, tSVector2, tSVector3, tSVector4, out tSVector5);
					if (flag)
					{
						FP alpha = YuPengClipper.GetAlpha(tSVector, tSVector2, tSVector5);
						bool flag2 = alpha > 0f && alpha < 1f;
						if (flag2)
						{
							int num = slicedPoly1.IndexOf(tSVector) + 1;
							while (num < slicedPoly1.Count && YuPengClipper.GetAlpha(tSVector, tSVector2, slicedPoly1[num]) <= alpha)
							{
								num++;
							}
							slicedPoly1.Insert(num, tSVector5);
						}
						alpha = YuPengClipper.GetAlpha(tSVector3, tSVector4, tSVector5);
						bool flag3 = alpha > 0f && alpha < 1f;
						if (flag3)
						{
							int num2 = slicedPoly2.IndexOf(tSVector3) + 1;
							while (num2 < slicedPoly2.Count && YuPengClipper.GetAlpha(tSVector3, tSVector4, slicedPoly2[num2]) <= alpha)
							{
								num2++;
							}
							slicedPoly2.Insert(num2, tSVector5);
						}
					}
				}
			}
			for (int k = 0; k < slicedPoly1.Count; k++)
			{
				int index = slicedPoly1.NextIndex(k);
				bool flag4 = (slicedPoly1[index] - slicedPoly1[k]).LengthSquared() <= YuPengClipper.ClipperEpsilonSquared;
				if (flag4)
				{
					slicedPoly1.RemoveAt(k);
					k--;
				}
			}
			for (int l = 0; l < slicedPoly2.Count; l++)
			{
				int index2 = slicedPoly2.NextIndex(l);
				bool flag5 = (slicedPoly2[index2] - slicedPoly2[l]).LengthSquared() <= YuPengClipper.ClipperEpsilonSquared;
				if (flag5)
				{
					slicedPoly2.RemoveAt(l);
					l--;
				}
			}
		}

		private static void CalculateSimplicalChain(Vertices poly, out List<FP> coeff, out List<YuPengClipper.Edge> simplicies)
		{
			simplicies = new List<YuPengClipper.Edge>();
			coeff = new List<FP>();
			for (int i = 0; i < poly.Count; i++)
			{
				simplicies.Add(new YuPengClipper.Edge(poly[i], poly[poly.NextIndex(i)]));
				coeff.Add(YuPengClipper.CalculateSimplexCoefficient(TSVector2.zero, poly[i], poly[poly.NextIndex(i)]));
			}
		}

		private static void CalculateResultChain(List<FP> poly1Coeff, List<YuPengClipper.Edge> poly1Simplicies, List<FP> poly2Coeff, List<YuPengClipper.Edge> poly2Simplicies, PolyClipType clipType, out List<YuPengClipper.Edge> resultSimplices)
		{
			resultSimplices = new List<YuPengClipper.Edge>();
			for (int i = 0; i < poly1Simplicies.Count; i++)
			{
				FP x = 0;
				bool flag = poly2Simplicies.Contains(poly1Simplicies[i]);
				if (flag)
				{
					x = 1f;
				}
				else
				{
					bool flag2 = poly2Simplicies.Contains(-poly1Simplicies[i]) && clipType == PolyClipType.Union;
					if (flag2)
					{
						x = 1f;
					}
					else
					{
						for (int j = 0; j < poly2Simplicies.Count; j++)
						{
							bool flag3 = !poly2Simplicies.Contains(-poly1Simplicies[i]);
							if (flag3)
							{
								x += YuPengClipper.CalculateBeta(poly1Simplicies[i].GetCenter(), poly2Simplicies[j], poly2Coeff[j]);
							}
						}
					}
				}
				bool flag4 = clipType == PolyClipType.Intersect;
				if (flag4)
				{
					bool flag5 = x == 1f;
					if (flag5)
					{
						resultSimplices.Add(poly1Simplicies[i]);
					}
				}
				else
				{
					bool flag6 = x == 0f;
					if (flag6)
					{
						resultSimplices.Add(poly1Simplicies[i]);
					}
				}
			}
			for (int k = 0; k < poly2Simplicies.Count; k++)
			{
				FP x2 = 0f;
				bool flag7 = !resultSimplices.Contains(poly2Simplicies[k]) && !resultSimplices.Contains(-poly2Simplicies[k]);
				if (flag7)
				{
					bool flag8 = poly1Simplicies.Contains(-poly2Simplicies[k]) && clipType == PolyClipType.Union;
					if (flag8)
					{
						x2 = 1f;
					}
					else
					{
						x2 = 0f;
						for (int l = 0; l < poly1Simplicies.Count; l++)
						{
							bool flag9 = !poly1Simplicies.Contains(poly2Simplicies[k]) && !poly1Simplicies.Contains(-poly2Simplicies[k]);
							if (flag9)
							{
								x2 += YuPengClipper.CalculateBeta(poly2Simplicies[k].GetCenter(), poly1Simplicies[l], poly1Coeff[l]);
							}
						}
						bool flag10 = clipType == PolyClipType.Intersect || clipType == PolyClipType.Difference;
						if (flag10)
						{
							bool flag11 = x2 == 1f;
							if (flag11)
							{
								resultSimplices.Add(-poly2Simplicies[k]);
							}
						}
						else
						{
							bool flag12 = x2 == 0f;
							if (flag12)
							{
								resultSimplices.Add(poly2Simplicies[k]);
							}
						}
					}
				}
			}
		}

		private static PolyClipError BuildPolygonsFromChain(List<YuPengClipper.Edge> simplicies, out List<Vertices> result)
		{
			result = new List<Vertices>();
			PolyClipError polyClipError = PolyClipError.None;
			PolyClipError result2;
			while (simplicies.Count > 0)
			{
				Vertices vertices = new Vertices();
				vertices.Add(simplicies[0].EdgeStart);
				vertices.Add(simplicies[0].EdgeEnd);
				simplicies.RemoveAt(0);
				bool flag = false;
				int num = 0;
				int count = simplicies.Count;
				while (!flag && simplicies.Count > 0)
				{
					bool flag2 = YuPengClipper.VectorEqual(vertices[vertices.Count - 1], simplicies[num].EdgeStart);
					if (flag2)
					{
						bool flag3 = YuPengClipper.VectorEqual(simplicies[num].EdgeEnd, vertices[0]);
						if (flag3)
						{
							flag = true;
						}
						else
						{
							vertices.Add(simplicies[num].EdgeEnd);
						}
						simplicies.RemoveAt(num);
						num--;
					}
					else
					{
						bool flag4 = YuPengClipper.VectorEqual(vertices[vertices.Count - 1], simplicies[num].EdgeEnd);
						if (flag4)
						{
							bool flag5 = YuPengClipper.VectorEqual(simplicies[num].EdgeStart, vertices[0]);
							if (flag5)
							{
								flag = true;
							}
							else
							{
								vertices.Add(simplicies[num].EdgeStart);
							}
							simplicies.RemoveAt(num);
							num--;
						}
					}
					bool flag6 = !flag;
					if (flag6)
					{
						bool flag7 = ++num == simplicies.Count;
						if (flag7)
						{
							bool flag8 = count == simplicies.Count;
							if (flag8)
							{
								result = new List<Vertices>();
								Debug.WriteLine("Undefined error while building result polygon(s).");
								result2 = PolyClipError.BrokenResult;
								return result2;
							}
							num = 0;
							count = simplicies.Count;
						}
					}
				}
				bool flag9 = vertices.Count < 3;
				if (flag9)
				{
					polyClipError = PolyClipError.DegeneratedOutput;
					Debug.WriteLine("Degenerated output polygon produced (vertices < 3).");
				}
				result.Add(vertices);
			}
			result2 = polyClipError;
			return result2;
		}

		private static FP CalculateBeta(TSVector2 point, YuPengClipper.Edge e, FP coefficient)
		{
			FP result = 0f;
			bool flag = YuPengClipper.PointInSimplex(point, e);
			if (flag)
			{
				result = coefficient;
			}
			bool flag2 = YuPengClipper.PointOnLineSegment(TSVector2.zero, e.EdgeStart, point) || YuPengClipper.PointOnLineSegment(TSVector2.zero, e.EdgeEnd, point);
			if (flag2)
			{
				result = 0.5f * coefficient;
			}
			return result;
		}

		private static FP GetAlpha(TSVector2 start, TSVector2 end, TSVector2 point)
		{
			return (point - start).LengthSquared() / (end - start).LengthSquared();
		}

		private static FP CalculateSimplexCoefficient(TSVector2 a, TSVector2 b, TSVector2 c)
		{
			FP x = MathUtils.Area(ref a, ref b, ref c);
			bool flag = x < 0f;
			FP result;
			if (flag)
			{
				result = -1f;
			}
			else
			{
				bool flag2 = x > 0f;
				if (flag2)
				{
					result = 1f;
				}
				else
				{
					result = 0f;
				}
			}
			return result;
		}

		private static bool PointInSimplex(TSVector2 point, YuPengClipper.Edge edge)
		{
			return new Vertices
			{
				TSVector2.zero,
				edge.EdgeStart,
				edge.EdgeEnd
			}.PointInPolygon(ref point) == 1;
		}

		private static bool PointOnLineSegment(TSVector2 start, TSVector2 end, TSVector2 point)
		{
			TSVector2 value = end - start;
			return MathUtils.Area(ref start, ref end, ref point) == 0f && TSVector2.Dot(point - start, value) >= 0f && TSVector2.Dot(point - end, value) <= 0f;
		}

		private static bool VectorEqual(TSVector2 vec1, TSVector2 vec2)
		{
			return (vec2 - vec1).LengthSquared() <= YuPengClipper.ClipperEpsilonSquared;
		}
	}
}
