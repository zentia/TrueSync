namespace TrueSync
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public abstract class Joint
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.World <World>k__BackingField;

        public Joint(TrueSync.World world)
        {
            this.World = world;
        }

        public abstract void Activate();
        public abstract void Deactivate();

        public TrueSync.World World { get; private set; }
    }
}

