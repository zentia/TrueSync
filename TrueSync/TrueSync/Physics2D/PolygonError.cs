namespace TrueSync.Physics2D
{
    using System;

    public enum PolygonError
    {
        NoError,
        InvalidAmountOfVertices,
        NotSimple,
        NotCounterClockWise,
        NotConvex,
        AreaTooSmall,
        SideTooSmall
    }
}

