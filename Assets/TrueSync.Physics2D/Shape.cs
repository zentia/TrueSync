using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public abstract class Shape
	{
		internal FP _density;

		internal FP _radius;

		internal FP _2radius;

		public MassData MassData;

		public ShapeType ShapeType
		{
			get;
			internal set;
		}

		public abstract int ChildCount
		{
			get;
		}

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

		protected Shape(FP density)
		{
			this._density = density;
			this.ShapeType = ShapeType.Unknown;
		}

		public abstract Shape Clone();

		public abstract bool TestPoint(ref Transform transform, ref TSVector2 point);

		public abstract bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex);

		public abstract void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex);

		protected abstract void ComputeProperties();

		public bool CompareTo(Shape shape)
		{
			bool flag = shape is PolygonShape && this is PolygonShape;
			bool result;
			if (flag)
			{
				result = ((PolygonShape)this).CompareTo((PolygonShape)shape);
			}
			else
			{
				bool flag2 = shape is CircleShape && this is CircleShape;
				if (flag2)
				{
					result = ((CircleShape)this).CompareTo((CircleShape)shape);
				}
				else
				{
					bool flag3 = shape is EdgeShape && this is EdgeShape;
					if (flag3)
					{
						result = ((EdgeShape)this).CompareTo((EdgeShape)shape);
					}
					else
					{
						bool flag4 = shape is ChainShape && this is ChainShape;
						result = (flag4 && ((ChainShape)this).CompareTo((ChainShape)shape));
					}
				}
			}
			return result;
		}

		public abstract FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc);
	}
}
