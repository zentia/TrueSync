namespace TrueSync.Physics2D
{
    using System;

    public sealed class JointEdge
    {
        public TrueSync.Physics2D.Joint Joint;
        public JointEdge Next;
        public Body Other;
        public JointEdge Prev;
    }
}

