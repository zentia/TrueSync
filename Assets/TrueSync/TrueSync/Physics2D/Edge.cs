namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    internal class Edge
    {
        public Trapezoid Above;
        public FP B;
        public Trapezoid Below;
        public HashSet<Point> MPoints;
        public Point P;
        public Point Q;
        public FP Slope;

        public Edge(Point p, Point q)
        {
            this.P = p;
            this.Q = q;
            if ((q.X - p.X) != 0)
            {
                this.Slope = (q.Y - p.Y) / (q.X - p.X);
            }
            else
            {
                this.Slope = 0;
            }
            this.B = p.Y - (p.X * this.Slope);
            this.Above = null;
            this.Below = null;
            this.MPoints = new HashSet<Point>();
            this.MPoints.Add(p);
            this.MPoints.Add(q);
        }

        public void AddMpoint(Point point)
        {
            foreach (Point point2 in this.MPoints)
            {
                if (!point2.Neq(point))
                {
                    return;
                }
            }
            this.MPoints.Add(point);
        }

        public bool IsAbove(Point point)
        {
            return (this.P.Orient2D(this.Q, point) < 0);
        }

        public bool IsBelow(Point point)
        {
            return (this.P.Orient2D(this.Q, point) > 0);
        }
    }
}

