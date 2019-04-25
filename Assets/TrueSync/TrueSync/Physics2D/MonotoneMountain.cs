namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    internal class MonotoneMountain
    {
        private HashSet<Point> _convexPoints = new HashSet<Point>();
        private Point _head = null;
        private List<Point> _monoPoly = new List<Point>();
        private bool _positive = false;
        private int _size = 0;
        private Point _tail = null;
        private static readonly FP PiSlop = 3.1f;
        public List<List<Point>> Triangles = new List<List<Point>>();

        public void Add(Point point)
        {
            if (this._size == 0)
            {
                this._head = point;
                this._size = 1;
            }
            else if (this._size == 1)
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

        private FP Angle(Point p)
        {
            Point point = p.Next - p;
            Point point2 = p.Prev - p;
            return FP.Atan2(point.Cross(point2), point.Dot(point2));
        }

        private bool AngleSign()
        {
            Point point = this._head.Next - this._head;
            Point p = this._tail - this._head;
            return (FP.Atan2(point.Cross(p), point.Dot(p)) >= 0);
        }

        private void GenMonoPoly()
        {
            for (Point point = this._head; point > null; point = point.Next)
            {
                this._monoPoly.Add(point);
            }
        }

        private bool IsConvex(Point p)
        {
            if (this._positive != (this.Angle(p) >= 0))
            {
                return false;
            }
            return true;
        }

        public void Process()
        {
            this._positive = this.AngleSign();
            this.GenMonoPoly();
            for (Point point = this._head.Next; point.Neq(this._tail); point = point.Next)
            {
                FP fp = this.Angle(point);
                if (((fp >= PiSlop) || (fp <= -PiSlop)) || (fp == 0f))
                {
                    this.Remove(point);
                }
                else if (this.IsConvex(point))
                {
                    this._convexPoints.Add(point);
                }
            }
            this.Triangulate();
        }

        public void Remove(Point point)
        {
            Point next = point.Next;
            Point prev = point.Prev;
            point.Prev.Next = next;
            point.Next.Prev = prev;
            this._size--;
        }

        private void Triangulate()
        {
            while (this._convexPoints.Count > 0)
            {
                List<Point> list;
                IEnumerator<Point> enumerator = this._convexPoints.GetEnumerator();
                enumerator.MoveNext();
                Point current = enumerator.Current;
                this._convexPoints.Remove(current);
                Point prev = current.Prev;
                Point point3 = current;
                Point next = current.Next;
                list = new List<Point>(3) {
                    prev,
                    point3,
                    next,
                    list
                };
                this.Remove(current);
                if (this.Valid(prev))
                {
                    this._convexPoints.Add(prev);
                }
                if (this.Valid(next))
                {
                    this._convexPoints.Add(next);
                }
            }
            Debug.Assert(this._size <= 3, "Triangulation bug, please report");
        }

        private bool Valid(Point p)
        {
            return ((p.Neq(this._head) && p.Neq(this._tail)) && this.IsConvex(p));
        }
    }
}

