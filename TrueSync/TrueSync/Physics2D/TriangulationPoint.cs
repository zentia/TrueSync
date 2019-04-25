namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    internal class TriangulationPoint
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<DTSweepConstraint> <Edges>k__BackingField;
        public FP X;
        public FP Y;

        public TriangulationPoint(FP x, FP y)
        {
            this.X = x;
            this.Y = y;
        }

        public void AddEdge(DTSweepConstraint e)
        {
            if (this.Edges == null)
            {
                this.Edges = new List<DTSweepConstraint>();
            }
            this.Edges.Add(e);
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { "[", this.X, ",", this.Y, "]" };
            return string.Concat(objArray1);
        }

        public List<DTSweepConstraint> Edges { get; private set; }

        public bool HasEdges
        {
            get
            {
                return (this.Edges > null);
            }
        }

        public FP Xf
        {
            get
            {
                return this.X;
            }
            set
            {
                this.X = value;
            }
        }

        public FP Yf
        {
            get
            {
                return this.Y;
            }
            set
            {
                this.Y = value;
            }
        }
    }
}

