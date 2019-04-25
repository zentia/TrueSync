namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using TrueSync;

    internal class Triangulator
    {
        private Trapezoid _boundingBox;
        private List<TrueSync.Physics2D.Edge> _edgeList;
        private QueryGraph _queryGraph;
        private FP _sheer = 0.001f;
        private TrapezoidalMap _trapezoidalMap;
        private List<MonotoneMountain> _xMonoPoly;
        public List<Trapezoid> Trapezoids;
        public List<List<Point>> Triangles;

        public Triangulator(List<Point> polyLine, FP sheer)
        {
            this._sheer = sheer;
            this.Triangles = new List<List<Point>>();
            this.Trapezoids = new List<Trapezoid>();
            this._xMonoPoly = new List<MonotoneMountain>();
            this._edgeList = this.InitEdges(polyLine);
            this._trapezoidalMap = new TrapezoidalMap();
            this._boundingBox = this._trapezoidalMap.BoundingBox(this._edgeList);
            this._queryGraph = new QueryGraph(Sink.Isink(this._boundingBox));
            this.Process();
        }

        private void CreateMountains()
        {
            foreach (TrueSync.Physics2D.Edge edge in this._edgeList)
            {
                if (edge.MPoints.Count > 2)
                {
                    MonotoneMountain item = new MonotoneMountain();
                    List<Point> list = new List<Point>(edge.MPoints);
                    list.Sort(<>c.<>9__10_0 ?? (<>c.<>9__10_0 = new Comparison<Point>(<>c.<>9.<CreateMountains>b__10_0)));
                    foreach (Point point in list)
                    {
                        item.Add(point);
                    }
                    item.Process();
                    foreach (List<Point> list2 in item.Triangles)
                    {
                        this.Triangles.Add(list2);
                    }
                    this._xMonoPoly.Add(item);
                }
            }
        }

        private List<TrueSync.Physics2D.Edge> InitEdges(List<Point> points)
        {
            List<TrueSync.Physics2D.Edge> edgeInput = new List<TrueSync.Physics2D.Edge>();
            for (int i = 0; i < (points.Count - 1); i++)
            {
                edgeInput.Add(new TrueSync.Physics2D.Edge(points[i], points[i + 1]));
            }
            edgeInput.Add(new TrueSync.Physics2D.Edge(points[0], points[points.Count - 1]));
            return this.OrderSegments(edgeInput);
        }

        private void MarkOutside(Trapezoid t)
        {
            if ((t.Top == this._boundingBox.Top) || (t.Bottom == this._boundingBox.Bottom))
            {
                t.TrimNeighbors();
            }
        }

        private List<TrueSync.Physics2D.Edge> OrderSegments(List<TrueSync.Physics2D.Edge> edgeInput)
        {
            List<TrueSync.Physics2D.Edge> list = new List<TrueSync.Physics2D.Edge>();
            foreach (TrueSync.Physics2D.Edge edge in edgeInput)
            {
                Point q = this.ShearTransform(edge.P);
                Point p = this.ShearTransform(edge.Q);
                if (q.X > p.X)
                {
                    list.Add(new TrueSync.Physics2D.Edge(p, q));
                }
                else if (q.X < p.X)
                {
                    list.Add(new TrueSync.Physics2D.Edge(q, p));
                }
            }
            Shuffle<TrueSync.Physics2D.Edge>(list);
            return list;
        }

        private void Process()
        {
            foreach (TrueSync.Physics2D.Edge edge in this._edgeList)
            {
                List<Trapezoid> list = this._queryGraph.FollowEdge(edge);
                foreach (Trapezoid trapezoid in list)
                {
                    Trapezoid[] trapezoidArray;
                    this._trapezoidalMap.Map.Remove(trapezoid);
                    bool flag = trapezoid.Contains(edge.P);
                    bool flag2 = trapezoid.Contains(edge.Q);
                    if (flag & flag2)
                    {
                        trapezoidArray = this._trapezoidalMap.Case1(trapezoid, edge);
                        this._queryGraph.Case1(trapezoid.Sink, edge, trapezoidArray);
                    }
                    else if (flag && !flag2)
                    {
                        trapezoidArray = this._trapezoidalMap.Case2(trapezoid, edge);
                        this._queryGraph.Case2(trapezoid.Sink, edge, trapezoidArray);
                    }
                    else if (!flag && !flag2)
                    {
                        trapezoidArray = this._trapezoidalMap.Case3(trapezoid, edge);
                        this._queryGraph.Case3(trapezoid.Sink, edge, trapezoidArray);
                    }
                    else
                    {
                        trapezoidArray = this._trapezoidalMap.Case4(trapezoid, edge);
                        this._queryGraph.Case4(trapezoid.Sink, edge, trapezoidArray);
                    }
                    foreach (Trapezoid trapezoid2 in trapezoidArray)
                    {
                        this._trapezoidalMap.Map.Add(trapezoid2);
                    }
                }
                this._trapezoidalMap.Clear();
            }
            foreach (Trapezoid trapezoid3 in this._trapezoidalMap.Map)
            {
                this.MarkOutside(trapezoid3);
            }
            foreach (Trapezoid trapezoid4 in this._trapezoidalMap.Map)
            {
                if (trapezoid4.Inside)
                {
                    this.Trapezoids.Add(trapezoid4);
                    trapezoid4.AddPoints();
                }
            }
            this.CreateMountains();
        }

        private Point ShearTransform(Point point)
        {
            return new Point(point.X + (this._sheer * point.Y), point.Y);
        }

        private static void Shuffle<T>(IList<T> list)
        {
            TSRandom random = TSRandom.New(0);
            int count = list.Count;
            while (count > 1)
            {
                count--;
                int num2 = random.Next(0, count + 1);
                T local = list[num2];
                list[num2] = list[count];
                list[count] = local;
            }
        }

        [Serializable, CompilerGenerated]
        private sealed class <>c
        {
            public static readonly Triangulator.<>c <>9 = new Triangulator.<>c();
            public static Comparison<Point> <>9__10_0;

            internal int <CreateMountains>b__10_0(Point p1, Point p2)
            {
                return p1.X.CompareTo(p2.X);
            }
        }
    }
}

