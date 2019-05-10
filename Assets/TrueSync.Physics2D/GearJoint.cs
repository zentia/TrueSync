using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class GearJoint : Joint
	{
		private JointType _typeA;

		private JointType _typeB;

		private Body _bodyA;

		private Body _bodyB;

		private Body _bodyC;

		private Body _bodyD;

		private TSVector2 _localAnchorA;

		private TSVector2 _localAnchorB;

		private TSVector2 _localAnchorC;

		private TSVector2 _localAnchorD;

		private TSVector2 _localAxisC;

		private TSVector2 _localAxisD;

		private FP _referenceAngleA;

		private FP _referenceAngleB;

		private FP _constant;

		private FP _ratio;

		private FP _impulse;

		private int _indexA;

		private int _indexB;

		private int _indexC;

		private int _indexD;

		private TSVector2 _lcA;

		private TSVector2 _lcB;

		private TSVector2 _lcC;

		private TSVector2 _lcD;

		private FP _mA;

		private FP _mB;

		private FP _mC;

		private FP _mD;

		private FP _iA;

		private FP _iB;

		private FP _iC;

		private FP _iD;

		private TSVector2 _JvAC;

		private TSVector2 _JvBD;

		private FP _JwA;

		private FP _JwB;

		private FP _JwC;

		private FP _JwD;

		private FP _mass;

		public override TSVector2 WorldAnchorA
		{
			get
			{
				return this._bodyA.GetWorldPoint(this._localAnchorA);
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
				return this._bodyB.GetWorldPoint(this._localAnchorB);
			}
			set
			{
				Debug.Assert(false, "You can't set the world anchor on this joint type.");
			}
		}

		public FP Ratio
		{
			get
			{
				return this._ratio;
			}
			set
			{
				Debug.Assert(MathUtils.IsValid(value));
				this._ratio = value;
			}
		}

		public Joint JointA
		{
			get;
			private set;
		}

		public Joint JointB
		{
			get;
			private set;
		}

		public GearJoint(Body bodyA, Body bodyB, Joint jointA, Joint jointB, FP ratio)
		{
			base.JointType = JointType.Gear;
			base.BodyA = bodyA;
			base.BodyB = bodyB;
			this.JointA = jointA;
			this.JointB = jointB;
			this.Ratio = ratio;
			this._typeA = jointA.JointType;
			this._typeB = jointB.JointType;
			Debug.Assert(this._typeA == JointType.Revolute || this._typeA == JointType.Prismatic || this._typeA == JointType.FixedRevolute || this._typeA == JointType.FixedPrismatic);
			Debug.Assert(this._typeB == JointType.Revolute || this._typeB == JointType.Prismatic || this._typeB == JointType.FixedRevolute || this._typeB == JointType.FixedPrismatic);
			this._bodyC = this.JointA.BodyA;
			this._bodyA = this.JointA.BodyB;
			Transform xf = this._bodyA._xf;
			FP a = this._bodyA._sweep.A;
			Transform xf2 = this._bodyC._xf;
			FP a2 = this._bodyC._sweep.A;
			bool flag = this._typeA == JointType.Revolute;
			FP x;
			if (flag)
			{
				RevoluteJoint revoluteJoint = (RevoluteJoint)jointA;
				this._localAnchorC = revoluteJoint.LocalAnchorA;
				this._localAnchorA = revoluteJoint.LocalAnchorB;
				this._referenceAngleA = revoluteJoint.ReferenceAngle;
				this._localAxisC = TSVector2.zero;
				x = a - a2 - this._referenceAngleA;
			}
			else
			{
				PrismaticJoint prismaticJoint = (PrismaticJoint)jointA;
				this._localAnchorC = prismaticJoint.LocalAnchorA;
				this._localAnchorA = prismaticJoint.LocalAnchorB;
				this._referenceAngleA = prismaticJoint.ReferenceAngle;
				this._localAxisC = prismaticJoint.LocalXAxis;
				TSVector2 localAnchorC = this._localAnchorC;
				TSVector2 value = MathUtils.MulT(xf2.q, MathUtils.Mul(xf.q, this._localAnchorA) + (xf.p - xf2.p));
				x = TSVector2.Dot(value - localAnchorC, this._localAxisC);
			}
			this._bodyD = this.JointB.BodyA;
			this._bodyB = this.JointB.BodyB;
			Transform xf3 = this._bodyB._xf;
			FP a3 = this._bodyB._sweep.A;
			Transform xf4 = this._bodyD._xf;
			FP a4 = this._bodyD._sweep.A;
			bool flag2 = this._typeB == JointType.Revolute;
			FP y;
			if (flag2)
			{
				RevoluteJoint revoluteJoint2 = (RevoluteJoint)jointB;
				this._localAnchorD = revoluteJoint2.LocalAnchorA;
				this._localAnchorB = revoluteJoint2.LocalAnchorB;
				this._referenceAngleB = revoluteJoint2.ReferenceAngle;
				this._localAxisD = TSVector2.zero;
				y = a3 - a4 - this._referenceAngleB;
			}
			else
			{
				PrismaticJoint prismaticJoint2 = (PrismaticJoint)jointB;
				this._localAnchorD = prismaticJoint2.LocalAnchorA;
				this._localAnchorB = prismaticJoint2.LocalAnchorB;
				this._referenceAngleB = prismaticJoint2.ReferenceAngle;
				this._localAxisD = prismaticJoint2.LocalXAxis;
				TSVector2 localAnchorD = this._localAnchorD;
				TSVector2 value2 = MathUtils.MulT(xf4.q, MathUtils.Mul(xf3.q, this._localAnchorB) + (xf3.p - xf4.p));
				y = TSVector2.Dot(value2 - localAnchorD, this._localAxisD);
			}
			this._ratio = ratio;
			this._constant = x + this._ratio * y;
			this._impulse = 0f;
		}

		public override TSVector2 GetReactionForce(FP invDt)
		{
			TSVector2 value = this._impulse * this._JvAC;
			return invDt * value;
		}

		public override FP GetReactionTorque(FP invDt)
		{
			FP y = this._impulse * this._JwA;
			return invDt * y;
		}

		internal override void InitVelocityConstraints(ref SolverData data)
		{
			this._indexA = this._bodyA.IslandIndex;
			this._indexB = this._bodyB.IslandIndex;
			this._indexC = this._bodyC.IslandIndex;
			this._indexD = this._bodyD.IslandIndex;
			this._lcA = this._bodyA._sweep.LocalCenter;
			this._lcB = this._bodyB._sweep.LocalCenter;
			this._lcC = this._bodyC._sweep.LocalCenter;
			this._lcD = this._bodyD._sweep.LocalCenter;
			this._mA = this._bodyA._invMass;
			this._mB = this._bodyB._invMass;
			this._mC = this._bodyC._invMass;
			this._mD = this._bodyD._invMass;
			this._iA = this._bodyA._invI;
			this._iB = this._bodyB._invI;
			this._iC = this._bodyC._invI;
			this._iD = this._bodyD._invI;
			FP a = data.positions[this._indexA].a;
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			FP a2 = data.positions[this._indexB].a;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			FP a3 = data.positions[this._indexC].a;
			TSVector2 tSVector3 = data.velocities[this._indexC].v;
			FP fP3 = data.velocities[this._indexC].w;
			FP a4 = data.positions[this._indexD].a;
			TSVector2 tSVector4 = data.velocities[this._indexD].v;
			FP fP4 = data.velocities[this._indexD].w;
			Rot q = new Rot(a);
			Rot q2 = new Rot(a2);
			Rot q3 = new Rot(a3);
			Rot q4 = new Rot(a4);
			this._mass = 0f;
			bool flag = this._typeA == JointType.Revolute;
			if (flag)
			{
				this._JvAC = TSVector2.zero;
				this._JwA = 1f;
				this._JwC = 1f;
				this._mass += this._iA + this._iC;
			}
			else
			{
				TSVector2 tSVector5 = MathUtils.Mul(q3, this._localAxisC);
				TSVector2 a5 = MathUtils.Mul(q3, this._localAnchorC - this._lcC);
				TSVector2 a6 = MathUtils.Mul(q, this._localAnchorA - this._lcA);
				this._JvAC = tSVector5;
				this._JwC = MathUtils.Cross(a5, tSVector5);
				this._JwA = MathUtils.Cross(a6, tSVector5);
				this._mass += this._mC + this._mA + this._iC * this._JwC * this._JwC + this._iA * this._JwA * this._JwA;
			}
			bool flag2 = this._typeB == JointType.Revolute;
			if (flag2)
			{
				this._JvBD = TSVector2.zero;
				this._JwB = this._ratio;
				this._JwD = this._ratio;
				this._mass += this._ratio * this._ratio * (this._iB + this._iD);
			}
			else
			{
				TSVector2 tSVector6 = MathUtils.Mul(q4, this._localAxisD);
				TSVector2 a7 = MathUtils.Mul(q4, this._localAnchorD - this._lcD);
				TSVector2 a8 = MathUtils.Mul(q2, this._localAnchorB - this._lcB);
				this._JvBD = this._ratio * tSVector6;
				this._JwD = this._ratio * MathUtils.Cross(a7, tSVector6);
				this._JwB = this._ratio * MathUtils.Cross(a8, tSVector6);
				this._mass += this._ratio * this._ratio * (this._mD + this._mB) + this._iD * this._JwD * this._JwD + this._iB * this._JwB * this._JwB;
			}
			this._mass = ((this._mass > 0f) ? (1f / this._mass) : 0f);
			tSVector += this._mA * this._impulse * this._JvAC;
			fP += this._iA * this._impulse * this._JwA;
			tSVector2 += this._mB * this._impulse * this._JvBD;
			fP2 += this._iB * this._impulse * this._JwB;
			tSVector3 -= this._mC * this._impulse * this._JvAC;
			fP3 -= this._iC * this._impulse * this._JwC;
			tSVector4 -= this._mD * this._impulse * this._JvBD;
			fP4 -= this._iD * this._impulse * this._JwD;
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
			data.velocities[this._indexC].v = tSVector3;
			data.velocities[this._indexC].w = fP3;
			data.velocities[this._indexD].v = tSVector4;
			data.velocities[this._indexD].w = fP4;
		}

		internal override void SolveVelocityConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.velocities[this._indexA].v;
			FP fP = data.velocities[this._indexA].w;
			TSVector2 tSVector2 = data.velocities[this._indexB].v;
			FP fP2 = data.velocities[this._indexB].w;
			TSVector2 tSVector3 = data.velocities[this._indexC].v;
			FP fP3 = data.velocities[this._indexC].w;
			TSVector2 tSVector4 = data.velocities[this._indexD].v;
			FP fP4 = data.velocities[this._indexD].w;
			FP fP5 = TSVector2.Dot(this._JvAC, tSVector - tSVector3) + TSVector2.Dot(this._JvBD, tSVector2 - tSVector4);
			fP5 += this._JwA * fP - this._JwC * fP3 + (this._JwB * fP2 - this._JwD * fP4);
			FP y = -this._mass * fP5;
			this._impulse += y;
			tSVector += this._mA * y * this._JvAC;
			fP += this._iA * y * this._JwA;
			tSVector2 += this._mB * y * this._JvBD;
			fP2 += this._iB * y * this._JwB;
			tSVector3 -= this._mC * y * this._JvAC;
			fP3 -= this._iC * y * this._JwC;
			tSVector4 -= this._mD * y * this._JvBD;
			fP4 -= this._iD * y * this._JwD;
			data.velocities[this._indexA].v = tSVector;
			data.velocities[this._indexA].w = fP;
			data.velocities[this._indexB].v = tSVector2;
			data.velocities[this._indexB].w = fP2;
			data.velocities[this._indexC].v = tSVector3;
			data.velocities[this._indexC].w = fP3;
			data.velocities[this._indexD].v = tSVector4;
			data.velocities[this._indexD].w = fP4;
		}

		internal override bool SolvePositionConstraints(ref SolverData data)
		{
			TSVector2 tSVector = data.positions[this._indexA].c;
			FP fP = data.positions[this._indexA].a;
			TSVector2 tSVector2 = data.positions[this._indexB].c;
			FP fP2 = data.positions[this._indexB].a;
			TSVector2 tSVector3 = data.positions[this._indexC].c;
			FP fP3 = data.positions[this._indexC].a;
			TSVector2 tSVector4 = data.positions[this._indexD].c;
			FP fP4 = data.positions[this._indexD].a;
			Rot q = new Rot(fP);
			Rot q2 = new Rot(fP2);
			Rot q3 = new Rot(fP3);
			Rot q4 = new Rot(fP4);
			FP x = 0f;
			FP fP5 = 0f;
			bool flag = this._typeA == JointType.Revolute;
			TSVector2 value;
			FP y;
			FP y2;
			FP x2;
			if (flag)
			{
				value = TSVector2.zero;
				y = 1f;
				y2 = 1f;
				fP5 += this._iA + this._iC;
				x2 = fP - fP3 - this._referenceAngleA;
			}
			else
			{
				TSVector2 tSVector5 = MathUtils.Mul(q3, this._localAxisC);
				TSVector2 a = MathUtils.Mul(q3, this._localAnchorC - this._lcC);
				TSVector2 tSVector6 = MathUtils.Mul(q, this._localAnchorA - this._lcA);
				value = tSVector5;
				y2 = MathUtils.Cross(a, tSVector5);
				y = MathUtils.Cross(tSVector6, tSVector5);
				fP5 += this._mC + this._mA + this._iC * y2 * y2 + this._iA * y * y;
				TSVector2 value2 = this._localAnchorC - this._lcC;
				TSVector2 value3 = MathUtils.MulT(q3, tSVector6 + (tSVector - tSVector3));
				x2 = TSVector2.Dot(value3 - value2, this._localAxisC);
			}
			bool flag2 = this._typeB == JointType.Revolute;
			TSVector2 value4;
			FP y3;
			FP y4;
			FP y5;
			if (flag2)
			{
				value4 = TSVector2.zero;
				y3 = this._ratio;
				y4 = this._ratio;
				fP5 += this._ratio * this._ratio * (this._iB + this._iD);
				y5 = fP2 - fP4 - this._referenceAngleB;
			}
			else
			{
				TSVector2 tSVector7 = MathUtils.Mul(q4, this._localAxisD);
				TSVector2 a2 = MathUtils.Mul(q4, this._localAnchorD - this._lcD);
				TSVector2 tSVector8 = MathUtils.Mul(q2, this._localAnchorB - this._lcB);
				value4 = this._ratio * tSVector7;
				y4 = this._ratio * MathUtils.Cross(a2, tSVector7);
				y3 = this._ratio * MathUtils.Cross(tSVector8, tSVector7);
				fP5 += this._ratio * this._ratio * (this._mD + this._mB) + this._iD * y4 * y4 + this._iB * y3 * y3;
				TSVector2 value5 = this._localAnchorD - this._lcD;
				TSVector2 value6 = MathUtils.MulT(q4, tSVector8 + (tSVector2 - tSVector4));
				y5 = TSVector2.Dot(value6 - value5, this._localAxisD);
			}
			FP x3 = x2 + this._ratio * y5 - this._constant;
			FP y6 = 0f;
			bool flag3 = fP5 > 0f;
			if (flag3)
			{
				y6 = -x3 / fP5;
			}
			tSVector += this._mA * y6 * value;
			fP += this._iA * y6 * y;
			tSVector2 += this._mB * y6 * value4;
			fP2 += this._iB * y6 * y3;
			tSVector3 -= this._mC * y6 * value;
			fP3 -= this._iC * y6 * y2;
			tSVector4 -= this._mD * y6 * value4;
			fP4 -= this._iD * y6 * y4;
			data.positions[this._indexA].c = tSVector;
			data.positions[this._indexA].a = fP;
			data.positions[this._indexB].c = tSVector2;
			data.positions[this._indexB].a = fP2;
			data.positions[this._indexC].c = tSVector3;
			data.positions[this._indexC].a = fP3;
			data.positions[this._indexD].c = tSVector4;
			data.positions[this._indexD].a = fP4;
			return x < Settings.LinearSlop;
		}
	}
}
