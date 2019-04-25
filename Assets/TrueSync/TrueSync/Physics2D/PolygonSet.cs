namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;

    internal class PolygonSet
    {
        protected List<Polygon> _polygons;

        public PolygonSet()
        {
            this._polygons = new List<Polygon>();
        }

        public PolygonSet(Polygon poly)
        {
            this._polygons = new List<Polygon>();
            this._polygons.Add(poly);
        }

        public void Add(Polygon p)
        {
            this._polygons.Add(p);
        }

        public IEnumerable<Polygon> Polygons
        {
            get
            {
                return this._polygons;
            }
        }
    }
}

