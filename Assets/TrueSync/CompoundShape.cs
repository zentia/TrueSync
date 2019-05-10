using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class CompoundShape : Multishape
	{
		public struct TransformedShape
		{
			private Shape shape;

			internal TSVector position;

			internal TSMatrix orientation;

			internal TSMatrix invOrientation;

			internal TSBBox boundingBox;

			public Shape Shape
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
				this.boundingBox.min = this.boundingBox.min + this.position;
				this.boundingBox.max = this.boundingBox.max + this.position;
			}

			public TransformedShape(Shape shape, TSMatrix orientation, TSVector position)
			{
				this.position = position;
				this.orientation = orientation;
				TSMatrix.Transpose(ref orientation, out this.invOrientation);
				this.shape = shape;
				this.boundingBox = default(TSBBox);
				this.UpdateBoundingBox();
			}
		}

		private CompoundShape.TransformedShape[] shapes;

		private TSVector shifted;

		private TSBBox mInternalBBox;

		private int currentShape = 0;

		private List<int> currentSubShapes = new List<int>();

		public CompoundShape.TransformedShape[] Shapes
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
				return -FP.One * this.shifted;
			}
		}

		public CompoundShape(List<CompoundShape.TransformedShape> shapes)
		{
			this.shapes = new CompoundShape.TransformedShape[shapes.Count];
			shapes.CopyTo(this.shapes);
			bool flag = !this.TestValidity();
			if (flag)
			{
				throw new ArgumentException("Multishapes are not supported!");
			}
			this.UpdateShape();
		}

		public CompoundShape(CompoundShape.TransformedShape[] shapes)
		{
			this.shapes = new CompoundShape.TransformedShape[shapes.Length];
			Array.Copy(shapes, this.shapes, shapes.Length);
			bool flag = !this.TestValidity();
			if (flag)
			{
				throw new ArgumentException("Multishapes are not supported!");
			}
			this.UpdateShape();
		}

		private bool TestValidity()
		{
			bool result;
			for (int i = 0; i < this.shapes.Length; i++)
			{
				bool flag = this.shapes[i].Shape is Multishape;
				if (flag)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}

		public override void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
		{
			List<TSVector> list = new List<TSVector>();
			for (int i = 0; i < this.shapes.Length; i++)
			{
				this.shapes[i].Shape.MakeHull(ref list, 4);
				for (int j = 0; j < list.Count; j++)
				{
					TSVector item = list[j];
					TSVector.Transform(ref item, ref this.shapes[i].orientation, out item);
					TSVector.Add(ref item, ref this.shapes[i].position, out item);
					triangleList.Add(item);
				}
				list.Clear();
			}
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
				CompoundShape.TransformedShape[] expr_77_cp_0_cp_0 = this.Shapes;
				int expr_77_cp_0_cp_1 = j;
				expr_77_cp_0_cp_0[expr_77_cp_0_cp_1].position = expr_77_cp_0_cp_0[expr_77_cp_0_cp_1].position - this.shifted;
			}
		}

		public override void CalculateMassInertia()
		{
			this.inertia = TSMatrix.Zero;
			this.mass = FP.Zero;
			for (int i = 0; i < this.Shapes.Length; i++)
			{
				TSMatrix value = this.Shapes[i].InverseOrientation * this.Shapes[i].Shape.Inertia * this.Shapes[i].Orientation;
				TSVector tSVector = this.Shapes[i].Position * -FP.One;
				FP mass = this.Shapes[i].Shape.Mass;
				value.M11 += mass * (tSVector.y * tSVector.y + tSVector.z * tSVector.z);
				value.M22 += mass * (tSVector.x * tSVector.x + tSVector.z * tSVector.z);
				value.M33 += mass * (tSVector.x * tSVector.x + tSVector.y * tSVector.y);
				value.M12 += -tSVector.x * tSVector.y * mass;
				value.M21 += -tSVector.x * tSVector.y * mass;
				value.M31 += -tSVector.x * tSVector.z * mass;
				value.M13 += -tSVector.x * tSVector.z * mass;
				value.M32 += -tSVector.y * tSVector.z * mass;
				value.M23 += -tSVector.y * tSVector.z * mass;
				this.inertia += value;
				this.mass += mass;
			}
		}

		internal CompoundShape()
		{
		}

		protected override Multishape CreateWorkingClone()
		{
			return new CompoundShape
			{
				shapes = this.shapes
			};
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			TSVector.Transform(ref direction, ref this.shapes[this.currentShape].invOrientation, out result);
			this.shapes[this.currentShape].Shape.SupportMapping(ref direction, out result);
			TSVector.Transform(ref result, ref this.shapes[this.currentShape].orientation, out result);
			TSVector.Add(ref result, ref this.shapes[this.currentShape].position, out result);
		}

		public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
		{
			box.min = this.mInternalBBox.min;
			box.max = this.mInternalBBox.max;
			TSVector tSVector = FP.Half * (box.max - box.min);
			TSVector tSVector2 = FP.Half * (box.max + box.min);
			TSVector value;
			TSVector.Transform(ref tSVector2, ref orientation, out value);
			TSMatrix tSMatrix;
			TSMath.Absolute(ref orientation, out tSMatrix);
			TSVector value2;
			TSVector.Transform(ref tSVector, ref tSMatrix, out value2);
			box.max = value + value2;
			box.min = value - value2;
		}

		public override void SetCurrentShape(int index)
		{
			this.currentShape = this.currentSubShapes[index];
			this.shapes[this.currentShape].Shape.SupportCenter(out this.geomCen);
			this.geomCen += this.shapes[this.currentShape].Position;
		}

		public override int Prepare(ref TSBBox box)
		{
			this.currentSubShapes.Clear();
			for (int i = 0; i < this.shapes.Length; i++)
			{
				bool flag = this.shapes[i].boundingBox.Contains(ref box) > TSBBox.ContainmentType.Disjoint;
				if (flag)
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

		public override void UpdateShape()
		{
			this.DoShifting();
			this.UpdateInternalBoundingBox();
			base.UpdateShape();
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
	}
}
