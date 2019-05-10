using System;

namespace TrueSync
{
	public class PointOnPoint : Constraint
	{
		private TSVector localAnchor1;

		private TSVector localAnchor2;

		private TSVector r1;

		private TSVector r2;

		private FP biasFactor = 5 * FP.EN2;

		private FP softness = FP.EN2;

		private FP effectiveMass = FP.Zero;

		private FP accumulatedImpulse = FP.Zero;

		private FP bias;

		private FP softnessOverDt;

		private TSVector[] jacobian = new TSVector[4];

		public FP AppliedImpulse
		{
			get
			{
				return this.accumulatedImpulse;
			}
		}

		public FP Softness
		{
			get
			{
				return this.softness;
			}
			set
			{
				this.softness = value;
			}
		}

		public FP BiasFactor
		{
			get
			{
				return this.biasFactor;
			}
			set
			{
				this.biasFactor = value;
			}
		}

		public PointOnPoint(RigidBody body1, RigidBody body2, TSVector anchor) : base(body1, body2)
		{
			TSVector.Subtract(ref anchor, ref body1.position, out this.localAnchor1);
			TSVector.Subtract(ref anchor, ref body2.position, out this.localAnchor2);
			TSVector.Transform(ref this.localAnchor1, ref body1.invOrientation, out this.localAnchor1);
			TSVector.Transform(ref this.localAnchor2, ref body2.invOrientation, out this.localAnchor2);
		}

		public override void PrepareForIteration(FP timestep)
		{
			TSVector.Transform(ref this.localAnchor1, ref this.body1.orientation, out this.r1);
			TSVector.Transform(ref this.localAnchor2, ref this.body2.orientation, out this.r2);
			TSVector value;
			TSVector.Add(ref this.body1.position, ref this.r1, out value);
			TSVector value2;
			TSVector.Add(ref this.body2.position, ref this.r2, out value2);
			TSVector tSVector;
			TSVector.Subtract(ref value2, ref value, out tSVector);
			FP magnitude = tSVector.magnitude;
			TSVector value3 = value2 - value;
			bool flag = value3.sqrMagnitude != FP.Zero;
			if (flag)
			{
				value3.Normalize();
			}
			this.jacobian[0] = -FP.One * value3;
			this.jacobian[1] = -FP.One * (this.r1 % value3);
			this.jacobian[2] = FP.One * value3;
			this.jacobian[3] = this.r2 % value3;
			this.effectiveMass = this.body1.inverseMass + this.body2.inverseMass + TSVector.Transform(this.jacobian[1], this.body1.invInertiaWorld) * this.jacobian[1] + TSVector.Transform(this.jacobian[3], this.body2.invInertiaWorld) * this.jacobian[3];
			this.softnessOverDt = this.softness / timestep;
			this.effectiveMass += this.softnessOverDt;
			this.effectiveMass = FP.One / this.effectiveMass;
			this.bias = magnitude * this.biasFactor * (FP.One / timestep);
			bool flag2 = !this.body1.isStatic;
			if (flag2)
			{
				this.body1.linearVelocity += this.body1.inverseMass * this.accumulatedImpulse * this.jacobian[0];
				this.body1.angularVelocity += TSVector.Transform(this.accumulatedImpulse * this.jacobian[1], this.body1.invInertiaWorld);
			}
			bool flag3 = !this.body2.isStatic;
			if (flag3)
			{
				this.body2.linearVelocity += this.body2.inverseMass * this.accumulatedImpulse * this.jacobian[2];
				this.body2.angularVelocity += TSVector.Transform(this.accumulatedImpulse * this.jacobian[3], this.body2.invInertiaWorld);
			}
		}

		public override void Iterate()
		{
			FP x = this.body1.linearVelocity * this.jacobian[0] + this.body1.angularVelocity * this.jacobian[1] + this.body2.linearVelocity * this.jacobian[2] + this.body2.angularVelocity * this.jacobian[3];
			FP y = this.accumulatedImpulse * this.softnessOverDt;
			FP fP = -this.effectiveMass * (x + this.bias + y);
			this.accumulatedImpulse += fP;
			bool flag = !this.body1.isStatic;
			if (flag)
			{
				this.body1.linearVelocity += this.body1.inverseMass * fP * this.jacobian[0];
				this.body1.angularVelocity += TSVector.Transform(fP * this.jacobian[1], this.body1.invInertiaWorld);
			}
			bool flag2 = !this.body2.isStatic;
			if (flag2)
			{
				this.body2.linearVelocity += this.body2.inverseMass * fP * this.jacobian[2];
				this.body2.angularVelocity += TSVector.Transform(fP * this.jacobian[3], this.body2.invInertiaWorld);
			}
		}

		public override void DebugDraw(IDebugDrawer drawer)
		{
			drawer.DrawLine(this.body1.position, this.body1.position + this.r1);
			drawer.DrawLine(this.body2.position, this.body2.position + this.r2);
		}
	}
}
