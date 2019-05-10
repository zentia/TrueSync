using System;

namespace TrueSync.Physics2D
{
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

		public static Point operator -(Point p1, Point p2)
		{
			return new Point(p1.X - p2.X, p1.Y - p2.Y);
		}

		public static Point operator +(Point p1, Point p2)
		{
			return new Point(p1.X + p2.X, p1.Y + p2.Y);
		}

		public static Point operator -(Point p1, FP f)
		{
			return new Point(p1.X - f, p1.Y - f);
		}

		public static Point operator +(Point p1, FP f)
		{
			return new Point(p1.X + f, p1.Y + f);
		}

		public FP Cross(Point p)
		{
			return this.X * p.Y - this.Y * p.X;
		}

		public FP Dot(Point p)
		{
			return this.X * p.X + this.Y * p.Y;
		}

		public bool Neq(Point p)
		{
			return p.X != this.X || p.Y != this.Y;
		}

		public FP Orient2D(Point pb, Point pc)
		{
			FP x = this.X - pc.X;
			FP y = pb.X - pc.X;
			FP x2 = this.Y - pc.Y;
			FP y2 = pb.Y - pc.Y;
			return x * y2 - x2 * y;
		}
	}
}
