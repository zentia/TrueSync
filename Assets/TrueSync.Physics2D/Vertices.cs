using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TrueSync.Physics2D
{
	[DebuggerDisplay("Count = {Count} Vertices = {ToString()}")]
	public class Vertices : List<TSVector2>
	{
		internal bool AttachedToBody
		{
			get;
			set;
		}

		public List<Vertices> Holes
		{
			get;
			set;
		}

		public Vertices()
		{
		}

		public Vertices(int capacity) : base(capacity)
		{
		}

		public Vertices(IEnumerable<TSVector2> vertices)
		{
			base.AddRange(vertices);
		}

		public int NextIndex(int index)
		{
			return (index + 1 > base.Count - 1) ? 0 : (index + 1);
		}

		public TSVector2 NextVertex(int index)
		{
			return base[this.NextIndex(index)];
		}

		public int PreviousIndex(int index)
		{
			return (index - 1 < 0) ? (base.Count - 1) : (index - 1);
		}

		public TSVector2 PreviousVertex(int index)
		{
			return base[this.PreviousIndex(index)];
		}

		public FP GetSignedArea()
		{
			bool flag = base.Count < 3;
			FP result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				FP fP = 0;
				for (int i = 0; i < base.Count; i++)
				{
					int index = (i + 1) % base.Count;
					TSVector2 tSVector = base[i];
					TSVector2 tSVector2 = base[index];
					fP += tSVector.x * tSVector2.y;
					fP -= tSVector.y * tSVector2.x;
				}
				fP /= 2f;
				result = fP;
			}
			return result;
		}

		public FP GetArea()
		{
			FP signedArea = this.GetSignedArea();
			return (signedArea < 0) ? (-signedArea) : signedArea;
		}

		public TSVector2 GetCentroid()
		{
			bool flag = base.Count < 3;
			TSVector2 result;
			if (flag)
			{
				result = new TSVector2(FP.NaN, FP.NaN);
			}
			else
			{
				TSVector2 tSVector = TSVector2.zero;
				FP fP = 0f;
				FP y = 0.333333343f;
				for (int i = 0; i < base.Count; i++)
				{
					TSVector2 tSVector2 = base[i];
					TSVector2 tSVector3 = (i + 1 < base.Count) ? base[i + 1] : base[0];
					FP fP2 = 0.5f * (tSVector2.x * tSVector3.y - tSVector2.y * tSVector3.x);
					fP += fP2;
					tSVector += fP2 * y * (tSVector2 + tSVector3);
				}
				tSVector *= 1f / fP;
				result = tSVector;
			}
			return result;
		}

		public AABB GetAABB()
		{
			TSVector2 tSVector = new TSVector2(FP.MaxValue, FP.MaxValue);
			TSVector2 tSVector2 = new TSVector2(FP.MinValue, FP.MinValue);
			for (int i = 0; i < base.Count; i++)
			{
				bool flag = base[i].x < tSVector.x;
				if (flag)
				{
					tSVector.x = base[i].x;
				}
				bool flag2 = base[i].x > tSVector2.x;
				if (flag2)
				{
					tSVector2.x = base[i].x;
				}
				bool flag3 = base[i].y < tSVector.y;
				if (flag3)
				{
					tSVector.y = base[i].y;
				}
				bool flag4 = base[i].y > tSVector2.y;
				if (flag4)
				{
					tSVector2.y = base[i].y;
				}
			}
			AABB result;
			result.LowerBound = tSVector;
			result.UpperBound = tSVector2;
			return result;
		}

		public void Translate(TSVector2 value)
		{
			this.Translate(ref value);
		}

		public void Translate(ref TSVector2 value)
		{
			Debug.Assert(!this.AttachedToBody, "Translating vertices that are used by a Body can result in unstable behavior. Use Body.Position instead.");
			for (int i = 0; i < base.Count; i++)
			{
				base[i] = TSVector2.Add(base[i], value);
			}
			bool flag = this.Holes != null && this.Holes.Count > 0;
			if (flag)
			{
				foreach (Vertices current in this.Holes)
				{
					current.Translate(ref value);
				}
			}
		}

		public void Scale(TSVector2 value)
		{
			this.Scale(ref value);
		}

		public void Scale(ref TSVector2 value)
		{
			Debug.Assert(!this.AttachedToBody, "Scaling vertices that are used by a Body can result in unstable behavior.");
			for (int i = 0; i < base.Count; i++)
			{
				base[i] = TSVector2.Multiply(base[i], value);
			}
			bool flag = this.Holes != null && this.Holes.Count > 0;
			if (flag)
			{
				foreach (Vertices current in this.Holes)
				{
					current.Scale(ref value);
				}
			}
		}

		public void Rotate(FP value)
		{
			Debug.Assert(!this.AttachedToBody, "Rotating vertices that are used by a Body can result in unstable behavior.");
			FP y = FP.Cos(value);
			FP fP = FP.Sin(value);
			for (int i = 0; i < base.Count; i++)
			{
				TSVector2 tSVector = base[i];
				base[i] = new TSVector2(tSVector.x * y + tSVector.y * -fP, tSVector.x * fP + tSVector.y * y);
			}
			bool flag = this.Holes != null && this.Holes.Count > 0;
			if (flag)
			{
				foreach (Vertices current in this.Holes)
				{
					current.Rotate(value);
				}
			}
		}

		public bool IsConvex()
		{
			bool flag = base.Count < 3;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = base.Count == 3;
				if (flag2)
				{
					result = true;
				}
				else
				{
					for (int i = 0; i < base.Count; i++)
					{
						int num = (i + 1 < base.Count) ? (i + 1) : 0;
						TSVector2 tSVector = base[num] - base[i];
						for (int j = 0; j < base.Count; j++)
						{
							bool flag3 = j == i || j == num;
							if (!flag3)
							{
								TSVector2 tSVector2 = base[j] - base[i];
								FP x = tSVector.x * tSVector2.y - tSVector.y * tSVector2.x;
								bool flag4 = x <= 0f;
								if (flag4)
								{
									result = false;
									return result;
								}
							}
						}
					}
					result = true;
				}
			}
			return result;
		}

		public bool IsCounterClockWise()
		{
			bool flag = base.Count < 3;
			return !flag && this.GetSignedArea() > 0f;
		}

		public void ForceCounterClockWise()
		{
			bool flag = base.Count < 3;
			if (!flag)
			{
				bool flag2 = !this.IsCounterClockWise();
				if (flag2)
				{
					base.Reverse();
				}
			}
		}

		public bool IsSimple()
		{
			bool flag = base.Count < 3;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				for (int i = 0; i < base.Count; i++)
				{
					TSVector2 tSVector = base[i];
					TSVector2 tSVector2 = this.NextVertex(i);
					for (int j = i + 1; j < base.Count; j++)
					{
						TSVector2 tSVector3 = base[j];
						TSVector2 tSVector4 = this.NextVertex(j);
						TSVector2 tSVector5;
						bool flag2 = LineTools.LineIntersect2(ref tSVector, ref tSVector2, ref tSVector3, ref tSVector4, out tSVector5);
						if (flag2)
						{
							result = false;
							return result;
						}
					}
				}
				result = true;
			}
			return result;
		}

		public PolygonError CheckPolygon()
		{
			bool flag = base.Count < 3 || base.Count > Settings.MaxPolygonVertices;
			PolygonError result;
			if (flag)
			{
				result = PolygonError.InvalidAmountOfVertices;
			}
			else
			{
				bool flag2 = !this.IsSimple();
				if (flag2)
				{
					result = PolygonError.NotSimple;
				}
				else
				{
					bool flag3 = this.GetArea() <= Settings.Epsilon;
					if (flag3)
					{
						result = PolygonError.AreaTooSmall;
					}
					else
					{
						bool flag4 = !this.IsConvex();
						if (flag4)
						{
							result = PolygonError.NotConvex;
						}
						else
						{
							for (int i = 0; i < base.Count; i++)
							{
								int index = (i + 1 < base.Count) ? (i + 1) : 0;
								bool flag5 = (base[index] - base[i]).LengthSquared() <= Settings.EpsilonSqr;
								if (flag5)
								{
									result = PolygonError.SideTooSmall;
									return result;
								}
							}
							bool flag6 = !this.IsCounterClockWise();
							if (flag6)
							{
								result = PolygonError.NotCounterClockWise;
							}
							else
							{
								result = PolygonError.NoError;
							}
						}
					}
				}
			}
			return result;
		}

		public void ProjectToAxis(ref TSVector2 axis, out FP min, out FP max)
		{
			FP fP = TSVector2.Dot(axis, base[0]);
			min = fP;
			max = fP;
			for (int i = 0; i < base.Count; i++)
			{
				fP = TSVector2.Dot(base[i], axis);
				bool flag = fP < min;
				if (flag)
				{
					min = fP;
				}
				else
				{
					bool flag2 = fP > max;
					if (flag2)
					{
						max = fP;
					}
				}
			}
		}

		public int PointInPolygon(ref TSVector2 point)
		{
			int num = 0;
			int result;
			for (int i = 0; i < base.Count; i++)
			{
				TSVector2 tSVector = base[i];
				TSVector2 tSVector2 = base[this.NextIndex(i)];
				TSVector2 value = tSVector2 - tSVector;
				FP x = MathUtils.Area(ref tSVector, ref tSVector2, ref point);
				bool flag = x == 0f && TSVector2.Dot(point - tSVector, value) >= 0f && TSVector2.Dot(point - tSVector2, value) <= 0f;
				if (flag)
				{
					result = 0;
					return result;
				}
				bool flag2 = tSVector.y <= point.y;
				if (flag2)
				{
					bool flag3 = tSVector2.y > point.y && x > 0f;
					if (flag3)
					{
						num++;
					}
				}
				else
				{
					bool flag4 = tSVector2.y <= point.y && x < 0f;
					if (flag4)
					{
						num--;
					}
				}
			}
			result = ((num == 0) ? -1 : 1);
			return result;
		}

		public bool PointInPolygonAngle(ref TSVector2 point)
		{
			FP fP = 0;
			for (int i = 0; i < base.Count; i++)
			{
				TSVector2 tSVector = base[i] - point;
				TSVector2 tSVector2 = base[this.NextIndex(i)] - point;
				fP += MathUtils.VectorAngle(ref tSVector, ref tSVector2);
			}
			bool flag = FP.Abs(fP) < FP.Pi;
			return !flag;
		}

		public void Transform(ref Matrix transform)
		{
			for (int i = 0; i < base.Count; i++)
			{
				base[i] = TSVector2.Transform(base[i], transform);
			}
			bool flag = this.Holes != null && this.Holes.Count > 0;
			if (flag)
			{
				for (int j = 0; j < this.Holes.Count; j++)
				{
					TSVector2[] array = this.Holes[j].ToArray();
					int k = 0;
					int num = array.Length;
					while (k < num)
					{
						array[k] = TSVector2.Transform(array[k], transform);
						k++;
					}
					this.Holes[j] = new Vertices(array);
				}
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < base.Count; i++)
			{
				stringBuilder.Append(base[i].ToString());
				bool flag = i < base.Count - 1;
				if (flag)
				{
					stringBuilder.Append(" ");
				}
			}
			return stringBuilder.ToString();
		}
	}
}
