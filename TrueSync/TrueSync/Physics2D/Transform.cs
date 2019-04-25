namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct Transform
    {
        public TSVector2 p;
        public Rot q;
        public Transform(ref TSVector2 position, ref Rot rotation)
        {
            this.p = position;
            this.q = rotation;
        }

        public void SetIdentity()
        {
            this.p = TSVector2.zero;
            this.q.SetIdentity();
        }

        public void Set(TSVector2 position, FP angle)
        {
            this.p = position;
            this.q.Set(angle);
        }
    }
}

