using System;

namespace TrueSync.Physics2D
{
	public class WheelJoint : Joint
	{
		private TSVector2 _localYAxis;

		private FP _impulse;

		private FP _motorImpulse;

		private FP _springImpulse;

		private FP _maxMotorTorque;

		private FP _motorSpeed;

		private bool _enableMotor;

		private int _indexA;

		private int _indexB;

		private TSVector2 _localCenterA;

		private TSVector2 _localCenterB;

		private FP _invMassA;

		private FP _invMassB;

		private FP _invIA;

		private FP _invIB;

		private TSVector2 _ax;

		private TSVector2 _ay;

		private FP _sAx;

		private FP _sBx;

		private FP _sAy;

		private FP _sBy;

		private FP _mass;

		private FP _motorMass;

		private FP _springMass;

		private FP _bias;

		private FP _gamma;

		private TSVector2 _axis;

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

		public TSVector2 Axis
		{
			get
			{
				return this._axis;
			}
			set
			{
				this._axis = value;
				this.LocalXAxis = base.BodyA.GetLocalVector(this._axis);
				this._localYAxis = MathUtils.Cross(1f, this.LocalXAxis);
			}
		}

		public TSVector2 LocalXAxis
		{
			get;
			private set;
		}

		public FP MotorSpeed
		{
			get
			{
				return this._motorSpeed;
			}
			set
			{
				base.WakeBodies();
				this._motorSpeed = value;
			}
		}

		public FP MaxMotorTorque
		{
			get
			{
				return this._maxMotorTorque;
			}
			set
			{
				base.WakeBodies();
				this._maxMotorTorque = value;
			}
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

		public FP JointTranslation
		{
			get
			{
				Body bodyA = base.BodyA;
				Body bodyB = base.BodyB;
				TSVector2 worldPoint = bodyA.GetWorldPoint(this.LocalAnchorA);
				TSVector2 worldPoint2 = bodyB.GetWorldPoint(this.LocalAnchorB);
				TSVector2 value = worldPoint2 - worldPoint;
				TSVector2 worldVector = bodyA.GetWorldVector(this.LocalXAxis);
				return TSVector2.Dot(value, worldVector);
			}
		}

		public FP JointSpeed
		{
			get
			{
				FP angularVelocity = base.BodyA.AngularVelocity;
				FP angularVelocity2 = base.BodyB.AngularVelocity;
				return angularVelocity2 - angularVelocity;
			}
		}

		public bool MotorEnabled
		{
			get
			{
				return this._enableMotor;
			}
			set
			{
				base.WakeBodies();
				this._enableMotor = value;
			}
		}

		internal WheelJoint()
		{
			base.JointType = JointType.Wheel;
		}

		public WheelJoint(Body bodyA, Body bodyB, TSVector2 anchor, TSVector2 axis, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Wheel;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = bodyA.GetLocalPoint(anchor);
				this.LocalAnchorB = bodyB.GetLocalPoint(anchor);
			}
			else
			{
				this.LocalAnchorA = bodyA.GetLocalPoint(bodyB.GetWorldPoint(anchor));
				this.LocalAnchorB = anchor;
			}
			this.Axis = axis;
		}

