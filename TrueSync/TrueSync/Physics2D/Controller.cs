namespace TrueSync.Physics2D
{
    using System;
    using TrueSync;

    public abstract class Controller : FilterData
    {
        private ControllerType _type;
        public bool Enabled;
        public TrueSync.Physics2D.World World;

        public Controller(ControllerType controllerType)
        {
            this._type = controllerType;
        }

        public override bool IsActiveOn(Body body)
        {
            if (body.ControllerFilter.IsControllerIgnored(this._type))
            {
                return false;
            }
            return base.IsActiveOn(body);
        }

        public abstract void Update(FP dt);
    }
}

