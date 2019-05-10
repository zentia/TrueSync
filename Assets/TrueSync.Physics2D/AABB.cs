using System;

namespace TrueSync.Physics2D
{
	public struct AABB
	{
		public TSVector2 LowerBound;

		public TSVector2 UpperBound;

		public FP Width
		{
			get
			{
				return this.UpperBound.x - this.LowerBound.x;
			}
		}

		public FP Height
		{
			get
			{
				return this.UpperBound.y - this.LowerBound.y;
			}
		}

		public TSVector2 Center
		{
			get
			{
				return 0.5f * (this.LowerBound + this.UpperBound);
			}
		}

		public TSVector2 Extents
		{
			get
			{
				return 0.5f * (this.UpperBound - this.LowerBound);
			}
		}

		public FP Perimeter
		{
			get
			{
				FP x = this.UpperBound.x - this.LowerBound.x;
				FP y = this.UpperBound.y - this.LowerBound.y;
				return 2f * (x + y);
			}
		}

		public Vertices Vertices
		{
			get
			{
				return new Vertices(4)
				{
					this.UpperBound,
					new TSVector2(this.UpperBound.x, this.LowerBound.y),
					this.LowerBound,
					new TSVector2(this.LowerBound.x, this.UpperBound.y)
				};
			}
		}

		public AABB Q1
		{
			get
			{
				return new AABB(this.Center, this.UpperBound);
			}
		}

		public AABB Q2
		{
			get
			{
				return new AABB(new TSVector2(this.LowerBound.x, this.Center.y), new TSVector2(this.Center.x, this.UpperBound.y));
			}
		}

		public AABB Q3
		{
			get
			{
				return new AABB(this.LowerBound, this.Center);
			}
		}

		public AABB Q4
		{
			get
			{
				return new AABB(new TSVector2(this.Center.x, this.LowerBound.y), new TSVector2(this.UpperBound.x, this.Center.y));
			}
		}

		public AABB(TSVector2 min, TSVector2 max)
		{
			this = new AABB(ref min, ref max);
		}

		public AABB(ref TSVector2 min, ref TSVector2 max)
		{
			this.LowerBound = min;
			this.UpperBound = max;
		}

		public AABB(TSVector2 center, FP width, FP height)
		{
			this.LowerBound = center - new TSVector2(width / 2, height / 2);
			this.UpperBound = center + new TSVector2(width / 2, height / 2);
		}

		public bool IsValid()
		{
			TSVector2 tSVector = this.UpperBound - this.LowerBound;
			bool flag = tSVector.x >= 0f && tSVector.y >= 0f;
			return flag && this.LowerBound.IsValid() && this.UpperBound.IsValid();
		}

		public void Combine(ref AABB aabb)
		{
			TSVector2.Min(ref this.LowerBound, ref aabb.LowerBound, out this.LowerBound);
			TSVector2.Max(ref this.UpperBound, ref aabb.UpperBound, out this.UpperBound);
		}

		public void Combine(ref AABB aabb1, ref AABB aabb2)
		{
			TSVector2.Min(ref aabb1.LowerBound, ref aabb2.LowerBound, out this.LowerBound);
			TSVector2.Max(ref aabb1.UpperBound, ref aabb2.UpperBound, out this.UpperBound);
		}

		public bool Contains(ref AABB aabb)
		{
			bool flag = true;
			flag = (flag && this.LowerBound.x <= aabb.LowerBound.x);
			flag = (flag && this.LowerBound.y <= aabb.LowerBound.y);
			flag = (flag && aabb.UpperBound.x <= this.UpperBound.x);
			return flag && aabb.UpperBound.y <= this.UpperBound.y;
		}

		public bool Contains(ref TSVector2 point)
		{
			return point.x > this.LowerBound.x + Settings.Epsilon && point.x < this.UpperBound.x - Settings.Epsilon && point.y > this.LowerBound.y + Settings.Epsilon && point.y < this.UpperBound.y - Settings.Epsilon;
		}

		public static bool TestOverlap(ref AABB a, ref AABB b)
		{
			TSVector2 tSVector = b.LowerBound - a.UpperBound;
			TSVector2 tSVector2 = a.LowerBound - b.UpperBound;
			bool flag = tSVector.x > 0f || tSVector.y > 0f;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = tSVector2.x > 0f || tSVector2.y > 0f;
				result = !flag2;
			}
			return result;
		}

		public bool RayCast(out RayCastOutput output, ref RayCastInput input, bool doInteriorCheck = true)
		{
			output = default(RayCastOutput);
			FP fP = -Settings.MaxFP;
			FP fP2 = Settings.MaxFP;
			TSVector2 point = input.Point1;
			TSVector2 tSVector = input.Point2 - input.Point1;
			TSVector2 tSVector2 = MathUtils.Abs(tSVector);
			TSVector2 zero = TSVector2.zero;
			bool result;
			for (int i = 0; i < 2; i++)
			{
				FP x = (i == 0) ? tSVector2.x : tSVector2.y;
				FP fP3 = (i == 0) ? this.LowerBound.x : this.LowerBound.y;
				FP x2 = (i == 0) ? this.UpperBound.x : this.UpperBound.y;
				FP fP4 = (i == 0) ? point.x : point.y;
				bool flag = x < Settings.Epsilon;
				if (flag)
				{
					bool flag2 = fP4 < fP3 || x2 < fP4;
					if (flag2)
					{
						result = false;
						return result;
					}
				}
				else
				{
					FP y = (i == 0) ? tSVector.x : tSVector.y;
					FP y2 = 1f / y;
					FP fP5 = (fP3 - fP4) * y2;
					FP fP6 = (x2 - fP4) * y2;
					FP fP7 = -1f;
					bool flag3 = fP5 > fP6;
					if (flag3)
					{
						MathUtils.Swap<FP>(ref fP5, ref fP6);
						fP7 = 1f;
					}
					bool flag4 = fP5 > fP;
					if (flag4)
					{
						bool flag5 = i == 0;
						if (flag5)
						{
							zero.x = fP7;
						}
						else
						{
							zero.y = fP7;
						}
						fP = fP5;
					}
					fP2 = TSMath.Min(fP2, fP6);
					bool flag6 = fP > fP2;
					if (flag6)
					{
						result = false;
						return result;
					}
				}
			}
			bool flag7 = doInteriorCheck && (fP < 0f || input.MaxFraction < fP);
			if (flag7)
			{
				result = false;
				return result;
			}
			output.Fraction = fP;
			output.Normal = zero;
			result = true;
			return result;
		}
	}
}
