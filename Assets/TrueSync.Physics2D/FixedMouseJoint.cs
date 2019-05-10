using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class FixedMouseJoint : Joint
	{
		private TSVector2 _worldAnchor;

		private FP _frequency;

		private FP _dampingRatio;

		private FP _beta;

		private TSVector2 _impulse;

		private FP _maxForce;

		private FP _gamma;

		private int _indexA;

		private TSVector2 _rA;

		private TSVector2 _localCenterA;

		private FP _invMassA;

		private FP _invIA;

		private Mat22 _mass;

		private TSVector2 _C;

		public TSVector2 LocalAnchorA
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
				return this._worldAnchor;
			}
			set
			{
				base.WakeBodies();
				this._worldAnchor = value;
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

		public FP Frequency
		{
			get
			{
				return this._frequency;
			}
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0f);
				this._frequency = value;
			}
		}

		public FP DampingRatio
		{
			get
			{
				return this._dampingRatio;
			}
			set
			{
				Debug.Assert(MathUtils.IsValid(value) && value >= 0f);
				this._dampingRatio = value;
			}
		}

		public FixedMouseJoint(Body body, TSVector2 worldAnchor) : base(body)
		{
			base.JointType = JointType.FixedMouse;
			this.Frequency = 5f;
			this.DampingRatio = 0.7f;
			this.MaxForce = 1000 * body.Mass;
			Debug.Assert(worldAnchor.IsValid());
			this._worldAnchor = worldAnchor;
			this.LocalAnchorA = MathUtils.MulT(base.BodyA._xf, worldAnchor);
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return invDt * this._impulse;
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return invDt * 0f;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			this._indexA = base.BodyA.IslandIndex;
			this._localCenterA = base.BodyA._sweep.LocalCenter;
			this._invMassA = base.BodyA._invMass;
			this._invIA = base.BodyA._invI;
			TSVector2 c = data.positions[this._indexA].c;
			FP a = data.positions[this._indexA].a;
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			Rot q = new Rot(a);
			FP mass = base.BodyA.Mass;
			FP fP2 = 2f * Settings.Pi * this.Frequency;
			FP x = 2f * mass * this.DampingRatio * fP2;
			FP y = mass * (fP2 * fP2);
			FP dt = data.step.dt;
			this._gamma = dt * (x + dt * y);
			bool flag = this._gamma != 0f;
			if (flag)
			{
				this._gamma = 1f / this._gamma;
			}
			this._beta = dt * y * this._gamma;
			this._rA = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			Mat22 mat = default(Mat22);
			mat.ex.x = this._invMassA + this._invIA * this._rA.y * this._rA.y + this._gamma;
			mat.ex.y = -this._invIA * this._rA.x * this._rA.y;
			mat.ey.x = mat.ex.y;
			mat.ey.y = this._invMassA + this._invIA * this._rA.x * this._rA.x + this._gamma;
			this._mass = mat.Inverse;
			this._C = c + this._rA - this._worldAnchor;
			this._C *= this._beta;
			fP *= 0.98f;
			this._impulse *= data.step.dtRatio;
			tSVector += this._invMassA * this._impulse;
			fP += this._invIA * MathUtils.Cross(this._rA, this._impulse);
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 value = tSVector + MathUtils.Cross(fP, this._rA);
			TSVector2 tSVector2 = MathUtils.Mul(ref this._mass, -(value + this._C + this._gamma * this._impulse));
			TSVector2 impulse = this._impulse;
			this._impulse += tSVector2;
			FP fP2 = data.step.dt * this.MaxForce;
			bool flag = this._impulse.LengthSquared() > fP2 * fP2;
			if (flag)
			{
				this._impulse *= fP2 / this._impulse.magnitude;
			}
			tSVector2 = this._impulse - impulse;
			tSVector += this._invMassA * tSVector2;
			fP += this._invIA * MathUtils.Cross(this._rA, tSVector2);
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			return true;
		}
	}
}
