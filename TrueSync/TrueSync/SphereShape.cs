namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public class SphereShape : Shape
    {
        internal FP radius = FP.One;

        public SphereShape(FP radius)
        {
            this.radius = radius;
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            base.mass = (((((4 * FP.One) / (3 * FP.One)) * TSMath.Pi) * this.radius) * this.radius) * this.radius;
            base.inertia = TSMatrix.Identity;
            this.inertia.M11 = (((4 * FP.EN1) * base.mass) * this.radius) * this.radius;
            this.inertia.M22 = (((4 * FP.EN1) * base.mass) * this.radius) * this.radius;
            this.inertia.M33 = (((4 * FP.EN1) * base.mass) * this.radius) * this.radius;
        }

        public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            box.min.x = -this.radius;
            box.min.y = -this.radius;
            box.min.z = -this.radius;
            box.max.x = this.radius;
            box.max.y = this.radius;
            box.max.z = this.radius;
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            result = direction;
            result.Normalize();
            TSVector.Multiply(ref result, this.radius, out result);
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
    }
}

