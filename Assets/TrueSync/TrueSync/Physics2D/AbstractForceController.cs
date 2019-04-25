// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.AbstractForceController
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using Microsoft.Xna.Framework;

namespace TrueSync.Physics2D
{
    public abstract class AbstractForceController : Controller
    {
        public Curve DecayCurve;
        public AbstractForceController.ForceTypes ForceType;
        protected TSRandom Randomize;
        public Curve StrengthCurve;

        public AbstractForceController()
          : base(ControllerType.AbstractForceController)
        {
            this.Enabled = true;
            this.Strength = (FP)1f;
            this.Position = new TSVector2((FP)0, (FP)0);
            this.MaximumSpeed = (FP)100f;
            this.TimingMode = AbstractForceController.TimingModes.Switched;
            this.ImpulseTime = (FP)0.0f;
            this.ImpulseLength = (FP)1f;
            this.Triggered = false;
            this.StrengthCurve = new Curve();
            this.Variation = (FP)0.0f;
            this.Randomize = TSRandom.New(1234);
            this.DecayMode = AbstractForceController.DecayModes.None;
            this.DecayCurve = new Curve();
            this.DecayStart = (FP)0.0f;
            this.DecayEnd = (FP)0.0f;
            this.StrengthCurve.Keys.Add(new CurveKey((FP)0, (FP)5));
            this.StrengthCurve.Keys.Add(new CurveKey((FP)0.1f, (FP)5));
            this.StrengthCurve.Keys.Add(new CurveKey((FP)0.2f, (FP) - 4));
            this.StrengthCurve.Keys.Add(new CurveKey((FP)1f, (FP)0));
        }

        public AbstractForceController(AbstractForceController.TimingModes mode)
          : base(ControllerType.AbstractForceController)
        {
            this.TimingMode = mode;
            switch (mode)
            {
                case AbstractForceController.TimingModes.Switched:
                    this.Enabled = true;
                    break;
                case AbstractForceController.TimingModes.Triggered:
                    this.Enabled = false;
                    break;
                case AbstractForceController.TimingModes.Curve:
                    this.Enabled = false;
                    break;
            }
        }

        public FP Strength { get; set; }

        public TSVector2 Position { get; set; }

        public FP MaximumSpeed { get; set; }

        public FP MaximumForce { get; set; }

        public AbstractForceController.TimingModes TimingMode { get; set; }

        public FP ImpulseTime { get; private set; }

        public FP ImpulseLength { get; set; }

        public bool Triggered { get; private set; }

        public FP Variation { get; set; }

        public AbstractForceController.DecayModes DecayMode { get; set; }

        public FP DecayStart { get; set; }

        public FP DecayEnd { get; set; }

        protected FP GetDecayMultiplier(Body body)
        {
            FP magnitude = (body.Position - this.Position).magnitude;
            switch (this.DecayMode)
            {
                case AbstractForceController.DecayModes.None:
                    return (FP)1f;
                case AbstractForceController.DecayModes.Step:
                    if (magnitude < this.DecayEnd)
                        return (FP)1f;
                    return (FP)0.0f;
                case AbstractForceController.DecayModes.Linear:
                    if (magnitude < this.DecayStart)
                        return (FP)1f;
                    if (magnitude > this.DecayEnd)
                        return (FP)0.0f;
                    return this.DecayEnd - this.DecayStart / magnitude - this.DecayStart;
                case AbstractForceController.DecayModes.InverseSquare:
                    if (magnitude < this.DecayStart)
                        return (FP)1f;
                    return (FP)1f / ((magnitude - this.DecayStart) * (magnitude - this.DecayStart));
                case AbstractForceController.DecayModes.Curve:
                    if (magnitude < this.DecayStart)
                        return (FP)1f;
                    return this.DecayCurve.Evaluate(magnitude - this.DecayStart);
                default:
                    return (FP)1f;
            }
        }

        public void Trigger()
        {
            this.Triggered = true;
            this.ImpulseTime = (FP)0;
        }

        public override void Update(FP dt)
        {
            switch (this.TimingMode)
            {
                case AbstractForceController.TimingModes.Switched:
                    if (!this.Enabled)
                        break;
                    this.ApplyForce(dt, this.Strength);
                    break;
                case AbstractForceController.TimingModes.Triggered:
                    if (!this.Enabled || !this.Triggered)
                        break;
                    if (this.ImpulseTime < this.ImpulseLength)
                    {
                        this.ApplyForce(dt, this.Strength);
                        this.ImpulseTime = this.ImpulseTime + dt;
                    }
                    else
                        this.Triggered = false;
                    break;
                case AbstractForceController.TimingModes.Curve:
                    if (!this.Enabled || !this.Triggered)
                        break;
                    if (this.ImpulseTime < this.ImpulseLength)
                    {
                        this.ApplyForce(dt, this.Strength * this.StrengthCurve.Evaluate(this.ImpulseTime));
                        this.ImpulseTime = this.ImpulseTime + dt;
                    }
                    else
                        this.Triggered = false;
                    break;
            }
        }

        public abstract void ApplyForce(FP dt, FP strength);

        public enum DecayModes
        {
            None,
            Step,
            Linear,
            InverseSquare,
            Curve,
        }

        public enum ForceTypes
        {
            Point,
            Line,
            Area,
        }

        public enum TimingModes
        {
            Switched,
            Triggered,
            Curve,
        }
    }
}
