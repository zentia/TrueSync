namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal abstract class TriangulationContext
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IsDebugEnabled>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <StepCount>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <Terminated>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.Triangulatable <Triangulatable>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.TriangulationMode <TriangulationMode>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <WaitUntilNotified>k__BackingField;
        public readonly List<TriangulationPoint> Points = new List<TriangulationPoint>(200);
        public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();

        public TriangulationContext()
        {
            this.Terminated = false;
        }

        public virtual void Clear()
        {
            this.Points.Clear();
            this.Terminated = false;
            this.StepCount = 0;
        }

        public void Done()
        {
            int stepCount = this.StepCount;
            this.StepCount = stepCount + 1;
        }

        public abstract TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b);
        public virtual void PrepareTriangulation(TrueSync.Physics2D.Triangulatable t)
        {
            this.Triangulatable = t;
            this.TriangulationMode = t.TriangulationMode;
            t.PrepareTriangulation(this);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Update(string message)
        {
        }

        public virtual bool IsDebugEnabled { get; protected set; }

        public int StepCount { get; private set; }

        public bool Terminated { get; set; }

        public TrueSync.Physics2D.Triangulatable Triangulatable { get; private set; }

        public TrueSync.Physics2D.TriangulationMode TriangulationMode { get; protected set; }

        public bool WaitUntilNotified { get; private set; }
    }
}

