namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
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
            xfb = new Transform();
            xfb.p.x = ((1f - beta) * this.C0.x) + (beta * this.C.x);
            xfb.p.y = ((1f - beta) * this.C0.y) + (beta * this.C.y);
            FP angle = ((1f - beta) * this.A0) + (beta * this.A);
            xfb.q.Set(angle);
            xfb.p -= MathUtils.Mul(xfb.q, this.LocalCenter);
        }

        public void Advance(FP alpha)
        {
            Debug.Assert(this.Alpha0 < 1f);
            FP fp = (alpha - this.Alpha0) / (1f - this.Alpha0);
            this.C0 += fp * (this.C - this.C0);
            this.A0 += fp * (this.A - this.A0);
            this.Alpha0 = alpha;
        }

        public void Normalize()
        {
            FP fp = MathHelper.TwoPi * FP.Floor(this.A0 / MathHelper.TwoPi);
            this.A0 -= fp;
            this.A -= fp;
        }
    }
}

