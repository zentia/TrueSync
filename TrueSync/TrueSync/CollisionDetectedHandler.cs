namespace TrueSync
{
    using System;
    using System.Runtime.CompilerServices;

    public delegate void CollisionDetectedHandler(RigidBody body1, RigidBody body2, TSVector point1, TSVector point2, TSVector normal, FP penetration);
}

