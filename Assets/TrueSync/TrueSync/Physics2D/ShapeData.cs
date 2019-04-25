namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    internal struct ShapeData
    {
        public TrueSync.Physics2D.Body Body;
        public FP Max;
        public FP Min;
    }
}

