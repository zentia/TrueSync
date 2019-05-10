using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class PointSet : Triangulatable
	{
		public IList<TriangulationPoint> Points
		{
			get;
			private set;
		}

		public IList<DelaunayTriangle> Triangles
		{
			get;
			private set;
		}

		public virtual TriangulationMode TriangulationMode
		{
			get
			{
				return TriangulationMode.Unconstrained;
			}
		}

		public PointSet(List<TriangulationPoint> points)
		{
			this.Points = new List<TriangulationPoint>(points);
		}

		public void AddTriangle(DelaunayTriangle t)
		{
			this.Triangles.Add(t);
		}

		public void AddTriangles(IEnumerable<DelaunayTriangle> list)
		{
			foreach (DelaunayTriangle current in list)
			{
				this.Triangles.Add(current);
			}
		}

		public void ClearTriangles()
		{
			this.Triangles.Clear();
		}

		public virtual void PrepareTriangulation(TriangulationContext tcx)
		{
			bool flag = this.Triangles == null;
			if (flag)
			{
				this.Triangles = new List<DelaunayTriangle>(this.Points.Count);
			}
			else
			{
				this.Triangles.Clear();
			}
			tcx.Points.AddRange(this.Points);
		}
	}
}
