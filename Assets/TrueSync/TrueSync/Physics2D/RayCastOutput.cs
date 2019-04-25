namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct RayCastOutput
    {
        public FP Fraction;
        public TSVector2 Normal;
    }
}

