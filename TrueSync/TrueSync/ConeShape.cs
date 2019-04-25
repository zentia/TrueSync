namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public class ConeShape : Shape
    {
        internal FP height;
        internal FP radius;
        internal FP sina = FP.Zero;

        public ConeShape(FP height, FP radius)
        {
            this.height = height;
            this.radius = radius;
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            base.mass = ((((FP.One / (3 * FP.One)) * TSMath.Pi) * this.radius) * this.radius) * this.height;
            base.inertia = TSMatrix.Identity;
            this.inertia.M11 = (((3 * FP.EN1) / 8) * base.mass) * ((this.radius * this.radius) + ((4 * this.height) * this.height));
            this.inertia.M22 = (((3 * FP.EN1) * base.mass) * this.radius) * this.radius;
            this.inertia.M33 = (((3 * FP.EN1) / 8) * base.mass) * ((this.radius * this.radius) + ((4 * this.height) * this.height));
            base.geomCen = TSVector.zero;
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            FP fp = FP.Sqrt((direction.x * direction.x) + (direction.z * direction.z));
            if (direction.y > (direction.magnitude * this.sina))
            {
                result.x = FP.Zero;
                result.y = ((2 * FP.One) / 3) * this.height;
                result.z = FP.Zero;
            }
            else if (fp > FP.Zero)
            {
                result.x = (this.radius * direction.x) / fp;
                result.y = -(FP.One / 3) * this.height;
                result.z = (this.radius * direction.z) / fp;
            }
            else
            {
                result.x = FP.Zero;
                result.y = -(FP.One / 3) * this.height;
                result.z = FP.Zero;
            }
        }

        public override void UpdateShape()
        {
            this.sina = this.radius / FP.Sqrt((this.radius * this.radius) + (this.height * this.height));
            base.UpdateShape();
        }

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
    }
}

