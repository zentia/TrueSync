namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class MinkowskiSumShape : Shape
    {
        private List<Shape> shapes = new List<Shape>();
        private TSVector shifted;

        public MinkowskiSumShape(IEnumerable<Shape> shapes)
        {
            this.AddShapes(shapes);
        }

        public void AddShape(Shape shape)
        {
            if (shape is Multishape)
            {
                throw new Exception("Multishapes not supported by MinkowskiSumShape.");
            }
            this.shapes.Add(shape);
            this.UpdateShape();
        }

        public void AddShapes(IEnumerable<Shape> shapes)
        {
            foreach (Shape shape in shapes)
            {
                if (shape is Multishape)
                {
                    throw new Exception("Multishapes not supported by MinkowskiSumShape.");
                }
                this.shapes.Add(shape);
            }
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            base.mass = Shape.CalculateMassInertia(this, out this.shifted, out this.inertia);
        }

        public bool Remove(Shape shape)
        {
            if (this.shapes.Count == 1)
            {
                throw new Exception("There must be at least one shape.");
            }
            bool flag = this.shapes.Remove(shape);
            this.UpdateShape();
            return flag;
        }

        public TSVector Shift()
        {
            return (TSVector) (-1 * this.shifted);
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            TSVector zero = TSVector.zero;
            for (int i = 0; i < this.shapes.Count; i++)
            {
                TSVector vector;
                this.shapes[i].SupportMapping(ref direction, out vector);
                TSVector.Add(ref vector, ref zero, out zero);
            }
            TSVector.Subtract(ref zero, ref this.shifted, out result);
        }
    }
}

