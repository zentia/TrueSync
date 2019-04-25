namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct TSBBox
    {
        public TSVector min;
        public TSVector max;
        public static readonly TSBBox LargeBox;
        public static readonly TSBBox SmallBox;
        static TSBBox()
        {
            LargeBox.min = new TSVector(FP.MinValue);
            LargeBox.max = new TSVector(FP.MaxValue);
            SmallBox.min = new TSVector(FP.MaxValue);
            SmallBox.max = new TSVector(FP.MinValue);
        }

        public TSBBox(TSVector min, TSVector max)
        {
            this.min = min;
            this.max = max;
        }

        internal void InverseTransform(ref TSVector position, ref TSMatrix orientation)
        {
            TSVector vector;
            TSVector vector2;
            TSMatrix matrix;
            TSVector.Subtract(ref this.max, ref position, out this.max);
            TSVector.Subtract(ref this.min, ref position, out this.min);
            TSVector.Add(ref this.max, ref this.min, out vector);
            vector.x *= FP.Half;
            vector.y *= FP.Half;
            vector.z *= FP.Half;
            TSVector.Subtract(ref this.max, ref this.min, out vector2);
            vector2.x *= FP.Half;
            vector2.y *= FP.Half;
            vector2.z *= FP.Half;
            TSVector.TransposedTransform(ref vector, ref orientation, out vector);
            TSMath.Absolute(ref orientation, out matrix);
            TSVector.TransposedTransform(ref vector2, ref matrix, out vector2);
            TSVector.Add(ref vector, ref vector2, out this.max);
            TSVector.Subtract(ref vector, ref vector2, out this.min);
        }

        public void Transform(ref TSMatrix orientation)
        {
            TSMatrix matrix;
            TSVector position = (TSVector) (FP.Half * (this.max - this.min));
            TSVector vector2 = (TSVector) (FP.Half * (this.max + this.min));
            TSVector.Transform(ref vector2, ref orientation, out vector2);
            TSMath.Absolute(ref orientation, out matrix);
            TSVector.Transform(ref position, ref matrix, out position);
            this.max = vector2 + position;
            this.min = vector2 - position;
        }

        private bool Intersect1D(FP start, FP dir, FP min, FP max, ref FP enter, ref FP exit)
        {
            if ((dir * dir) < (TSMath.Epsilon * TSMath.Epsilon))
            {
                return ((start >= min) && (start <= max));
            }
            FP fp = (min - start) / dir;
            FP fp2 = (max - start) / dir;
            if (fp > fp2)
            {
                FP fp3 = fp;
                fp = fp2;
                fp2 = fp3;
            }
            if ((fp > exit) || (fp2 < enter))
            {
                return false;
            }
            if (fp > enter)
            {
                enter = fp;
            }
            if (fp2 < exit)
            {
                exit = fp2;
            }
            return true;
        }

        public bool SegmentIntersect(ref TSVector origin, ref TSVector direction)
        {
            FP zero = FP.Zero;
            FP one = FP.One;
            if (!this.Intersect1D(origin.x, direction.x, this.min.x, this.max.x, ref zero, ref one))
            {
                return false;
            }
            if (!this.Intersect1D(origin.y, direction.y, this.min.y, this.max.y, ref zero, ref one))
            {
                return false;
            }
            if (!this.Intersect1D(origin.z, direction.z, this.min.z, this.max.z, ref zero, ref one))
            {
                return false;
            }
            return true;
        }

        public bool RayIntersect(ref TSVector origin, ref TSVector direction)
        {
            FP zero = FP.Zero;
            FP maxValue = FP.MaxValue;
            if (!this.Intersect1D(origin.x, direction.x, this.min.x, this.max.x, ref zero, ref maxValue))
            {
                return false;
            }
            if (!this.Intersect1D(origin.y, direction.y, this.min.y, this.max.y, ref zero, ref maxValue))
            {
                return false;
            }
            if (!this.Intersect1D(origin.z, direction.z, this.min.z, this.max.z, ref zero, ref maxValue))
            {
                return false;
            }
            return true;
        }

        public bool SegmentIntersect(TSVector origin, TSVector direction)
        {
            return this.SegmentIntersect(ref origin, ref direction);
        }

        public bool RayIntersect(TSVector origin, TSVector direction)
        {
            return this.RayIntersect(ref origin, ref direction);
        }

        public ContainmentType Contains(TSVector point)
        {
            return this.Contains(ref point);
        }

        public ContainmentType Contains(ref TSVector point)
        {
            return (((((this.min.x <= point.x) && (point.x <= this.max.x)) && ((this.min.y <= point.y) && (point.y <= this.max.y))) && ((this.min.z <= point.z) && (point.z <= this.max.z))) ? ContainmentType.Contains : ContainmentType.Disjoint);
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
            TSVector vector = new TSVector(FP.MaxValue);
            TSVector vector2 = new TSVector(FP.MinValue);
            for (int i = 0; i < points.Length; i++)
            {
                TSVector.Min(ref vector, ref points[i], out vector);
                TSVector.Max(ref vector2, ref points[i], out vector2);
            }
            return new TSBBox(vector, vector2);
        }

        public ContainmentType Contains(TSBBox box)
        {
            return this.Contains(ref box);
        }

        public ContainmentType Contains(ref TSBBox box)
        {
            ContainmentType disjoint = ContainmentType.Disjoint;
            if ((((this.max.x < box.min.x) || (this.min.x > box.max.x)) || ((this.max.y < box.min.y) || (this.min.y > box.max.y))) || ((this.max.z < box.min.z) || (this.min.z > box.max.z)))
            {
                return disjoint;
            }
            return (((((this.min.x <= box.min.x) && (box.max.x <= this.max.x)) && ((this.min.y <= box.min.y) && (box.max.y <= this.max.y))) && ((this.min.z <= box.min.z) && (box.max.z <= this.max.z))) ? ContainmentType.Contains : ContainmentType.Intersects);
        }

        public static TSBBox CreateFromCenter(TSVector center, TSVector size)
        {
            TSVector vector = size * FP.Half;
            return new TSBBox(center - vector, center + vector);
        }

        public static TSBBox CreateMerged(TSBBox original, TSBBox additional)
        {
            TSBBox box;
            CreateMerged(ref original, ref additional, out box);
            return box;
        }

        public static void CreateMerged(ref TSBBox original, ref TSBBox additional, out TSBBox result)
        {
            TSVector vector;
            TSVector vector2;
            TSVector.Min(ref original.min, ref additional.min, out vector2);
            TSVector.Max(ref original.max, ref additional.max, out vector);
            result.min = vector2;
            result.max = vector;
        }

        public TSVector center
        {
            get
            {
                return ((this.min + this.max) * FP.Half);
            }
        }
        public TSVector size
        {
            get
            {
                return (this.max - this.min);
            }
        }
        public TSVector extents
        {
            get
            {
                return (this.size * FP.Half);
            }
        }
        internal FP Perimeter
        {
            get
            {
                return ((2 * FP.One) * ((((this.max.x - this.min.x) * (this.max.y - this.min.y)) + ((this.max.x - this.min.x) * (this.max.z - this.min.z))) + ((this.max.z - this.min.z) * (this.max.y - this.min.y))));
            }
        }
        public override string ToString()
        {
            return (this.min + "|" + this.max);
        }
        public enum ContainmentType
        {
            Disjoint,
            Contains,
            Intersects
        }
    }
}

