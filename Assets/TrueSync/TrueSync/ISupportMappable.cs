namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    public interface ISupportMappable
    {
        void SupportCenter(out TSVector center);
        void SupportMapping(ref TSVector direction, out TSVector result);
    }
}

