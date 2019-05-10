using System;

namespace TrueSync.Physics2D
{
	public struct Mat22
	{
		public TSVector2 ex;

		public TSVector2 ey;

		public Mat22 Inverse
		{
			get
			{
				FP x = this.ex.x;
				FP x2 = this.ey.x;
				FP y = this.ex.y;
				FP y2 = this.ey.y;
				FP fP = x * y2 - x2 * y;
				bool flag = fP != 0f;
				if (flag)
				{
					fP = 1f / fP;
				}
				Mat22 result = default(Mat22);
				result.ex.x = fP * y2;
				result.ex.y = -fP * y;
				result.ey.x = -fP * x2;
				result.ey.y = fP * x;
				return result;
			}
		}

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
			FP x2 = this.ey.x;
			FP y = this.ex.y;
			FP y2 = this.ey.y;
			FP fP = x * y2 - x2 * y;
			bool flag = fP != 0f;
			if (flag)
			{
				fP = 1f / fP;
			}
			return new TSVector2(fP * (y2 * b.x - x2 * b.y), fP * (x * b.y - y * b.x));
		}

		public static void Add(ref Mat22 A, ref Mat22 B, out Mat22 R)
		{
			R.ex = A.ex + B.ex;
			R.ey = A.ey + B.ey;
		}
	}
}
