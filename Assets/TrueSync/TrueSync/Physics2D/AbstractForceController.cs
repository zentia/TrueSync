namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public abstract class AbstractForceController : Controller
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <DecayEnd>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private DecayModes <DecayMode>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <DecayStart>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <ImpulseLength>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <ImpulseTime>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <MaximumForce>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <MaximumSpeed>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <Position>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Strength>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TimingModes <TimingMode>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <Triggered>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Variation>k__BackingField;
        public Curve DecayCurve;
        public ForceTypes ForceType;
        protected TSRandom Randomize;
        public Curve StrengthCurve;

        public AbstractForceController() : base(ControllerType.AbstractForceController)
        {
            base.Enabled = true;
            this.Strength = 1f;
            this.Position = new TSVector2(0, 0);
            this.MaximumSpeed = 100f;
            this.TimingMode = TimingModes.Switched;
            this.ImpulseTime = 0f;
            this.ImpulseLength = 1f;
            this.Triggered = false;
            this.StrengthCurve = new Curve();
            this.Variation = 0f;
            this.Randomize = TSRandom.New(0x4d2);
            this.DecayMode = DecayModes.None;
            this.DecayCurve = new Curve();
            this.DecayStart = 0f;
            this.DecayEnd = 0f;
            this.StrengthCurve.Keys.Add(new CurveKey(0, 5));
            this.StrengthCurve.Keys.Add(new CurveKey(0.1f, 5));
            this.StrengthCurve.Keys.Add(new CurveKey(0.2f, -4));
            this.StrengthCurve.Keys.Add(new CurveKey(1f, 0));
        }

        public AbstractForceController(TimingModes mode) : base(ControllerType.AbstractForceController)
        {
            this.TimingMode = mode;
            switch (mode)
            {
                case TimingModes.Switched:
                    base.Enabled = true;
                    break;

                case TimingModes.Triggered:
                    base.Enabled = false;
                    break;

                case TimingModes.Curve:
                    base.Enabled = false;
                    break;
            }
        }

        public abstract void ApplyForce(FP dt, FP strength);
        protected FP GetDecayMultiplier(Body body)
        {
            TSVector2 vector = body.Position - this.Position;
            FP magnitude = vector.magnitude;
            switch (this.DecayMode)
            {
                case DecayModes.None:
                    return 1f;

                case DecayModes.Step:
                    if (magnitude >= this.DecayEnd)
                    {
                        return 0f;
                    }
                    return 1f;

                case DecayModes.Linear:
                    if (magnitude >= this.DecayStart)
                    {
                        if (magnitude > this.DecayEnd)
                        {
                            return 0f;
                        }
                        return ((this.DecayEnd - (this.DecayStart / magnitude)) - this.DecayStart);
                    }
                    return 1f;

                case DecayModes.InverseSquare:
                    if (magnitude >= this.DecayStart)
                    {
                        return (1f / ((magnitude - this.DecayStart) * (magnitude - this.DecayStart)));
                    }
                    return 1f;

                case DecayModes.Curve:
                    if (magnitude >= this.DecayStart)
                    {
                        return this.DecayCurve.Evaluate(magnitude - this.DecayStart);
                    }
                    return 1f;
            }
            return 1f;
        }

        public void Trigger()
        {
            this.Triggered = true;
            this.ImpulseTime = 0;
        }

        public override void Update(FP dt)
        {
            switch (this.TimingMode)
            {
                case TimingModes.Switched:
                    if (base.Enabled)
                    {
                        this.ApplyForce(dt, this.Strength);
                    }
                    break;

                case TimingModes.Triggered:
                    if (base.Enabled && this.Triggered)
                    {
                        if (this.ImpulseTime >= this.ImpulseLength)
                        {
                            this.Triggered = false;
                            break;
                        }
                        this.ApplyForce(dt, this.Strength);
                        this.ImpulseTime += dt;
                    }
                    break;

                case TimingModes.Curve:
                    if (base.Enabled && this.Triggered)
                    {
                        if (this.ImpulseTime >= this.ImpulseLength)
                        {
                            this.Triggered = false;
                            break;
                        }
                        this.ApplyForce(dt, this.Strength * this.StrengthCurve.Evaluate(this.ImpulseTime));
                        this.ImpulseTime += dt;
                    }
                    break;
            }
        }

        public FP DecayEnd { get; set; }

        public DecayModes DecayMode { get; set; }

        public FP DecayStart { get; set; }

        public FP ImpulseLength { get; set; }

        public FP ImpulseTime { get; private set; }

        public FP MaximumForce { get; set; }

        public FP MaximumSpeed { get; set; }

        public TSVector2 Position { get; set; }

        public FP Strength { get; set; }

        public TimingModes TimingMode { get; set; }

        public bool Triggered { get; private set; }

        public FP Variation { get; set; }

        public enum DecayModes
        {
            None,
            Step,
            Linear,
            InverseSquare,
            Curve
        }

        public enum ForceTypes
        {
            Point,
            Line,
            Area
        }

        public enum TimingModes
        {
            Switched,
            Triggered,
            Curve
        }
    }
}

