using System;

namespace TrueSync.Physics2D
{
	public class RopeJoint : Joint
	{
		private FP _impulse;

		private FP _length;

		private int _indexA;

		private int _indexB;

		private TSVector2 _localCenterA;

		private TSVector2 _localCenterB;

		private FP _invMassA;

		private FP _invMassB;

		private FP _invIA;

		private FP _invIB;

		private FP _mass;

		private TSVector2 _rA;

		private TSVector2 _rB;

		private TSVector2 _u;

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

		public sealed override TSVector2 WorldAnchorA
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

		public sealed override TSVector2 WorldAnchorB
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

		public FP MaxLength
		{
			get;
			set;
		}

		public LimitState State
		{
			get;
			private set;
		}

		internal RopeJoint()
		{
			base.JointType = JointType.Rope;
		}

		public RopeJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Rope;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = bodyA.GetLocalPoint(anchorA);
				this.LocalAnchorB = bodyB.GetLocalPoint(anchorB);
			}
			else
			{
				this.LocalAnchorA = anchorA;
				this.LocalAnchorB = anchorB;
			}
			this.MaxLength = (this.WorldAnchorB - this.WorldAnchorA).magnitude;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return invDt * this._impulse * this._u;
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return 0;
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
			this._rA = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			this._rB = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			this._u = c2 + this._rB - c - this._rA;
			this._length = this._u.magnitude;
			FP x = this._length - this.MaxLength;
			bool flag = x > 0f;
			if (flag)
			{
				this.State = LimitState.AtUpper;
			}
			else
			{
				this.State = LimitState.Inactive;
			}
			bool flag2 = this._length > Settings.LinearSlop;
			if (flag2)
			{
				this._u *= 1f / this._length;
				FP y = MathUtils.Cross(this._rA, this._u);
				FP y2 = MathUtils.Cross(this._rB, this._u);
				FP fP3 = this._invMassA + this._invIA * y * y + this._invMassB + this._invIB * y2 * y2;
				this._mass = ((fP3 != 0f) ? (1f / fP3) : 0f);
				this._impulse *= data.step.dtRatio;
				TSVector2 tSVector3 = this._impulse * this._u;
				tSVector -= this._invMassA * tSVector3;
				fP -= this._invIA * MathUtils.Cross(this._rA, tSVector3);
				tSVector2 += this._invMassB * tSVector3;
				fP2 += this._invIB * MathUtils.Cross(this._rB, tSVector3);
				data.velocities[this._indexA].v = tSVector;
				data.velocities[this._indexA].w = fP;
				data.velocities[this._indexB].v = tSVector2;
				data.velocities[this._indexB].w = fP2;
			}
			else
			{
				this._u = TSVector2.zero;
				this._mass = 0f;
				this._impulse = 0f;
			}
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			TSVector2 value = tSVector + MathUtils.Cross(fP, this._rA);
			TSVector2 value2 = tSVector2 + MathUtils.Cross(fP2, this._rB);
			FP fP3 = this._length - this.MaxLength;
			FP fP4 = TSVector2.Dot(this._u, value2 - value);
			bool flag = fP3 < 0f;
			if (flag)
			{
				fP4 += data.step.inv_dt * fP3;
			}
			FP fP5 = -this._mass * fP4;
			FP impulse = this._impulse;
			this._impulse = TSMath.Min(0f, this._impulse + fP5);
			fP5 = this._impulse - impulse;
			TSVector2 tSVector3 = fP5 * this._u;
			tSVector -= this._invMassA * tSVector3;
			fP -= this._invIA * MathUtils.Cross(this._rA, tSVector3);
			tSVector2 += this._invMassB * tSVector3;
			fP2 += this._invIB * MathUtils.Cross(this._rB, tSVector3);
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.positions[this._indexA].c;
			FP fP = data.positions[this._indexA].a;
			TSVector2 tSVector2 = data.positions[this._indexB].c;
			FP fP2 = data.positions[this._indexB].a;
			Rot q = new Rot(fP);
			Rot q2 = new Rot(fP2);
			TSVector2 tSVector3 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			TSVector2 tSVector4 = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			TSVector2 value = tSVector2 + tSVector4 - tSVector - tSVector3;
			FP magnitude = value.magnitude;
			value.Normalize();
			FP fP3 = magnitude - this.MaxLength;
			fP3 = MathUtils.Clamp(fP3, 0f, Settings.MaxLinearCorrection);
			FP scaleFactor = -this._mass * fP3;
			TSVector2 tSVector5 = scaleFactor * value;
			tSVector -= this._invMassA * tSVector5;
			fP -= this._invIA * MathUtils.Cross(tSVector3, tSVector5);
			tSVector2 += this._invMassB * tSVector5;
			fP2 += this._invIB * MathUtils.Cross(tSVector4, tSVector5);
			data.positions[this._indexA].c = tSVector;
			data.positions[this._indexA].a = fP;
			data.positions[this._indexB].c = tSVector2;
			data.positions[this._indexB].a = fP2;
			return magnitude - this.MaxLength < Settings.LinearSlop;
		}
	}
}
