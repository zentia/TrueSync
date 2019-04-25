namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct Pair : IComparable<Pair>
    {
        public int ProxyIdA;
        public int ProxyIdB;
        public int CompareTo(Pair other)
        {
            if (this.ProxyIdA < other.ProxyIdA)
            {
                return -1;
            }
            if (this.ProxyIdA == other.ProxyIdA)
            {
                if (this.ProxyIdB < other.ProxyIdB)
                {
                    return -1;
                }
                if (this.ProxyIdB == other.ProxyIdB)
                {
                    return 0;
                }
            }
            return 1;
        }
    }
}

