namespace Microsoft.Xna.Framework
{
    using System;
    using TrueSync;

    public class CurveKey : IEquatable<CurveKey>, IComparable<CurveKey>
    {
        private CurveContinuity continuity;
        private FP position;
        private FP tangentIn;
        private FP tangentOut;
        private FP value;

        public CurveKey(FP position, FP value) : this(position, value, 0, 0, CurveContinuity.Smooth)
        {
        }

        public CurveKey(FP position, FP value, FP tangentIn, FP tangentOut) : this(position, value, tangentIn, tangentOut, CurveContinuity.Smooth)
        {
        }

        public CurveKey(FP position, FP value, FP tangentIn, FP tangentOut, CurveContinuity continuity)
        {
            this.position = position;
            this.value = value;
            this.tangentIn = tangentIn;
            this.tangentOut = tangentOut;
            this.continuity = continuity;
        }

        public CurveKey Clone()
        {
            return new CurveKey(this.position, this.value, this.tangentIn, this.tangentOut, this.continuity);
        }

        public int CompareTo(CurveKey other)
        {
            return this.position.CompareTo(other.position);
        }

        public bool Equals(CurveKey other)
        {
            return (this == other);
        }

        public override bool Equals(object obj)
        {
            return ((obj is CurveKey) ? (((CurveKey) obj) == this) : false);
        }

        public override int GetHashCode()
        {
            return ((((this.position.GetHashCode() ^ this.value.GetHashCode()) ^ this.tangentIn.GetHashCode()) ^ this.tangentOut.GetHashCode()) ^ this.continuity.GetHashCode());
        }

        public static bool operator ==(CurveKey a, CurveKey b)
        {
            if (object.Equals(a, null))
            {
                return object.Equals(b, null);
            }
            if (object.Equals(b, null))
            {
                return object.Equals(a, null);
            }
            return ((((a.position == b.position) && (a.value == b.value)) && ((a.tangentIn == b.tangentIn) && (a.tangentOut == b.tangentOut))) && (a.continuity == b.continuity));
        }

        public static bool operator !=(CurveKey a, CurveKey b)
        {
            return !(a == b);
        }

        public CurveContinuity Continuity
        {
            get
            {
                return this.continuity;
            }
            set
            {
                this.continuity = value;
            }
        }

        public FP Position
        {
            get
            {
                return this.position;
            }
        }

        public FP TangentIn
        {
            get
            {
                return this.tangentIn;
            }
            set
            {
                this.tangentIn = value;
            }
        }

        public FP TangentOut
        {
            get
            {
                return this.tangentOut;
            }
            set
            {
                this.tangentOut = value;
            }
        }

        public FP Value
        {
            get
            {
                return this.value;
            }
            set
            {
                this.value = value;
            }
        }
    }
}

