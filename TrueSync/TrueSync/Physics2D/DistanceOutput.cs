namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct DistanceOutput
    {
        public FP Distance;
        public int Iterations;
        public TSVector2 PointA;
        public TSVector2 PointB;
    }
}

