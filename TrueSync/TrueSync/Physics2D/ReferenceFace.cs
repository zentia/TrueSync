namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct ReferenceFace
    {
        public int i1;
        public int i2;
        public TSVector2 v1;
        public TSVector2 v2;
        public TSVector2 normal;
        public TSVector2 sideNormal1;
        public FP sideOffset1;
        public TSVector2 sideNormal2;
        public FP sideOffset2;
    }
}

