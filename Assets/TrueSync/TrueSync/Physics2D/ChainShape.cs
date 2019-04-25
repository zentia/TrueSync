namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class ChainShape : TrueSync.Physics2D.Shape
    {
        private static EdgeShape _edgeShape = new EdgeShape();
        private bool _hasNextVertex;
        private bool _hasPrevVertex;
        private TSVector2 _nextVertex;
        private TSVector2 _prevVertex;
        public TrueSync.Physics2D.Vertices Vertices;

        public ChainShape() : base(0)
        {
            base.ShapeType = ShapeType.Chain;
            base._radius = Settings.PolygonRadius;
        }

        public ChainShape(TrueSync.Physics2D.Vertices vertices, bool createLoop = false) : base(0)
        {
            base.ShapeType = ShapeType.Chain;
            base._radius = Settings.PolygonRadius;
            Debug.Assert((vertices != null) && (vertices.Count >= 3));
            Debug.Assert(vertices[0] != vertices[vertices.Count - 1]);
            for (int i = 1; i < vertices.Count; i++)
            {
                TSVector2 vector = vertices[i - 1];
                TSVector2 vector2 = vertices[i];
                Debug.Assert(TSVector2.DistanceSquared(vector, vector2) > (Settings.LinearSlop * Settings.LinearSlop));
            }
            this.Vertices = new TrueSync.Physics2D.Vertices(vertices);
            if (createLoop)
            {
                this.Vertices.Add(vertices[0]);
                this.PrevVertex = this.Vertices[this.Vertices.Count - 2];
                this.NextVertex = this.Vertices[1];
            }
        }

        public override TrueSync.Physics2D.Shape Clone()
        {
            return new ChainShape { ShapeType = base.ShapeType, _density = base._density, _radius = base._radius, PrevVertex = this._prevVertex, NextVertex = this._nextVertex, _hasNextVertex = this._hasNextVertex, _hasPrevVertex = this._hasPrevVertex, Vertices = new TrueSync.Physics2D.Vertices(this.Vertices), MassData = base.MassData };
        }

        public bool CompareTo(ChainShape shape)
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
            return ((this.PrevVertex == shape.PrevVertex) && (this.NextVertex == shape.NextVertex));
        }

        public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
        {
            Debug.Assert(childIndex < this.Vertices.Count);
            int num = childIndex;
            int num2 = childIndex + 1;
            if (num2 == this.Vertices.Count)
            {
                num2 = 0;
            }
            TSVector2 vector = MathUtils.Mul(ref transform, this.Vertices[num]);
            TSVector2 vector2 = MathUtils.Mul(ref transform, this.Vertices[num2]);
            aabb.LowerBound = TSVector2.Min(vector, vector2);
            aabb.UpperBound = TSVector2.Max(vector, vector2);
        }

        protected override void ComputeProperties()
        {
        }

        public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
        {
            sc = TSVector2.zero;
            return 0;
        }

        public EdgeShape GetChildEdge(int index)
        {
            EdgeShape edge = new EdgeShape();
            this.GetChildEdge(edge, index);
            return edge;
        }

        internal void GetChildEdge(EdgeShape edge, int index)
        {
            Debug.Assert((0 <= index) && (index < (this.Vertices.Count - 1)));
            Debug.Assert(edge > null);
            edge.ShapeType = ShapeType.Edge;
            edge._radius = base._radius;
            edge.Vertex1 = this.Vertices[index];
            edge.Vertex2 = this.Vertices[index + 1];
            if (index > 0)
            {
                edge.Vertex0 = this.Vertices[index - 1];
                edge.HasVertex0 = true;
            }
            else
            {
                edge.Vertex0 = this._prevVertex;
                edge.HasVertex0 = this._hasPrevVertex;
            }
            if (index < (this.Vertices.Count - 2))
            {
                edge.Vertex3 = this.Vertices[index + 2];
                edge.HasVertex3 = true;
            }
            else
            {
                edge.Vertex3 = this._nextVertex;
                edge.HasVertex3 = this._hasNextVertex;
            }
        }

        public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
        {
            Debug.Assert(childIndex < this.Vertices.Count);
            int num = childIndex;
            int num2 = childIndex + 1;
            if (num2 == this.Vertices.Count)
            {
                num2 = 0;
            }
            _edgeShape.Vertex1 = this.Vertices[num];
            _edgeShape.Vertex2 = this.Vertices[num2];
            return _edgeShape.RayCast(out output, ref input, ref transform, 0);
        }

        public override bool TestPoint(ref Transform transform, ref TSVector2 point)
        {
            return false;
        }

        public override int ChildCount
        {
            get
            {
                return (this.Vertices.Count - 1);
            }
        }

        public TSVector2 NextVertex
        {
            get
            {
                return this._nextVertex;
            }
            set
            {
                this._nextVertex = value;
                this._hasNextVertex = true;
            }
        }

        public TSVector2 PrevVertex
        {
            get
            {
                return this._prevVertex;
            }
            set
            {
                this._prevVertex = value;
                this._hasPrevVertex = true;
            }
        }
    }
}

