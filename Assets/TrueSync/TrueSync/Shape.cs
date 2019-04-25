// Decompiled with JetBrains decompiler
// Type: TrueSync.Shape
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections.Generic;

namespace TrueSync
{
    public abstract class Shape : ISupportMappable
    {
        internal TSMatrix inertia = TSMatrix.Identity;
        internal FP mass = FP.One;
        internal TSBBox boundingBox = TSBBox.LargeBox;
        internal TSVector geomCen = TSVector.zero;

        public event ShapeUpdatedHandler ShapeUpdated;

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

        protected void RaiseShapeUpdated()
        {
            // ISSUE: reference to a compiler-generated field
            if (this.ShapeUpdated == null)
                return;
            // ISSUE: reference to a compiler-generated field
            this.ShapeUpdated();
        }

        public TSBBox BoundingBox
        {
            get
            {
                return this.boundingBox;
            }
        }

        public object Tag { get; set; }

        public virtual void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
        {
            FP zero = FP.Zero;
            if (generationThreshold < 0)
                generationThreshold = 4;
            Stack<Shape.ClipTriangle> clipTriangleStack = new Stack<Shape.ClipTriangle>();
            TSVector[] tsVectorArray = new TSVector[6]
            {
        new TSVector(-1, 0, 0),
        new TSVector(1, 0, 0),
        new TSVector(0, -1, 0),
        new TSVector(0, 1, 0),
        new TSVector(0, 0, -1),
        new TSVector(0, 0, 1)
            };
            int[,] numArray = new int[8, 3]
            {
        {
          5,
          1,
          3
        },
        {
          4,
          3,
          1
        },
        {
          3,
          4,
          0
        },
        {
          0,
          5,
          3
        },
        {
          5,
          2,
          1
        },
        {
          4,
          1,
          2
        },
        {
          2,
          0,
          4
        },
        {
          0,
          2,
          5
        }
            };
            for (int index = 0; index < 8; ++index)
                clipTriangleStack.Push(new Shape.ClipTriangle()
                {
                    n1 = tsVectorArray[numArray[index, 0]],
                    n2 = tsVectorArray[numArray[index, 1]],
                    n3 = tsVectorArray[numArray[index, 2]],
                    generation = 0
                });
            while (clipTriangleStack.Count > 0)
            {
                Shape.ClipTriangle clipTriangle1 = clipTriangleStack.Pop();
                TSVector result1;
                this.SupportMapping(ref clipTriangle1.n1, out result1);
                TSVector result2;
                this.SupportMapping(ref clipTriangle1.n2, out result2);
                TSVector result3;
                this.SupportMapping(ref clipTriangle1.n3, out result3);
                if (TSMath.Max(TSMath.Max((result2 - result1).sqrMagnitude, (result3 - result2).sqrMagnitude), (result1 - result3).sqrMagnitude) > zero && clipTriangle1.generation < generationThreshold)
                {
                    Shape.ClipTriangle clipTriangle2 = new Shape.ClipTriangle();
                    Shape.ClipTriangle clipTriangle3 = new Shape.ClipTriangle();
                    Shape.ClipTriangle clipTriangle4 = new Shape.ClipTriangle();
                    Shape.ClipTriangle clipTriangle5 = new Shape.ClipTriangle();
                    clipTriangle2.generation = clipTriangle1.generation + 1;
                    clipTriangle3.generation = clipTriangle1.generation + 1;
                    clipTriangle4.generation = clipTriangle1.generation + 1;
                    clipTriangle5.generation = clipTriangle1.generation + 1;
                    clipTriangle2.n1 = clipTriangle1.n1;
                    clipTriangle3.n2 = clipTriangle1.n2;
                    clipTriangle4.n3 = clipTriangle1.n3;
                    TSVector tsVector1 = FP.Half * (clipTriangle1.n1 + clipTriangle1.n2);
                    tsVector1.Normalize();
                    clipTriangle2.n2 = tsVector1;
                    clipTriangle3.n1 = tsVector1;
                    clipTriangle5.n3 = tsVector1;
                    TSVector tsVector2 = FP.Half * (clipTriangle1.n2 + clipTriangle1.n3);
                    tsVector2.Normalize();
                    clipTriangle3.n3 = tsVector2;
                    clipTriangle4.n2 = tsVector2;
                    clipTriangle5.n1 = tsVector2;
                    TSVector tsVector3 = FP.Half * (clipTriangle1.n3 + clipTriangle1.n1);
                    tsVector3.Normalize();
                    clipTriangle2.n3 = tsVector3;
                    clipTriangle4.n1 = tsVector3;
                    clipTriangle5.n2 = tsVector3;
                    clipTriangleStack.Push(clipTriangle2);
                    clipTriangleStack.Push(clipTriangle3);
                    clipTriangleStack.Push(clipTriangle4);
                    clipTriangleStack.Push(clipTriangle5);
                }
                else if (((result3 - result1) % (result2 - result1)).sqrMagnitude > TSMath.Epsilon)
                {
                    triangleList.Add(result1);
                    triangleList.Add(result2);
                    triangleList.Add(result3);
                }
            }
        }

