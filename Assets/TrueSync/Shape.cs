using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TrueSync
{
	public abstract class Shape : ISupportMappable
	{
		private struct ClipTriangle
		{
			public TSVector n1;

			public TSVector n2;

			public TSVector n3;

			public int generation;
		}

		internal TSMatrix inertia = TSMatrix.Identity;

		internal FP mass = FP.One;

		internal TSBBox boundingBox = TSBBox.LargeBox;

		internal TSVector geomCen = TSVector.zero;

		[method: CompilerGenerated]
		[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
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

		public TSBBox BoundingBox
		{
			get
			{
				return this.boundingBox;
			}
		}

		public object Tag
		{
			get;
			set;
		}

		public Shape()
		{
		}

		protected void RaiseShapeUpdated()
		{
			bool flag = this.ShapeUpdated != null;
			if (flag)
			{
				this.ShapeUpdated();
			}
		}

		public virtual void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
		{
			FP zero = FP.Zero;
			bool flag = generationThreshold < 0;
			if (flag)
			{
				generationThreshold = 4;
			}
			Stack<Shape.ClipTriangle> stack = new Stack<Shape.ClipTriangle>();
			TSVector[] array = new TSVector[]
			{
				new TSVector(-1, 0, 0),
				new TSVector(1, 0, 0),
				new TSVector(0, -1, 0),
				new TSVector(0, 1, 0),
				new TSVector(0, 0, -1),
				new TSVector(0, 0, 1)
			};
			int[,] array2 = new int[,]
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
			for (int i = 0; i < 8; i++)
			{
				stack.Push(new Shape.ClipTriangle
				{
					n1 = array[array2[i, 0]],
					n2 = array[array2[i, 1]],
					n3 = array[array2[i, 2]],
					generation = 0
				});
			}
			while (stack.Count > 0)
			{
				Shape.ClipTriangle clipTriangle = stack.Pop();
				TSVector tSVector;
				this.SupportMapping(ref clipTriangle.n1, out tSVector);
				TSVector tSVector2;
				this.SupportMapping(ref clipTriangle.n2, out tSVector2);
				TSVector tSVector3;
				this.SupportMapping(ref clipTriangle.n3, out tSVector3);
				FP sqrMagnitude = (tSVector2 - tSVector).sqrMagnitude;
				FP sqrMagnitude2 = (tSVector3 - tSVector2).sqrMagnitude;
				FP sqrMagnitude3 = (tSVector - tSVector3).sqrMagnitude;
				bool flag2 = TSMath.Max(TSMath.Max(sqrMagnitude, sqrMagnitude2), sqrMagnitude3) > zero && clipTriangle.generation < generationThreshold;
				if (flag2)
				{
					Shape.ClipTriangle item = default(Shape.ClipTriangle);
					Shape.ClipTriangle item2 = default(Shape.ClipTriangle);
					Shape.ClipTriangle item3 = default(Shape.ClipTriangle);
					Shape.ClipTriangle item4 = default(Shape.ClipTriangle);
					item.generation = clipTriangle.generation + 1;
					item2.generation = clipTriangle.generation + 1;
					item3.generation = clipTriangle.generation + 1;
					item4.generation = clipTriangle.generation + 1;
					item.n1 = clipTriangle.n1;
					item2.n2 = clipTriangle.n2;
					item3.n3 = clipTriangle.n3;
					TSVector tSVector4 = FP.Half * (clipTriangle.n1 + clipTriangle.n2);
					tSVector4.Normalize();
					item.n2 = tSVector4;
					item2.n1 = tSVector4;
					item4.n3 = tSVector4;
					tSVector4 = FP.Half * (clipTriangle.n2 + clipTriangle.n3);
					tSVector4.Normalize();
					item2.n3 = tSVector4;
					item3.n2 = tSVector4;
					item4.n1 = tSVector4;
					tSVector4 = FP.Half * (clipTriangle.n3 + clipTriangle.n1);
					tSVector4.Normalize();
					item.n3 = tSVector4;
					item3.n1 = tSVector4;
					item4.n2 = tSVector4;
					stack.Push(item);
					stack.Push(item2);
					stack.Push(item3);
					stack.Push(item4);
				}
				else
				{
					bool flag3 = ((tSVector3 - tSVector) % (tSVector2 - tSVector)).sqrMagnitude > TSMath.Epsilon;
					if (flag3)
					{
						triangleList.Add(tSVector);
						triangleList.Add(tSVector2);
						triangleList.Add(tSVector3);
					}
				}
			}
		}

		public virtual void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
		{
			TSVector zero = TSVector.zero;
			zero.Set(orientation.M11, orientation.M21, orientation.M31);
			this.SupportMapping(ref zero, out zero);
			box.max.x = orientation.M11 * zero.x + orientation.M21 * zero.y + orientation.M31 * zero.z;
			zero.Set(orientation.M12, orientation.M22, orientation.M32);
			this.SupportMapping(ref zero, out zero);
			box.max.y = orientation.M12 * zero.x + orientation.M22 * zero.y + orientation.M32 * zero.z;
			zero.Set(orientation.M13, orientation.M23, orientation.M33);
			this.SupportMapping(ref zero, out zero);
			box.max.z = orientation.M13 * zero.x + orientation.M23 * zero.y + orientation.M33 * zero.z;
			zero.Set(-orientation.M11, -orientation.M21, -orientation.M31);
			this.SupportMapping(ref zero, out zero);
			box.min.x = orientation.M11 * zero.x + orientation.M21 * zero.y + orientation.M31 * zero.z;
			zero.Set(-orientation.M12, -orientation.M22, -orientation.M32);
			this.SupportMapping(ref zero, out zero);
			box.min.y = orientation.M12 * zero.x + orientation.M22 * zero.y + orientation.M32 * zero.z;
			zero.Set(-orientation.M13, -orientation.M23, -orientation.M33);
			this.SupportMapping(ref zero, out zero);
			box.min.z = orientation.M13 * zero.x + orientation.M23 * zero.y + orientation.M33 * zero.z;
		}

		public virtual void UpdateShape()
		{
			this.GetBoundingBox(ref TSMatrix.InternalIdentity, out this.boundingBox);
			this.CalculateMassInertia();
			this.RaiseShapeUpdated();
		}

		public static FP CalculateMassInertia(Shape shape, out TSVector centerOfMass, out TSMatrix inertia)
		{
			FP fP = FP.Zero;
			centerOfMass = TSVector.zero;
			inertia = TSMatrix.Zero;
			bool flag = shape is Multishape;
			if (flag)
			{
				throw new ArgumentException("Can't calculate inertia of multishapes.", "shape");
			}
			List<TSVector> list = new List<TSVector>();
			shape.MakeHull(ref list, 3);
			FP fP2 = FP.One / (60 * FP.One);
			FP fP3 = FP.One / (120 * FP.One);
			TSMatrix value = new TSMatrix(fP2, fP3, fP3, fP3, fP2, fP3, fP3, fP3, fP2);
			for (int i = 0; i < list.Count; i += 3)
			{
				TSVector tSVector = list[i];
				TSVector tSVector2 = list[i + 1];
				TSVector tSVector3 = list[i + 2];
				TSMatrix tSMatrix = new TSMatrix(tSVector.x, tSVector2.x, tSVector3.x, tSVector.y, tSVector2.y, tSVector3.y, tSVector.z, tSVector2.z, tSVector3.z);
				FP fP4 = tSMatrix.Determinant();
				TSMatrix value2 = TSMatrix.Multiply(tSMatrix * value * TSMatrix.Transpose(tSMatrix), fP4);
				TSVector value3 = FP.One / (4 * FP.One) * (list[i] + list[i + 1] + list[i + 2]);
				FP fP5 = FP.One / (6 * FP.One) * fP4;
				inertia += value2;
				centerOfMass += fP5 * value3;
				fP += fP5;
			}
			inertia = TSMatrix.Multiply(TSMatrix.Identity, inertia.Trace()) - inertia;
			centerOfMass *= FP.One / fP;
			FP x = centerOfMass.x;
			FP y = centerOfMass.y;
			FP z = centerOfMass.z;
			TSMatrix tSMatrix2 = new TSMatrix(-fP * (y * y + z * z), fP * x * y, fP * x * z, fP * y * x, -fP * (z * z + x * x), fP * y * z, fP * z * x, fP * z * y, -fP * (x * x + y * y));
			TSMatrix.Add(ref inertia, ref tSMatrix2, out inertia);
			return fP;
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
	}
}