		public FP GetMotorTorque(FP invDt)
		{
			return invDt * this._motorImpulse;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return invDt * (this._impulse * this._ay + this._springImpulse * this._ax);
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return invDt * this._motorImpulse;
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
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
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
			TSVector2 value = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			TSVector2 tSVector3 = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			TSVector2 value2 = c2 + tSVector3 - c - value;
			this._ay = MathUtils.Mul(q, this._localYAxis);
			this._sAy = MathUtils.Cross(value2 + value, this._ay);
			this._sBy = MathUtils.Cross(tSVector3, this._ay);
			this._mass = invMassA + invMassB + invIA * this._sAy * this._sAy + invIB * this._sBy * this._sBy;
			bool flag = this._mass > 0f;
			if (flag)
			{
				this._mass = 1f / this._mass;
			}
			this._springMass = 0f;
			this._bias = 0f;
			this._gamma = 0f;
			bool flag2 = this.Frequency > 0f;
			if (flag2)
			{
				this._ax = MathUtils.Mul(q, this.LocalXAxis);
				this._sAx = MathUtils.Cross(value2 + value, this._ax);
				this._sBx = MathUtils.Cross(tSVector3, this._ax);
				FP fP3 = invMassA + invMassB + invIA * this._sAx * this._sAx + invIB * this._sBx * this._sBx;
				bool flag3 = fP3 > 0f;
				if (flag3)
				{
					this._springMass = 1f / fP3;
					FP x = TSVector2.Dot(value2, this._ax);
					FP y = 2f * Settings.Pi * this.Frequency;
					FP x2 = 2f * this._springMass * this.DampingRatio * y;
					FP y2 = this._springMass * y * y;
					FP dt = data.step.dt;
					this._gamma = dt * (x2 + dt * y2);
					bool flag4 = this._gamma > 0f;
					if (flag4)
					{
						this._gamma = 1f / this._gamma;
					}
					this._bias = x * dt * y2 * this._gamma;
					this._springMass = fP3 + this._gamma;
					bool flag5 = this._springMass > 0f;
					if (flag5)
					{
						this._springMass = 1f / this._springMass;
					}
				}
			}
			else
			{
				this._springImpulse = 0f;
			}
			bool enableMotor = this._enableMotor;
			if (enableMotor)
			{
				this._motorMass = invIA + invIB;
				bool flag6 = this._motorMass > 0f;
				if (flag6)
				{
					this._motorMass = 1f / this._motorMass;
				}
			}
			else
			{
				this._motorMass = 0f;
				this._motorImpulse = 0f;
			}
			this._impulse *= data.step.dtRatio;
			this._springImpulse *= data.step.dtRatio;
			this._motorImpulse *= data.step.dtRatio;
			TSVector2 value3 = this._impulse * this._ay + this._springImpulse * this._ax;
			FP y3 = this._impulse * this._sAy + this._springImpulse * this._sAx + this._motorImpulse;
			FP y4 = this._impulse * this._sBy + this._springImpulse * this._sBx + this._motorImpulse;
			tSVector -= this._invMassA * value3;
			fP -= this._invIA * y3;
			tSVector2 += this._invMassB * value3;
			fP2 += this._invIB * y4;
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			FP x = TSVector2.Dot(this._ax, tSVector2 - tSVector) + this._sBx * fP2 - this._sAx * fP;
			FP fP3 = -this._springMass * (x + this._bias + this._gamma * this._springImpulse);
			this._springImpulse += fP3;
			TSVector2 value = fP3 * this._ax;
			FP y = fP3 * this._sAx;
			FP y2 = fP3 * this._sBx;
			tSVector -= invMassA * value;
			fP -= invIA * y;
			tSVector2 += invMassB * value;
			fP2 += invIB * y2;
			FP y3 = fP2 - fP - this._motorSpeed;
			FP y4 = -this._motorMass * y3;
			FP motorImpulse = this._motorImpulse;
			FP fP4 = data.step.dt * this._maxMotorTorque;
			this._motorImpulse = MathUtils.Clamp(this._motorImpulse + y4, -fP4, fP4);
			y4 = this._motorImpulse - motorImpulse;
			fP -= invIA * y4;
			fP2 += invIB * y4;
			FP y5 = TSVector2.Dot(this._ay, tSVector2 - tSVector) + this._sBy * fP2 - this._sAy * fP;
			FP fP5 = -this._mass * y5;
			this._impulse += fP5;
			TSVector2 value2 = fP5 * this._ay;
			FP y6 = fP5 * this._sAy;
			FP y7 = fP5 * this._sBy;
			tSVector -= invMassA * value2;
			fP -= invIA * y6;
			tSVector2 += invMassB * value2;
			fP2 += invIB * y7;
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
			TSVector2 value = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			TSVector2 tSVector3 = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			TSVector2 value2 = tSVector2 - tSVector + tSVector3 - value;
			TSVector2 tSVector4 = MathUtils.Mul(q, this._localYAxis);
			FP y = MathUtils.Cross(value2 + value, tSVector4);
			FP y2 = MathUtils.Cross(tSVector3, tSVector4);
			FP fP3 = TSVector2.Dot(value2, tSVector4);
			FP fP4 = this._invMassA + this._invMassB + this._invIA * this._sAy * this._sAy + this._invIB * this._sBy * this._sBy;
			bool flag = fP4 != 0f;
			FP fP5;
			if (flag)
			{
				fP5 = -fP3 / fP4;
			}
			else
			{
				fP5 = 0f;
			}
			TSVector2 value3 = fP5 * tSVector4;
			FP y3 = fP5 * y;
			FP y4 = fP5 * y2;
			tSVector -= this._invMassA * value3;
			fP -= this._invIA * y3;
			tSVector2 += this._invMassB * value3;
			fP2 += this._invIB * y4;
			data.positions[this._indexA].c = tSVector;
			data.positions[this._indexA].a = fP;
			data.positions[this._indexB].c = tSVector2;
			data.positions[this._indexB].a = fP2;
			return FP.Abs(fP3) <= Settings.LinearSlop;
		}
	}
}
