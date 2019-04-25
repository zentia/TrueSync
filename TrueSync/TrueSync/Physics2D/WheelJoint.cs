namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class WheelJoint : TrueSync.Physics2D.Joint
    {
        private TSVector2 _ax;
        private TSVector2 _axis;
        private TSVector2 _ay;
        private FP _bias;
        private bool _enableMotor;
        private FP _gamma;
        private FP _impulse;
        private int _indexA;
        private int _indexB;
        private FP _invIA;
        private FP _invIB;
        private FP _invMassA;
        private FP _invMassB;
        private TSVector2 _localCenterA;
        private TSVector2 _localCenterB;
        private TSVector2 _localYAxis;
        private FP _mass;
        private FP _maxMotorTorque;
        private FP _motorImpulse;
        private FP _motorMass;
        private FP _motorSpeed;
        private FP _sAx;
        private FP _sAy;
        private FP _sBx;
        private FP _sBy;
        private FP _springImpulse;
        private FP _springMass;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <DampingRatio>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Frequency>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalXAxis>k__BackingField;

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
            return (invDt * this._motorImpulse);
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            return (TSVector2) (invDt * ((this._impulse * this._ay) + (this._springImpulse * this._ax)));
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return (invDt * this._motorImpulse);
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
            FP fp = this._invMassA;
            FP fp2 = this._invMassB;
            FP fp3 = this._invIA;
            FP fp4 = this._invIB;
            TSVector2 c = data.positions[this._indexA].c;
            FP a = data.positions[this._indexA].a;
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector3 = data.positions[this._indexB].c;
            FP angle = data.positions[this._indexB].a;
            TSVector2 vector4 = data.velocities[this._indexB].v;
            FP fp8 = data.velocities[this._indexB].w;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            TSVector2 vector5 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            TSVector2 vector6 = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            TSVector2 vector7 = ((vector3 + vector6) - c) - vector5;
            this._ay = MathUtils.Mul(q, this._localYAxis);
            this._sAy = MathUtils.Cross(vector7 + vector5, this._ay);
            this._sBy = MathUtils.Cross(vector6, this._ay);
            this._mass = ((fp + fp2) + ((fp3 * this._sAy) * this._sAy)) + ((fp4 * this._sBy) * this._sBy);
            if (this._mass > 0f)
            {
                this._mass = 1f / this._mass;
            }
            this._springMass = 0f;
            this._bias = 0f;
            this._gamma = 0f;
            if (this.Frequency > 0f)
            {
                this._ax = MathUtils.Mul(q, this.LocalXAxis);
                this._sAx = MathUtils.Cross(vector7 + vector5, this._ax);
                this._sBx = MathUtils.Cross(vector6, this._ax);
                FP fp9 = ((fp + fp2) + ((fp3 * this._sAx) * this._sAx)) + ((fp4 * this._sBx) * this._sBx);
                if (fp9 > 0f)
                {
                    this._springMass = 1f / fp9;
                    FP fp10 = TSVector2.Dot(vector7, this._ax);
                    FP fp11 = (2f * Settings.Pi) * this.Frequency;
                    FP fp12 = ((2f * this._springMass) * this.DampingRatio) * fp11;
                    FP fp13 = (this._springMass * fp11) * fp11;
                    FP dt = data.step.dt;
                    this._gamma = dt * (fp12 + (dt * fp13));
                    if (this._gamma > 0f)
                    {
                        this._gamma = 1f / this._gamma;
                    }
                    this._bias = ((fp10 * dt) * fp13) * this._gamma;
                    this._springMass = fp9 + this._gamma;
                    if (this._springMass > 0f)
                    {
                        this._springMass = 1f / this._springMass;
                    }
                }
            }
            else
            {
                this._springImpulse = 0f;
            }
            if (this._enableMotor)
            {
                this._motorMass = fp3 + fp4;
                if (this._motorMass > 0f)
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
            TSVector2 vector8 = (TSVector2) ((this._impulse * this._ay) + (this._springImpulse * this._ax));
            FP fp15 = ((this._impulse * this._sAy) + (this._springImpulse * this._sAx)) + this._motorImpulse;
            FP fp16 = ((this._impulse * this._sBy) + (this._springImpulse * this._sBx)) + this._motorImpulse;
            v -= this._invMassA * vector8;
            w -= this._invIA * fp15;
            vector4 += this._invMassB * vector8;
            fp8 += this._invIB * fp16;
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector4;
            data.velocities[this._indexB].w = fp8;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            FP fp7;
            TSVector2 c = data.positions[this._indexA].c;
            FP a = data.positions[this._indexA].a;
            TSVector2 vector2 = data.positions[this._indexB].c;
            FP angle = data.positions[this._indexB].a;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            TSVector2 vector3 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            TSVector2 vector4 = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            TSVector2 vector5 = ((vector2 - c) + vector4) - vector3;
            TSVector2 b = MathUtils.Mul(q, this._localYAxis);
            FP fp3 = MathUtils.Cross(vector5 + vector3, b);
            FP fp4 = MathUtils.Cross(vector4, b);
            FP fp5 = TSVector2.Dot(vector5, b);
            FP fp6 = ((this._invMassA + this._invMassB) + ((this._invIA * this._sAy) * this._sAy)) + ((this._invIB * this._sBy) * this._sBy);
            if (fp6 != 0f)
            {
                fp7 = -fp5 / fp6;
            }
            else
            {
                fp7 = 0f;
            }
            TSVector2 vector7 = (TSVector2) (fp7 * b);
            FP fp8 = fp7 * fp3;
            FP fp9 = fp7 * fp4;
            c -= this._invMassA * vector7;
            a -= this._invIA * fp8;
            vector2 += this._invMassB * vector7;
            angle += this._invIB * fp9;
            data.positions[this._indexA].c = c;
            data.positions[this._indexA].a = a;
            data.positions[this._indexB].c = vector2;
            data.positions[this._indexB].a = angle;
            return (FP.Abs(fp5) <= Settings.LinearSlop);
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            FP fp = this._invMassA;
            FP fp2 = this._invMassB;
            FP fp3 = this._invIA;
            FP fp4 = this._invIB;
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP fp6 = data.velocities[this._indexB].w;
            FP fp7 = (TSVector2.Dot(this._ax, vector2 - v) + (this._sBx * fp6)) - (this._sAx * w);
            FP fp8 = -this._springMass * ((fp7 + this._bias) + (this._gamma * this._springImpulse));
            this._springImpulse += fp8;
            TSVector2 vector3 = (TSVector2) (fp8 * this._ax);
            FP fp9 = fp8 * this._sAx;
            FP fp10 = fp8 * this._sBx;
            v -= fp * vector3;
            w -= fp3 * fp9;
            vector2 += fp2 * vector3;
            fp6 += fp4 * fp10;
            FP fp11 = (fp6 - w) - this._motorSpeed;
            FP fp12 = -this._motorMass * fp11;
            FP fp13 = this._motorImpulse;
            FP high = data.step.dt * this._maxMotorTorque;
            this._motorImpulse = MathUtils.Clamp(this._motorImpulse + fp12, -high, high);
            fp12 = this._motorImpulse - fp13;
            w -= fp3 * fp12;
            fp6 += fp4 * fp12;
            FP fp15 = (TSVector2.Dot(this._ay, vector2 - v) + (this._sBy * fp6)) - (this._sAy * w);
            FP fp16 = -this._mass * fp15;
            this._impulse += fp16;
            TSVector2 vector4 = (TSVector2) (fp16 * this._ay);
            FP fp17 = fp16 * this._sAy;
            FP fp18 = fp16 * this._sBy;
            v -= fp * vector4;
            w -= fp3 * fp17;
            vector2 += fp2 * vector4;
            fp6 += fp4 * fp18;
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = fp6;
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

        public FP DampingRatio { get; set; }

        public FP Frequency { get; set; }

        public FP JointSpeed
        {
            get
            {
                FP angularVelocity = base.BodyA.AngularVelocity;
                return (base.BodyB.AngularVelocity - angularVelocity);
            }
        }

        public FP JointTranslation
        {
            get
            {
                Body bodyA = base.BodyA;
                Body bodyB = base.BodyB;
                TSVector2 worldPoint = bodyA.GetWorldPoint(this.LocalAnchorA);
                TSVector2 vector3 = bodyB.GetWorldPoint(this.LocalAnchorB) - worldPoint;
                TSVector2 worldVector = bodyA.GetWorldVector(this.LocalXAxis);
                return TSVector2.Dot(vector3, worldVector);
            }
        }

        public TSVector2 LocalAnchorA { get; set; }

        public TSVector2 LocalAnchorB { get; set; }

        public TSVector2 LocalXAxis { get; private set; }

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
    }
}

