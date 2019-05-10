using Microsoft.Xna.Framework;
using System;

namespace TrueSync.Physics2D
{
	public class RevoluteJoint : Joint
	{
		private Vector3 _impulse;

		private FP _motorImpulse;

		private bool _enableMotor;

		private FP _maxMotorTorque;

		private FP _motorSpeed;

		private bool _enableLimit;

		private FP _referenceAngle;

		private FP _lowerAngle;

		private FP _upperAngle;

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

		private Mat33 _mass;

		private FP _motorMass;

		private LimitState _limitState;

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

		public FP ReferenceAngle
		{
			get
			{
				return this._referenceAngle;
			}
			set
			{
				base.WakeBodies();
				this._referenceAngle = value;
			}
		}

		public FP JointAngle
		{
			get
			{
				return base.BodyB._sweep.A - base.BodyA._sweep.A - this.ReferenceAngle;
			}
		}

		public FP JointSpeed
		{
			get
			{
				return base.BodyB._angularVelocity - base.BodyA._angularVelocity;
			}
		}

		public bool LimitEnabled
		{
			get
			{
				return this._enableLimit;
			}
			set
			{
				bool flag = this._enableLimit != value;
				if (flag)
				{
					base.WakeBodies();
					this._enableLimit = value;
					this._impulse.Z = 0f;
				}
			}
		}

		public FP LowerLimit
		{
			get
			{
				return this._lowerAngle;
			}
			set
			{
				bool flag = this._lowerAngle != value;
				if (flag)
				{
					base.WakeBodies();
					this._lowerAngle = value;
					this._impulse.Z = 0f;
				}
			}
		}

		public FP UpperLimit
		{
			get
			{
				return this._upperAngle;
			}
			set
			{
				bool flag = this._upperAngle != value;
				if (flag)
				{
					base.WakeBodies();
					this._upperAngle = value;
					this._impulse.Z = 0f;
				}
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

		public FP MotorImpulse
		{
			get
			{
				return this._motorImpulse;
			}
			set
			{
				base.WakeBodies();
				this._motorImpulse = value;
			}
		}

		internal RevoluteJoint()
		{
			base.JointType = JointType.Revolute;
		}

		public RevoluteJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			base.JointType = JointType.Revolute;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = base.BodyA.GetLocalPoint(anchorA);
				this.LocalAnchorB = base.BodyB.GetLocalPoint(anchorB);
			}
			else
			{
				this.LocalAnchorA = anchorA;
				this.LocalAnchorB = anchorB;
			}
			this.ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
			this._impulse = Vector3.Zero;
			this._limitState = LimitState.Inactive;
		}

		public RevoluteJoint(Body bodyA, Body bodyB, TSVector2 anchor, bool useWorldCoordinates = false) : this(bodyA, bodyB, anchor, anchor, useWorldCoordinates)
		{
		}

		public void SetLimits(FP lower, FP upper)
		{
			bool flag = lower != this._lowerAngle || upper != this._upperAngle;
			if (flag)
			{
				base.WakeBodies();
				this._upperAngle = upper;
				this._lowerAngle = lower;
				this._impulse.Z = 0f;
			}
		}

