using System;

namespace TrueSync.Physics2D
{
	public abstract class Controller : FilterData
	{
		public bool Enabled;

		public World World;

		private ControllerType _type;

		public Controller(ControllerType controllerType)
		{
			this._type = controllerType;
		}

		public override bool IsActiveOn(Body body)
		{
			bool flag = body.ControllerFilter.IsControllerIgnored(this._type);
			return !flag && base.IsActiveOn(body);
		}

		public abstract void Update(FP dt);
	}
}
