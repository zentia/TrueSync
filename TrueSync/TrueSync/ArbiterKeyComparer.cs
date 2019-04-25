namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    internal class ArbiterKeyComparer : IEqualityComparer<ArbiterKey>
    {
        public bool Equals(ArbiterKey x, ArbiterKey y)
        {
            return ((x.body1.Equals(y.body1) && x.body2.Equals(y.body2)) || (x.body1.Equals(y.body2) && x.body2.Equals(y.body1)));
        }

        public int GetHashCode(ArbiterKey obj)
        {
            return (obj.body1.GetHashCode() + obj.body2.GetHashCode());
        }
    }
}

