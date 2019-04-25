namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    internal class Trapezoid
    {
        public TrueSync.Physics2D.Edge Bottom;
        public bool Inside;
        public Point LeftPoint;
        public Trapezoid LowerLeft;
        public Trapezoid LowerRight;
        public Point RightPoint;
        public TrueSync.Physics2D.Sink Sink;
        public TrueSync.Physics2D.Edge Top;
        public Trapezoid UpperLeft;
        public Trapezoid UpperRight;

        public Trapezoid(Point leftPoint, Point rightPoint, TrueSync.Physics2D.Edge top, TrueSync.Physics2D.Edge bottom)
        {
            this.LeftPoint = leftPoint;
            this.RightPoint = rightPoint;
            this.Top = top;
            this.Bottom = bottom;
            this.UpperLeft = null;
            this.UpperRight = null;
            this.LowerLeft = null;
            this.LowerRight = null;
            this.Inside = true;
            this.Sink = null;
        }

        public void AddPoints()
        {
            if (this.LeftPoint != this.Bottom.P)
            {
                this.Bottom.AddMpoint(this.LeftPoint);
            }
            if (this.RightPoint != this.Bottom.Q)
            {
                this.Bottom.AddMpoint(this.RightPoint);
            }
            if (this.LeftPoint != this.Top.P)
            {
                this.Top.AddMpoint(this.LeftPoint);
            }
            if (this.RightPoint != this.Top.Q)
            {
                this.Top.AddMpoint(this.RightPoint);
            }
        }

        public bool Contains(Point point)
        {
            return ((((point.X > this.LeftPoint.X) && (point.X < this.RightPoint.X)) && this.Top.IsAbove(point)) && this.Bottom.IsBelow(point));
        }

        public List<Point> GetVertices()
        {
            return new List<Point>(4) { this.LineIntersect(this.Top, this.LeftPoint.X), this.LineIntersect(this.Bottom, this.LeftPoint.X), this.LineIntersect(this.Bottom, this.RightPoint.X), this.LineIntersect(this.Top, this.RightPoint.X) };
        }

        private Point LineIntersect(TrueSync.Physics2D.Edge edge, FP x)
        {
            return new Point(x, (edge.Slope * x) + edge.B);
        }

        public void TrimNeighbors()
        {
            if (this.Inside)
            {
                this.Inside = false;
                if (this.UpperLeft > null)
                {
                    this.UpperLeft.TrimNeighbors();
                }
                if (this.LowerLeft > null)
                {
                    this.LowerLeft.TrimNeighbors();
                }
                if (this.UpperRight > null)
                {
                    this.UpperRight.TrimNeighbors();
                }
                if (this.LowerRight > null)
                {
                    this.LowerRight.TrimNeighbors();
                }
            }
        }

        public void UpdateLeft(Trapezoid ul, Trapezoid ll)
        {
            this.UpperLeft = ul;
            if (ul > null)
            {
                ul.UpperRight = this;
            }
            this.LowerLeft = ll;
            if (ll > null)
            {
                ll.LowerRight = this;
            }
        }

        public void UpdateLeftRight(Trapezoid ul, Trapezoid ll, Trapezoid ur, Trapezoid lr)
        {
            this.UpperLeft = ul;
            if (ul > null)
            {
                ul.UpperRight = this;
            }
            this.LowerLeft = ll;
            if (ll > null)
            {
                ll.LowerRight = this;
            }
            this.UpperRight = ur;
            if (ur > null)
            {
                ur.UpperLeft = this;
            }
            this.LowerRight = lr;
            if (lr > null)
            {
                lr.LowerLeft = this;
            }
        }

        public void UpdateRight(Trapezoid ur, Trapezoid lr)
        {
            this.UpperRight = ur;
            if (ur > null)
            {
                ur.UpperLeft = this;
            }
            this.LowerRight = lr;
            if (lr > null)
            {
                lr.LowerLeft = this;
            }
        }
    }
}

