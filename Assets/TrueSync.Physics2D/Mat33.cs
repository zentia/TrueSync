using Microsoft.Xna.Framework;
using System;

namespace TrueSync.Physics2D
{
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
			FP fP = Vector3.Dot(this.ex, Vector3.Cross(this.ey, this.ez));
			bool flag = fP != 0f;
			if (flag)
			{
				fP = 1f / fP;
			}
			return new Vector3(fP * Vector3.Dot(b, Vector3.Cross(this.ey, this.ez)), fP * Vector3.Dot(this.ex, Vector3.Cross(b, this.ez)), fP * Vector3.Dot(this.ex, Vector3.Cross(this.ey, b)));
		}

		public TSVector2 Solve22(TSVector2 b)
		{
			FP x = this.ex.X;
			FP x2 = this.ey.X;
			FP y = this.ex.Y;
			FP y2 = this.ey.Y;
			FP fP = x * y2 - x2 * y;
			bool flag = fP != 0f;
			if (flag)
			{
				fP = 1f / fP;
			}
			return new TSVector2(fP * (y2 * b.x - x2 * b.y), fP * (x * b.y - y * b.x));
		}

		public void GetInverse22(ref Mat33 M)
		{
			FP x = this.ex.X;
			FP x2 = this.ey.X;
			FP y = this.ex.Y;
			FP y2 = this.ey.Y;
			FP fP = x * y2 - x2 * y;
			bool flag = fP != 0f;
			if (flag)
			{
				fP = 1f / fP;
			}
			M.ex.X = fP * y2;
			M.ey.X = -fP * x2;
			M.ex.Z = 0f;
			M.ex.Y = -fP * y;
			M.ey.Y = fP * x;
			M.ey.Z = 0f;
			M.ez.X = 0f;
			M.ez.Y = 0f;
			M.ez.Z = 0f;
		}

		public void GetSymInverse33(ref Mat33 M)
		{
			FP fP = MathUtils.Dot(this.ex, MathUtils.Cross(this.ey, this.ez));
			bool flag = fP != 0f;
			if (flag)
			{
				fP = 1f / fP;
			}
			FP x = this.ex.X;
			FP x2 = this.ey.X;
			FP x3 = this.ez.X;
			FP y = this.ey.Y;
			FP y2 = this.ez.Y;
			FP z = this.ez.Z;
			M.ex.X = fP * (y * z - y2 * y2);
			M.ex.Y = fP * (x3 * y2 - x2 * z);
			M.ex.Z = fP * (x2 * y2 - x3 * y);
			M.ey.X = M.ex.Y;
			M.ey.Y = fP * (x * z - x3 * x3);
			M.ey.Z = fP * (x3 * x2 - x * y2);
			M.ez.X = M.ex.Z;
			M.ez.Y = M.ey.Z;
			M.ez.Z = fP * (x * y - x2 * x2);
		}
	}
}
