using System;

namespace TrueSync.Physics2D
{
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
