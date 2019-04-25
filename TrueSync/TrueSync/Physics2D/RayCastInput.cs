namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct RayCastInput
    {
        public FP MaxFraction;
        public TSVector2 Point1;
        public TSVector2 Point2;
    }
}

