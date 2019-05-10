using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal static class DTSweep
	{
		private static readonly FP PI_div2 = FP.Pi / 2;

		private static readonly FP PI_3div4 = 3 * FP.Pi / 4;

		public static void Triangulate(DTSweepContext tcx)
		{
			tcx.CreateAdvancingFront();
			DTSweep.Sweep(tcx);
			bool flag = tcx.TriangulationMode == TriangulationMode.Polygon;
			if (flag)
			{
				DTSweep.FinalizationPolygon(tcx);
			}
			else
			{
				DTSweep.FinalizationConvexHull(tcx);
			}
			tcx.Done();
		}

		private static void Sweep(DTSweepContext tcx)
		{
			List<TriangulationPoint> points = tcx.Points;
			for (int i = 1; i < points.Count; i++)
			{
				TriangulationPoint triangulationPoint = points[i];
				AdvancingFrontNode node = DTSweep.PointEvent(tcx, triangulationPoint);
				bool hasEdges = triangulationPoint.HasEdges;
				if (hasEdges)
				{
					foreach (DTSweepConstraint current in triangulationPoint.Edges)
					{
						DTSweep.EdgeEvent(tcx, current, node);
					}
				}
				tcx.Update(null);
			}
		}

		private static void FinalizationConvexHull(DTSweepContext tcx)
		{
			AdvancingFrontNode advancingFrontNode = tcx.aFront.Head.Next;
			AdvancingFrontNode advancingFrontNode2 = advancingFrontNode.Next;
			DTSweep.TurnAdvancingFrontConvex(tcx, advancingFrontNode, advancingFrontNode2);
			advancingFrontNode = tcx.aFront.Tail.Prev;
			bool flag = advancingFrontNode.Triangle.Contains(advancingFrontNode.Next.Point) && advancingFrontNode.Triangle.Contains(advancingFrontNode.Prev.Point);
			DelaunayTriangle delaunayTriangle;
			if (flag)
			{
				delaunayTriangle = advancingFrontNode.Triangle.NeighborAcross(advancingFrontNode.Point);
				DTSweep.RotateTrianglePair(advancingFrontNode.Triangle, advancingFrontNode.Point, delaunayTriangle, delaunayTriangle.OppositePoint(advancingFrontNode.Triangle, advancingFrontNode.Point));
				tcx.MapTriangleToNodes(advancingFrontNode.Triangle);
				tcx.MapTriangleToNodes(delaunayTriangle);
			}
			advancingFrontNode = tcx.aFront.Head.Next;
			bool flag2 = advancingFrontNode.Triangle.Contains(advancingFrontNode.Prev.Point) && advancingFrontNode.Triangle.Contains(advancingFrontNode.Next.Point);
			if (flag2)
			{
				delaunayTriangle = advancingFrontNode.Triangle.NeighborAcross(advancingFrontNode.Point);
				DTSweep.RotateTrianglePair(advancingFrontNode.Triangle, advancingFrontNode.Point, delaunayTriangle, delaunayTriangle.OppositePoint(advancingFrontNode.Triangle, advancingFrontNode.Point));
				tcx.MapTriangleToNodes(advancingFrontNode.Triangle);
				tcx.MapTriangleToNodes(delaunayTriangle);
			}
			TriangulationPoint point = tcx.aFront.Head.Point;
			advancingFrontNode2 = tcx.aFront.Tail.Prev;
			delaunayTriangle = advancingFrontNode2.Triangle;
			TriangulationPoint triangulationPoint = advancingFrontNode2.Point;
			advancingFrontNode2.Triangle = null;
			DelaunayTriangle delaunayTriangle2;
			while (true)
			{
				tcx.RemoveFromList(delaunayTriangle);
				triangulationPoint = delaunayTriangle.PointCCW(triangulationPoint);
				bool flag3 = triangulationPoint == point;
				if (flag3)
				{
					break;
				}
				delaunayTriangle2 = delaunayTriangle.NeighborCCW(triangulationPoint);
				delaunayTriangle.Clear();
				delaunayTriangle = delaunayTriangle2;
			}
			point = tcx.aFront.Head.Next.Point;
			triangulationPoint = delaunayTriangle.PointCW(tcx.aFront.Head.Point);
			delaunayTriangle2 = delaunayTriangle.NeighborCW(tcx.aFront.Head.Point);
			delaunayTriangle.Clear();
			delaunayTriangle = delaunayTriangle2;
			while (triangulationPoint != point)
			{
				tcx.RemoveFromList(delaunayTriangle);
				triangulationPoint = delaunayTriangle.PointCCW(triangulationPoint);
				delaunayTriangle2 = delaunayTriangle.NeighborCCW(triangulationPoint);
				delaunayTriangle.Clear();
				delaunayTriangle = delaunayTriangle2;
			}
			tcx.aFront.Head = tcx.aFront.Head.Next;
			tcx.aFront.Head.Prev = null;
			tcx.aFront.Tail = tcx.aFront.Tail.Prev;
			tcx.aFront.Tail.Next = null;
			tcx.FinalizeTriangulation();
		}

		private static void TurnAdvancingFrontConvex(DTSweepContext tcx, AdvancingFrontNode b, AdvancingFrontNode c)
		{
			AdvancingFrontNode advancingFrontNode = b;
			while (c != tcx.aFront.Tail)
			{
				bool flag = TriangulationUtil.Orient2d(b.Point, c.Point, c.Next.Point) == Orientation.CCW;
				if (flag)
				{
					DTSweep.Fill(tcx, c);
					c = c.Next;
				}
				else
				{
					bool flag2 = b != advancingFrontNode && TriangulationUtil.Orient2d(b.Prev.Point, b.Point, c.Point) == Orientation.CCW;
					if (flag2)
					{
						DTSweep.Fill(tcx, b);
						b = b.Prev;
					}
					else
					{
						b = c;
						c = c.Next;
					}
				}
			}
		}

		private static void FinalizationPolygon(DTSweepContext tcx)
		{
			DelaunayTriangle delaunayTriangle = tcx.aFront.Head.Next.Triangle;
			TriangulationPoint point = tcx.aFront.Head.Next.Point;
			while (!delaunayTriangle.GetConstrainedEdgeCW(point))
			{
				delaunayTriangle = delaunayTriangle.NeighborCCW(point);
			}
			tcx.MeshClean(delaunayTriangle);
		}

		private static AdvancingFrontNode PointEvent(DTSweepContext tcx, TriangulationPoint point)
		{
			AdvancingFrontNode advancingFrontNode = tcx.LocateNode(point);
			AdvancingFrontNode advancingFrontNode2 = DTSweep.NewFrontTriangle(tcx, point, advancingFrontNode);
			bool flag = point.X <= advancingFrontNode.Point.X + TriangulationUtil.EPSILON;
			if (flag)
			{
				DTSweep.Fill(tcx, advancingFrontNode);
			}
			tcx.AddNode(advancingFrontNode2);
			DTSweep.FillAdvancingFront(tcx, advancingFrontNode2);
			return advancingFrontNode2;
		}

		private static AdvancingFrontNode NewFrontTriangle(DTSweepContext tcx, TriangulationPoint point, AdvancingFrontNode node)
		{
			DelaunayTriangle delaunayTriangle = new DelaunayTriangle(point, node.Point, node.Next.Point);
			delaunayTriangle.MarkNeighbor(node.Triangle);
			tcx.Triangles.Add(delaunayTriangle);
			AdvancingFrontNode advancingFrontNode = new AdvancingFrontNode(point);
			advancingFrontNode.Next = node.Next;
			advancingFrontNode.Prev = node;
			node.Next.Prev = advancingFrontNode;
			node.Next = advancingFrontNode;
			tcx.AddNode(advancingFrontNode);
			bool flag = !DTSweep.Legalize(tcx, delaunayTriangle);
			if (flag)
			{
				tcx.MapTriangleToNodes(delaunayTriangle);
			}
			return advancingFrontNode;
		}

		private static void EdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			try
			{
				tcx.EdgeEvent.ConstrainedEdge = edge;
				tcx.EdgeEvent.Right = (edge.P.X > edge.Q.X);
				bool flag = DTSweep.IsEdgeSideOfTriangle(node.Triangle, edge.P, edge.Q);
				if (!flag)
				{
					DTSweep.FillEdgeEvent(tcx, edge, node);
					DTSweep.EdgeEvent(tcx, edge.P, edge.Q, node.Triangle, edge.Q);
				}
			}
			catch (PointOnEdgeException ex)
			{
				Debug.WriteLine("Skipping Edge: " + ex.Message);
			}
		}

		private static void FillEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			bool right = tcx.EdgeEvent.Right;
			if (right)
			{
				DTSweep.FillRightAboveEdgeEvent(tcx, edge, node);
			}
			else
			{
				DTSweep.FillLeftAboveEdgeEvent(tcx, edge, node);
			}
		}

		private static void FillRightConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			DTSweep.Fill(tcx, node.Next);
			bool flag = node.Next.Point != edge.P;
			if (flag)
			{
				bool flag2 = TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Orientation.CCW;
				if (flag2)
				{
					bool flag3 = TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW;
					if (flag3)
					{
						DTSweep.FillRightConcaveEdgeEvent(tcx, edge, node);
					}
				}
			}
		}

		private static void FillRightConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			bool flag = TriangulationUtil.Orient2d(node.Next.Point, node.Next.Next.Point, node.Next.Next.Next.Point) == Orientation.CCW;
			if (flag)
			{
				DTSweep.FillRightConcaveEdgeEvent(tcx, edge, node.Next);
			}
			else
			{
				bool flag2 = TriangulationUtil.Orient2d(edge.Q, node.Next.Next.Point, edge.P) == Orientation.CCW;
				if (flag2)
				{
					DTSweep.FillRightConvexEdgeEvent(tcx, edge, node.Next);
				}
			}
		}

		private static void FillRightBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			bool flag = node.Point.X < edge.P.X;
			if (flag)
			{
				bool flag2 = TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW;
				if (flag2)
				{
					DTSweep.FillRightConcaveEdgeEvent(tcx, edge, node);
				}
				else
				{
					DTSweep.FillRightConvexEdgeEvent(tcx, edge, node);
					DTSweep.FillRightBelowEdgeEvent(tcx, edge, node);
				}
			}
		}

		private static void FillRightAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			while (node.Next.Point.X < edge.P.X)
			{
				Orientation orientation = TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P);
				bool flag = orientation == Orientation.CCW;
				if (flag)
				{
					DTSweep.FillRightBelowEdgeEvent(tcx, edge, node);
				}
				else
				{
					node = node.Next;
				}
			}
		}

		private static void FillLeftConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			bool flag = TriangulationUtil.Orient2d(node.Prev.Point, node.Prev.Prev.Point, node.Prev.Prev.Prev.Point) == Orientation.CW;
			if (flag)
			{
				DTSweep.FillLeftConcaveEdgeEvent(tcx, edge, node.Prev);
			}
			else
			{
				bool flag2 = TriangulationUtil.Orient2d(edge.Q, node.Prev.Prev.Point, edge.P) == Orientation.CW;
				if (flag2)
				{
					DTSweep.FillLeftConvexEdgeEvent(tcx, edge, node.Prev);
				}
			}
		}

		private static void FillLeftConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			DTSweep.Fill(tcx, node.Prev);
			bool flag = node.Prev.Point != edge.P;
			if (flag)
			{
				bool flag2 = TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Orientation.CW;
				if (flag2)
				{
					bool flag3 = TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW;
					if (flag3)
					{
						DTSweep.FillLeftConcaveEdgeEvent(tcx, edge, node);
					}
				}
			}
		}

		private static void FillLeftBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			bool flag = node.Point.X > edge.P.X;
			if (flag)
			{
				bool flag2 = TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW;
				if (flag2)
				{
					DTSweep.FillLeftConcaveEdgeEvent(tcx, edge, node);
				}
				else
				{
					DTSweep.FillLeftConvexEdgeEvent(tcx, edge, node);
					DTSweep.FillLeftBelowEdgeEvent(tcx, edge, node);
				}
			}
		}

		private static void FillLeftAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
		{
			while (node.Prev.Point.X > edge.P.X)
			{
				Orientation orientation = TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P);
				bool flag = orientation == Orientation.CW;
				if (flag)
				{
					DTSweep.FillLeftBelowEdgeEvent(tcx, edge, node);
				}
				else
				{
					node = node.Prev;
				}
			}
		}

		private static bool IsEdgeSideOfTriangle(DelaunayTriangle triangle, TriangulationPoint ep, TriangulationPoint eq)
		{
			int num = triangle.EdgeIndex(ep, eq);
			bool flag = num != -1;
			bool result;
			if (flag)
			{
				triangle.MarkConstrainedEdge(num);
				triangle = triangle.Neighbors[num];
				bool flag2 = triangle != null;
				if (flag2)
				{
					triangle.MarkConstrainedEdge(ep, eq);
				}
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		private static void EdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle triangle, TriangulationPoint point)
		{
			bool flag = DTSweep.IsEdgeSideOfTriangle(triangle, ep, eq);
			if (!flag)
			{
				TriangulationPoint triangulationPoint = triangle.PointCCW(point);
				Orientation orientation = TriangulationUtil.Orient2d(eq, triangulationPoint, ep);
				bool flag2 = orientation == Orientation.Collinear;
				if (flag2)
				{
					bool flag3 = triangle.Contains(eq, triangulationPoint);
					if (!flag3)
					{
						throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet");
					}
					triangle.MarkConstrainedEdge(eq, triangulationPoint);
					tcx.EdgeEvent.ConstrainedEdge.Q = triangulationPoint;
					triangle = triangle.NeighborAcross(point);
					DTSweep.EdgeEvent(tcx, ep, triangulationPoint, triangle, triangulationPoint);
					bool isDebugEnabled = tcx.IsDebugEnabled;
					if (isDebugEnabled)
					{
						Debug.WriteLine("EdgeEvent - Point on constrained edge");
					}
				}
				else
				{
					TriangulationPoint triangulationPoint2 = triangle.PointCW(point);
					Orientation orientation2 = TriangulationUtil.Orient2d(eq, triangulationPoint2, ep);
					bool flag4 = orientation2 == Orientation.Collinear;
					if (flag4)
					{
						bool flag5 = triangle.Contains(eq, triangulationPoint2);
						if (!flag5)
						{
							throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet");
						}
						triangle.MarkConstrainedEdge(eq, triangulationPoint2);
						tcx.EdgeEvent.ConstrainedEdge.Q = triangulationPoint2;
						triangle = triangle.NeighborAcross(point);
						DTSweep.EdgeEvent(tcx, ep, triangulationPoint2, triangle, triangulationPoint2);
						bool isDebugEnabled2 = tcx.IsDebugEnabled;
						if (isDebugEnabled2)
						{
							Debug.WriteLine("EdgeEvent - Point on constrained edge");
						}
					}
					else
					{
						bool flag6 = orientation == orientation2;
						if (flag6)
						{
							bool flag7 = orientation == Orientation.CW;
							if (flag7)
							{
								triangle = triangle.NeighborCCW(point);
							}
							else
							{
								triangle = triangle.NeighborCW(point);
							}
							DTSweep.EdgeEvent(tcx, ep, eq, triangle, point);
						}
						else
						{
							DTSweep.FlipEdgeEvent(tcx, ep, eq, triangle, point);
						}
					}
				}
			}
		}

		private static void FlipEdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle t, TriangulationPoint p)
		{
			DelaunayTriangle delaunayTriangle = t.NeighborAcross(p);
			TriangulationPoint triangulationPoint = delaunayTriangle.OppositePoint(t, p);
			bool flag = delaunayTriangle == null;
			if (flag)
			{
				throw new InvalidOperationException("[BUG:FIXME] FLIP failed due to missing triangle");
			}
			bool constrainedEdgeAcross = t.GetConstrainedEdgeAcross(p);
			if (constrainedEdgeAcross)
			{
				throw new Exception("Intersecting Constraints");
			}
			bool flag2 = TriangulationUtil.InScanArea(p, t.PointCCW(p), t.PointCW(p), triangulationPoint);
			bool flag3 = flag2;
			if (flag3)
			{
				DTSweep.RotateTrianglePair(t, p, delaunayTriangle, triangulationPoint);
				tcx.MapTriangleToNodes(t);
				tcx.MapTriangleToNodes(delaunayTriangle);
				bool flag4 = p == eq && triangulationPoint == ep;
				if (flag4)
				{
					bool flag5 = eq == tcx.EdgeEvent.ConstrainedEdge.Q && ep == tcx.EdgeEvent.ConstrainedEdge.P;
					if (flag5)
					{
						bool isDebugEnabled = tcx.IsDebugEnabled;
						if (isDebugEnabled)
						{
							Console.WriteLine("[FLIP] - constrained edge done");
						}
						t.MarkConstrainedEdge(ep, eq);
						delaunayTriangle.MarkConstrainedEdge(ep, eq);
						DTSweep.Legalize(tcx, t);
						DTSweep.Legalize(tcx, delaunayTriangle);
					}
					else
					{
						bool isDebugEnabled2 = tcx.IsDebugEnabled;
						if (isDebugEnabled2)
						{
							Console.WriteLine("[FLIP] - subedge done");
						}
					}
				}
				else
				{
					bool isDebugEnabled3 = tcx.IsDebugEnabled;
					if (isDebugEnabled3)
					{
						Console.WriteLine("[FLIP] - flipping and continuing with triangle still crossing edge");
					}
					Orientation o = TriangulationUtil.Orient2d(eq, triangulationPoint, ep);
					t = DTSweep.NextFlipTriangle(tcx, o, t, delaunayTriangle, p, triangulationPoint);
					DTSweep.FlipEdgeEvent(tcx, ep, eq, t, p);
				}
			}
			else
			{
				TriangulationPoint p2 = DTSweep.NextFlipPoint(ep, eq, delaunayTriangle, triangulationPoint);
				DTSweep.FlipScanEdgeEvent(tcx, ep, eq, t, delaunayTriangle, p2);
				DTSweep.EdgeEvent(tcx, ep, eq, t, p);
			}
		}

		private static TriangulationPoint NextFlipPoint(TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle ot, TriangulationPoint op)
		{
			Orientation orientation = TriangulationUtil.Orient2d(eq, op, ep);
			bool flag = orientation == Orientation.CW;
			TriangulationPoint result;
			if (flag)
			{
				result = ot.PointCCW(op);
			}
			else
			{
				bool flag2 = orientation == Orientation.CCW;
				if (!flag2)
				{
					throw new PointOnEdgeException("Point on constrained edge not supported yet");
				}
				result = ot.PointCW(op);
			}
			return result;
		}

		private static DelaunayTriangle NextFlipTriangle(DTSweepContext tcx, Orientation o, DelaunayTriangle t, DelaunayTriangle ot, TriangulationPoint p, TriangulationPoint op)
		{
			bool flag = o == Orientation.CCW;
			DelaunayTriangle result;
			if (flag)
			{
				int index = ot.EdgeIndex(p, op);
				ot.EdgeIsDelaunay[index] = true;
				DTSweep.Legalize(tcx, ot);
				ot.EdgeIsDelaunay.Clear();
				result = t;
			}
			else
			{
				int index = t.EdgeIndex(p, op);
				t.EdgeIsDelaunay[index] = true;
				DTSweep.Legalize(tcx, t);
				t.EdgeIsDelaunay.Clear();
				result = ot;
			}
			return result;
		}

		private static void FlipScanEdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle flipTriangle, DelaunayTriangle t, TriangulationPoint p)
		{
			DelaunayTriangle delaunayTriangle = t.NeighborAcross(p);
			TriangulationPoint triangulationPoint = delaunayTriangle.OppositePoint(t, p);
			bool flag = delaunayTriangle == null;
			if (flag)
			{
				throw new Exception("[BUG:FIXME] FLIP failed due to missing triangle");
			}
			bool flag2 = TriangulationUtil.InScanArea(eq, flipTriangle.PointCCW(eq), flipTriangle.PointCW(eq), triangulationPoint);
			bool flag3 = flag2;
			if (flag3)
			{
				DTSweep.FlipEdgeEvent(tcx, eq, triangulationPoint, delaunayTriangle, triangulationPoint);
			}
			else
			{
				TriangulationPoint p2 = DTSweep.NextFlipPoint(ep, eq, delaunayTriangle, triangulationPoint);
				DTSweep.FlipScanEdgeEvent(tcx, ep, eq, flipTriangle, delaunayTriangle, p2);
			}
		}

		private static void FillAdvancingFront(DTSweepContext tcx, AdvancingFrontNode n)
		{
			AdvancingFrontNode advancingFrontNode = n.Next;
			while (advancingFrontNode.HasNext)
			{
				bool flag = DTSweep.LargeHole_DontFill(advancingFrontNode);
				if (flag)
				{
					break;
				}
				DTSweep.Fill(tcx, advancingFrontNode);
				advancingFrontNode = advancingFrontNode.Next;
			}
			advancingFrontNode = n.Prev;
			while (advancingFrontNode.HasPrev)
			{
				bool flag2 = DTSweep.LargeHole_DontFill(advancingFrontNode);
				if (flag2)
				{
					break;
				}
				FP x = DTSweep.HoleAngle(advancingFrontNode);
				bool flag3 = x > DTSweep.PI_div2 || x < -DTSweep.PI_div2;
				if (flag3)
				{
					break;
				}
				DTSweep.Fill(tcx, advancingFrontNode);
				advancingFrontNode = advancingFrontNode.Prev;
			}
			bool flag4 = n.HasNext && n.Next.HasNext;
			if (flag4)
			{
				FP x = DTSweep.BasinAngle(n);
				bool flag5 = x < DTSweep.PI_3div4;
				if (flag5)
				{
					DTSweep.FillBasin(tcx, n);
				}
			}
		}

		private static bool LargeHole_DontFill(AdvancingFrontNode node)
		{
			AdvancingFrontNode next = node.Next;
			AdvancingFrontNode prev = node.Prev;
			bool flag = !DTSweep.AngleExceeds90Degrees(node.Point, next.Point, prev.Point);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				AdvancingFrontNode next2 = next.Next;
				bool flag2 = next2 != null && !DTSweep.AngleExceedsPlus90DegreesOrIsNegative(node.Point, next2.Point, prev.Point);
				if (flag2)
				{
					result = false;
				}
				else
				{
					AdvancingFrontNode prev2 = prev.Prev;
					bool flag3 = prev2 != null && !DTSweep.AngleExceedsPlus90DegreesOrIsNegative(node.Point, next.Point, prev2.Point);
					result = !flag3;
				}
			}
			return result;
		}

		private static bool AngleExceeds90Degrees(TriangulationPoint origin, TriangulationPoint pa, TriangulationPoint pb)
		{
			FP x = DTSweep.Angle(origin, pa, pb);
			return x > DTSweep.PI_div2 || x < -DTSweep.PI_div2;
		}

		private static bool AngleExceedsPlus90DegreesOrIsNegative(TriangulationPoint origin, TriangulationPoint pa, TriangulationPoint pb)
		{
			FP x = DTSweep.Angle(origin, pa, pb);
			return x > DTSweep.PI_div2 || x < 0;
		}

		private static FP Angle(TriangulationPoint origin, TriangulationPoint pa, TriangulationPoint pb)
		{
			FP x = origin.X;
			FP y = origin.Y;
			FP x2 = pa.X - x;
			FP x3 = pa.Y - y;
			FP y2 = pb.X - x;
			FP y3 = pb.Y - y;
			FP y4 = x2 * y3 - x3 * y2;
			FP x4 = x2 * y2 + x3 * y3;
			return FP.Atan2(y4, x4);
		}

		private static void FillBasin(DTSweepContext tcx, AdvancingFrontNode node)
		{
			bool flag = TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW;
			if (flag)
			{
				tcx.Basin.leftNode = node;
			}
			else
			{
				tcx.Basin.leftNode = node.Next;
			}
			tcx.Basin.bottomNode = tcx.Basin.leftNode;
			while (tcx.Basin.bottomNode.HasNext && tcx.Basin.bottomNode.Point.Y >= tcx.Basin.bottomNode.Next.Point.Y)
			{
				tcx.Basin.bottomNode = tcx.Basin.bottomNode.Next;
			}
			bool flag2 = tcx.Basin.bottomNode == tcx.Basin.leftNode;
			if (!flag2)
			{
				tcx.Basin.rightNode = tcx.Basin.bottomNode;
				while (tcx.Basin.rightNode.HasNext && tcx.Basin.rightNode.Point.Y < tcx.Basin.rightNode.Next.Point.Y)
				{
					tcx.Basin.rightNode = tcx.Basin.rightNode.Next;
				}
				bool flag3 = tcx.Basin.rightNode == tcx.Basin.bottomNode;
				if (!flag3)
				{
					tcx.Basin.width = tcx.Basin.rightNode.Point.X - tcx.Basin.leftNode.Point.X;
					tcx.Basin.leftHighest = (tcx.Basin.leftNode.Point.Y > tcx.Basin.rightNode.Point.Y);
					DTSweep.FillBasinReq(tcx, tcx.Basin.bottomNode);
				}
			}
		}

		private static void FillBasinReq(DTSweepContext tcx, AdvancingFrontNode node)
		{
			bool flag = DTSweep.IsShallow(tcx, node);
			if (!flag)
			{
				DTSweep.Fill(tcx, node);
				bool flag2 = node.Prev == tcx.Basin.leftNode && node.Next == tcx.Basin.rightNode;
				if (!flag2)
				{
					bool flag3 = node.Prev == tcx.Basin.leftNode;
					if (flag3)
					{
						Orientation orientation = TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point);
						bool flag4 = orientation == Orientation.CW;
						if (flag4)
						{
							return;
						}
						node = node.Next;
					}
					else
					{
						bool flag5 = node.Next == tcx.Basin.rightNode;
						if (flag5)
						{
							Orientation orientation2 = TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point);
							bool flag6 = orientation2 == Orientation.CCW;
							if (flag6)
							{
								return;
							}
							node = node.Prev;
						}
						else
						{
							bool flag7 = node.Prev.Point.Y < node.Next.Point.Y;
							if (flag7)
							{
								node = node.Prev;
							}
							else
							{
								node = node.Next;
							}
						}
					}
					DTSweep.FillBasinReq(tcx, node);
				}
			}
		}

		private static bool IsShallow(DTSweepContext tcx, AdvancingFrontNode node)
		{
			bool leftHighest = tcx.Basin.leftHighest;
			FP y;
			if (leftHighest)
			{
				y = tcx.Basin.leftNode.Point.Y - node.Point.Y;
			}
			else
			{
				y = tcx.Basin.rightNode.Point.Y - node.Point.Y;
			}
			return tcx.Basin.width > y;
		}

		private static FP HoleAngle(AdvancingFrontNode node)
		{
			FP x = node.Point.X;
			FP y = node.Point.Y;
			FP x2 = node.Next.Point.X - x;
			FP x3 = node.Next.Point.Y - y;
			FP y2 = node.Prev.Point.X - x;
			FP y3 = node.Prev.Point.Y - y;
			return FP.Atan2(x2 * y3 - x3 * y2, x2 * y2 + x3 * y3);
		}

		private static FP BasinAngle(AdvancingFrontNode node)
		{
			FP x = node.Point.X - node.Next.Next.Point.X;
			FP y = node.Point.Y - node.Next.Next.Point.Y;
			return FP.Atan2(y, x);
		}

		private static void Fill(DTSweepContext tcx, AdvancingFrontNode node)
		{
			DelaunayTriangle delaunayTriangle = new DelaunayTriangle(node.Prev.Point, node.Point, node.Next.Point);
			delaunayTriangle.MarkNeighbor(node.Prev.Triangle);
			delaunayTriangle.MarkNeighbor(node.Triangle);
			tcx.Triangles.Add(delaunayTriangle);
			node.Prev.Next = node.Next;
			node.Next.Prev = node.Prev;
			tcx.RemoveNode(node);
			bool flag = !DTSweep.Legalize(tcx, delaunayTriangle);
			if (flag)
			{
				tcx.MapTriangleToNodes(delaunayTriangle);
			}
		}

		private static bool Legalize(DTSweepContext tcx, DelaunayTriangle t)
		{
			bool result;
			for (int i = 0; i < 3; i++)
			{
				bool flag = t.EdgeIsDelaunay[i];
				if (!flag)
				{
					DelaunayTriangle delaunayTriangle = t.Neighbors[i];
					bool flag2 = delaunayTriangle != null;
					if (flag2)
					{
						TriangulationPoint triangulationPoint = t.Points[i];
						TriangulationPoint triangulationPoint2 = delaunayTriangle.OppositePoint(t, triangulationPoint);
						int index = delaunayTriangle.IndexOf(triangulationPoint2);
						bool flag3 = delaunayTriangle.EdgeIsConstrained[index] || delaunayTriangle.EdgeIsDelaunay[index];
						if (flag3)
						{
							t.EdgeIsConstrained[i] = delaunayTriangle.EdgeIsConstrained[index];
						}
						else
						{
							bool flag4 = TriangulationUtil.SmartIncircle(triangulationPoint, t.PointCCW(triangulationPoint), t.PointCW(triangulationPoint), triangulationPoint2);
							bool flag5 = flag4;
							if (flag5)
							{
								t.EdgeIsDelaunay[i] = true;
								delaunayTriangle.EdgeIsDelaunay[index] = true;
								DTSweep.RotateTrianglePair(t, triangulationPoint, delaunayTriangle, triangulationPoint2);
								bool flag6 = !DTSweep.Legalize(tcx, t);
								bool flag7 = flag6;
								if (flag7)
								{
									tcx.MapTriangleToNodes(t);
								}
								flag6 = !DTSweep.Legalize(tcx, delaunayTriangle);
								bool flag8 = flag6;
								if (flag8)
								{
									tcx.MapTriangleToNodes(delaunayTriangle);
								}
								t.EdgeIsDelaunay[i] = false;
								delaunayTriangle.EdgeIsDelaunay[index] = false;
								result = true;
								return result;
							}
						}
					}
				}
			}
			result = false;
			return result;
		}

		private static void RotateTrianglePair(DelaunayTriangle t, TriangulationPoint p, DelaunayTriangle ot, TriangulationPoint op)
		{
			DelaunayTriangle delaunayTriangle = t.NeighborCCW(p);
			DelaunayTriangle delaunayTriangle2 = t.NeighborCW(p);
			DelaunayTriangle delaunayTriangle3 = ot.NeighborCCW(op);
			DelaunayTriangle delaunayTriangle4 = ot.NeighborCW(op);
			bool constrainedEdgeCCW = t.GetConstrainedEdgeCCW(p);
			bool constrainedEdgeCW = t.GetConstrainedEdgeCW(p);
			bool constrainedEdgeCCW2 = ot.GetConstrainedEdgeCCW(op);
			bool constrainedEdgeCW2 = ot.GetConstrainedEdgeCW(op);
			bool delaunayEdgeCCW = t.GetDelaunayEdgeCCW(p);
			bool delaunayEdgeCW = t.GetDelaunayEdgeCW(p);
			bool delaunayEdgeCCW2 = ot.GetDelaunayEdgeCCW(op);
			bool delaunayEdgeCW2 = ot.GetDelaunayEdgeCW(op);
			t.Legalize(p, op);
			ot.Legalize(op, p);
			ot.SetDelaunayEdgeCCW(p, delaunayEdgeCCW);
			t.SetDelaunayEdgeCW(p, delaunayEdgeCW);
			t.SetDelaunayEdgeCCW(op, delaunayEdgeCCW2);
			ot.SetDelaunayEdgeCW(op, delaunayEdgeCW2);
			ot.SetConstrainedEdgeCCW(p, constrainedEdgeCCW);
			t.SetConstrainedEdgeCW(p, constrainedEdgeCW);
			t.SetConstrainedEdgeCCW(op, constrainedEdgeCCW2);
			ot.SetConstrainedEdgeCW(op, constrainedEdgeCW2);
			t.Neighbors.Clear();
			ot.Neighbors.Clear();
			bool flag = delaunayTriangle != null;
			if (flag)
			{
				ot.MarkNeighbor(delaunayTriangle);
			}
			bool flag2 = delaunayTriangle2 != null;
			if (flag2)
			{
				t.MarkNeighbor(delaunayTriangle2);
			}
			bool flag3 = delaunayTriangle3 != null;
			if (flag3)
			{
				t.MarkNeighbor(delaunayTriangle3);
			}
			bool flag4 = delaunayTriangle4 != null;
			if (flag4)
			{
				ot.MarkNeighbor(delaunayTriangle4);
			}
			t.MarkNeighbor(ot);
		}
	}
}
