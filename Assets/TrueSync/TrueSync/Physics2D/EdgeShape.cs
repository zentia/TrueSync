// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.EdgeShape
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

namespace TrueSync.Physics2D
{
    public class EdgeShape : Shape
    {
        internal TSVector2 _vertex1;
        internal TSVector2 _vertex2;

        internal EdgeShape()
          : base((FP)0)
        {
            this.ShapeType = ShapeType.Edge;
            this._radius = Settings.PolygonRadius;
        }

        public EdgeShape(TSVector2 start, TSVector2 end)
          : base((FP)0)
        {
            this.ShapeType = ShapeType.Edge;
            this._radius = Settings.PolygonRadius;
            this.Set(start, end);
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

        public TSVector2 Vertex3 { get; set; }

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

        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            output = new RayCastOutput();
            TSVector2 tsVector2_1 = MathUtils.MulT(transform.q, input.Point1 - transform.p);
            TSVector2 tsVector2_2 = MathUtils.MulT(transform.q, input.Point2 - transform.p) - tsVector2_1;
            TSVector2 vertex1 = this._vertex1;
            TSVector2 vertex2 = this._vertex2;
            TSVector2 tsVector2_3 = vertex2 - vertex1;
            TSVector2 tsVector2_4 = new TSVector2(tsVector2_3.y, -tsVector2_3.x);
            tsVector2_4.Normalize();
            FP fp1 = TSVector2.Dot(tsVector2_4, vertex1 - tsVector2_1);
            FP fp2 = TSVector2.Dot(tsVector2_4, tsVector2_2);
            if (fp2 == (FP)0.0f)
                return false;
            FP fp3 = fp1 / fp2;
            if (fp3 < (FP)0.0f || input.MaxFraction < fp3)
                return false;
            TSVector2 tsVector2_5 = tsVector2_1 + fp3 * tsVector2_2;
            TSVector2 tsVector2_6 = vertex2 - vertex1;
            FP fp4 = TSVector2.Dot(tsVector2_6, tsVector2_6);
            if (fp4 == (FP)0.0f)
                return false;
            FP fp5 = TSVector2.Dot(tsVector2_5 - vertex1, tsVector2_6) / fp4;
            if (fp5 < (FP)0.0f || (FP)1f < fp5)
                return false;
            output.Fraction = fp3;
            output.Normal = !(fp1 > (FP)0.0f) ? tsVector2_4 : -tsVector2_4;
            return true;
        }

        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            TSVector2 tsVector2_1 = MathUtils.Mul(ref transform, this._vertex1);
            TSVector2 tsVector2_2 = MathUtils.Mul(ref transform, this._vertex2);
            TSVector2 tsVector2_3 = TSVector2.Min(tsVector2_1, tsVector2_2);
            TSVector2 tsVector2_4 = TSVector2.Max(tsVector2_1, tsVector2_2);
            TSVector2 tsVector2_5 = new TSVector2(this.Radius, this.Radius);
            aabb.LowerBound = tsVector2_3 - tsVector2_5;
            aabb.UpperBound = tsVector2_4 + tsVector2_5;
        }

        protected override void ComputeProperties()
        {
            this.MassData.Centroid = (FP)0.5f * (this._vertex1 + this._vertex2);
        }

        public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
        {
            sc = TSVector2.zero;
            return (FP)0;
        }

        public bool CompareTo(EdgeShape shape)
        {
            return this.HasVertex0 == shape.HasVertex0 && this.HasVertex3 == shape.HasVertex3 && (this.Vertex0 == shape.Vertex0 && this.Vertex1 == shape.Vertex1) && this.Vertex2 == shape.Vertex2 && this.Vertex3 == shape.Vertex3;
        }

        public override Shape Clone()
        {
            EdgeShape edgeShape = new EdgeShape();
            edgeShape.ShapeType = this.ShapeType;
            edgeShape._radius = this._radius;
            edgeShape._density = this._density;
            edgeShape.HasVertex0 = this.HasVertex0;
            edgeShape.HasVertex3 = this.HasVertex3;
            edgeShape.Vertex0 = this.Vertex0;
            edgeShape._vertex1 = this._vertex1;
            edgeShape._vertex2 = this._vertex2;
            edgeShape.Vertex3 = this.Vertex3;
            edgeShape.MassData = this.MassData;
            return (Shape)edgeShape;
        }
    }
}
