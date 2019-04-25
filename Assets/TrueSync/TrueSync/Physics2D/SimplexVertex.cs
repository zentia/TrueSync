namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    internal struct SimplexVertex
    {
        public FP A;
        public int IndexA;
        public int IndexB;
        public TSVector2 W;
        public TSVector2 WA;
        public TSVector2 WB;
    }
}

