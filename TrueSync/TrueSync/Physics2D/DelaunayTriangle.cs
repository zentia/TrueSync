namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    internal class DelaunayTriangle
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IsInterior>k__BackingField;
        public FixedBitArray3 EdgeIsConstrained;
        public FixedBitArray3 EdgeIsDelaunay;
        public FixedArray3Enum<DelaunayTriangle> Neighbors;
        public FixedArray3Enum<TriangulationPoint> Points;

        public DelaunayTriangle(TriangulationPoint p1, TriangulationPoint p2, TriangulationPoint p3)
        {
            this.Points[0] = p1;
            this.Points[1] = p2;
            this.Points[2] = p3;
        }

        public FP Area()
        {
            FP fp = this.Points[0].X - this.Points[1].X;
            FP fp2 = this.Points[2].Y - this.Points[1].Y;
            return FP.Abs((fp * fp2) * 0.5f);
        }

        public TriangulationPoint Centroid()
        {
            FP x = ((this.Points[0].X + this.Points[1].X) + this.Points[2].X) / 3f;
            return new TriangulationPoint(x, ((this.Points[0].Y + this.Points[1].Y) + this.Points[2].Y) / 3f);
        }

        public void Clear()
        {
            TriangulationPoint point2;
            for (int i = 0; i < 3; i++)
            {
                DelaunayTriangle triangle = this.Neighbors[i];
                if (triangle > null)
                {
                    triangle.ClearNeighbor(this);
                }
            }
            this.ClearNeighbors();
            this.Points[2] = (TriangulationPoint) (point2 = null);
            this.Points[0] = this.Points[1] = point2;
        }

        public void ClearNeighbor(DelaunayTriangle triangle)
        {
            if (this.Neighbors[0] == triangle)
            {
                this.Neighbors[0] = null;
            }
            else if (this.Neighbors[1] == triangle)
            {
                this.Neighbors[1] = null;
            }
            else
            {
                this.Neighbors[2] = null;
            }
        }

        public void ClearNeighbors()
        {
            DelaunayTriangle triangle2;
            this.Neighbors[2] = (DelaunayTriangle) (triangle2 = null);
            this.Neighbors[0] = this.Neighbors[1] = triangle2;
        }

        public bool Contains(DTSweepConstraint e)
        {
            return (this.Contains(e.P) && this.Contains(e.Q));
        }

        public bool Contains(TriangulationPoint p)
        {
            return (((p == this.Points[0]) || (p == this.Points[1])) || (p == this.Points[2]));
        }

        public bool Contains(TriangulationPoint p, TriangulationPoint q)
        {
            return (this.Contains(p) && this.Contains(q));
        }

        public int EdgeIndex(TriangulationPoint p1, TriangulationPoint p2)
        {
            int index = this.Points.IndexOf(p1);
            int num2 = this.Points.IndexOf(p2);
            bool flag = (index == 0) || (num2 == 0);
            bool flag2 = (index == 1) || (num2 == 1);
            bool flag3 = (index == 2) || (num2 == 2);
            if (flag2 & flag3)
            {
                return 0;
            }
            if (flag & flag3)
            {
                return 1;
            }
            if (flag & flag2)
            {
                return 2;
            }
            return -1;
        }

        public bool GetConstrainedEdgeAcross(TriangulationPoint p)
        {
            return this.EdgeIsConstrained[this.IndexOf(p)];
        }

        public bool GetConstrainedEdgeCCW(TriangulationPoint p)
        {
            return this.EdgeIsConstrained[(this.IndexOf(p) + 2) % 3];
        }

        public bool GetConstrainedEdgeCW(TriangulationPoint p)
        {
            return this.EdgeIsConstrained[(this.IndexOf(p) + 1) % 3];
        }

        public bool GetDelaunayEdgeAcross(TriangulationPoint p)
        {
            return this.EdgeIsDelaunay[this.IndexOf(p)];
        }

        public bool GetDelaunayEdgeCCW(TriangulationPoint p)
        {
            return this.EdgeIsDelaunay[(this.IndexOf(p) + 2) % 3];
        }

        public bool GetDelaunayEdgeCW(TriangulationPoint p)
        {
            return this.EdgeIsDelaunay[(this.IndexOf(p) + 1) % 3];
        }

        public int IndexCCW(TriangulationPoint p)
        {
            switch (this.IndexOf(p))
            {
                case 0:
                    return 1;

                case 1:
                    return 2;
            }
            return 0;
        }

        public int IndexCW(TriangulationPoint p)
        {
            switch (this.IndexOf(p))
            {
                case 0:
                    return 2;

                case 1:
                    return 0;
            }
            return 1;
        }

        public int IndexOf(TriangulationPoint p)
        {
            int index = this.Points.IndexOf(p);
            if (index == -1)
            {
                throw new Exception("Calling index with a point that doesn't exist in triangle");
            }
            return index;
        }

        public void Legalize(TriangulationPoint oPoint, TriangulationPoint nPoint)
        {
            this.RotateCW();
            this.Points[this.IndexCCW(oPoint)] = nPoint;
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
            if (num != -1)
            {
                this.EdgeIsConstrained[num] = true;
            }
        }

        public void MarkEdge(List<DelaunayTriangle> tList)
        {
            foreach (DelaunayTriangle triangle in tList)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (triangle.EdgeIsConstrained[i])
                    {
                        this.MarkConstrainedEdge(triangle.Points[(i + 1) % 3], triangle.Points[(i + 2) % 3]);
                    }
                }
            }
        }

        public void MarkEdge(DelaunayTriangle triangle)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this.EdgeIsConstrained[i])
                {
                    triangle.MarkConstrainedEdge(this.Points[(i + 1) % 3], this.Points[(i + 2) % 3]);
                }
            }
        }

        public void MarkNeighbor(DelaunayTriangle t)
        {
            if (t.Contains(this.Points[1], this.Points[2]))
            {
                this.Neighbors[0] = t;
                t.MarkNeighbor(this.Points[1], this.Points[2], this);
            }
            else if (t.Contains(this.Points[0], this.Points[2]))
            {
                this.Neighbors[1] = t;
                t.MarkNeighbor(this.Points[0], this.Points[2], this);
            }
            else if (t.Contains(this.Points[0], this.Points[1]))
            {
                this.Neighbors[2] = t;
                t.MarkNeighbor(this.Points[0], this.Points[1], this);
            }
            else
            {
                Debug.WriteLine("markNeighbor failed");
            }
        }

        private void MarkNeighbor(TriangulationPoint p1, TriangulationPoint p2, DelaunayTriangle t)
        {
            if (((p1 == this.Points[2]) && (p2 == this.Points[1])) || ((p1 == this.Points[1]) && (p2 == this.Points[2])))
            {
                this.Neighbors[0] = t;
            }
            else if (((p1 == this.Points[0]) && (p2 == this.Points[2])) || ((p1 == this.Points[2]) && (p2 == this.Points[0])))
            {
                this.Neighbors[1] = t;
            }
            else if (((p1 == this.Points[0]) && (p2 == this.Points[1])) || ((p1 == this.Points[1]) && (p2 == this.Points[0])))
            {
                this.Neighbors[2] = t;
            }
            else
            {
                Debug.WriteLine("Neighbor error, please report!");
            }
        }

        public void MarkNeighborEdges()
        {
            for (int i = 0; i < 3; i++)
            {
                if (this.EdgeIsConstrained[i] && (this.Neighbors[i] > null))
                {
                    this.Neighbors[i].MarkConstrainedEdge(this.Points[(i + 1) % 3], this.Points[(i + 2) % 3]);
                }
            }
        }

        public DelaunayTriangle NeighborAcross(TriangulationPoint point)
        {
            return this.Neighbors[this.Points.IndexOf(point)];
        }

        public DelaunayTriangle NeighborCCW(TriangulationPoint point)
        {
            return this.Neighbors[(this.Points.IndexOf(point) + 2) % 3];
        }

        public DelaunayTriangle NeighborCW(TriangulationPoint point)
        {
            return this.Neighbors[(this.Points.IndexOf(point) + 1) % 3];
        }

        public TriangulationPoint OppositePoint(DelaunayTriangle t, TriangulationPoint p)
        {
            Debug.Assert(t != this, "self-pointer error");
            return this.PointCW(t.PointCW(p));
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
            TriangulationPoint point = this.Points[2];
            this.Points[2] = this.Points[1];
            this.Points[1] = this.Points[0];
            this.Points[0] = point;
        }

        public void SetConstrainedEdgeAcross(TriangulationPoint p, bool ce)
        {
            this.EdgeIsConstrained[this.IndexOf(p)] = ce;
        }

        public void SetConstrainedEdgeCCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsConstrained[(this.IndexOf(p) + 2) % 3] = ce;
        }

        public void SetConstrainedEdgeCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsConstrained[(this.IndexOf(p) + 1) % 3] = ce;
        }

        public void SetDelaunayEdgeAcross(TriangulationPoint p, bool ce)
        {
            this.EdgeIsDelaunay[this.IndexOf(p)] = ce;
        }

        public void SetDelaunayEdgeCCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsDelaunay[(this.IndexOf(p) + 2) % 3] = ce;
        }

        public void SetDelaunayEdgeCW(TriangulationPoint p, bool ce)
        {
            this.EdgeIsDelaunay[(this.IndexOf(p) + 1) % 3] = ce;
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { this.Points[0], ",", this.Points[1], ",", this.Points[2] };
            return string.Concat(objArray1);
        }

        public bool IsInterior { get; set; }
    }
}

