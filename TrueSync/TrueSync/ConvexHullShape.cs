namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class ConvexHullShape : Shape
    {
        private TSVector shifted;
        private List<TSVector> vertices = null;

        public ConvexHullShape(List<TSVector> vertices)
        {
            this.vertices = vertices;
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            base.mass = Shape.CalculateMassInertia(this, out this.shifted, out this.inertia);
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            FP minValue = FP.MinValue;
            int num = 0;
            for (int i = 0; i < this.vertices.Count; i++)
            {
                FP fp2 = TSVector.Dot(this.vertices[i], direction);
                if (fp2 > minValue)
                {
                    minValue = fp2;
                    num = i;
                }
            }
            result = this.vertices[num] - this.shifted;
        }

        public TSVector Shift
        {
            get
            {
                return (TSVector) (-1 * this.shifted);
            }
        }
    }
}

