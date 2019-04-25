namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public class FixedMouseJoint : TrueSync.Physics2D.Joint
    {
        private FP _beta;
        private TSVector2 _C;
        private FP _dampingRatio;
        private FP _frequency;
        private FP _gamma;
        private TSVector2 _impulse;
        private int _indexA;
        private FP _invIA;
        private FP _invMassA;
        private TSVector2 _localCenterA;
        private Mat22 _mass;
        private FP _maxForce;
        private TSVector2 _rA;
        private TSVector2 _worldAnchor;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorA>k__BackingField;

        public FixedMouseJoint(Body body, TSVector2 worldAnchor) : base(body)
        {
            base.JointType = JointType.FixedMouse;
            this.Frequency = 5f;
            this.DampingRatio = 0.7f;
            this.MaxForce = 0x3e8 * body.Mass;
            Debug.Assert(worldAnchor.IsValid());
            this._worldAnchor = worldAnchor;
            this.LocalAnchorA = MathUtils.MulT(base.BodyA._xf, worldAnchor);
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            return (TSVector2) (invDt * this._impulse);
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return (invDt * 0f);
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            this._indexA = base.BodyA.IslandIndex;
            this._localCenterA = base.BodyA._sweep.LocalCenter;
            this._invMassA = base.BodyA._invMass;
            this._invIA = base.BodyA._invI;
            TSVector2 c = data.positions[this._indexA].c;
            FP a = data.positions[this._indexA].a;
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            Rot q = new Rot(a);
            FP mass = base.BodyA.Mass;
            FP fp4 = (2f * Settings.Pi) * this.Frequency;
            FP fp5 = ((2f * mass) * this.DampingRatio) * fp4;
            FP fp6 = mass * (fp4 * fp4);
            FP dt = data.step.dt;
            this._gamma = dt * (fp5 + (dt * fp6));
            if (this._gamma != 0f)
            {
                this._gamma = 1f / this._gamma;
            }
            this._beta = (dt * fp6) * this._gamma;
            this._rA = MathUtils.Mul(q, this.LocalAnchorA - this._localCenterA);
            Mat22 mat = new Mat22();
            mat.ex.x = (this._invMassA + ((this._invIA * this._rA.y) * this._rA.y)) + this._gamma;
            mat.ex.y = (-this._invIA * this._rA.x) * this._rA.y;
            mat.ey.x = mat.ex.y;
            mat.ey.y = (this._invMassA + ((this._invIA * this._rA.x) * this._rA.x)) + this._gamma;
            this._mass = mat.Inverse;
            this._C = (c + this._rA) - this._worldAnchor;
            this._C *= this._beta;
            w *= 0.98f;
            this._impulse *= data.step.dtRatio;
            v += this._invMassA * this._impulse;
            w += this._invIA * MathUtils.Cross(this._rA, this._impulse);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            TSVector2 v = data.velocities[this._indexA].v;
            FP w = data.velocities[this._indexA].w;
            TSVector2 vector2 = v + MathUtils.Cross(w, this._rA);
            TSVector2 b = MathUtils.Mul(ref this._mass, -((vector2 + this._C) + (this._gamma * this._impulse)));
            TSVector2 vector4 = this._impulse;
            this._impulse += b;
            FP fp2 = data.step.dt * this.MaxForce;
            if (this._impulse.LengthSquared() > (fp2 * fp2))
            {
                this._impulse *= fp2 / this._impulse.magnitude;
            }
            b = this._impulse - vector4;
            v += this._invMassA * b;
            w += this._invIA * MathUtils.Cross(this._rA, b);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
        }

        public FP DampingRatio
        {
            get
            {
                return this._dampingRatio;
            }
            set
            {
                Debug.Assert(MathUtils.IsValid(value) && (value >= 0f));
                this._dampingRatio = value;
            }
        }

        public FP Frequency
        {
            get
            {
                return this._frequency;
            }
            set
            {
                Debug.Assert(MathUtils.IsValid(value) && (value >= 0f));
                this._frequency = value;
            }
        }

        public TSVector2 LocalAnchorA { get; set; }

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
                return this._worldAnchor;
            }
            set
            {
                base.WakeBodies();
                this._worldAnchor = value;
            }
        }
    }
}

