using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public static class ChainHull
	{
		private class PointComparer : Comparer<TSVector2>
		{
			public override int Compare(TSVector2 a, TSVector2 b)
			{
				int num = a.x.CompareTo(b.x);
				return (num != 0) ? num : a.y.CompareTo(b.y);
			}
		}

		private static ChainHull.PointComparer _pointComparer = new ChainHull.PointComparer();

		public static Vertices GetConvexHull(Vertices vertices)
		{
			bool flag = vertices.Count <= 3;
			Vertices result;
			if (flag)
			{
				result = vertices;
			}
			else
			{
				Vertices vertices2 = new Vertices(vertices);
				vertices2.Sort(ChainHull._pointComparer);
				TSVector2[] array = new TSVector2[vertices2.Count];
				int i = -1;
				FP x = vertices2[0].x;
				int j;
				for (j = 1; j < vertices2.Count; j++)
				{
					bool flag2 = vertices2[j].x != x;
					if (flag2)
					{
						break;
					}
				}
				int num = j - 1;
				bool flag3 = num == vertices2.Count - 1;
				if (flag3)
				{
					array[++i] = vertices2[0];
					bool flag4 = vertices2[num].y != vertices2[0].y;
					if (flag4)
					{
						array[++i] = vertices2[num];
					}
					array[++i] = vertices2[0];
					Vertices vertices3 = new Vertices(i + 1);
					for (int k = 0; k < i + 1; k++)
					{
						vertices3.Add(array[k]);
					}
					result = vertices3;
				}
				else
				{
					i = -1;
					int num2 = vertices2.Count - 1;
					FP x2 = vertices2[vertices2.Count - 1].x;
					for (j = vertices2.Count - 2; j >= 0; j--)
					{
						bool flag5 = vertices2[j].x != x2;
						if (flag5)
						{
							break;
						}
					}
					int num3 = j + 1;
					array[++i] = vertices2[0];
					j = num;
					while (++j <= num3)
					{
						bool flag6 = MathUtils.Area(vertices2[0], vertices2[num3], vertices2[j]) >= 0 && j < num3;
						if (!flag6)
						{
							while (i > 0)
							{
								bool flag7 = MathUtils.Area(array[i - 1], array[i], vertices2[j]) > 0;
								if (flag7)
								{
									break;
								}
								i--;
							}
							array[++i] = vertices2[j];
						}
					}
					bool flag8 = num2 != num3;
					if (flag8)
					{
						array[++i] = vertices2[num2];
					}
					int num4 = i;
					j = num3;
					while (--j >= num)
					{
						bool flag9 = MathUtils.Area(vertices2[num2], vertices2[num], vertices2[j]) >= 0 && j > num;
						if (!flag9)
						{
							while (i > num4)
							{
								bool flag10 = MathUtils.Area(array[i - 1], array[i], vertices2[j]) > 0;
								if (flag10)
								{
									break;
								}
								i--;
							}
							array[++i] = vertices2[j];
						}
					}
					bool flag11 = num != 0;
					if (flag11)
					{
						array[++i] = vertices2[0];
					}
					Vertices vertices3 = new Vertices(i + 1);
					for (int l = 0; l < i + 1; l++)
					{
						vertices3.Add(array[l]);
					}
					result = vertices3;
				}
			}
			return result;
		}
	}
}
