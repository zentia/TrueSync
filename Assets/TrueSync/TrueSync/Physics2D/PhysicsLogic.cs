namespace TrueSync.Physics2D
{
    using System;

    public abstract class PhysicsLogic : FilterData
    {
        private PhysicsLogicType _type;
        public TrueSync.Physics2D.World World;

        public PhysicsLogic(TrueSync.Physics2D.World world, PhysicsLogicType type)
        {
            this._type = type;
            this.World = world;
        }

        public override bool IsActiveOn(Body body)
        {
            if (body.PhysicsLogicFilter.IsPhysicsLogicIgnored(this._type))
            {
                return false;
            }
            return base.IsActiveOn(body);
        }
    }
}

