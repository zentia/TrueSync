using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class TrapezoidalMap
	{
		public HashSet<Trapezoid> Map;

		private Edge _bCross;

		private Edge _cross;

		private FP _margin;

		public TrapezoidalMap()
		{
			this.Map = new HashSet<Trapezoid>();
			this._margin = 50f;
			this._bCross = null;
			this._cross = null;
		}

		public void Clear()
		{
			this._bCross = null;
			this._cross = null;
		}

		public Trapezoid[] Case1(Trapezoid t, Edge e)
		{
			Trapezoid[] array = new Trapezoid[]
			{
				new Trapezoid(t.LeftPoint, e.P, t.Top, t.Bottom),
				new Trapezoid(e.P, e.Q, t.Top, e),
				new Trapezoid(e.P, e.Q, e, t.Bottom),
				new Trapezoid(e.Q, t.RightPoint, t.Top, t.Bottom)
			};
			array[0].UpdateLeft(t.UpperLeft, t.LowerLeft);
			array[1].UpdateLeftRight(array[0], null, array[3], null);
			array[2].UpdateLeftRight(null, array[0], null, array[3]);
			array[3].UpdateRight(t.UpperRight, t.LowerRight);
			return array;
		}

		public Trapezoid[] Case2(Trapezoid t, Edge e)
		{
			bool flag = e.Q.X == t.RightPoint.X;
			Point rightPoint;
			if (flag)
			{
				rightPoint = e.Q;
			}
			else
			{
				rightPoint = t.RightPoint;
			}
			Trapezoid[] array = new Trapezoid[]
			{
				new Trapezoid(t.LeftPoint, e.P, t.Top, t.Bottom),
				new Trapezoid(e.P, rightPoint, t.Top, e),
				new Trapezoid(e.P, rightPoint, e, t.Bottom)
			};
			array[0].UpdateLeft(t.UpperLeft, t.LowerLeft);
			array[1].UpdateLeftRight(array[0], null, t.UpperRight, null);
			array[2].UpdateLeftRight(null, array[0], null, t.LowerRight);
			this._bCross = t.Bottom;
			this._cross = t.Top;
			e.Above = array[1];
			e.Below = array[2];
			return array;
		}

		public Trapezoid[] Case3(Trapezoid t, Edge e)
		{
			bool flag = e.P.X == t.LeftPoint.X;
			Point leftPoint;
			if (flag)
			{
				leftPoint = e.P;
			}
			else
			{
				leftPoint = t.LeftPoint;
			}
			bool flag2 = e.Q.X == t.RightPoint.X;
			Point rightPoint;
			if (flag2)
			{
				rightPoint = e.Q;
			}
			else
			{
				rightPoint = t.RightPoint;
			}
			Trapezoid[] array = new Trapezoid[2];
			bool flag3 = this._cross == t.Top;
			if (flag3)
			{
				array[0] = t.UpperLeft;
				array[0].UpdateRight(t.UpperRight, null);
				array[0].RightPoint = rightPoint;
			}
			else
			{
				array[0] = new Trapezoid(leftPoint, rightPoint, t.Top, e);
				array[0].UpdateLeftRight(t.UpperLeft, e.Above, t.UpperRight, null);
			}
			bool flag4 = this._bCross == t.Bottom;
			if (flag4)
			{
				array[1] = t.LowerLeft;
				array[1].UpdateRight(null, t.LowerRight);
				array[1].RightPoint = rightPoint;
			}
			else
			{
				array[1] = new Trapezoid(leftPoint, rightPoint, e, t.Bottom);
				array[1].UpdateLeftRight(e.Below, t.LowerLeft, null, t.LowerRight);
			}
			this._bCross = t.Bottom;
			this._cross = t.Top;
			e.Above = array[0];
			e.Below = array[1];
			return array;
		}

		public Trapezoid[] Case4(Trapezoid t, Edge e)
		{
			bool flag = e.P.X == t.LeftPoint.X;
			Point leftPoint;
			if (flag)
			{
				leftPoint = e.P;
			}
			else
			{
				leftPoint = t.LeftPoint;
			}
			Trapezoid[] array = new Trapezoid[3];
			bool flag2 = this._cross == t.Top;
			if (flag2)
			{
				array[0] = t.UpperLeft;
				array[0].RightPoint = e.Q;
			}
			else
			{
				array[0] = new Trapezoid(leftPoint, e.Q, t.Top, e);
				array[0].UpdateLeft(t.UpperLeft, e.Above);
			}
			bool flag3 = this._bCross == t.Bottom;
			if (flag3)
			{
				array[1] = t.LowerLeft;
				array[1].RightPoint = e.Q;
			}
			else
			{
				array[1] = new Trapezoid(leftPoint, e.Q, e, t.Bottom);
				array[1].UpdateLeft(e.Below, t.LowerLeft);
			}
			array[2] = new Trapezoid(e.Q, t.RightPoint, t.Top, t.Bottom);
			array[2].UpdateLeftRight(array[0], array[1], t.UpperRight, t.LowerRight);
			return array;
		}

		public Trapezoid BoundingBox(List<Edge> edges)
		{
			Point point = edges[0].P + this._margin;
			Point point2 = edges[0].Q - this._margin;
			foreach (Edge current in edges)
			{
				bool flag = current.P.X > point.X;
				if (flag)
				{
					point = new Point(current.P.X + this._margin, point.Y);
				}
				bool flag2 = current.P.Y > point.Y;
				if (flag2)
				{
					point = new Point(point.X, current.P.Y + this._margin);
				}
				bool flag3 = current.Q.X > point.X;
				if (flag3)
				{
					point = new Point(current.Q.X + this._margin, point.Y);
				}
				bool flag4 = current.Q.Y > point.Y;
				if (flag4)
				{
					point = new Point(point.X, current.Q.Y + this._margin);
				}
				bool flag5 = current.P.X < point2.X;
				if (flag5)
				{
					point2 = new Point(current.P.X - this._margin, point2.Y);
				}
				bool flag6 = current.P.Y < point2.Y;
				if (flag6)
				{
					point2 = new Point(point2.X, current.P.Y - this._margin);
				}
				bool flag7 = current.Q.X < point2.X;
				if (flag7)
				{
					point2 = new Point(current.Q.X - this._margin, point2.Y);
				}
				bool flag8 = current.Q.Y < point2.Y;
				if (flag8)
				{
					point2 = new Point(point2.X, current.Q.Y - this._margin);
				}
			}
			Edge edge = new Edge(new Point(point2.X, point.Y), new Point(point.X, point.Y));
			Edge edge2 = new Edge(new Point(point2.X, point2.Y), new Point(point.X, point2.Y));
			Point p = edge2.P;
			Point q = edge.Q;
			return new Trapezoid(p, q, edge, edge2);
		}
	}
}
