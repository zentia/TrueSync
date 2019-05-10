using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	internal struct Simplex
	{
		internal int Count;

		internal FixedArray3<SimplexVertex> V;

		internal void ReadCache(ref SimplexCache cache, DistanceProxy proxyA, ref Transform transformA, DistanceProxy proxyB, ref Transform transformB)
		{
			Debug.Assert(cache.Count <= 3);
			this.Count = (int)cache.Count;
			for (int i = 0; i < this.Count; i++)
			{
				SimplexVertex simplexVertex = this.V[i];
				simplexVertex.IndexA = (int)cache.IndexA[i];
				simplexVertex.IndexB = (int)cache.IndexB[i];
				TSVector2 v = proxyA.Vertices[simplexVertex.IndexA];
				TSVector2 v2 = proxyB.Vertices[simplexVertex.IndexB];
				simplexVertex.WA = MathUtils.Mul(ref transformA, v);
				simplexVertex.WB = MathUtils.Mul(ref transformB, v2);
				simplexVertex.W = simplexVertex.WB - simplexVertex.WA;
				simplexVertex.A = 0f;
				this.V[i] = simplexVertex;
			}
			bool flag = this.Count > 1;
			if (flag)
			{
				FP metric = cache.Metric;
				FP metric2 = this.GetMetric();
				bool flag2 = metric2 < 0.5f * metric || 2f * metric < metric2 || metric2 < Settings.Epsilon;
				if (flag2)
				{
					this.Count = 0;
				}
			}
			bool flag3 = this.Count == 0;
			if (flag3)
			{
				SimplexVertex simplexVertex2 = this.V[0];
				simplexVertex2.IndexA = 0;
				simplexVertex2.IndexB = 0;
				TSVector2 v3 = proxyA.Vertices[0];
				TSVector2 v4 = proxyB.Vertices[0];
				simplexVertex2.WA = MathUtils.Mul(ref transformA, v3);
				simplexVertex2.WB = MathUtils.Mul(ref transformB, v4);
				simplexVertex2.W = simplexVertex2.WB - simplexVertex2.WA;
				simplexVertex2.A = 1f;
				this.V[0] = simplexVertex2;
				this.Count = 1;
			}
		}

		internal void WriteCache(ref SimplexCache cache)
		{
			cache.Metric = this.GetMetric();
			cache.Count = (ushort)this.Count;
			for (int i = 0; i < this.Count; i++)
			{
				cache.IndexA[i] = (byte)this.V[i].IndexA;
				cache.IndexB[i] = (byte)this.V[i].IndexB;
			}
		}

		internal TSVector2 GetSearchDirection()
		{
			int count = this.Count;
			TSVector2 result;
			if (count != 1)
			{
				if (count != 2)
				{
					Debug.Assert(false);
					result = TSVector2.zero;
				}
				else
				{
					TSVector2 tSVector = this.V[1].W - this.V[0].W;
					FP x = MathUtils.Cross(tSVector, -this.V[0].W);
					bool flag = x > 0f;
					if (flag)
					{
						result = new TSVector2(-tSVector.y, tSVector.x);
					}
					else
					{
						result = new TSVector2(tSVector.y, -tSVector.x);
					}
				}
			}
			else
			{
				result = -this.V[0].W;
			}
			return result;
		}

		internal TSVector2 GetClosestPoint()
		{
			TSVector2 result;
			switch (this.Count)
			{
			case 0:
				Debug.Assert(false);
				result = TSVector2.zero;
				break;
			case 1:
				result = this.V[0].W;
				break;
			case 2:
				result = this.V[0].A * this.V[0].W + this.V[1].A * this.V[1].W;
				break;
			case 3:
				result = TSVector2.zero;
				break;
			default:
				Debug.Assert(false);
				result = TSVector2.zero;
				break;
			}
			return result;
		}

		internal void GetWitnessPoints(out TSVector2 pA, out TSVector2 pB)
		{
			switch (this.Count)
			{
			case 0:
				pA = TSVector2.zero;
				pB = TSVector2.zero;
				Debug.Assert(false);
				break;
			case 1:
				pA = this.V[0].WA;
				pB = this.V[0].WB;
				break;
			case 2:
				pA = this.V[0].A * this.V[0].WA + this.V[1].A * this.V[1].WA;
				pB = this.V[0].A * this.V[0].WB + this.V[1].A * this.V[1].WB;
				break;
			case 3:
				pA = this.V[0].A * this.V[0].WA + this.V[1].A * this.V[1].WA + this.V[2].A * this.V[2].WA;
				pB = pA;
				break;
			default:
				throw new Exception();
			}
		}

		internal FP GetMetric()
		{
			FP result;
			switch (this.Count)
			{
			case 0:
				Debug.Assert(false);
				result = 0f;
				break;
			case 1:
				result = 0f;
				break;
			case 2:
				result = (this.V[0].W - this.V[1].W).magnitude;
				break;
			case 3:
				result = MathUtils.Cross(this.V[1].W - this.V[0].W, this.V[2].W - this.V[0].W);
				break;
			default:
				Debug.Assert(false);
				result = 0f;
				break;
			}
			return result;
		}

		internal void Solve2()
		{
			TSVector2 w = this.V[0].W;
			TSVector2 w2 = this.V[1].W;
			TSVector2 value = w2 - w;
			FP fP = -TSVector2.Dot(w, value);
			bool flag = fP <= 0f;
			if (flag)
			{
				SimplexVertex value2 = this.V[0];
				value2.A = 1f;
				this.V[0] = value2;
				this.Count = 1;
			}
			else
			{
				FP x = TSVector2.Dot(w2, value);
				bool flag2 = x <= 0f;
				if (flag2)
				{
					SimplexVertex value3 = this.V[1];
					value3.A = 1f;
					this.V[1] = value3;
					this.Count = 1;
					this.V[0] = this.V[1];
				}
				else
				{
					FP y = 1f / (x + fP);
					SimplexVertex value4 = this.V[0];
					SimplexVertex value5 = this.V[1];
					value4.A = x * y;
					value5.A = fP * y;
					this.V[0] = value4;
					this.V[1] = value5;
					this.Count = 2;
				}
			}
		}

		internal void Solve3()
		{
			TSVector2 w = this.V[0].W;
			TSVector2 w2 = this.V[1].W;
			TSVector2 w3 = this.V[2].W;
			TSVector2 tSVector = w2 - w;
			FP x = TSVector2.Dot(w, tSVector);
			FP fP = TSVector2.Dot(w2, tSVector);
			FP x2 = fP;
			FP fP2 = -x;
			TSVector2 tSVector2 = w3 - w;
			FP x3 = TSVector2.Dot(w, tSVector2);
			FP fP3 = TSVector2.Dot(w3, tSVector2);
			FP x4 = fP3;
			FP fP4 = -x3;
			TSVector2 value = w3 - w2;
			FP x5 = TSVector2.Dot(w2, value);
			FP fP5 = TSVector2.Dot(w3, value);
			FP x6 = fP5;
			FP fP6 = -x5;
			FP x7 = MathUtils.Cross(tSVector, tSVector2);
			FP x8 = x7 * MathUtils.Cross(w2, w3);
			FP fP7 = x7 * MathUtils.Cross(w3, w);
			FP fP8 = x7 * MathUtils.Cross(w, w2);
			bool flag = fP2 <= 0f && fP4 <= 0f;
			if (flag)
			{
				SimplexVertex value2 = this.V[0];
				value2.A = 1f;
				this.V[0] = value2;
				this.Count = 1;
			}
			else
			{
				bool flag2 = x2 > 0f && fP2 > 0f && fP8 <= 0f;
				if (flag2)
				{
					FP y = 1f / (x2 + fP2);
					SimplexVertex value3 = this.V[0];
					SimplexVertex value4 = this.V[1];
					value3.A = x2 * y;
					value4.A = fP2 * y;
					this.V[0] = value3;
					this.V[1] = value4;
					this.Count = 2;
				}
				else
				{
					bool flag3 = x4 > 0f && fP4 > 0f && fP7 <= 0f;
					if (flag3)
					{
						FP y2 = 1f / (x4 + fP4);
						SimplexVertex value5 = this.V[0];
						SimplexVertex value6 = this.V[2];
						value5.A = x4 * y2;
						value6.A = fP4 * y2;
						this.V[0] = value5;
						this.V[2] = value6;
						this.Count = 2;
						this.V[1] = this.V[2];
					}
					else
					{
						bool flag4 = x2 <= 0f && fP6 <= 0f;
						if (flag4)
						{
							SimplexVertex value7 = this.V[1];
							value7.A = 1f;
							this.V[1] = value7;
							this.Count = 1;
							this.V[0] = this.V[1];
						}
						else
						{
							bool flag5 = x4 <= 0f && x6 <= 0f;
							if (flag5)
							{
								SimplexVertex value8 = this.V[2];
								value8.A = 1f;
								this.V[2] = value8;
								this.Count = 1;
								this.V[0] = this.V[2];
							}
							else
							{
								bool flag6 = x6 > 0f && fP6 > 0f && x8 <= 0f;
								if (flag6)
								{
									FP y3 = 1f / (x6 + fP6);
									SimplexVertex value9 = this.V[1];
									SimplexVertex value10 = this.V[2];
									value9.A = x6 * y3;
									value10.A = fP6 * y3;
									this.V[1] = value9;
									this.V[2] = value10;
									this.Count = 2;
									this.V[0] = this.V[2];
								}
								else
								{
									FP y4 = 1f / (x8 + fP7 + fP8);
									SimplexVertex value11 = this.V[0];
									SimplexVertex value12 = this.V[1];
									SimplexVertex value13 = this.V[2];
									value11.A = x8 * y4;
									value12.A = fP7 * y4;
									value13.A = fP8 * y4;
									this.V[0] = value11;
									this.V[1] = value12;
									this.V[2] = value13;
									this.Count = 3;
								}
							}
						}
					}
				}
			}
		}
	}
}
