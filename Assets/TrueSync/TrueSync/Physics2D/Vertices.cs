// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.Vertices
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace TrueSync.Physics2D
{
    [DebuggerDisplay("Count = {Count} Vertices = {ToString()}")]
    public class Vertices : List<TSVector2>
    {
        public Vertices()
        {
        }

        public Vertices(int capacity)
          : base(capacity)
        {
        }

        public Vertices(IEnumerable<TSVector2> vertices)
        {
            this.AddRange(vertices);
        }

        internal bool AttachedToBody { get; set; }

        public List<Vertices> Holes { get; set; }

        public int NextIndex(int index)
        {
            return index + 1 > this.Count - 1 ? 0 : index + 1;
        }

        public TSVector2 NextVertex(int index)
        {
            return this[this.NextIndex(index)];
        }

        public int PreviousIndex(int index)
        {
            return index - 1 < 0 ? this.Count - 1 : index - 1;
        }

        public TSVector2 PreviousVertex(int index)
        {
            return this[this.PreviousIndex(index)];
        }

        public FP GetSignedArea()
        {
            if (this.Count < 3)
                return (FP)0;
            FP fp = (FP)0;
            for (int index1 = 0; index1 < this.Count; ++index1)
            {
                int index2 = (index1 + 1) % this.Count;
                TSVector2 tsVector2_1 = this[index1];
                TSVector2 tsVector2_2 = this[index2];
                fp = fp + tsVector2_1.x * tsVector2_2.y - tsVector2_1.y * tsVector2_2.x;
            }
            return fp / (FP)2f;
        }

        public FP GetArea()
        {
            FP signedArea = this.GetSignedArea();
            return signedArea < (FP)0 ? -signedArea : signedArea;
        }

        public TSVector2 GetCentroid()
        {
            if (this.Count < 3)
                return new TSVector2(FP.NaN, FP.NaN);
            TSVector2 zero = TSVector2.zero;
            FP fp1 = (FP)0.0f;
            FP fp2 = (FP)0.3333333f;
            for (int index = 0; index < this.Count; ++index)
            {
                TSVector2 tsVector2_1 = this[index];
                TSVector2 tsVector2_2 = index + 1 < this.Count ? this[index + 1] : this[0];
                FP fp3 = (FP)0.5f * (tsVector2_1.x * tsVector2_2.y - tsVector2_1.y * tsVector2_2.x);
                fp1 += fp3;
                zero += fp3 * fp2 * (tsVector2_1 + tsVector2_2);
            }
            return zero * ((FP)1f / fp1);
        }

        public AABB GetAABB()
        {
            TSVector2 tsVector2_1 = new TSVector2(FP.MaxValue, FP.MaxValue);
            TSVector2 tsVector2_2 = new TSVector2(FP.MinValue, FP.MinValue);
            for (int index = 0; index < this.Count; ++index)
            {
                if (this[index].x < tsVector2_1.x)
                    tsVector2_1.x = this[index].x;
                if (this[index].x > tsVector2_2.x)
                    tsVector2_2.x = this[index].x;
                if (this[index].y < tsVector2_1.y)
                    tsVector2_1.y = this[index].y;
                if (this[index].y > tsVector2_2.y)
                    tsVector2_2.y = this[index].y;
            }
            AABB aabb;
            aabb.LowerBound = tsVector2_1;
            aabb.UpperBound = tsVector2_2;
            return aabb;
        }

        public void Translate(TSVector2 value)
        {
            this.Translate(ref value);
        }

        public void Translate(ref TSVector2 value)
        {
            Debug.Assert(!this.AttachedToBody, "Translating vertices that are used by a Body can result in unstable behavior. Use Body.Position instead.");
            for (int index = 0; index < this.Count; ++index)
                this[index] = TSVector2.Add(this[index], value);
            if (this.Holes == null || this.Holes.Count <= 0)
                return;
            foreach (Vertices hole in this.Holes)
                hole.Translate(ref value);
        }

        public void Scale(TSVector2 value)
        {
            this.Scale(ref value);
        }

        public void Scale(ref TSVector2 value)
        {
            Debug.Assert(!this.AttachedToBody, "Scaling vertices that are used by a Body can result in unstable behavior.");
            for (int index = 0; index < this.Count; ++index)
                this[index] = TSVector2.Multiply(this[index], value);
            if (this.Holes == null || this.Holes.Count <= 0)
                return;
            foreach (Vertices hole in this.Holes)
                hole.Scale(ref value);
        }

        public void Rotate(FP value)
        {
            Debug.Assert(!this.AttachedToBody, "Rotating vertices that are used by a Body can result in unstable behavior.");
            FP fp1 = FP.Cos(value);
            FP fp2 = FP.Sin(value);
            for (int index = 0; index < this.Count; ++index)
            {
                TSVector2 tsVector2 = this[index];
                this[index] = new TSVector2(tsVector2.x * fp1 + tsVector2.y * -fp2, tsVector2.x * fp2 + tsVector2.y * fp1);
            }
            if (this.Holes == null || this.Holes.Count <= 0)
                return;
            foreach (Vertices hole in this.Holes)
                hole.Rotate(value);
        }

        public bool IsConvex()
        {
            if (this.Count < 3)
                return false;
            if (this.Count == 3)
                return true;
            for (int index1 = 0; index1 < this.Count; ++index1)
            {
                int index2 = index1 + 1 < this.Count ? index1 + 1 : 0;
                TSVector2 tsVector2_1 = this[index2] - this[index1];
                for (int index3 = 0; index3 < this.Count; ++index3)
                {
                    if (index3 != index1 && index3 != index2)
                    {
                        TSVector2 tsVector2_2 = this[index3] - this[index1];
                        if (tsVector2_1.x * tsVector2_2.y - tsVector2_1.y * tsVector2_2.x <= (FP)0.0f)
                            return false;
                    }
                }
            }
            return true;
        }

        public bool IsCounterClockWise()
        {
            if (this.Count < 3)
                return false;
            return this.GetSignedArea() > (FP)0.0f;
        }

        public void ForceCounterClockWise()
        {
            if (this.Count < 3 || this.IsCounterClockWise())
                return;
            this.Reverse();
        }

        public bool IsSimple()
        {
            if (this.Count < 3)
                return false;
            for (int index1 = 0; index1 < this.Count; ++index1)
            {
                TSVector2 a0 = this[index1];
                TSVector2 a1 = this.NextVertex(index1);
                for (int index2 = index1 + 1; index2 < this.Count; ++index2)
                {
                    TSVector2 b0 = this[index2];
                    TSVector2 b1 = this.NextVertex(index2);
                    TSVector2 intersectionPoint;
                    if (LineTools.LineIntersect2(ref a0, ref a1, ref b0, ref b1, out intersectionPoint))
                        return false;
                }
            }
            return true;
        }

        public PolygonError CheckPolygon()
        {
            if (this.Count < 3 || this.Count > Settings.MaxPolygonVertices)
                return PolygonError.InvalidAmountOfVertices;
            if (!this.IsSimple())
                return PolygonError.NotSimple;
            if (this.GetArea() <= Settings.Epsilon)
                return PolygonError.AreaTooSmall;
            if (!this.IsConvex())
                return PolygonError.NotConvex;
            for (int index = 0; index < this.Count; ++index)
            {
                if ((this[index + 1 < this.Count ? index + 1 : 0] - this[index]).LengthSquared() <= Settings.EpsilonSqr)
                    return PolygonError.SideTooSmall;
            }
            return !this.IsCounterClockWise() ? PolygonError.NotCounterClockWise : PolygonError.NoError;
        }

        public void ProjectToAxis(ref TSVector2 axis, out FP min, out FP max)
        {
            FP fp1 = TSVector2.Dot(axis, this[0]);
            min = fp1;
            max = fp1;
            for (int index = 0; index < this.Count; ++index)
            {
                FP fp2 = TSVector2.Dot(this[index], axis);
                if (fp2 < min)
                    min = fp2;
                else if (fp2 > max)
                    max = fp2;
            }
        }

        public int PointInPolygon(ref TSVector2 point)
        {
            int num = 0;
            for (int index = 0; index < this.Count; ++index)
            {
                TSVector2 a = this[index];
                TSVector2 b = this[this.NextIndex(index)];
                TSVector2 tsVector2 = b - a;
                FP fp = MathUtils.Area(ref a, ref b, ref point);
                if (fp == (FP)0.0f && TSVector2.Dot(point - a, tsVector2) >= (FP)0.0f && TSVector2.Dot(point - b, tsVector2) <= (FP)0.0f)
                    return 0;
                if (a.y <= point.y)
                {
                    if (b.y > point.y && fp > (FP)0.0f)
                        ++num;
                }
                else if (b.y <= point.y && fp < (FP)0.0f)
                    --num;
            }
            return num == 0 ? -1 : 1;
        }

        public bool PointInPolygonAngle(ref TSVector2 point)
        {
            FP fp = (FP)0;
            for (int index = 0; index < this.Count; ++index)
            {
                TSVector2 p1 = this[index] - point;
                TSVector2 p2 = this[this.NextIndex(index)] - point;
                fp += MathUtils.VectorAngle(ref p1, ref p2);
            }
            return !(FP.Abs(fp) < FP.Pi);
        }

        public void Transform(ref Matrix transform)
        {
            for (int index = 0; index < this.Count; ++index)
                this[index] = TSVector2.Transform(this[index], transform);
            if (this.Holes == null || this.Holes.Count <= 0)
                return;
            for (int index1 = 0; index1 < this.Holes.Count; ++index1)
            {
                TSVector2[] array = this.Holes[index1].ToArray();
                int index2 = 0;
                for (int length = array.Length; index2 < length; ++index2)
                    array[index2] = TSVector2.Transform(array[index2], transform);
                this.Holes[index1] = new Vertices((IEnumerable<TSVector2>)array);
            }
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < this.Count; ++index)
            {
                stringBuilder.Append(this[index].ToString());
                if (index < this.Count - 1)
                    stringBuilder.Append(" ");
            }
            return stringBuilder.ToString();
        }
    }
}
