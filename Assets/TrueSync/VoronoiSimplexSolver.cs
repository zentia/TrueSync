using System;

namespace TrueSync
{
	public class VoronoiSimplexSolver
	{
		private const int VertexA = 0;

		private const int VertexB = 1;

		private const int VertexC = 2;

		private const int VertexD = 3;

		private const int VoronoiSimplexMaxVerts = 5;

		private const bool CatchDegenerateTetrahedron = true;

		private int _numVertices;

		private TSVector[] _simplexVectorW = new TSVector[5];

		private TSVector[] _simplexPointsP = new TSVector[5];

		private TSVector[] _simplexPointsQ = new TSVector[5];

		private TSVector _cachedPA;

		private TSVector _cachedPB;

		private TSVector _cachedV;

		private TSVector _lastW;

		private bool _cachedValidClosest;

		private SubSimplexClosestResult _cachedBC = new SubSimplexClosestResult();

		private SubSimplexClosestResult tempResult = new SubSimplexClosestResult();

		private bool _needsUpdate;

		public bool FullSimplex
		{
			get
			{
				return this._numVertices == 4;
			}
		}

		public int NumVertices
		{
			get
			{
				return this._numVertices;
			}
		}

		public FP MaxVertex
		{
			get
			{
				int numVertices = this.NumVertices;
				FP fP = FP.Zero;
				for (int i = 0; i < numVertices; i++)
				{
					FP sqrMagnitude = this._simplexVectorW[i].sqrMagnitude;
					bool flag = fP < sqrMagnitude;
					if (flag)
					{
						fP = sqrMagnitude;
					}
				}
				return fP;
			}
		}

		public bool EmptySimplex
		{
			get
			{
				return this.NumVertices == 0;
			}
		}

		public void Reset()
		{
			this._cachedValidClosest = false;
			this._numVertices = 0;
			this._needsUpdate = true;
			this._lastW = new TSVector(FP.MaxValue, FP.MaxValue, FP.MaxValue);
			this._cachedBC.Reset();
		}

		public void AddVertex(TSVector w, TSVector p, TSVector q)
		{
			this._lastW = w;
			this._needsUpdate = true;
			this._simplexVectorW[this._numVertices] = w;
			this._simplexPointsP[this._numVertices] = p;
			this._simplexPointsQ[this._numVertices] = q;
			this._numVertices++;
		}

		public bool Closest(out TSVector v)
		{
			bool result = this.UpdateClosestVectorAndPoints();
			v = this._cachedV;
			return result;
		}

		public int GetSimplex(out TSVector[] pBuf, out TSVector[] qBuf, out TSVector[] yBuf)
		{
			int numVertices = this.NumVertices;
			pBuf = new TSVector[numVertices];
			qBuf = new TSVector[numVertices];
			yBuf = new TSVector[numVertices];
			for (int i = 0; i < numVertices; i++)
			{
				yBuf[i] = this._simplexVectorW[i];
				pBuf[i] = this._simplexPointsP[i];
				qBuf[i] = this._simplexPointsQ[i];
			}
			return numVertices;
		}

		public bool InSimplex(TSVector w)
		{
			bool flag = w == this._lastW;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				int numVertices = this.NumVertices;
				for (int i = 0; i < numVertices; i++)
				{
					bool flag2 = this._simplexVectorW[i] == w;
					if (flag2)
					{
						result = true;
						return result;
					}
				}
				result = false;
			}
			return result;
		}

		public void BackupClosest(out TSVector v)
		{
			v = this._cachedV;
		}

		public void ComputePoints(out TSVector p1, out TSVector p2)
		{
			this.UpdateClosestVectorAndPoints();
			p1 = this._cachedPA;
			p2 = this._cachedPB;
		}

		public void RemoveVertex(int index)
		{
			this._numVertices--;
			this._simplexVectorW[index] = this._simplexVectorW[this._numVertices];
			this._simplexPointsP[index] = this._simplexPointsP[this._numVertices];
			this._simplexPointsQ[index] = this._simplexPointsQ[this._numVertices];
		}

