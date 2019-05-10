using System;

namespace TrueSync.Physics2D
{
	public struct Manifold
	{
		public TSVector2 LocalNormal;

		public TSVector2 LocalPoint;

		public int PointCount;

		public FixedArray2<ManifoldPoint> Points;

		public ManifoldType Type;
	}
}
