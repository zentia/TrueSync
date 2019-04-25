namespace TrueSync.Physics2D
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct SolverData
    {
        public TimeStep step;
        public Position[] positions;
        public Velocity[] velocities;
    }
}

