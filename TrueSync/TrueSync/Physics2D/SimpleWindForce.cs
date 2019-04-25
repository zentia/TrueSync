namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public class SimpleWindForce : AbstractForceController
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TSVector2 <Direction>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Divergence>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IgnorePosition>k__BackingField;

        public override void ApplyForce(FP dt, FP strength)
        {
            foreach (Body body in base.World.BodyList)
            {
                FP decayMultiplier = base.GetDecayMultiplier(body);
                if (decayMultiplier != 0)
                {
                    TSVector2 direction;
                    if (base.ForceType == AbstractForceController.ForceTypes.Point)
                    {
                        direction = body.Position - base.Position;
                    }
                    else
                    {
                        this.Direction.Normalize();
                        direction = this.Direction;
                        if (direction.magnitude == 0)
                        {
                            direction = new TSVector2(0, 1);
                        }
                    }
                    if (base.Variation != 0)
                    {
                        FP fp2 = TSRandom.value * MathHelper.Clamp(base.Variation, 0, 1);
                        direction.Normalize();
                        body.ApplyForce(((direction * strength) * decayMultiplier) * fp2);
                    }
                    else
                    {
                        direction.Normalize();
                        body.ApplyForce((direction * strength) * decayMultiplier);
                    }
                }
            }
        }

        public TSVector2 Direction { get; set; }

        public FP Divergence { get; set; }

        public bool IgnorePosition { get; set; }
    }
}

