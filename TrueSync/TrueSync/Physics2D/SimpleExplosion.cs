namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public sealed class SimpleExplosion : PhysicsLogic
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Power>k__BackingField;

        public SimpleExplosion(TrueSync.Physics2D.World world) : base(world, PhysicsLogicType.Explosion)
        {
            this.Power = 1;
        }

        public Dictionary<Body, TSVector2> Activate(TSVector2 pos, FP radius, FP force, FP maxForce)
        {
            AABB aabb;
            HashSet<Body> affectedBodies = new HashSet<Body>();
            aabb.LowerBound = pos - new TSVector2(radius);
            aabb.UpperBound = pos + new TSVector2(radius);
            base.World.QueryAABB(delegate (Fixture fixture) {
                if ((TSVector2.Distance(fixture.Body.Position, pos) <= radius) && !affectedBodies.Contains(fixture.Body))
                {
                    affectedBodies.Add(fixture.Body);
                }
                return true;
            }, ref aabb);
            return this.ApplyImpulse(pos, radius, force, maxForce, affectedBodies);
        }

        private Dictionary<Body, TSVector2> ApplyImpulse(TSVector2 pos, FP radius, FP force, FP maxForce, HashSet<Body> overlappingBodies)
        {
            Dictionary<Body, TSVector2> dictionary = new Dictionary<Body, TSVector2>(overlappingBodies.Count);
            foreach (Body body in overlappingBodies)
            {
                if (this.IsActiveOn(body))
                {
                    FP distance = TSVector2.Distance(pos, body.Position);
                    FP percent = this.GetPercent(distance, radius);
                    TSVector2 impulse = pos - body.Position;
                    impulse = (TSVector2) (impulse * (1f / FP.Sqrt((impulse.x * impulse.x) + (impulse.y * impulse.y))));
                    impulse *= MathHelper.Min(force * percent, maxForce);
                    impulse *= -1;
                    body.ApplyLinearImpulse(impulse);
                    dictionary.Add(body, impulse);
                }
            }
            return dictionary;
        }

        private FP GetPercent(FP distance, FP radius)
        {
            FP fp2 = 1 - ((distance - radius) / radius);
            FP fp = Math.Pow((double) fp2.AsFloat(), (double) this.Power.AsFloat()) - 1;
            if (FP.IsNaN(fp))
            {
                return 0f;
            }
            return MathHelper.Clamp(fp, 0f, 1f);
        }

        public FP Power { get; set; }
    }
}

