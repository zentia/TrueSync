namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using TrueSync;

    internal static class CDTDecomposer
    {
        public static List<Vertices> ConvexPartition(Vertices vertices)
        {
            Debug.Assert(vertices.Count > 3);
            Polygon t = new Polygon();
            foreach (TSVector2 vector in vertices)
            {
                t.Points.Add(new TriangulationPoint(vector.x, vector.y));
            }
            if (vertices.Holes > null)
            {
                foreach (Vertices vertices2 in vertices.Holes)
                {
                    Polygon poly = new Polygon();
                    foreach (TSVector2 vector2 in vertices2)
                    {
                        poly.Points.Add(new TriangulationPoint(vector2.x, vector2.y));
                    }
                    t.AddHole(poly);
                }
            }
            DTSweepContext tcx = new DTSweepContext();
            tcx.PrepareTriangulation(t);
            DTSweep.Triangulate(tcx);
            List<Vertices> list = new List<Vertices>();
            foreach (DelaunayTriangle triangle in t.Triangles)
            {
                Vertices item = new Vertices();
                foreach (TriangulationPoint point in triangle.Points)
                {
                    item.Add(new TSVector2(point.X, point.Y));
                }
                list.Add(item);
            }
            return list;
        }
    }
}

