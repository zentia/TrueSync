namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    internal static class DTSweep
    {
        private static readonly FP PI_3div4 = ((3 * FP.Pi) / 4);
        private static readonly FP PI_div2 = (FP.Pi / 2);

        private static FP Angle(TriangulationPoint origin, TriangulationPoint pa, TriangulationPoint pb)
        {
            FP x = origin.X;
            FP y = origin.Y;
            FP fp3 = pa.X - x;
            FP fp4 = pa.Y - y;
            FP fp5 = pb.X - x;
            FP fp6 = pb.Y - y;
            FP fp7 = (fp3 * fp6) - (fp4 * fp5);
            FP fp8 = (fp3 * fp5) + (fp4 * fp6);
            return FP.Atan2(fp7, fp8);
        }

        private static bool AngleExceeds90Degrees(TriangulationPoint origin, TriangulationPoint pa, TriangulationPoint pb)
        {
            FP fp = Angle(origin, pa, pb);
            return ((fp > PI_div2) || (fp < -PI_div2));
        }

        private static bool AngleExceedsPlus90DegreesOrIsNegative(TriangulationPoint origin, TriangulationPoint pa, TriangulationPoint pb)
        {
            FP fp = Angle(origin, pa, pb);
            return ((fp > PI_div2) || (fp < 0));
        }

        private static FP BasinAngle(AdvancingFrontNode node)
        {
            FP x = node.Point.X - node.Next.Next.Point.X;
            FP y = node.Point.Y - node.Next.Next.Point.Y;
            return FP.Atan2(y, x);
        }

        private static void EdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            try
            {
                tcx.EdgeEvent.ConstrainedEdge = edge;
                tcx.EdgeEvent.Right = edge.P.X > edge.Q.X;
                if (!IsEdgeSideOfTriangle(node.Triangle, edge.P, edge.Q))
                {
                    FillEdgeEvent(tcx, edge, node);
                    EdgeEvent(tcx, edge.P, edge.Q, node.Triangle, edge.Q);
                }
            }
            catch (PointOnEdgeException exception)
            {
                Debug.WriteLine("Skipping Edge: " + exception.Message);
            }
        }

        private static void EdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle triangle, TriangulationPoint point)
        {
            if (!IsEdgeSideOfTriangle(triangle, ep, eq))
            {
                TriangulationPoint pb = triangle.PointCCW(point);
                Orientation orientation = TriangulationUtil.Orient2d(eq, pb, ep);
                if (orientation == Orientation.Collinear)
                {
                    if (!triangle.Contains(eq, pb))
                    {
                        throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet");
                    }
                    triangle.MarkConstrainedEdge(eq, pb);
                    tcx.EdgeEvent.ConstrainedEdge.Q = pb;
                    triangle = triangle.NeighborAcross(point);
                    EdgeEvent(tcx, ep, pb, triangle, pb);
                    if (tcx.IsDebugEnabled)
                    {
                        Debug.WriteLine("EdgeEvent - Point on constrained edge");
                    }
                }
                else
                {
                    TriangulationPoint point3 = triangle.PointCW(point);
                    Orientation orientation2 = TriangulationUtil.Orient2d(eq, point3, ep);
                    if (orientation2 == Orientation.Collinear)
                    {
                        if (!triangle.Contains(eq, point3))
                        {
                            throw new PointOnEdgeException("EdgeEvent - Point on constrained edge not supported yet");
                        }
                        triangle.MarkConstrainedEdge(eq, point3);
                        tcx.EdgeEvent.ConstrainedEdge.Q = point3;
                        triangle = triangle.NeighborAcross(point);
                        EdgeEvent(tcx, ep, point3, triangle, point3);
                        if (tcx.IsDebugEnabled)
                        {
                            Debug.WriteLine("EdgeEvent - Point on constrained edge");
                        }
                    }
                    else if (orientation == orientation2)
                    {
                        if (orientation == Orientation.CW)
                        {
                            triangle = triangle.NeighborCCW(point);
                        }
                        else
                        {
                            triangle = triangle.NeighborCW(point);
                        }
                        EdgeEvent(tcx, ep, eq, triangle, point);
                    }
                    else
                    {
                        FlipEdgeEvent(tcx, ep, eq, triangle, point);
                    }
                }
            }
        }

        private static void Fill(DTSweepContext tcx, AdvancingFrontNode node)
        {
            DelaunayTriangle item = new DelaunayTriangle(node.Prev.Point, node.Point, node.Next.Point);
            item.MarkNeighbor(node.Prev.Triangle);
            item.MarkNeighbor(node.Triangle);
            tcx.Triangles.Add(item);
            node.Prev.Next = node.Next;
            node.Next.Prev = node.Prev;
            tcx.RemoveNode(node);
            if (!Legalize(tcx, item))
            {
                tcx.MapTriangleToNodes(item);
            }
        }

        private static void FillAdvancingFront(DTSweepContext tcx, AdvancingFrontNode n)
        {
            AdvancingFrontNode node;
            for (node = n.Next; node.HasNext; node = node.Next)
            {
                if (LargeHole_DontFill(node))
                {
                    break;
                }
                Fill(tcx, node);
            }
            for (node = n.Prev; node.HasPrev; node = node.Prev)
            {
                if (LargeHole_DontFill(node))
                {
                    break;
                }
                FP fp = HoleAngle(node);
                if ((fp > PI_div2) || (fp < -PI_div2))
                {
                    break;
                }
                Fill(tcx, node);
            }
            if ((n.HasNext && n.Next.HasNext) && (BasinAngle(n) < PI_3div4))
            {
                FillBasin(tcx, n);
            }
        }

        private static void FillBasin(DTSweepContext tcx, AdvancingFrontNode node)
        {
            if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW)
            {
                tcx.Basin.leftNode = node;
            }
            else
            {
                tcx.Basin.leftNode = node.Next;
            }
            tcx.Basin.bottomNode = tcx.Basin.leftNode;
            while (tcx.Basin.bottomNode.HasNext && (tcx.Basin.bottomNode.Point.Y >= tcx.Basin.bottomNode.Next.Point.Y))
            {
                tcx.Basin.bottomNode = tcx.Basin.bottomNode.Next;
            }
            if (tcx.Basin.bottomNode != tcx.Basin.leftNode)
            {
                tcx.Basin.rightNode = tcx.Basin.bottomNode;
                while (tcx.Basin.rightNode.HasNext && (tcx.Basin.rightNode.Point.Y < tcx.Basin.rightNode.Next.Point.Y))
                {
                    tcx.Basin.rightNode = tcx.Basin.rightNode.Next;
                }
                if (tcx.Basin.rightNode != tcx.Basin.bottomNode)
                {
                    tcx.Basin.width = tcx.Basin.rightNode.Point.X - tcx.Basin.leftNode.Point.X;
                    tcx.Basin.leftHighest = tcx.Basin.leftNode.Point.Y > tcx.Basin.rightNode.Point.Y;
                    FillBasinReq(tcx, tcx.Basin.bottomNode);
                }
            }
        }

        private static void FillBasinReq(DTSweepContext tcx, AdvancingFrontNode node)
        {
            if (!IsShallow(tcx, node))
            {
                Fill(tcx, node);
                if ((node.Prev != tcx.Basin.leftNode) || (node.Next != tcx.Basin.rightNode))
                {
                    if (node.Prev == tcx.Basin.leftNode)
                    {
                        if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CW)
                        {
                            return;
                        }
                        node = node.Next;
                    }
                    else if (node.Next == tcx.Basin.rightNode)
                    {
                        if (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CCW)
                        {
                            return;
                        }
                        node = node.Prev;
                    }
                    else if (node.Prev.Point.Y < node.Next.Point.Y)
                    {
                        node = node.Prev;
                    }
                    else
                    {
                        node = node.Next;
                    }
                    FillBasinReq(tcx, node);
                }
            }
        }

        private static void FillEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (tcx.EdgeEvent.Right)
            {
                FillRightAboveEdgeEvent(tcx, edge, node);
            }
            else
            {
                FillLeftAboveEdgeEvent(tcx, edge, node);
            }
        }

        private static void FillLeftAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            while (node.Prev.Point.X > edge.P.X)
            {
                if (TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Orientation.CW)
                {
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                }
                else
                {
                    node = node.Prev;
                }
            }
        }

        private static void FillLeftBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (node.Point.X > edge.P.X)
            {
                if (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW)
                {
                    FillLeftConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    FillLeftConvexEdgeEvent(tcx, edge, node);
                    FillLeftBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private static void FillLeftConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            Fill(tcx, node.Prev);
            if (((node.Prev.Point != edge.P) && (TriangulationUtil.Orient2d(edge.Q, node.Prev.Point, edge.P) == Orientation.CW)) && (TriangulationUtil.Orient2d(node.Point, node.Prev.Point, node.Prev.Prev.Point) == Orientation.CW))
            {
                FillLeftConcaveEdgeEvent(tcx, edge, node);
            }
        }

        private static void FillLeftConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (TriangulationUtil.Orient2d(node.Prev.Point, node.Prev.Prev.Point, node.Prev.Prev.Prev.Point) == Orientation.CW)
            {
                FillLeftConcaveEdgeEvent(tcx, edge, node.Prev);
            }
            else if (TriangulationUtil.Orient2d(edge.Q, node.Prev.Prev.Point, edge.P) == Orientation.CW)
            {
                FillLeftConvexEdgeEvent(tcx, edge, node.Prev);
            }
        }

        private static void FillRightAboveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            while (node.Next.Point.X < edge.P.X)
            {
                if (TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Orientation.CCW)
                {
                    FillRightBelowEdgeEvent(tcx, edge, node);
                }
                else
                {
                    node = node.Next;
                }
            }
        }

        private static void FillRightBelowEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (node.Point.X < edge.P.X)
            {
                if (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW)
                {
                    FillRightConcaveEdgeEvent(tcx, edge, node);
                }
                else
                {
                    FillRightConvexEdgeEvent(tcx, edge, node);
                    FillRightBelowEdgeEvent(tcx, edge, node);
                }
            }
        }

        private static void FillRightConcaveEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            Fill(tcx, node.Next);
            if (((node.Next.Point != edge.P) && (TriangulationUtil.Orient2d(edge.Q, node.Next.Point, edge.P) == Orientation.CCW)) && (TriangulationUtil.Orient2d(node.Point, node.Next.Point, node.Next.Next.Point) == Orientation.CCW))
            {
                FillRightConcaveEdgeEvent(tcx, edge, node);
            }
        }

        private static void FillRightConvexEdgeEvent(DTSweepContext tcx, DTSweepConstraint edge, AdvancingFrontNode node)
        {
            if (TriangulationUtil.Orient2d(node.Next.Point, node.Next.Next.Point, node.Next.Next.Next.Point) == Orientation.CCW)
            {
                FillRightConcaveEdgeEvent(tcx, edge, node.Next);
            }
            else if (TriangulationUtil.Orient2d(edge.Q, node.Next.Next.Point, edge.P) == Orientation.CCW)
            {
                FillRightConvexEdgeEvent(tcx, edge, node.Next);
            }
        }

        private static void FinalizationConvexHull(DTSweepContext tcx)
        {
            DelaunayTriangle triangle;
            DelaunayTriangle triangle2;
            AdvancingFrontNode next = tcx.aFront.Head.Next;
            AdvancingFrontNode c = next.Next;
            TurnAdvancingFrontConvex(tcx, next, c);
            next = tcx.aFront.Tail.Prev;
            if (next.Triangle.Contains(next.Next.Point) && next.Triangle.Contains(next.Prev.Point))
            {
                triangle = next.Triangle.NeighborAcross(next.Point);
                RotateTrianglePair(next.Triangle, next.Point, triangle, triangle.OppositePoint(next.Triangle, next.Point));
                tcx.MapTriangleToNodes(next.Triangle);
                tcx.MapTriangleToNodes(triangle);
            }
            next = tcx.aFront.Head.Next;
            if (next.Triangle.Contains(next.Prev.Point) && next.Triangle.Contains(next.Next.Point))
            {
                triangle = next.Triangle.NeighborAcross(next.Point);
                RotateTrianglePair(next.Triangle, next.Point, triangle, triangle.OppositePoint(next.Triangle, next.Point));
                tcx.MapTriangleToNodes(next.Triangle);
                tcx.MapTriangleToNodes(triangle);
            }
            TriangulationPoint point = tcx.aFront.Head.Point;
            c = tcx.aFront.Tail.Prev;
            triangle = c.Triangle;
            TriangulationPoint point2 = c.Point;
            c.Triangle = null;
            while (true)
            {
                tcx.RemoveFromList(triangle);
                point2 = triangle.PointCCW(point2);
                if (point2 == point)
                {
                    break;
                }
                triangle2 = triangle.NeighborCCW(point2);
                triangle.Clear();
                triangle = triangle2;
            }
            point = tcx.aFront.Head.Next.Point;
            point2 = triangle.PointCW(tcx.aFront.Head.Point);
            triangle2 = triangle.NeighborCW(tcx.aFront.Head.Point);
            triangle.Clear();
            for (triangle = triangle2; point2 != point; triangle = triangle2)
            {
                tcx.RemoveFromList(triangle);
                point2 = triangle.PointCCW(point2);
                triangle2 = triangle.NeighborCCW(point2);
                triangle.Clear();
            }
            tcx.aFront.Head = tcx.aFront.Head.Next;
            tcx.aFront.Head.Prev = null;
            tcx.aFront.Tail = tcx.aFront.Tail.Prev;
            tcx.aFront.Tail.Next = null;
            tcx.FinalizeTriangulation();
        }

        private static void FinalizationPolygon(DTSweepContext tcx)
        {
            DelaunayTriangle triangle = tcx.aFront.Head.Next.Triangle;
            TriangulationPoint p = tcx.aFront.Head.Next.Point;
            while (!triangle.GetConstrainedEdgeCW(p))
            {
                triangle = triangle.NeighborCCW(p);
            }
            tcx.MeshClean(triangle);
        }

        private static void FlipEdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle t, TriangulationPoint p)
        {
            DelaunayTriangle ot = t.NeighborAcross(p);
            TriangulationPoint pd = ot.OppositePoint(t, p);
            if (ot == null)
            {
                throw new InvalidOperationException("[BUG:FIXME] FLIP failed due to missing triangle");
            }
            if (t.GetConstrainedEdgeAcross(p))
            {
                throw new Exception("Intersecting Constraints");
            }
            if (TriangulationUtil.InScanArea(p, t.PointCCW(p), t.PointCW(p), pd))
            {
                RotateTrianglePair(t, p, ot, pd);
                tcx.MapTriangleToNodes(t);
                tcx.MapTriangleToNodes(ot);
                if ((p == eq) && (pd == ep))
                {
                    if ((eq == tcx.EdgeEvent.ConstrainedEdge.Q) && (ep == tcx.EdgeEvent.ConstrainedEdge.P))
                    {
                        if (tcx.IsDebugEnabled)
                        {
                            Console.WriteLine("[FLIP] - constrained edge done");
                        }
                        t.MarkConstrainedEdge(ep, eq);
                        ot.MarkConstrainedEdge(ep, eq);
                        Legalize(tcx, t);
                        Legalize(tcx, ot);
                    }
                    else if (tcx.IsDebugEnabled)
                    {
                        Console.WriteLine("[FLIP] - subedge done");
                    }
                }
                else
                {
                    if (tcx.IsDebugEnabled)
                    {
                        Console.WriteLine("[FLIP] - flipping and continuing with triangle still crossing edge");
                    }
                    Orientation o = TriangulationUtil.Orient2d(eq, pd, ep);
                    t = NextFlipTriangle(tcx, o, t, ot, p, pd);
                    FlipEdgeEvent(tcx, ep, eq, t, p);
                }
            }
            else
            {
                TriangulationPoint point2 = NextFlipPoint(ep, eq, ot, pd);
                FlipScanEdgeEvent(tcx, ep, eq, t, ot, point2);
                EdgeEvent(tcx, ep, eq, t, p);
            }
        }

        private static void FlipScanEdgeEvent(DTSweepContext tcx, TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle flipTriangle, DelaunayTriangle t, TriangulationPoint p)
        {
            DelaunayTriangle triangle = t.NeighborAcross(p);
            TriangulationPoint pd = triangle.OppositePoint(t, p);
            if (triangle == null)
            {
                throw new Exception("[BUG:FIXME] FLIP failed due to missing triangle");
            }
            if (TriangulationUtil.InScanArea(eq, flipTriangle.PointCCW(eq), flipTriangle.PointCW(eq), pd))
            {
                FlipEdgeEvent(tcx, eq, pd, triangle, pd);
            }
            else
            {
                TriangulationPoint point2 = NextFlipPoint(ep, eq, triangle, pd);
                FlipScanEdgeEvent(tcx, ep, eq, flipTriangle, triangle, point2);
            }
        }

        private static FP HoleAngle(AdvancingFrontNode node)
        {
            FP x = node.Point.X;
            FP y = node.Point.Y;
            FP fp3 = node.Next.Point.X - x;
            FP fp4 = node.Next.Point.Y - y;
            FP fp5 = node.Prev.Point.X - x;
            FP fp6 = node.Prev.Point.Y - y;
            return FP.Atan2((fp3 * fp6) - (fp4 * fp5), (fp3 * fp5) + (fp4 * fp6));
        }

        private static bool IsEdgeSideOfTriangle(DelaunayTriangle triangle, TriangulationPoint ep, TriangulationPoint eq)
        {
            int index = triangle.EdgeIndex(ep, eq);
            if (index != -1)
            {
                triangle.MarkConstrainedEdge(index);
                triangle = triangle.Neighbors[index];
                if (triangle > null)
                {
                    triangle.MarkConstrainedEdge(ep, eq);
                }
                return true;
            }
            return false;
        }

        private static bool IsShallow(DTSweepContext tcx, AdvancingFrontNode node)
        {
            FP fp;
            if (tcx.Basin.leftHighest)
            {
                fp = tcx.Basin.leftNode.Point.Y - node.Point.Y;
            }
            else
            {
                fp = tcx.Basin.rightNode.Point.Y - node.Point.Y;
            }
            return (tcx.Basin.width > fp);
        }

        private static bool LargeHole_DontFill(AdvancingFrontNode node)
        {
            AdvancingFrontNode next = node.Next;
            AdvancingFrontNode prev = node.Prev;
            if (!AngleExceeds90Degrees(node.Point, next.Point, prev.Point))
            {
                return false;
            }
            AdvancingFrontNode node4 = next.Next;
            if ((node4 != null) && !AngleExceedsPlus90DegreesOrIsNegative(node.Point, node4.Point, prev.Point))
            {
                return false;
            }
            AdvancingFrontNode node5 = prev.Prev;
            if ((node5 != null) && !AngleExceedsPlus90DegreesOrIsNegative(node.Point, next.Point, node5.Point))
            {
                return false;
            }
            return true;
        }

        private static bool Legalize(DTSweepContext tcx, DelaunayTriangle t)
        {
            for (int i = 0; i < 3; i++)
            {
                if (!t.EdgeIsDelaunay[i])
                {
                    DelaunayTriangle ot = t.Neighbors[i];
                    if (ot > null)
                    {
                        TriangulationPoint p = t.Points[i];
                        TriangulationPoint point2 = ot.OppositePoint(t, p);
                        int index = ot.IndexOf(point2);
                        if (ot.EdgeIsConstrained[index] || ot.EdgeIsDelaunay[index])
                        {
                            t.EdgeIsConstrained[i] = ot.EdgeIsConstrained[index];
                        }
                        else if (TriangulationUtil.SmartIncircle(p, t.PointCCW(p), t.PointCW(p), point2))
                        {
                            t.EdgeIsDelaunay[i] = true;
                            ot.EdgeIsDelaunay[index] = true;
                            RotateTrianglePair(t, p, ot, point2);
                            if (!Legalize(tcx, t))
                            {
                                tcx.MapTriangleToNodes(t);
                            }
                            if (!Legalize(tcx, ot))
                            {
                                tcx.MapTriangleToNodes(ot);
                            }
                            t.EdgeIsDelaunay[i] = false;
                            ot.EdgeIsDelaunay[index] = false;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static AdvancingFrontNode NewFrontTriangle(DTSweepContext tcx, TriangulationPoint point, AdvancingFrontNode node)
        {
            DelaunayTriangle item = new DelaunayTriangle(point, node.Point, node.Next.Point);
            item.MarkNeighbor(node.Triangle);
            tcx.Triangles.Add(item);
            AdvancingFrontNode node2 = new AdvancingFrontNode(point) {
                Next = node.Next,
                Prev = node
            };
            node.Next.Prev = node2;
            node.Next = node2;
            tcx.AddNode(node2);
            if (!Legalize(tcx, item))
            {
                tcx.MapTriangleToNodes(item);
            }
            return node2;
        }

        private static TriangulationPoint NextFlipPoint(TriangulationPoint ep, TriangulationPoint eq, DelaunayTriangle ot, TriangulationPoint op)
        {
            switch (TriangulationUtil.Orient2d(eq, op, ep))
            {
                case Orientation.CW:
                    return ot.PointCCW(op);

                case Orientation.CCW:
                    return ot.PointCW(op);
            }
            throw new PointOnEdgeException("Point on constrained edge not supported yet");
        }

        private static DelaunayTriangle NextFlipTriangle(DTSweepContext tcx, Orientation o, DelaunayTriangle t, DelaunayTriangle ot, TriangulationPoint p, TriangulationPoint op)
        {
            int num;
            if (o == Orientation.CCW)
            {
                num = ot.EdgeIndex(p, op);
                ot.EdgeIsDelaunay[num] = true;
                Legalize(tcx, ot);
                ot.EdgeIsDelaunay.Clear();
                return t;
            }
            num = t.EdgeIndex(p, op);
            t.EdgeIsDelaunay[num] = true;
            Legalize(tcx, t);
            t.EdgeIsDelaunay.Clear();
            return ot;
        }

        private static AdvancingFrontNode PointEvent(DTSweepContext tcx, TriangulationPoint point)
        {
            AdvancingFrontNode node = tcx.LocateNode(point);
            AdvancingFrontNode node2 = NewFrontTriangle(tcx, point, node);
            if (point.X <= (node.Point.X + TriangulationUtil.EPSILON))
            {
                Fill(tcx, node);
            }
            tcx.AddNode(node2);
            FillAdvancingFront(tcx, node2);
            return node2;
        }

        private static void RotateTrianglePair(DelaunayTriangle t, TriangulationPoint p, DelaunayTriangle ot, TriangulationPoint op)
        {
            DelaunayTriangle triangle = t.NeighborCCW(p);
            DelaunayTriangle triangle2 = t.NeighborCW(p);
            DelaunayTriangle triangle3 = ot.NeighborCCW(op);
            DelaunayTriangle triangle4 = ot.NeighborCW(op);
            bool constrainedEdgeCCW = t.GetConstrainedEdgeCCW(p);
            bool constrainedEdgeCW = t.GetConstrainedEdgeCW(p);
            bool ce = ot.GetConstrainedEdgeCCW(op);
            bool flag4 = ot.GetConstrainedEdgeCW(op);
            bool delaunayEdgeCCW = t.GetDelaunayEdgeCCW(p);
            bool delaunayEdgeCW = t.GetDelaunayEdgeCW(p);
            bool flag7 = ot.GetDelaunayEdgeCCW(op);
            bool flag8 = ot.GetDelaunayEdgeCW(op);
            t.Legalize(p, op);
            ot.Legalize(op, p);
            ot.SetDelaunayEdgeCCW(p, delaunayEdgeCCW);
            t.SetDelaunayEdgeCW(p, delaunayEdgeCW);
            t.SetDelaunayEdgeCCW(op, flag7);
            ot.SetDelaunayEdgeCW(op, flag8);
            ot.SetConstrainedEdgeCCW(p, constrainedEdgeCCW);
            t.SetConstrainedEdgeCW(p, constrainedEdgeCW);
            t.SetConstrainedEdgeCCW(op, ce);
            ot.SetConstrainedEdgeCW(op, flag4);
            t.Neighbors.Clear();
            ot.Neighbors.Clear();
            if (triangle > null)
            {
                ot.MarkNeighbor(triangle);
            }
            if (triangle2 > null)
            {
                t.MarkNeighbor(triangle2);
            }
            if (triangle3 > null)
            {
                t.MarkNeighbor(triangle3);
            }
            if (triangle4 > null)
            {
                ot.MarkNeighbor(triangle4);
            }
            t.MarkNeighbor(ot);
        }

        private static void Sweep(DTSweepContext tcx)
        {
            List<TriangulationPoint> points = tcx.Points;
            for (int i = 1; i < points.Count; i++)
            {
                TriangulationPoint point = points[i];
                AdvancingFrontNode node = PointEvent(tcx, point);
                if (point.HasEdges)
                {
                    foreach (DTSweepConstraint constraint in point.Edges)
                    {
                        EdgeEvent(tcx, constraint, node);
                    }
                }
                tcx.Update(null);
            }
        }

        public static void Triangulate(DTSweepContext tcx)
        {
            tcx.CreateAdvancingFront();
            Sweep(tcx);
            if (tcx.TriangulationMode == TriangulationMode.Polygon)
            {
                FinalizationPolygon(tcx);
            }
            else
            {
                FinalizationConvexHull(tcx);
            }
            tcx.Done();
        }

        private static void TurnAdvancingFrontConvex(DTSweepContext tcx, AdvancingFrontNode b, AdvancingFrontNode c)
        {
            AdvancingFrontNode node = b;
            while (c != tcx.aFront.Tail)
            {
                if (TriangulationUtil.Orient2d(b.Point, c.Point, c.Next.Point) == Orientation.CCW)
                {
                    Fill(tcx, c);
                    c = c.Next;
                }
                else if ((b != node) && (TriangulationUtil.Orient2d(b.Prev.Point, b.Point, c.Point) == Orientation.CCW))
                {
                    Fill(tcx, b);
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
}

