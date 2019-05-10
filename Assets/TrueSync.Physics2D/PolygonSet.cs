using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class PolygonSet
	{
		protected List<Polygon> _polygons = new List<Polygon>();

		public IEnumerable<Polygon> Polygons
		{
			get
			{
				return this._polygons;
			}
		}

		public PolygonSet()
		{
		}

		public PolygonSet(Polygon poly)
		{
			this._polygons.Add(poly);
		}

		public void Add(Polygon p)
		{
			this._polygons.Add(p);
		}
	}
}
