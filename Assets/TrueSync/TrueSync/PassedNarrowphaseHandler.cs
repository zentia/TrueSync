namespace TrueSync
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate bool PassedNarrowphaseHandler(RigidBody body1, RigidBody body2, ref TSVector point, ref TSVector normal, FP penetration);
}

