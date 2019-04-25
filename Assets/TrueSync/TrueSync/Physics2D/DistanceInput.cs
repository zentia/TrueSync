namespace TrueSync.Physics2D
{
    using System;

    public class DistanceInput
    {
        public DistanceProxy ProxyA = new DistanceProxy();
        public DistanceProxy ProxyB = new DistanceProxy();
        public Transform TransformA;
        public Transform TransformB;
        public bool UseRadii;
    }
}

