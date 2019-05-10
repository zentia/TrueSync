using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal static class CDTDecomposer
	{
		public static List<Vertices> ConvexPartition(Vertices vertices)
		{
			Debug.Assert(vertices.Count > 3);
			Polygon polygon = new Polygon();
			foreach (TSVector2 current in vertices)
			{
				polygon.Points.Add(new TriangulationPoint(current.x, current.y));
			}
			bool flag = vertices.Holes != null;
			if (flag)
			{
				foreach (Vertices current2 in vertices.Holes)
				{
					Polygon polygon2 = new Polygon();
					foreach (TSVector2 current3 in current2)
					{
						polygon2.Points.Add(new TriangulationPoint(current3.x, current3.y));
					}
					polygon.AddHole(polygon2);
				}
			}
			DTSweepContext dTSweepContext = new DTSweepContext();
			dTSweepContext.PrepareTriangulation(polygon);
			DTSweep.Triangulate(dTSweepContext);
			List<Vertices> list = new List<Vertices>();
			foreach (DelaunayTriangle current4 in polygon.Triangles)
			{
				Vertices vertices2 = new Vertices();
				foreach (TriangulationPoint current5 in current4.Points)
				{
					vertices2.Add(new TSVector2(current5.X, current5.Y));
				}
				list.Add(vertices2);
			}
			return list;
		}
	}
}