		public void ReduceVertices(UsageBitfield usedVerts)
		{
			bool flag = this.NumVertices >= 4 && !usedVerts.UsedVertexD;
			if (flag)
			{
				this.RemoveVertex(3);
			}
			bool flag2 = this.NumVertices >= 3 && !usedVerts.UsedVertexC;
			if (flag2)
			{
				this.RemoveVertex(2);
			}
			bool flag3 = this.NumVertices >= 2 && !usedVerts.UsedVertexB;
			if (flag3)
			{
				this.RemoveVertex(1);
			}
			bool flag4 = this.NumVertices >= 1 && !usedVerts.UsedVertexA;
			if (flag4)
			{
				this.RemoveVertex(0);
			}
		}

		public bool UpdateClosestVectorAndPoints()
		{
			bool needsUpdate = this._needsUpdate;
			if (needsUpdate)
			{
				this._cachedBC.Reset();
				this._needsUpdate = false;
				switch (this.NumVertices)
				{
				case 0:
					this._cachedValidClosest = false;
					break;
				case 1:
					this._cachedPA = this._simplexPointsP[0];
					this._cachedPB = this._simplexPointsQ[0];
					this._cachedV = this._cachedPA - this._cachedPB;
					this._cachedBC.Reset();
					this._cachedBC.SetBarycentricCoordinates(1f, FP.Zero, FP.Zero, FP.Zero);
					this._cachedValidClosest = this._cachedBC.IsValid;
					break;
				case 2:
				{
					TSVector tSVector = this._simplexVectorW[0];
					TSVector value = this._simplexVectorW[1];
					TSVector tSVector2 = tSVector * -1;
					TSVector tSVector3 = value - tSVector;
					FP fP = TSVector.Dot(tSVector3, tSVector2);
					bool flag = fP > 0;
					if (flag)
					{
						FP sqrMagnitude = tSVector3.sqrMagnitude;
						bool flag2 = fP < sqrMagnitude;
						if (flag2)
						{
							fP /= sqrMagnitude;
							tSVector2 -= fP * tSVector3;
							this._cachedBC.UsedVertices.UsedVertexA = true;
							this._cachedBC.UsedVertices.UsedVertexB = true;
						}
						else
						{
							fP = 1;
							tSVector2 -= tSVector3;
							this._cachedBC.UsedVertices.UsedVertexB = true;
						}
					}
					else
					{
						fP = 0;
						this._cachedBC.UsedVertices.UsedVertexA = true;
					}
					this._cachedBC.SetBarycentricCoordinates(1 - fP, fP, 0, 0);
					this._cachedPA = this._simplexPointsP[0] + fP * (this._simplexPointsP[1] - this._simplexPointsP[0]);
					this._cachedPB = this._simplexPointsQ[0] + fP * (this._simplexPointsQ[1] - this._simplexPointsQ[0]);
					this._cachedV = this._cachedPA - this._cachedPB;
					this.ReduceVertices(this._cachedBC.UsedVertices);
					this._cachedValidClosest = this._cachedBC.IsValid;
					break;
				}
				case 3:
				{
					TSVector p = default(TSVector);
					TSVector a = this._simplexVectorW[0];
					TSVector b = this._simplexVectorW[1];
					TSVector c = this._simplexVectorW[2];
					this.ClosestPtPointTriangle(p, a, b, c, ref this._cachedBC);
					this._cachedPA = this._simplexPointsP[0] * this._cachedBC.BarycentricCoords[0] + this._simplexPointsP[1] * this._cachedBC.BarycentricCoords[1] + this._simplexPointsP[2] * this._cachedBC.BarycentricCoords[2] + this._simplexPointsP[3] * this._cachedBC.BarycentricCoords[3];
					this._cachedPB = this._simplexPointsQ[0] * this._cachedBC.BarycentricCoords[0] + this._simplexPointsQ[1] * this._cachedBC.BarycentricCoords[1] + this._simplexPointsQ[2] * this._cachedBC.BarycentricCoords[2] + this._simplexPointsQ[3] * this._cachedBC.BarycentricCoords[3];
					this._cachedV = this._cachedPA - this._cachedPB;
					this.ReduceVertices(this._cachedBC.UsedVertices);
					this._cachedValidClosest = this._cachedBC.IsValid;
					break;
				}
				case 4:
				{
					TSVector p = default(TSVector);
					TSVector a = this._simplexVectorW[0];
					TSVector b = this._simplexVectorW[1];
					TSVector c = this._simplexVectorW[2];
					TSVector d = this._simplexVectorW[3];
					bool flag3 = this.ClosestPtPointTetrahedron(p, a, b, c, d, ref this._cachedBC);
					bool flag4 = flag3;
					if (flag4)
					{
						this._cachedPA = this._simplexPointsP[0] * this._cachedBC.BarycentricCoords[0] + this._simplexPointsP[1] * this._cachedBC.BarycentricCoords[1] + this._simplexPointsP[2] * this._cachedBC.BarycentricCoords[2] + this._simplexPointsP[3] * this._cachedBC.BarycentricCoords[3];
						this._cachedPB = this._simplexPointsQ[0] * this._cachedBC.BarycentricCoords[0] + this._simplexPointsQ[1] * this._cachedBC.BarycentricCoords[1] + this._simplexPointsQ[2] * this._cachedBC.BarycentricCoords[2] + this._simplexPointsQ[3] * this._cachedBC.BarycentricCoords[3];
						this._cachedV = this._cachedPA - this._cachedPB;
						this.ReduceVertices(this._cachedBC.UsedVertices);
						this._cachedValidClosest = this._cachedBC.IsValid;
					}
					else
					{
						bool degenerate = this._cachedBC.Degenerate;
						if (degenerate)
						{
							this._cachedValidClosest = false;
						}
						else
						{
							this._cachedValidClosest = true;
							this._cachedV.x = (this._cachedV.y = (this._cachedV.z = FP.Zero));
						}
					}
					break;
				}
				default:
					this._cachedValidClosest = false;
					break;
				}
			}
			return this._cachedValidClosest;
		}

