using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class DistanceJoint : Joint
	{
		private FP _bias;

		private FP _gamma;

		private FP _impulse;

		private int _indexA;

		private int _indexB;

		private TSVector2 _u;

		private TSVector2 _rA;

		private TSVector2 _rB;

		private TSVector2 _localCenterA;

		private TSVector2 _localCenterB;

		private FP _invMassA;

		private FP _invMassB;

		private FP _invIA;

		private FP _invIB;

		private FP _mass;

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
				Debug.Assert(false, "You can't set the world anchor on this joint type.");
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
				Debug.Assert(false, "You can't set the world anchor on this joint type.");
			}
		}

		public FP Length
		{
			get;
			set;
		}

		public FP Frequency
		{
			get;
			set;
		}

		public FP DampingRatio
		{
			get;
			set;
		}

		internal DistanceJoint()
		{
			base.JointType = JointType.Distance;
		}

		public DistanceJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Distance;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = bodyA.GetLocalPoint(ref anchorA);
				this.LocalAnchorB = bodyB.GetLocalPoint(ref anchorB);
				this.Length = (anchorB - anchorA).magnitude;
			}
			else
			{
				this.LocalAnchorA = anchorA;
				this.LocalAnchorB = anchorB;
				this.Length = (base.BodyB.GetWorldPoint(ref anchorB) - base.BodyA.GetWorldPoint(ref anchorA)).magnitude;
			}
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return invDt * this._impulse * this._u;
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return 0f;
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
			FP magnitude = this._u.magnitude;
			bool flag = magnitude > Settings.LinearSlop;
			if (flag)
			{
				this._u *= 1f / magnitude;
			}
			else
			{
				this._u = TSVector2.zero;
			}
			FP y = MathUtils.Cross(this._rA, this._u);
			FP y2 = MathUtils.Cross(this._rB, this._u);
			FP fP3 = this._invMassA + this._invIA * y * y + this._invMassB + this._invIB * y2 * y2;
			this._mass = ((fP3 != 0f) ? (1f / fP3) : 0f);
			bool flag2 = this.Frequency > 0f;
			if (flag2)
			{
				FP x = magnitude - this.Length;
				FP y3 = 2f * Settings.Pi * this.Frequency;
				FP x2 = 2f * this._mass * this.DampingRatio * y3;
				FP y4 = this._mass * y3 * y3;
				FP dt = data.step.dt;
				this._gamma = dt * (x2 + dt * y4);
				this._gamma = ((this._gamma != 0f) ? (1f / this._gamma) : 0f);
				this._bias = x * dt * y4 * this._gamma;
				fP3 += this._gamma;
				this._mass = ((fP3 != 0f) ? (1f / fP3) : 0f);
			}
			else
			{
				this._gamma = 0f;
				this._bias = 0f;
			}
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

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			TSVector2 value = tSVector + MathUtils.Cross(fP, this._rA);
			TSVector2 value2 = tSVector2 + MathUtils.Cross(fP2, this._rB);
			FP x = TSVector2.Dot(this._u, value2 - value);
			FP fP3 = -this._mass * (x + this._bias + this._gamma * this._impulse);
			this._impulse += fP3;
			TSVector2 tSVector3 = fP3 * this._u;
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
			bool flag = this.Frequency > 0f;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
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
				FP fP3 = magnitude - this.Length;
				fP3 = MathUtils.Clamp(fP3, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);
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
				result = (FP.Abs(fP3) < Settings.LinearSlop);
			}
			return result;
		}
	}
}
