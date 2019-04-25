namespace TrueSync.Physics2D
{
    using System;

    [Flags]
    public enum ControllerType
    {
        AbstractForceController = 4,
        BuoyancyController = 8,
        GravityController = 1,
        VelocityLimitController = 2
    }
}

