namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct ClipVertex
    {
        public ContactID ID;
        public TSVector2 V;
    }
}

