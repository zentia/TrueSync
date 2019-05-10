using System;

namespace TrueSync
{
	public interface IBody3D : IBody
	{
		TSVector TSPosition
		{
			get;
			set;
		}

		TSMatrix TSOrientation
		{
			get;
			set;
		}

		bool TSAffectedByGravity
		{
			get;
			set;
		}

		bool TSIsKinematic
		{
			get;
			set;
		}

		TSVector TSLinearVelocity
		{
			get;
			set;
		}

		TSVector TSAngularVelocity
		{
			get;
			set;
		}

		void TSApplyForce(TSVector force);

		void TSApplyForce(TSVector force, TSVector position);

		void TSApplyImpulse(TSVector force);

		void TSApplyImpulse(TSVector force, TSVector position);

		void TSApplyTorque(TSVector force);
	}
}
