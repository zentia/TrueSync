using System;

namespace TrueSync.Physics2D
{
	[Flags]
	public enum DebugViewFlags
	{
		Shape = 1,
		Joint = 2,
		AABB = 4,
		CenterOfMass = 16,
		DebugPanel = 32,
		ContactPoints = 64,
		ContactNormals = 128,
		PolygonPoints = 256,
		PerformanceGraph = 512,
		Controllers = 1024
	}
}
