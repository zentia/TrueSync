namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public class AngleJoint : TrueSync.Physics2D.Joint
    {
        private FP _bias;
        private FP _jointError;
        private FP _massFactor;
        private FP _targetAngle;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <BiasFactor>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <MaxImpulse>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Softness>k__BackingField;

        internal AngleJoint()
        {
            base.JointType = JointType.Angle;
        }

        public AngleJoint(Body bodyA, Body bodyB) : base(bodyA, bodyB)
        {
            base.JointType = JointType.Angle;
            this.BiasFactor = 0.2f;
            this.MaxImpulse = FP.MaxValue;
        }

        public override TSVector2 GetReactionForce(FP invDt)
        {
            return TSVector2.zero;
        }

        public override FP GetReactionTorque(FP invDt)
        {
            return 0;
        }

        internal override void InitVelocityConstraints(ref SolverData data)
        {
            int islandIndex = base.BodyA.IslandIndex;
            int index = base.BodyB.IslandIndex;
            FP a = data.positions[islandIndex].a;
            FP fp2 = data.positions[index].a;
            this._jointError = (fp2 - a) - this.TargetAngle;
            this._bias = (-this.BiasFactor * data.step.inv_dt) * this._jointError;
            this._massFactor = (1 - this.Softness) / (base.BodyA._invI + base.BodyB._invI);
        }

        internal override bool SolvePositionConstraints(ref SolverData data)
        {
            return true;
        }

        internal override void SolveVelocityConstraints(ref SolverData data)
        {
            int islandIndex = base.BodyA.IslandIndex;
            int index = base.BodyB.IslandIndex;
            FP fp = ((this._bias - data.velocities[index].w) + data.velocities[islandIndex].w) * this._massFactor;
            data.velocities[islandIndex].w -= (base.BodyA._invI * FP.Sign(fp)) * TSMath.Min(FP.Abs(fp), this.MaxImpulse);
            data.velocities[index].w += (base.BodyB._invI * FP.Sign(fp)) * TSMath.Min(FP.Abs(fp), this.MaxImpulse);
        }

        public FP BiasFactor { get; set; }

        public FP MaxImpulse { get; set; }

        public FP Softness { get; set; }

        public FP TargetAngle
        {
            get
            {
                return this._targetAngle;
            }
            set
            {
                if (value != this._targetAngle)
                {
                    this._targetAngle = value;
                    base.WakeBodies();
                }
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

