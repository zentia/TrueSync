namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct AABB
    {
        public TSVector2 LowerBound;
        public TSVector2 UpperBound;
        public AABB(TSVector2 min, TSVector2 max) : this(ref min, ref max)
        {
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

        public FP Width
        {
            get
            {
                return (this.UpperBound.x - this.LowerBound.x);
            }
        }
        public FP Height
        {
            get
            {
                return (this.UpperBound.y - this.LowerBound.y);
            }
        }
        public TSVector2 Center
        {
            get
            {
                return (TSVector2) (0.5f * (this.LowerBound + this.UpperBound));
            }
        }
        public TSVector2 Extents
        {
            get
            {
                return (TSVector2) (0.5f * (this.UpperBound - this.LowerBound));
            }
        }
        public FP Perimeter
        {
            get
            {
                FP fp = this.UpperBound.x - this.LowerBound.x;
                FP fp2 = this.UpperBound.y - this.LowerBound.y;
                return (2f * (fp + fp2));
            }
        }
        public TrueSync.Physics2D.Vertices Vertices
        {
            get
            {
                return new TrueSync.Physics2D.Vertices(4) { this.UpperBound, new TSVector2(this.UpperBound.x, this.LowerBound.y), this.LowerBound, new TSVector2(this.LowerBound.x, this.UpperBound.y) };
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
        public bool IsValid()
        {
            TSVector2 vector = this.UpperBound - this.LowerBound;
            return ((((vector.x >= 0f) && (vector.y >= 0f)) && this.LowerBound.IsValid()) && this.UpperBound.IsValid());
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
            return ((((flag && (this.LowerBound.x <= aabb.LowerBound.x)) && (this.LowerBound.y <= aabb.LowerBound.y)) && (aabb.UpperBound.x <= this.UpperBound.x)) && (aabb.UpperBound.y <= this.UpperBound.y));
        }

        public bool Contains(ref TSVector2 point)
        {
            return (((point.x > (this.LowerBound.x + Settings.Epsilon)) && (point.x < (this.UpperBound.x - Settings.Epsilon))) && ((point.y > (this.LowerBound.y + Settings.Epsilon)) && (point.y < (this.UpperBound.y - Settings.Epsilon))));
        }

        public static bool TestOverlap(ref AABB a, ref AABB b)
        {
            TSVector2 vector = b.LowerBound - a.UpperBound;
            TSVector2 vector2 = a.LowerBound - b.UpperBound;
            if ((vector.x > 0f) || (vector.y > 0f))
            {
                return false;
            }
            if ((vector2.x > 0f) || (vector2.y > 0f))
            {
                return false;
            }
            return true;
        }

        public bool RayCast(out RayCastOutput output, ref RayCastInput input, bool doInteriorCheck = true)
        {
            output = new RayCastOutput();
            FP fp = -Settings.MaxFP;
            FP maxFP = Settings.MaxFP;
            TSVector2 vector = input.Point1;
            TSVector2 v = input.Point2 - input.Point1;
            TSVector2 vector3 = MathUtils.Abs(v);
            TSVector2 zero = TSVector2.zero;
            for (int i = 0; i < 2; i++)
            {
                FP fp3 = (i == 0) ? vector3.x : vector3.y;
                FP fp4 = (i == 0) ? this.LowerBound.x : this.LowerBound.y;
                FP fp5 = (i == 0) ? this.UpperBound.x : this.UpperBound.y;
                FP fp6 = (i == 0) ? vector.x : vector.y;
                if (fp3 < Settings.Epsilon)
                {
                    if ((fp6 < fp4) || (fp5 < fp6))
                    {
                        return false;
                    }
                }
                else
                {
                    FP fp7 = (i == 0) ? v.x : v.y;
                    FP fp8 = 1f / fp7;
                    FP a = (fp4 - fp6) * fp8;
                    FP b = (fp5 - fp6) * fp8;
                    FP fp11 = -1f;
                    if (a > b)
                    {
                        MathUtils.Swap<FP>(ref a, ref b);
                        fp11 = 1f;
                    }
                    if (a > fp)
                    {
                        if (i == 0)
                        {
                            zero.x = fp11;
                        }
                        else
                        {
                            zero.y = fp11;
                        }
                        fp = a;
                    }
                    maxFP = TSMath.Min(maxFP, b);
                    if (fp > maxFP)
                    {
                        return false;
                    }
                }
            }
            if (doInteriorCheck && ((fp < 0f) || (input.MaxFraction < fp)))
            {
                return false;
            }
            output.Fraction = fp;
            output.Normal = zero;
            return true;
        }
    }
}

