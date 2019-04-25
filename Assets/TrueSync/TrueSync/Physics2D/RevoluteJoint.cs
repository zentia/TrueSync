namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class RevoluteJoint : TrueSync.Physics2D.Joint
    {
        private bool _enableLimit;
        private bool _enableMotor;
        private Vector3 _impulse;
        private int _indexA;
        private int _indexB;
        private FP _invIA;
        private FP _invIB;
        private FP _invMassA;
        private FP _invMassB;
        private LimitState _limitState;
        private TSVector2 _localCenterA;
        private TSVector2 _localCenterB;
        private FP _lowerAngle;
        private Mat33 _mass;
        private FP _maxMotorTorque;
        private FP _motorImpulse;
        private FP _motorMass;
        private FP _motorSpeed;
        private TSVector2 _rA;
        private TSVector2 _rB;
        private FP _referenceAngle;
        private FP _upperAngle;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorB>k__BackingField;

        internal RevoluteJoint()
        {
            base.JointType = JointType.Revolute;
        }

        public RevoluteJoint(Body bodyA, Body bodyB, TSVector2 anchor, bool useWorldCoordinates = false) : this(bodyA, bodyB, anchor, anchor, useWorldCoordinates)
        {
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

        public FP GetMotorTorque(FP invDt)
        {
            return (invDt * this._motorImpulse);
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            TSVector2 vector = new TSVector2(this._impulse.X, this._impulse.Y);
            return (TSVector2) (invDt * vector);
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return (invDt * this._impulse.Z);
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
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            FP angle = data.positions[this._indexB].a;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP fp4 = data.velocities[this._indexB].w;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            this._rA = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            this._rB = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            FP fp5 = this._invMassA;
            FP fp6 = this._invMassB;
            FP fp7 = this._invIA;
            FP fp8 = this._invIB;
            bool flag = (fp7 + fp8) == 0f;
            this._mass.ex.X = ((fp5 + fp6) + ((this._rA.y * this._rA.y) * fp7)) + ((this._rB.y * this._rB.y) * fp8);
            this._mass.ey.X = ((-this._rA.y * this._rA.x) * fp7) - ((this._rB.y * this._rB.x) * fp8);
            this._mass.ez.X = (-this._rA.y * fp7) - (this._rB.y * fp8);
            this._mass.ex.Y = this._mass.ey.X;
            this._mass.ey.Y = ((fp5 + fp6) + ((this._rA.x * this._rA.x) * fp7)) + ((this._rB.x * this._rB.x) * fp8);
            this._mass.ez.Y = (this._rA.x * fp7) + (this._rB.x * fp8);
            this._mass.ex.Z = this._mass.ez.X;
            this._mass.ey.Z = this._mass.ez.Y;
            this._mass.ez.Z = fp7 + fp8;
            this._motorMass = fp7 + fp8;
            if (this._motorMass > 0f)
            {
                this._motorMass = 1f / this._motorMass;
            }
            if (!this._enableMotor | flag)
            {
                this._motorImpulse = 0f;
            }
            if (this._enableLimit && !flag)
            {
                FP fp9 = (angle - a) - this.ReferenceAngle;
                if (FP.Abs(this._upperAngle - this._lowerAngle) < (2f * Settings.AngularSlop))
                {
                    this._limitState = LimitState.Equal;
                }
                else if (fp9 <= this._lowerAngle)
                {
                    if (this._limitState != LimitState.AtLower)
                    {
                        this._impulse.Z = 0f;
                    }
                    this._limitState = LimitState.AtLower;
                }
                else if (fp9 >= this._upperAngle)
                {
                    if (this._limitState != LimitState.AtUpper)
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
            else
            {
                this._limitState = LimitState.Inactive;
            }
            this._impulse *= data.step.dtRatio;
            this._motorImpulse *= data.step.dtRatio;
            TSVector2 b = new TSVector2(this._impulse.X, this._impulse.Y);
            v -= fp5 * b;
            w -= fp7 * ((MathUtils.Cross(this._rA, b) + this.MotorImpulse) + this._impulse.Z);
            vector2 += fp6 * b;
            fp4 += fp8 * ((MathUtils.Cross(this._rB, b) + this.MotorImpulse) + this._impulse.Z);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = fp4;
        }

        public void SetLimits(FP lower, FP upper)
        {
            if ((lower != this._lowerAngle) || (upper != this._upperAngle))
            {
                base.WakeBodies();
                this._upperAngle = upper;
                this._lowerAngle = lower;
                this._impulse.Z = 0f;
            }
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            TSVector2 c = data.positions[this._indexA].c;
            FP a = data.positions[this._indexA].a;
            TSVector2 vector2 = data.positions[this._indexB].c;
            FP angle = data.positions[this._indexB].a;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            FP fp3 = 0f;
            bool flag = (this._invIA + this._invIB) == 0f;
            if ((this._enableLimit && (this._limitState != LimitState.Inactive)) && !flag)
            {
                FP fp5 = (angle - a) - this.ReferenceAngle;
                FP fp6 = 0f;
                if (this._limitState == LimitState.Equal)
                {
                    FP fp7 = MathUtils.Clamp(fp5 - this._lowerAngle, -Settings.MaxAngularCorrection, Settings.MaxAngularCorrection);
                    fp6 = -this._motorMass * fp7;
                    fp3 = FP.Abs(fp7);
                }
                else if (this._limitState == LimitState.AtLower)
                {
                    FP fp8 = fp5 - this._lowerAngle;
                    fp3 = -fp8;
                    fp8 = MathUtils.Clamp(fp8 + Settings.AngularSlop, -Settings.MaxAngularCorrection, 0f);
                    fp6 = -this._motorMass * fp8;
                }
                else if (this._limitState == LimitState.AtUpper)
                {
                    FP fp9 = fp5 - this._upperAngle;
                    fp3 = fp9;
                    fp9 = MathUtils.Clamp(fp9 - Settings.AngularSlop, 0f, Settings.MaxAngularCorrection);
                    fp6 = -this._motorMass * fp9;
                }
                a -= this._invIA * fp6;
                angle += this._invIB * fp6;
            }
            q.Set(a);
            rot2.Set(angle);
            TSVector2 vector3 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            TSVector2 vector4 = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            TSVector2 b = ((vector2 + vector4) - c) - vector3;
            FP magnitude = b.magnitude;
            FP fp10 = this._invMassA;
            FP fp11 = this._invMassB;
            FP fp12 = this._invIA;
            FP fp13 = this._invIB;
            Mat22 mat = new Mat22();
            mat.ex.x = ((fp10 + fp11) + ((fp12 * vector3.y) * vector3.y)) + ((fp13 * vector4.y) * vector4.y);
            mat.ex.y = ((-fp12 * vector3.x) * vector3.y) - ((fp13 * vector4.x) * vector4.y);
            mat.ey.x = mat.ex.y;
            mat.ey.y = ((fp10 + fp11) + ((fp12 * vector3.x) * vector3.x)) + ((fp13 * vector4.x) * vector4.x);
            TSVector2 vector6 = -mat.Solve(b);
            c -= fp10 * vector6;
            a -= fp12 * MathUtils.Cross(vector3, vector6);
            vector2 += fp11 * vector6;
            angle += fp13 * MathUtils.Cross(vector4, vector6);
            data.positions[this._indexA].c = c;
            data.positions[this._indexA].a = a;
            data.positions[this._indexB].c = vector2;
            data.positions[this._indexB].a = angle;
            return ((magnitude <= Settings.LinearSlop) && (fp3 <= Settings.AngularSlop));
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP s = data.velocities[this._indexB].w;
            FP fp3 = this._invMassA;
            FP fp4 = this._invMassB;
            FP fp5 = this._invIA;
            FP fp6 = this._invIB;
            bool flag = (fp5 + fp6) == 0f;
            if ((this._enableMotor && (this._limitState != LimitState.Equal)) && !flag)
            {
                FP fp7 = (s - w) - this._motorSpeed;
                FP fp8 = this._motorMass * -fp7;
                FP fp9 = this._motorImpulse;
                FP high = data.step.dt * this._maxMotorTorque;
                this._motorImpulse = MathUtils.Clamp(this._motorImpulse + fp8, -high, high);
                fp8 = this._motorImpulse - fp9;
                w -= fp5 * fp8;
                s += fp6 * fp8;
            }
            if ((this._enableLimit && (this._limitState != LimitState.Inactive)) && !flag)
            {
                TSVector2 vector3 = ((vector2 + MathUtils.Cross(s, this._rB)) - v) - MathUtils.Cross(w, this._rA);
                FP z = s - w;
                Vector3 b = new Vector3(vector3.x, vector3.y, z);
                Vector3 vector5 = -this._mass.Solve33(b);
                if (this._limitState == LimitState.Equal)
                {
                    this._impulse += vector5;
                }
                else if (this._limitState == LimitState.AtLower)
                {
                    FP fp12 = this._impulse.Z + vector5.Z;
                    if (fp12 < 0f)
                    {
                        TSVector2 vector7 = -vector3 + (this._impulse.Z * new TSVector2(this._mass.ez.X, this._mass.ez.Y));
                        TSVector2 vector8 = this._mass.Solve22(vector7);
                        vector5.X = vector8.x;
                        vector5.Y = vector8.y;
                        vector5.Z = -this._impulse.Z;
                        this._impulse.X += vector8.x;
                        this._impulse.Y += vector8.y;
                        this._impulse.Z = 0f;
                    }
                    else
                    {
                        this._impulse += vector5;
                    }
                }
                else if (this._limitState == LimitState.AtUpper)
                {
                    FP fp13 = this._impulse.Z + vector5.Z;
                    if (fp13 > 0f)
                    {
                        TSVector2 vector9 = -vector3 + (this._impulse.Z * new TSVector2(this._mass.ez.X, this._mass.ez.Y));
                        TSVector2 vector10 = this._mass.Solve22(vector9);
                        vector5.X = vector10.x;
                        vector5.Y = vector10.y;
                        vector5.Z = -this._impulse.Z;
                        this._impulse.X += vector10.x;
                        this._impulse.Y += vector10.y;
                        this._impulse.Z = 0f;
                    }
                    else
                    {
                        this._impulse += vector5;
                    }
                }
                TSVector2 vector6 = new TSVector2(vector5.X, vector5.Y);
                v -= fp3 * vector6;
                w -= fp5 * (MathUtils.Cross(this._rA, vector6) + vector5.Z);
                vector2 += fp4 * vector6;
                s += fp6 * (MathUtils.Cross(this._rB, vector6) + vector5.Z);
            }
            else
            {
                TSVector2 vector11 = ((vector2 + MathUtils.Cross(s, this._rB)) - v) - MathUtils.Cross(w, this._rA);
                TSVector2 vector12 = this._mass.Solve22(-vector11);
                this._impulse.X += vector12.x;
                this._impulse.Y += vector12.y;
                v -= fp3 * vector12;
                w -= fp5 * MathUtils.Cross(this._rA, vector12);
                vector2 += fp4 * vector12;
                s += fp6 * MathUtils.Cross(this._rB, vector12);
            }
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = s;
        }

        public FP JointAngle
        {
            get
            {
                return ((base.BodyB._sweep.A - base.BodyA._sweep.A) - this.ReferenceAngle);
            }
        }

        public FP JointSpeed
        {
            get
            {
                return (base.BodyB._angularVelocity - base.BodyA._angularVelocity);
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
                if (this._enableLimit != value)
                {
                    base.WakeBodies();
                    this._enableLimit = value;
                    this._impulse.Z = 0f;
                }
            }
        }

        public TSVector2 LocalAnchorA { get; set; }

        public TSVector2 LocalAnchorB { get; set; }

        public FP LowerLimit
        {
            get
            {
                return this._lowerAngle;
            }
            set
            {
                if (this._lowerAngle != value)
                {
                    base.WakeBodies();
                    this._lowerAngle = value;
                    this._impulse.Z = 0f;
                }
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

        public FP UpperLimit
        {
            get
            {
                return this._upperAngle;
            }
            set
            {
                if (this._upperAngle != value)
                {
                    base.WakeBodies();
                    this._upperAngle = value;
                    this._impulse.Z = 0f;
                }
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

