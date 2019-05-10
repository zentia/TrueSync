using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class AngleJoint : Joint
	{
		private FP _bias;

		private FP _jointError;

		private FP _massFactor;

		private FP _targetAngle;

		public override TSVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.Position;
			}
			set
			{
				Debug.Assert(false, "You can't set the world anchor on this joint type.");
			}
		}

		public override TSVector2 WorldAnchorB
		{
			get
			{
				return base.BodyB.Position;
			}
			set
			{
				Debug.Assert(false, "You can't set the world anchor on this joint type.");
			}
		}

		public FP TargetAngle
		{
			get
			{
				return this._targetAngle;
			}
			set
			{
				bool flag = value != this._targetAngle;
				if (flag)
				{
					this._targetAngle = value;
					base.WakeBodies();
				}
			}
		}

		public FP BiasFactor
		{
			get;
			set;
		}

		public FP MaxImpulse
		{
			get;
			set;
		}

		public FP Softness
		{
			get;
			set;
		}

		internal AngleJoint()
		{
			base.JointType = JointType.Angle;
		}

		public AngleJoint(Body bodyA, Body bodyB) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Angle;
			this.BiasFactor = 0.2f;
			this.MaxImpulse = FP.MaxValue;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return TSVector2.zero;
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return 0;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			int islandIndex = base.BodyA.IslandIndex;
			int islandIndex2 = base.BodyB.IslandIndex;
			FP a = data.positions[islandIndex].a;
			FP a2 = data.positions[islandIndex2].a;
			this._jointError = a2 - a - this.TargetAngle;
			this._bias = -this.BiasFactor * data.step.inv_dt * this._jointError;
			this._massFactor = (1 - this.Softness) / (base.BodyA._invI + base.BodyB._invI);
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			int islandIndex = base.BodyA.IslandIndex;
			int islandIndex2 = base.BodyB.IslandIndex;
			FP value = (this._bias - data.velocities[islandIndex2].w + data.velocities[islandIndex].w) * this._massFactor;
			Velocity[] expr_68_cp_0_cp_0 = data.velocities;
			int expr_68_cp_0_cp_1 = islandIndex;
			expr_68_cp_0_cp_0[expr_68_cp_0_cp_1].w = expr_68_cp_0_cp_0[expr_68_cp_0_cp_1].w - base.BodyA._invI * FP.Sign(value) * TSMath.Min(FP.Abs(value), this.MaxImpulse);
			Velocity[] expr_BA_cp_0_cp_0 = data.velocities;
			int expr_BA_cp_0_cp_1 = islandIndex2;
			expr_BA_cp_0_cp_0[expr_BA_cp_0_cp_1].w = expr_BA_cp_0_cp_0[expr_BA_cp_0_cp_1].w + base.BodyB._invI * FP.Sign(value) * TSMath.Min(FP.Abs(value), this.MaxImpulse);
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}
