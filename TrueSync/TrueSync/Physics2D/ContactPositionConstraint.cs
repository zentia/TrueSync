namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public sealed class ContactPositionConstraint
    {
        public int indexA;
        public int indexB;
        public FP invIA;
        public FP invIB;
        public FP invMassA;
        public FP invMassB;
        public TSVector2 localCenterA;
        public TSVector2 localCenterB;
        public TSVector2 localNormal;
        public TSVector2 localPoint;
        public TSVector2[] localPoints = new TSVector2[2];
        public int pointCount;
        public FP radiusA;
        public FP radiusB;
        public ManifoldType type;
    }
}

