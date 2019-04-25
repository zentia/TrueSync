namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public class CylinderShape : Shape
    {
        internal FP height;
        internal FP radius;

        public CylinderShape(FP height, FP radius)
        {
            this.height = height;
            this.radius = radius;
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            base.mass = ((TSMath.Pi * this.radius) * this.radius) * this.height;
            this.inertia.M11 = ((((FP.One / (4 * FP.One)) * base.mass) * this.radius) * this.radius) + ((((FP.One / (12 * FP.One)) * base.mass) * this.height) * this.height);
            this.inertia.M22 = (((FP.One / (2 * FP.One)) * base.mass) * this.radius) * this.radius;
            this.inertia.M33 = ((((FP.One / (4 * FP.One)) * base.mass) * this.radius) * this.radius) + ((((FP.One / (12 * FP.One)) * base.mass) * this.height) * this.height);
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            FP fp = FP.Sqrt((direction.x * direction.x) + (direction.z * direction.z));
            if (fp > FP.Zero)
            {
                result.x = (direction.x / fp) * this.radius;
                result.y = (FP.Sign(direction.y) * this.height) * FP.Half;
                result.z = (direction.z / fp) * this.radius;
            }
            else
            {
                result.x = FP.Zero;
                result.y = (FP.Sign(direction.y) * this.height) * FP.Half;
                result.z = FP.Zero;
            }
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

