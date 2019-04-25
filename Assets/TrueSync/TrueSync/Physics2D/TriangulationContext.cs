// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.TriangulationContext
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TrueSync.Physics2D
{
    internal abstract class TriangulationContext
    {
        public readonly List<TriangulationPoint> Points = new List<TriangulationPoint>(200);
        public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();

        public TriangulationContext()
        {
            this.Terminated = false;
        }

        public TriangulationMode TriangulationMode { get; protected set; }

        public Triangulatable Triangulatable { get; private set; }

        public bool WaitUntilNotified { get; private set; }

        public bool Terminated { get; set; }

        public int StepCount { get; private set; }

        public virtual bool IsDebugEnabled { get; protected set; }

        public void Done()
        {
            this.StepCount = this.StepCount + 1;
        }

        public virtual void PrepareTriangulation(Triangulatable t)
        {
            this.Triangulatable = t;
            this.TriangulationMode = t.TriangulationMode;
            t.PrepareTriangulation(this);
        }

        public abstract TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b);

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Update(string message)
        {
        }

        public virtual void Clear()
        {
            this.Points.Clear();
            this.Terminated = false;
            this.StepCount = 0;
        }
    }
}
