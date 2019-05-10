using System;

namespace TrueSync.Physics2D
{
	public sealed class VelocityConstraintPoint
	{
		public TSVector2 rA;

		public TSVector2 rB;

		public FP normalImpulse;

		public FP tangentImpulse;

		public FP normalMass;

		public FP tangentMass;

		public FP velocityBias;
	}
}
