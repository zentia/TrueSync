using System;

namespace TrueSync
{
	public class BoxShape : Shape
	{
		internal TSVector size = TSVector.zero;

		internal TSVector halfSize = TSVector.zero;

		public TSVector Size
		{
			get
			{
				return this.size;
			}
			set
			{
				this.size = value;
				this.UpdateShape();
			}
		}

		public BoxShape(TSVector size)
		{
			this.size = size;
			this.UpdateShape();
		}

		public BoxShape(FP length, FP height, FP width)
		{
			this.size.x = length;
			this.size.y = height;
			this.size.z = width;
			this.UpdateShape();
		}

		public override void UpdateShape()
		{
			this.halfSize = this.size * FP.Half;
			base.UpdateShape();
		}

		public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
		{
			TSMatrix tSMatrix;
			TSMath.Absolute(ref orientation, out tSMatrix);
			TSVector max;
			TSVector.Transform(ref this.halfSize, ref tSMatrix, out max);
			box.max = max;
			TSVector.Negate(ref max, out box.min);
		}

		public override void CalculateMassInertia()
		{
			this.mass = this.size.x * this.size.y * this.size.z;
			this.inertia = TSMatrix.Identity;
			this.inertia.M11 = FP.One / (12 * FP.One) * this.mass * (this.size.y * this.size.y + this.size.z * this.size.z);
			this.inertia.M22 = FP.One / (12 * FP.One) * this.mass * (this.size.x * this.size.x + this.size.z * this.size.z);
			this.inertia.M33 = FP.One / (12 * FP.One) * this.mass * (this.size.x * this.size.x + this.size.y * this.size.y);
			this.geomCen = TSVector.zero;
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			result.x = FP.Sign(direction.x) * this.halfSize.x;
			result.y = FP.Sign(direction.y) * this.halfSize.y;
			result.z = FP.Sign(direction.z) * this.halfSize.z;
		}
	}
}
