using Microsoft.Xna.Framework;
using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class PrismaticJoint : Joint
	{
		private TSVector2 _localYAxisA;

		private Vector3 _impulse;

		private FP _lowerTranslation;

		private FP _upperTranslation;

		private FP _maxMotorForce;

		private FP _motorSpeed;

		private bool _enableLimit;

		private bool _enableMotor;

		private LimitState _limitState;

		private int _indexA;

		private int _indexB;

		private TSVector2 _localCenterA;

		private TSVector2 _localCenterB;

		private FP _invMassA;

		private FP _invMassB;

		private FP _invIA;

		private FP _invIB;

		private TSVector2 _axis;

		private TSVector2 _perp;

		private FP _s1;

		private FP _s2;

		private FP _a1;

		private FP _a2;

		private Mat33 _K;

		private FP _motorMass;

		private TSVector2 _axis1;

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

		public FP JointTranslation
		{
			get
			{
				TSVector2 value = base.BodyB.GetWorldPoint(this.LocalAnchorB) - base.BodyA.GetWorldPoint(this.LocalAnchorA);
				TSVector2 worldVector = base.BodyA.GetWorldVector(this.LocalXAxis);
				return TSVector2.Dot(value, worldVector);
			}
		}

		public FP JointSpeed
		{
			get
			{
				Transform transform;
				base.BodyA.GetTransform(out transform);
				Transform transform2;
				base.BodyB.GetTransform(out transform2);
				TSVector2 tSVector = MathUtils.Mul(ref transform.q, this.LocalAnchorA - base.BodyA.LocalCenter);
				TSVector2 tSVector2 = MathUtils.Mul(ref transform2.q, this.LocalAnchorB - base.BodyB.LocalCenter);
				TSVector2 value = base.BodyA._sweep.C + tSVector;
				TSVector2 value2 = base.BodyB._sweep.C + tSVector2;
				TSVector2 value3 = value2 - value;
				TSVector2 worldVector = base.BodyA.GetWorldVector(this.LocalXAxis);
				TSVector2 linearVelocity = base.BodyA._linearVelocity;
				TSVector2 linearVelocity2 = base.BodyB._linearVelocity;
				FP angularVelocity = base.BodyA._angularVelocity;
				FP angularVelocity2 = base.BodyB._angularVelocity;
				return TSVector2.Dot(value3, MathUtils.Cross(angularVelocity, worldVector)) + TSVector2.Dot(worldVector, linearVelocity2 + MathUtils.Cross(angularVelocity2, tSVector2) - linearVelocity - MathUtils.Cross(angularVelocity, tSVector));
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
				Debug.Assert(!base.BodyA.FixedRotation || !base.BodyB.FixedRotation, "Warning: limits does currently not work with fixed rotation");
				bool flag = value != this._enableLimit;
				if (flag)
				{
					base.WakeBodies();
					this._enableLimit = value;
					this._impulse.Z = 0;
				}
			}
		}

		public FP LowerLimit
		{
			get
			{
				return this._lowerTranslation;
			}
			set
			{
				bool flag = value != this._lowerTranslation;
				if (flag)
				{
					base.WakeBodies();
					this._lowerTranslation = value;
					this._impulse.Z = 0f;
				}
			}
		}

		public FP UpperLimit
		{
			get
			{
				return this._upperTranslation;
			}
			set
			{
				bool flag = value != this._upperTranslation;
				if (flag)
				{
					base.WakeBodies();
					this._upperTranslation = value;
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

		public FP MaxMotorForce
		{
			get
			{
				return this._maxMotorForce;
			}
			set
			{
				base.WakeBodies();
				this._maxMotorForce = value;
			}
		}

		public FP MotorImpulse
		{
			get;
			set;
		}

		public TSVector2 Axis
		{
			get
			{
				return this._axis1;
			}
			set
			{
				this._axis1 = value;
				this.LocalXAxis = base.BodyA.GetLocalVector(this._axis1);
				this.LocalXAxis.Normalize();
				this._localYAxisA = MathUtils.Cross(1f, this.LocalXAxis);
			}
		}

		public TSVector2 LocalXAxis
		{
			get;
			private set;
		}

		public FP ReferenceAngle
		{
			get;
			set;
		}

		internal PrismaticJoint()
		{
			base.JointType = JointType.Prismatic;
		}

		public PrismaticJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, TSVector2 axis, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			this.Initialize(anchorA, anchorB, axis, useWorldCoordinates);
		}

		public PrismaticJoint(Body bodyA, Body bodyB, TSVector2 anchor, TSVector2 axis, bool useWorldCoordinates = false) : base(bodyA, bodyB)
		{
			this.Initialize(anchor, anchor, axis, useWorldCoordinates);
		}

		private void Initialize(TSVector2 localAnchorA, TSVector2 localAnchorB, TSVector2 axis, bool useWorldCoordinates)
		{
			base.JointType = JointType.Prismatic;
			if (useWorldCoordinates)
			{
				this.LocalAnchorA = base.BodyA.GetLocalPoint(localAnchorA);
				this.LocalAnchorB = base.BodyB.GetLocalPoint(localAnchorB);
			}
			else
			{
				this.LocalAnchorA = localAnchorA;
				this.LocalAnchorB = localAnchorB;
			}
			this.Axis = axis;
			this.ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
			this._limitState = LimitState.Inactive;
		}

		public void SetLimits(FP lower, FP upper)
		{
			bool flag = upper != this._upperTranslation || lower != this._lowerTranslation;
			if (flag)
			{
				base.WakeBodies();
				this._upperTranslation = upper;
				this._lowerTranslation = lower;
				this._impulse.Z = 0f;
			}
		}

		public FP GetMotorForce(FP invDt)
		{
			return invDt * this.MotorImpulse;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			return invDt * (this._impulse.X * this._perp + (this.MotorImpulse + this._impulse.Z) * this._axis);
		}

		public override FP GetReactionTorque(FP invDt)
		{
			return invDt * this._impulse.Y;
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
			TSVector2 value = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			TSVector2 tSVector3 = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			TSVector2 tSVector4 = c2 - c + tSVector3 - value;
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			this._axis = MathUtils.Mul(q, this.LocalXAxis);
			this._a1 = MathUtils.Cross(tSVector4 + value, this._axis);
			this._a2 = MathUtils.Cross(tSVector3, this._axis);
			this._motorMass = invMassA + invMassB + invIA * this._a1 * this._a1 + invIB * this._a2 * this._a2;
			bool flag = this._motorMass > 0f;
			if (flag)
			{
				this._motorMass = 1f / this._motorMass;
			}
			this._perp = MathUtils.Mul(q, this._localYAxisA);
			this._s1 = MathUtils.Cross(tSVector4 + value, this._perp);
			this._s2 = MathUtils.Cross(tSVector3, this._perp);
			FP x = invMassA + invMassB + invIA * this._s1 * this._s1 + invIB * this._s2 * this._s2;
			FP fP3 = invIA * this._s1 + invIB * this._s2;
			FP fP4 = invIA * this._s1 * this._a1 + invIB * this._s2 * this._a2;
			FP fP5 = invIA + invIB;
			bool flag2 = fP5 == 0f;
			if (flag2)
			{
				fP5 = 1f;
			}
			FP fP6 = invIA * this._a1 + invIB * this._a2;
			FP z = invMassA + invMassB + invIA * this._a1 * this._a1 + invIB * this._a2 * this._a2;
			this._K.ex = new Vector3(x, fP3, fP4);
			this._K.ey = new Vector3(fP3, fP5, fP6);
			this._K.ez = new Vector3(fP4, fP6, z);
			bool enableLimit = this._enableLimit;
			if (enableLimit)
			{
				FP x2 = TSVector2.Dot(this._axis, tSVector4);
				bool flag3 = FP.Abs(this._upperTranslation - this._lowerTranslation) < 2f * Settings.LinearSlop;
				if (flag3)
				{
					this._limitState = LimitState.Equal;
				}
				else
				{
					bool flag4 = x2 <= this._lowerTranslation;
					if (flag4)
					{
						bool flag5 = this._limitState != LimitState.AtLower;
						if (flag5)
						{
							this._limitState = LimitState.AtLower;
							this._impulse.Z = 0f;
						}
					}
					else
					{
						bool flag6 = x2 >= this._upperTranslation;
						if (flag6)
						{
							bool flag7 = this._limitState != LimitState.AtUpper;
							if (flag7)
							{
								this._limitState = LimitState.AtUpper;
								this._impulse.Z = 0f;
							}
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
				this._impulse.Z = 0f;
			}
			bool flag8 = !this._enableMotor;
			if (flag8)
			{
				this.MotorImpulse = 0f;
			}
			this._impulse *= data.step.dtRatio;
			this.MotorImpulse *= data.step.dtRatio;
			TSVector2 value2 = this._impulse.X * this._perp + (this.MotorImpulse + this._impulse.Z) * this._axis;
			FP y = this._impulse.X * this._s1 + this._impulse.Y + (this.MotorImpulse + this._impulse.Z) * this._a1;
			FP y2 = this._impulse.X * this._s2 + this._impulse.Y + (this.MotorImpulse + this._impulse.Z) * this._a2;
			tSVector -= invMassA * value2;
			fP -= invIA * y;
			tSVector2 += invMassB * value2;
			fP2 += invIB * y2;
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
			bool flag = this._enableMotor && this._limitState != LimitState.Equal;
			if (flag)
			{
				FP y = TSVector2.Dot(this._axis, tSVector2 - tSVector) + this._a2 * fP2 - this._a1 * fP;
				FP fP3 = this._motorMass * (this._motorSpeed - y);
				FP motorImpulse = this.MotorImpulse;
				FP fP4 = data.step.dt * this._maxMotorForce;
				this.MotorImpulse = MathUtils.Clamp(this.MotorImpulse + fP3, -fP4, fP4);
				fP3 = this.MotorImpulse - motorImpulse;
				TSVector2 value = fP3 * this._axis;
				FP y2 = fP3 * this._a1;
				FP y3 = fP3 * this._a2;
				tSVector -= invMassA * value;
				fP -= invIA * y2;
				tSVector2 += invMassB * value;
				fP2 += invIB * y3;
			}
			TSVector2 tSVector3 = default(TSVector2);
			tSVector3.x = TSVector2.Dot(this._perp, tSVector2 - tSVector) + this._s2 * fP2 - this._s1 * fP;
			tSVector3.y = fP2 - fP;
			bool flag2 = this._enableLimit && this._limitState > LimitState.Inactive;
			if (flag2)
			{
				FP z = TSVector2.Dot(this._axis, tSVector2 - tSVector) + this._a2 * fP2 - this._a1 * fP;
				Vector3 value2 = new Vector3(tSVector3.x, tSVector3.y, z);
				Vector3 impulse = this._impulse;
				Vector3 vector = this._K.Solve33(-value2);
				this._impulse += vector;
				bool flag3 = this._limitState == LimitState.AtLower;
				if (flag3)
				{
					this._impulse.Z = TSMath.Max(this._impulse.Z, 0f);
				}
				else
				{
					bool flag4 = this._limitState == LimitState.AtUpper;
					if (flag4)
					{
						this._impulse.Z = TSMath.Min(this._impulse.Z, 0f);
					}
				}
				TSVector2 b = -tSVector3 - (this._impulse.Z - impulse.Z) * new TSVector2(this._K.ez.X, this._K.ez.Y);
				TSVector2 tSVector4 = this._K.Solve22(b) + new TSVector2(impulse.X, impulse.Y);
				this._impulse.X = tSVector4.x;
				this._impulse.Y = tSVector4.y;
				vector = this._impulse - impulse;
				TSVector2 value3 = vector.X * this._perp + vector.Z * this._axis;
				FP y4 = vector.X * this._s1 + vector.Y + vector.Z * this._a1;
				FP y5 = vector.X * this._s2 + vector.Y + vector.Z * this._a2;
				tSVector -= invMassA * value3;
				fP -= invIA * y4;
				tSVector2 += invMassB * value3;
				fP2 += invIB * y5;
			}
			else
			{
				TSVector2 tSVector5 = this._K.Solve22(-tSVector3);
				this._impulse.X = this._impulse.X + tSVector5.x;
				this._impulse.Y = this._impulse.Y + tSVector5.y;
				TSVector2 value4 = tSVector5.x * this._perp;
				FP y6 = tSVector5.x * this._s1 + tSVector5.y;
				FP y7 = tSVector5.x * this._s2 + tSVector5.y;
				tSVector -= invMassA * value4;
				fP -= invIA * y6;
				tSVector2 += invMassB * value4;
				fP2 += invIB * y7;
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
			FP invMassA = this._invMassA;
			FP invMassB = this._invMassB;
			FP invIA = this._invIA;
			FP invIB = this._invIB;
			TSVector2 value = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
			TSVector2 tSVector3 = MathUtils.Mul(q2, this.LocalAnchorB - this._localCenterB);
			TSVector2 tSVector4 = tSVector2 + tSVector3 - tSVector - value;
			TSVector2 tSVector5 = MathUtils.Mul(q, this.LocalXAxis);
			FP y = MathUtils.Cross(tSVector4 + value, tSVector5);
			FP y2 = MathUtils.Cross(tSVector3, tSVector5);
			TSVector2 tSVector6 = MathUtils.Mul(q, this._localYAxisA);
			FP y3 = MathUtils.Cross(tSVector4 + value, tSVector6);
			FP y4 = MathUtils.Cross(tSVector3, tSVector6);
			TSVector2 tSVector7 = default(TSVector2);
			tSVector7.x = TSVector2.Dot(tSVector6, tSVector4);
			tSVector7.y = fP2 - fP - this.ReferenceAngle;
			FP fP3 = FP.Abs(tSVector7.x);
			FP x = FP.Abs(tSVector7.y);
			bool flag = false;
			FP z = 0f;
			bool enableLimit = this._enableLimit;
			if (enableLimit)
			{
				FP fP4 = TSVector2.Dot(tSVector5, tSVector4);
				bool flag2 = FP.Abs(this._upperTranslation - this._lowerTranslation) < 2f * Settings.LinearSlop;
				if (flag2)
				{
					z = MathUtils.Clamp(fP4, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);
					fP3 = TSMath.Max(fP3, FP.Abs(fP4));
					flag = true;
				}
				else
				{
					bool flag3 = fP4 <= this._lowerTranslation;
					if (flag3)
					{
						z = MathUtils.Clamp(fP4 - this._lowerTranslation + Settings.LinearSlop, -Settings.MaxLinearCorrection, 0f);
						fP3 = TSMath.Max(fP3, this._lowerTranslation - fP4);
						flag = true;
					}
					else
					{
						bool flag4 = fP4 >= this._upperTranslation;
						if (flag4)
						{
							z = MathUtils.Clamp(fP4 - this._upperTranslation - Settings.LinearSlop, 0f, Settings.MaxLinearCorrection);
							fP3 = TSMath.Max(fP3, fP4 - this._upperTranslation);
							flag = true;
						}
					}
				}
			}
			bool flag5 = flag;
			Vector3 vector;
			if (flag5)
			{
				FP x2 = invMassA + invMassB + invIA * y3 * y3 + invIB * y4 * y4;
				FP fP5 = invIA * y3 + invIB * y4;
				FP fP6 = invIA * y3 * y + invIB * y4 * y2;
				FP fP7 = invIA + invIB;
				bool flag6 = fP7 == 0f;
				if (flag6)
				{
					fP7 = 1f;
				}
				FP fP8 = invIA * y + invIB * y2;
				FP z2 = invMassA + invMassB + invIA * y * y + invIB * y2 * y2;
				Mat33 mat = default(Mat33);
				mat.ex = new Vector3(x2, fP5, fP6);
				mat.ey = new Vector3(fP5, fP7, fP8);
				mat.ez = new Vector3(fP6, fP8, z2);
				vector = mat.Solve33(-new Vector3
				{
					X = tSVector7.x,
					Y = tSVector7.y,
					Z = z
				});
			}
			else
			{
				FP x3 = invMassA + invMassB + invIA * y3 * y3 + invIB * y4 * y4;
				FP fP9 = invIA * y3 + invIB * y4;
				FP fP10 = invIA + invIB;
				bool flag7 = fP10 == 0f;
				if (flag7)
				{
					fP10 = 1f;
				}
				Mat22 mat2 = default(Mat22);
				mat2.ex = new TSVector2(x3, fP9);
				mat2.ey = new TSVector2(fP9, fP10);
				TSVector2 tSVector8 = mat2.Solve(-tSVector7);
				vector = default(Vector3);
				vector.X = tSVector8.x;
				vector.Y = tSVector8.y;
				vector.Z = 0f;
			}
			TSVector2 value2 = vector.X * tSVector6 + vector.Z * tSVector5;
			FP y5 = vector.X * y3 + vector.Y + vector.Z * y;
			FP y6 = vector.X * y4 + vector.Y + vector.Z * y2;
			tSVector -= invMassA * value2;
			fP -= invIA * y5;
			tSVector2 += invMassB * value2;
			fP2 += invIB * y6;
			data.positions[this._indexA].c = tSVector;
			data.positions[this._indexA].a = fP;
			data.positions[this._indexB].c = tSVector2;
			data.positions[this._indexB].a = fP2;
			return fP3 <= Settings.LinearSlop && x <= Settings.AngularSlop;
		}
	}
}
