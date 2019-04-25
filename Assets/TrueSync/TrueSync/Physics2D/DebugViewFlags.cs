namespace TrueSync.Physics2D
{
    using System;

    [Flags]
    public enum DebugViewFlags
    {
        AABB = 4,
        CenterOfMass = 0x10,
        ContactNormals = 0x80,
        ContactPoints = 0x40,
        Controllers = 0x400,
        DebugPanel = 0x20,
        Joint = 2,
        PerformanceGraph = 0x200,
        PolygonPoints = 0x100,
        Shape = 1
    }
}

