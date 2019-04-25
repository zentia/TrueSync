namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public class BoxShape : Shape
    {
        internal TSVector halfSize;
        internal TSVector size;

        public BoxShape(TSVector size)
        {
            this.size = TSVector.zero;
            this.halfSize = TSVector.zero;
            this.size = size;
            this.UpdateShape();
        }

        public BoxShape(FP length, FP height, FP width)
        {
            this.size = TSVector.zero;
            this.halfSize = TSVector.zero;
            this.size.x = length;
            this.size.y = height;
            this.size.z = width;
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            base.mass = (this.size.x * this.size.y) * this.size.z;
            base.inertia = TSMatrix.Identity;
            this.inertia.M11 = ((FP.One / (12 * FP.One)) * base.mass) * ((this.size.y * this.size.y) + (this.size.z * this.size.z));
            this.inertia.M22 = ((FP.One / (12 * FP.One)) * base.mass) * ((this.size.x * this.size.x) + (this.size.z * this.size.z));
            this.inertia.M33 = ((FP.One / (12 * FP.One)) * base.mass) * ((this.size.x * this.size.x) + (this.size.y * this.size.y));
            base.geomCen = TSVector.zero;
        }

        public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            TSMatrix matrix;
            TSVector vector;
            TSMath.Absolute(ref orientation, out matrix);
            TSVector.Transform(ref this.halfSize, ref matrix, out vector);
            box.max = vector;
            TSVector.Negate(ref vector, out box.min);
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            result.x = FP.Sign(direction.x) * this.halfSize.x;
            result.y = FP.Sign(direction.y) * this.halfSize.y;
            result.z = FP.Sign(direction.z) * this.halfSize.z;
        }

        public override void UpdateShape()
        {
            this.halfSize = this.size * FP.Half;
            base.UpdateShape();
        }

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
    }
}

