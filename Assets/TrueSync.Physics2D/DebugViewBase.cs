using System;

namespace TrueSync.Physics2D
{
	public abstract class DebugViewBase
	{
		protected World World
		{
			get;
			private set;
		}

		public DebugViewFlags Flags
		{
			get;
			set;
		}

		protected DebugViewBase(World world)
		{
			this.World = world;
		}

		public void AppendFlags(DebugViewFlags flags)
		{
			this.Flags |= flags;
		}

		public void RemoveFlags(DebugViewFlags flags)
		{
			this.Flags &= ~flags;
		}

		public abstract void DrawPolygon(TSVector2[] vertices, int count, FP red, FP blue, FP green, bool closed = true);

		public abstract void DrawSolidPolygon(TSVector2[] vertices, int count, FP red, FP blue, FP green);

		public abstract void DrawCircle(TSVector2 center, FP radius, FP red, FP blue, FP green);

		public abstract void DrawSolidCircle(TSVector2 center, FP radius, TSVector2 axis, FP red, FP blue, FP green);

		public abstract void DrawSegment(TSVector2 start, TSVector2 end, FP red, FP blue, FP green);

		public abstract void DrawTransform(ref Transform transform);
	}
}
