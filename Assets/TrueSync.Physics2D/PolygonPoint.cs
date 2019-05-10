using System;

namespace TrueSync.Physics2D
{
	internal class PolygonPoint : TriangulationPoint
	{
		public PolygonPoint Next
		{
			get;
			set;
		}

		public PolygonPoint Previous
		{
			get;
			set;
		}

		public PolygonPoint(FP x, FP y) : base(x, y)
		{
		}
	}
}
