namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public abstract class Shape
    {
        internal FP _2radius;
        internal FP _density;
        internal FP _radius;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.ShapeType <ShapeType>k__BackingField;
        public TrueSync.Physics2D.MassData MassData;

        protected Shape(FP density)
        {
            this._density = density;
            this.ShapeType = TrueSync.Physics2D.ShapeType.Unknown;
        }

        public abstract TrueSync.Physics2D.Shape Clone();
        public bool CompareTo(TrueSync.Physics2D.Shape shape)
        {
            if ((shape is PolygonShape) && (this is PolygonShape))
            {
                return ((PolygonShape) this).CompareTo((PolygonShape) shape);
            }
            if ((shape is CircleShape) && (this is CircleShape))
            {
                return ((CircleShape) this).CompareTo((CircleShape) shape);
            }
            if ((shape is EdgeShape) && (this is EdgeShape))
            {
                return ((EdgeShape) this).CompareTo((EdgeShape) shape);
            }
            return (((shape is ChainShape) && (this is ChainShape)) && ((ChainShape) this).CompareTo((ChainShape) shape));
        }

        public abstract void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex);
        protected abstract void ComputeProperties();
        public abstract FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc);
        public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex);
        public abstract bool TestPoint(ref Transform transform, ref TSVector2 point);

        public abstract int ChildCount { get; }

        public FP Density
        {
            get
            {
                return this._density;
            }
            set
            {
                Debug.Assert(value >= 0);
                this._density = value;
                this.ComputeProperties();
            }
        }

        public FP Radius
        {
            get
            {
                return this._radius;
            }
            set
            {
                Debug.Assert(value >= 0);
                this._radius = value;
                this._2radius = this._radius * this._radius;
                this.ComputeProperties();
            }
        }

        public TrueSync.Physics2D.ShapeType ShapeType { get; internal set; }
    }
}

