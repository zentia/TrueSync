namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CompoundShape : Multishape
    {
        private int currentShape;
        private List<int> currentSubShapes;
        private TSBBox mInternalBBox;
        private TransformedShape[] shapes;
        private TSVector shifted;

        internal CompoundShape()
        {
            this.currentShape = 0;
            this.currentSubShapes = new List<int>();
        }

        public CompoundShape(List<TransformedShape> shapes)
        {
            this.currentShape = 0;
            this.currentSubShapes = new List<int>();
            this.shapes = new TransformedShape[shapes.Count];
            shapes.CopyTo(this.shapes);
            if (!this.TestValidity())
            {
                throw new ArgumentException("Multishapes are not supported!");
            }
            this.UpdateShape();
        }

        public CompoundShape(TransformedShape[] shapes)
        {
            this.currentShape = 0;
            this.currentSubShapes = new List<int>();
            this.shapes = new TransformedShape[shapes.Length];
            Array.Copy(shapes, this.shapes, shapes.Length);
            if (!this.TestValidity())
            {
                throw new ArgumentException("Multishapes are not supported!");
            }
            this.UpdateShape();
        }

        public override void CalculateMassInertia()
        {
            base.inertia = TSMatrix.Zero;
            base.mass = FP.Zero;
            for (int i = 0; i < this.Shapes.Length; i++)
            {
                TSMatrix matrix = (this.Shapes[i].InverseOrientation * this.Shapes[i].Shape.Inertia) * this.Shapes[i].Orientation;
                TSVector vector = this.Shapes[i].Position * -FP.One;
                FP mass = this.Shapes[i].Shape.Mass;
                matrix.M11 += mass * ((vector.y * vector.y) + (vector.z * vector.z));
                matrix.M22 += mass * ((vector.x * vector.x) + (vector.z * vector.z));
                matrix.M33 += mass * ((vector.x * vector.x) + (vector.y * vector.y));
                matrix.M12 += (-vector.x * vector.y) * mass;
                matrix.M21 += (-vector.x * vector.y) * mass;
                matrix.M31 += (-vector.x * vector.z) * mass;
                matrix.M13 += (-vector.x * vector.z) * mass;
                matrix.M32 += (-vector.y * vector.z) * mass;
                matrix.M23 += (-vector.y * vector.z) * mass;
                base.inertia += matrix;
                base.mass += mass;
            }
        }

        protected override Multishape CreateWorkingClone()
        {
            return new CompoundShape { shapes = this.shapes };
        }

        private void DoShifting()
        {
            for (int i = 0; i < this.Shapes.Length; i++)
            {
                this.shifted += this.Shapes[i].position;
            }
            this.shifted *= FP.One / this.shapes.Length;
            for (int j = 0; j < this.Shapes.Length; j++)
            {
                this.Shapes[j].position -= this.shifted;
            }
        }

        public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            TSVector vector3;
            TSMatrix matrix;
            TSVector vector4;
            box.min = this.mInternalBBox.min;
            box.max = this.mInternalBBox.max;
            TSVector position = (TSVector) (FP.Half * (box.max - box.min));
            TSVector vector2 = (TSVector) (FP.Half * (box.max + box.min));
            TSVector.Transform(ref vector2, ref orientation, out vector3);
            TSMath.Absolute(ref orientation, out matrix);
            TSVector.Transform(ref position, ref matrix, out vector4);
            box.max = vector3 + vector4;
            box.min = vector3 - vector4;
        }

        public override void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
        {
            List<TSVector> list = new List<TSVector>();
            for (int i = 0; i < this.shapes.Length; i++)
            {
                this.shapes[i].Shape.MakeHull(ref list, 4);
                for (int j = 0; j < list.Count; j++)
                {
                    TSVector position = list[j];
                    TSVector.Transform(ref position, ref this.shapes[i].orientation, out position);
                    TSVector.Add(ref position, ref this.shapes[i].position, out position);
                    triangleList.Add(position);
                }
                list.Clear();
            }
        }

        public override int Prepare(ref TSBBox box)
        {
            this.currentSubShapes.Clear();
            for (int i = 0; i < this.shapes.Length; i++)
            {
                if (this.shapes[i].boundingBox.Contains(ref box) > TSBBox.ContainmentType.Disjoint)
                {
                    this.currentSubShapes.Add(i);
                }
            }
            return this.currentSubShapes.Count;
        }

        public override int Prepare(ref TSVector rayOrigin, ref TSVector rayEnd)
        {
            TSBBox smallBox = TSBBox.SmallBox;
            smallBox.AddPoint(ref rayOrigin);
            smallBox.AddPoint(ref rayEnd);
            return this.Prepare(ref smallBox);
        }

        public override void SetCurrentShape(int index)
        {
            this.currentShape = this.currentSubShapes[index];
            this.shapes[this.currentShape].Shape.SupportCenter(out this.geomCen);
            base.geomCen += this.shapes[this.currentShape].Position;
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            TSVector.Transform(ref direction, ref this.shapes[this.currentShape].invOrientation, out result);
            this.shapes[this.currentShape].Shape.SupportMapping(ref direction, out result);
            TSVector.Transform(ref result, ref this.shapes[this.currentShape].orientation, out result);
            TSVector.Add(ref result, ref this.shapes[this.currentShape].position, out result);
        }

        private bool TestValidity()
        {
            for (int i = 0; i < this.shapes.Length; i++)
            {
                if (this.shapes[i].Shape is Multishape)
                {
                    return false;
                }
            }
            return true;
        }

        protected void UpdateInternalBoundingBox()
        {
            this.mInternalBBox.min = new TSVector(FP.MaxValue);
            this.mInternalBBox.max = new TSVector(FP.MinValue);
            for (int i = 0; i < this.shapes.Length; i++)
            {
                this.shapes[i].UpdateBoundingBox();
                TSBBox.CreateMerged(ref this.mInternalBBox, ref this.shapes[i].boundingBox, out this.mInternalBBox);
            }
        }

        public override void UpdateShape()
        {
            this.DoShifting();
            this.UpdateInternalBoundingBox();
            base.UpdateShape();
        }

        public TransformedShape[] Shapes
        {
            get
            {
                return this.shapes;
            }
        }

        public TSVector Shift
        {
            get
            {
                return (TSVector) (-FP.One * this.shifted);
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct TransformedShape
        {
            private TrueSync.Shape shape;
            internal TSVector position;
            internal TSMatrix orientation;
            internal TSMatrix invOrientation;
            internal TSBBox boundingBox;
            public TrueSync.Shape Shape
            {
                get
                {
                    return this.shape;
                }
                set
                {
                    this.shape = value;
                }
            }
            public TSVector Position
            {
                get
                {
                    return this.position;
                }
                set
                {
                    this.position = value;
                }
            }
            public TSBBox BoundingBox
            {
                get
                {
                    return this.boundingBox;
                }
            }
            public TSMatrix InverseOrientation
            {
                get
                {
                    return this.invOrientation;
                }
            }
            public TSMatrix Orientation
            {
                get
                {
                    return this.orientation;
                }
                set
                {
                    this.orientation = value;
                    TSMatrix.Transpose(ref this.orientation, out this.invOrientation);
                }
            }
            public void UpdateBoundingBox()
            {
                this.Shape.GetBoundingBox(ref this.orientation, out this.boundingBox);
                this.boundingBox.min += this.position;
                this.boundingBox.max += this.position;
            }

            public TransformedShape(TrueSync.Shape shape, TSMatrix orientation, TSVector position)
            {
                this.position = position;
                this.orientation = orientation;
                TSMatrix.Transpose(ref orientation, out this.invOrientation);
                this.shape = shape;
                this.boundingBox = new TSBBox();
                this.UpdateBoundingBox();
            }
        }
    }
}

