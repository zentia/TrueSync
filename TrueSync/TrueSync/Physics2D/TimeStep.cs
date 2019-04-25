namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct TimeStep
    {
        public FP dt;
        public FP dtRatio;
        public FP inv_dt;
    }
}

