namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct Mat33
    {
        public Vector3 ex;
        public Vector3 ey;
        public Vector3 ez;
        public Mat33(Vector3 c1, Vector3 c2, Vector3 c3)
        {
            this.ex = c1;
            this.ey = c2;
            this.ez = c3;
        }

        public void SetZero()
        {
            this.ex = Vector3.Zero;
            this.ey = Vector3.Zero;
            this.ez = Vector3.Zero;
        }

        public Vector3 Solve33(Vector3 b)
        {
            FP fp = Vector3.Dot(this.ex, Vector3.Cross(this.ey, this.ez));
            if (fp != 0f)
            {
                fp = 1f / fp;
            }
            return new Vector3(fp * Vector3.Dot(b, Vector3.Cross(this.ey, this.ez)), fp * Vector3.Dot(this.ex, Vector3.Cross(b, this.ez)), fp * Vector3.Dot(this.ex, Vector3.Cross(this.ey, b)));
        }

        public TSVector2 Solve22(TSVector2 b)
        {
            FP x = this.ex.X;
            FP fp2 = this.ey.X;
            FP y = this.ex.Y;
            FP fp4 = this.ey.Y;
            FP fp5 = (x * fp4) - (fp2 * y);
            if (fp5 != 0f)
            {
                fp5 = 1f / fp5;
            }
            return new TSVector2(fp5 * ((fp4 * b.x) - (fp2 * b.y)), fp5 * ((x * b.y) - (y * b.x)));
        }

        public void GetInverse22(ref Mat33 M)
        {
            FP x = this.ex.X;
            FP fp2 = this.ey.X;
            FP y = this.ex.Y;
            FP fp4 = this.ey.Y;
            FP fp5 = (x * fp4) - (fp2 * y);
            if (fp5 != 0f)
            {
                fp5 = 1f / fp5;
            }
            M.ex.X = fp5 * fp4;
            M.ey.X = -fp5 * fp2;
            M.ex.Z = 0f;
            M.ex.Y = -fp5 * y;
            M.ey.Y = fp5 * x;
            M.ey.Z = 0f;
            M.ez.X = 0f;
            M.ez.Y = 0f;
            M.ez.Z = 0f;
        }

        public void GetSymInverse33(ref Mat33 M)
        {
            FP fp = MathUtils.Dot(this.ex, MathUtils.Cross(this.ey, this.ez));
            if (fp != 0f)
            {
                fp = 1f / fp;
            }
            FP x = this.ex.X;
            FP fp3 = this.ey.X;
            FP fp4 = this.ez.X;
            FP y = this.ey.Y;
            FP fp6 = this.ez.Y;
            FP z = this.ez.Z;
            M.ex.X = fp * ((y * z) - (fp6 * fp6));
            M.ex.Y = fp * ((fp4 * fp6) - (fp3 * z));
            M.ex.Z = fp * ((fp3 * fp6) - (fp4 * y));
            M.ey.X = M.ex.Y;
            M.ey.Y = fp * ((x * z) - (fp4 * fp4));
            M.ey.Z = fp * ((fp4 * fp3) - (x * fp6));
            M.ez.X = M.ex.Z;
            M.ez.Y = M.ey.Z;
            M.ez.Z = fp * ((x * y) - (fp3 * fp3));
        }
    }
}

