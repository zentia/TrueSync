using System;

namespace TrueSync.Physics2D
{
	public struct Rot
	{
		public FP s;

		public FP c;

		public Rot(FP angle)
		{
			this.s = FP.Sin(angle);
			this.c = FP.Cos(angle);
		}

		public void Set(FP angle)
		{
			this.s = FP.Sin(angle);
			this.c = FP.Cos(angle);
		}

		public void SetIdentity()
		{
			this.s = 0f;
			this.c = 1f;
		}

		public FP GetAngle()
		{
			return FP.Atan2(this.s, this.c);
		}

		public TSVector2 GetXAxis()
		{
			return new TSVector2(this.c, this.s);
		}

		public TSVector2 GetYAxis()
		{
			return new TSVector2(-this.s, this.c);
		}
	}
}
