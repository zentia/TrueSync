namespace TrueSync
{
    using System;

    public class BodyMaterial
    {
        internal FP kineticFriction = (FP.One / 4);
        internal FP restitution = FP.Zero;
        internal FP staticFriction = (FP.One / 2);

        public FP KineticFriction
        {
            get
            {
                return this.kineticFriction;
            }
            set
            {
                this.kineticFriction = value;
            }
        }

        public FP Restitution
        {
            get
            {
                return this.restitution;
            }
            set
            {
                this.restitution = value;
            }
        }

        public FP StaticFriction
        {
            get
            {
                return this.staticFriction;
            }
            set
            {
                this.staticFriction = value;
            }
        }
    }
}

