using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class TerrainShape : Multishape
	{
		private FP[,] heights;

		private FP scaleX;

		private FP scaleZ;

		private int heightsLength0;

		private int heightsLength1;

		private int minX;

		private int maxX;

		private int minZ;

		private int maxZ;

		private int numX;

		private int numZ;

		private TSBBox boundings;

		private FP sphericalExpansion = 5 * FP.EN2;

		private TSVector[] points = new TSVector[3];

		private TSVector normal = TSVector.up;

		public FP SphericalExpansion
		{
			get
			{
				return this.sphericalExpansion;
			}
			set
			{
				this.sphericalExpansion = value;
			}
		}

		public TerrainShape(FP[,] heights, FP scaleX, FP scaleZ)
		{
			this.heightsLength0 = heights.GetLength(0);
			this.heightsLength1 = heights.GetLength(1);
			this.boundings = TSBBox.SmallBox;
			for (int i = 0; i < this.heightsLength0; i++)
			{
				for (int j = 0; j < this.heightsLength1; j++)
				{
					bool flag = heights[i, j] > this.boundings.max.y;
					if (flag)
					{
						this.boundings.max.y = heights[i, j];
					}
					else
					{
						bool flag2 = heights[i, j] < this.boundings.min.y;
						if (flag2)
						{
							this.boundings.min.y = heights[i, j];
						}
					}
				}
			}
			this.boundings.min.x = FP.Zero;
			this.boundings.min.z = FP.Zero;
			this.boundings.max.x = this.heightsLength0 * scaleX;
			this.boundings.max.z = this.heightsLength1 * scaleZ;
			this.heights = heights;
			this.scaleX = scaleX;
			this.scaleZ = scaleZ;
			this.UpdateShape();
		}

		internal TerrainShape()
		{
		}

		protected override Multishape CreateWorkingClone()
		{
			return new TerrainShape
			{
				heights = this.heights,
				scaleX = this.scaleX,
				scaleZ = this.scaleZ,
				boundings = this.boundings,
				heightsLength0 = this.heightsLength0,
				heightsLength1 = this.heightsLength1,
				sphericalExpansion = this.sphericalExpansion
			};
		}

		public override void SetCurrentShape(int index)
		{
			bool flag = false;
			bool flag2 = index >= this.numX * this.numZ;
			if (flag2)
			{
				flag = true;
				index -= this.numX * this.numZ;
			}
			int num = index % this.numX;
			int num2 = index / this.numX;
			bool flag3 = flag;
			if (flag3)
			{
				this.points[0].Set((this.minX + num) * this.scaleX, this.heights[this.minX + num, this.minZ + num2], (this.minZ + num2) * this.scaleZ);
				this.points[1].Set((this.minX + num + 1) * this.scaleX, this.heights[this.minX + num + 1, this.minZ + num2], (this.minZ + num2) * this.scaleZ);
				this.points[2].Set((this.minX + num) * this.scaleX, this.heights[this.minX + num, this.minZ + num2 + 1], (this.minZ + num2 + 1) * this.scaleZ);
			}
			else
			{
				this.points[0].Set((this.minX + num + 1) * this.scaleX, this.heights[this.minX + num + 1, this.minZ + num2], (this.minZ + num2) * this.scaleZ);
				this.points[1].Set((this.minX + num + 1) * this.scaleX, this.heights[this.minX + num + 1, this.minZ + num2 + 1], (this.minZ + num2 + 1) * this.scaleZ);
				this.points[2].Set((this.minX + num) * this.scaleX, this.heights[this.minX + num, this.minZ + num2 + 1], (this.minZ + num2 + 1) * this.scaleZ);
			}
			TSVector geomCen = this.points[0];
			TSVector.Add(ref geomCen, ref this.points[1], out geomCen);
			TSVector.Add(ref geomCen, ref this.points[2], out geomCen);
			TSVector.Multiply(ref geomCen, FP.One / (3 * FP.One), out geomCen);
			this.geomCen = geomCen;
			TSVector.Subtract(ref this.points[1], ref this.points[0], out geomCen);
			TSVector.Subtract(ref this.points[2], ref this.points[0], out this.normal);
			TSVector.Cross(ref geomCen, ref this.normal, out this.normal);
		}

		public void CollisionNormal(out TSVector normal)
		{
			normal = this.normal;
		}

		public override int Prepare(ref TSBBox box)
		{
			bool flag = box.min.x < this.boundings.min.x;
			if (flag)
			{
				this.minX = 0;
			}
			else
			{
				this.minX = (int)((long)FP.Floor((box.min.x - this.sphericalExpansion) / this.scaleX));
				this.minX = (int)((long)TSMath.Max(this.minX, 0));
			}
			bool flag2 = box.max.x > this.boundings.max.x;
			if (flag2)
			{
				this.maxX = this.heightsLength0 - 1;
			}
			else
			{
				this.maxX = (int)((long)FP.Ceiling((box.max.x + this.sphericalExpansion) / this.scaleX));
				this.maxX = (int)((long)TSMath.Min(this.maxX, this.heightsLength0 - 1));
			}
			bool flag3 = box.min.z < this.boundings.min.z;
			if (flag3)
			{
				this.minZ = 0;
			}
			else
			{
				this.minZ = (int)((long)FP.Floor((box.min.z - this.sphericalExpansion) / this.scaleZ));
				this.minZ = (int)((long)TSMath.Max(this.minZ, 0));
			}
			bool flag4 = box.max.z > this.boundings.max.z;
			if (flag4)
			{
				this.maxZ = this.heightsLength1 - 1;
			}
			else
			{
				this.maxZ = (int)((long)FP.Ceiling((box.max.z + this.sphericalExpansion) / this.scaleZ));
				this.maxZ = (int)((long)TSMath.Min(this.maxZ, this.heightsLength1 - 1));
			}
			this.numX = this.maxX - this.minX;
			this.numZ = this.maxZ - this.minZ;
			return this.numX * this.numZ * 2;
		}

		public override void CalculateMassInertia()
		{
			this.inertia = TSMatrix.Identity;
			base.Mass = FP.One;
		}

		public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
		{
			box = this.boundings;
			box.min.x = box.min.x - this.sphericalExpansion;
			box.min.y = box.min.y - this.sphericalExpansion;
			box.min.z = box.min.z - this.sphericalExpansion;
			box.max.x = box.max.x + this.sphericalExpansion;
			box.max.y = box.max.y + this.sphericalExpansion;
			box.max.z = box.max.z + this.sphericalExpansion;
			box.Transform(ref orientation);
		}

		public override void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
		{
			for (int i = 0; i < (this.heightsLength0 - 1) * (this.heightsLength1 - 1); i++)
			{
				int num = i % (this.heightsLength0 - 1);
				int num2 = i / (this.heightsLength0 - 1);
				triangleList.Add(new TSVector(num * this.scaleX, this.heights[num, num2], num2 * this.scaleZ));
				triangleList.Add(new TSVector((num + 1) * this.scaleX, this.heights[num + 1, num2], num2 * this.scaleZ));
				triangleList.Add(new TSVector(num * this.scaleX, this.heights[num, num2 + 1], (num2 + 1) * this.scaleZ));
				triangleList.Add(new TSVector((num + 1) * this.scaleX, this.heights[num + 1, num2], num2 * this.scaleZ));
				triangleList.Add(new TSVector((num + 1) * this.scaleX, this.heights[num + 1, num2 + 1], (num2 + 1) * this.scaleZ));
				triangleList.Add(new TSVector(num * this.scaleX, this.heights[num, num2 + 1], (num2 + 1) * this.scaleZ));
			}
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			TSVector tSVector;
			TSVector.Normalize(ref direction, out tSVector);
			TSVector.Multiply(ref tSVector, this.sphericalExpansion, out tSVector);
			int num = 0;
			FP y = TSVector.Dot(ref this.points[0], ref direction);
			FP fP = TSVector.Dot(ref this.points[1], ref direction);
			bool flag = fP > y;
			if (flag)
			{
				y = fP;
				num = 1;
			}
			fP = TSVector.Dot(ref this.points[2], ref direction);
			bool flag2 = fP > y;
			if (flag2)
			{
				num = 2;
			}
			TSVector.Add(ref this.points[num], ref tSVector, out result);
		}

		public override int Prepare(ref TSVector rayOrigin, ref TSVector rayDelta)
		{
			TSBBox smallBox = TSBBox.SmallBox;
			TSVector value;
			TSVector.Normalize(ref rayDelta, out value);
			value = rayOrigin + rayDelta + value * this.sphericalExpansion;
			smallBox.AddPoint(ref rayOrigin);
			smallBox.AddPoint(ref value);
			return this.Prepare(ref smallBox);
		}
	}
}
