namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class PrismaticJoint : TrueSync.Physics2D.Joint
    {
        private FP _a1;
        private FP _a2;
        private TSVector2 _axis;
        private TSVector2 _axis1;
        private bool _enableLimit;
        private bool _enableMotor;
        private Vector3 _impulse;
        private int _indexA;
        private int _indexB;
        private FP _invIA;
        private FP _invIB;
        private FP _invMassA;
        private FP _invMassB;
        private Mat33 _K;
        private LimitState _limitState;
        private TSVector2 _localCenterA;
        private TSVector2 _localCenterB;
        private TSVector2 _localYAxisA;
        private FP _lowerTranslation;
        private FP _maxMotorForce;
        private FP _motorMass;
        private FP _motorSpeed;
        private TSVector2 _perp;
        private FP _s1;
        private FP _s2;
        private FP _upperTranslation;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalXAxis>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <MotorImpulse>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <ReferenceAngle>k__BackingField;

        internal PrismaticJoint()
        {
            base.JointType = JointType.Prismatic;
        }

        public PrismaticJoint(Body bodyA, Body bodyB, TSVector2 anchor, TSVector2 axis, bool useWorldCoordinates = false) : base(bodyA, bodyB)
        {
            this.Initialize(anchor, anchor, axis, useWorldCoordinates);
        }

        public PrismaticJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, TSVector2 axis, bool useWorldCoordinates = false) : base(bodyA, bodyB)
        {
            this.Initialize(anchorA, anchorB, axis, useWorldCoordinates);
        }

        public FP GetMotorForce(FP invDt)
        {
            return (invDt * this.MotorImpulse);
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            return (TSVector2) (invDt * ((this._impulse.X * this._perp) + ((this.MotorImpulse + this._impulse.Z) * this._axis)));
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return (invDt * this._impulse.Y);
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
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector3 = data.positions[this._indexB].c;
            FP angle = data.positions[this._indexB].a;
            TSVector2 vector4 = data.velocities[this._indexB].v;
            FP fp4 = data.velocities[this._indexB].w;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            TSVector2 vector5 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            TSVector2 vector6 = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            TSVector2 vector7 = ((vector3 - c) + vector6) - vector5;
            FP fp5 = this._invMassA;
            FP fp6 = this._invMassB;
            FP fp7 = this._invIA;
            FP fp8 = this._invIB;
            this._axis = MathUtils.Mul(q, this.LocalXAxis);
            this._a1 = MathUtils.Cross(vector7 + vector5, this._axis);
            this._a2 = MathUtils.Cross(vector6, this._axis);
            this._motorMass = ((fp5 + fp6) + ((fp7 * this._a1) * this._a1)) + ((fp8 * this._a2) * this._a2);
            if (this._motorMass > 0f)
            {
                this._motorMass = 1f / this._motorMass;
            }
            this._perp = MathUtils.Mul(q, this._localYAxisA);
            this._s1 = MathUtils.Cross(vector7 + vector5, this._perp);
            this._s2 = MathUtils.Cross(vector6, this._perp);
            FP x = ((fp5 + fp6) + ((fp7 * this._s1) * this._s1)) + ((fp8 * this._s2) * this._s2);
            FP y = (fp7 * this._s1) + (fp8 * this._s2);
            FP z = ((fp7 * this._s1) * this._a1) + ((fp8 * this._s2) * this._a2);
            FP fp12 = fp7 + fp8;
            if (fp12 == 0f)
            {
                fp12 = 1f;
            }
            FP fp13 = (fp7 * this._a1) + (fp8 * this._a2);
            FP fp14 = ((fp5 + fp6) + ((fp7 * this._a1) * this._a1)) + ((fp8 * this._a2) * this._a2);
            this._K.ex = new Vector3(x, y, z);
            this._K.ey = new Vector3(y, fp12, fp13);
            this._K.ez = new Vector3(z, fp13, fp14);
            if (this._enableLimit)
            {
                FP fp15 = TSVector2.Dot(this._axis, vector7);
                if (FP.Abs(this._upperTranslation - this._lowerTranslation) < (2f * Settings.LinearSlop))
                {
                    this._limitState = LimitState.Equal;
                }
                else if (fp15 <= this._lowerTranslation)
                {
                    if (this._limitState != LimitState.AtLower)
                    {
                        this._limitState = LimitState.AtLower;
                        this._impulse.Z = 0f;
                    }
                }
                else if (fp15 >= this._upperTranslation)
                {
                    if (this._limitState != LimitState.AtUpper)
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
            else
            {
                this._limitState = LimitState.Inactive;
                this._impulse.Z = 0f;
            }
            if (!this._enableMotor)
            {
                this.MotorImpulse = 0f;
            }
            this._impulse *= data.step.dtRatio;
            this.MotorImpulse *= data.step.dtRatio;
            TSVector2 vector8 = (TSVector2) ((this._impulse.X * this._perp) + ((this.MotorImpulse + this._impulse.Z) * this._axis));
            FP fp16 = ((this._impulse.X * this._s1) + this._impulse.Y) + ((this.MotorImpulse + this._impulse.Z) * this._a1);
            FP fp17 = ((this._impulse.X * this._s2) + this._impulse.Y) + ((this.MotorImpulse + this._impulse.Z) * this._a2);
            v -= fp5 * vector8;
            w -= fp7 * fp16;
            vector4 += fp6 * vector8;
            fp4 += fp8 * fp17;
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector4;
            data.velocities[this._indexB].w = fp4;
        }

        public void SetLimits(FP lower, FP upper)
        {
            if ((upper != this._upperTranslation) || (lower != this._lowerTranslation))
            {
                base.WakeBodies();
                this._upperTranslation = upper;
                this._lowerTranslation = lower;
                this._impulse.Z = 0f;
            }
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            Vector3 vector8;
            TSVector2 c = data.positions[this._indexA].c;
            FP a = data.positions[this._indexA].a;
            TSVector2 vector2 = data.positions[this._indexB].c;
            FP angle = data.positions[this._indexB].a;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            FP fp3 = this._invMassA;
            FP fp4 = this._invMassB;
            FP fp5 = this._invIA;
            FP fp6 = this._invIB;
            TSVector2 vector3 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            TSVector2 vector4 = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            TSVector2 vector5 = ((vector2 + vector4) - c) - vector3;
            TSVector2 b = MathUtils.Mul(q, this.LocalXAxis);
            FP fp7 = MathUtils.Cross(vector5 + vector3, b);
            FP fp8 = MathUtils.Cross(vector4, b);
            TSVector2 vector7 = MathUtils.Mul(q, this._localYAxisA);
            FP fp9 = MathUtils.Cross(vector5 + vector3, vector7);
            FP fp10 = MathUtils.Cross(vector4, vector7);
            TSVector2 vector9 = new TSVector2 {
                x = TSVector2.Dot(vector7, vector5),
                y = (angle - a) - this.ReferenceAngle
            };
            FP fp11 = FP.Abs(vector9.x);
            FP fp12 = FP.Abs(vector9.y);
            bool flag = false;
            FP fp13 = 0f;
            if (this._enableLimit)
            {
                FP fp16 = TSVector2.Dot(b, vector5);
                if (FP.Abs(this._upperTranslation - this._lowerTranslation) < (2f * Settings.LinearSlop))
                {
                    fp13 = MathUtils.Clamp(fp16, -Settings.MaxLinearCorrection, Settings.MaxLinearCorrection);
                    fp11 = TSMath.Max(fp11, FP.Abs(fp16));
                    flag = true;
                }
                else if (fp16 <= this._lowerTranslation)
                {
                    fp13 = MathUtils.Clamp((fp16 - this._lowerTranslation) + Settings.LinearSlop, -Settings.MaxLinearCorrection, 0f);
                    fp11 = TSMath.Max(fp11, this._lowerTranslation - fp16);
                    flag = true;
                }
                else if (fp16 >= this._upperTranslation)
                {
                    fp13 = MathUtils.Clamp((fp16 - this._upperTranslation) - Settings.LinearSlop, 0f, Settings.MaxLinearCorrection);
                    fp11 = TSMath.Max(fp11, fp16 - this._upperTranslation);
                    flag = true;
                }
            }
            if (flag)
            {
                FP x = ((fp3 + fp4) + ((fp5 * fp9) * fp9)) + ((fp6 * fp10) * fp10);
                FP y = (fp5 * fp9) + (fp6 * fp10);
                FP z = ((fp5 * fp9) * fp7) + ((fp6 * fp10) * fp8);
                FP fp20 = fp5 + fp6;
                if (fp20 == 0f)
                {
                    fp20 = 1f;
                }
                FP fp21 = (fp5 * fp7) + (fp6 * fp8);
                FP fp22 = ((fp3 + fp4) + ((fp5 * fp7) * fp7)) + ((fp6 * fp8) * fp8);
                Mat33 mat = new Mat33 {
                    ex = new Vector3(x, y, z),
                    ey = new Vector3(y, fp20, fp21),
                    ez = new Vector3(z, fp21, fp22)
                };
                Vector3 vector11 = new Vector3 {
                    X = vector9.x,
                    Y = vector9.y,
                    Z = fp13
                };
                vector8 = mat.Solve33(-vector11);
            }
            else
            {
                FP fp23 = ((fp3 + fp4) + ((fp5 * fp9) * fp9)) + ((fp6 * fp10) * fp10);
                FP fp24 = (fp5 * fp9) + (fp6 * fp10);
                FP fp25 = fp5 + fp6;
                if (fp25 == 0f)
                {
                    fp25 = 1f;
                }
                TSVector2 vector12 = new Mat22 { ex = new TSVector2(fp23, fp24), ey = new TSVector2(fp24, fp25) }.Solve(-vector9);
                vector8 = new Vector3 {
                    X = vector12.x,
                    Y = vector12.y,
                    Z = 0f
                };
            }
            TSVector2 vector10 = (TSVector2) ((vector8.X * vector7) + (vector8.Z * b));
            FP fp14 = ((vector8.X * fp9) + vector8.Y) + (vector8.Z * fp7);
            FP fp15 = ((vector8.X * fp10) + vector8.Y) + (vector8.Z * fp8);
            c -= fp3 * vector10;
            a -= fp5 * fp14;
            vector2 += fp4 * vector10;
            angle += fp6 * fp15;
            data.positions[this._indexA].c = c;
            data.positions[this._indexA].a = a;
            data.positions[this._indexB].c = vector2;
            data.positions[this._indexB].a = angle;
            return ((fp11 <= Settings.LinearSlop) && (fp12 <= Settings.AngularSlop));
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP fp2 = data.velocities[this._indexB].w;
            FP fp3 = this._invMassA;
            FP fp4 = this._invMassB;
            FP fp5 = this._invIA;
            FP fp6 = this._invIB;
            if (this._enableMotor && (this._limitState != LimitState.Equal))
            {
                FP fp7 = (TSVector2.Dot(this._axis, vector2 - v) + (this._a2 * fp2)) - (this._a1 * w);
                FP fp8 = this._motorMass * (this._motorSpeed - fp7);
                FP motorImpulse = this.MotorImpulse;
                FP high = data.step.dt * this._maxMotorForce;
                this.MotorImpulse = MathUtils.Clamp(this.MotorImpulse + fp8, -high, high);
                fp8 = this.MotorImpulse - motorImpulse;
                TSVector2 vector4 = (TSVector2) (fp8 * this._axis);
                FP fp11 = fp8 * this._a1;
                FP fp12 = fp8 * this._a2;
                v -= fp3 * vector4;
                w -= fp5 * fp11;
                vector2 += fp4 * vector4;
                fp2 += fp6 * fp12;
            }
            TSVector2 vector3 = new TSVector2 {
                x = (TSVector2.Dot(this._perp, vector2 - v) + (this._s2 * fp2)) - (this._s1 * w),
                y = fp2 - w
            };
            if (this._enableLimit && (this._limitState > LimitState.Inactive))
            {
                FP z = (TSVector2.Dot(this._axis, vector2 - v) + (this._a2 * fp2)) - (this._a1 * w);
                Vector3 vector5 = new Vector3(vector3.x, vector3.y, z);
                Vector3 vector6 = this._impulse;
                Vector3 vector7 = this._K.Solve33(-vector5);
                this._impulse += vector7;
                if (this._limitState == LimitState.AtLower)
                {
                    this._impulse.Z = TSMath.Max(this._impulse.Z, 0f);
                }
                else if (this._limitState == LimitState.AtUpper)
                {
                    this._impulse.Z = TSMath.Min(this._impulse.Z, 0f);
                }
                TSVector2 b = -vector3 - ((this._impulse.Z - vector6.Z) * new TSVector2(this._K.ez.X, this._K.ez.Y));
                TSVector2 vector9 = this._K.Solve22(b) + new TSVector2(vector6.X, vector6.Y);
                this._impulse.X = vector9.x;
                this._impulse.Y = vector9.y;
                vector7 = this._impulse - vector6;
                TSVector2 vector10 = (TSVector2) ((vector7.X * this._perp) + (vector7.Z * this._axis));
                FP fp14 = ((vector7.X * this._s1) + vector7.Y) + (vector7.Z * this._a1);
                FP fp15 = ((vector7.X * this._s2) + vector7.Y) + (vector7.Z * this._a2);
                v -= fp3 * vector10;
                w -= fp5 * fp14;
                vector2 += fp4 * vector10;
                fp2 += fp6 * fp15;
            }
            else
            {
                TSVector2 vector11 = this._K.Solve22(-vector3);
                this._impulse.X += vector11.x;
                this._impulse.Y += vector11.y;
                TSVector2 vector12 = (TSVector2) (vector11.x * this._perp);
                FP fp16 = (vector11.x * this._s1) + vector11.y;
                FP fp17 = (vector11.x * this._s2) + vector11.y;
                v -= fp3 * vector12;
                w -= fp5 * fp16;
                vector2 += fp4 * vector12;
                fp2 += fp6 * fp17;
            }
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = fp2;
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

        public FP JointSpeed
        {
            get
            {
                Transform transform;
                Transform transform2;
                base.BodyA.GetTransform(out transform);
                base.BodyB.GetTransform(out transform2);
                TSVector2 a = MathUtils.Mul(ref transform.q, this.LocalAnchorA - base.BodyA.LocalCenter);
                TSVector2 vector2 = MathUtils.Mul(ref transform2.q, this.LocalAnchorB - base.BodyB.LocalCenter);
                TSVector2 vector3 = base.BodyA._sweep.C + a;
                TSVector2 vector4 = base.BodyB._sweep.C + vector2;
                TSVector2 vector5 = vector4 - vector3;
                TSVector2 worldVector = base.BodyA.GetWorldVector(this.LocalXAxis);
                TSVector2 vector7 = base.BodyA._linearVelocity;
                TSVector2 vector8 = base.BodyB._linearVelocity;
                FP s = base.BodyA._angularVelocity;
                FP fp2 = base.BodyB._angularVelocity;
                return (TSVector2.Dot(vector5, MathUtils.Cross(s, worldVector)) + TSVector2.Dot(worldVector, ((vector8 + MathUtils.Cross(fp2, vector2)) - vector7) - MathUtils.Cross(s, a)));
            }
        }

        public FP JointTranslation
        {
            get
            {
                TSVector2 vector = base.BodyB.GetWorldPoint(this.LocalAnchorB) - base.BodyA.GetWorldPoint(this.LocalAnchorA);
                TSVector2 worldVector = base.BodyA.GetWorldVector(this.LocalXAxis);
                return TSVector2.Dot(vector, worldVector);
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
                if (value != this._enableLimit)
                {
                    base.WakeBodies();
                    this._enableLimit = value;
                    this._impulse.Z = 0;
                }
            }
        }

        public TSVector2 LocalAnchorA { get; set; }

        public TSVector2 LocalAnchorB { get; set; }

        public TSVector2 LocalXAxis { get; private set; }

        public FP LowerLimit
        {
            get
            {
                return this._lowerTranslation;
            }
            set
            {
                if (value != this._lowerTranslation)
                {
                    base.WakeBodies();
                    this._lowerTranslation = value;
                    this._impulse.Z = 0f;
                }
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

        public FP MotorImpulse { get; set; }

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

        public FP ReferenceAngle { get; set; }

        public FP UpperLimit
        {
            get
            {
                return this._upperTranslation;
            }
            set
            {
                if (value != this._upperTranslation)
                {
                    base.WakeBodies();
                    this._upperTranslation = value;
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