		public bool ClosestPtPointTriangle(TSVector p, TSVector a, TSVector b, TSVector c, ref SubSimplexClosestResult result)
		{
			result.UsedVertices.Reset();
			TSVector tSVector = b - a;
			TSVector tSVector2 = c - a;
			TSVector vector = p - a;
			FP x = TSVector.Dot(tSVector, vector);
			FP fP = TSVector.Dot(tSVector2, vector);
			bool flag = x <= FP.Zero && fP <= FP.Zero;
			bool result2;
			if (flag)
			{
				result.ClosestPointOnSimplex = a;
				result.UsedVertices.UsedVertexA = true;
				result.SetBarycentricCoordinates(1, 0, 0, 0);
				result2 = true;
			}
			else
			{
				TSVector vector2 = p - b;
				FP fP2 = TSVector.Dot(tSVector, vector2);
				FP fP3 = TSVector.Dot(tSVector2, vector2);
				bool flag2 = fP2 >= FP.Zero && fP3 <= fP2;
				if (flag2)
				{
					result.ClosestPointOnSimplex = b;
					result.UsedVertices.UsedVertexB = true;
					result.SetBarycentricCoordinates(0, 1, 0, 0);
					result2 = true;
				}
				else
				{
					FP fP4 = x * fP3 - fP2 * fP;
					bool flag3 = fP4 <= FP.Zero && x >= FP.Zero && fP2 <= FP.Zero;
					if (flag3)
					{
						FP fP5 = x / (x - fP2);
						result.ClosestPointOnSimplex = a + fP5 * tSVector;
						result.UsedVertices.UsedVertexA = true;
						result.UsedVertices.UsedVertexB = true;
						result.SetBarycentricCoordinates(1 - fP5, fP5, 0, 0);
						result2 = true;
					}
					else
					{
						TSVector vector3 = p - c;
						FP x2 = TSVector.Dot(tSVector, vector3);
						FP fP6 = TSVector.Dot(tSVector2, vector3);
						bool flag4 = fP6 >= FP.Zero && x2 <= fP6;
						if (flag4)
						{
							result.ClosestPointOnSimplex = c;
							result.UsedVertices.UsedVertexC = true;
							result.SetBarycentricCoordinates(0, 0, 1, 0);
							result2 = true;
						}
						else
						{
							FP fP7 = x2 * fP - x * fP6;
							bool flag5 = fP7 <= FP.Zero && fP >= FP.Zero && fP6 <= FP.Zero;
							if (flag5)
							{
								FP fP8 = fP / (fP - fP6);
								result.ClosestPointOnSimplex = a + fP8 * tSVector2;
								result.UsedVertices.UsedVertexA = true;
								result.UsedVertices.UsedVertexC = true;
								result.SetBarycentricCoordinates(1 - fP8, 0, fP8, 0);
								result2 = true;
							}
							else
							{
								FP x3 = fP2 * fP6 - x2 * fP3;
								bool flag6 = x3 <= FP.Zero && fP3 - fP2 >= FP.Zero && x2 - fP6 >= FP.Zero;
								if (flag6)
								{
									FP fP8 = (fP3 - fP2) / (fP3 - fP2 + (x2 - fP6));
									result.ClosestPointOnSimplex = b + fP8 * (c - b);
									result.UsedVertices.UsedVertexB = true;
									result.UsedVertices.UsedVertexC = true;
									result.SetBarycentricCoordinates(0, 1 - fP8, fP8, 0);
									result2 = true;
								}
								else
								{
									FP y = FP.One / (x3 + fP7 + fP4);
									FP fP5 = fP7 * y;
									FP fP8 = fP4 * y;
									result.ClosestPointOnSimplex = a + tSVector * fP5 + tSVector2 * fP8;
									result.UsedVertices.UsedVertexA = true;
									result.UsedVertices.UsedVertexB = true;
									result.UsedVertices.UsedVertexC = true;
									result.SetBarycentricCoordinates(1 - fP5 - fP8, fP5, fP8, 0);
									result2 = true;
								}
							}
						}
					}
				}
			}
			return result2;
		}

