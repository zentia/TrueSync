namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct Mat22
    {
        public TSVector2 ex;
        public TSVector2 ey;
        public Mat22(TSVector2 c1, TSVector2 c2)
        {
            this.ex = c1;
            this.ey = c2;
        }

        public Mat22(FP a11, FP a12, FP a21, FP a22)
        {
            this.ex = new TSVector2(a11, a21);
            this.ey = new TSVector2(a12, a22);
        }

        public Mat22 Inverse
        {
            get
            {
                FP x = this.ex.x;
                FP fp2 = this.ey.x;
                FP y = this.ex.y;
                FP fp4 = this.ey.y;
                FP fp5 = (x * fp4) - (fp2 * y);
                if (fp5 != 0f)
                {
                    fp5 = 1f / fp5;
                }
                Mat22 mat = new Mat22();
                mat.ex.x = fp5 * fp4;
                mat.ex.y = -fp5 * y;
                mat.ey.x = -fp5 * fp2;
                mat.ey.y = fp5 * x;
                return mat;
            }
        }
        public void Set(TSVector2 c1, TSVector2 c2)
        {
            this.ex = c1;
            this.ey = c2;
        }

        public void SetIdentity()
        {
            this.ex.x = 1f;
            this.ey.x = 0f;
            this.ex.y = 0f;
            this.ey.y = 1f;
        }

        public void SetZero()
        {
            this.ex.x = 0f;
            this.ey.x = 0f;
            this.ex.y = 0f;
            this.ey.y = 0f;
        }

        public TSVector2 Solve(TSVector2 b)
        {
            FP x = this.ex.x;
            FP fp2 = this.ey.x;
            FP y = this.ex.y;
            FP fp4 = this.ey.y;
            FP fp5 = (x * fp4) - (fp2 * y);
            if (fp5 != 0f)
            {
                fp5 = 1f / fp5;
            }
            return new TSVector2(fp5 * ((fp4 * b.x) - (fp2 * b.y)), fp5 * ((x * b.y) - (y * b.x)));
        }

        public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
        {
            R.ex = A.ex + B.ex;
            R.ey = A.ey + B.ey;
        }
    }
}

