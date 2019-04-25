namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    internal class AdvancingFrontNode
    {
        public AdvancingFrontNode Next;
        public TriangulationPoint Point;
        public AdvancingFrontNode Prev;
        public DelaunayTriangle Triangle;
        public FP Value;

        public AdvancingFrontNode(TriangulationPoint point)
        {
            this.Point = point;
            this.Value = point.X;
        }

        public bool HasNext
        {
            get
            {
                return (this.Next > null);
            }
        }

        public bool HasPrev
        {
            get
            {
                return (this.Prev > null);
            }
        }
    }
}

