using Microsoft.Xna.Framework;
using System;

namespace TrueSync.Physics2D
{
	public abstract class AbstractForceController : Controller
	{
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

		public Curve DecayCurve;

		public AbstractForceController.ForceTypes ForceType;

		protected TSRandom Randomize;

		public Curve StrengthCurve;

		public FP Strength
		{
			get;
			set;
		}

		public TSVector2 Position
		{
			get;
			set;
		}

		public FP MaximumSpeed
		{
			get;
			set;
		}

		public FP MaximumForce
		{
			get;
			set;
		}

		public AbstractForceController.TimingModes TimingMode
		{
			get;
			set;
		}

		public FP ImpulseTime
		{
			get;
			private set;
		}

		public FP ImpulseLength
		{
			get;
			set;
		}

		public bool Triggered
		{
			get;
			private set;
		}

		public FP Variation
		{
			get;
			set;
		}

		public AbstractForceController.DecayModes DecayMode
		{
			get;
			set;
		}

		public FP DecayStart
		{
			get;
			set;
		}

		public FP DecayEnd
		{
			get;
			set;
		}

		public AbstractForceController() : base(ControllerType.AbstractForceController)
		{
			this.Enabled = true;
			this.Strength = 1f;
			this.Position = new TSVector2(0, 0);
			this.MaximumSpeed = 100f;
			this.TimingMode = AbstractForceController.TimingModes.Switched;
			this.ImpulseTime = 0f;
			this.ImpulseLength = 1f;
			this.Triggered = false;
			this.StrengthCurve = new Curve();
			this.Variation = 0f;
			this.Randomize = TSRandom.New(1234);
			this.DecayMode = AbstractForceController.DecayModes.None;
			this.DecayCurve = new Curve();
			this.DecayStart = 0f;
			this.DecayEnd = 0f;
			this.StrengthCurve.Keys.Add(new CurveKey(0, 5));
			this.StrengthCurve.Keys.Add(new CurveKey(0.1f, 5));
			this.StrengthCurve.Keys.Add(new CurveKey(0.2f, -4));
			this.StrengthCurve.Keys.Add(new CurveKey(1f, 0));
		}

		public AbstractForceController(AbstractForceController.TimingModes mode) : base(ControllerType.AbstractForceController)
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

		protected FP GetDecayMultiplier(Body body)
		{
			FP magnitude = (body.Position - this.Position).magnitude;
			FP result;
			switch (this.DecayMode)
			{
			case AbstractForceController.DecayModes.None:
				result = 1f;
				break;
			case AbstractForceController.DecayModes.Step:
			{
				bool flag = magnitude < this.DecayEnd;
				if (flag)
				{
					result = 1f;
				}
				else
				{
					result = 0f;
				}
				break;
			}
			case AbstractForceController.DecayModes.Linear:
			{
				bool flag2 = magnitude < this.DecayStart;
				if (flag2)
				{
					result = 1f;
				}
				else
				{
					bool flag3 = magnitude > this.DecayEnd;
					if (flag3)
					{
						result = 0f;
					}
					else
					{
						result = this.DecayEnd - this.DecayStart / magnitude - this.DecayStart;
					}
				}
				break;
			}
			case AbstractForceController.DecayModes.InverseSquare:
			{
				bool flag4 = magnitude < this.DecayStart;
				if (flag4)
				{
					result = 1f;
				}
				else
				{
					result = 1f / ((magnitude - this.DecayStart) * (magnitude - this.DecayStart));
				}
				break;
			}
			case AbstractForceController.DecayModes.Curve:
			{
				bool flag5 = magnitude < this.DecayStart;
				if (flag5)
				{
					result = 1f;
				}
				else
				{
					result = this.DecayCurve.Evaluate(magnitude - this.DecayStart);
				}
				break;
			}
			default:
				result = 1f;
				break;
			}
			return result;
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
			case AbstractForceController.TimingModes.Switched:
			{
				bool enabled = this.Enabled;
				if (enabled)
				{
					this.ApplyForce(dt, this.Strength);
				}
				break;
			}
			case AbstractForceController.TimingModes.Triggered:
			{
				bool flag = this.Enabled && this.Triggered;
				if (flag)
				{
					bool flag2 = this.ImpulseTime < this.ImpulseLength;
					if (flag2)
					{
						this.ApplyForce(dt, this.Strength);
						this.ImpulseTime += dt;
					}
					else
					{
						this.Triggered = false;
					}
				}
				break;
			}
			case AbstractForceController.TimingModes.Curve:
			{
				bool flag3 = this.Enabled && this.Triggered;
				if (flag3)
				{
					bool flag4 = this.ImpulseTime < this.ImpulseLength;
					if (flag4)
					{
						this.ApplyForce(dt, this.Strength * this.StrengthCurve.Evaluate(this.ImpulseTime));
						this.ImpulseTime += dt;
					}
					else
					{
						this.Triggered = false;
					}
				}
				break;
			}
			}
		}

		public abstract void ApplyForce(FP dt, FP strength);
	}
}
