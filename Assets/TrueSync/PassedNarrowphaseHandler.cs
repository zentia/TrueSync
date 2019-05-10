using System;

namespace TrueSync
{
	public delegate bool PassedNarrowphaseHandler(RigidBody body1, RigidBody body2, ref TSVector point, ref TSVector normal, FP penetration);
}
