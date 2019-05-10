using System;
using System.Collections.Generic;
using System.Linq;

namespace TrueSync.Physics2D
{
	internal class Polygon : Triangulatable
	{
		protected List<Polygon> _holes;

		protected PolygonPoint _last;

		protected List<TriangulationPoint> _points = new List<TriangulationPoint>();

		protected List<TriangulationPoint> _steinerPoints;

		protected List<DelaunayTriangle> _triangles;

		public IList<Polygon> Holes
		{
			get
			{
				return this._holes;
			}
		}

		public TriangulationMode TriangulationMode
		{
			get
			{
				return TriangulationMode.Polygon;
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

		public Polygon(IList<PolygonPoint> points)
		{
			bool flag = points.Count < 3;
			if (flag)
			{
				throw new ArgumentException("List has fewer than 3 points", "points");
			}
			bool flag2 = points[0].Equals(points[points.Count - 1]);
			if (flag2)
			{
				points.RemoveAt(points.Count - 1);
			}
			this._points.AddRange(points.Cast<TriangulationPoint>());
		}

		public Polygon(IEnumerable<PolygonPoint> points) : this((points as IList<PolygonPoint>) ?? points.ToArray<PolygonPoint>())
		{
		}

		public Polygon()
		{
		}

		public void AddTriangle(DelaunayTriangle t)
		{
			this._triangles.Add(t);
		}

		public void AddTriangles(IEnumerable<DelaunayTriangle> list)
		{
			this._triangles.AddRange(list);
		}

		public void ClearTriangles()
		{
			bool flag = this._triangles != null;
			if (flag)
			{
				this._triangles.Clear();
			}
		}

		public void PrepareTriangulation(TriangulationContext tcx)
		{
			bool flag = this._triangles == null;
			if (flag)
			{
				this._triangles = new List<DelaunayTriangle>(this._points.Count);
			}
			else
			{
				this._triangles.Clear();
			}
			for (int i = 0; i < this._points.Count - 1; i++)
			{
				tcx.NewConstraint(this._points[i], this._points[i + 1]);
			}
			tcx.NewConstraint(this._points[0], this._points[this._points.Count - 1]);
			tcx.Points.AddRange(this._points);
			bool flag2 = this._holes != null;
			if (flag2)
			{
				foreach (Polygon current in this._holes)
				{
					for (int j = 0; j < current._points.Count - 1; j++)
					{
						tcx.NewConstraint(current._points[j], current._points[j + 1]);
					}
					tcx.NewConstraint(current._points[0], current._points[current._points.Count - 1]);
					tcx.Points.AddRange(current._points);
				}
			}
			bool flag3 = this._steinerPoints != null;
			if (flag3)
			{
				tcx.Points.AddRange(this._steinerPoints);
			}
		}

		public void AddSteinerPoint(TriangulationPoint point)
		{
			bool flag = this._steinerPoints == null;
			if (flag)
			{
				this._steinerPoints = new List<TriangulationPoint>();
			}
			this._steinerPoints.Add(point);
		}

		public void AddSteinerPoints(List<TriangulationPoint> points)
		{
			bool flag = this._steinerPoints == null;
			if (flag)
			{
				this._steinerPoints = new List<TriangulationPoint>();
			}
			this._steinerPoints.AddRange(points);
		}

		public void ClearSteinerPoints()
		{
			bool flag = this._steinerPoints != null;
			if (flag)
			{
				this._steinerPoints.Clear();
			}
		}

		public void AddHole(Polygon poly)
		{
			bool flag = this._holes == null;
			if (flag)
			{
				this._holes = new List<Polygon>();
			}
			this._holes.Add(poly);
		}

		public void InsertPointAfter(PolygonPoint point, PolygonPoint newPoint)
		{
			int num = this._points.IndexOf(point);
			bool flag = num == -1;
			if (flag)
			{
				throw new ArgumentException("Tried to insert a point into a Polygon after a point not belonging to the Polygon", "point");
			}
			newPoint.Next = point.Next;
			newPoint.Previous = point;
			point.Next.Previous = newPoint;
			point.Next = newPoint;
			this._points.Insert(num + 1, newPoint);
		}

		public void AddPoints(IEnumerable<PolygonPoint> list)
		{
			foreach (PolygonPoint current in list)
			{
				current.Previous = this._last;
				bool flag = this._last != null;
				if (flag)
				{
					current.Next = this._last.Next;
					this._last.Next = current;
				}
				this._last = current;
				this._points.Add(current);
			}
			PolygonPoint polygonPoint = (PolygonPoint)this._points[0];
			this._last.Next = polygonPoint;
			polygonPoint.Previous = this._last;
		}

		public void AddPoint(PolygonPoint p)
		{
			p.Previous = this._last;
			p.Next = this._last.Next;
			this._last.Next = p;
			this._points.Add(p);
		}

		public void RemovePoint(PolygonPoint p)
		{
			PolygonPoint next = p.Next;
			PolygonPoint previous = p.Previous;
			previous.Next = next;
			next.Previous = previous;
			this._points.Remove(p);
		}
	}
}