		public int PointOutsideOfPlane(TSVector p, TSVector a, TSVector b, TSVector c, TSVector d)
		{
			TSVector vector = TSVector.Cross(b - a, c - a);
			FP x = TSVector.Dot(p - a, vector);
			FP fP = TSVector.Dot(d - a, vector);
			bool flag = fP * fP < FP.EN8;
			int result;
			if (flag)
			{
				result = -1;
			}
			else
			{
				result = ((x * fP < FP.Zero) ? 1 : 0);
			}
			return result;
		}

		public bool ClosestPtPointTetrahedron(TSVector p, TSVector a, TSVector b, TSVector c, TSVector d, ref SubSimplexClosestResult finalResult)
		{
			this.tempResult.Reset();
			finalResult.ClosestPointOnSimplex = p;
			finalResult.UsedVertices.Reset();
			finalResult.UsedVertices.UsedVertexA = true;
			finalResult.UsedVertices.UsedVertexB = true;
			finalResult.UsedVertices.UsedVertexC = true;
			finalResult.UsedVertices.UsedVertexD = true;
			int num = this.PointOutsideOfPlane(p, a, b, c, d);
			int num2 = this.PointOutsideOfPlane(p, a, c, d, b);
			int num3 = this.PointOutsideOfPlane(p, a, d, b, c);
			int num4 = this.PointOutsideOfPlane(p, b, d, c, a);
			bool flag = num < 0 || num2 < 0 || num3 < 0 || num4 < 0;
			bool result;
			if (flag)
			{
				finalResult.Degenerate = true;
				result = false;
			}
			else
			{
				bool flag2 = num == 0 && num2 == 0 && num3 == 0 && num4 == 0;
				if (flag2)
				{
					result = false;
				}
				else
				{
					FP y = FP.MaxValue;
					bool flag3 = num != 0;
					if (flag3)
					{
						this.ClosestPtPointTriangle(p, a, b, c, ref this.tempResult);
						TSVector closestPointOnSimplex = this.tempResult.ClosestPointOnSimplex;
						FP sqrMagnitude = (closestPointOnSimplex - p).sqrMagnitude;
						bool flag4 = sqrMagnitude < y;
						if (flag4)
						{
							y = sqrMagnitude;
							finalResult.ClosestPointOnSimplex = closestPointOnSimplex;
							finalResult.UsedVertices.Reset();
							finalResult.UsedVertices.UsedVertexA = this.tempResult.UsedVertices.UsedVertexA;
							finalResult.UsedVertices.UsedVertexB = this.tempResult.UsedVertices.UsedVertexB;
							finalResult.UsedVertices.UsedVertexC = this.tempResult.UsedVertices.UsedVertexC;
							finalResult.SetBarycentricCoordinates(this.tempResult.BarycentricCoords[0], this.tempResult.BarycentricCoords[1], this.tempResult.BarycentricCoords[2], 0);
						}
					}
					bool flag5 = num2 != 0;
					if (flag5)
					{
						this.ClosestPtPointTriangle(p, a, c, d, ref this.tempResult);
						TSVector closestPointOnSimplex2 = this.tempResult.ClosestPointOnSimplex;
						FP sqrMagnitude2 = (closestPointOnSimplex2 - p).sqrMagnitude;
						bool flag6 = sqrMagnitude2 < y;
						if (flag6)
						{
							y = sqrMagnitude2;
							finalResult.ClosestPointOnSimplex = closestPointOnSimplex2;
							finalResult.UsedVertices.Reset();
							finalResult.UsedVertices.UsedVertexA = this.tempResult.UsedVertices.UsedVertexA;
							finalResult.UsedVertices.UsedVertexC = this.tempResult.UsedVertices.UsedVertexB;
							finalResult.UsedVertices.UsedVertexD = this.tempResult.UsedVertices.UsedVertexC;
							finalResult.SetBarycentricCoordinates(this.tempResult.BarycentricCoords[0], 0, this.tempResult.BarycentricCoords[1], this.tempResult.BarycentricCoords[2]);
						}
					}
					bool flag7 = num3 != 0;
					if (flag7)
					{
						this.ClosestPtPointTriangle(p, a, d, b, ref this.tempResult);
						TSVector closestPointOnSimplex3 = this.tempResult.ClosestPointOnSimplex;
						FP sqrMagnitude3 = (closestPointOnSimplex3 - p).sqrMagnitude;
						bool flag8 = sqrMagnitude3 < y;
						if (flag8)
						{
							y = sqrMagnitude3;
							finalResult.ClosestPointOnSimplex = closestPointOnSimplex3;
							finalResult.UsedVertices.Reset();
							finalResult.UsedVertices.UsedVertexA = this.tempResult.UsedVertices.UsedVertexA;
							finalResult.UsedVertices.UsedVertexD = this.tempResult.UsedVertices.UsedVertexB;
							finalResult.UsedVertices.UsedVertexB = this.tempResult.UsedVertices.UsedVertexC;
							finalResult.SetBarycentricCoordinates(this.tempResult.BarycentricCoords[0], this.tempResult.BarycentricCoords[2], 0, this.tempResult.BarycentricCoords[1]);
						}
					}
					bool flag9 = num4 != 0;
					if (flag9)
					{
						this.ClosestPtPointTriangle(p, b, d, c, ref this.tempResult);
						TSVector closestPointOnSimplex4 = this.tempResult.ClosestPointOnSimplex;
						FP sqrMagnitude4 = (closestPointOnSimplex4 - p).sqrMagnitude;
						bool flag10 = sqrMagnitude4 < y;
						if (flag10)
						{
							finalResult.ClosestPointOnSimplex = closestPointOnSimplex4;
							finalResult.UsedVertices.Reset();
							finalResult.UsedVertices.UsedVertexB = this.tempResult.UsedVertices.UsedVertexA;
							finalResult.UsedVertices.UsedVertexD = this.tempResult.UsedVertices.UsedVertexB;
							finalResult.UsedVertices.UsedVertexC = this.tempResult.UsedVertices.UsedVertexC;
							finalResult.SetBarycentricCoordinates(0, this.tempResult.BarycentricCoords[0], this.tempResult.BarycentricCoords[2], this.tempResult.BarycentricCoords[1]);
						}
					}
					bool flag11 = finalResult.UsedVertices.UsedVertexA && finalResult.UsedVertices.UsedVertexB && finalResult.UsedVertices.UsedVertexC && finalResult.UsedVertices.UsedVertexD;
					result = (!flag11 || true);
				}
			}
			return result;
		}
	}
}
