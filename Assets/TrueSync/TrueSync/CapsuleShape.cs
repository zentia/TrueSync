namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public class CapsuleShape : Shape
    {
        internal FP length;
        internal FP radius;

        public CapsuleShape(FP length, FP radius)
        {
            this.length = length;
            this.radius = radius;
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            FP fp = (((((3 * FP.One) / (4 * FP.One)) * TSMath.Pi) * this.radius) * this.radius) * this.radius;
            FP fp2 = ((TSMath.Pi * this.radius) * this.radius) * this.length;
            base.mass = fp2 + fp;
            this.inertia.M11 = ((((((FP.One / (4 * FP.One)) * fp2) * this.radius) * this.radius) + ((((FP.One / (12 * FP.One)) * fp2) * this.length) * this.length)) + (((((2 * FP.One) / (5 * FP.One)) * fp) * this.radius) * this.radius)) + ((((FP.One / (4 * FP.One)) * this.length) * this.length) * fp);
            this.inertia.M22 = ((((FP.One / (2 * FP.One)) * fp2) * this.radius) * this.radius) + (((((2 * FP.One) / (5 * FP.One)) * fp) * this.radius) * this.radius);
            this.inertia.M33 = ((((((FP.One / (4 * FP.One)) * fp2) * this.radius) * this.radius) + ((((FP.One / (12 * FP.One)) * fp2) * this.length) * this.length)) + (((((2 * FP.One) / (5 * FP.One)) * fp) * this.radius) * this.radius)) + ((((FP.One / (4 * FP.One)) * this.length) * this.length) * fp);
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            FP fp = FP.Sqrt((direction.x * direction.x) + (direction.z * direction.z));
            if (FP.Abs(direction.y) > FP.Zero)
            {
                TSVector vector;
                TSVector.Normalize(ref direction, out vector);
                TSVector.Multiply(ref vector, this.radius, out result);
                result.y += (FP.Sign(direction.y) * FP.Half) * this.length;
            }
            else if (fp > FP.Zero)
            {
                result.x = (direction.x / fp) * this.radius;
                result.y = FP.Zero;
                result.z = (direction.z / fp) * this.radius;
            }
            else
            {
                result.x = FP.Zero;
                result.y = FP.Zero;
                result.z = FP.Zero;
            }
        }

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
    }
}

