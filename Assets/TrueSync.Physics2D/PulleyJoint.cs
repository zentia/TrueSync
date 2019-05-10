using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class PulleyJoint : Joint
	{
		private FP _impulse;

		private int _indexA;

		private int _indexB;

		private TSVector2 _uA;

		private TSVector2 _uB;

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
			get;
			set;
		}

		public sealed override TSVector2 WorldAnchorB
		{
			get;
			set;
		}

		public FP LengthA
		{
			get;
			set;
		}

		public FP LengthB
		{
			get;
			set;
		}

		public FP CurrentLengthA
		{
			get
			{
				TSVector2 worldPoint = base.BodyA.GetWorldPoint(this.LocalAnchorA);
				TSVector2 worldAnchorA = this.WorldAnchorA;
				return (worldPoint - worldAnchorA).magnitude;
			}
		}

		public FP CurrentLengthB
		{
			get
			{
				TSVector2 worldPoint = base.BodyB.GetWorldPoint(this.LocalAnchorB);
				TSVector2 worldAnchorB = this.WorldAnchorB;
				return (worldPoint - worldAnchorB).magnitude;
			}
		}

		public FP Ratio
		{
			get;
			set;
		}

		internal FP Constant
		{
			get;
			set;
		}

		internal PulleyJoint()
		{
			base.JointType = JointType.Pulley;
		}

		public PulleyJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, TSVector2 worldAnchorA, TSVector2 worldAnchorB, FP ratio, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Pulley;
			this.WorldAnchorA = worldAnchorA;
			this.WorldAnchorB = worldAnchorB;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = base.BodyA.GetLocalPoint(anchorA);
				this.LocalAnchorB = base.BodyB.GetLocalPoint(anchorB);
				this.LengthA = (anchorA - worldAnchorA).magnitude;
				this.LengthB = (anchorB - worldAnchorB).magnitude;
			}
			else
			{
				this.LocalAnchorA = anchorA;
				this.LocalAnchorB = anchorB;
				this.LengthA = (anchorA - base.BodyA.GetLocalPoint(worldAnchorA)).magnitude;
				this.LengthB = (anchorB - base.BodyB.GetLocalPoint(worldAnchorB)).magnitude;
			}
			Debug.Assert(ratio != 0f);
			Debug.Assert(ratio > Settings.Epsilon);
			this.Ratio = ratio;
			this.Constant = this.LengthA + ratio * this.LengthB;
			this._impulse = 0f;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			TSVector2 value = this._impulse * this._uB;
			return invDt * value;
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
			this._uA = c + this._rA - this.WorldAnchorA;
			this._uB = c2 + this._rB - this.WorldAnchorB;
			FP magnitude = this._uA.magnitude;
			FP magnitude2 = this._uB.magnitude;
			bool flag = magnitude > 10f * Settings.LinearSlop;
			if (flag)
			{
				this._uA *= 1f / magnitude;
			}
			else
			{
				this._uA = TSVector2.zero;
			}
			bool flag2 = magnitude2 > 10f * Settings.LinearSlop;
			if (flag2)
			{
				this._uB *= 1f / magnitude2;
			}
			else
			{
				this._uB = TSVector2.zero;
			}
			FP y = MathUtils.Cross(this._rA, this._uA);
			FP y2 = MathUtils.Cross(this._rB, this._uB);
			FP x = this._invMassA + this._invIA * y * y;
			FP y3 = this._invMassB + this._invIB * y2 * y2;
			this._mass = x + this.Ratio * this.Ratio * y3;
			bool flag3 = this._mass > 0f;
			if (flag3)
			{
				this._mass = 1f / this._mass;
			}
			this._impulse *= data.step.dtRatio;
			TSVector2 tSVector3 = -this._impulse * this._uA;
			TSVector2 tSVector4 = -this.Ratio * this._impulse * this._uB;
			tSVector += this._invMassA * tSVector3;
			fP += this._invIA * MathUtils.Cross(this._rA, tSVector3);
			tSVector2 += this._invMassB * tSVector4;
			fP2 += this._invIB * MathUtils.Cross(this._rB, tSVector4);
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
			FP y = -TSVector2.Dot(this._uA, value) - this.Ratio * TSVector2.Dot(this._uB, value2);
			FP fP3 = -this._mass * y;
			this._impulse += fP3;
			TSVector2 tSVector3 = -fP3 * this._uA;
			TSVector2 tSVector4 = -this.Ratio * fP3 * this._uB;
			tSVector += this._invMassA * tSVector3;
			fP += this._invIA * MathUtils.Cross(this._rA, tSVector3);
			tSVector2 += this._invMassB * tSVector4;
			fP2 += this._invIB * MathUtils.Cross(this._rB, tSVector4);
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
			TSVector2 tSVector5 = tSVector + tSVector3 - this.WorldAnchorA;
			TSVector2 tSVector6 = tSVector2 + tSVector4 - this.WorldAnchorB;
			FP magnitude = tSVector5.magnitude;
			FP magnitude2 = tSVector6.magnitude;
			bool flag = magnitude > 10f * Settings.LinearSlop;
			if (flag)
			{
				tSVector5 *= 1f / magnitude;
			}
			else
			{
				tSVector5 = TSVector2.zero;
			}
			bool flag2 = magnitude2 > 10f * Settings.LinearSlop;
			if (flag2)
			{
				tSVector6 *= 1f / magnitude2;
			}
			else
			{
				tSVector6 = TSVector2.zero;
			}
			FP y = MathUtils.Cross(tSVector3, tSVector5);
			FP y2 = MathUtils.Cross(tSVector4, tSVector6);
			FP x = this._invMassA + this._invIA * y * y;
			FP y3 = this._invMassB + this._invIB * y2 * y2;
			FP fP3 = x + this.Ratio * this.Ratio * y3;
			bool flag3 = fP3 > 0f;
			if (flag3)
			{
				fP3 = 1f / fP3;
			}
			FP fP4 = this.Constant - magnitude - this.Ratio * magnitude2;
			FP x2 = FP.Abs(fP4);
			FP fP5 = -fP3 * fP4;
			TSVector2 tSVector7 = -fP5 * tSVector5;
			TSVector2 tSVector8 = -this.Ratio * fP5 * tSVector6;
			tSVector += this._invMassA * tSVector7;
			fP += this._invIA * MathUtils.Cross(tSVector3, tSVector7);
			tSVector2 += this._invMassB * tSVector8;
			fP2 += this._invIB * MathUtils.Cross(tSVector4, tSVector8);
			data.positions[this._indexA].c = tSVector;
			data.positions[this._indexA].a = fP;
			data.positions[this._indexB].c = tSVector2;
			data.positions[this._indexB].a = fP2;
			return x2 < Settings.LinearSlop;
		}
	}
}
