// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.PointSet
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System.Collections.Generic;

namespace TrueSync.Physics2D
{
    internal class PointSet : Triangulatable
    {
        public PointSet(List<TriangulationPoint> points)
        {
            this.Points = (IList<TriangulationPoint>)new List<TriangulationPoint>((IEnumerable<TriangulationPoint>)points);
        }

        public IList<TriangulationPoint> Points { get; private set; }

        public IList<DelaunayTriangle> Triangles { get; private set; }

        public virtual TriangulationMode TriangulationMode
        {
            get
            {
                return TriangulationMode.Unconstrained;
            }
        }

        public void AddTriangle(DelaunayTriangle t)
        {
            this.Triangles.Add(t);
        }

        public void AddTriangles(IEnumerable<DelaunayTriangle> list)
        {
            foreach (DelaunayTriangle delaunayTriangle in list)
                this.Triangles.Add(delaunayTriangle);
        }

        public void ClearTriangles()
        {
            this.Triangles.Clear();
        }

        public virtual void PrepareTriangulation(TriangulationContext tcx)
        {
            if (this.Triangles == null)
                this.Triangles = (IList<DelaunayTriangle>)new List<DelaunayTriangle>(this.Points.Count);
            else
                this.Triangles.Clear();
            tcx.Points.AddRange((IEnumerable<TriangulationPoint>)this.Points);
        }
    }
}
