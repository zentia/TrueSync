using System;
using System.Collections.Generic;

namespace TrueSync
{
	public static class TSConvexHull
	{
		public enum Approximation
		{
			Level1 = 6,
			Level2,
			Level3,
			Level4,
			Level5,
			Level6,
			Level7,
			Level8,
			Level9 = 15,
			Level10 = 20,
			Level15 = 25,
			Level20 = 30
		}

		public static int[] Build(List<TSVector> pointCloud, TSConvexHull.Approximation factor)
		{
			List<int> list = new List<int>();
			for (int i = 0; i < (int)factor; i++)
			{
				FP x = TSMath.Pi / (factor - (TSConvexHull.Approximation)1) * i;
				FP x2 = FP.Sin(x);
				FP y = FP.Cos(x);
				for (int j = 0; j < (int)factor; j++)
				{
					FP x3 = 2 * FP.One * TSMath.Pi / (int)factor * j - TSMath.Pi;
					FP y2 = FP.Sin(x3);
					FP y3 = FP.Cos(x3);
					TSVector tSVector = new TSVector(x2 * y3, y, x2 * y2);
					int item = TSConvexHull.FindExtremePoint(pointCloud, ref tSVector);
					list.Add(item);
				}
			}
			list.Sort();
			for (int k = 1; k < list.Count; k++)
			{
				bool flag = list[k - 1] == list[k];
				if (flag)
				{
					list.RemoveAt(k - 1);
					k--;
				}
			}
			return list.ToArray();
		}

		private static int FindExtremePoint(List<TSVector> points, ref TSVector dir)
		{
			int result = 0;
			FP y = FP.MinValue;
			for (int i = 1; i < points.Count; i++)
			{
				TSVector tSVector = points[i];
				FP fP = TSVector.Dot(ref tSVector, ref dir);
				bool flag = fP > y;
				if (flag)
				{
					y = fP;
					result = i;
				}
			}
			return result;
		}
	}
}
