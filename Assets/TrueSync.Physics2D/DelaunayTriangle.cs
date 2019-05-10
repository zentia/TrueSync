using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal class DelaunayTriangle
	{
		public FixedBitArray3 EdgeIsConstrained;

		public FixedBitArray3 EdgeIsDelaunay;

		public FixedArray3Enum<DelaunayTriangle> Neighbors;

		public FixedArray3Enum<TriangulationPoint> Points;

		public bool IsInterior
		{
			get;
			set;
		}

		public DelaunayTriangle(TriangulationPoint p1, TriangulationPoint p2, TriangulationPoint p3)
		{
			this.Points[0] = p1;
			this.Points[1] = p2;
			this.Points[2] = p3;
		}

		public int IndexOf(TriangulationPoint p)
		{
			int num = this.Points.IndexOf(p);
			bool flag = num == -1;
			if (flag)
			{
				throw new Exception("Calling index with a point that doesn't exist in triangle");
			}
			return num;
		}

		public int IndexCW(TriangulationPoint p)
		{
			int num = this.IndexOf(p);
			int num2 = num;
			int result;
			if (num2 != 0)
			{
				if (num2 != 1)
				{
					result = 1;
				}
				else
				{
					result = 0;
				}
			}
			else
			{
				result = 2;
			}
			return result;
		}

		public int IndexCCW(TriangulationPoint p)
		{
			int num = this.IndexOf(p);
			int num2 = num;
			int result;
			if (num2 != 0)
			{
				if (num2 != 1)
				{
					result = 0;
				}
				else
				{
					result = 2;
				}
			}
			else
			{
				result = 1;
			}
			return result;
		}

		public bool Contains(TriangulationPoint p)
		{
			return p == this.Points[0] || p == this.Points[1] || p == this.Points[2];
		}

		public bool Contains(DTSweepConstraint e)
		{
			return this.Contains(e.P) && this.Contains(e.Q);
		}

		public bool Contains(TriangulationPoint p, TriangulationPoint q)
		{
			return this.Contains(p) && this.Contains(q);
		}

		private void MarkNeighbor(TriangulationPoint p1, TriangulationPoint p2, DelaunayTriangle t)
		{
			bool flag = (p1 == this.Points[2] && p2 == this.Points[1]) || (p1 == this.Points[1] && p2 == this.Points[2]);
			if (flag)
			{
				this.Neighbors[0] = t;
			}
			else
			{
				bool flag2 = (p1 == this.Points[0] && p2 == this.Points[2]) || (p1 == this.Points[2] && p2 == this.Points[0]);
				if (flag2)
				{
					this.Neighbors[1] = t;
				}
				else
				{
					bool flag3 = (p1 == this.Points[0] && p2 == this.Points[1]) || (p1 == this.Points[1] && p2 == this.Points[0]);
					if (flag3)
					{
						this.Neighbors[2] = t;
					}
					else
					{
						Debug.WriteLine("Neighbor error, please report!");
					}
				}
			}
		}

		public void MarkNeighbor(DelaunayTriangle t)
		{
			bool flag = t.Contains(this.Points[1], this.Points[2]);
			if (flag)
			{
				this.Neighbors[0] = t;
				t.MarkNeighbor(this.Points[1], this.Points[2], this);
			}
			else
			{
				bool flag2 = t.Contains(this.Points[0], this.Points[2]);
				if (flag2)
				{
					this.Neighbors[1] = t;
					t.MarkNeighbor(this.Points[0], this.Points[2], this);
				}
				else
				{
					bool flag3 = t.Contains(this.Points[0], this.Points[1]);
					if (flag3)
					{
						this.Neighbors[2] = t;
						t.MarkNeighbor(this.Points[0], this.Points[1], this);
					}
					else
					{
						Debug.WriteLine("markNeighbor failed");
					}
				}
			}
		}

		public void ClearNeighbors()
		{
			this.Neighbors[0] = (this.Neighbors[1] = (this.Neighbors[2] = null));
		}

		public void ClearNeighbor(DelaunayTriangle triangle)
		{
			bool flag = this.Neighbors[0] == triangle;
			if (flag)
			{
				this.Neighbors[0] = null;
			}
			else
			{
				bool flag2 = this.Neighbors[1] == triangle;
				if (flag2)
				{
					this.Neighbors[1] = null;
				}
				else
				{
					this.Neighbors[2] = null;
				}
			}
		}

		public void Clear()
		{
			for (int i = 0; i < 3; i++)
			{
				DelaunayTriangle delaunayTriangle = this.Neighbors[i];
				bool flag = delaunayTriangle != null;
				if (flag)
				{
					delaunayTriangle.ClearNeighbor(this);
				}
			}
			this.ClearNeighbors();
			this.Points[0] = (this.Points[1] = (this.Points[2] = null));
		}

		public TriangulationPoint OppositePoint(DelaunayTriangle t, TriangulationPoint p)
		{
			Debug.Assert(t != this, "self-pointer error");
			return this.PointCW(t.PointCW(p));
		}

		public DelaunayTriangle NeighborCW(TriangulationPoint point)
		{
			return this.Neighbors[(this.Points.IndexOf(point) + 1) % 3];
		}

		public DelaunayTriangle NeighborCCW(TriangulationPoint point)
		{
			return this.Neighbors[(this.Points.IndexOf(point) + 2) % 3];
		}

		public DelaunayTriangle NeighborAcross(TriangulationPoint point)
		{
			return this.Neighbors[this.Points.IndexOf(point)];
		}

		public TriangulationPoint PointCCW(TriangulationPoint point)
		{
			return this.Points[(this.IndexOf(point) + 1) % 3];
		}

		public TriangulationPoint PointCW(TriangulationPoint point)
		{
			return this.Points[(this.IndexOf(point) + 2) % 3];
		}

		private void RotateCW()
		{
			TriangulationPoint value = this.Points[2];
			this.Points[2] = this.Points[1];
			this.Points[1] = this.Points[0];
			this.Points[0] = value;
		}

		public void Legalize(TriangulationPoint oPoint, TriangulationPoint nPoint)
		{
			this.RotateCW();
			this.Points[this.IndexCCW(oPoint)] = nPoint;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				this.Points[0],
				",",
				this.Points[1],
				",",
				this.Points[2]
			});
		}

		public void MarkNeighborEdges()
		{
			for (int i = 0; i < 3; i++)
			{
				bool flag = this.EdgeIsConstrained[i] && this.Neighbors[i] != null;
				if (flag)
				{
					this.Neighbors[i].MarkConstrainedEdge(this.Points[(i + 1) % 3], this.Points[(i + 2) % 3]);
				}
			}
		}

		public void MarkEdge(DelaunayTriangle triangle)
		{
			for (int i = 0; i < 3; i++)
			{
				bool flag = this.EdgeIsConstrained[i];
				if (flag)
				{
					triangle.MarkConstrainedEdge(this.Points[(i + 1) % 3], this.Points[(i + 2) % 3]);
				}
			}
		}

		public void MarkEdge(List<DelaunayTriangle> tList)
		{
			foreach (DelaunayTriangle current in tList)
			{
				for (int i = 0; i < 3; i++)
				{
					bool flag = current.EdgeIsConstrained[i];
					if (flag)
					{
						this.MarkConstrainedEdge(current.Points[(i + 1) % 3], current.Points[(i + 2) % 3]);
					}
				}
			}
		}

		public void MarkConstrainedEdge(int index)
		{
			this.EdgeIsConstrained[index] = true;
		}

		public void MarkConstrainedEdge(DTSweepConstraint edge)
		{
			this.MarkConstrainedEdge(edge.P, edge.Q);
		}

		public void MarkConstrainedEdge(TriangulationPoint p, TriangulationPoint q)
		{
			int num = this.EdgeIndex(p, q);
			bool flag = num != -1;
			if (flag)
			{
				this.EdgeIsConstrained[num] = true;
			}
		}

		public FP Area()
		{
			FP x = this.Points[0].X - this.Points[1].X;
			FP y = this.Points[2].Y - this.Points[1].Y;
			return FP.Abs(x * y * 0.5f);
		}

		public TriangulationPoint Centroid()
		{
			FP x = (this.Points[0].X + this.Points[1].X + this.Points[2].X) / 3f;
			FP y = (this.Points[0].Y + this.Points[1].Y + this.Points[2].Y) / 3f;
			return new TriangulationPoint(x, y);
		}

		public int EdgeIndex(TriangulationPoint p1, TriangulationPoint p2)
		{
			int num = this.Points.IndexOf(p1);
			int num2 = this.Points.IndexOf(p2);
			bool flag = num == 0 || num2 == 0;
			bool flag2 = num == 1 || num2 == 1;
			bool flag3 = num == 2 || num2 == 2;
			bool flag4 = flag2 & flag3;
			int result;
			if (flag4)
			{
				result = 0;
			}
			else
			{
				bool flag5 = flag & flag3;
				if (flag5)
				{
					result = 1;
				}
				else
				{
					bool flag6 = flag & flag2;
					if (flag6)
					{
						result = 2;
					}
					else
					{
						result = -1;
					}
				}
			}
			return result;
		}

		public bool GetConstrainedEdgeCCW(TriangulationPoint p)
		{
			return this.EdgeIsConstrained[(this.IndexOf(p) + 2) % 3];
		}

		public bool GetConstrainedEdgeCW(TriangulationPoint p)
		{
			return this.EdgeIsConstrained[(this.IndexOf(p) + 1) % 3];
		}

		public bool GetConstrainedEdgeAcross(TriangulationPoint p)
		{
			return this.EdgeIsConstrained[this.IndexOf(p)];
		}

		public void SetConstrainedEdgeCCW(TriangulationPoint p, bool ce)
		{
			this.EdgeIsConstrained[(this.IndexOf(p) + 2) % 3] = ce;
		}

		public void SetConstrainedEdgeCW(TriangulationPoint p, bool ce)
		{
			this.EdgeIsConstrained[(this.IndexOf(p) + 1) % 3] = ce;
		}

		public void SetConstrainedEdgeAcross(TriangulationPoint p, bool ce)
		{
			this.EdgeIsConstrained[this.IndexOf(p)] = ce;
		}

		public bool GetDelaunayEdgeCCW(TriangulationPoint p)
		{
			return this.EdgeIsDelaunay[(this.IndexOf(p) + 2) % 3];
		}

		public bool GetDelaunayEdgeCW(TriangulationPoint p)
		{
			return this.EdgeIsDelaunay[(this.IndexOf(p) + 1) % 3];
		}

		public bool GetDelaunayEdgeAcross(TriangulationPoint p)
		{
			return this.EdgeIsDelaunay[this.IndexOf(p)];
		}

		public void SetDelaunayEdgeCCW(TriangulationPoint p, bool ce)
		{
			this.EdgeIsDelaunay[(this.IndexOf(p) + 2) % 3] = ce;
		}

		public void SetDelaunayEdgeCW(TriangulationPoint p, bool ce)
		{
			this.EdgeIsDelaunay[(this.IndexOf(p) + 1) % 3] = ce;
		}

		public void SetDelaunayEdgeAcross(TriangulationPoint p, bool ce)
		{
			this.EdgeIsDelaunay[this.IndexOf(p)] = ce;
		}
	}
}
