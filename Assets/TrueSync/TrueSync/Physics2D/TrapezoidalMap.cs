namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    internal class TrapezoidalMap
    {
        private TrueSync.Physics2D.Edge _bCross = null;
        private TrueSync.Physics2D.Edge _cross = null;
        private FP _margin = 50f;
        public HashSet<Trapezoid> Map = new HashSet<Trapezoid>();

        public Trapezoid BoundingBox(List<TrueSync.Physics2D.Edge> edges)
        {
            Point point = edges[0].P + this._margin;
            Point point2 = edges[0].Q - this._margin;
            foreach (TrueSync.Physics2D.Edge edge3 in edges)
            {
                if (edge3.P.X > point.X)
                {
                    point = new Point(edge3.P.X + this._margin, point.Y);
                }
                if (edge3.P.Y > point.Y)
                {
                    point = new Point(point.X, edge3.P.Y + this._margin);
                }
                if (edge3.Q.X > point.X)
                {
                    point = new Point(edge3.Q.X + this._margin, point.Y);
                }
                if (edge3.Q.Y > point.Y)
                {
                    point = new Point(point.X, edge3.Q.Y + this._margin);
                }
                if (edge3.P.X < point2.X)
                {
                    point2 = new Point(edge3.P.X - this._margin, point2.Y);
                }
                if (edge3.P.Y < point2.Y)
                {
                    point2 = new Point(point2.X, edge3.P.Y - this._margin);
                }
                if (edge3.Q.X < point2.X)
                {
                    point2 = new Point(edge3.Q.X - this._margin, point2.Y);
                }
                if (edge3.Q.Y < point2.Y)
                {
                    point2 = new Point(point2.X, edge3.Q.Y - this._margin);
                }
            }
            TrueSync.Physics2D.Edge top = new TrueSync.Physics2D.Edge(new Point(point2.X, point.Y), new Point(point.X, point.Y));
            TrueSync.Physics2D.Edge bottom = new TrueSync.Physics2D.Edge(new Point(point2.X, point2.Y), new Point(point.X, point2.Y));
            Point p = bottom.P;
            return new Trapezoid(p, top.Q, top, bottom);
        }

        public Trapezoid[] Case1(Trapezoid t, TrueSync.Physics2D.Edge e)
        {
            Trapezoid[] trapezoidArray = new Trapezoid[] { new Trapezoid(t.LeftPoint, e.P, t.Top, t.Bottom), new Trapezoid(e.P, e.Q, t.Top, e), new Trapezoid(e.P, e.Q, e, t.Bottom), new Trapezoid(e.Q, t.RightPoint, t.Top, t.Bottom) };
            trapezoidArray[0].UpdateLeft(t.UpperLeft, t.LowerLeft);
            trapezoidArray[1].UpdateLeftRight(trapezoidArray[0], null, trapezoidArray[3], null);
            trapezoidArray[2].UpdateLeftRight(null, trapezoidArray[0], null, trapezoidArray[3]);
            trapezoidArray[3].UpdateRight(t.UpperRight, t.LowerRight);
            return trapezoidArray;
        }

        public Trapezoid[] Case2(Trapezoid t, TrueSync.Physics2D.Edge e)
        {
            Point q;
            if (e.Q.X == t.RightPoint.X)
            {
                q = e.Q;
            }
            else
            {
                q = t.RightPoint;
            }
            Trapezoid[] trapezoidArray = new Trapezoid[] { new Trapezoid(t.LeftPoint, e.P, t.Top, t.Bottom), new Trapezoid(e.P, q, t.Top, e), new Trapezoid(e.P, q, e, t.Bottom) };
            trapezoidArray[0].UpdateLeft(t.UpperLeft, t.LowerLeft);
            trapezoidArray[1].UpdateLeftRight(trapezoidArray[0], null, t.UpperRight, null);
            trapezoidArray[2].UpdateLeftRight(null, trapezoidArray[0], null, t.LowerRight);
            this._bCross = t.Bottom;
            this._cross = t.Top;
            e.Above = trapezoidArray[1];
            e.Below = trapezoidArray[2];
            return trapezoidArray;
        }

        public Trapezoid[] Case3(Trapezoid t, TrueSync.Physics2D.Edge e)
        {
            Point p;
            Point q;
            if (e.P.X == t.LeftPoint.X)
            {
                p = e.P;
            }
            else
            {
                p = t.LeftPoint;
            }
            if (e.Q.X == t.RightPoint.X)
            {
                q = e.Q;
            }
            else
            {
                q = t.RightPoint;
            }
            Trapezoid[] trapezoidArray = new Trapezoid[2];
            if (this._cross == t.Top)
            {
                trapezoidArray[0] = t.UpperLeft;
                trapezoidArray[0].UpdateRight(t.UpperRight, null);
                trapezoidArray[0].RightPoint = q;
            }
            else
            {
                trapezoidArray[0] = new Trapezoid(p, q, t.Top, e);
                trapezoidArray[0].UpdateLeftRight(t.UpperLeft, e.Above, t.UpperRight, null);
            }
            if (this._bCross == t.Bottom)
            {
                trapezoidArray[1] = t.LowerLeft;
                trapezoidArray[1].UpdateRight(null, t.LowerRight);
                trapezoidArray[1].RightPoint = q;
            }
            else
            {
                trapezoidArray[1] = new Trapezoid(p, q, e, t.Bottom);
                trapezoidArray[1].UpdateLeftRight(e.Below, t.LowerLeft, null, t.LowerRight);
            }
            this._bCross = t.Bottom;
            this._cross = t.Top;
            e.Above = trapezoidArray[0];
            e.Below = trapezoidArray[1];
            return trapezoidArray;
        }

        public Trapezoid[] Case4(Trapezoid t, TrueSync.Physics2D.Edge e)
        {
            Point p;
            if (e.P.X == t.LeftPoint.X)
            {
                p = e.P;
            }
            else
            {
                p = t.LeftPoint;
            }
            Trapezoid[] trapezoidArray = new Trapezoid[3];
            if (this._cross == t.Top)
            {
                trapezoidArray[0] = t.UpperLeft;
                trapezoidArray[0].RightPoint = e.Q;
            }
            else
            {
                trapezoidArray[0] = new Trapezoid(p, e.Q, t.Top, e);
                trapezoidArray[0].UpdateLeft(t.UpperLeft, e.Above);
            }
            if (this._bCross == t.Bottom)
            {
                trapezoidArray[1] = t.LowerLeft;
                trapezoidArray[1].RightPoint = e.Q;
            }
            else
            {
                trapezoidArray[1] = new Trapezoid(p, e.Q, e, t.Bottom);
                trapezoidArray[1].UpdateLeft(e.Below, t.LowerLeft);
            }
            trapezoidArray[2] = new Trapezoid(e.Q, t.RightPoint, t.Top, t.Bottom);
            trapezoidArray[2].UpdateLeftRight(trapezoidArray[0], trapezoidArray[1], t.UpperRight, t.LowerRight);
            return trapezoidArray;
        }

        public void Clear()
        {
            this._bCross = null;
            this._cross = null;
        }
    }
}

