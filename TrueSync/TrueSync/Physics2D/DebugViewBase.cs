namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public abstract class DebugViewBase
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DebugViewFlags <Flags>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.World <World>k__BackingField;

        protected DebugViewBase(TrueSync.Physics2D.World world)
        {
            this.World = world;
        }

        public void AppendFlags(DebugViewFlags flags)
        {
            this.Flags |= flags;
        }

        public abstract void DrawCircle(TSVector2 center, FP radius, FP red, FP blue, FP green);
        public abstract void DrawPolygon(TSVector2[] vertices, int count, FP red, FP blue, FP green, bool closed = true);
        public abstract void DrawSegment(TSVector2 start, TSVector2 end, FP red, FP blue, FP green);
        public abstract void DrawSolidCircle(TSVector2 center, FP radius, TSVector2 axis, FP red, FP blue, FP green);
        public abstract void DrawSolidPolygon(TSVector2[] vertices, int count, FP red, FP blue, FP green);
        public abstract void DrawTransform(ref Transform transform);
        public void RemoveFlags(DebugViewFlags flags)
        {
            this.Flags &= ~flags;
        }

        public DebugViewFlags Flags { get; set; }

        protected TrueSync.Physics2D.World World { get; private set; }
    }
}

