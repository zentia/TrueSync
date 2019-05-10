using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public struct Sweep
	{
		public FP A;

		public FP A0;

		public FP Alpha0;

		public TSVector2 C;

		public TSVector2 C0;

		public TSVector2 LocalCenter;

		public void GetTransform(out Transform xfb, FP beta)
		{
			xfb = default(Transform);
			xfb.p.x = (1f - beta) * this.C0.x + beta * this.C.x;
			xfb.p.y = (1f - beta) * this.C0.y + beta * this.C.y;
			FP angle = (1f - beta) * this.A0 + beta * this.A;
			xfb.q.Set(angle);
			xfb.p -= MathUtils.Mul(xfb.q, this.LocalCenter);
		}

		public void Advance(FP alpha)
		{
			Debug.Assert(this.Alpha0 < 1f);
			FP fP = (alpha - this.Alpha0) / (1f - this.Alpha0);
			this.C0 += fP * (this.C - this.C0);
			this.A0 += fP * (this.A - this.A0);
			this.Alpha0 = alpha;
		}

		public void Normalize()
		{
			FP y = MathHelper.TwoPi * FP.Floor(this.A0 / MathHelper.TwoPi);
			this.A0 -= y;
			this.A -= y;
		}
	}
}
