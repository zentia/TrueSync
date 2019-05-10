using System;

namespace TrueSync
{
	public class PointOnLine : Constraint
	{
		private TSVector lineNormal;

		private TSVector localAnchor1;

		private TSVector localAnchor2;

		private TSVector r1;

		private TSVector r2;

		private FP biasFactor = FP.Half;

		private FP softness = FP.Zero;

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

		public PointOnLine(RigidBody body1, RigidBody body2, TSVector lineStartPointBody1, TSVector pointBody2) : base(body1, body2)
		{
			TSVector.Subtract(ref lineStartPointBody1, ref body1.position, out this.localAnchor1);
			TSVector.Subtract(ref pointBody2, ref body2.position, out this.localAnchor2);
			TSVector.Transform(ref this.localAnchor1, ref body1.invOrientation, out this.localAnchor1);
			TSVector.Transform(ref this.localAnchor2, ref body2.invOrientation, out this.localAnchor2);
			this.lineNormal = TSVector.Normalize(lineStartPointBody1 - pointBody2);
		}

		public override void PrepareForIteration(FP timestep)
		{
			TSVector.Transform(ref this.localAnchor1, ref this.body1.orientation, out this.r1);
			TSVector.Transform(ref this.localAnchor2, ref this.body2.orientation, out this.r2);
			TSVector tSVector;
			TSVector.Add(ref this.body1.position, ref this.r1, out tSVector);
			TSVector tSVector2;
			TSVector.Add(ref this.body2.position, ref this.r2, out tSVector2);
			TSVector tSVector3;
			TSVector.Subtract(ref tSVector2, ref tSVector, out tSVector3);
			TSVector tSVector4 = TSVector.Transform(this.lineNormal, this.body1.orientation);
			tSVector4.Normalize();
			TSVector tSVector5 = (tSVector - tSVector2) % tSVector4;
			bool flag = tSVector5.sqrMagnitude != FP.Zero;
			if (flag)
			{
				tSVector5.Normalize();
			}
			tSVector5 %= tSVector4;
			this.jacobian[0] = tSVector5;
			this.jacobian[1] = (this.r1 + tSVector2 - tSVector) % tSVector5;
			this.jacobian[2] = -FP.One * tSVector5;
			this.jacobian[3] = -FP.One * this.r2 % tSVector5;
			this.effectiveMass = this.body1.inverseMass + this.body2.inverseMass + TSVector.Transform(this.jacobian[1], this.body1.invInertiaWorld) * this.jacobian[1] + TSVector.Transform(this.jacobian[3], this.body2.invInertiaWorld) * this.jacobian[3];
			this.softnessOverDt = this.softness / timestep;
			this.effectiveMass += this.softnessOverDt;
			bool flag2 = this.effectiveMass != 0;
			if (flag2)
			{
				this.effectiveMass = FP.One / this.effectiveMass;
			}
			this.bias = -(tSVector4 % (tSVector2 - tSVector)).magnitude * this.biasFactor * (FP.One / timestep);
			bool flag3 = !this.body1.isStatic;
			if (flag3)
			{
				this.body1.linearVelocity += this.body1.inverseMass * this.accumulatedImpulse * this.jacobian[0];
				this.body1.angularVelocity += TSVector.Transform(this.accumulatedImpulse * this.jacobian[1], this.body1.invInertiaWorld);
			}
			bool flag4 = !this.body2.isStatic;
			if (flag4)
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
			drawer.DrawLine(this.body1.position + this.r1, this.body1.position + this.r1 + TSVector.Transform(this.lineNormal, this.body1.orientation) * 100);
		}
	}
}
