using System;

namespace TrueSync
{
	public class ConeShape : Shape
	{
		internal FP height;

		internal FP radius;

		internal FP sina = FP.Zero;

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

		public ConeShape(FP height, FP radius)
		{
			this.height = height;
			this.radius = radius;
			this.UpdateShape();
		}

		public override void UpdateShape()
		{
			this.sina = this.radius / FP.Sqrt(this.radius * this.radius + this.height * this.height);
			base.UpdateShape();
		}

		public override void CalculateMassInertia()
		{
			this.mass = FP.One / (3 * FP.One) * TSMath.Pi * this.radius * this.radius * this.height;
			this.inertia = TSMatrix.Identity;
			this.inertia.M11 = 3 * FP.EN1 / 8 * this.mass * (this.radius * this.radius + 4 * this.height * this.height);
			this.inertia.M22 = 3 * FP.EN1 * this.mass * this.radius * this.radius;
			this.inertia.M33 = 3 * FP.EN1 / 8 * this.mass * (this.radius * this.radius + 4 * this.height * this.height);
			this.geomCen = TSVector.zero;
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			FP fP = FP.Sqrt(direction.x * direction.x + direction.z * direction.z);
			bool flag = direction.y > direction.magnitude * this.sina;
			if (flag)
			{
				result.x = FP.Zero;
				result.y = 2 * FP.One / 3 * this.height;
				result.z = FP.Zero;
			}
			else
			{
				bool flag2 = fP > FP.Zero;
				if (flag2)
				{
					result.x = this.radius * direction.x / fP;
					result.y = -(FP.One / 3) * this.height;
					result.z = this.radius * direction.z / fP;
				}
				else
				{
					result.x = FP.Zero;
					result.y = -(FP.One / 3) * this.height;
					result.z = FP.Zero;
				}
			}
		}
	}
}
