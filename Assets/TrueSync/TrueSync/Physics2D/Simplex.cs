namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Simplex
    {
        internal int Count;
        internal FixedArray3<SimplexVertex> V;
        internal void ReadCache(ref SimplexCache cache, DistanceProxy proxyA, ref Transform transformA, DistanceProxy proxyB, ref Transform transformB)
        {
            Debug.Assert(cache.Count <= 3);
            this.Count = cache.Count;
            for (int i = 0; i < this.Count; i++)
            {
                SimplexVertex vertex = this.V[i];
                vertex.IndexA = cache.IndexA[i];
                vertex.IndexB = cache.IndexB[i];
                TSVector2 v = proxyA.Vertices[vertex.IndexA];
                TSVector2 vector2 = proxyB.Vertices[vertex.IndexB];
                vertex.WA = MathUtils.Mul(ref transformA, v);
                vertex.WB = MathUtils.Mul(ref transformB, vector2);
                vertex.W = vertex.WB - vertex.WA;
                vertex.A = 0f;
                this.V[i] = vertex;
            }
            if (this.Count > 1)
            {
                FP metric = cache.Metric;
                FP fp2 = this.GetMetric();
                if (((fp2 < (0.5f * metric)) || ((2f * metric) < fp2)) || (fp2 < Settings.Epsilon))
                {
                    this.Count = 0;
                }
            }
            if (this.Count == 0)
            {
                SimplexVertex vertex2 = this.V[0];
                vertex2.IndexA = 0;
                vertex2.IndexB = 0;
                TSVector2 vector3 = proxyA.Vertices[0];
                TSVector2 vector4 = proxyB.Vertices[0];
                vertex2.WA = MathUtils.Mul(ref transformA, vector3);
                vertex2.WB = MathUtils.Mul(ref transformB, vector4);
                vertex2.W = vertex2.WB - vertex2.WA;
                vertex2.A = 1f;
                this.V[0] = vertex2;
                this.Count = 1;
            }
        }

        internal void WriteCache(ref SimplexCache cache)
        {
            cache.Metric = this.GetMetric();
            cache.Count = (ushort) this.Count;
            for (int i = 0; i < this.Count; i++)
            {
                cache.IndexA[i] = (byte) this.V[i].IndexA;
                cache.IndexB[i] = (byte) this.V[i].IndexB;
            }
        }

        internal TSVector2 GetSearchDirection()
        {
            switch (this.Count)
            {
                case 1:
                    return -this.V[0].W;

                case 2:
                {
                    TSVector2 a = this.V[1].W - this.V[0].W;
                    if (MathUtils.Cross(a, -this.V[0].W) > 0f)
                    {
                        return new TSVector2(-a.y, a.x);
                    }
                    return new TSVector2(a.y, -a.x);
                }
            }
            Debug.Assert(false);
            return TSVector2.zero;
        }

        internal TSVector2 GetClosestPoint()
        {
            switch (this.Count)
            {
                case 0:
                    Debug.Assert(false);
                    return TSVector2.zero;

                case 1:
                    return this.V[0].W;

                case 2:
                    return (TSVector2) ((this.V[0].A * this.V[0].W) + (this.V[1].A * this.V[1].W));

                case 3:
                    return TSVector2.zero;
            }
            Debug.Assert(false);
            return TSVector2.zero;
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
                    pA = (TSVector2) ((this.V[0].A * this.V[0].WA) + (this.V[1].A * this.V[1].WA));
                    pB = (TSVector2) ((this.V[0].A * this.V[0].WB) + (this.V[1].A * this.V[1].WB));
                    break;

                case 3:
                    pA = (TSVector2) (((this.V[0].A * this.V[0].WA) + (this.V[1].A * this.V[1].WA)) + (this.V[2].A * this.V[2].WA));
                    pB = pA;
                    break;

                default:
                    throw new Exception();
            }
        }

        internal FP GetMetric()
        {
            switch (this.Count)
            {
                case 0:
                    Debug.Assert(false);
                    return 0f;

                case 1:
                    return 0f;

                case 2:
                {
                    TSVector2 vector = this.V[0].W - this.V[1].W;
                    return vector.magnitude;
                }
                case 3:
                    return MathUtils.Cross(this.V[1].W - this.V[0].W, this.V[2].W - this.V[0].W);
            }
            Debug.Assert(false);
            return 0f;
        }

        internal void Solve2()
        {
            TSVector2 w = this.V[0].W;
            TSVector2 vector2 = this.V[1].W;
            TSVector2 vector3 = vector2 - w;
            FP fp = -TSVector2.Dot(w, vector3);
            if (fp <= 0f)
            {
                SimplexVertex vertex3 = this.V[0];
                vertex3.A = 1f;
                this.V[0] = vertex3;
                this.Count = 1;
            }
            else
            {
                FP fp2 = TSVector2.Dot(vector2, vector3);
                if (fp2 <= 0f)
                {
                    SimplexVertex vertex4 = this.V[1];
                    vertex4.A = 1f;
                    this.V[1] = vertex4;
                    this.Count = 1;
                    this.V[0] = this.V[1];
                }
                else
                {
                    FP fp3 = 1f / (fp2 + fp);
                    SimplexVertex vertex = this.V[0];
                    SimplexVertex vertex2 = this.V[1];
                    vertex.A = fp2 * fp3;
                    vertex2.A = fp * fp3;
                    this.V[0] = vertex;
                    this.V[1] = vertex2;
                    this.Count = 2;
                }
            }
        }

        internal void Solve3()
        {
            TSVector2 w = this.V[0].W;
            TSVector2 vector2 = this.V[1].W;
            TSVector2 vector3 = this.V[2].W;
            TSVector2 vector4 = vector2 - w;
            FP fp = TSVector2.Dot(w, vector4);
            FP fp3 = TSVector2.Dot(vector2, vector4);
            FP fp4 = -fp;
            TSVector2 vector5 = vector3 - w;
            FP fp5 = TSVector2.Dot(w, vector5);
            FP fp7 = TSVector2.Dot(vector3, vector5);
            FP fp8 = -fp5;
            TSVector2 vector6 = vector3 - vector2;
            FP fp9 = TSVector2.Dot(vector2, vector6);
            FP fp11 = TSVector2.Dot(vector3, vector6);
            FP fp12 = -fp9;
            FP fp13 = MathUtils.Cross(vector4, vector5);
            FP fp14 = fp13 * MathUtils.Cross(vector2, vector3);
            FP fp15 = fp13 * MathUtils.Cross(vector3, w);
            FP fp16 = fp13 * MathUtils.Cross(w, vector2);
            if ((fp4 <= 0f) && (fp8 <= 0f))
            {
                SimplexVertex vertex4 = this.V[0];
                vertex4.A = 1f;
                this.V[0] = vertex4;
                this.Count = 1;
            }
            else if (((fp3 > 0f) && (fp4 > 0f)) && (fp16 <= 0f))
            {
                FP fp18 = 1f / (fp3 + fp4);
                SimplexVertex vertex5 = this.V[0];
                SimplexVertex vertex6 = this.V[1];
                vertex5.A = fp3 * fp18;
                vertex6.A = fp4 * fp18;
                this.V[0] = vertex5;
                this.V[1] = vertex6;
                this.Count = 2;
            }
            else if (((fp7 > 0f) && (fp8 > 0f)) && (fp15 <= 0f))
            {
                FP fp19 = 1f / (fp7 + fp8);
                SimplexVertex vertex7 = this.V[0];
                SimplexVertex vertex8 = this.V[2];
                vertex7.A = fp7 * fp19;
                vertex8.A = fp8 * fp19;
                this.V[0] = vertex7;
                this.V[2] = vertex8;
                this.Count = 2;
                this.V[1] = this.V[2];
            }
            else if ((fp3 <= 0f) && (fp12 <= 0f))
            {
                SimplexVertex vertex9 = this.V[1];
                vertex9.A = 1f;
                this.V[1] = vertex9;
                this.Count = 1;
                this.V[0] = this.V[1];
            }
            else if ((fp7 <= 0f) && (fp11 <= 0f))
            {
                SimplexVertex vertex10 = this.V[2];
                vertex10.A = 1f;
                this.V[2] = vertex10;
                this.Count = 1;
                this.V[0] = this.V[2];
            }
            else if (((fp11 > 0f) && (fp12 > 0f)) && (fp14 <= 0f))
            {
                FP fp20 = 1f / (fp11 + fp12);
                SimplexVertex vertex11 = this.V[1];
                SimplexVertex vertex12 = this.V[2];
                vertex11.A = fp11 * fp20;
                vertex12.A = fp12 * fp20;
                this.V[1] = vertex11;
                this.V[2] = vertex12;
                this.Count = 2;
                this.V[0] = this.V[2];
            }
            else
            {
                FP fp17 = 1f / ((fp14 + fp15) + fp16);
                SimplexVertex vertex = this.V[0];
                SimplexVertex vertex2 = this.V[1];
                SimplexVertex vertex3 = this.V[2];
                vertex.A = fp14 * fp17;
                vertex2.A = fp15 * fp17;
                vertex3.A = fp16 * fp17;
                this.V[0] = vertex;
                this.V[1] = vertex2;
                this.V[2] = vertex3;
                this.Count = 3;
            }
        }
    }
}

