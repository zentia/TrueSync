namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct SimplexCache
    {
        public ushort Count;
        public FixedArray3<byte> IndexA;
        public FixedArray3<byte> IndexB;
        public FP Metric;
    }
}

