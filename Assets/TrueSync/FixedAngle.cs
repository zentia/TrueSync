using System;

namespace TrueSync
{
	public class FixedAngle : Constraint
	{
		private FP biasFactor = 5 * FP.EN2;

		private FP softness = FP.Zero;

		private TSVector accumulatedImpulse;

		private TSMatrix initialOrientation1;

		private TSMatrix initialOrientation2;

		private TSMatrix effectiveMass;

		private TSVector bias;

		private FP softnessOverDt;

		public TSVector AppliedImpulse
		{
			get
			{
				return this.accumulatedImpulse;
			}
		}

		public TSMatrix InitialOrientationBody1
		{
			get
			{
				return this.initialOrientation1;
			}
			set
			{
				this.initialOrientation1 = value;
			}
		}

		public TSMatrix InitialOrientationBody2
		{
			get
			{
				return this.initialOrientation2;
			}
			set
			{
				this.initialOrientation2 = value;
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

		public FixedAngle(RigidBody body1, RigidBody body2) : base(body1, body2)
		{
			this.initialOrientation1 = body1.orientation;
			this.initialOrientation2 = body2.orientation;
		}

		public override void PrepareForIteration(FP timestep)
		{
			this.effectiveMass = this.body1.invInertiaWorld + this.body2.invInertiaWorld;
			this.softnessOverDt = this.softness / timestep;
			this.effectiveMass.M11 = this.effectiveMass.M11 + this.softnessOverDt;
			this.effectiveMass.M22 = this.effectiveMass.M22 + this.softnessOverDt;
			this.effectiveMass.M33 = this.effectiveMass.M33 + this.softnessOverDt;
			TSMatrix.Inverse(ref this.effectiveMass, out this.effectiveMass);
			TSMatrix value;
			TSMatrix.Multiply(ref this.initialOrientation1, ref this.initialOrientation2, out value);
			TSMatrix.Transpose(ref value, out value);
			TSMatrix tSMatrix = value * this.body2.invOrientation * this.body1.orientation;
			FP fP = tSMatrix.M32 - tSMatrix.M23;
			FP fP2 = tSMatrix.M13 - tSMatrix.M31;
			FP fP3 = tSMatrix.M21 - tSMatrix.M12;
			FP fP4 = TSMath.Sqrt(fP * fP + fP2 * fP2 + fP3 * fP3);
			FP x = tSMatrix.M11 + tSMatrix.M22 + tSMatrix.M33;
			FP value2 = FP.Atan2(fP4, x - 1);
			TSVector value3 = new TSVector(fP, fP2, fP3) * value2;
			bool flag = fP4 != FP.Zero;
			if (flag)
			{
				value3 *= FP.One / fP4;
			}
			this.bias = value3 * this.biasFactor * (-FP.One / timestep);
			bool flag2 = !this.body1.IsStatic;
			if (flag2)
			{
				this.body1.angularVelocity += TSVector.Transform(this.accumulatedImpulse, this.body1.invInertiaWorld);
			}
			bool flag3 = !this.body2.IsStatic;
			if (flag3)
			{
				this.body2.angularVelocity += TSVector.Transform(-FP.One * this.accumulatedImpulse, this.body2.invInertiaWorld);
			}
		}

		public override void Iterate()
		{
			TSVector value = this.body1.angularVelocity - this.body2.angularVelocity;
			TSVector value2 = this.accumulatedImpulse * this.softnessOverDt;
			TSVector tSVector = -FP.One * TSVector.Transform(value + this.bias + value2, this.effectiveMass);
			this.accumulatedImpulse += tSVector;
			bool flag = !this.body1.IsStatic;
			if (flag)
			{
				this.body1.angularVelocity += TSVector.Transform(tSVector, this.body1.invInertiaWorld);
			}
			bool flag2 = !this.body2.IsStatic;
			if (flag2)
			{
				this.body2.angularVelocity += TSVector.Transform(-FP.One * tSVector, this.body2.invInertiaWorld);
			}
		}
	}
}
