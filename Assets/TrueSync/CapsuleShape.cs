using System;

namespace TrueSync
{
	public class CapsuleShape : Shape
	{
		internal FP length;

		internal FP radius;

		public FP Length
		{
			get
			{
				return this.length;
			}
			set
			{
				this.length = value;
				this.UpdateShape();
			}
		}

		public FP Radius
		{
			get
			{
				return this.radius;
			}
			set
			{
				this.radius = value;
				this.UpdateShape();
			}
		}

		public CapsuleShape(FP length, FP radius)
		{
			this.length = length;
			this.radius = radius;
			this.UpdateShape();
		}

		public override void CalculateMassInertia()
		{
			FP y = 3 * FP.One / (4 * FP.One) * TSMath.Pi * this.radius * this.radius * this.radius;
			FP fP = TSMath.Pi * this.radius * this.radius * this.length;
			this.mass = fP + y;
			this.inertia.M11 = FP.One / (4 * FP.One) * fP * this.radius * this.radius + FP.One / (12 * FP.One) * fP * this.length * this.length + 2 * FP.One / (5 * FP.One) * y * this.radius * this.radius + FP.One / (4 * FP.One) * this.length * this.length * y;
			this.inertia.M22 = FP.One / (2 * FP.One) * fP * this.radius * this.radius + 2 * FP.One / (5 * FP.One) * y * this.radius * this.radius;
			this.inertia.M33 = FP.One / (4 * FP.One) * fP * this.radius * this.radius + FP.One / (12 * FP.One) * fP * this.length * this.length + 2 * FP.One / (5 * FP.One) * y * this.radius * this.radius + FP.One / (4 * FP.One) * this.length * this.length * y;
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			FP fP = FP.Sqrt(direction.x * direction.x + direction.z * direction.z);
			bool flag = FP.Abs(direction.y) > FP.Zero;
			if (flag)
			{
				TSVector tSVector;
				TSVector.Normalize(ref direction, out tSVector);
				TSVector.Multiply(ref tSVector, this.radius, out result);
				result.y += FP.Sign(direction.y) * FP.Half * this.length;
			}
			else
			{
				bool flag2 = fP > FP.Zero;
				if (flag2)
				{
					result.x = direction.x / fP * this.radius;
					result.y = FP.Zero;
					result.z = direction.z / fP * this.radius;
				}
				else
				{
					result.x = FP.Zero;
					result.y = FP.Zero;
					result.z = FP.Zero;
				}
			}
		}
	}
}
