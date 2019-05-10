using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal class MonotoneMountain
	{
		private static readonly FP PiSlop = 3.1f;

		public List<List<Point>> Triangles;

		private HashSet<Point> _convexPoints;

		private Point _head;

		private List<Point> _monoPoly;

		private bool _positive;

		private int _size;

		private Point _tail;

		public MonotoneMountain()
		{
			this._size = 0;
			this._tail = null;
			this._head = null;
			this._positive = false;
			this._convexPoints = new HashSet<Point>();
			this._monoPoly = new List<Point>();
			this.Triangles = new List<List<Point>>();
		}

		public void Add(Point point)
		{
			bool flag = this._size == 0;
			if (flag)
			{
				this._head = point;
				this._size = 1;
			}
			else
			{
				bool flag2 = this._size == 1;
				if (flag2)
				{
					this._tail = point;
					this._tail.Prev = this._head;
					this._head.Next = this._tail;
					this._size = 2;
				}
				else
				{
					this._tail.Next = point;
					point.Prev = this._tail;
					this._tail = point;
					this._size++;
				}
			}
		}

		public void Remove(Point point)
		{
			Point next = point.Next;
			Point prev = point.Prev;
			point.Prev.Next = next;
			point.Next.Prev = prev;
			this._size--;
		}

		public void Process()
		{
			this._positive = this.AngleSign();
			this.GenMonoPoly();
			Point next = this._head.Next;
			while (next.Neq(this._tail))
			{
				FP x = this.Angle(next);
				bool flag = x >= MonotoneMountain.PiSlop || x <= -MonotoneMountain.PiSlop || x == 0f;
				if (flag)
				{
					this.Remove(next);
				}
				else
				{
					bool flag2 = this.IsConvex(next);
					if (flag2)
					{
						this._convexPoints.Add(next);
					}
				}
				next = next.Next;
			}
			this.Triangulate();
		}

		private void Triangulate()
		{
			while (this._convexPoints.Count != 0)
			{
				IEnumerator<Point> enumerator = this._convexPoints.GetEnumerator();
				enumerator.MoveNext();
				Point current = enumerator.Current;
				this._convexPoints.Remove(current);
				Point prev = current.Prev;
				Point item = current;
				Point next = current.Next;
				List<Point> list = new List<Point>(3);
				list.Add(prev);
				list.Add(item);
				list.Add(next);
				this.Triangles.Add(list);
				this.Remove(current);
				bool flag = this.Valid(prev);
				if (flag)
				{
					this._convexPoints.Add(prev);
				}
				bool flag2 = this.Valid(next);
				if (flag2)
				{
					this._convexPoints.Add(next);
				}
			}
			Debug.Assert(this._size <= 3, "Triangulation bug, please report");
		}

		private bool Valid(Point p)
		{
			return p.Neq(this._head) && p.Neq(this._tail) && this.IsConvex(p);
		}

		private void GenMonoPoly()
		{
			for (Point point = this._head; point != null; point = point.Next)
			{
				this._monoPoly.Add(point);
			}
		}

		private FP Angle(Point p)
		{
			Point point = p.Next - p;
			Point p2 = p.Prev - p;
			return FP.Atan2(point.Cross(p2), point.Dot(p2));
		}

		private bool AngleSign()
		{
			Point point = this._head.Next - this._head;
			Point p = this._tail - this._head;
			return FP.Atan2(point.Cross(p), point.Dot(p)) >= 0;
		}

		private bool IsConvex(Point p)
		{
			bool flag = this._positive != this.Angle(p) >= 0;
			return !flag;
		}
	}
}
