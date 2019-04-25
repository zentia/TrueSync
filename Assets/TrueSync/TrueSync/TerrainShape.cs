namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class TerrainShape : Multishape
    {
        private TSBBox boundings;
        private FP[,] heights;
        private int heightsLength0;
        private int heightsLength1;
        private int maxX;
        private int maxZ;
        private int minX;
        private int minZ;
        private TSVector normal;
        private int numX;
        private int numZ;
        private TSVector[] points;
        private FP scaleX;
        private FP scaleZ;
        private FP sphericalExpansion;

        internal TerrainShape()
        {
            this.sphericalExpansion = 5 * FP.EN2;
            this.points = new TSVector[3];
            this.normal = TSVector.up;
        }

        public TerrainShape(FP[,] heights, FP scaleX, FP scaleZ)
        {
            this.sphericalExpansion = 5 * FP.EN2;
            this.points = new TSVector[3];
            this.normal = TSVector.up;
            this.heightsLength0 = heights.GetLength(0);
            this.heightsLength1 = heights.GetLength(1);
            this.boundings = TSBBox.SmallBox;
            for (int i = 0; i < this.heightsLength0; i++)
            {
                for (int j = 0; j < this.heightsLength1; j++)
                {
                    if (heights[i, j] > this.boundings.max.y)
                    {
                        this.boundings.max.y = heights[i, j];
                    }
                    else if (heights[i, j] < this.boundings.min.y)
                    {
                        this.boundings.min.y = heights[i, j];
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

        public override void CalculateMassInertia()
        {
            base.inertia = TSMatrix.Identity;
            base.Mass = FP.One;
        }

        public void CollisionNormal(out TSVector normal)
        {
            normal = this.normal;
        }

        protected override Multishape CreateWorkingClone()
        {
            return new TerrainShape { heights = this.heights, scaleX = this.scaleX, scaleZ = this.scaleZ, boundings = this.boundings, heightsLength0 = this.heightsLength0, heightsLength1 = this.heightsLength1, sphericalExpansion = this.sphericalExpansion };
        }

        public override void GetBoundingBox(ref TSMatrix orientation, out TSBBox box)
        {
            box = this.boundings;
            box.min.x -= this.sphericalExpansion;
            box.min.y -= this.sphericalExpansion;
            box.min.z -= this.sphericalExpansion;
            box.max.x += this.sphericalExpansion;
            box.max.y += this.sphericalExpansion;
            box.max.z += this.sphericalExpansion;
            box.Transform(ref orientation);
        }

        public override void MakeHull(ref List<TSVector> triangleList, int generationThreshold)
        {
            for (int i = 0; i < ((this.heightsLength0 - 1) * (this.heightsLength1 - 1)); i++)
            {
                int num2 = i % (this.heightsLength0 - 1);
                int num3 = i / (this.heightsLength0 - 1);
                triangleList.Add(new TSVector(num2 * this.scaleX, this.heights[num2, num3], num3 * this.scaleZ));
                triangleList.Add(new TSVector((num2 + 1) * this.scaleX, this.heights[num2 + 1, num3], num3 * this.scaleZ));
                triangleList.Add(new TSVector(num2 * this.scaleX, this.heights[num2, num3 + 1], (num3 + 1) * this.scaleZ));
                triangleList.Add(new TSVector((num2 + 1) * this.scaleX, this.heights[num2 + 1, num3], num3 * this.scaleZ));
                triangleList.Add(new TSVector((num2 + 1) * this.scaleX, this.heights[num2 + 1, num3 + 1], (num3 + 1) * this.scaleZ));
                triangleList.Add(new TSVector(num2 * this.scaleX, this.heights[num2, num3 + 1], (num3 + 1) * this.scaleZ));
            }
        }

        public override int Prepare(ref TSBBox box)
        {
            if (box.min.x < this.boundings.min.x)
            {
                this.minX = 0;
            }
            else
            {
                this.minX = (int) ((long) FP.Floor((box.min.x - this.sphericalExpansion) / this.scaleX));
                this.minX = (int) ((long) TSMath.Max(this.minX, 0));
            }
            if (box.max.x > this.boundings.max.x)
            {
                this.maxX = this.heightsLength0 - 1;
            }
            else
            {
                this.maxX = (int) ((long) FP.Ceiling((box.max.x + this.sphericalExpansion) / this.scaleX));
                this.maxX = (int) ((long) TSMath.Min(this.maxX, this.heightsLength0 - 1));
            }
            if (box.min.z < this.boundings.min.z)
            {
                this.minZ = 0;
            }
            else
            {
                this.minZ = (int) ((long) FP.Floor((box.min.z - this.sphericalExpansion) / this.scaleZ));
                this.minZ = (int) ((long) TSMath.Max(this.minZ, 0));
            }
            if (box.max.z > this.boundings.max.z)
            {
                this.maxZ = this.heightsLength1 - 1;
            }
            else
            {
                this.maxZ = (int) ((long) FP.Ceiling((box.max.z + this.sphericalExpansion) / this.scaleZ));
                this.maxZ = (int) ((long) TSMath.Min(this.maxZ, this.heightsLength1 - 1));
            }
            this.numX = this.maxX - this.minX;
            this.numZ = this.maxZ - this.minZ;
            return ((this.numX * this.numZ) * 2);
        }

        public override int Prepare(ref TSVector rayOrigin, ref TSVector rayDelta)
        {
            TSVector vector;
            TSBBox smallBox = TSBBox.SmallBox;
            TSVector.Normalize(ref rayDelta, out vector);
            vector = (rayOrigin + rayDelta) + (vector * this.sphericalExpansion);
            smallBox.AddPoint(ref rayOrigin);
            smallBox.AddPoint(ref vector);
            return this.Prepare(ref smallBox);
        }

        public override void SetCurrentShape(int index)
        {
            bool flag = false;
            if (index >= (this.numX * this.numZ))
            {
                flag = true;
                index -= this.numX * this.numZ;
            }
            int num = index % this.numX;
            int num2 = index / this.numX;
            if (flag)
            {
                this.points[0].Set((this.minX + num) * this.scaleX, this.heights[this.minX + num, this.minZ + num2], (this.minZ + num2) * this.scaleZ);
                this.points[1].Set(((this.minX + num) + 1) * this.scaleX, this.heights[(this.minX + num) + 1, this.minZ + num2], (this.minZ + num2) * this.scaleZ);
                this.points[2].Set((this.minX + num) * this.scaleX, this.heights[this.minX + num, (this.minZ + num2) + 1], ((this.minZ + num2) + 1) * this.scaleZ);
            }
            else
            {
                this.points[0].Set(((this.minX + num) + 1) * this.scaleX, this.heights[(this.minX + num) + 1, this.minZ + num2], (this.minZ + num2) * this.scaleZ);
                this.points[1].Set(((this.minX + num) + 1) * this.scaleX, this.heights[(this.minX + num) + 1, (this.minZ + num2) + 1], ((this.minZ + num2) + 1) * this.scaleZ);
                this.points[2].Set((this.minX + num) * this.scaleX, this.heights[this.minX + num, (this.minZ + num2) + 1], ((this.minZ + num2) + 1) * this.scaleZ);
            }
            TSVector vector = this.points[0];
            TSVector.Add(ref vector, ref this.points[1], out vector);
            TSVector.Add(ref vector, ref this.points[2], out vector);
            TSVector.Multiply(ref vector, FP.One / (3 * FP.One), out vector);
            base.geomCen = vector;
            TSVector.Subtract(ref this.points[1], ref this.points[0], out vector);
            TSVector.Subtract(ref this.points[2], ref this.points[0], out this.normal);
            TSVector.Cross(ref vector, ref this.normal, out this.normal);
        }

        public override void SupportMapping(ref TSVector direction, out TSVector result)
        {
            TSVector vector;
            TSVector.Normalize(ref direction, out vector);
            TSVector.Multiply(ref vector, this.sphericalExpansion, out vector);
            int index = 0;
            FP fp = TSVector.Dot(ref this.points[0], ref direction);
            FP fp2 = TSVector.Dot(ref this.points[1], ref direction);
            if (fp2 > fp)
            {
                fp = fp2;
                index = 1;
            }
            fp2 = TSVector.Dot(ref this.points[2], ref direction);
            if (fp2 > fp)
            {
                fp = fp2;
                index = 2;
            }
            TSVector.Add(ref this.points[index], ref vector, out result);
        }

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
    }
}

