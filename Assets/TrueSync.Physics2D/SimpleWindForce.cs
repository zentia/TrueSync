using Microsoft.Xna.Framework;
using System;

namespace TrueSync.Physics2D
{
	public class SimpleWindForce : AbstractForceController
	{
		public TSVector2 Direction
		{
			get;
			set;
		}

		public FP Divergence
		{
			get;
			set;
		}

		public bool IgnorePosition
		{
			get;
			set;
		}

		public override void ApplyForce(FP dt, FP strength)
		{
			foreach (Body current in this.World.BodyList)
			{
				FP decayMultiplier = base.GetDecayMultiplier(current);
				bool flag = decayMultiplier != 0;
				if (flag)
				{
					bool flag2 = this.ForceType == AbstractForceController.ForceTypes.Point;
					TSVector2 value;
					if (flag2)
					{
						value = current.Position - base.Position;
					}
					else
					{
						this.Direction.Normalize();
						value = this.Direction;
						bool flag3 = value.magnitude == 0;
						if (flag3)
						{
							value = new TSVector2(0, 1);
						}
					}
					bool flag4 = base.Variation != 0;
					if (flag4)
					{
						FP scaleFactor = TSRandom.value * MathHelper.Clamp(base.Variation, 0, 1);
						value.Normalize();
						current.ApplyForce(value * strength * decayMultiplier * scaleFactor);
					}
					else
					{
						value.Normalize();
						current.ApplyForce(value * strength * decayMultiplier);
					}
				}
			}
		}
	}
}
