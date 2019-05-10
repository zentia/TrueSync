using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class PolygonShape : Shape
	{
		internal Vertices _vertices;

		internal Vertices _normals;

		public Vertices Vertices
		{
			get
			{
				return this._vertices;
			}
			set
			{
				this._vertices = new Vertices(value);
				Debug.Assert(this._vertices.Count >= 3 && this._vertices.Count <= Settings.MaxPolygonVertices);
				bool useConvexHullPolygons = Settings.UseConvexHullPolygons;
				if (useConvexHullPolygons)
				{
					bool flag = this._vertices.Count <= 3;
					if (flag)
					{
						this._vertices.ForceCounterClockWise();
					}
					else
					{
						this._vertices = GiftWrap.GetConvexHull(this._vertices);
					}
				}
				this._vertices.ForceCounterClockWise();
				this._normals = new Vertices(this._vertices.Count);
				for (int i = 0; i < this._vertices.Count; i++)
				{
					int index = (i + 1 < this._vertices.Count) ? (i + 1) : 0;
					TSVector2 tSVector = this._vertices[index] - this._vertices[i];
					TSVector2 item = new TSVector2(tSVector.y, -tSVector.x);
					item.Normalize();
					this._normals.Add(item);
				}
				this.ComputeProperties();
			}
		}

		public Vertices Normals
		{
			get
			{
				return this._normals;
			}
		}

		public override int ChildCount
		{
			get
			{
				return 1;
			}
		}

		public PolygonShape(Vertices vertices, FP density) : base(density)
		{
			base.ShapeType = ShapeType.Polygon;
			this._radius = Settings.PolygonRadius;
			this.Vertices = vertices;
		}

		public PolygonShape(FP density) : base(density)
		{
			Debug.Assert(density >= 0f);
			base.ShapeType = ShapeType.Polygon;
			this._radius = Settings.PolygonRadius;
			this._vertices = new Vertices(Settings.MaxPolygonVertices);
			this._normals = new Vertices(Settings.MaxPolygonVertices);
		}

		internal PolygonShape() : base(0)
		{
			base.ShapeType = ShapeType.Polygon;
			this._radius = Settings.PolygonRadius;
			this._vertices = new Vertices(Settings.MaxPolygonVertices);
			this._normals = new Vertices(Settings.MaxPolygonVertices);
		}

		protected override void ComputeProperties()
		{
			Debug.Assert(this.Vertices.Count >= 3);
			bool flag = this._density <= 0;
			if (!flag)
			{
				TSVector2 tSVector = TSVector2.zero;
				FP fP = 0f;
				FP fP2 = 0f;
				TSVector2 tSVector2 = TSVector2.zero;
				for (int i = 0; i < this.Vertices.Count; i++)
				{
					tSVector2 += this.Vertices[i];
				}
				tSVector2 *= 1f / (float)this.Vertices.Count;
				FP y = 0.333333343f;
				for (int j = 0; j < this.Vertices.Count; j++)
				{
					TSVector2 tSVector3 = this.Vertices[j] - tSVector2;
					TSVector2 tSVector4 = (j + 1 < this.Vertices.Count) ? (this.Vertices[j + 1] - tSVector2) : (this.Vertices[0] - tSVector2);
					FP y2 = MathUtils.Cross(tSVector3, tSVector4);
					FP fP3 = 0.5f * y2;
					fP += fP3;
					tSVector += fP3 * y * (tSVector3 + tSVector4);
					FP x = tSVector3.x;
					FP y3 = tSVector3.y;
					FP x2 = tSVector4.x;
					FP y4 = tSVector4.y;
					FP x3 = x * x + x2 * x + x2 * x2;
					FP y5 = y3 * y3 + y4 * y3 + y4 * y4;
					fP2 += 0.25f * y * y2 * (x3 + y5);
				}
				Debug.Assert(fP > Settings.Epsilon);
				this.MassData.Area = fP;
				this.MassData.Mass = this._density * fP;
				tSVector *= 1f / fP;
				this.MassData.Centroid = tSVector + tSVector2;
				this.MassData.Inertia = this._density * fP2;
				this.MassData.Inertia = this.MassData.Inertia + this.MassData.Mass * (TSVector2.Dot(this.MassData.Centroid, this.MassData.Centroid) - TSVector2.Dot(tSVector, tSVector));
			}
		}

		public override bool TestPoint(ref Transform transform, ref TSVector2 point)
		{
			TSVector2 value = MathUtils.MulT(transform.q, point - transform.p);
			bool result;
			for (int i = 0; i < this.Vertices.Count; i++)
			{
				FP x = TSVector2.Dot(this.Normals[i], value - this.Vertices[i]);
				bool flag = x > 0f;
				if (flag)
				{
					result = false;
					return result;
				}
			}
			result = true;
			return result;
		}

		public override bool RayCast(out RayCastOutput output, ref RayCastInput input, ref Transform transform, int childIndex)
		{
			output = default(RayCastOutput);
			TSVector2 value = MathUtils.MulT(transform.q, input.Point1 - transform.p);
			TSVector2 value2 = MathUtils.MulT(transform.q, input.Point2 - transform.p);
			TSVector2 value3 = value2 - value;
			FP fP = 0f;
			FP x = input.MaxFraction;
			int num = -1;
			int i = 0;
			bool result;
			while (i < this.Vertices.Count)
			{
				FP x2 = TSVector2.Dot(this.Normals[i], this.Vertices[i] - value);
				FP fP2 = TSVector2.Dot(this.Normals[i], value3);
				bool flag = fP2 == 0f;
				if (flag)
				{
					bool flag2 = x2 < 0f;
					if (flag2)
					{
						result = false;
						return result;
					}
				}
				else
				{
					bool flag3 = fP2 < 0f && x2 < fP * fP2;
					if (flag3)
					{
						fP = x2 / fP2;
						num = i;
					}
					else
					{
						bool flag4 = fP2 > 0f && x2 < x * fP2;
						if (flag4)
						{
							x = x2 / fP2;
						}
					}
				}
				bool flag5 = x < fP;
				if (!flag5)
				{
					i++;
					continue;
				}
				result = false;
				return result;
			}
			Debug.Assert(0f <= fP && fP <= input.MaxFraction);
			bool flag6 = num >= 0;
			if (flag6)
			{
				output.Fraction = fP;
				output.Normal = MathUtils.Mul(transform.q, this.Normals[num]);
				result = true;
				return result;
			}
			result = false;
			return result;
		}

		public override void ComputeAABB(out AABB aabb, ref Transform transform, int childIndex)
		{
			TSVector2 tSVector = MathUtils.Mul(ref transform, this.Vertices[0]);
			TSVector2 value = tSVector;
			for (int i = 1; i < this.Vertices.Count; i++)
			{
				TSVector2 value2 = MathUtils.Mul(ref transform, this.Vertices[i]);
				tSVector = TSVector2.Min(tSVector, value2);
				value = TSVector2.Max(value, value2);
			}
			TSVector2 value3 = new TSVector2(base.Radius, base.Radius);
			aabb.LowerBound = tSVector - value3;
			aabb.UpperBound = value + value3;
		}

		public override FP ComputeSubmergedArea(ref TSVector2 normal, FP offset, ref Transform xf, out TSVector2 sc)
		{
			sc = TSVector2.zero;
			TSVector2 value = MathUtils.MulT(xf.q, normal);
			FP y = offset - TSVector2.Dot(normal, xf.p);
			FP[] array = new FP[Settings.MaxPolygonVertices];
			int num = 0;
			int num2 = -1;
			int num3 = -1;
			bool flag = false;
			for (int i = 0; i < this.Vertices.Count; i++)
			{
				array[i] = TSVector2.Dot(value, this.Vertices[i]) - y;
				bool flag2 = array[i] < -Settings.Epsilon;
				bool flag3 = i > 0;
				if (flag3)
				{
					bool flag4 = flag2;
					if (flag4)
					{
						bool flag5 = !flag;
						if (flag5)
						{
							num2 = i - 1;
							num++;
						}
					}
					else
					{
						bool flag6 = flag;
						if (flag6)
						{
							num3 = i - 1;
							num++;
						}
					}
				}
				flag = flag2;
			}
			int num4 = num;
			FP result;
			if (num4 != 0)
			{
				if (num4 == 1)
				{
					bool flag7 = num2 == -1;
					if (flag7)
					{
						num2 = this.Vertices.Count - 1;
					}
					else
					{
						num3 = this.Vertices.Count - 1;
					}
				}
				int num5 = (num2 + 1) % this.Vertices.Count;
				int num6 = (num3 + 1) % this.Vertices.Count;
				FP y2 = (0 - array[num2]) / (array[num5] - array[num2]);
				FP y3 = (0 - array[num3]) / (array[num6] - array[num3]);
				TSVector2 tSVector = new TSVector2(this.Vertices[num2].x * (1 - y2) + this.Vertices[num5].x * y2, this.Vertices[num2].y * (1 - y2) + this.Vertices[num5].y * y2);
				TSVector2 tSVector2 = new TSVector2(this.Vertices[num3].x * (1 - y3) + this.Vertices[num6].x * y3, this.Vertices[num3].y * (1 - y3) + this.Vertices[num6].y * y3);
				FP fP = 0;
				TSVector2 tSVector3 = new TSVector2(0, 0);
				TSVector2 tSVector4 = this.Vertices[num5];
				FP y4 = 0.333333343f;
				int i = num5;
				while (i != num6)
				{
					i = (i + 1) % this.Vertices.Count;
					bool flag8 = i == num6;
					TSVector2 tSVector5;
					if (flag8)
					{
						tSVector5 = tSVector2;
					}
					else
					{
						tSVector5 = this.Vertices[i];
					}
					TSVector2 a = tSVector4 - tSVector;
					TSVector2 b = tSVector5 - tSVector;
					FP y5 = MathUtils.Cross(a, b);
					FP fP2 = 0.5f * y5;
					fP += fP2;
					tSVector3 += fP2 * y4 * (tSVector + tSVector4 + tSVector5);
					tSVector4 = tSVector5;
				}
				tSVector3 *= 1f / fP;
				sc = MathUtils.Mul(ref xf, tSVector3);
				result = fP;
			}
			else
			{
				bool flag9 = flag;
				if (flag9)
				{
					sc = MathUtils.Mul(ref xf, this.MassData.Centroid);
					result = this.MassData.Mass / base.Density;
				}
				else
				{
					result = 0;
				}
			}
			return result;
		}

		public bool CompareTo(PolygonShape shape)
		{
			bool flag = this.Vertices.Count != shape.Vertices.Count;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < this.Vertices.Count; i++)
				{
					bool flag2 = this.Vertices[i] != shape.Vertices[i];
					if (flag2)
					{
						result = false;
						return result;
					}
				}
				result = (base.Radius == shape.Radius && this.MassData == shape.MassData);
			}
			return result;
		}

		public override Shape Clone()
		{
			return new PolygonShape
			{
				ShapeType = base.ShapeType,
				_radius = this._radius,
				_density = this._density,
				_vertices = new Vertices(this._vertices),
				_normals = new Vertices(this._normals),
				MassData = this.MassData
			};
		}
	}
}
