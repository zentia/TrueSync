namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    internal class DTSweepContext : TriangulationContext
    {
        private DTSweepPointComparator _comparator = new DTSweepPointComparator();
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TriangulationPoint <Head>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TriangulationPoint <Tail>k__BackingField;
        public AdvancingFront aFront;
        private static readonly FP ALPHA = 0.3f;
        public DTSweepBasin Basin = new DTSweepBasin();
        public DTSweepEdgeEvent EdgeEvent = new DTSweepEdgeEvent();

        public DTSweepContext()
        {
            this.Clear();
        }

        public void AddNode(AdvancingFrontNode node)
        {
            this.aFront.AddNode(node);
        }

        public override void Clear()
        {
            base.Clear();
            base.Triangles.Clear();
        }

        public void CreateAdvancingFront()
        {
            DelaunayTriangle item = new DelaunayTriangle(base.Points[0], this.Tail, this.Head);
            base.Triangles.Add(item);
            AdvancingFrontNode head = new AdvancingFrontNode(item.Points[1]) {
                Triangle = item
            };
            AdvancingFrontNode node = new AdvancingFrontNode(item.Points[0]) {
                Triangle = item
            };
            AdvancingFrontNode tail = new AdvancingFrontNode(item.Points[2]);
            this.aFront = new AdvancingFront(head, tail);
            this.aFront.AddNode(node);
            this.aFront.Head.Next = node;
            node.Next = this.aFront.Tail;
            node.Prev = this.aFront.Head;
            this.aFront.Tail.Prev = node;
        }

        public void FinalizeTriangulation()
        {
            base.Triangulatable.AddTriangles(base.Triangles);
            base.Triangles.Clear();
        }

        public AdvancingFrontNode LocateNode(TriangulationPoint point)
        {
            return this.aFront.LocateNode(point);
        }

        public void MapTriangleToNodes(DelaunayTriangle t)
        {
            for (int i = 0; i < 3; i++)
            {
                if (t.Neighbors[i] == null)
                {
                    AdvancingFrontNode node = this.aFront.LocatePoint(t.PointCW(t.Points[i]));
                    if (node > null)
                    {
                        node.Triangle = t;
                    }
                }
            }
        }

        public void MeshClean(DelaunayTriangle triangle)
        {
            this.MeshCleanReq(triangle);
        }

        private void MeshCleanReq(DelaunayTriangle triangle)
        {
            if ((triangle != null) && !triangle.IsInterior)
            {
                triangle.IsInterior = true;
                base.Triangulatable.AddTriangle(triangle);
                for (int i = 0; i < 3; i++)
                {
                    if (!triangle.EdgeIsConstrained[i])
                    {
                        this.MeshCleanReq(triangle.Neighbors[i]);
                    }
                }
            }
        }

        public override TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b)
        {
            return new DTSweepConstraint(a, b);
        }

        public override void PrepareTriangulation(Triangulatable t)
        {
            FP fp2;
            FP fp4;
            base.PrepareTriangulation(t);
            FP x = fp2 = base.Points[0].X;
            FP y = fp4 = base.Points[0].Y;
            foreach (TriangulationPoint point3 in base.Points)
            {
                if (point3.X > x)
                {
                    x = point3.X;
                }
                if (point3.X < fp2)
                {
                    fp2 = point3.X;
                }
                if (point3.Y > y)
                {
                    y = point3.Y;
                }
                if (point3.Y < fp4)
                {
                    fp4 = point3.Y;
                }
            }
            FP fp5 = ALPHA * (x - fp2);
            FP fp6 = ALPHA * (y - fp4);
            TriangulationPoint point = new TriangulationPoint(x + fp5, fp4 - fp6);
            TriangulationPoint point2 = new TriangulationPoint(fp2 - fp5, fp4 - fp6);
            this.Head = point;
            this.Tail = point2;
            base.Points.Sort(this._comparator);
        }

        public void RemoveFromList(DelaunayTriangle triangle)
        {
            base.Triangles.Remove(triangle);
        }

        public void RemoveNode(AdvancingFrontNode node)
        {
            this.aFront.RemoveNode(node);
        }

        public TriangulationPoint Head { get; set; }

        public TriangulationPoint Tail { get; set; }

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
    }
}

