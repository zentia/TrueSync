namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class MotorJoint : TrueSync.Physics2D.Joint
    {
        private FP _angularError;
        private FP _angularImpulse;
        private FP _angularMass;
        private FP _angularOffset;
        private int _indexA;
        private int _indexB;
        private FP _invIA;
        private FP _invIB;
        private FP _invMassA;
        private FP _invMassB;
        private TSVector2 _linearError;
        private TSVector2 _linearImpulse;
        private Mat22 _linearMass;
        private TSVector2 _linearOffset;
        private TSVector2 _localCenterA;
        private TSVector2 _localCenterB;
        private FP _maxForce;
        private FP _maxTorque;
        private TSVector2 _rA;
        private TSVector2 _rB;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <CorrectionFactor>k__BackingField;

        internal MotorJoint()
        {
            base.JointType = JointType.Motor;
        }

        public MotorJoint(Body bodyA, Body bodyB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
        {
            base.JointType = JointType.Motor;
            TSVector2 position = base.BodyB.Position;
            if (useWorldCoordinates)
            {
                this._linearOffset = base.BodyA.GetLocalPoint(position);
            }
            else
            {
                this._linearOffset = position;
            }
            this._angularOffset = 0f;
            this._maxForce = 1f;
            this._maxTorque = 1f;
            this.CorrectionFactor = 0.3f;
            this._angularOffset = base.BodyB.Rotation - base.BodyA.Rotation;
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            return (TSVector2) (invDt * this._linearImpulse);
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return (invDt * this._angularImpulse);
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
            this._rA = MathUtils.Mul(q, -this._localCenterA);
            this._rB = MathUtils.Mul(rot2, -this._localCenterB);
            FP fp5 = this._invMassA;
            FP fp6 = this._invMassB;
            FP fp7 = this._invIA;
            FP fp8 = this._invIB;
            Mat22 mat = new Mat22();
            mat.ex.x = ((fp5 + fp6) + ((fp7 * this._rA.y) * this._rA.y)) + ((fp8 * this._rB.y) * this._rB.y);
            mat.ex.y = ((-fp7 * this._rA.x) * this._rA.y) - ((fp8 * this._rB.x) * this._rB.y);
            mat.ey.x = mat.ex.y;
            mat.ey.y = ((fp5 + fp6) + ((fp7 * this._rA.x) * this._rA.x)) + ((fp8 * this._rB.x) * this._rB.x);
            this._linearMass = mat.Inverse;
            this._angularMass = fp7 + fp8;
            if (this._angularMass > 0f)
            {
                this._angularMass = 1f / this._angularMass;
            }
            this._linearError = (((vector3 + this._rB) - c) - this._rA) - MathUtils.Mul(q, this._linearOffset);
            this._angularError = (angle - a) - this._angularOffset;
            this._linearImpulse *= data.step.dtRatio;
            this._angularImpulse *= data.step.dtRatio;
            TSVector2 b = new TSVector2(this._linearImpulse.x, this._linearImpulse.y);
            v -= fp5 * b;
            w -= fp7 * (MathUtils.Cross(this._rA, b) + this._angularImpulse);
            vector4 += fp6 * b;
            fp4 += fp8 * (MathUtils.Cross(this._rB, b) + this._angularImpulse);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector4;
            data.velocities[this._indexB].w = fp4;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
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
            FP dt = data.step.dt;
            FP fp8 = data.step.inv_dt;
            FP fp9 = (s - w) + ((fp8 * this.CorrectionFactor) * this._angularError);
            FP fp10 = -this._angularMass * fp9;
            FP fp11 = this._angularImpulse;
            FP high = dt * this._maxTorque;
            this._angularImpulse = MathUtils.Clamp(this._angularImpulse + fp10, -high, high);
            fp10 = this._angularImpulse - fp11;
            w -= fp5 * fp10;
            s += fp6 * fp10;
            TSVector2 vector3 = (((vector2 + MathUtils.Cross(s, this._rB)) - v) - MathUtils.Cross(w, this._rA)) + ((fp8 * this.CorrectionFactor) * this._linearError);
            TSVector2 b = -MathUtils.Mul(ref this._linearMass, ref vector3);
            TSVector2 vector5 = this._linearImpulse;
            this._linearImpulse += b;
            FP fp13 = dt * this._maxForce;
            if (this._linearImpulse.LengthSquared() > (fp13 * fp13))
            {
                this._linearImpulse.Normalize();
                this._linearImpulse *= fp13;
            }
            b = this._linearImpulse - vector5;
            v -= fp3 * b;
            w -= fp5 * MathUtils.Cross(this._rA, b);
            vector2 += fp4 * b;
            s += fp6 * MathUtils.Cross(this._rB, b);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = s;
        }

        public FP AngularOffset
        {
            get
            {
                return this._angularOffset;
            }
            set
            {
                if (this._angularOffset != value)
                {
                    base.WakeBodies();
                    this._angularOffset = value;
                }
            }
        }

        internal FP CorrectionFactor { get; set; }

        public TSVector2 LinearOffset
        {
            get
            {
                return this._linearOffset;
            }
            set
            {
                if ((this._linearOffset.x != value.x) || (this._linearOffset.y != value.y))
                {
                    base.WakeBodies();
                    this._linearOffset = value;
                }
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
                Debug.Assert(MathUtils.IsValid(value) && (value >= 0f));
                this._maxForce = value;
            }
        }

        public FP MaxTorque
        {
            get
            {
                return this._maxTorque;
            }
            set
            {
                Debug.Assert(MathUtils.IsValid(value) && (value >= 0f));
                this._maxTorque = value;
            }
        }

        public override TSVector2 WorldAnchorA
        {
            get
            {
                return base.BodyA.Position;
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
                return base.BodyB.Position;
            }
            set
            {
                Debug.Assert(false, "You can't set the world anchor on this joint type.");
            }
        }
    }
}

