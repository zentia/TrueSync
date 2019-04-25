namespace TrueSync
{
    using System;

    public class OverlapPairContact : IComparable
    {
        public Contact contact;
        public IBroadphaseEntity Entity1;
        public IBroadphaseEntity Entity2;

        public OverlapPairContact(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            this.Entity1 = entity1;
            this.Entity2 = entity2;
        }

        public int CompareTo(object obj)
        {
            if (obj is OverlapPairContact)
            {
                long hashCode = ((OverlapPairContact) obj).GetHashCode();
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
            OverlapPairContact contact = (OverlapPairContact) obj;
            return ((contact.Entity1.Equals(this.Entity1) && contact.Entity2.Equals(this.Entity2)) || (contact.Entity1.Equals(this.Entity2) && contact.Entity2.Equals(this.Entity1)));
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

