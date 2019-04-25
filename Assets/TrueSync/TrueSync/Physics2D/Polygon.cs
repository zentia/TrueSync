namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal class Polygon : Triangulatable
    {
        protected List<Polygon> _holes;
        protected PolygonPoint _last;
        protected List<TriangulationPoint> _points;
        protected List<TriangulationPoint> _steinerPoints;
        protected List<DelaunayTriangle> _triangles;

        public Polygon()
        {
            this._points = new List<TriangulationPoint>();
        }

        public Polygon(IEnumerable<PolygonPoint> points) : this((points as IList<PolygonPoint>) ?? points.ToArray<PolygonPoint>())
        {
        }

        public Polygon(IList<PolygonPoint> points)
        {
            this._points = new List<TriangulationPoint>();
            if (points.Count < 3)
            {
                throw new ArgumentException("List has fewer than 3 points", "points");
            }
            if (points[0].Equals(points[points.Count - 1]))
            {
                points.RemoveAt(points.Count - 1);
            }
            this._points.AddRange(points.Cast<TriangulationPoint>());
        }

        public void AddHole(Polygon poly)
        {
            if (this._holes == null)
            {
                this._holes = new List<Polygon>();
            }
            this._holes.Add(poly);
        }

        public void AddPoint(PolygonPoint p)
        {
            p.Previous = this._last;
            p.Next = this._last.Next;
            this._last.Next = p;
            this._points.Add(p);
        }

        public void AddPoints(IEnumerable<PolygonPoint> list)
        {
            foreach (PolygonPoint point2 in list)
            {
                point2.Previous = this._last;
                if (this._last > null)
                {
                    point2.Next = this._last.Next;
                    this._last.Next = point2;
                }
                this._last = point2;
                this._points.Add(point2);
            }
            PolygonPoint point = (PolygonPoint) this._points[0];
            this._last.Next = point;
            point.Previous = this._last;
        }

        public void AddSteinerPoint(TriangulationPoint point)
        {
            if (this._steinerPoints == null)
            {
                this._steinerPoints = new List<TriangulationPoint>();
            }
            this._steinerPoints.Add(point);
        }

        public void AddSteinerPoints(List<TriangulationPoint> points)
        {
            if (this._steinerPoints == null)
            {
                this._steinerPoints = new List<TriangulationPoint>();
            }
            this._steinerPoints.AddRange(points);
        }

        public void AddTriangle(DelaunayTriangle t)
        {
            this._triangles.Add(t);
        }

        public void AddTriangles(IEnumerable<DelaunayTriangle> list)
        {
            this._triangles.AddRange(list);
        }

        public void ClearSteinerPoints()
        {
            if (this._steinerPoints > null)
            {
                this._steinerPoints.Clear();
            }
        }

        public void ClearTriangles()
        {
            if (this._triangles > null)
            {
                this._triangles.Clear();
            }
        }

        public void InsertPointAfter(PolygonPoint point, PolygonPoint newPoint)
        {
            int index = this._points.IndexOf(point);
            if (index == -1)
            {
                throw new ArgumentException("Tried to insert a point into a Polygon after a point not belonging to the Polygon", "point");
            }
            newPoint.Next = point.Next;
            newPoint.Previous = point;
            point.Next.Previous = newPoint;
            point.Next = newPoint;
            this._points.Insert(index + 1, newPoint);
        }

        public void PrepareTriangulation(TriangulationContext tcx)
        {
            if (this._triangles == null)
            {
                this._triangles = new List<DelaunayTriangle>(this._points.Count);
            }
            else
            {
                this._triangles.Clear();
            }
            for (int i = 0; i < (this._points.Count - 1); i++)
            {
                tcx.NewConstraint(this._points[i], this._points[i + 1]);
            }
            tcx.NewConstraint(this._points[0], this._points[this._points.Count - 1]);
            tcx.Points.AddRange(this._points);
            if (this._holes > null)
            {
                foreach (Polygon polygon in this._holes)
                {
                    for (int j = 0; j < (polygon._points.Count - 1); j++)
                    {
                        tcx.NewConstraint(polygon._points[j], polygon._points[j + 1]);
                    }
                    tcx.NewConstraint(polygon._points[0], polygon._points[polygon._points.Count - 1]);
                    tcx.Points.AddRange(polygon._points);
                }
            }
            if (this._steinerPoints > null)
            {
                tcx.Points.AddRange(this._steinerPoints);
            }
        }

        public void RemovePoint(PolygonPoint p)
        {
            PolygonPoint next = p.Next;
            PolygonPoint previous = p.Previous;
            previous.Next = next;
            next.Previous = previous;
            this._points.Remove(p);
        }

        public IList<Polygon> Holes
        {
            get
            {
                return this._holes;
            }
        }

        public IList<TriangulationPoint> Points
        {
            get
            {
                return this._points;
            }
        }

        public IList<DelaunayTriangle> Triangles
        {
            get
            {
                return this._triangles;
            }
        }

        public TrueSync.Physics2D.TriangulationMode TriangulationMode
        {
            get
            {
                return TrueSync.Physics2D.TriangulationMode.Polygon;
            }
        }
    }
}

