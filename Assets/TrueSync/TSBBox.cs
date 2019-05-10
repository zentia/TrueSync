using System;

namespace TrueSync
{
	public struct TSBBox
	{
		public enum ContainmentType
		{
			Disjoint,
			Contains,
			Intersects
		}

		public TSVector min;

		public TSVector max;

		public static readonly TSBBox LargeBox;

		public static readonly TSBBox SmallBox;

		public TSVector center
		{
			get
			{
				return (this.min + this.max) * FP.Half;
			}
		}

		public TSVector size
		{
			get
			{
				return this.max - this.min;
			}
		}

		public TSVector extents
		{
			get
			{
				return this.size * FP.Half;
			}
		}

		internal FP Perimeter
		{
			get
			{
				return 2 * FP.One * ((this.max.x - this.min.x) * (this.max.y - this.min.y) + (this.max.x - this.min.x) * (this.max.z - this.min.z) + (this.max.z - this.min.z) * (this.max.y - this.min.y));
			}
		}

		static TSBBox()
		{
			TSBBox.LargeBox.min = new TSVector(FP.MinValue);
			TSBBox.LargeBox.max = new TSVector(FP.MaxValue);
			TSBBox.SmallBox.min = new TSVector(FP.MaxValue);
			TSBBox.SmallBox.max = new TSVector(FP.MinValue);
		}

		public TSBBox(TSVector min, TSVector max)
		{
			this.min = min;
			this.max = max;
		}

		internal void InverseTransform(ref TSVector position, ref TSMatrix orientation)
		{
			TSVector.Subtract(ref this.max, ref position, out this.max);
			TSVector.Subtract(ref this.min, ref position, out this.min);
			TSVector tSVector;
			TSVector.Add(ref this.max, ref this.min, out tSVector);
			tSVector.x *= FP.Half;
			tSVector.y *= FP.Half;
			tSVector.z *= FP.Half;
			TSVector tSVector2;
			TSVector.Subtract(ref this.max, ref this.min, out tSVector2);
			tSVector2.x *= FP.Half;
			tSVector2.y *= FP.Half;
			tSVector2.z *= FP.Half;
			TSVector.TransposedTransform(ref tSVector, ref orientation, out tSVector);
			TSMatrix tSMatrix;
			TSMath.Absolute(ref orientation, out tSMatrix);
			TSVector.TransposedTransform(ref tSVector2, ref tSMatrix, out tSVector2);
			TSVector.Add(ref tSVector, ref tSVector2, out this.max);
			TSVector.Subtract(ref tSVector, ref tSVector2, out this.min);
		}

		public void Transform(ref TSMatrix orientation)
		{
			TSVector value = FP.Half * (this.max - this.min);
			TSVector value2 = FP.Half * (this.max + this.min);
			TSVector.Transform(ref value2, ref orientation, out value2);
			TSMatrix tSMatrix;
			TSMath.Absolute(ref orientation, out tSMatrix);
			TSVector.Transform(ref value, ref tSMatrix, out value);
			this.max = value2 + value;
			this.min = value2 - value;
		}

		private bool Intersect1D(FP start, FP dir, FP min, FP max, ref FP enter, ref FP exit)
		{
			bool flag = dir * dir < TSMath.Epsilon * TSMath.Epsilon;
			bool result;
			if (flag)
			{
				result = (start >= min && start <= max);
			}
			else
			{
				FP fP = (min - start) / dir;
				FP fP2 = (max - start) / dir;
				bool flag2 = fP > fP2;
				if (flag2)
				{
					FP fP3 = fP;
					fP = fP2;
					fP2 = fP3;
				}
				bool flag3 = fP > exit || fP2 < enter;
				if (flag3)
				{
					result = false;
				}
				else
				{
					bool flag4 = fP > enter;
					if (flag4)
					{
						enter = fP;
					}
					bool flag5 = fP2 < exit;
					if (flag5)
					{
						exit = fP2;
					}
					result = true;
				}
			}
			return result;
		}

