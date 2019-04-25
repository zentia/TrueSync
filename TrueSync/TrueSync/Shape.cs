namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class Shape : ISupportMappable
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object <Tag>k__BackingField;
        internal TSBBox boundingBox = TSBBox.LargeBox;
        internal TSVector geomCen = TSVector.zero;
        internal TSMatrix inertia = TSMatrix.Identity;
        internal FP mass = FP.One;

        [field: CompilerGenerated, DebuggerBrowsable(0)]
        public event ShapeUpdatedHandler ShapeUpdated;

        public virtual void CalculateMassInertia()
        {
            this.mass = CalculateMassInertia(this, out this.geomCen, out this.inertia);
        }

        public static FP CalculateMassInertia(Shape shape, out TSVector centerOfMass, out TSMatrix inertia)
        {
            FP zero = FP.Zero;
            centerOfMass = TSVector.zero;
            inertia = TSMatrix.Zero;
            if (shape is Multishape)
            {
                throw new ArgumentException("Can't calculate inertia of multishapes.", "shape");
            }
            List<TSVector> triangleList = new List<TSVector>();
            shape.MakeHull(ref triangleList, 3);
            FP fp2 = FP.One / (60 * FP.One);
            FP fp3 = FP.One / (120 * FP.One);
            TSMatrix matrix = new TSMatrix(fp2, fp3, fp3, fp3, fp2, fp3, fp3, fp3, fp2);
            for (int i = 0; i < triangleList.Count; i += 3)
            {
                TSVector vector = triangleList[i];
                TSVector vector2 = triangleList[i + 1];
                TSVector vector3 = triangleList[i + 2];
                TSMatrix matrix3 = new TSMatrix(vector.x, vector2.x, vector3.x, vector.y, vector2.y, vector3.y, vector.z, vector2.z, vector3.z);
                FP scaleFactor = matrix3.Determinant();
                TSMatrix matrix4 = TSMatrix.Multiply((matrix3 * matrix) * TSMatrix.Transpose(matrix3), scaleFactor);
                TSVector vector4 = (TSVector) ((FP.One / (4 * FP.One)) * ((triangleList[i] + triangleList[i + 1]) + triangleList[i + 2]));
                FP fp8 = (FP.One / (6 * FP.One)) * scaleFactor;
                inertia += matrix4;
                centerOfMass += fp8 * vector4;
                zero += fp8;
            }
            inertia = TSMatrix.Multiply(TSMatrix.Identity, inertia.Trace()) - inertia;
            centerOfMass *= FP.One / zero;
            FP x = centerOfMass.x;
            FP y = centerOfMass.y;
            FP z = centerOfMass.z;
            TSMatrix matrix2 = new TSMatrix(-zero * ((y * y) + (z * z)), (zero * x) * y, (zero * x) * z, (zero * y) * x, -zero * ((z * z) + (x * x)), (zero * y) * z, (zero * z) * x, (zero * z) * y, -zero * ((x * x) + (y * y)));
            TSMatrix.Add(ref inertia, ref matrix2, out inertia);
            return zero;
        }

        public virtual void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            TSVector zero = TSVector.zero;
            zero.Set(orientation.M11, orientation.M21, orientation.M31);
            this.SupportMapping(ref zero, out zero);
            box.max.x = ((orientation.M11 * zero.x) + (orientation.M21 * zero.y)) + (orientation.M31 * zero.z);
            zero.Set(orientation.M12, orientation.M22, orientation.M32);
            this.SupportMapping(ref zero, out zero);
            box.max.y = ((orientation.M12 * zero.x) + (orientation.M22 * zero.y)) + (orientation.M32 * zero.z);
            zero.Set(orientation.M13, orientation.M23, orientation.M33);
            this.SupportMapping(ref zero, out zero);
            box.max.z = ((orientation.M13 * zero.x) + (orientation.M23 * zero.y)) + (orientation.M33 * zero.z);
            zero.Set(-orientation.M11, -orientation.M21, -orientation.M31);
            this.SupportMapping(ref zero, out zero);
            box.min.x = ((orientation.M11 * zero.x) + (orientation.M21 * zero.y)) + (orientation.M31 * zero.z);
            zero.Set(-orientation.M12, -orientation.M22, -orientation.M32);
            this.SupportMapping(ref zero, out zero);
            box.min.y = ((orientation.M12 * zero.x) + (orientation.M22 * zero.y)) + (orientation.M32 * zero.z);
            zero.Set(-orientation.M13, -orientation.M23, -orientation.M33);
            this.SupportMapping(ref zero, out zero);
            box.min.z = ((orientation.M13 * zero.x) + (orientation.M23 * zero.y)) + (orientation.M33 * zero.z);
        }

        public virtual void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
        {
            FP zero = FP.Zero;
            if (generationThreshold < 0)
            {
                generationThreshold = 4;
            }
            Stack<ClipTriangle> stack = new Stack<ClipTriangle>();
            TSVector[] vectorArray = new TSVector[] { new TSVector(-1, 0, 0), new TSVector(1, 0, 0), new TSVector(0, -1, 0), new TSVector(0, 1, 0), new TSVector(0, 0, -1), new TSVector(0, 0, 1) };
            int[,] numArray = new int[,] { { 5, 1, 3 }, { 4, 3, 1 }, { 3, 4, 0 }, { 0, 5, 3 }, { 5, 2, 1 }, { 4, 1, 2 }, { 2, 0, 4 }, { 0, 2, 5 } };
            for (int i = 0; i < 8; i++)
            {
                ClipTriangle item = new ClipTriangle {
                    n1 = vectorArray[numArray[i, 0]],
                    n2 = vectorArray[numArray[i, 1]],
                    n3 = vectorArray[numArray[i, 2]],
                    generation = 0
                };
                stack.Push(item);
            }
            while (stack.Count > 0)
            {
                TSVector vector;
                TSVector vector2;
                TSVector vector3;
                ClipTriangle triangle2 = stack.Pop();
                this.SupportMapping(ref triangle2.n1, out vector);
                this.SupportMapping(ref triangle2.n2, out vector2);
                this.SupportMapping(ref triangle2.n3, out vector3);
                TSVector vector4 = vector2 - vector;
                FP sqrMagnitude = vector4.sqrMagnitude;
                vector4 = vector3 - vector2;
                FP fp3 = vector4.sqrMagnitude;
                vector4 = vector - vector3;
                FP fp4 = vector4.sqrMagnitude;
                if ((TSMath.Max(TSMath.Max(sqrMagnitude, fp3), fp4) > zero) && (triangle2.generation < generationThreshold))
                {
                    ClipTriangle triangle3 = new ClipTriangle();
                    ClipTriangle triangle4 = new ClipTriangle();
                    ClipTriangle triangle5 = new ClipTriangle();
                    ClipTriangle triangle6 = new ClipTriangle();
                    triangle3.generation = triangle2.generation + 1;
                    triangle4.generation = triangle2.generation + 1;
                    triangle5.generation = triangle2.generation + 1;
                    triangle6.generation = triangle2.generation + 1;
                    triangle3.n1 = triangle2.n1;
                    triangle4.n2 = triangle2.n2;
                    triangle5.n3 = triangle2.n3;
                    TSVector vector5 = (TSVector) (FP.Half * (triangle2.n1 + triangle2.n2));
                    vector5.Normalize();
                    triangle3.n2 = vector5;
                    triangle4.n1 = vector5;
                    triangle6.n3 = vector5;
                    vector5 = (TSVector) (FP.Half * (triangle2.n2 + triangle2.n3));
                    vector5.Normalize();
                    triangle4.n3 = vector5;
                    triangle5.n2 = vector5;
                    triangle6.n1 = vector5;
                    vector5 = (TSVector) (FP.Half * (triangle2.n3 + triangle2.n1));
                    vector5.Normalize();
                    triangle3.n3 = vector5;
                    triangle5.n1 = vector5;
                    triangle6.n2 = vector5;
                    stack.Push(triangle3);
                    stack.Push(triangle4);
                    stack.Push(triangle5);
                    stack.Push(triangle6);
                }
                else
                {
                    vector4 = (vector3 - vector) % (vector2 - vector);
                    if (vector4.sqrMagnitude > TSMath.Epsilon)
                    {
                        triangleList.Add(vector);
                        triangleList.Add(vector2);
                        triangleList.Add(vector3);
                    }
                }
            }
        }

        protected void RaiseShapeUpdated()
        {
            if (this.ShapeUpdated > null)
            {
                this.ShapeUpdated();
            }
        }

        public void SupportCenter(out TSVector geomCenter)
        {
            geomCenter = this.geomCen;
        }

        public abstract void SupportMapping(ref TSVector direction, out TSVector result);
        public virtual void UpdateShape()
        {
            this.GetBoundingBox(ref TSMatrix.InternalIdentity, out this.boundingBox);
            this.CalculateMassInertia();
            this.RaiseShapeUpdated();
        }

        public TSBBox BoundingBox
        {
            get
            {
                return this.boundingBox;
            }
        }

        public TSMatrix Inertia
        {
            get
            {
                return this.inertia;
            }
            protected set
            {
                this.inertia = value;
            }
        }

        public FP Mass
        {
            get
            {
                return this.mass;
            }
            protected set
            {
                this.mass = value;
            }
        }

        public object Tag { get; set; }

        [StructLayout(LayoutKind.Sequential)]
        private struct ClipTriangle
        {
            public TSVector n1;
            public TSVector n2;
            public TSVector n3;
            public int generation;
        }
    }
}

