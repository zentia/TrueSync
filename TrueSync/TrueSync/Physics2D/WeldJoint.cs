namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class WeldJoint : TrueSync.Physics2D.Joint
    {
        private FP _bias;
        private FP _gamma;
        private Vector3 _impulse;
        private int _indexA;
        private int _indexB;
        private FP _invIA;
        private FP _invIB;
        private FP _invMassA;
        private FP _invMassB;
        private TSVector2 _localCenterA;
        private TSVector2 _localCenterB;
        private Mat33 _mass;
        private TSVector2 _rA;
        private TSVector2 _rB;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <DampingRatio>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <FrequencyHz>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <LocalAnchorB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <ReferenceAngle>k__BackingField;

        internal WeldJoint()
        {
            base.JointType = JointType.Weld;
        }

        public WeldJoint(Body bodyA, Body bodyB, TSVector2 anchorA, TSVector2 anchorB, bool useWorldCoordinates = false) : base(bodyA, bodyB)
        {
            base.JointType = JointType.Weld;
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
            this.ReferenceAngle = base.BodyB.Rotation - base.BodyA.Rotation;
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            return (TSVector2) (invDt * new TSVector2(this._impulse.X, this._impulse.Y));
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
            Mat33 mat = new Mat33();
            mat.ex.X = ((fp5 + fp6) + ((this._rA.y * this._rA.y) * fp7)) + ((this._rB.y * this._rB.y) * fp8);
            mat.ey.X = ((-this._rA.y * this._rA.x) * fp7) - ((this._rB.y * this._rB.x) * fp8);
            mat.ez.X = (-this._rA.y * fp7) - (this._rB.y * fp8);
            mat.ex.Y = mat.ey.X;
            mat.ey.Y = ((fp5 + fp6) + ((this._rA.x * this._rA.x) * fp7)) + ((this._rB.x * this._rB.x) * fp8);
            mat.ez.Y = (this._rA.x * fp7) + (this._rB.x * fp8);
            mat.ex.Z = mat.ez.X;
            mat.ey.Z = mat.ez.Y;
            mat.ez.Z = fp7 + fp8;
            if (this.FrequencyHz > 0f)
            {
                mat.GetInverse22(ref this._mass);
                FP fp9 = fp7 + fp8;
                FP fp10 = (fp9 > 0f) ? (1f / fp9) : 0f;
                FP fp11 = (angle - a) - this.ReferenceAngle;
                FP fp12 = (2f * Settings.Pi) * this.FrequencyHz;
                FP fp13 = ((2f * fp10) * this.DampingRatio) * fp12;
                FP fp14 = (fp10 * fp12) * fp12;
                FP dt = data.step.dt;
                this._gamma = dt * (fp13 + (dt * fp14));
                this._gamma = (this._gamma != 0f) ? (1f / this._gamma) : 0f;
                this._bias = ((fp11 * dt) * fp14) * this._gamma;
                fp9 += this._gamma;
                this._mass.ez.Z = (fp9 != 0f) ? (1f / fp9) : 0f;
            }
            else
            {
                mat.GetSymInverse33(ref this._mass);
                this._gamma = 0f;
                this._bias = 0f;
            }
            this._impulse *= data.step.dtRatio;
            TSVector2 b = new TSVector2(this._impulse.X, this._impulse.Y);
            v -= fp5 * b;
            w -= fp7 * (MathUtils.Cross(this._rA, b) + this._impulse.Z);
            vector2 += fp6 * b;
            fp4 += fp8 * (MathUtils.Cross(this._rB, b) + this._impulse.Z);
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = fp4;
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            FP magnitude;
            FP fp8;
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
            Mat33 mat = new Mat33();
            mat.ex.X = ((fp3 + fp4) + ((vector3.y * vector3.y) * fp5)) + ((vector4.y * vector4.y) * fp6);
            mat.ey.X = ((-vector3.y * vector3.x) * fp5) - ((vector4.y * vector4.x) * fp6);
            mat.ez.X = (-vector3.y * fp5) - (vector4.y * fp6);
            mat.ex.Y = mat.ey.X;
            mat.ey.Y = ((fp3 + fp4) + ((vector3.x * vector3.x) * fp5)) + ((vector4.x * vector4.x) * fp6);
            mat.ez.Y = (vector3.x * fp5) + (vector4.x * fp6);
            mat.ex.Z = mat.ez.X;
            mat.ey.Z = mat.ez.Y;
            mat.ez.Z = fp5 + fp6;
            if (this.FrequencyHz > 0f)
            {
                TSVector2 b = ((vector2 + vector4) - c) - vector3;
                magnitude = b.magnitude;
                fp8 = 0f;
                TSVector2 vector6 = -mat.Solve22(b);
                c -= fp3 * vector6;
                a -= fp5 * MathUtils.Cross(vector3, vector6);
                vector2 += fp4 * vector6;
                angle += fp6 * MathUtils.Cross(vector4, vector6);
            }
            else
            {
                TSVector2 vector7 = ((vector2 + vector4) - c) - vector3;
                FP fp9 = (angle - a) - this.ReferenceAngle;
                magnitude = vector7.magnitude;
                fp8 = FP.Abs(fp9);
                Vector3 vector8 = new Vector3(vector7.x, vector7.y, fp9);
                Vector3 vector9 = -mat.Solve33(vector8);
                TSVector2 vector10 = new TSVector2(vector9.X, vector9.Y);
                c -= fp3 * vector10;
                a -= fp5 * (MathUtils.Cross(vector3, vector10) + vector9.Z);
                vector2 += fp4 * vector10;
                angle += fp6 * (MathUtils.Cross(vector4, vector10) + vector9.Z);
            }
            data.positions[this._indexA].c = c;
            data.positions[this._indexA].a = a;
            data.positions[this._indexB].c = vector2;
            data.positions[this._indexB].a = angle;
            return ((magnitude <= Settings.LinearSlop) && (fp8 <= Settings.AngularSlop));
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
            if (this.FrequencyHz > 0f)
            {
                FP fp7 = s - w;
                FP fp8 = -this._mass.ez.Z * ((fp7 + this._bias) + (this._gamma * this._impulse.Z));
                this._impulse.Z += fp8;
                w -= fp5 * fp8;
                s += fp6 * fp8;
                TSVector2 vector3 = ((vector2 + MathUtils.Cross(s, this._rB)) - v) - MathUtils.Cross(w, this._rA);
                TSVector2 vector4 = -MathUtils.Mul22(this._mass, vector3);
                this._impulse.X += vector4.x;
                this._impulse.Y += vector4.y;
                TSVector2 b = vector4;
                v -= fp3 * b;
                w -= fp5 * MathUtils.Cross(this._rA, b);
                vector2 += fp4 * b;
                s += fp6 * MathUtils.Cross(this._rB, b);
            }
            else
            {
                TSVector2 vector6 = ((vector2 + MathUtils.Cross(s, this._rB)) - v) - MathUtils.Cross(w, this._rA);
                FP z = s - w;
                Vector3 vector7 = new Vector3(vector6.x, vector6.y, z);
                Vector3 vector8 = -MathUtils.Mul(this._mass, vector7);
                this._impulse += vector8;
                TSVector2 vector9 = new TSVector2(vector8.X, vector8.Y);
                v -= fp3 * vector9;
                w -= fp5 * (MathUtils.Cross(this._rA, vector9) + vector8.Z);
                vector2 += fp4 * vector9;
                s += fp6 * (MathUtils.Cross(this._rB, vector9) + vector8.Z);
            }
            data.velocities[this._indexA].v = v;
            data.velocities[this._indexA].w = w;
            data.velocities[this._indexB].v = vector2;
            data.velocities[this._indexB].w = s;
        }

        public FP DampingRatio { get; set; }

        public FP FrequencyHz { get; set; }

        public TSVector2 LocalAnchorA { get; set; }

        public TSVector2 LocalAnchorB { get; set; }

        public FP ReferenceAngle { get; set; }

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

