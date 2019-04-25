namespace TrueSync
{
    using System;

    public enum TSRigidBodyConstraints
    {
        FreezeAll = 0x7e,
        FreezePosition = 14,
        FreezePositionX = 2,
        FreezePositionY = 4,
        FreezePositionZ = 8,
        FreezeRotation = 0x70,
        FreezeRotationX = 0x10,
        FreezeRotationY = 0x20,
        FreezeRotationZ = 0x40,
        None = 0
    }
}

