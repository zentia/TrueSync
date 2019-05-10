using System;

namespace TrueSync
{
	public class CylinderShape : Shape
	{
		internal FP height;

		internal FP radius;

		public FP Height
		{
			get
			{
				return this.height;
			}
			set
			{
				this.height = value;
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

		public CylinderShape(FP height, FP radius)
		{
			this.height = height;
			this.radius = radius;
			this.UpdateShape();
		}

		public override void CalculateMassInertia()
		{
			this.mass = TSMath.Pi * this.radius * this.radius * this.height;
			this.inertia.M11 = FP.One / (4 * FP.One) * this.mass * this.radius * this.radius + FP.One / (12 * FP.One) * this.mass * this.height * this.height;
			this.inertia.M22 = FP.One / (2 * FP.One) * this.mass * this.radius * this.radius;
			this.inertia.M33 = FP.One / (4 * FP.One) * this.mass * this.radius * this.radius + FP.One / (12 * FP.One) * this.mass * this.height * this.height;
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			FP fP = FP.Sqrt(direction.x * direction.x + direction.z * direction.z);
			bool flag = fP > FP.Zero;
			if (flag)
			{
				result.x = direction.x / fP * this.radius;
				result.y = FP.Sign(direction.y) * this.height * FP.Half;
				result.z = direction.z / fP * this.radius;
			}
			else
			{
				result.x = FP.Zero;
				result.y = FP.Sign(direction.y) * this.height * FP.Half;
				result.z = FP.Zero;
			}
		}
	}
}
