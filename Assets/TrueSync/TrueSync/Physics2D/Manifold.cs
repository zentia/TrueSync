namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct Manifold
    {
        public TSVector2 LocalNormal;
        public TSVector2 LocalPoint;
        public int PointCount;
        public FixedArray2<ManifoldPoint> Points;
        public ManifoldType Type;
    }
}

