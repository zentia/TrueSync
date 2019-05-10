using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class PointGenerator
	{
		private static readonly TSRandom RNG = TSRandom.New(0);

		public static List<TriangulationPoint> UniformDistribution(int n, FP scale)
		{
			List<TriangulationPoint> list = new List<TriangulationPoint>();
			for (int i = 0; i < n; i++)
			{
				list.Add(new TriangulationPoint(scale * (0.5 - PointGenerator.RNG.NextFP()), scale * (0.5 - PointGenerator.RNG.NextFP())));
			}
			return list;
		}

		public static List<TriangulationPoint> UniformGrid(int n, FP scale)
		{
			FP x = 0;
			FP y = scale / n;
			FP x2 = 0.5 * scale;
			List<TriangulationPoint> list = new List<TriangulationPoint>();
			for (int i = 0; i < n + 1; i++)
			{
				x = x2 - i * y;
				for (int j = 0; j < n + 1; j++)
				{
					list.Add(new TriangulationPoint(x, x2 - j * y));
				}
			}
			return list;
		}
	}
}