		public bool SegmentIntersect(ref TSVector origin, ref TSVector direction)
		{
			FP zero = FP.Zero;
			FP one = FP.One;
			bool flag = !this.Intersect1D(origin.x, direction.x, this.min.x, this.max.x, ref zero, ref one);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = !this.Intersect1D(origin.y, direction.y, this.min.y, this.max.y, ref zero, ref one);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !this.Intersect1D(origin.z, direction.z, this.min.z, this.max.z, ref zero, ref one);
					result = !flag3;
				}
			}
			return result;
		}

		public bool RayIntersect(ref TSVector origin, ref TSVector direction)
		{
			FP zero = FP.Zero;
			FP maxValue = FP.MaxValue;
			bool flag = !this.Intersect1D(origin.x, direction.x, this.min.x, this.max.x, ref zero, ref maxValue);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = !this.Intersect1D(origin.y, direction.y, this.min.y, this.max.y, ref zero, ref maxValue);
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = !this.Intersect1D(origin.z, direction.z, this.min.z, this.max.z, ref zero, ref maxValue);
					result = !flag3;
				}
			}
			return result;
		}

		public bool SegmentIntersect(TSVector origin, TSVector direction)
		{
			return this.SegmentIntersect(ref origin, ref direction);
		}

		public bool RayIntersect(TSVector origin, TSVector direction)
		{
			return this.RayIntersect(ref origin, ref direction);
		}

		public TSBBox.ContainmentType Contains(TSVector point)
		{
			return this.Contains(ref point);
		}

		public TSBBox.ContainmentType Contains(ref TSVector point)
		{
			return (this.min.x <= point.x && point.x <= this.max.x && this.min.y <= point.y && point.y <= this.max.y && this.min.z <= point.z && point.z <= this.max.z) ? TSBBox.ContainmentType.Contains : TSBBox.ContainmentType.Disjoint;
		}

		public void GetCorners(TSVector[] corners)
		{
			corners[0].Set(this.min.x, this.max.y, this.max.z);
			corners[1].Set(this.max.x, this.max.y, this.max.z);
			corners[2].Set(this.max.x, this.min.y, this.max.z);
			corners[3].Set(this.min.x, this.min.y, this.max.z);
			corners[4].Set(this.min.x, this.max.y, this.min.z);
			corners[5].Set(this.max.x, this.max.y, this.min.z);
			corners[6].Set(this.max.x, this.min.y, this.min.z);
			corners[7].Set(this.min.x, this.min.y, this.min.z);
		}

		public void AddPoint(TSVector point)
		{
			this.AddPoint(ref point);
		}

		public void AddPoint(ref TSVector point)
		{
			TSVector.Max(ref this.max, ref point, out this.max);
			TSVector.Min(ref this.min, ref point, out this.min);
		}

		public static TSBBox CreateFromPoints(TSVector[] points)
		{
			TSVector tSVector = new TSVector(FP.MaxValue);
			TSVector tSVector2 = new TSVector(FP.MinValue);
			for (int i = 0; i < points.Length; i++)
			{
				TSVector.Min(ref tSVector, ref points[i], out tSVector);
				TSVector.Max(ref tSVector2, ref points[i], out tSVector2);
			}
			return new TSBBox(tSVector, tSVector2);
		}

		public TSBBox.ContainmentType Contains(TSBBox box)
		{
			return this.Contains(ref box);
		}

		public TSBBox.ContainmentType Contains(ref TSBBox box)
		{
			TSBBox.ContainmentType result = TSBBox.ContainmentType.Disjoint;
			bool flag = this.max.x >= box.min.x && this.min.x <= box.max.x && this.max.y >= box.min.y && this.min.y <= box.max.y && this.max.z >= box.min.z && this.min.z <= box.max.z;
			if (flag)
			{
				result = ((this.min.x <= box.min.x && box.max.x <= this.max.x && this.min.y <= box.min.y && box.max.y <= this.max.y && this.min.z <= box.min.z && box.max.z <= this.max.z) ? TSBBox.ContainmentType.Contains : TSBBox.ContainmentType.Intersects);
			}
			return result;
		}

		public static TSBBox CreateFromCenter(TSVector center, TSVector size)
		{
			TSVector value = size * FP.Half;
			return new TSBBox(center - value, center + value);
		}

		public static TSBBox CreateMerged(TSBBox original, TSBBox additional)
		{
			TSBBox result;
			TSBBox.CreateMerged(ref original, ref additional, out result);
			return result;
		}

		public static void CreateMerged(ref TSBBox original, ref TSBBox additional, out TSBBox result)
		{
			TSVector tSVector;
			TSVector.Min(ref original.min, ref additional.min, out tSVector);
			TSVector tSVector2;
			TSVector.Max(ref original.max, ref additional.max, out tSVector2);
			result.min = tSVector;
			result.max = tSVector2;
		}

		public override string ToString()
		{
			return this.min + "|" + this.max;
		}
	}
}
