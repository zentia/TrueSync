namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct EPAxis
    {
        public int Index;
        public FP Separation;
        public EPAxisType Type;
    }
}

