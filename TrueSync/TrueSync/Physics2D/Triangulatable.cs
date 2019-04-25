namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;

    internal interface Triangulatable
    {
        void AddTriangle(DelaunayTriangle t);
        void AddTriangles(IEnumerable<DelaunayTriangle> list);
        void ClearTriangles();
        void PrepareTriangulation(TriangulationContext tcx);

        IList<TriangulationPoint> Points { get; }

        IList<DelaunayTriangle> Triangles { get; }

        TrueSync.Physics2D.TriangulationMode TriangulationMode { get; }
    }
}

