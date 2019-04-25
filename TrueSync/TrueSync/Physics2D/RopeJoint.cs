namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class RopeJoint : TrueSync.Physics2D.Joint
    {
        private FP _impulse;
        private int _indexA;
        private int _indexB;
        private FP _invIA;
        private FP _invIB;
        private FP _invMassA;
        private FP _invMassB;
        private FP _length;
        private TSVector2 _localCenterA;
        private TSVector2 _localCenterB;
        private FP _mass;
        private TSVector2 _rA;
        private TSVector2 _rB;
        private TSVector2 _u;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <MaxLength>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private LimitState <State>k__BackingField;

        internal RopeJoint()
        {
            base.JointType = JointType.Rope;
        }

        public RopeJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
        {
            base.JointType = JointType.Rope;
            if (useWorldCoordinates)
            {
                this.LocalAnchorA = bodyA.GetLocalPoint(anchorA);
                this.LocalAnchorB = bodyB.GetLocalPoint(anchorB);
            }
            else
            {
                this.LocalAnchorA = anchorA;
                this.LocalAnchorB = anchorB;
            }
            TSVector2 vector = this.WorldAnchorB - this.WorldAnchorA;
            this.MaxLength = vector.magnitude;
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            return (TSVector2) ((invDt * this._impulse) * this._u);
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return 0;
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
            this._rA = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            this._rB = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            this._u = ((vector3 + this._rB) - c) - this._rA;
            this._length = this._u.magnitude;
            FP fp5 = this._length - this.MaxLength;
            if (fp5 > 0f)
            {
                this.State = LimitState.AtUpper;
            }
            else
            {
                this.State = LimitState.Inactive;
            }
            if (this._length > Settings.LinearSlop)
            {
                this._u = (TSVector2) (this._u * (1f / this._length));
            }
            else
            {
                this._u = TSVector2.zero;
                this._mass = 0f;
                this._impulse = 0f;
                return;
            }
            FP fp6 = MathUtils.Cross(this._rA, this._u);
            FP fp7 = MathUtils.Cross(this._rB, this._u);
            FP fp8 = ((this._invMassA + ((this._invIA * fp6) * fp6)) + this._invMassB) + ((this._invIB * fp7) * fp7);
            this._mass = (fp8 != 0f) ? (1f / fp8) : 0f;
            this._impulse *= data.step.dtRatio;
            TSVector2 b = (TSVector2) (this._impulse * this._u);
            v -= this._invMassA * b;
            w -= this._invIA * MathUtils.Cross(this._rA, b);
            vector4 += this._invMassB * b;
            fp4 += this._invIB * MathUtils.Cross(this._rB, b);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector4;
            data.velocities[this._indexB].w = fp4;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            TSVector2 c = data.positions[this._indexA].c;
            FP a = data.positions[this._indexA].a;
            TSVector2 vector2 = data.positions[this._indexB].c;
            FP angle = data.positions[this._indexB].a;
            Rot q = new Rot(a);
            Rot rot2 = new Rot(angle);
            TSVector2 vector3 = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            TSVector2 vector4 = MathUtils.Mul(rot2, this.LocalAnchorB - this._localCenterB);
            TSVector2 vector5 = ((vector2 + vector4) - c) - vector3;
            FP magnitude = vector5.magnitude;
            vector5.Normalize();
            FP fp4 = magnitude - this.MaxLength;
            fp4 = MathUtils.Clamp(fp4, 0f, Settings.MaxLinearCorrection);
            FP fp5 = -this._mass * fp4;
            TSVector2 b = (TSVector2) (fp5 * vector5);
            c -= this._invMassA * b;
            a -= this._invIA * MathUtils.Cross(vector3, b);
            vector2 += this._invMassB * b;
            angle += this._invIB * MathUtils.Cross(vector4, b);
            data.positions[this._indexA].c = c;
            data.positions[this._indexA].a = a;
            data.positions[this._indexB].c = vector2;
            data.positions[this._indexB].a = angle;
            return ((magnitude - this.MaxLength) < Settings.LinearSlop);
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP s = data.velocities[this._indexB].w;
            TSVector2 vector3 = v + MathUtils.Cross(w, this._rA);
            TSVector2 vector4 = vector2 + MathUtils.Cross(s, this._rB);
            FP fp3 = this._length - this.MaxLength;
            FP fp4 = TSVector2.Dot(this._u, vector4 - vector3);
            if (fp3 < 0f)
            {
                fp4 += data.step.inv_dt * fp3;
            }
            FP fp5 = -this._mass * fp4;
            FP fp6 = this._impulse;
            this._impulse = TSMath.Min(0f, this._impulse + fp5);
            fp5 = this._impulse - fp6;
            TSVector2 b = (TSVector2) (fp5 * this._u);
            v -= this._invMassA * b;
            w -= this._invIA * MathUtils.Cross(this._rA, b);
            vector2 += this._invMassB * b;
            s += this._invIB * MathUtils.Cross(this._rB, b);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = s;
        }

        public TSVector2 LocalAnchorA { get; set; }

        public TSVector2 LocalAnchorB { get; set; }

        public FP MaxLength { get; set; }

        public LimitState State { get; private set; }

        public sealed override TSVector2 WorldAnchorA
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

        public sealed override TSVector2 WorldAnchorB
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

