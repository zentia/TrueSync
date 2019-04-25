namespace TrueSync
{
    using System;

    public class OverlapPair : IComparable
    {
        public IBroadphaseEntity Entity1;
        public IBroadphaseEntity Entity2;

        public OverlapPair(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            this.Entity1 = entity1;
            this.Entity2 = entity2;
        }

        public int CompareTo(object obj)
        {
            if (obj is OverlapPair)
            {
                long hashCode = ((OverlapPair) obj).GetHashCode();
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

        public override bool Equals(object obj)
        {
            OverlapPair pair = (OverlapPair) obj;
            return ((pair.Entity1.Equals(this.Entity1) && pair.Entity2.Equals(this.Entity2)) || (pair.Entity1.Equals(this.Entity2) && pair.Entity2.Equals(this.Entity1)));
        }

        public override int GetHashCode()
        {
            return (this.Entity1.GetHashCode() + this.Entity2.GetHashCode());
        }

        internal void SetBodies(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            this.Entity1 = entity1;
            this.Entity2 = entity2;
        }
    }
}

