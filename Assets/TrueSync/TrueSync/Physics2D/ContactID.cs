namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit)]
    public struct ContactID
    {
        [FieldOffset(0)]
        public ContactFeature Features;
        [FieldOffset(0)]
        public uint Key;
    }
}