		public FP GetMotorTorque(FP invDt)
		{
			return invDt * this._motorImpulse;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			TSVector2 value = new TSVector2(this._impulse.X, this._impulse.Y);
			return invDt * value;
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return invDt * this._impulse.Z;
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
			bool flag = invIA + invIB == 0f;
			this._mass.ex.X = invMassA + invMassB + this._rA.y * this._rA.y * invIA + this._rB.y * this._rB.y * invIB;
			this._mass.ey.X = -this._rA.y * this._rA.x * invIA - this._rB.y * this._rB.x * invIB;
			this._mass.ez.X = -this._rA.y * invIA - this._rB.y * invIB;
			this._mass.ex.Y = this._mass.ey.X;
			this._mass.ey.Y = invMassA + invMassB + this._rA.x * this._rA.x * invIA + this._rB.x * this._rB.x * invIB;
			this._mass.ez.Y = this._rA.x * invIA + this._rB.x * invIB;
			this._mass.ex.Z = this._mass.ez.X;
			this._mass.ey.Z = this._mass.ez.Y;
			this._mass.ez.Z = invIA + invIB;
			this._motorMass = invIA + invIB;
			bool flag2 = this._motorMass > 0f;
			if (flag2)
			{
				this._motorMass = 1f / this._motorMass;
			}
			bool flag3 = !this._enableMotor | flag;
			if (flag3)
			{
				this._motorImpulse = 0f;
			}
			bool flag4 = this._enableLimit && !flag;
			if (flag4)
			{
				FP x = a2 - a - this.ReferenceAngle;
				bool flag5 = FP.Abs(this._upperAngle - this._lowerAngle) < 2f * Settings.AngularSlop;
				if (flag5)
				{
					this._limitState = LimitState.Equal;
				}
				else
				{
					bool flag6 = x <= this._lowerAngle;
					if (flag6)
					{
						bool flag7 = this._limitState != LimitState.AtLower;
						if (flag7)
						{
							this._impulse.Z = 0f;
						}
						this._limitState = LimitState.AtLower;
					}
					else
					{
						bool flag8 = x >= this._upperAngle;
						if (flag8)
						{
							bool flag9 = this._limitState != LimitState.AtUpper;
							if (flag9)
							{
								this._impulse.Z = 0f;
							}
							this._limitState = LimitState.AtUpper;
						}
						else
						{
							this._limitState = LimitState.Inactive;
							this._impulse.Z = 0f;
						}
					}
				}
			}
			else
			{
				this._limitState = LimitState.Inactive;
			}
			this._impulse *= data.step.dtRatio;
			this._motorImpulse *= data.step.dtRatio;
			TSVector2 tSVector3 = new TSVector2(this._impulse.X, this._impulse.Y);
			tSVector -= invMassA * tSVector3;
			fP -= invIA * (MathUtils.Cross(this._rA, tSVector3) + this.MotorImpulse + this._impulse.Z);
			tSVector2 += invMassB * tSVector3;
			fP2 += invIB * (MathUtils.Cross(this._rB, tSVector3) + this.MotorImpulse + this._impulse.Z);
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
			bool flag = invIA + invIB == 0f;
			bool flag2 = this._enableMotor && this._limitState != LimitState.Equal && !flag;
			if (flag2)
			{
				FP x = fP2 - fP - this._motorSpeed;
				FP y = this._motorMass * -x;
				FP motorImpulse = this._motorImpulse;
				FP fP3 = data.step.dt * this._maxMotorTorque;
				this._motorImpulse = MathUtils.Clamp(this._motorImpulse + y, -fP3, fP3);
				y = this._motorImpulse - motorImpulse;
				fP -= invIA * y;
				fP2 += invIB * y;
			}
			bool flag3 = this._enableLimit && this._limitState != LimitState.Inactive && !flag;
			if (flag3)
			{
				TSVector2 tSVector3 = tSVector2 + MathUtils.Cross(fP2, this._rB) - tSVector - MathUtils.Cross(fP, this._rA);
				FP z = fP2 - fP;
				Vector3 b = new Vector3(tSVector3.x, tSVector3.y, z);
				Vector3 vector = -this._mass.Solve33(b);
				bool flag4 = this._limitState == LimitState.Equal;
				if (flag4)
				{
					this._impulse += vector;
				}
				else
				{
					bool flag5 = this._limitState == LimitState.AtLower;
					if (flag5)
					{
						FP x2 = this._impulse.Z + vector.Z;
						bool flag6 = x2 < 0f;
						if (flag6)
						{
							TSVector2 b2 = -tSVector3 + this._impulse.Z * new TSVector2(this._mass.ez.X, this._mass.ez.Y);
							TSVector2 tSVector4 = this._mass.Solve22(b2);
							vector.X = tSVector4.x;
							vector.Y = tSVector4.y;
							vector.Z = -this._impulse.Z;
							this._impulse.X = this._impulse.X + tSVector4.x;
							this._impulse.Y = this._impulse.Y + tSVector4.y;
							this._impulse.Z = 0f;
						}
						else
						{
							this._impulse += vector;
						}
					}
					else
					{
						bool flag7 = this._limitState == LimitState.AtUpper;
						if (flag7)
						{
							FP x3 = this._impulse.Z + vector.Z;
							bool flag8 = x3 > 0f;
							if (flag8)
							{
								TSVector2 b3 = -tSVector3 + this._impulse.Z * new TSVector2(this._mass.ez.X, this._mass.ez.Y);
								TSVector2 tSVector5 = this._mass.Solve22(b3);
								vector.X = tSVector5.x;
								vector.Y = tSVector5.y;
								vector.Z = -this._impulse.Z;
								this._impulse.X = this._impulse.X + tSVector5.x;
								this._impulse.Y = this._impulse.Y + tSVector5.y;
								this._impulse.Z = 0f;
							}
							else
							{
								this._impulse += vector;
							}
						}
					}
				}
				TSVector2 tSVector6 = new TSVector2(vector.X, vector.Y);
				tSVector -= invMassA * tSVector6;
				fP -= invIA * (MathUtils.Cross(this._rA, tSVector6) + vector.Z);
				tSVector2 += invMassB * tSVector6;
				fP2 += invIB * (MathUtils.Cross(this._rB, tSVector6) + vector.Z);
			}
			else
			{
				TSVector2 value = tSVector2 + MathUtils.Cross(fP2, this._rB) - tSVector - MathUtils.Cross(fP, this._rA);
				TSVector2 tSVector7 = this._mass.Solve22(-value);
				this._impulse.X = this._impulse.X + tSVector7.x;
				this._impulse.Y = this._impulse.Y + tSVector7.y;
				tSVector -= invMassA * tSVector7;
				fP -= invIA * MathUtils.Cross(this._rA, tSVector7);
				tSVector2 += invMassB * tSVector7;
				fP2 += invIB * MathUtils.Cross(this._rB, tSVector7);
			}
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
			FP x = 0f;
			bool flag = this._invIA + this._invIB == 0f;
			bool flag2 = this._enableLimit && this._limitState != LimitState.Inactive && !flag;
			if (flag2)
			{
				FP x2 = fP2 - fP - this.ReferenceAngle;
				FP y = 0f;
				bool flag3 = this._limitState == LimitState.Equal;
				if (flag3)
				{
					FP fP3 = MathUtils.Clamp(x2 - this._lowerAngle, -Settings.MaxAngularCorrection, Settings.MaxAngularCorrection);
					y = -this._motorMass * fP3;
					x = FP.Abs(fP3);
				}
				else
				{
					bool flag4 = this._limitState == LimitState.AtLower;
					if (flag4)
					{
						FP fP4 = x2 - this._lowerAngle;
						x = -fP4;
						fP4 = MathUtils.Clamp(fP4 + Settings.AngularSlop, -Settings.MaxAngularCorrection, 0f);
						y = -this._motorMass * fP4;
					}
					else
					{
						bool flag5 = this._limitState == LimitState.AtUpper;
						if (flag5)
						{
							FP fP5 = x2 - this._upperAngle;
							x = fP5;
							fP5 = MathUtils.Clamp(fP5 - Settings.AngularSlop, 0f, Settings.MaxAngularCorrection);
							y = -this._motorMass * fP5;
						}
					}
				}
				fP -= this._invIA * y;
				fP2 += this._invIB * y;
			}
			q.Set(fP);
			q2.Set(fP2);
			TSVector2 tSVector3 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			TSVector2 tSVector4 = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			TSVector2 b = tSVector2 + tSVector4 - tSVector - tSVector3;
			FP magnitude = b.magnitude;
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			Mat22 mat = default(Mat22);
			mat.ex.x = invMassA + invMassB + invIA * tSVector3.y * tSVector3.y + invIB * tSVector4.y * tSVector4.y;
			mat.ex.y = -invIA * tSVector3.x * tSVector3.y - invIB * tSVector4.x * tSVector4.y;
			mat.ey.x = mat.ex.y;
			mat.ey.y = invMassA + invMassB + invIA * tSVector3.x * tSVector3.x + invIB * tSVector4.x * tSVector4.x;
			TSVector2 tSVector5 = -mat.Solve(b);
			tSVector -= invMassA * tSVector5;
			fP -= invIA * MathUtils.Cross(tSVector3, tSVector5);
			tSVector2 += invMassB * tSVector5;
			fP2 += invIB * MathUtils.Cross(tSVector4, tSVector5);
			data.positions[this._indexA].c = tSVector;
			data.positions[this._indexA].a = fP;
			data.positions[this._indexB].c = tSVector2;
			data.positions[this._indexB].a = fP2;
			return magnitude <= Settings.LinearSlop && x <= Settings.AngularSlop;
		}
	}
}
