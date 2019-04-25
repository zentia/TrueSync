namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FixtureProxy
    {
        public TrueSync.Physics2D.AABB AABB;
        public int ChildIndex;
        public TrueSync.Physics2D.Fixture Fixture;
        public int ProxyId;
    }
}

