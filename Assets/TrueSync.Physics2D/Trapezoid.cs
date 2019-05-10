using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class Trapezoid
	{
		public Edge Bottom;

		public bool Inside;

		public Point LeftPoint;

		public Trapezoid LowerLeft;

		public Trapezoid LowerRight;

		public Point RightPoint;

		public Sink Sink;

		public Edge Top;

		public Trapezoid UpperLeft;

		public Trapezoid UpperRight;

		public Trapezoid(Point leftPoint, Point rightPoint, Edge top, Edge bottom)
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

		public void UpdateLeft(Trapezoid ul, Trapezoid ll)
		{
			this.UpperLeft = ul;
			bool flag = ul != null;
			if (flag)
			{
				ul.UpperRight = this;
			}
			this.LowerLeft = ll;
			bool flag2 = ll != null;
			if (flag2)
			{
				ll.LowerRight = this;
			}
		}

		public void UpdateRight(Trapezoid ur, Trapezoid lr)
		{
			this.UpperRight = ur;
			bool flag = ur != null;
			if (flag)
			{
				ur.UpperLeft = this;
			}
			this.LowerRight = lr;
			bool flag2 = lr != null;
			if (flag2)
			{
				lr.LowerLeft = this;
			}
		}

		public void UpdateLeftRight(Trapezoid ul, Trapezoid ll, Trapezoid ur, Trapezoid lr)
		{
			this.UpperLeft = ul;
			bool flag = ul != null;
			if (flag)
			{
				ul.UpperRight = this;
			}
			this.LowerLeft = ll;
			bool flag2 = ll != null;
			if (flag2)
			{
				ll.LowerRight = this;
			}
			this.UpperRight = ur;
			bool flag3 = ur != null;
			if (flag3)
			{
				ur.UpperLeft = this;
			}
			this.LowerRight = lr;
			bool flag4 = lr != null;
			if (flag4)
			{
				lr.LowerLeft = this;
			}
		}

		public void TrimNeighbors()
		{
			bool inside = this.Inside;
			if (inside)
			{
				this.Inside = false;
				bool flag = this.UpperLeft != null;
				if (flag)
				{
					this.UpperLeft.TrimNeighbors();
				}
				bool flag2 = this.LowerLeft != null;
				if (flag2)
				{
					this.LowerLeft.TrimNeighbors();
				}
				bool flag3 = this.UpperRight != null;
				if (flag3)
				{
					this.UpperRight.TrimNeighbors();
				}
				bool flag4 = this.LowerRight != null;
				if (flag4)
				{
					this.LowerRight.TrimNeighbors();
				}
			}
		}

		public bool Contains(Point point)
		{
			return point.X > this.LeftPoint.X && point.X < this.RightPoint.X && this.Top.IsAbove(point) && this.Bottom.IsBelow(point);
		}

		public List<Point> GetVertices()
		{
			return new List<Point>(4)
			{
				this.LineIntersect(this.Top, this.LeftPoint.X),
				this.LineIntersect(this.Bottom, this.LeftPoint.X),
				this.LineIntersect(this.Bottom, this.RightPoint.X),
				this.LineIntersect(this.Top, this.RightPoint.X)
			};
		}

		private Point LineIntersect(Edge edge, FP x)
		{
			FP y = edge.Slope * x + edge.B;
			return new Point(x, y);
		}

		public void AddPoints()
		{
			bool flag = this.LeftPoint != this.Bottom.P;
			if (flag)
			{
				this.Bottom.AddMpoint(this.LeftPoint);
			}
			bool flag2 = this.RightPoint != this.Bottom.Q;
			if (flag2)
			{
				this.Bottom.AddMpoint(this.RightPoint);
			}
			bool flag3 = this.LeftPoint != this.Top.P;
			if (flag3)
			{
				this.Top.AddMpoint(this.LeftPoint);
			}
			bool flag4 = this.RightPoint != this.Top.Q;
			if (flag4)
			{
				this.Top.AddMpoint(this.RightPoint);
			}
		}
	}
}
