namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class PolygonShape : TrueSync.Physics2D.Shape
    {
        internal TrueSync.Physics2D.Vertices _normals;
        internal TrueSync.Physics2D.Vertices _vertices;

        internal PolygonShape() : base(0)
        {
            base.ShapeType = ShapeType.Polygon;
            base._radius = Settings.PolygonRadius;
            this._vertices = new TrueSync.Physics2D.Vertices(Settings.MaxPolygonVertices);
            this._normals = new TrueSync.Physics2D.Vertices(Settings.MaxPolygonVertices);
        }

        public PolygonShape(FP density) : base(density)
        {
            Debug.Assert(density >= 0f);
            base.ShapeType = ShapeType.Polygon;
            base._radius = Settings.PolygonRadius;
            this._vertices = new TrueSync.Physics2D.Vertices(Settings.MaxPolygonVertices);
            this._normals = new TrueSync.Physics2D.Vertices(Settings.MaxPolygonVertices);
        }

        public PolygonShape(TrueSync.Physics2D.Vertices vertices, FP density) : base(density)
        {
            base.ShapeType = ShapeType.Polygon;
            base._radius = Settings.PolygonRadius;
            this.Vertices = vertices;
        }

        public override TrueSync.Physics2D.Shape Clone()
        {
            return new PolygonShape { ShapeType = base.ShapeType, _radius = base._radius, _density = base._density, _vertices = new TrueSync.Physics2D.Vertices(this._vertices), _normals = new TrueSync.Physics2D.Vertices(this._normals), MassData = base.MassData };
        }

        public bool CompareTo(PolygonShape shape)
        {
            if (this.Vertices.Count != shape.Vertices.Count)
            {
                return false;
            }
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                if (this.Vertices[i] != shape.Vertices[i])
                {
                    return false;
                }
            }
            return ((base.Radius == shape.Radius) && (base.MassData == shape.MassData));
        }

        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            TSVector2 vector = MathUtils.Mul(ref transform, this.Vertices[0]);
            TSVector2 vector2 = vector;
            for (int i = 1; i < this.Vertices.Count; i++)
            {
                TSVector2 vector4 = MathUtils.Mul(ref transform, this.Vertices[i]);
                vector = TSVector2.Min(vector, vector4);
                vector2 = TSVector2.Max(vector2, vector4);
            }
            TSVector2 vector3 = new TSVector2(base.Radius, base.Radius);
            aabb.LowerBound = vector - vector3;
            aabb.UpperBound = vector2 + vector3;
        }

        protected override void ComputeProperties()
        {
            Debug.Assert(this.Vertices.Count >= 3);
            if (base._density > 0)
            {
                TSVector2 zero = TSVector2.zero;
                FP fp = 0f;
                FP fp2 = 0f;
                TSVector2 vector2 = TSVector2.zero;
                for (int i = 0; i < this.Vertices.Count; i++)
                {
                    vector2 += this.Vertices[i];
                }
                vector2 = (TSVector2) (vector2 * (1f / ((float) this.Vertices.Count)));
                FP fp3 = 0.3333333f;
                for (int j = 0; j < this.Vertices.Count; j++)
                {
                    TSVector2 a = this.Vertices[j] - vector2;
                    TSVector2 b = ((j + 1) < this.Vertices.Count) ? (this.Vertices[j + 1] - vector2) : (this.Vertices[0] - vector2);
                    FP fp4 = MathUtils.Cross(a, b);
                    FP fp5 = 0.5f * fp4;
                    fp += fp5;
                    zero += (fp5 * fp3) * (a + b);
                    FP x = a.x;
                    FP y = a.y;
                    FP fp8 = b.x;
                    FP fp9 = b.y;
                    FP fp10 = ((x * x) + (fp8 * x)) + (fp8 * fp8);
                    FP fp11 = ((y * y) + (fp9 * y)) + (fp9 * fp9);
                    fp2 += ((0.25f * fp3) * fp4) * (fp10 + fp11);
                }
                Debug.Assert(fp > Settings.Epsilon);
                this.MassData.Area = fp;
                this.MassData.Mass = base._density * fp;
                zero = (TSVector2) (zero * (1f / fp));
                this.MassData.Centroid = zero + vector2;
                this.MassData.Inertia = base._density * fp2;
                this.MassData.Inertia += this.MassData.Mass * (TSVector2.Dot(this.MassData.Centroid, this.MassData.Centroid) - TSVector2.Dot(zero, zero));
            }
        }

        public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
        {
            int num4;
            sc = TSVector2.zero;
            TSVector2 vector = MathUtils.MulT(xf.q, (TSVector2) normal);
            FP fp = offset - TSVector2.Dot(normal, xf.p);
            FP[] fpArray = new FP[Settings.MaxPolygonVertices];
            int num = 0;
            int index = -1;
            int num3 = -1;
            bool flag = false;
            for (num4 = 0; num4 < this.Vertices.Count; num4++)
            {
                fpArray[num4] = TSVector2.Dot(vector, this.Vertices[num4]) - fp;
                bool flag2 = fpArray[num4] < -Settings.Epsilon;
                if (num4 > 0)
                {
                    if (flag2)
                    {
                        if (!flag)
                        {
                            index = num4 - 1;
                            num++;
                        }
                    }
                    else if (flag)
                    {
                        num3 = num4 - 1;
                        num++;
                    }
                }
                flag = flag2;
            }
            switch (num)
            {
                case 0:
                    if (flag)
                    {
                        sc = MathUtils.Mul(ref xf, this.MassData.Centroid);
                        return (this.MassData.Mass / base.Density);
                    }
                    return 0;

                case 1:
                    if (index == -1)
                    {
                        index = this.Vertices.Count - 1;
                    }
                    else
                    {
                        num3 = this.Vertices.Count - 1;
                    }
                    break;
            }
            int num5 = (index + 1) % this.Vertices.Count;
            int num6 = (num3 + 1) % this.Vertices.Count;
            FP fp2 = (0 - fpArray[index]) / (fpArray[num5] - fpArray[index]);
            FP fp3 = (0 - fpArray[num3]) / (fpArray[num6] - fpArray[num3]);
            TSVector2 vector2 = new TSVector2((this.Vertices[index].x * (1 - fp2)) + (this.Vertices[num5].x * fp2), (this.Vertices[index].y * (1 - fp2)) + (this.Vertices[num5].y * fp2));
            TSVector2 vector3 = new TSVector2((this.Vertices[num3].x * (1 - fp3)) + (this.Vertices[num6].x * fp3), (this.Vertices[num3].y * (1 - fp3)) + (this.Vertices[num6].y * fp3));
            FP fp4 = 0;
            TSVector2 v = new TSVector2(0, 0);
            TSVector2 vector5 = this.Vertices[num5];
            FP fp5 = 0.3333333f;
            num4 = num5;
            while (num4 != num6)
            {
                TSVector2 vector6;
                num4 = (num4 + 1) % this.Vertices.Count;
                if (num4 == num6)
                {
                    vector6 = vector3;
                }
                else
                {
                    vector6 = this.Vertices[num4];
                }
                TSVector2 a = vector5 - vector2;
                TSVector2 b = vector6 - vector2;
                FP fp7 = MathUtils.Cross(a, b);
                FP fp8 = 0.5f * fp7;
                fp4 += fp8;
                v += (fp8 * fp5) * ((vector2 + vector5) + vector6);
                vector5 = vector6;
            }
            v = (TSVector2) (v * (1f / fp4));
            sc = MathUtils.Mul(ref xf, v);
            return fp4;
        }

        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            output = new RayCastOutput();
            TSVector2 vector = MathUtils.MulT(transform.q, input.Point1 - transform.p);
            TSVector2 vector3 = MathUtils.MulT(transform.q, input.Point2 - transform.p) - vector;
            FP fp = 0f;
            FP maxFraction = input.MaxFraction;
            int num = -1;
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                FP fp3 = TSVector2.Dot(this.Normals[i], this.Vertices[i] - vector);
                FP fp4 = TSVector2.Dot(this.Normals[i], vector3);
                if (fp4 == 0f)
                {
                    if (fp3 < 0f)
                    {
                        return false;
                    }
                }
                else if ((fp4 < 0f) && (fp3 < (fp * fp4)))
                {
                    fp = fp3 / fp4;
                    num = i;
                }
                else if ((fp4 > 0f) && (fp3 < (maxFraction * fp4)))
                {
                    maxFraction = fp3 / fp4;
                }
                if (maxFraction < fp)
                {
                    return false;
                }
            }
            Debug.Assert((0f <= fp) && (fp <= input.MaxFraction));
            if (num >= 0)
            {
                output.Fraction = fp;
                output.Normal = MathUtils.Mul(transform.q, this.Normals[num]);
                return true;
            }
            return false;
        }

        public override bool TestPoint(ref Transform transform, ref TSVector2 point)
        {
            TSVector2 vector = MathUtils.MulT(transform.q, ((TSVector2) point) - transform.p);
            for (int i = 0; i < this.Vertices.Count; i++)
            {
                if (TSVector2.Dot(this.Normals[i], vector - this.Vertices[i]) > 0f)
                {
                    return false;
                }
            }
            return true;
        }

        public override int ChildCount
        {
            get
            {
                return 1;
            }
        }

        public TrueSync.Physics2D.Vertices Normals
        {
            get
            {
                return this._normals;
            }
        }

        public TrueSync.Physics2D.Vertices Vertices
        {
            get
            {
                return this._vertices;
            }
            set
            {
                this._vertices = new TrueSync.Physics2D.Vertices(value);
                Debug.Assert((this._vertices.Count >= 3) && (this._vertices.Count <= Settings.MaxPolygonVertices));
                if (Settings.UseConvexHullPolygons)
                {
                    if (this._vertices.Count <= 3)
                    {
                        this._vertices.ForceCounterClockWise();
                    }
                    else
                    {
                        this._vertices = GiftWrap.GetConvexHull(this._vertices);
                    }
                }
                this._vertices.ForceCounterClockWise();
                this._normals = new TrueSync.Physics2D.Vertices(this._vertices.Count);
                for (int i = 0; i < this._vertices.Count; i++)
                {
                    int num2 = ((i + 1) < this._vertices.Count) ? (i + 1) : 0;
                    TSVector2 vector = this._vertices[num2] - this._vertices[i];
                    TSVector2 item = new TSVector2(vector.y, -vector.x);
                    item.Normalize();
                    this._normals.Add(item);
                }
                this.ComputeProperties();
            }
        }
    }
}

