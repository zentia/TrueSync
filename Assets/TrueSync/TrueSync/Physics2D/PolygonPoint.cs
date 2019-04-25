namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    internal class PolygonPoint : TriangulationPoint
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PolygonPoint <Next>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private PolygonPoint <Previous>k__BackingField;

        public PolygonPoint(FP x, FP y) : base(x, y)
        {
        }

        public PolygonPoint Next { get; set; }

        public PolygonPoint Previous { get; set; }
    }
}

