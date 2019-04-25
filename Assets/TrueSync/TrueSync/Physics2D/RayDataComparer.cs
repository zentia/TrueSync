namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    internal class RayDataComparer : IComparer<FP>
    {
        int IComparer<FP>.Compare(FP a, FP b)
        {
            FP fp = a - b;
            if (fp > 0)
            {
                return 1;
            }
            if (fp < 0)
            {
                return -1;
            }
            return 0;
        }
    }
}

