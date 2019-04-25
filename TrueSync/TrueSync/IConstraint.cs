namespace TrueSync
{
    using System;

    public interface IConstraint
    {
        void Iterate();
        void PrepareForIteration(FP timestep);

        RigidBody Body1 { get; }

        RigidBody Body2 { get; }
    }
}

