using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
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
			bool flag = q.X - p.X != 0;
			if (flag)
			{
				this.Slope = (q.Y - p.Y) / (q.X - p.X);
			}
			else
			{
				this.Slope = 0;
			}
			this.B = p.Y - p.X * this.Slope;
			this.Above = null;
			this.Below = null;
			this.MPoints = new HashSet<Point>();
			this.MPoints.Add(p);
			this.MPoints.Add(q);
		}

		public bool IsAbove(Point point)
		{
			return this.P.Orient2D(this.Q, point) < 0;
		}

		public bool IsBelow(Point point)
		{
			return this.P.Orient2D(this.Q, point) > 0;
		}

		public void AddMpoint(Point point)
		{
			foreach (Point current in this.MPoints)
			{
				bool flag = !current.Neq(point);
				if (flag)
				{
					return;
				}
			}
			this.MPoints.Add(point);
		}
	}
}
