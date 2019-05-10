using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public sealed class SimpleExplosion : PhysicsLogic
	{
		public FP Power
		{
			get;
			set;
		}

		public SimpleExplosion(World world) : base(world, PhysicsLogicType.Explosion)
		{
			this.Power = 1;
		}

		public Dictionary<Body, TSVector2> Activate(TSVector2 pos, FP radius, FP force, FP maxForce)
		{
			HashSet<Body> affectedBodies = new HashSet<Body>();
			AABB aABB;
			aABB.LowerBound = pos - new TSVector2(radius);
			aABB.UpperBound = pos + new TSVector2(radius);
			this.World.QueryAABB(delegate(Fixture fixture)
			{
				bool flag = TSVector2.Distance(fixture.Body.Position, pos) <= radius;
				if (flag)
				{
					bool flag2 = !affectedBodies.Contains(fixture.Body);
					if (flag2)
					{
						affectedBodies.Add(fixture.Body);
					}
				}
				return true;
			}, ref aABB);
			return this.ApplyImpulse(pos, radius, force, maxForce, affectedBodies);
		}

		private Dictionary<Body, TSVector2> ApplyImpulse(TSVector2 pos, FP radius, FP force, FP maxForce, HashSet<Body> overlappingBodies)
		{
			Dictionary<Body, TSVector2> dictionary = new Dictionary<Body, TSVector2>(overlappingBodies.Count);
			foreach (Body current in overlappingBodies)
			{
				bool flag = this.IsActiveOn(current);
				if (flag)
				{
					FP distance = TSVector2.Distance(pos, current.Position);
					FP percent = this.GetPercent(distance, radius);
					TSVector2 tSVector = pos - current.Position;
					tSVector *= 1f / FP.Sqrt(tSVector.x * tSVector.x + tSVector.y * tSVector.y);
					tSVector *= MathHelper.Min(force * percent, maxForce);
					tSVector *= -1;
					current.ApplyLinearImpulse(tSVector);
					dictionary.Add(current, tSVector);
				}
			}
			return dictionary;
		}

		private FP GetPercent(FP distance, FP radius)
		{
			FP value = Math.Pow((double)(1 - (distance - radius) / radius).AsFloat(), (double)this.Power.AsFloat()) - 1;
			bool flag = FP.IsNaN(value);
			FP result;
			if (flag)
			{
				result = 0f;
			}
			else
			{
				result = MathHelper.Clamp(value, 0f, 1f);
			}
			return result;
		}
	}
}
