using System;

namespace TrueSync.Physics2D
{
	public struct ManifoldPoint
	{
		public ContactID Id;

		public TSVector2 LocalPoint;

		public FP NormalImpulse;

		public FP TangentImpulse;
	}
}
