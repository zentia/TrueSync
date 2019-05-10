using System;

namespace TrueSync.Physics2D
{
	public struct SimplexCache
	{
		public ushort Count;

		public FixedArray3<byte> IndexA;

		public FixedArray3<byte> IndexB;

		public FP Metric;
	}
}
