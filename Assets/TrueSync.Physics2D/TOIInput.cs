using System;

namespace TrueSync.Physics2D
{
	public class TOIInput
	{
		public DistanceProxy ProxyA = new DistanceProxy();

		public DistanceProxy ProxyB = new DistanceProxy();

		public Sweep SweepA;

		public Sweep SweepB;

		public FP TMax;
	}
}
