using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public static class Triangulate
	{
		public static List<Vertices> ConvexPartition(Vertices vertices, TriangulationAlgorithm algorithm, bool discardAndFixInvalid, FP tolerance)
		{
			bool flag = vertices.Count <= 3;
			List<Vertices> result;
			if (flag)
			{
				result = new List<Vertices>
				{
					vertices
				};
			}
			else
			{
				List<Vertices> list;
				switch (algorithm)
				{
				case TriangulationAlgorithm.Earclip:
				{
					bool flag2 = vertices.IsCounterClockWise();
					if (flag2)
					{
						Vertices vertices2 = new Vertices(vertices);
						vertices2.Reverse();
						list = EarclipDecomposer.ConvexPartition(vertices2, tolerance);
					}
					else
					{
						list = EarclipDecomposer.ConvexPartition(vertices, tolerance);
					}
					break;
				}
				case TriangulationAlgorithm.Bayazit:
				{
					bool flag3 = !vertices.IsCounterClockWise();
					if (flag3)
					{
						Vertices vertices3 = new Vertices(vertices);
						vertices3.Reverse();
						list = BayazitDecomposer.ConvexPartition(vertices3);
					}
					else
					{
						list = BayazitDecomposer.ConvexPartition(vertices);
					}
					break;
				}
				case TriangulationAlgorithm.Flipcode:
				{
					bool flag4 = !vertices.IsCounterClockWise();
					if (flag4)
					{
						Vertices vertices4 = new Vertices(vertices);
						vertices4.Reverse();
						list = FlipcodeDecomposer.ConvexPartition(vertices4);
					}
					else
					{
						list = FlipcodeDecomposer.ConvexPartition(vertices);
					}
					break;
				}
				case TriangulationAlgorithm.Seidel:
					list = SeidelDecomposer.ConvexPartition(vertices, tolerance);
					break;
				case TriangulationAlgorithm.SeidelTrapezoids:
					list = SeidelDecomposer.ConvexPartitionTrapezoid(vertices, tolerance);
					break;
				case TriangulationAlgorithm.Delauny:
					list = CDTDecomposer.ConvexPartition(vertices);
					break;
				default:
					throw new ArgumentOutOfRangeException("algorithm");
				}
				if (discardAndFixInvalid)
				{
					for (int i = list.Count - 1; i >= 0; i--)
					{
						Vertices polygon = list[i];
						bool flag5 = !Triangulate.ValidatePolygon(polygon);
						if (flag5)
						{
							list.RemoveAt(i);
						}
					}
				}
				result = list;
			}
			return result;
		}

		private static bool ValidatePolygon(Vertices polygon)
		{
			PolygonError polygonError = polygon.CheckPolygon();
			bool flag = polygonError == PolygonError.InvalidAmountOfVertices || polygonError == PolygonError.AreaTooSmall || polygonError == PolygonError.SideTooSmall || polygonError == PolygonError.NotSimple;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = polygonError == PolygonError.NotCounterClockWise;
				if (flag2)
				{
					polygon.Reverse();
				}
				bool flag3 = polygonError == PolygonError.NotConvex;
				if (flag3)
				{
					polygon = GiftWrap.GetConvexHull(polygon);
					result = Triangulate.ValidatePolygon(polygon);
				}
				else
				{
					result = true;
				}
			}
			return result;
		}
	}
}
