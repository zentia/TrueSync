namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Text;
    using TrueSync;

    [DebuggerDisplay("Count = {Count} Vertices = {ToString()}")]
    public class Vertices : List<TSVector2>
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <AttachedToBody>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Vertices> <Holes>k__BackingField;

        public Vertices()
        {
        }

        public Vertices(IEnumerable<TSVector2> vertices)
        {
            base.AddRange(vertices);
        }

        public Vertices(int capacity) : base(capacity)
        {
        }

        public PolygonError CheckPolygon()
        {
            if ((base.Count < 3) || (base.Count > Settings.MaxPolygonVertices))
            {
                return PolygonError.InvalidAmountOfVertices;
            }
            if (!this.IsSimple())
            {
                return PolygonError.NotSimple;
            }
            if (this.GetArea() <= Settings.Epsilon)
            {
                return PolygonError.AreaTooSmall;
            }
            if (!this.IsConvex())
            {
                return PolygonError.NotConvex;
            }
            for (int i = 0; i < base.Count; i++)
            {
                int num2 = ((i + 1) < base.Count) ? (i + 1) : 0;
                TSVector2 vector = base[num2] - base[i];
                if (vector.LengthSquared() <= Settings.EpsilonSqr)
                {
                    return PolygonError.SideTooSmall;
                }
            }
            if (!this.IsCounterClockWise())
            {
                return PolygonError.NotCounterClockWise;
            }
            return PolygonError.NoError;
        }

        public void ForceCounterClockWise()
        {
            if ((base.Count >= 3) && !this.IsCounterClockWise())
            {
                base.Reverse();
            }
        }

        public AABB GetAABB()
        {
            AABB aabb;
            TSVector2 vector = new TSVector2(FP.MaxValue, FP.MaxValue);
            TSVector2 vector2 = new TSVector2(FP.MinValue, FP.MinValue);
            for (int i = 0; i < base.Count; i++)
            {
                if (base[i].x < vector.x)
                {
                    vector.x = base[i].x;
                }
                if (base[i].x > vector2.x)
                {
                    vector2.x = base[i].x;
                }
                if (base[i].y < vector.y)
                {
                    vector.y = base[i].y;
                }
                if (base[i].y > vector2.y)
                {
                    vector2.y = base[i].y;
                }
            }
            aabb.LowerBound = vector;
            aabb.UpperBound = vector2;
            return aabb;
        }

        public FP GetArea()
        {
            FP signedArea = this.GetSignedArea();
            return ((signedArea < 0) ? -signedArea : signedArea);
        }

        public TSVector2 GetCentroid()
        {
            if (base.Count < 3)
            {
                return new TSVector2(FP.NaN, FP.NaN);
            }
            TSVector2 zero = TSVector2.zero;
            FP fp = 0f;
            FP fp2 = 0.3333333f;
            for (int i = 0; i < base.Count; i++)
            {
                TSVector2 vector3 = base[i];
                TSVector2 vector4 = ((i + 1) < base.Count) ? base[i + 1] : base[0];
                FP fp3 = 0.5f * ((vector3.x * vector4.y) - (vector3.y * vector4.x));
                fp += fp3;
                zero += (fp3 * fp2) * (vector3 + vector4);
            }
            return (TSVector2) (zero * (1f / fp));
        }

        public FP GetSignedArea()
        {
            if (base.Count < 3)
            {
                return 0;
            }
            FP fp = 0;
            for (int i = 0; i < base.Count; i++)
            {
                int num2 = (i + 1) % base.Count;
                TSVector2 vector = base[i];
                TSVector2 vector2 = base[num2];
                fp += vector.x * vector2.y;
                fp -= vector.y * vector2.x;
            }
            return (fp / 2f);
        }

        public bool IsConvex()
        {
            if (base.Count < 3)
            {
                return false;
            }
            if (base.Count != 3)
            {
                for (int i = 0; i < base.Count; i++)
                {
                    int num2 = ((i + 1) < base.Count) ? (i + 1) : 0;
                    TSVector2 vector = base[num2] - base[i];
                    for (int j = 0; j < base.Count; j++)
                    {
                        if ((j != i) && (j != num2))
                        {
                            TSVector2 vector2 = base[j] - base[i];
                            FP fp = (vector.x * vector2.y) - (vector.y * vector2.x);
                            if (fp <= 0f)
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            return true;
        }

        public bool IsCounterClockWise()
        {
            if (base.Count < 3)
            {
                return false;
            }
            return (this.GetSignedArea() > 0f);
        }

        public bool IsSimple()
        {
            if (base.Count < 3)
            {
                return false;
            }
            for (int i = 0; i < base.Count; i++)
            {
                TSVector2 vector = base[i];
                TSVector2 vector2 = this.NextVertex(i);
                for (int j = i + 1; j < base.Count; j++)
                {
                    TSVector2 vector5;
                    TSVector2 vector3 = base[j];
                    TSVector2 vector4 = this.NextVertex(j);
                    if (LineTools.LineIntersect2(ref vector, ref vector2, ref vector3, ref vector4, out vector5))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        public int NextIndex(int index)
        {
            return (((index + 1) > (base.Count - 1)) ? 0 : (index + 1));
        }

        public TSVector2 NextVertex(int index)
        {
            return base[this.NextIndex(index)];
        }

        public int PointInPolygon(ref TSVector2 point)
        {
            int num = 0;
            for (int i = 0; i < base.Count; i++)
            {
                TSVector2 a = base[i];
                TSVector2 b = base[this.NextIndex(i)];
                TSVector2 vector3 = b - a;
                FP fp = MathUtils.Area(ref a, ref b, ref point);
                if (((fp == 0f) && (TSVector2.Dot(point - a, vector3) >= 0f)) && (TSVector2.Dot(point - b, vector3) <= 0f))
                {
                    return 0;
                }
                if (a.y <= point.y)
                {
                    if ((b.y > point.y) && (fp > 0f))
                    {
                        num++;
                    }
                }
                else if ((b.y <= point.y) && (fp < 0f))
                {
                    num--;
                }
            }
            return ((num == 0) ? -1 : 1);
        }

        public bool PointInPolygonAngle(ref TSVector2 point)
        {
            FP fp = 0;
            for (int i = 0; i < base.Count; i++)
            {
                TSVector2 vector = base[i] - point;
                TSVector2 vector2 = base[this.NextIndex(i)] - point;
                fp += MathUtils.VectorAngle(ref vector, ref vector2);
            }
            if (FP.Abs(fp) < FP.Pi)
            {
                return false;
            }
            return true;
        }

        public int PreviousIndex(int index)
        {
            return (((index - 1) < 0) ? (base.Count - 1) : (index - 1));
        }

        public TSVector2 PreviousVertex(int index)
        {
            return base[this.PreviousIndex(index)];
        }

        public void ProjectToAxis(ref TSVector2 axis, out FP min, out FP max)
        {
            FP fp = TSVector2.Dot(axis, base[0]);
            min = fp;
            max = fp;
            for (int i = 0; i < base.Count; i++)
            {
                fp = TSVector2.Dot(base[i], axis);
                if (fp < min)
                {
                    min = fp;
                }
                else if (fp > max)
                {
                    max = fp;
                }
            }
        }

        public void Rotate(FP value)
        {
            Debug.Assert(!this.AttachedToBody, "Rotating vertices that are used by a Body can result in unstable behavior.");
            FP fp = FP.Cos(value);
            FP fp2 = FP.Sin(value);
            for (int i = 0; i < base.Count; i++)
            {
                TSVector2 vector = base[i];
                base[i] = new TSVector2((vector.x * fp) + (vector.y * -fp2), (vector.x * fp2) + (vector.y * fp));
            }
            if ((this.Holes != null) && (this.Holes.Count > 0))
            {
                foreach (Vertices vertices in this.Holes)
                {
                    vertices.Rotate(value);
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
            if ((this.Holes != null) && (this.Holes.Count > 0))
            {
                foreach (Vertices vertices in this.Holes)
                {
                    vertices.Scale(ref value);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < base.Count; i++)
            {
                builder.Append(base[i].ToString());
                if (i < (base.Count - 1))
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        public void Transform(ref Matrix transform)
        {
            for (int i = 0; i < base.Count; i++)
            {
                base[i] = TSVector2.Transform(base[i], transform);
            }
            if ((this.Holes != null) && (this.Holes.Count > 0))
            {
                for (int j = 0; j < this.Holes.Count; j++)
                {
                    TSVector2[] vertices = this.Holes[j].ToArray();
                    int index = 0;
                    int length = vertices.Length;
                    while (index < length)
                    {
                        vertices[index] = TSVector2.Transform(vertices[index], transform);
                        index++;
                    }
                    this.Holes[j] = new Vertices(vertices);
                }
            }
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
            if ((this.Holes != null) && (this.Holes.Count > 0))
            {
                foreach (Vertices vertices in this.Holes)
                {
                    vertices.Translate(ref value);
                }
            }
        }

        internal bool AttachedToBody { get; set; }

        public List<Vertices> Holes { get; set; }
    }
}

