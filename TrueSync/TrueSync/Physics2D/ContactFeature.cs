namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ContactFeature
    {
        public byte IndexA;
        public byte IndexB;
        public byte TypeA;
        public byte TypeB;
    }
}

