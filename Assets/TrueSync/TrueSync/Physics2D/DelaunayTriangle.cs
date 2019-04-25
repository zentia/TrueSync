// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.DelaunayTriangle
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

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

        public DelaunayTriangle(TriangulationPoint p1, TriangulationPoint p2, TriangulationPoint p3)
        {
            this.Points[0] = p1;
            this.Points[1] = p2;
            this.Points[2] = p3;
        }

        public bool IsInterior { get; set; }

        public int IndexOf(TriangulationPoint p)
        {
            int num = this.Points.IndexOf(p);
            if (num == -1)
                throw new Exception("Calling index with a point that doesn't exist in triangle");
            return num;
        }

        public int IndexCW(TriangulationPoint p)
        {
            switch (this.IndexOf(p))
            {
                case 0:
                    return 2;
                case 1:
                    return 0;
                default:
                    return 1;
            }
        }

        public int IndexCCW(TriangulationPoint p)
        {
            switch (this.IndexOf(p))
            {
                case 0:
                    return 1;
                case 1:
                    return 2;
                default:
                    return 0;
            }
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
            if (p1 == this.Points[2] && p2 == this.Points[1] || p1 == this.Points[1] && p2 == this.Points[2])
                this.Neighbors[0] = t;
            else if (p1 == this.Points[0] && p2 == this.Points[2] || p1 == this.Points[2] && p2 == this.Points[0])
                this.Neighbors[1] = t;
            else if (p1 == this.Points[0] && p2 == this.Points[1] || p1 == this.Points[1] && p2 == this.Points[0])
                this.Neighbors[2] = t;
            else
                Debug.WriteLine("Neighbor error, please report!");
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
                Debug.WriteLine("markNeighbor failed");
        }

        public void ClearNeighbors()
        {
            this.Neighbors[0] = this.Neighbors[1] = this.Neighbors[2] = (DelaunayTriangle)null;
        }

        public void ClearNeighbor(DelaunayTriangle triangle)
        {
            if (this.Neighbors[0] == triangle)
                this.Neighbors[0] = (DelaunayTriangle)null;
            else if (this.Neighbors[1] == triangle)
                this.Neighbors[1] = (DelaunayTriangle)null;
            else
                this.Neighbors[2] = (DelaunayTriangle)null;
        }

        public void Clear()
        {
            for (int index = 0; index < 3; ++index)
            {
                DelaunayTriangle neighbor = this.Neighbors[index];
                if (neighbor != null)
                    neighbor.ClearNeighbor(this);
            }
            this.ClearNeighbors();
            this.Points[0] = this.Points[1] = this.Points[2] = (TriangulationPoint)null;
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
            TriangulationPoint point = this.Points[2];
            this.Points[2] = this.Points[1];
            this.Points[1] = this.Points[0];
            this.Points[0] = point;
        }

        public void Legalize(TriangulationPoint oPoint, TriangulationPoint nPoint)
        {
            this.RotateCW();
            this.Points[this.IndexCCW(oPoint)] = nPoint;
        }

        public override string ToString()
        {
            return this.Points[0].ToString() + "," + (object)this.Points[1] + "," + (object)this.Points[2];
        }

        public void MarkNeighborEdges()
        {
            for (int index = 0; index < 3; ++index)
            {
                if (this.EdgeIsConstrained[index] && this.Neighbors[index] != null)
                    this.Neighbors[index].MarkConstrainedEdge(this.Points[(index + 1) % 3], this.Points[(index + 2) % 3]);
            }
        }

        public void MarkEdge(DelaunayTriangle triangle)
        {
            for (int index = 0; index < 3; ++index)
            {
                if (this.EdgeIsConstrained[index])
                    triangle.MarkConstrainedEdge(this.Points[(index + 1) % 3], this.Points[(index + 2) % 3]);
            }
        }

        public void MarkEdge(List<DelaunayTriangle> tList)
        {
            foreach (DelaunayTriangle t in tList)
            {
                for (int index = 0; index < 3; ++index)
                {
                    if (t.EdgeIsConstrained[index])
                        this.MarkConstrainedEdge(t.Points[(index + 1) % 3], t.Points[(index + 2) % 3]);
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
            int index = this.EdgeIndex(p, q);
            if (index == -1)
                return;
            this.EdgeIsConstrained[index] = true;
        }

        public FP Area()
        {
            return FP.Abs((this.Points[0].X - this.Points[1].X) * (this.Points[2].Y - this.Points[1].Y) * (FP)0.5f);
        }

        public TriangulationPoint Centroid()
        {
            return new TriangulationPoint((this.Points[0].X + this.Points[1].X + this.Points[2].X) / (FP)3f, (this.Points[0].Y + this.Points[1].Y + this.Points[2].Y) / (FP)3f);
        }

        public int EdgeIndex(TriangulationPoint p1, TriangulationPoint p2)
        {
            int num1 = this.Points.IndexOf(p1);
            int num2 = this.Points.IndexOf(p2);
            bool flag1 = num1 == 0 || num2 == 0;
            bool flag2 = num1 == 1 || num2 == 1;
            bool flag3 = num1 == 2 || num2 == 2;
            if (flag2 & flag3)
                return 0;
            if (flag1 & flag3)
                return 1;
            return flag1 & flag2 ? 2 : -1;
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
