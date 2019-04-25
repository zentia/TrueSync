// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.SimpleWindForce
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using Microsoft.Xna.Framework;

namespace TrueSync.Physics2D
{
    public class SimpleWindForce : AbstractForceController
    {
        public TSVector2 Direction { get; set; }

        public FP Divergence { get; set; }

        public bool IgnorePosition { get; set; }

        public override void ApplyForce(FP dt, FP strength)
        {
            foreach (Body body in this.World.BodyList)
            {
                FP decayMultiplier = this.GetDecayMultiplier(body);
                if (decayMultiplier != (FP)0)
                {
                    TSVector2 tsVector2;
                    if (this.ForceType == AbstractForceController.ForceTypes.Point)
                    {
                        tsVector2 = body.Position - this.Position;
                    }
                    else
                    {
                        this.Direction.Normalize();
                        tsVector2 = this.Direction;
                        if (tsVector2.magnitude == (FP)0)
                            tsVector2 = new TSVector2((FP)0, (FP)1);
                    }
                    if (this.Variation != (FP)0)
                    {
                        FP fp = TSRandom.value * MathHelper.Clamp(this.Variation, (FP)0, (FP)1);
                        tsVector2.Normalize();
                        body.ApplyForce(tsVector2 * strength * decayMultiplier * fp);
                    }
                    else
                    {
                        tsVector2.Normalize();
                        body.ApplyForce(tsVector2 * strength * decayMultiplier);
                    }
                }
            }
        }
    }
}
