// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.TriangulationPoint
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System.Collections.Generic;

namespace TrueSync.Physics2D
{
    internal class TriangulationPoint
    {
        public FP X;
        public FP Y;

        public TriangulationPoint(FP x, FP y)
        {
            this.X = x;
            this.Y = y;
        }

        public List<DTSweepConstraint> Edges { get; private set; }

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

        public bool HasEdges
        {
            get
            {
                return this.Edges != null;
            }
        }

        public override string ToString()
        {
            return "[" + (object)this.X + "," + (object)this.Y + "]";
        }

        public void AddEdge(DTSweepConstraint e)
        {
            if (this.Edges == null)
                this.Edges = new List<DTSweepConstraint>();
            this.Edges.Add(e);
        }
    }
}
