using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal interface Triangulatable
	{
		IList<TriangulationPoint> Points
		{
			get;
		}

		IList<DelaunayTriangle> Triangles
		{
			get;
		}

		TriangulationMode TriangulationMode
		{
			get;
		}

		void PrepareTriangulation(TriangulationContext tcx);

		void AddTriangle(DelaunayTriangle t);

		void AddTriangles(IEnumerable<DelaunayTriangle> list);

		void ClearTriangles();
	}
}
