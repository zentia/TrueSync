using System;

namespace TrueSync.Physics2D
{
	public struct FixtureProxy
	{
		public AABB AABB;

		public int ChildIndex;

		public Fixture Fixture;

		public int ProxyId;
	}
}
