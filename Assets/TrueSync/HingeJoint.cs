using System;

namespace TrueSync
{
	public class HingeJoint : Joint
	{
		private PointOnPoint[] worldPointConstraint;

		public PointOnPoint PointOnPointConstraint1
		{
			get
			{
				return this.worldPointConstraint[0];
			}
		}

		public PointOnPoint PointOnPointConstraint2
		{
			get
			{
				return this.worldPointConstraint[1];
			}
		}

		public FP AppliedImpulse
		{
			get
			{
				return this.worldPointConstraint[0].AppliedImpulse + this.worldPointConstraint[1].AppliedImpulse;
			}
		}

		public HingeJoint(World world, RigidBody body1, RigidBody body2, TSVector position, TSVector hingeAxis) : base(world)
		{
			this.worldPointConstraint = new PointOnPoint[2];
			hingeAxis *= FP.Half;
			TSVector anchor = position;
			TSVector.Add(ref anchor, ref hingeAxis, out anchor);
			TSVector anchor2 = position;
			TSVector.Subtract(ref anchor2, ref hingeAxis, out anchor2);
			this.worldPointConstraint[0] = new PointOnPoint(body1, body2, anchor);
			this.worldPointConstraint[1] = new PointOnPoint(body1, body2, anchor2);
		}

		public override void Activate()
		{
			base.World.AddConstraint(this.worldPointConstraint[0]);
			base.World.AddConstraint(this.worldPointConstraint[1]);
		}

		public override void Deactivate()
		{
			base.World.RemoveConstraint(this.worldPointConstraint[0]);
			base.World.RemoveConstraint(this.worldPointConstraint[1]);
		}
	}
}
