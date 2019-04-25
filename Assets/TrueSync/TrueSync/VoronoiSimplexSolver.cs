namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public class VoronoiSimplexSolver
    {
        private SubSimplexClosestResult _cachedBC = new SubSimplexClosestResult();
        private TSVector _cachedPA;
        private TSVector _cachedPB;
        private TSVector _cachedV;
        private bool _cachedValidClosest;
        private TSVector _lastW;
        private bool _needsUpdate;
        private int _numVertices;
        private TSVector[] _simplexPointsP = new TSVector[5];
        private TSVector[] _simplexPointsQ = new TSVector[5];
        private TSVector[] _simplexVectorW = new TSVector[5];
        private const bool CatchDegenerateTetrahedron = true;
        private SubSimplexClosestResult tempResult = new SubSimplexClosestResult();
        private const int VertexA = 0;
        private const int VertexB = 1;
        private const int VertexC = 2;
        private const int VertexD = 3;
        private const int VoronoiSimplexMaxVerts = 5;

        public void AddVertex(TSVector w, TSVector p, TSVector q)
        {
            this._lastW = w;
            this._needsUpdate = true;
            this._simplexVectorW[this._numVertices] = w;
            this._simplexPointsP[this._numVertices] = p;
            this._simplexPointsQ[this._numVertices] = q;
            this._numVertices++;
        }

        public void BackupClosest(out TSVector v)
        {
            v = this._cachedV;
        }

        public bool Closest(out TSVector v)
        {
            bool flag = this.UpdateClosestVectorAndPoints();
            v = this._cachedV;
            return flag;
        }

        public bool ClosestPtPointTetrahedron(TSVector p, TSVector a, TSVector b, TSVector c, TSVector d, ref SubSimplexClosestResult finalResult)
        {
            TSVector vector2;
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
            if ((((num < 0) || (num2 < 0)) || (num3 < 0)) || (num4 < 0))
            {
                finalResult.Degenerate = true;
                return false;
            }
            if ((((num == 0) && (num2 == 0)) && (num3 == 0)) && (num4 == 0))
            {
                return false;
            }
            FP maxValue = FP.MaxValue;
            if (num > 0)
            {
                this.ClosestPtPointTriangle(p, a, b, c, ref this.tempResult);
                TSVector closestPointOnSimplex = this.tempResult.ClosestPointOnSimplex;
                vector2 = closestPointOnSimplex - p;
                FP sqrMagnitude = vector2.sqrMagnitude;
                if (sqrMagnitude < maxValue)
                {
                    maxValue = sqrMagnitude;
                    finalResult.ClosestPointOnSimplex = closestPointOnSimplex;
                    finalResult.UsedVertices.Reset();
                    finalResult.UsedVertices.UsedVertexA = this.tempResult.UsedVertices.UsedVertexA;
                    finalResult.UsedVertices.UsedVertexB = this.tempResult.UsedVertices.UsedVertexB;
                    finalResult.UsedVertices.UsedVertexC = this.tempResult.UsedVertices.UsedVertexC;
                    finalResult.SetBarycentricCoordinates(this.tempResult.BarycentricCoords[0], this.tempResult.BarycentricCoords[1], this.tempResult.BarycentricCoords[2], 0);
                }
            }
            if (num2 > 0)
            {
                this.ClosestPtPointTriangle(p, a, c, d, ref this.tempResult);
                TSVector vector3 = this.tempResult.ClosestPointOnSimplex;
                vector2 = vector3 - p;
                FP fp3 = vector2.sqrMagnitude;
                if (fp3 < maxValue)
                {
                    maxValue = fp3;
                    finalResult.ClosestPointOnSimplex = vector3;
                    finalResult.UsedVertices.Reset();
                    finalResult.UsedVertices.UsedVertexA = this.tempResult.UsedVertices.UsedVertexA;
                    finalResult.UsedVertices.UsedVertexC = this.tempResult.UsedVertices.UsedVertexB;
                    finalResult.UsedVertices.UsedVertexD = this.tempResult.UsedVertices.UsedVertexC;
                    finalResult.SetBarycentricCoordinates(this.tempResult.BarycentricCoords[0], 0, this.tempResult.BarycentricCoords[1], this.tempResult.BarycentricCoords[2]);
                }
            }
            if (num3 > 0)
            {
                this.ClosestPtPointTriangle(p, a, d, b, ref this.tempResult);
                TSVector vector4 = this.tempResult.ClosestPointOnSimplex;
                vector2 = vector4 - p;
                FP fp4 = vector2.sqrMagnitude;
                if (fp4 < maxValue)
                {
                    maxValue = fp4;
                    finalResult.ClosestPointOnSimplex = vector4;
                    finalResult.UsedVertices.Reset();
                    finalResult.UsedVertices.UsedVertexA = this.tempResult.UsedVertices.UsedVertexA;
                    finalResult.UsedVertices.UsedVertexD = this.tempResult.UsedVertices.UsedVertexB;
                    finalResult.UsedVertices.UsedVertexB = this.tempResult.UsedVertices.UsedVertexC;
                    finalResult.SetBarycentricCoordinates(this.tempResult.BarycentricCoords[0], this.tempResult.BarycentricCoords[2], 0, this.tempResult.BarycentricCoords[1]);
                }
            }
            if (num4 > 0)
            {
                this.ClosestPtPointTriangle(p, b, d, c, ref this.tempResult);
                TSVector vector5 = this.tempResult.ClosestPointOnSimplex;
                vector2 = vector5 - p;
                FP fp5 = vector2.sqrMagnitude;
                if (fp5 < maxValue)
                {
                    maxValue = fp5;
                    finalResult.ClosestPointOnSimplex = vector5;
                    finalResult.UsedVertices.Reset();
                    finalResult.UsedVertices.UsedVertexB = this.tempResult.UsedVertices.UsedVertexA;
                    finalResult.UsedVertices.UsedVertexD = this.tempResult.UsedVertices.UsedVertexB;
                    finalResult.UsedVertices.UsedVertexC = this.tempResult.UsedVertices.UsedVertexC;
                    finalResult.SetBarycentricCoordinates(0, this.tempResult.BarycentricCoords[0], this.tempResult.BarycentricCoords[2], this.tempResult.BarycentricCoords[1]);
                }
            }
            return ((((finalResult.UsedVertices.UsedVertexA && finalResult.UsedVertices.UsedVertexB) && finalResult.UsedVertices.UsedVertexC) && finalResult.UsedVertices.UsedVertexD) || true);
        }

        public bool ClosestPtPointTriangle(TSVector p, TSVector a, TSVector b, TSVector c, ref SubSimplexClosestResult result)
        {
            FP fp;
            FP fp2;
            result.UsedVertices.Reset();
            TSVector vector = b - a;
            TSVector vector2 = c - a;
            TSVector vector3 = p - a;
            FP fp3 = TSVector.Dot(vector, vector3);
            FP fp4 = TSVector.Dot(vector2, vector3);
            if ((fp3 <= FP.Zero) && (fp4 <= FP.Zero))
            {
                result.ClosestPointOnSimplex = a;
                result.UsedVertices.UsedVertexA = true;
                result.SetBarycentricCoordinates(1, 0, 0, 0);
                return true;
            }
            TSVector vector4 = p - b;
            FP fp5 = TSVector.Dot(vector, vector4);
            FP fp6 = TSVector.Dot(vector2, vector4);
            if ((fp5 >= FP.Zero) && (fp6 <= fp5))
            {
                result.ClosestPointOnSimplex = b;
                result.UsedVertices.UsedVertexB = true;
                result.SetBarycentricCoordinates(0, 1, 0, 0);
                return true;
            }
            FP fp7 = (fp3 * fp6) - (fp5 * fp4);
            if (((fp7 <= FP.Zero) && (fp3 >= FP.Zero)) && (fp5 <= FP.Zero))
            {
                fp = fp3 / (fp3 - fp5);
                result.ClosestPointOnSimplex = a + (fp * vector);
                result.UsedVertices.UsedVertexA = true;
                result.UsedVertices.UsedVertexB = true;
                result.SetBarycentricCoordinates(1 - fp, fp, 0, 0);
                return true;
            }
            TSVector vector5 = p - c;
            FP fp8 = TSVector.Dot(vector, vector5);
            FP fp9 = TSVector.Dot(vector2, vector5);
            if ((fp9 >= FP.Zero) && (fp8 <= fp9))
            {
                result.ClosestPointOnSimplex = c;
                result.UsedVertices.UsedVertexC = true;
                result.SetBarycentricCoordinates(0, 0, 1, 0);
                return true;
            }
            FP fp10 = (fp8 * fp4) - (fp3 * fp9);
            if (((fp10 <= FP.Zero) && (fp4 >= FP.Zero)) && (fp9 <= FP.Zero))
            {
                fp2 = fp4 / (fp4 - fp9);
                result.ClosestPointOnSimplex = a + (fp2 * vector2);
                result.UsedVertices.UsedVertexA = true;
                result.UsedVertices.UsedVertexC = true;
                result.SetBarycentricCoordinates(1 - fp2, 0, fp2, 0);
                return true;
            }
            FP fp11 = (fp5 * fp9) - (fp8 * fp6);
            if (((fp11 <= FP.Zero) && ((fp6 - fp5) >= FP.Zero)) && ((fp8 - fp9) >= FP.Zero))
            {
                fp2 = (fp6 - fp5) / ((fp6 - fp5) + (fp8 - fp9));
                result.ClosestPointOnSimplex = b + (fp2 * (c - b));
                result.UsedVertices.UsedVertexB = true;
                result.UsedVertices.UsedVertexC = true;
                result.SetBarycentricCoordinates(0, 1 - fp2, fp2, 0);
                return true;
            }
            FP fp12 = FP.One / ((fp11 + fp10) + fp7);
            fp = fp10 * fp12;
            fp2 = fp7 * fp12;
            result.ClosestPointOnSimplex = (a + (vector * fp)) + (vector2 * fp2);
            result.UsedVertices.UsedVertexA = true;
            result.UsedVertices.UsedVertexB = true;
            result.UsedVertices.UsedVertexC = true;
            result.SetBarycentricCoordinates((1 - fp) - fp2, fp, fp2, 0);
            return true;
        }

        public void ComputePoints(out TSVector p1, out TSVector p2)
        {
            this.UpdateClosestVectorAndPoints();
            p1 = this._cachedPA;
            p2 = this._cachedPB;
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
            if (w == this._lastW)
            {
                return true;
            }
            int numVertices = this.NumVertices;
            for (int i = 0; i < numVertices; i++)
            {
                if (this._simplexVectorW[i] == w)
                {
                    return true;
                }
            }
            return false;
        }

        public int PointOutsideOfPlane(TSVector p, TSVector a, TSVector b, TSVector c, TSVector d)
        {
            TSVector vector = TSVector.Cross(b - a, c - a);
            FP fp = TSVector.Dot(p - a, vector);
            FP fp2 = TSVector.Dot(d - a, vector);
            if ((fp2 * fp2) < FP.EN8)
            {
                return -1;
            }
            return (((fp * fp2) < FP.Zero) ? 1 : 0);
        }

        public void ReduceVertices(UsageBitfield usedVerts)
        {
            if ((this.NumVertices >= 4) && !usedVerts.UsedVertexD)
            {
                this.RemoveVertex(3);
            }
            if ((this.NumVertices >= 3) && !usedVerts.UsedVertexC)
            {
                this.RemoveVertex(2);
            }
            if ((this.NumVertices >= 2) && !usedVerts.UsedVertexB)
            {
                this.RemoveVertex(1);
            }
            if ((this.NumVertices >= 1) && !usedVerts.UsedVertexA)
            {
                this.RemoveVertex(0);
            }
        }

        public void RemoveVertex(int index)
        {
            this._numVertices--;
            this._simplexVectorW[index] = this._simplexVectorW[this._numVertices];
            this._simplexPointsP[index] = this._simplexPointsP[this._numVertices];
            this._simplexPointsQ[index] = this._simplexPointsQ[this._numVertices];
        }

        public void Reset()
        {
            this._cachedValidClosest = false;
            this._numVertices = 0;
            this._needsUpdate = true;
            this._lastW = new TSVector(FP.MaxValue, FP.MaxValue, FP.MaxValue);
            this._cachedBC.Reset();
        }

        public bool UpdateClosestVectorAndPoints()
        {
            if (this._needsUpdate)
            {
                TSVector vector;
                TSVector vector2;
                TSVector vector3;
                TSVector vector4;
                FP fp;
                this._cachedBC.Reset();
                this._needsUpdate = false;
                switch (this.NumVertices)
                {
                    case 0:
                        this._cachedValidClosest = false;
                        goto Label_06A4;

                    case 1:
                        this._cachedPA = this._simplexPointsP[0];
                        this._cachedPB = this._simplexPointsQ[0];
                        this._cachedV = this._cachedPA - this._cachedPB;
                        this._cachedBC.Reset();
                        this._cachedBC.SetBarycentricCoordinates(1f, FP.Zero, FP.Zero, FP.Zero);
                        this._cachedValidClosest = this._cachedBC.IsValid;
                        goto Label_06A4;

                    case 2:
                    {
                        TSVector vector6 = this._simplexVectorW[0];
                        TSVector vector7 = this._simplexVectorW[1];
                        TSVector vector8 = vector6 * -1;
                        TSVector vector9 = vector7 - vector6;
                        fp = TSVector.Dot(vector9, vector8);
                        if (fp <= 0)
                        {
                            fp = 0;
                            this._cachedBC.UsedVertices.UsedVertexA = true;
                            break;
                        }
                        FP sqrMagnitude = vector9.sqrMagnitude;
                        if (fp >= sqrMagnitude)
                        {
                            fp = 1;
                            vector8 -= vector9;
                            this._cachedBC.UsedVertices.UsedVertexB = true;
                            break;
                        }
                        fp /= sqrMagnitude;
                        vector8 -= fp * vector9;
                        this._cachedBC.UsedVertices.UsedVertexA = true;
                        this._cachedBC.UsedVertices.UsedVertexB = true;
                        break;
                    }
                    case 3:
                        vector = new TSVector();
                        vector2 = this._simplexVectorW[0];
                        vector3 = this._simplexVectorW[1];
                        vector4 = this._simplexVectorW[2];
                        this.ClosestPtPointTriangle(vector, vector2, vector3, vector4, ref this._cachedBC);
                        this._cachedPA = (((this._simplexPointsP[0] * this._cachedBC.BarycentricCoords[0]) + (this._simplexPointsP[1] * this._cachedBC.BarycentricCoords[1])) + (this._simplexPointsP[2] * this._cachedBC.BarycentricCoords[2])) + (this._simplexPointsP[3] * this._cachedBC.BarycentricCoords[3]);
                        this._cachedPB = (((this._simplexPointsQ[0] * this._cachedBC.BarycentricCoords[0]) + (this._simplexPointsQ[1] * this._cachedBC.BarycentricCoords[1])) + (this._simplexPointsQ[2] * this._cachedBC.BarycentricCoords[2])) + (this._simplexPointsQ[3] * this._cachedBC.BarycentricCoords[3]);
                        this._cachedV = this._cachedPA - this._cachedPB;
                        this.ReduceVertices(this._cachedBC.UsedVertices);
                        this._cachedValidClosest = this._cachedBC.IsValid;
                        goto Label_06A4;

                    case 4:
                    {
                        vector = new TSVector();
                        vector2 = this._simplexVectorW[0];
                        vector3 = this._simplexVectorW[1];
                        vector4 = this._simplexVectorW[2];
                        TSVector d = this._simplexVectorW[3];
                        if (!this.ClosestPtPointTetrahedron(vector, vector2, vector3, vector4, d, ref this._cachedBC))
                        {
                            if (this._cachedBC.Degenerate)
                            {
                                this._cachedValidClosest = false;
                            }
                            else
                            {
                                this._cachedValidClosest = true;
                                this._cachedV.x = this._cachedV.y = this._cachedV.z = FP.Zero;
                            }
                        }
                        else
                        {
                            this._cachedPA = (((this._simplexPointsP[0] * this._cachedBC.BarycentricCoords[0]) + (this._simplexPointsP[1] * this._cachedBC.BarycentricCoords[1])) + (this._simplexPointsP[2] * this._cachedBC.BarycentricCoords[2])) + (this._simplexPointsP[3] * this._cachedBC.BarycentricCoords[3]);
                            this._cachedPB = (((this._simplexPointsQ[0] * this._cachedBC.BarycentricCoords[0]) + (this._simplexPointsQ[1] * this._cachedBC.BarycentricCoords[1])) + (this._simplexPointsQ[2] * this._cachedBC.BarycentricCoords[2])) + (this._simplexPointsQ[3] * this._cachedBC.BarycentricCoords[3]);
                            this._cachedV = this._cachedPA - this._cachedPB;
                            this.ReduceVertices(this._cachedBC.UsedVertices);
                            this._cachedValidClosest = this._cachedBC.IsValid;
                        }
                        goto Label_06A4;
                    }
                    default:
                        this._cachedValidClosest = false;
                        goto Label_06A4;
                }
                this._cachedBC.SetBarycentricCoordinates(1 - fp, fp, 0, 0);
                this._cachedPA = this._simplexPointsP[0] + (fp * (this._simplexPointsP[1] - this._simplexPointsP[0]));
                this._cachedPB = this._simplexPointsQ[0] + (fp * (this._simplexPointsQ[1] - this._simplexPointsQ[0]));
                this._cachedV = this._cachedPA - this._cachedPB;
                this.ReduceVertices(this._cachedBC.UsedVertices);
                this._cachedValidClosest = this._cachedBC.IsValid;
            }
        Label_06A4:
            return this._cachedValidClosest;
        }

        public bool EmptySimplex
        {
            get
            {
                return (this.NumVertices == 0);
            }
        }

        public bool FullSimplex
        {
            get
            {
                return (this._numVertices == 4);
            }
        }

        public FP MaxVertex
        {
            get
            {
                int numVertices = this.NumVertices;
                FP zero = FP.Zero;
                for (int i = 0; i < numVertices; i++)
                {
                    FP sqrMagnitude = this._simplexVectorW[i].sqrMagnitude;
                    if (zero < sqrMagnitude)
                    {
                        zero = sqrMagnitude;
                    }
                }
                return zero;
            }
        }

        public int NumVertices
        {
            get
            {
                return this._numVertices;
            }
        }
    }
}

