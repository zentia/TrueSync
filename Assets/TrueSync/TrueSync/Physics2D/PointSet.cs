namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class PointSet : Triangulatable
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IList<TriangulationPoint> <Points>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private IList<DelaunayTriangle> <Triangles>k__BackingField;

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
            foreach (DelaunayTriangle triangle in list)
            {
                this.Triangles.Add(triangle);
            }
        }

        public void ClearTriangles()
        {
            this.Triangles.Clear();
        }

        public virtual void PrepareTriangulation(TriangulationContext tcx)
        {
            if (this.Triangles == null)
            {
                this.Triangles = new List<DelaunayTriangle>(this.Points.Count);
            }
            else
            {
                this.Triangles.Clear();
            }
            tcx.Points.AddRange(this.Points);
        }

        public IList<TriangulationPoint> Points { get; private set; }

        public IList<DelaunayTriangle> Triangles { get; private set; }

        public virtual TrueSync.Physics2D.TriangulationMode TriangulationMode
        {
            get
            {
                return TrueSync.Physics2D.TriangulationMode.Unconstrained;
            }
        }
    }
}

