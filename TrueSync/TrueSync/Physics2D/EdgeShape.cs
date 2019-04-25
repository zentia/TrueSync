namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class EdgeShape : TrueSync.Physics2D.Shape
    {
        internal TSVector2 _vertex1;
        internal TSVector2 _vertex2;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <HasVertex0>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <HasVertex3>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <Vertex0>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <Vertex3>k__BackingField;

        internal EdgeShape() : base(0)
        {
            base.ShapeType = ShapeType.Edge;
            base._radius = Settings.PolygonRadius;
        }

        public EdgeShape(TSVector2 start, TSVector2 end) : base(0)
        {
            base.ShapeType = ShapeType.Edge;
            base._radius = Settings.PolygonRadius;
            this.Set(start, end);
        }

        public override TrueSync.Physics2D.Shape Clone()
        {
            return new EdgeShape { ShapeType = base.ShapeType, _radius = base._radius, _density = base._density, HasVertex0 = this.HasVertex0, HasVertex3 = this.HasVertex3, Vertex0 = this.Vertex0, _vertex1 = this._vertex1, _vertex2 = this._vertex2, Vertex3 = this.Vertex3, MassData = base.MassData };
        }

        public bool CompareTo(EdgeShape shape)
        {
            return (((((this.HasVertex0 == shape.HasVertex0) && (this.HasVertex3 == shape.HasVertex3)) && ((this.Vertex0 == shape.Vertex0) && (this.Vertex1 == shape.Vertex1))) && (this.Vertex2 == shape.Vertex2)) && (this.Vertex3 == shape.Vertex3));
        }

        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            TSVector2 vector = MathUtils.Mul(ref transform, this._vertex1);
            TSVector2 vector2 = MathUtils.Mul(ref transform, this._vertex2);
            TSVector2 vector3 = TSVector2.Min(vector, vector2);
            TSVector2 vector4 = TSVector2.Max(vector, vector2);
            TSVector2 vector5 = new TSVector2(base.Radius, base.Radius);
            aabb.LowerBound = vector3 - vector5;
            aabb.UpperBound = vector4 + vector5;
        }

        protected override void ComputeProperties()
        {
            this.MassData.Centroid = (TSVector2) (0.5f * (this._vertex1 + this._vertex2));
        }

        public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
        {
            sc = TSVector2.zero;
            return 0;
        }

        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            output = new RayCastOutput();
            TSVector2 vector = MathUtils.MulT(transform.q, input.Point1 - transform.p);
            TSVector2 vector3 = MathUtils.MulT(transform.q, input.Point2 - transform.p) - vector;
            TSVector2 vector4 = this._vertex1;
            TSVector2 vector5 = this._vertex2;
            TSVector2 vector6 = vector5 - vector4;
            TSVector2 vector7 = new TSVector2(vector6.y, -vector6.x);
            vector7.Normalize();
            FP fp = TSVector2.Dot(vector7, vector4 - vector);
            FP fp2 = TSVector2.Dot(vector7, vector3);
            if (fp2 == 0f)
            {
                return false;
            }
            FP fp3 = fp / fp2;
            if ((fp3 < 0f) || (input.MaxFraction < fp3))
            {
                return false;
            }
            TSVector2 vector8 = vector + (fp3 * vector3);
            TSVector2 vector9 = vector5 - vector4;
            FP fp4 = TSVector2.Dot(vector9, vector9);
            if (fp4 == 0f)
            {
                return false;
            }
            FP fp5 = TSVector2.Dot(vector8 - vector4, vector9) / fp4;
            if ((fp5 < 0f) || (1f < fp5))
            {
                return false;
            }
            output.Fraction = fp3;
            if (fp > 0f)
            {
                output.Normal = -vector7;
            }
            else
            {
                output.Normal = vector7;
            }
            return true;
        }

        public void Set(TSVector2 start, TSVector2 end)
        {
            this._vertex1 = start;
            this._vertex2 = end;
            this.HasVertex0 = false;
            this.HasVertex3 = false;
            this.ComputeProperties();
        }

        public override bool TestPoint(ref Transform transform, ref TSVector2 point)
        {
            return false;
        }

        public override int ChildCount
        {
            get
            {
                return 1;
            }
        }

        public bool HasVertex0 { get; set; }

        public bool HasVertex3 { get; set; }

        public TSVector2 Vertex0 { get; set; }

        public TSVector2 Vertex1
        {
            get
            {
                return this._vertex1;
            }
            set
            {
                this._vertex1 = value;
                this.ComputeProperties();
            }
        }

        public TSVector2 Vertex2
        {
            get
            {
                return this._vertex2;
            }
            set
            {
                this._vertex2 = value;
                this.ComputeProperties();
            }
        }

        public TSVector2 Vertex3 { get; set; }
    }
}

