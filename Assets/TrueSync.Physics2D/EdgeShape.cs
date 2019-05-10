using System;

namespace TrueSync.Physics2D
{
	public class EdgeShape : Shape
	{
		internal TSVector2 _vertex1;

		internal TSVector2 _vertex2;

		public override int ChildCount
		{
			get
			{
				return 1;
			}
		}

		public bool HasVertex0
		{
			get;
			set;
		}

		public bool HasVertex3
		{
			get;
			set;
		}

		public TSVector2 Vertex0
		{
			get;
			set;
		}

		public TSVector2 Vertex3
		{
			get;
			set;
		}

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

		internal EdgeShape() : base(0)
		{
			base.ShapeType = ShapeType.Edge;
			this._radius = Settings.PolygonRadius;
		}

		public EdgeShape(TSVector2 start, TSVector2 end) : base(0)
		{
			base.ShapeType = ShapeType.Edge;
			this._radius = Settings.PolygonRadius;
			this.Set(start, end);
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
			output = default(RayCastOutput);
			TSVector2 tSVector = MathUtils.MulT(transform.q, input.Point1 - transform.p);
			TSVector2 value = MathUtils.MulT(transform.q, input.Point2 - transform.p);
			TSVector2 tSVector2 = value - tSVector;
			TSVector2 vertex = this._vertex1;
			TSVector2 vertex2 = this._vertex2;
			TSVector2 tSVector3 = vertex2 - vertex;
			TSVector2 tSVector4 = new TSVector2(tSVector3.y, -tSVector3.x);
			tSVector4.Normalize();
			FP x = TSVector2.Dot(tSVector4, vertex - tSVector);
			FP fP = TSVector2.Dot(tSVector4, tSVector2);
			bool flag = fP == 0f;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				FP fP2 = x / fP;
				bool flag2 = fP2 < 0f || input.MaxFraction < fP2;
				if (flag2)
				{
					result = false;
				}
				else
				{
					TSVector2 value2 = tSVector + fP2 * tSVector2;
					TSVector2 tSVector5 = vertex2 - vertex;
					FP fP3 = TSVector2.Dot(tSVector5, tSVector5);
					bool flag3 = fP3 == 0f;
					if (flag3)
					{
						result = false;
					}
					else
					{
						FP fP4 = TSVector2.Dot(value2 - vertex, tSVector5) / fP3;
						bool flag4 = fP4 < 0f || 1f < fP4;
						if (flag4)
						{
							result = false;
						}
						else
						{
							output.Fraction = fP2;
							bool flag5 = x > 0f;
							if (flag5)
							{
								output.Normal = -tSVector4;
							}
							else
							{
								output.Normal = tSVector4;
							}
							result = true;
						}
					}
				}
			}
			return result;
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			TSVector2 value = MathUtils.Mul(ref transform, this._vertex1);
			TSVector2 value2 = MathUtils.Mul(ref transform, this._vertex2);
			TSVector2 value3 = TSVector2.Min(value, value2);
			TSVector2 value4 = TSVector2.Max(value, value2);
			TSVector2 value5 = new TSVector2(base.Radius, base.Radius);
			aabb.LowerBound = value3 - value5;
			aabb.UpperBound = value4 + value5;
		}

		protected override void ComputeProperties()
		{
			this.MassData.Centroid = 0.5f * (this._vertex1 + this._vertex2);
		}

		public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
		{
			sc = TSVector2.zero;
			return 0;
		}

		public bool CompareTo(EdgeShape shape)
		{
			return this.HasVertex0 == shape.HasVertex0 && this.HasVertex3 == shape.HasVertex3 && this.Vertex0 == shape.Vertex0 && this.Vertex1 == shape.Vertex1 && this.Vertex2 == shape.Vertex2 && this.Vertex3 == shape.Vertex3;
		}

		public override Shape Clone()
		{
			return new EdgeShape
			{
				ShapeType = base.ShapeType,
				_radius = this._radius,
				_density = this._density,
				HasVertex0 = this.HasVertex0,
				HasVertex3 = this.HasVertex3,
				Vertex0 = this.Vertex0,
				_vertex1 = this._vertex1,
				_vertex2 = this._vertex2,
				Vertex3 = this.Vertex3,
				MassData = this.MassData
			};
		}
	}
}
