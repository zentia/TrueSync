namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    internal class Point
    {
        public Point Next;
        public Point Prev;
        public FP X;
        public FP Y;

        public Point(FP x, FP y)
        {
            this.X = x;
            this.Y = y;
            this.Next = null;
            this.Prev = null;
        }

        public FP Cross(Point p)
        {
            return ((this.X * p.Y) - (this.Y * p.X));
        }

        public FP Dot(Point p)
        {
            return ((this.X * p.X) + (this.Y * p.Y));
        }

        public bool Neq(Point p)
        {
            return ((p.X != this.X) || (p.Y != this.Y));
        }

        public static Point operator +(Point p1, FP f)
        {
            return new Point(p1.X + f, p1.Y + f);
        }

        public static Point operator +(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point operator -(Point p1, FP f)
        {
            return new Point(p1.X - f, p1.Y - f);
        }

        public static Point operator -(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }

        public FP Orient2D(Point pb, Point pc)
        {
            FP fp = this.X - pc.X;
            FP fp2 = pb.X - pc.X;
            FP fp3 = this.Y - pc.Y;
            FP fp4 = pb.Y - pc.Y;
            return ((fp * fp4) - (fp3 * fp2));
        }
    }
}

