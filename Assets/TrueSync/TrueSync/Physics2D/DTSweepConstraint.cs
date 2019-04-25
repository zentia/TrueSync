namespace TrueSync.Physics2D
{
    using System;

    internal class DTSweepConstraint : TriangulationConstraint
    {
        public DTSweepConstraint(TriangulationPoint p1, TriangulationPoint p2)
        {
            base.P = p1;
            base.Q = p2;
            if (p1.Y > p2.Y)
            {
                base.Q = p1;
                base.P = p2;
            }
            else if (p1.Y == p2.Y)
            {
                if (p1.X > p2.X)
                {
                    base.Q = p1;
                    base.P = p2;
                }
                else if (p1.X == p2.X)
                {
                }
            }
            base.Q.AddEdge(this);
        }
    }
}

