using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class CircleShape : Shape
	{
		internal TSVector2 _position;

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

		public CircleShape(FP radius, FP density) : base(density)
		{
			Debug.Assert(radius >= 0);
			Debug.Assert(density >= 0);
			base.ShapeType = ShapeType.Circle;
			this._position = TSVector2.zero;
			base.Radius = radius;
		}

		internal CircleShape() : base(0)
		{
			base.ShapeType = ShapeType.Circle;
			this._radius = 0f;
			this._position = TSVector2.zero;
		}

		public override bool TestPoint(ref Transform transform, ref TSVector2 point)
		{
			TSVector2 value = transform.p + MathUtils.Mul(transform.q, this.Position);
			TSVector2 tSVector = point - value;
			return TSVector2.Dot(tSVector, tSVector) <= this._2radius;
		}

		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			output = default(RayCastOutput);
			TSVector2 value = transform.p + MathUtils.Mul(transform.q, this.Position);
			TSVector2 tSVector = input.Point1 - value;
			FP y = TSVector2.Dot(tSVector, tSVector) - this._2radius;
			TSVector2 tSVector2 = input.Point2 - input.Point1;
			FP fP = TSVector2.Dot(tSVector, tSVector2);
			FP fP2 = TSVector2.Dot(tSVector2, tSVector2);
			FP x = fP * fP - fP2 * y;
			bool flag = x < 0f || fP2 < Settings.Epsilon;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				FP fP3 = -(fP + FP.Sqrt(x));
				bool flag2 = 0f <= fP3 && fP3 <= input.MaxFraction * fP2;
				if (flag2)
				{
					fP3 /= fP2;
					output.Fraction = fP3;
					output.Normal = tSVector + fP3 * tSVector2;
					output.Normal.Normalize();
					result = true;
				}
				else
				{
					result = false;
				}
			}
			return result;
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			TSVector2 tSVector = transform.p + MathUtils.Mul(transform.q, this.Position);
			aabb.LowerBound = new TSVector2(tSVector.x - base.Radius, tSVector.y - base.Radius);
			aabb.UpperBound = new TSVector2(tSVector.x + base.Radius, tSVector.y + base.Radius);
		}

		protected sealed override void ComputeProperties()
		{
			FP fP = Settings.Pi * this._2radius;
			this.MassData.Area = fP;
			this.MassData.Mass = base.Density * fP;
			this.MassData.Centroid = this.Position;
			this.MassData.Inertia = this.MassData.Mass * (0.5f * this._2radius + TSVector2.Dot(this.Position, this.Position));
		}

		public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
		{
			sc = TSVector2.zero;
			TSVector2 tSVector = MathUtils.Mul(ref xf, this.Position);
			FP fP = -(TSVector2.Dot(normal, tSVector) - offset);
			bool flag = fP < -base.Radius + Settings.Epsilon;
			FP result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				bool flag2 = fP > base.Radius;
				if (flag2)
				{
					sc = tSVector;
					result = Settings.Pi * this._2radius;
				}
				else
				{
					FP y = fP * fP;
					FP fP2 = this._2radius * (TSMath.Asin(fP / base.Radius) + TSMath.PiOver2 + fP * TSMath.Sqrt(this._2radius - y));
					FP y2 = new FP(-2) / new FP(3) * Math.Pow((double)(this._2radius - y).AsFloat(), 1.5) / fP2;
					sc.x = tSVector.x + normal.x * y2;
					sc.y = tSVector.y + normal.y * y2;
					result = fP2;
				}
			}
			return result;
		}

		public bool CompareTo(CircleShape shape)
		{
			return base.Radius == shape.Radius && this.Position == shape.Position;
		}

		public override Shape Clone()
		{
			return new CircleShape
			{
				ShapeType = base.ShapeType,
				_radius = base.Radius,
				_2radius = this._2radius,
				_density = this._density,
				_position = this._position,
				MassData = this.MassData
			};
		}
	}
}
