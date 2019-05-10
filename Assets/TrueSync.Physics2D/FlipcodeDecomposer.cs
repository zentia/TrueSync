using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal static class FlipcodeDecomposer
	{
		private static TSVector2 _tmpA;

		private static TSVector2 _tmpB;

		private static TSVector2 _tmpC;

		public static List<Vertices> ConvexPartition(Vertices vertices)
		{
			Debug.Assert(vertices.Count > 3);
			Debug.Assert(vertices.IsCounterClockWise());
			int[] array = new int[vertices.Count];
			for (int i = 0; i < vertices.Count; i++)
			{
				array[i] = i;
			}
			int j = vertices.Count;
			int num = 2 * j;
			List<Vertices> list = new List<Vertices>();
			int num2 = j - 1;
			List<Vertices> result;
			while (j > 2)
			{
				bool flag = 0 >= num--;
				if (flag)
				{
					result = new List<Vertices>();
					return result;
				}
				int num3 = num2;
				bool flag2 = j <= num3;
				if (flag2)
				{
					num3 = 0;
				}
				num2 = num3 + 1;
				bool flag3 = j <= num2;
				if (flag3)
				{
					num2 = 0;
				}
				int num4 = num2 + 1;
				bool flag4 = j <= num4;
				if (flag4)
				{
					num4 = 0;
				}
				FlipcodeDecomposer._tmpA = vertices[array[num3]];
				FlipcodeDecomposer._tmpB = vertices[array[num2]];
				FlipcodeDecomposer._tmpC = vertices[array[num4]];
				bool flag5 = FlipcodeDecomposer.Snip(vertices, num3, num2, num4, j, array);
				if (flag5)
				{
					list.Add(new Vertices(3)
					{
						FlipcodeDecomposer._tmpA,
						FlipcodeDecomposer._tmpB,
						FlipcodeDecomposer._tmpC
					});
					int num5 = num2;
					for (int k = num2 + 1; k < j; k++)
					{
						array[num5] = array[k];
						num5++;
					}
					j--;
					num = 2 * j;
				}
			}
			result = list;
			return result;
		}

		private static bool InsideTriangle(ref TSVector2 a, ref TSVector2 b, ref TSVector2 c, ref TSVector2 p)
		{
			FP x = (c.x - b.x) * (p.y - b.y) - (c.y - b.y) * (p.x - b.x);
			FP x2 = (b.x - a.x) * (p.y - a.y) - (b.y - a.y) * (p.x - a.x);
			FP x3 = (a.x - c.x) * (p.y - c.y) - (a.y - c.y) * (p.x - c.x);
			return x >= 0f && x3 >= 0f && x2 >= 0f;
		}

		private static bool Snip(Vertices contour, int u, int v, int w, int n, int[] V)
		{
			bool flag = Settings.Epsilon > MathUtils.Area(ref FlipcodeDecomposer._tmpA, ref FlipcodeDecomposer._tmpB, ref FlipcodeDecomposer._tmpC);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < n; i++)
				{
					bool flag2 = i == u || i == v || i == w;
					if (!flag2)
					{
						TSVector2 tSVector = contour[V[i]];
						bool flag3 = FlipcodeDecomposer.InsideTriangle(ref FlipcodeDecomposer._tmpA, ref FlipcodeDecomposer._tmpB, ref FlipcodeDecomposer._tmpC, ref tSVector);
						if (flag3)
						{
							result = false;
							return result;
						}
					}
				}
				result = true;
			}
			return result;
		}
	}
}
