using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal static class SeidelDecomposer
	{
		public static List<Vertices> ConvexPartition(Vertices vertices, FP sheer)
		{
			Debug.Assert(vertices.Count > 3);
			List<Point> list = new List<Point>(vertices.Count);
			foreach (TSVector2 current in vertices)
			{
				list.Add(new Point(current.x, current.y));
			}
			Triangulator triangulator = new Triangulator(list, sheer);
			List<Vertices> list2 = new List<Vertices>();
			foreach (List<Point> current2 in triangulator.Triangles)
			{
				Vertices vertices2 = new Vertices(current2.Count);
				foreach (Point current3 in current2)
				{
					vertices2.Add(new TSVector2(current3.X, current3.Y));
				}
				list2.Add(vertices2);
			}
			return list2;
		}

		public static List<Vertices> ConvexPartitionTrapezoid(Vertices vertices, FP sheer)
		{
			List<Point> list = new List<Point>(vertices.Count);
			foreach (TSVector2 current in vertices)
			{
				list.Add(new Point(current.x, current.y));
			}
			Triangulator triangulator = new Triangulator(list, sheer);
			List<Vertices> list2 = new List<Vertices>();
			foreach (Trapezoid current2 in triangulator.Trapezoids)
			{
				Vertices vertices2 = new Vertices();
				List<Point> vertices3 = current2.GetVertices();
				foreach (Point current3 in vertices3)
				{
					vertices2.Add(new TSVector2(current3.X, current3.Y));
				}
				list2.Add(vertices2);
			}
			return list2;
		}
	}
}
