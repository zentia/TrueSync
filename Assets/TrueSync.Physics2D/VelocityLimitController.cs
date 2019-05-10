using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public class VelocityLimitController : Controller
	{
		public bool LimitAngularVelocity = true;

		public bool LimitLinearVelocity = true;

		private List<Body> _bodies = new List<Body>();

		private FP _maxAngularSqared;

		private FP _maxAngularVelocity;

		private FP _maxLinearSqared;

		private FP _maxLinearVelocity;

		public FP MaxAngularVelocity
		{
			get
			{
				return this._maxAngularVelocity;
			}
			set
			{
				this._maxAngularVelocity = value;
				this._maxAngularSqared = this._maxAngularVelocity * this._maxAngularVelocity;
			}
		}

		public FP MaxLinearVelocity
		{
			get
			{
				return this._maxLinearVelocity;
			}
			set
			{
				this._maxLinearVelocity = value;
				this._maxLinearSqared = this._maxLinearVelocity * this._maxLinearVelocity;
			}
		}

		public VelocityLimitController() : base(ControllerType.VelocityLimitController)
		{
			this.MaxLinearVelocity = Settings.MaxTranslation;
			this.MaxAngularVelocity = Settings.MaxRotation;
		}

		public VelocityLimitController(FP maxLinearVelocity, FP maxAngularVelocity) : base(ControllerType.VelocityLimitController)
		{
			bool flag = maxLinearVelocity == 0 || maxLinearVelocity == FP.MaxValue;
			if (flag)
			{
				this.LimitLinearVelocity = false;
			}
			bool flag2 = maxAngularVelocity == 0 || maxAngularVelocity == FP.MaxValue;
			if (flag2)
			{
				this.LimitAngularVelocity = false;
			}
			this.MaxLinearVelocity = maxLinearVelocity;
			this.MaxAngularVelocity = maxAngularVelocity;
		}

		public override void Update(FP dt)
		{
			foreach (Body current in this._bodies)
			{
				bool flag = !this.IsActiveOn(current);
				if (!flag)
				{
					bool limitLinearVelocity = this.LimitLinearVelocity;
					if (limitLinearVelocity)
					{
						FP fP = dt * current._linearVelocity.x;
						FP fP2 = dt * current._linearVelocity.y;
						FP x = fP * fP + fP2 * fP2;
						bool flag2 = x > dt * this._maxLinearSqared;
						if (flag2)
						{
							FP y = FP.Sqrt(x);
							FP y2 = this._maxLinearVelocity / y;
							Body expr_B9_cp_0_cp_0 = current;
							expr_B9_cp_0_cp_0._linearVelocity.x = expr_B9_cp_0_cp_0._linearVelocity.x * y2;
							Body expr_D6_cp_0_cp_0 = current;
							expr_D6_cp_0_cp_0._linearVelocity.y = expr_D6_cp_0_cp_0._linearVelocity.y * y2;
						}
					}
					bool limitAngularVelocity = this.LimitAngularVelocity;
					if (limitAngularVelocity)
					{
						FP fP3 = dt * current._angularVelocity;
						bool flag3 = fP3 * fP3 > this._maxAngularSqared;
						if (flag3)
						{
							FP y3 = this._maxAngularVelocity / FP.Abs(fP3);
							current._angularVelocity *= y3;
						}
					}
				}
			}
		}

		public void AddBody(Body body)
		{
			this._bodies.Add(body);
		}

		public void RemoveBody(Body body)
		{
			this._bodies.Remove(body);
		}
	}
}
