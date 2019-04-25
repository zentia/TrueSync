namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct TOIOutput
    {
        public TOIOutputState State;
        public FP T;
    }
}

