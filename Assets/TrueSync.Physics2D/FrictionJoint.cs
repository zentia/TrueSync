using System;

namespace TrueSync.Physics2D
{
	public class FrictionJoint : Joint
	{
		private TSVector2 _linearImpulse;

		private FP _angularImpulse;

		private int _indexA;

		private int _indexB;

		private TSVector2 _rA;

		private TSVector2 _rB;

		private TSVector2 _localCenterA;

		private TSVector2 _localCenterB;

		private FP _invMassA;

		private FP _invMassB;

		private FP _invIA;

		private FP _invIB;

		private FP _angularMass;

		private Mat22 _linearMass;

		public TSVector2 LocalAnchorA
		{
			get;
			set;
		}

		public TSVector2 LocalAnchorB
		{
			get;
			set;
		}

		public override TSVector2 WorldAnchorA
		{
			get
			{
				return base.BodyA.GetWorldPoint(this.LocalAnchorA);
			}
			set
			{
				this.LocalAnchorA = base.BodyA.GetLocalPoint(value);
			}
		}

		public override TSVector2 WorldAnchorB
		{
			get
			{
				return base.BodyB.GetWorldPoint(this.LocalAnchorB);
			}
			set
			{
				this.LocalAnchorB = base.BodyB.GetLocalPoint(value);
			}
		}

		public FP MaxForce
		{
			get;
			set;
		}

		public FP MaxTorque
		{
			get;
			set;
		}

		internal FrictionJoint()
		{
			base.JointType = JointType.Friction;
		}

		public FrictionJoint(Body bodyA, Body bodyB, TSVector2 anchor, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Friction;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = base.BodyA.GetLocalPoint(anchor);
				this.LocalAnchorB = base.BodyB.GetLocalPoint(anchor);
			}
			else
			{
				this.LocalAnchorA = anchor;
				this.LocalAnchorB = anchor;
			}
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
			FP a = data.positions[this._indexA].a;
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			FP a2 = data.positions[this._indexB].a;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			this._rA = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			this._rB = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
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
			FP y = fP2 - fP;
			FP y2 = -this._angularMass * y;
			FP angularImpulse = this._angularImpulse;
			FP fP3 = dt * this.MaxTorque;
			this._angularImpulse = MathUtils.Clamp(this._angularImpulse + y2, -fP3, fP3);
			y2 = this._angularImpulse - angularImpulse;
			fP -= invIA * y2;
			fP2 += invIB * y2;
			TSVector2 v = tSVector2 + MathUtils.Cross(fP2, this._rB) - tSVector - MathUtils.Cross(fP, this._rA);
			TSVector2 tSVector3 = -MathUtils.Mul(ref this._linearMass, v);
			TSVector2 linearImpulse = this._linearImpulse;
			this._linearImpulse += tSVector3;
			FP fP4 = dt * this.MaxForce;
			bool flag = this._linearImpulse.LengthSquared() > fP4 * fP4;
			if (flag)
			{
				this._linearImpulse.Normalize();
				this._linearImpulse *= fP4;
			}
			tSVector3 = this._linearImpulse - linearImpulse;
			tSVector -= invMassA * tSVector3;
			fP -= invIA * MathUtils.Cross(this._rA, tSVector3);
			tSVector2 += invMassB * tSVector3;
			fP2 += invIB * MathUtils.Cross(this._rB, tSVector3);
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
