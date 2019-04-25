namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public sealed class ContactVelocityConstraint
    {
        public int contactIndex;
        public FP friction;
        public int indexA;
        public int indexB;
        public FP invIA;
        public FP invIB;
        public FP invMassA;
        public FP invMassB;
        public Mat22 K;
        public TSVector2 normal;
        public Mat22 normalMass;
        public int pointCount;
        public VelocityConstraintPoint[] points = new VelocityConstraintPoint[2];
        public FP restitution;
        public FP tangentSpeed;

        public ContactVelocityConstraint()
        {
            for (int i = 0; i < 2; i++)
            {
                this.points[i] = new VelocityConstraintPoint();
            }
        }
    }
}

