namespace TrueSync
{
    using System;

    public interface IWorldClone
    {
        void Clone(IWorld iWorld);
        void Clone(IWorld iWorld, bool doChecksum);
        void Restore(IWorld iWorld);

        string checksum { get; }
    }
}

