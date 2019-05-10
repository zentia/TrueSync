using System;

namespace TrueSync.Physics2D
{
	internal class DTSweepContext : TriangulationContext
	{
		public class DTSweepBasin
		{
			public AdvancingFrontNode bottomNode;

			public bool leftHighest;

			public AdvancingFrontNode leftNode;

			public AdvancingFrontNode rightNode;

			public FP width;
		}

		public class DTSweepEdgeEvent
		{
			public DTSweepConstraint ConstrainedEdge;

			public bool Right;
		}

		private static readonly FP ALPHA = 0.3f;

		public DTSweepContext.DTSweepBasin Basin = new DTSweepContext.DTSweepBasin();

		public DTSweepContext.DTSweepEdgeEvent EdgeEvent = new DTSweepContext.DTSweepEdgeEvent();

		private DTSweepPointComparator _comparator = new DTSweepPointComparator();

		public AdvancingFront aFront;

		public TriangulationPoint Head
		{
			get;
			set;
		}

		public TriangulationPoint Tail
		{
			get;
			set;
		}

		public DTSweepContext()
		{
			this.Clear();
		}

		public void RemoveFromList(DelaunayTriangle triangle)
		{
			this.Triangles.Remove(triangle);
		}

		public void MeshClean(DelaunayTriangle triangle)
		{
			this.MeshCleanReq(triangle);
		}

		private void MeshCleanReq(DelaunayTriangle triangle)
		{
			bool flag = triangle != null && !triangle.IsInterior;
			if (flag)
			{
				triangle.IsInterior = true;
				base.Triangulatable.AddTriangle(triangle);
				for (int i = 0; i < 3; i++)
				{
					bool flag2 = !triangle.EdgeIsConstrained[i];
					if (flag2)
					{
						this.MeshCleanReq(triangle.Neighbors[i]);
					}
				}
			}
		}

		public override void Clear()
		{
			base.Clear();
			this.Triangles.Clear();
		}

		public void AddNode(AdvancingFrontNode node)
		{
			this.aFront.AddNode(node);
		}

		public void RemoveNode(AdvancingFrontNode node)
		{
			this.aFront.RemoveNode(node);
		}

		public AdvancingFrontNode LocateNode(TriangulationPoint point)
		{
			return this.aFront.LocateNode(point);
		}

		public void CreateAdvancingFront()
		{
			DelaunayTriangle delaunayTriangle = new DelaunayTriangle(this.Points[0], this.Tail, this.Head);
			this.Triangles.Add(delaunayTriangle);
			AdvancingFrontNode advancingFrontNode = new AdvancingFrontNode(delaunayTriangle.Points[1]);
			advancingFrontNode.Triangle = delaunayTriangle;
			AdvancingFrontNode advancingFrontNode2 = new AdvancingFrontNode(delaunayTriangle.Points[0]);
			advancingFrontNode2.Triangle = delaunayTriangle;
			AdvancingFrontNode tail = new AdvancingFrontNode(delaunayTriangle.Points[2]);
			this.aFront = new AdvancingFront(advancingFrontNode, tail);
			this.aFront.AddNode(advancingFrontNode2);
			this.aFront.Head.Next = advancingFrontNode2;
			advancingFrontNode2.Next = this.aFront.Tail;
			advancingFrontNode2.Prev = this.aFront.Head;
			this.aFront.Tail.Prev = advancingFrontNode2;
		}

		public void MapTriangleToNodes(DelaunayTriangle t)
		{
			for (int i = 0; i < 3; i++)
			{
				bool flag = t.Neighbors[i] == null;
				if (flag)
				{
					AdvancingFrontNode advancingFrontNode = this.aFront.LocatePoint(t.PointCW(t.Points[i]));
					bool flag2 = advancingFrontNode != null;
					if (flag2)
					{
						advancingFrontNode.Triangle = t;
					}
				}
			}
		}

		public override void PrepareTriangulation(Triangulatable t)
		{
			base.PrepareTriangulation(t);
			FP x;
			FP fP = x = this.Points[0].X;
			FP y;
			FP fP2 = y = this.Points[0].Y;
			foreach (TriangulationPoint current in this.Points)
			{
				bool flag = current.X > x;
				if (flag)
				{
					x = current.X;
				}
				bool flag2 = current.X < fP;
				if (flag2)
				{
					fP = current.X;
				}
				bool flag3 = current.Y > y;
				if (flag3)
				{
					y = current.Y;
				}
				bool flag4 = current.Y < fP2;
				if (flag4)
				{
					fP2 = current.Y;
				}
			}
			FP y2 = DTSweepContext.ALPHA * (x - fP);
			FP y3 = DTSweepContext.ALPHA * (y - fP2);
			TriangulationPoint head = new TriangulationPoint(x + y2, fP2 - y3);
			TriangulationPoint tail = new TriangulationPoint(fP - y2, fP2 - y3);
			this.Head = head;
			this.Tail = tail;
			this.Points.Sort(this._comparator);
		}

		public void FinalizeTriangulation()
		{
			base.Triangulatable.AddTriangles(this.Triangles);
			this.Triangles.Clear();
		}

		public override TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b)
		{
			return new DTSweepConstraint(a, b);
		}
	}
}
