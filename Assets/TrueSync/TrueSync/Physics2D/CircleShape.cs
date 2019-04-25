namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class CircleShape : TrueSync.Physics2D.Shape
    {
        internal TSVector2 _position;

        internal CircleShape() : base(0)
        {
            base.ShapeType = ShapeType.Circle;
            base._radius = 0f;
            this._position = TSVector2.zero;
        }

        public CircleShape(FP radius, FP density) : base(density)
        {
            Debug.Assert(radius >= 0);
            Debug.Assert(density >= 0);
            base.ShapeType = ShapeType.Circle;
            this._position = TSVector2.zero;
            base.Radius = radius;
        }

        public override TrueSync.Physics2D.Shape Clone()
        {
            return new CircleShape { ShapeType = base.ShapeType, _radius = base.Radius, _2radius = base._2radius, _density = base._density, _position = this._position, MassData = base.MassData };
        }

        public bool CompareTo(CircleShape shape)
        {
            return ((base.Radius == shape.Radius) && (this.Position == shape.Position));
        }

        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            TSVector2 vector = transform.p + MathUtils.Mul(transform.q, this.Position);
            aabb.LowerBound = new TSVector2(vector.x - base.Radius, vector.y - base.Radius);
            aabb.UpperBound = new TSVector2(vector.x + base.Radius, vector.y + base.Radius);
        }

        protected sealed override void ComputeProperties()
        {
            FP fp = Settings.Pi * base._2radius;
            this.MassData.Area = fp;
            this.MassData.Mass = base.Density * fp;
            this.MassData.Centroid = this.Position;
            this.MassData.Inertia = this.MassData.Mass * ((0.5f * base._2radius) + TSVector2.Dot(this.Position, this.Position));
        }

        public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
        {
            sc = TSVector2.zero;
            TSVector2 vector = MathUtils.Mul(ref xf, this.Position);
            FP fp = -(TSVector2.Dot(normal, vector) - offset);
            if (fp < (-base.Radius + Settings.Epsilon))
            {
                return 0;
            }
            if (fp > base.Radius)
            {
                sc = vector;
                return (Settings.Pi * base._2radius);
            }
            FP fp2 = fp * fp;
            FP fp3 = base._2radius * ((TSMath.Asin(fp / base.Radius) + TSMath.PiOver2) + (fp * TSMath.Sqrt(base._2radius - fp2)));
            FP fp6 = base._2radius - fp2;
            FP fp4 = ((new FP(-2) / new FP(3)) * Math.Pow((double) fp6.AsFloat(), 1.5)) / fp3;
            sc.x = vector.x + (normal.x * fp4);
            sc.y = vector.y + (normal.y * fp4);
            return fp3;
        }

        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            output = new RayCastOutput();
            TSVector2 vector = transform.p + MathUtils.Mul(transform.q, this.Position);
            TSVector2 vector2 = input.Point1 - vector;
            FP fp = TSVector2.Dot(vector2, vector2) - base._2radius;
            TSVector2 vector3 = input.Point2 - input.Point1;
            FP fp2 = TSVector2.Dot(vector2, vector3);
            FP fp3 = TSVector2.Dot(vector3, vector3);
            FP x = (fp2 * fp2) - (fp3 * fp);
            if ((x >= 0f) && (fp3 >= Settings.Epsilon))
            {
                FP fp5 = -(fp2 + FP.Sqrt(x));
                if ((0f <= fp5) && (fp5 <= (input.MaxFraction * fp3)))
                {
                    fp5 /= fp3;
                    output.Fraction = fp5;
                    output.Normal = vector2 + (fp5 * vector3);
                    output.Normal.Normalize();
                    return true;
                }
            }
            return false;
        }

        public override bool TestPoint(ref Transform transform, ref TSVector2 point)
        {
            TSVector2 vector = transform.p + MathUtils.Mul(transform.q, this.Position);
            TSVector2 vector2 = point - vector;
            return (TSVector2.Dot(vector2, vector2) <= base._2radius);
        }

        public override int ChildCount
        {
            get
            {
                return 1;
            }
        }

        public TSVector2 Position
        {
            get
            {
                return this._position;
            }
            set
            {
                this._position = value;
                this.ComputeProperties();
            }
        }
    }
}

