namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct ManifoldPoint
    {
        public ContactID Id;
        public TSVector2 LocalPoint;
        public FP NormalImpulse;
        public FP TangentImpulse;
    }
}

