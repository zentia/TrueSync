using System;

namespace TrueSync
{
	public class BodyMaterial
	{
		internal FP kineticFriction = FP.One / 4;

		internal FP staticFriction = FP.One / 2;

		internal FP restitution = FP.Zero;

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
	}
}
