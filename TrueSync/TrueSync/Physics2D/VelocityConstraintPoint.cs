namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public sealed class VelocityConstraintPoint
    {
        public FP normalImpulse;
        public FP normalMass;
        public TSVector2 rA;
        public TSVector2 rB;
        public FP tangentImpulse;
        public FP tangentMass;
        public FP velocityBias;
    }
}

