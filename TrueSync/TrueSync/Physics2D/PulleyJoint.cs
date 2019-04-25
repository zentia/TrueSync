namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class PulleyJoint : TrueSync.Physics2D.Joint
    {
        private FP _impulse;
        private int _indexA;
        private int _indexB;
        private FP _invIA;
        private FP _invIB;
        private FP _invMassA;
        private FP _invMassB;
        private TSVector2 _localCenterA;
        private TSVector2 _localCenterB;
        private FP _mass;
        private TSVector2 _rA;
        private TSVector2 _rB;
        private TSVector2 _uA;
        private TSVector2 _uB;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Constant>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <LengthA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <LengthB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Ratio>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <WorldAnchorA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <WorldAnchorB>k__BackingField;

        internal PulleyJoint()
        {
            base.JointType = JointType.Pulley;
        }

        public PulleyJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, TSVector2 worldAnchorA, TSVector2 worldAnchorB, FP ratio, bool useWorldCoordinates = false) : base(bodyA, bodyB)
        {
            base.JointType = JointType.Pulley;
            this.WorldAnchorA = worldAnchorA;
            this.WorldAnchorB = worldAnchorB;
            if (useWorldCoordinates)
            {
                this.LocalAnchorA = base.BodyA.GetLocalPoint(anchorA);
                this.LocalAnchorB = base.BodyB.GetLocalPoint(anchorB);
                TSVector2 vector = anchorA - worldAnchorA;
                this.LengthA = vector.magnitude;
                TSVector2 vector2 = anchorB - worldAnchorB;
                this.LengthB = vector2.magnitude;
            }
            else
            {
                this.LocalAnchorA = anchorA;
                this.LocalAnchorB = anchorB;
                TSVector2 vector3 = anchorA - base.BodyA.GetLocalPoint(worldAnchorA);
                this.LengthA = vector3.magnitude;
                TSVector2 vector4 = anchorB - base.BodyB.GetLocalPoint(worldAnchorB);
                this.LengthB = vector4.magnitude;
            }
            Debug.Assert(ratio != 0f);
            Debug.Assert(ratio > Settings.Epsilon);
            this.Ratio = ratio;
            this.Constant = this.LengthA + (ratio * this.LengthB);
            this._impulse = 0f;
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            TSVector2 vector = (TSVector2) (this._impulse * this._uB);
            return (TSVector2) (invDt * vector);
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return 0f;
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
            this._uA = (c + this._rA) - this.WorldAnchorA;
            this._uB = (vector3 + this._rB) - this.WorldAnchorB;
            FP magnitude = this._uA.magnitude;
            FP fp6 = this._uB.magnitude;
            if (magnitude > (10f * Settings.LinearSlop))
            {
                this._uA = (TSVector2) (this._uA * (1f / magnitude));
            }
            else
            {
                this._uA = TSVector2.zero;
            }
            if (fp6 > (10f * Settings.LinearSlop))
            {
                this._uB = (TSVector2) (this._uB * (1f / fp6));
            }
            else
            {
                this._uB = TSVector2.zero;
            }
            FP fp7 = MathUtils.Cross(this._rA, this._uA);
            FP fp8 = MathUtils.Cross(this._rB, this._uB);
            FP fp9 = this._invMassA + ((this._invIA * fp7) * fp7);
            FP fp10 = this._invMassB + ((this._invIB * fp8) * fp8);
            this._mass = fp9 + ((this.Ratio * this.Ratio) * fp10);
            if (this._mass > 0f)
            {
                this._mass = 1f / this._mass;
            }
            this._impulse *= data.step.dtRatio;
            TSVector2 b = (TSVector2) (-this._impulse * this._uA);
            TSVector2 vector6 = (TSVector2) ((-this.Ratio * this._impulse) * this._uB);
            v += this._invMassA * b;
            w += this._invIA * MathUtils.Cross(this._rA, b);
            vector4 += this._invMassB * vector6;
            fp4 += this._invIB * MathUtils.Cross(this._rB, vector6);
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
            TSVector2 b = (c + vector3) - this.WorldAnchorA;
            TSVector2 zero = (vector2 + vector4) - this.WorldAnchorB;
            FP magnitude = b.magnitude;
            FP fp4 = zero.magnitude;
            if (magnitude > (10f * Settings.LinearSlop))
            {
                b = (TSVector2) (b * (1f / magnitude));
            }
            else
            {
                b = TSVector2.zero;
            }
            if (fp4 > (10f * Settings.LinearSlop))
            {
                zero = (TSVector2) (zero * (1f / fp4));
            }
            else
            {
                zero = TSVector2.zero;
            }
            FP fp5 = MathUtils.Cross(vector3, b);
            FP fp6 = MathUtils.Cross(vector4, zero);
            FP fp7 = this._invMassA + ((this._invIA * fp5) * fp5);
            FP fp8 = this._invMassB + ((this._invIB * fp6) * fp6);
            FP fp9 = fp7 + ((this.Ratio * this.Ratio) * fp8);
            if (fp9 > 0f)
            {
                fp9 = 1f / fp9;
            }
            FP fp10 = (this.Constant - magnitude) - (this.Ratio * fp4);
            FP fp11 = FP.Abs(fp10);
            FP fp12 = -fp9 * fp10;
            TSVector2 vector7 = (TSVector2) (-fp12 * b);
            TSVector2 vector8 = (TSVector2) ((-this.Ratio * fp12) * zero);
            c += this._invMassA * vector7;
            a += this._invIA * MathUtils.Cross(vector3, vector7);
            vector2 += this._invMassB * vector8;
            angle += this._invIB * MathUtils.Cross(vector4, vector8);
            data.positions[this._indexA].c = c;
            data.positions[this._indexA].a = a;
            data.positions[this._indexB].c = vector2;
            data.positions[this._indexB].a = angle;
            return (fp11 < Settings.LinearSlop);
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector2 = data.velocities[this._indexB].v;
            FP s = data.velocities[this._indexB].w;
            TSVector2 vector3 = v + MathUtils.Cross(w, this._rA);
            TSVector2 vector4 = vector2 + MathUtils.Cross(s, this._rB);
            FP fp3 = -TSVector2.Dot(this._uA, vector3) - (this.Ratio * TSVector2.Dot(this._uB, vector4));
            FP fp4 = -this._mass * fp3;
            this._impulse += fp4;
            TSVector2 b = (TSVector2) (-fp4 * this._uA);
            TSVector2 vector6 = (TSVector2) ((-this.Ratio * fp4) * this._uB);
            v += this._invMassA * b;
            w += this._invIA * MathUtils.Cross(this._rA, b);
            vector2 += this._invMassB * vector6;
            s += this._invIB * MathUtils.Cross(this._rB, vector6);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = s;
        }

        internal FP Constant { get; set; }

        public FP CurrentLengthA
        {
            get
            {
                TSVector2 worldPoint = base.BodyA.GetWorldPoint(this.LocalAnchorA);
                TSVector2 worldAnchorA = this.WorldAnchorA;
                TSVector2 vector3 = worldPoint - worldAnchorA;
                return vector3.magnitude;
            }
        }

        public FP CurrentLengthB
        {
            get
            {
                TSVector2 worldPoint = base.BodyB.GetWorldPoint(this.LocalAnchorB);
                TSVector2 worldAnchorB = this.WorldAnchorB;
                TSVector2 vector3 = worldPoint - worldAnchorB;
                return vector3.magnitude;
            }
        }

        public FP LengthA { get; set; }

        public FP LengthB { get; set; }

        public TSVector2 LocalAnchorA { get; set; }

        public TSVector2 LocalAnchorB { get; set; }

        public FP Ratio { get; set; }

        public sealed override TSVector2 WorldAnchorA { get; set; }

        public sealed override TSVector2 WorldAnchorB { get; set; }
    }
}

