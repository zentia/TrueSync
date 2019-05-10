using System;

namespace TrueSync
{
	public class PointPointDistance : Constraint
	{
		public enum DistanceBehavior
		{
			LimitDistance,
			LimitMaximumDistance,
			LimitMinimumDistance
		}

		private TSVector localAnchor1;

		private TSVector localAnchor2;

		private TSVector r1;

		private TSVector r2;

		private FP biasFactor = FP.EN1;

		private FP softness = FP.EN2;

		private FP distance;

		private PointPointDistance.DistanceBehavior behavior = PointPointDistance.DistanceBehavior.LimitDistance;

		private FP effectiveMass = FP.Zero;

		private FP accumulatedImpulse = FP.Zero;

		private FP bias;

		private FP softnessOverDt;

		private TSVector[] jacobian = new TSVector[4];

		private bool skipConstraint = false;

		public FP AppliedImpulse
		{
			get
			{
				return this.accumulatedImpulse;
			}
		}

		public FP Distance
		{
			get
			{
				return this.distance;
			}
			set
			{
				this.distance = value;
			}
		}

		public PointPointDistance.DistanceBehavior Behavior
		{
			get
			{
				return this.behavior;
			}
			set
			{
				this.behavior = value;
			}
		}

		public TSVector LocalAnchor1
		{
			get
			{
				return this.localAnchor1;
			}
			set
			{
				this.localAnchor1 = value;
			}
		}

		public TSVector LocalAnchor2
		{
			get
			{
				return this.localAnchor2;
			}
			set
			{
				this.localAnchor2 = value;
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

		public PointPointDistance(RigidBody body1, RigidBody body2, TSVector anchor1, TSVector anchor2) : base(body1, body2)
		{
			TSVector.Subtract(ref anchor1, ref body1.position, out this.localAnchor1);
			TSVector.Subtract(ref anchor2, ref body2.position, out this.localAnchor2);
			TSVector.Transform(ref this.localAnchor1, ref body1.invOrientation, out this.localAnchor1);
			TSVector.Transform(ref this.localAnchor2, ref body2.invOrientation, out this.localAnchor2);
			this.distance = (anchor1 - anchor2).magnitude;
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
			FP x = tSVector.magnitude - this.distance;
			bool flag = this.behavior == PointPointDistance.DistanceBehavior.LimitMaximumDistance && x <= FP.Zero;
			if (flag)
			{
				this.skipConstraint = true;
			}
			else
			{
				bool flag2 = this.behavior == PointPointDistance.DistanceBehavior.LimitMinimumDistance && x >= FP.Zero;
				if (flag2)
				{
					this.skipConstraint = true;
				}
				else
				{
					this.skipConstraint = false;
					TSVector value3 = value2 - value;
					bool flag3 = value3.sqrMagnitude != FP.Zero;
					if (flag3)
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
					this.bias = x * this.biasFactor * (FP.One / timestep);
					bool flag4 = !this.body1.isStatic;
					if (flag4)
					{
						this.body1.linearVelocity += this.body1.inverseMass * this.accumulatedImpulse * this.jacobian[0];
						this.body1.angularVelocity += TSVector.Transform(this.accumulatedImpulse * this.jacobian[1], this.body1.invInertiaWorld);
					}
					bool flag5 = !this.body2.isStatic;
					if (flag5)
					{
						this.body2.linearVelocity += this.body2.inverseMass * this.accumulatedImpulse * this.jacobian[2];
						this.body2.angularVelocity += TSVector.Transform(this.accumulatedImpulse * this.jacobian[3], this.body2.invInertiaWorld);
					}
				}
			}
		}

		public override void Iterate()
		{
			bool flag = this.skipConstraint;
			if (!flag)
			{
				FP x = this.body1.linearVelocity * this.jacobian[0] + this.body1.angularVelocity * this.jacobian[1] + this.body2.linearVelocity * this.jacobian[2] + this.body2.angularVelocity * this.jacobian[3];
				FP y = this.accumulatedImpulse * this.softnessOverDt;
				FP fP = -this.effectiveMass * (x + this.bias + y);
				bool flag2 = this.behavior == PointPointDistance.DistanceBehavior.LimitMinimumDistance;
				if (flag2)
				{
					FP y2 = this.accumulatedImpulse;
					this.accumulatedImpulse = TSMath.Max(this.accumulatedImpulse + fP, 0);
					fP = this.accumulatedImpulse - y2;
				}
				else
				{
					bool flag3 = this.behavior == PointPointDistance.DistanceBehavior.LimitMaximumDistance;
					if (flag3)
					{
						FP y3 = this.accumulatedImpulse;
						this.accumulatedImpulse = TSMath.Min(this.accumulatedImpulse + fP, 0);
						fP = this.accumulatedImpulse - y3;
					}
					else
					{
						this.accumulatedImpulse += fP;
					}
				}
				bool flag4 = !this.body1.isStatic;
				if (flag4)
				{
					this.body1.linearVelocity += this.body1.inverseMass * fP * this.jacobian[0];
					this.body1.angularVelocity += TSVector.Transform(fP * this.jacobian[1], this.body1.invInertiaWorld);
				}
				bool flag5 = !this.body2.isStatic;
				if (flag5)
				{
					this.body2.linearVelocity += this.body2.inverseMass * fP * this.jacobian[2];
					this.body2.angularVelocity += TSVector.Transform(fP * this.jacobian[3], this.body2.invInertiaWorld);
				}
			}
		}

		public override void DebugDraw(IDebugDrawer drawer)
		{
			drawer.DrawLine(this.body1.position + this.r1, this.body2.position + this.r2);
		}
	}
}
