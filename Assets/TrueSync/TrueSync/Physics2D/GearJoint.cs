namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public class GearJoint : TrueSync.Physics2D.Joint
    {
        private Body _bodyA;
        private Body _bodyB;
        private Body _bodyC;
        private Body _bodyD;
        private FP _constant;
        private FP _iA;
        private FP _iB;
        private FP _iC;
        private FP _iD;
        private FP _impulse;
        private int _indexA;
        private int _indexB;
        private int _indexC;
        private int _indexD;
        private TSVector2 _JvAC;
        private TSVector2 _JvBD;
        private FP _JwA;
        private FP _JwB;
        private FP _JwC;
        private FP _JwD;
        private TSVector2 _lcA;
        private TSVector2 _lcB;
        private TSVector2 _lcC;
        private TSVector2 _lcD;
        private TSVector2 _localAnchorA;
        private TSVector2 _localAnchorB;
        private TSVector2 _localAnchorC;
        private TSVector2 _localAnchorD;
        private TSVector2 _localAxisC;
        private TSVector2 _localAxisD;
        private FP _mA;
        private FP _mass;
        private FP _mB;
        private FP _mC;
        private FP _mD;
        private FP _ratio;
        private FP _referenceAngleA;
        private FP _referenceAngleB;
        private JointType _typeA;
        private JointType _typeB;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.Joint <JointA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.Joint <JointB>k__BackingField;

        public GearJoint(Body bodyA, Body bodyB, TrueSync.Physics2D.Joint jointA, TrueSync.Physics2D.Joint jointB, FP ratio)
        {
            FP fp;
            FP fp2;
            base.JointType = JointType.Gear;
            base.BodyA = bodyA;
            base.BodyB = bodyB;
            this.JointA = jointA;
            this.JointB = jointB;
            this.Ratio = ratio;
            this._typeA = jointA.JointType;
            this._typeB = jointB.JointType;
            Debug.Assert((((this._typeA == JointType.Revolute) || (this._typeA == JointType.Prismatic)) || (this._typeA == JointType.FixedRevolute)) || (this._typeA == JointType.FixedPrismatic));
            Debug.Assert((((this._typeB == JointType.Revolute) || (this._typeB == JointType.Prismatic)) || (this._typeB == JointType.FixedRevolute)) || (this._typeB == JointType.FixedPrismatic));
            this._bodyC = this.JointA.BodyA;
            this._bodyA = this.JointA.BodyB;
            Transform transform = this._bodyA._xf;
            FP a = this._bodyA._sweep.A;
            Transform transform2 = this._bodyC._xf;
            FP fp4 = this._bodyC._sweep.A;
            if (this._typeA == JointType.Revolute)
            {
                RevoluteJoint joint = (RevoluteJoint) jointA;
                this._localAnchorC = joint.LocalAnchorA;
                this._localAnchorA = joint.LocalAnchorB;
                this._referenceAngleA = joint.ReferenceAngle;
                this._localAxisC = TSVector2.zero;
                fp = (a - fp4) - this._referenceAngleA;
            }
            else
            {
                TrueSync.Physics2D.PrismaticJoint joint2 = (TrueSync.Physics2D.PrismaticJoint) jointA;
                this._localAnchorC = joint2.LocalAnchorA;
                this._localAnchorA = joint2.LocalAnchorB;
                this._referenceAngleA = joint2.ReferenceAngle;
                this._localAxisC = joint2.LocalXAxis;
                TSVector2 vector = this._localAnchorC;
                fp = TSVector2.Dot(MathUtils.MulT(transform2.q, MathUtils.Mul(transform.q, this._localAnchorA) + (transform.p - transform2.p)) - vector, this._localAxisC);
            }
            this._bodyD = this.JointB.BodyA;
            this._bodyB = this.JointB.BodyB;
            Transform transform3 = this._bodyB._xf;
            FP fp5 = this._bodyB._sweep.A;
            Transform transform4 = this._bodyD._xf;
            FP fp6 = this._bodyD._sweep.A;
            if (this._typeB == JointType.Revolute)
            {
                RevoluteJoint joint3 = (RevoluteJoint) jointB;
                this._localAnchorD = joint3.LocalAnchorA;
                this._localAnchorB = joint3.LocalAnchorB;
                this._referenceAngleB = joint3.ReferenceAngle;
                this._localAxisD = TSVector2.zero;
                fp2 = (fp5 - fp6) - this._referenceAngleB;
            }
            else
            {
                TrueSync.Physics2D.PrismaticJoint joint4 = (TrueSync.Physics2D.PrismaticJoint) jointB;
                this._localAnchorD = joint4.LocalAnchorA;
                this._localAnchorB = joint4.LocalAnchorB;
                this._referenceAngleB = joint4.ReferenceAngle;
                this._localAxisD = joint4.LocalXAxis;
                TSVector2 vector3 = this._localAnchorD;
                fp2 = TSVector2.Dot(MathUtils.MulT(transform4.q, MathUtils.Mul(transform3.q, this._localAnchorB) + (transform3.p - transform4.p)) - vector3, this._localAxisD);
            }
            this._ratio = ratio;
            this._constant = fp + (this._ratio * fp2);
            this._impulse = 0f;
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            TSVector2 vector = (TSVector2) (this._impulse * this._JvAC);
            return (TSVector2) (invDt * vector);
        }

        public override FP GetReactionTorque(FP invDt)
        {
            FP fp = this._impulse * this._JwA;
            return (invDt * fp);
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
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            FP angle = data.positions[this._indexB].a;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP fp4 = data.velocities[this._indexB].w;
            FP fp5 = data.positions[this._indexC].a;
            TSVector2 vector3 = data.velocities[this._indexC].v;
            FP fp6 = data.velocities[this._indexC].w;
            FP fp7 = data.positions[this._indexD].a;
            TSVector2 vector4 = data.velocities[this._indexD].v;
            FP fp8 = data.velocities[this._indexD].w;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            Rot rot3 = new Rot(fp5);
            Rot rot4 = new Rot(fp7);
            this._mass = 0f;
            if (this._typeA == JointType.Revolute)
            {
                this._JvAC = TSVector2.zero;
                this._JwA = 1f;
                this._JwC = 1f;
                this._mass += this._iA + this._iC;
            }
            else
            {
                TSVector2 b = MathUtils.Mul(rot3, this._localAxisC);
                TSVector2 vector6 = MathUtils.Mul(rot3, this._localAnchorC - this._lcC);
                TSVector2 vector7 = MathUtils.Mul(q, this._localAnchorA - this._lcA);
                this._JvAC = b;
                this._JwC = MathUtils.Cross(vector6, b);
                this._JwA = MathUtils.Cross(vector7, b);
                this._mass += ((this._mC + this._mA) + ((this._iC * this._JwC) * this._JwC)) + ((this._iA * this._JwA) * this._JwA);
            }
            if (this._typeB == JointType.Revolute)
            {
                this._JvBD = TSVector2.zero;
                this._JwB = this._ratio;
                this._JwD = this._ratio;
                this._mass += (this._ratio * this._ratio) * (this._iB + this._iD);
            }
            else
            {
                TSVector2 vector8 = MathUtils.Mul(rot4, this._localAxisD);
                TSVector2 vector9 = MathUtils.Mul(rot4, this._localAnchorD - this._lcD);
                TSVector2 vector10 = MathUtils.Mul(rot2, this._localAnchorB - this._lcB);
                this._JvBD = (TSVector2) (this._ratio * vector8);
                this._JwD = this._ratio * MathUtils.Cross(vector9, vector8);
                this._JwB = this._ratio * MathUtils.Cross(vector10, vector8);
                this._mass += (((this._ratio * this._ratio) * (this._mD + this._mB)) + ((this._iD * this._JwD) * this._JwD)) + ((this._iB * this._JwB) * this._JwB);
            }
            this._mass = (this._mass > 0f) ? (1f / this._mass) : 0f;
            v += (this._mA * this._impulse) * this._JvAC;
            w += (this._iA * this._impulse) * this._JwA;
            vector2 += (this._mB * this._impulse) * this._JvBD;
            fp4 += (this._iB * this._impulse) * this._JwB;
            vector3 -= (this._mC * this._impulse) * this._JvAC;
            fp6 -= (this._iC * this._impulse) * this._JwC;
            vector4 -= (this._mD * this._impulse) * this._JvBD;
            fp8 -= (this._iD * this._impulse) * this._JwD;
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = fp4;
            data.velocities[this._indexC].v = vector3;
            data.velocities[this._indexC].w = fp6;
            data.velocities[this._indexD].v = vector4;
            data.velocities[this._indexD].w = fp8;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            FP fp6;
            FP fp7;
            TSVector2 zero;
            TSVector2 vector6;
            FP fp8;
            FP fp9;
            FP fp10;
            FP fp11;
            TSVector2 c = data.positions[this._indexA].c;
            FP a = data.positions[this._indexA].a;
            TSVector2 vector2 = data.positions[this._indexB].c;
            FP angle = data.positions[this._indexB].a;
            TSVector2 vector3 = data.positions[this._indexC].c;
            FP fp3 = data.positions[this._indexC].a;
            TSVector2 vector4 = data.positions[this._indexD].c;
            FP fp4 = data.positions[this._indexD].a;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            Rot rot3 = new Rot(fp3);
            Rot rot4 = new Rot(fp4);
            FP fp5 = 0f;
            FP fp12 = 0f;
            if (this._typeA == JointType.Revolute)
            {
                zero = TSVector2.zero;
                fp8 = 1f;
                fp10 = 1f;
                fp12 += this._iA + this._iC;
                fp6 = (a - fp3) - this._referenceAngleA;
            }
            else
            {
                TSVector2 b = MathUtils.Mul(rot3, this._localAxisC);
                TSVector2 vector8 = MathUtils.Mul(rot3, this._localAnchorC - this._lcC);
                TSVector2 vector9 = MathUtils.Mul(q, this._localAnchorA - this._lcA);
                zero = b;
                fp10 = MathUtils.Cross(vector8, b);
                fp8 = MathUtils.Cross(vector9, b);
                fp12 += ((this._mC + this._mA) + ((this._iC * fp10) * fp10)) + ((this._iA * fp8) * fp8);
                TSVector2 vector10 = this._localAnchorC - this._lcC;
                fp6 = TSVector2.Dot(MathUtils.MulT(rot3, vector9 + (c - vector3)) - vector10, this._localAxisC);
            }
            if (this._typeB == JointType.Revolute)
            {
                vector6 = TSVector2.zero;
                fp9 = this._ratio;
                fp11 = this._ratio;
                fp12 += (this._ratio * this._ratio) * (this._iB + this._iD);
                fp7 = (angle - fp4) - this._referenceAngleB;
            }
            else
            {
                TSVector2 vector12 = MathUtils.Mul(rot4, this._localAxisD);
                TSVector2 vector13 = MathUtils.Mul(rot4, this._localAnchorD - this._lcD);
                TSVector2 vector14 = MathUtils.Mul(rot2, this._localAnchorB - this._lcB);
                vector6 = (TSVector2) (this._ratio * vector12);
                fp11 = this._ratio * MathUtils.Cross(vector13, vector12);
                fp9 = this._ratio * MathUtils.Cross(vector14, vector12);
                fp12 += (((this._ratio * this._ratio) * (this._mD + this._mB)) + ((this._iD * fp11) * fp11)) + ((this._iB * fp9) * fp9);
                TSVector2 vector15 = this._localAnchorD - this._lcD;
                fp7 = TSVector2.Dot(MathUtils.MulT(rot4, vector14 + (vector2 - vector4)) - vector15, this._localAxisD);
            }
            FP fp13 = (fp6 + (this._ratio * fp7)) - this._constant;
            FP fp14 = 0f;
            if (fp12 > 0f)
            {
                fp14 = -fp13 / fp12;
            }
            c += (this._mA * fp14) * zero;
            a += (this._iA * fp14) * fp8;
            vector2 += (this._mB * fp14) * vector6;
            angle += (this._iB * fp14) * fp9;
            vector3 -= (this._mC * fp14) * zero;
            fp3 -= (this._iC * fp14) * fp10;
            vector4 -= (this._mD * fp14) * vector6;
            fp4 -= (this._iD * fp14) * fp11;
            data.positions[this._indexA].c = c;
            data.positions[this._indexA].a = a;
            data.positions[this._indexB].c = vector2;
            data.positions[this._indexB].a = angle;
            data.positions[this._indexC].c = vector3;
            data.positions[this._indexC].a = fp3;
            data.positions[this._indexD].c = vector4;
            data.positions[this._indexD].a = fp4;
            return (fp5 < Settings.LinearSlop);
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP fp2 = data.velocities[this._indexB].w;
            TSVector2 vector3 = data.velocities[this._indexC].v;
            FP fp3 = data.velocities[this._indexC].w;
            TSVector2 vector4 = data.velocities[this._indexD].v;
            FP fp4 = data.velocities[this._indexD].w;
            FP fp5 = TSVector2.Dot(this._JvAC, v - vector3) + TSVector2.Dot(this._JvBD, vector2 - vector4);
            fp5 += ((this._JwA * w) - (this._JwC * fp3)) + ((this._JwB * fp2) - (this._JwD * fp4));
            FP fp6 = -this._mass * fp5;
            this._impulse += fp6;
            v += (this._mA * fp6) * this._JvAC;
            w += (this._iA * fp6) * this._JwA;
            vector2 += (this._mB * fp6) * this._JvBD;
            fp2 += (this._iB * fp6) * this._JwB;
            vector3 -= (this._mC * fp6) * this._JvAC;
            fp3 -= (this._iC * fp6) * this._JwC;
            vector4 -= (this._mD * fp6) * this._JvBD;
            fp4 -= (this._iD * fp6) * this._JwD;
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = fp2;
            data.velocities[this._indexC].v = vector3;
            data.velocities[this._indexC].w = fp3;
            data.velocities[this._indexD].v = vector4;
            data.velocities[this._indexD].w = fp4;
        }

        public TrueSync.Physics2D.Joint JointA { get; private set; }

        public TrueSync.Physics2D.Joint JointB { get; private set; }

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
    }
}

