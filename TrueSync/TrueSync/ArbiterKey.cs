namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct ArbiterKey : IComparable
    {
        internal RigidBody body1;
        internal RigidBody body2;
        public ArbiterKey(RigidBody body1, RigidBody body2)
        {
            this.body1 = body1;
            this.body2 = body2;
        }

        internal void SetBodies(RigidBody body1, RigidBody body2)
        {
            this.body1 = body1;
            this.body2 = body2;
        }

        public override bool Equals(object obj)
        {
            ArbiterKey key = (ArbiterKey) obj;
            return ((key.body1.Equals(this.body1) && key.body2.Equals(this.body2)) || (key.body1.Equals(this.body2) && key.body2.Equals(this.body1)));
        }

        public override int GetHashCode()
        {
            return (this.body1.GetHashCode() + this.body2.GetHashCode());
        }

        public int CompareTo(object obj)
        {
            if (obj is ArbiterKey)
            {
                long hashCode = ((ArbiterKey) obj).GetHashCode();
                long num2 = this.GetHashCode();
                long num3 = hashCode - num2;
                if (num3 < 0L)
                {
                    return 1;
                }
                if (num3 > 0L)
                {
                    return -1;
                }
            }
            return 0;
        }
    }
}

