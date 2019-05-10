using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class MotorJoint : Joint
	{
		private TSVector2 _linearOffset;

		private FP _angularOffset;

		private TSVector2 _linearImpulse;

		private FP _angularImpulse;

		private FP _maxForce;

		private FP _maxTorque;

		private int _indexA;

		private int _indexB;

		private TSVector2 _rA;

		private TSVector2 _rB;

		private TSVector2 _localCenterA;

		private TSVector2 _localCenterB;

		private TSVector2 _linearError;

		private FP _angularError;

		private FP _invMassA;

		private FP _invMassB;

		private FP _invIA;

		private FP _invIB;

		private Mat22 _linearMass;

		private FP _angularMass;

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

		public FP MaxForce
		{
			get
			{
				return this._maxForce;
			}
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0f);
				this._maxForce = value;
			}
		}

		public FP MaxTorque
		{
			get
			{
				return this._maxTorque;
			}
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0f);
				this._maxTorque = value;
			}
		}

		public TSVector2 LinearOffset
		{
			get
			{
				return this._linearOffset;
			}
			set
			{
				bool flag = this._linearOffset.x != value.x || this._linearOffset.y != value.y;
				if (flag)
				{
					base.WakeBodies();
					this._linearOffset = value;
				}
			}
		}

		public FP AngularOffset
		{
			get
			{
				return this._angularOffset;
			}
			set
			{
				bool flag = this._angularOffset != value;
				if (flag)
				{
					base.WakeBodies();
					this._angularOffset = value;
				}
			}
		}

		internal FP CorrectionFactor
		{
			get;
			set;
		}

		internal MotorJoint()
		{
			base.JointType = JointType.Motor;
		}

		public MotorJoint(Body bodyA, Body bodyB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Motor;
			TSVector2 position = base.BodyB.Position;
			if (useWorldCoordinates)
			{
				this._linearOffset = base.BodyA.GetLocalPoint(position);
			}
			else
			{
				this._linearOffset = position;
			}
			this._angularOffset = 0f;
			this._maxForce = 1f;
			this._maxTorque = 1f;
			this.CorrectionFactor = 0.3f;
			this._angularOffset = base.BodyB.Rotation - base.BodyA.Rotation;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return invDt * this._linearImpulse;
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return invDt * this._angularImpulse;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			this._indexA = base.BodyA.IslandIndex;
			this._indexB = base.BodyB.IslandIndex;
			this._localCenterA = base.BodyA._sweep.LocalCenter;
			this._localCenterB = base.BodyB._sweep.LocalCenter;
			this._invMassA = base.BodyA._invMass;
			this._invMassB = base.BodyB._invMass;
			this._invIA = base.BodyA._invI;
			this._invIB = base.BodyB._invI;
			TSVector2 c = data.positions[this._indexA].c;
			FP a = data.positions[this._indexA].a;
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 c2 = data.positions[this._indexB].c;
			FP a2 = data.positions[this._indexB].a;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			this._rA = MathUtils.Mul(q, -this._localCenterA);
			this._rB = MathUtils.Mul(q2, -this._localCenterB);
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			Mat22 mat = default(Mat22);
			mat.ex.x = invMassA + invMassB + invIA * this._rA.y * this._rA.y + invIB * this._rB.y * this._rB.y;
			mat.ex.y = -invIA * this._rA.x * this._rA.y - invIB * this._rB.x * this._rB.y;
			mat.ey.x = mat.ex.y;
			mat.ey.y = invMassA + invMassB + invIA * this._rA.x * this._rA.x + invIB * this._rB.x * this._rB.x;
			this._linearMass = mat.Inverse;
			this._angularMass = invIA + invIB;
			bool flag = this._angularMass > 0f;
			if (flag)
			{
				this._angularMass = 1f / this._angularMass;
			}
			this._linearError = c2 + this._rB - c - this._rA - MathUtils.Mul(q, this._linearOffset);
			this._angularError = a2 - a - this._angularOffset;
			this._linearImpulse *= data.step.dtRatio;
			this._angularImpulse *= data.step.dtRatio;
			TSVector2 tSVector3 = new TSVector2(this._linearImpulse.x, this._linearImpulse.y);
			tSVector -= invMassA * tSVector3;
			fP -= invIA * (MathUtils.Cross(this._rA, tSVector3) + this._angularImpulse);
			tSVector2 += invMassB * tSVector3;
			fP2 += invIB * (MathUtils.Cross(this._rB, tSVector3) + this._angularImpulse);
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			FP dt = data.step.dt;
			FP inv_dt = data.step.inv_dt;
			FP y = fP2 - fP + inv_dt * this.CorrectionFactor * this._angularError;
			FP y2 = -this._angularMass * y;
			FP angularImpulse = this._angularImpulse;
			FP fP3 = dt * this._maxTorque;
			this._angularImpulse = MathUtils.Clamp(this._angularImpulse + y2, -fP3, fP3);
			y2 = this._angularImpulse - angularImpulse;
			fP -= invIA * y2;
			fP2 += invIB * y2;
			TSVector2 tSVector3 = tSVector2 + MathUtils.Cross(fP2, this._rB) - tSVector - MathUtils.Cross(fP, this._rA) + inv_dt * this.CorrectionFactor * this._linearError;
			TSVector2 tSVector4 = -MathUtils.Mul(ref this._linearMass, ref tSVector3);
			TSVector2 linearImpulse = this._linearImpulse;
			this._linearImpulse += tSVector4;
			FP fP4 = dt * this._maxForce;
			bool flag = this._linearImpulse.LengthSquared() > fP4 * fP4;
			if (flag)
			{
				this._linearImpulse.Normalize();
				this._linearImpulse *= fP4;
			}
			tSVector4 = this._linearImpulse - linearImpulse;
			tSVector -= invMassA * tSVector4;
			fP -= invIA * MathUtils.Cross(this._rA, tSVector4);
			tSVector2 += invMassB * tSVector4;
			fP2 += invIB * MathUtils.Cross(this._rB, tSVector4);
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}
