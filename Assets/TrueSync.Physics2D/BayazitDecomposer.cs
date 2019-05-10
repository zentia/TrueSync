using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal static class BayazitDecomposer
	{
		public static List<Vertices> ConvexPartition(Vertices vertices)
		{
			Debug.Assert(vertices.Count > 3);
			Debug.Assert(vertices.IsCounterClockWise());
			return BayazitDecomposer.TriangulatePolygon(vertices);
		}

		private static List<Vertices> TriangulatePolygon(Vertices vertices)
		{
			List<Vertices> list = new List<Vertices>();
			TSVector2 value = default(TSVector2);
			TSVector2 value2 = default(TSVector2);
			int num = 0;
			int i = 0;
			List<Vertices> result;
			for (int j = 0; j < vertices.Count; j++)
			{
				bool flag = BayazitDecomposer.Reflex(j, vertices);
				if (flag)
				{
					FP y2;
					FP y = y2 = FP.MaxValue;
					for (int k = 0; k < vertices.Count; k++)
					{
						bool flag2 = BayazitDecomposer.Left(BayazitDecomposer.At(j - 1, vertices), BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(k, vertices)) && BayazitDecomposer.RightOn(BayazitDecomposer.At(j - 1, vertices), BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(k - 1, vertices));
						if (flag2)
						{
							TSVector2 tSVector = LineTools.LineIntersect(BayazitDecomposer.At(j - 1, vertices), BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(k, vertices), BayazitDecomposer.At(k - 1, vertices));
							bool flag3 = BayazitDecomposer.Right(BayazitDecomposer.At(j + 1, vertices), BayazitDecomposer.At(j, vertices), tSVector);
							if (flag3)
							{
								FP fP = BayazitDecomposer.SquareDist(BayazitDecomposer.At(j, vertices), tSVector);
								bool flag4 = fP < y2;
								if (flag4)
								{
									y2 = fP;
									value = tSVector;
									num = k;
								}
							}
						}
						bool flag5 = BayazitDecomposer.Left(BayazitDecomposer.At(j + 1, vertices), BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(k + 1, vertices)) && BayazitDecomposer.RightOn(BayazitDecomposer.At(j + 1, vertices), BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(k, vertices));
						if (flag5)
						{
							TSVector2 tSVector = LineTools.LineIntersect(BayazitDecomposer.At(j + 1, vertices), BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(k, vertices), BayazitDecomposer.At(k + 1, vertices));
							bool flag6 = BayazitDecomposer.Left(BayazitDecomposer.At(j - 1, vertices), BayazitDecomposer.At(j, vertices), tSVector);
							if (flag6)
							{
								FP fP = BayazitDecomposer.SquareDist(BayazitDecomposer.At(j, vertices), tSVector);
								bool flag7 = fP < y;
								if (flag7)
								{
									y = fP;
									i = k;
									value2 = tSVector;
								}
							}
						}
					}
					bool flag8 = num == (i + 1) % vertices.Count;
					Vertices vertices2;
					Vertices vertices3;
					if (flag8)
					{
						TSVector2 item = (value + value2) / 2;
						vertices2 = BayazitDecomposer.Copy(j, i, vertices);
						vertices2.Add(item);
						vertices3 = BayazitDecomposer.Copy(num, j, vertices);
						vertices3.Add(item);
					}
					else
					{
						FP y3 = 0;
						FP value3 = num;
						while (i < num)
						{
							i += vertices.Count;
						}
						for (int l = num; l <= i; l++)
						{
							bool flag9 = BayazitDecomposer.CanSee(j, l, vertices);
							if (flag9)
							{
								FP fP2 = 1 / (BayazitDecomposer.SquareDist(BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(l, vertices)) + 1);
								bool flag10 = BayazitDecomposer.Reflex(l, vertices);
								if (flag10)
								{
									bool flag11 = BayazitDecomposer.RightOn(BayazitDecomposer.At(l - 1, vertices), BayazitDecomposer.At(l, vertices), BayazitDecomposer.At(j, vertices)) && BayazitDecomposer.LeftOn(BayazitDecomposer.At(l + 1, vertices), BayazitDecomposer.At(l, vertices), BayazitDecomposer.At(j, vertices));
									if (flag11)
									{
										fP2 += 3;
									}
									else
									{
										fP2 += 2;
									}
								}
								else
								{
									fP2 += 1;
								}
								bool flag12 = fP2 > y3;
								if (flag12)
								{
									value3 = l;
									y3 = fP2;
								}
							}
						}
						vertices2 = BayazitDecomposer.Copy(j, (int)((long)value3), vertices);
						vertices3 = BayazitDecomposer.Copy((int)((long)value3), j, vertices);
					}
					list.AddRange(BayazitDecomposer.TriangulatePolygon(vertices2));
					list.AddRange(BayazitDecomposer.TriangulatePolygon(vertices3));
					result = list;
					return result;
				}
			}
			bool flag13 = vertices.Count > Settings.MaxPolygonVertices;
			if (flag13)
			{
				Vertices vertices2 = BayazitDecomposer.Copy(0, vertices.Count / 2, vertices);
				Vertices vertices3 = BayazitDecomposer.Copy(vertices.Count / 2, 0, vertices);
				list.AddRange(BayazitDecomposer.TriangulatePolygon(vertices2));
				list.AddRange(BayazitDecomposer.TriangulatePolygon(vertices3));
			}
			else
			{
				list.Add(vertices);
			}
			result = list;
			return result;
		}

		private static TSVector2 At(int i, Vertices vertices)
		{
			int count = vertices.Count;
			return vertices[(i < 0) ? (count - 1 - (-i - 1) % count) : (i % count)];
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
				vertices2.Add(BayazitDecomposer.At(i, vertices));
				i++;
			}
			return vertices2;
		}

		private static bool CanSee(int i, int j, Vertices vertices)
		{
			bool flag = BayazitDecomposer.Reflex(i, vertices);
			bool result;
			if (flag)
			{
				bool flag2 = BayazitDecomposer.LeftOn(BayazitDecomposer.At(i, vertices), BayazitDecomposer.At(i - 1, vertices), BayazitDecomposer.At(j, vertices)) && BayazitDecomposer.RightOn(BayazitDecomposer.At(i, vertices), BayazitDecomposer.At(i + 1, vertices), BayazitDecomposer.At(j, vertices));
				if (flag2)
				{
					result = false;
					return result;
				}
			}
			else
			{
				bool flag3 = BayazitDecomposer.RightOn(BayazitDecomposer.At(i, vertices), BayazitDecomposer.At(i + 1, vertices), BayazitDecomposer.At(j, vertices)) || BayazitDecomposer.LeftOn(BayazitDecomposer.At(i, vertices), BayazitDecomposer.At(i - 1, vertices), BayazitDecomposer.At(j, vertices));
				if (flag3)
				{
					result = false;
					return result;
				}
			}
			bool flag4 = BayazitDecomposer.Reflex(j, vertices);
			if (flag4)
			{
				bool flag5 = BayazitDecomposer.LeftOn(BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(j - 1, vertices), BayazitDecomposer.At(i, vertices)) && BayazitDecomposer.RightOn(BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(j + 1, vertices), BayazitDecomposer.At(i, vertices));
				if (flag5)
				{
					result = false;
					return result;
				}
			}
			else
			{
				bool flag6 = BayazitDecomposer.RightOn(BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(j + 1, vertices), BayazitDecomposer.At(i, vertices)) || BayazitDecomposer.LeftOn(BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(j - 1, vertices), BayazitDecomposer.At(i, vertices));
				if (flag6)
				{
					result = false;
					return result;
				}
			}
			for (int k = 0; k < vertices.Count; k++)
			{
				bool flag7 = (k + 1) % vertices.Count == i || k == i || (k + 1) % vertices.Count == j || k == j;
				if (!flag7)
				{
					TSVector2 tSVector;
					bool flag8 = LineTools.LineIntersect(BayazitDecomposer.At(i, vertices), BayazitDecomposer.At(j, vertices), BayazitDecomposer.At(k, vertices), BayazitDecomposer.At(k + 1, vertices), out tSVector);
					if (flag8)
					{
						result = false;
						return result;
					}
				}
			}
			result = true;
			return result;
		}

		private static bool Reflex(int i, Vertices vertices)
		{
			return BayazitDecomposer.Right(i, vertices);
		}

		private static bool Right(int i, Vertices vertices)
		{
			return BayazitDecomposer.Right(BayazitDecomposer.At(i - 1, vertices), BayazitDecomposer.At(i, vertices), BayazitDecomposer.At(i + 1, vertices));
		}

		private static bool Left(TSVector2 a, TSVector2 b, TSVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) > 0;
		}

		private static bool LeftOn(TSVector2 a, TSVector2 b, TSVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) >= 0;
		}

		private static bool Right(TSVector2 a, TSVector2 b, TSVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) < 0;
		}

		private static bool RightOn(TSVector2 a, TSVector2 b, TSVector2 c)
		{
			return MathUtils.Area(ref a, ref b, ref c) <= 0;
		}

		private static FP SquareDist(TSVector2 a, TSVector2 b)
		{
			FP fP = b.x - a.x;
			FP fP2 = b.y - a.y;
			return fP * fP + fP2 * fP2;
		}
	}
}
