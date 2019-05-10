using System;

namespace TrueSync.Physics2D
{
	public struct ControllerFilter
	{
		public ControllerType ControllerFlags;

		public void IgnoreController(ControllerType controller)
		{
			this.ControllerFlags |= controller;
		}

		public void RestoreController(ControllerType controller)
		{
			this.ControllerFlags &= ~controller;
		}

		public bool IsControllerIgnored(ControllerType controller)
		{
			return (this.ControllerFlags & controller) == controller;
		}
	}
}