        public virtual void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            TSVector result = TSVector.zero;
            result.Set(orientation.M11, orientation.M21, orientation.M31);
            this.SupportMapping(ref result, out result);
            box.max.x = orientation.M11 * result.x + orientation.M21 * result.y + orientation.M31 * result.z;
            result.Set(orientation.M12, orientation.M22, orientation.M32);
            this.SupportMapping(ref result, out result);
            box.max.y = orientation.M12 * result.x + orientation.M22 * result.y + orientation.M32 * result.z;
            result.Set(orientation.M13, orientation.M23, orientation.M33);
            this.SupportMapping(ref result, out result);
            box.max.z = orientation.M13 * result.x + orientation.M23 * result.y + orientation.M33 * result.z;
            result.Set(-orientation.M11, -orientation.M21, -orientation.M31);
            this.SupportMapping(ref result, out result);
            box.min.x = orientation.M11 * result.x + orientation.M21 * result.y + orientation.M31 * result.z;
            result.Set(-orientation.M12, -orientation.M22, -orientation.M32);
            this.SupportMapping(ref result, out result);
            box.min.y = orientation.M12 * result.x + orientation.M22 * result.y + orientation.M32 * result.z;
            result.Set(-orientation.M13, -orientation.M23, -orientation.M33);
            this.SupportMapping(ref result, out result);
            box.min.z = orientation.M13 * result.x + orientation.M23 * result.y + orientation.M33 * result.z;
        }

        public virtual void UpdateShape()
        {
            this.GetBoundingBox(ref TSMatrix.InternalIdentity, out this.boundingBox);
            this.CalculateMassInertia();
            this.RaiseShapeUpdated();
        }

        public static FP CalculateMassInertia(Shape shape, out TSVector centerOfMass, out TSMatrix inertia)
        {
            FP zero = FP.Zero;
            centerOfMass = TSVector.zero;
            inertia = TSMatrix.Zero;
            if (shape is Multishape)
                throw new ArgumentException("Can't calculate inertia of multishapes.", nameof(shape));
            List<TSVector> triangleList = new List<TSVector>();
            shape.MakeHull(ref triangleList, 3);
            FP fp1 = FP.One / ((FP)60 * FP.One);
            FP fp2 = FP.One / ((FP)120 * FP.One);
            TSMatrix tsMatrix1 = new TSMatrix(fp1, fp2, fp2, fp2, fp1, fp2, fp2, fp2, fp1);
            int index = 0;
            while (index < triangleList.Count)
            {
                TSVector tsVector1 = triangleList[index];
                TSVector tsVector2 = triangleList[index + 1];
                TSVector tsVector3 = triangleList[index + 2];
                TSMatrix matrix = new TSMatrix(tsVector1.x, tsVector2.x, tsVector3.x, tsVector1.y, tsVector2.y, tsVector3.y, tsVector1.z, tsVector2.z, tsVector3.z);
                FP scaleFactor = matrix.Determinant();
                TSMatrix tsMatrix2 = TSMatrix.Multiply(matrix * tsMatrix1 * TSMatrix.Transpose(matrix), scaleFactor);
                TSVector tsVector4 = FP.One / ((FP)4 * FP.One) * (triangleList[index] + triangleList[index + 1] + triangleList[index + 2]);
                FP fp3 = FP.One / ((FP)6 * FP.One) * scaleFactor;
                inertia = inertia + tsMatrix2;
                centerOfMass = centerOfMass + fp3 * tsVector4;
                zero += fp3;
                index += 3;
            }
            inertia = TSMatrix.Multiply(TSMatrix.Identity, inertia.Trace()) - inertia;
            centerOfMass = centerOfMass * (FP.One / zero);
            FP x = centerOfMass.x;
            FP y = centerOfMass.y;
            FP z = centerOfMass.z;
            TSMatrix matrix2 = new TSMatrix(-zero * (y * y + z * z), zero * x * y, zero * x * z, zero * y * x, -zero * (z * z + x * x), zero * y * z, zero * z * x, zero * z * y, -zero * (x * x + y * y));
            TSMatrix.Add(ref inertia, ref matrix2, out inertia);
            return zero;
        }

        public virtual void CalculateMassInertia()
        {
            this.mass = Shape.CalculateMassInertia(this, out this.geomCen, out this.inertia);
        }

        public abstract void SupportMapping(ref TSVector direction, out TSVector result);

        public void SupportCenter(out TSVector geomCenter)
        {
            geomCenter = this.geomCen;
        }

        private struct ClipTriangle
        {
            public TSVector n1;
            public TSVector n2;
            public TSVector n3;
            public int generation;
        }
    }
}
