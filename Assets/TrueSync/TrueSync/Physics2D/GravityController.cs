// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.GravityController
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System.Collections.Generic;

namespace TrueSync.Physics2D
{
    public class GravityController : Controller
    {
        public GravityController(FP strength)
          : base(ControllerType.GravityController)
        {
            this.Strength = strength;
            this.MaxRadius = FP.MaxValue;
            this.GravityType = GravityType.DistanceSquared;
            this.Points = new List<TSVector2>();
            this.Bodies = new List<Body>();
        }

        public GravityController(FP strength, FP maxRadius, FP minRadius)
          : base(ControllerType.GravityController)
        {
            this.MinRadius = minRadius;
            this.MaxRadius = maxRadius;
            this.Strength = strength;
            this.GravityType = GravityType.DistanceSquared;
            this.Points = new List<TSVector2>();
            this.Bodies = new List<Body>();
        }

        public FP MinRadius { get; set; }

        public FP MaxRadius { get; set; }

        public FP Strength { get; set; }

        public GravityType GravityType { get; set; }

        public List<Body> Bodies { get; set; }

        public List<TSVector2> Points { get; set; }

        public override void Update(FP dt)
        {
            TSVector2 force = TSVector2.zero;
            foreach (Body body1 in this.World.BodyList)
            {
                if (this.IsActiveOn(body1))
                {
                    foreach (Body body2 in this.Bodies)
                    {
                        if (body1 != body2 && (!body1.IsStatic || !body2.IsStatic) && body2.Enabled)
                        {
                            TSVector2 tsVector2 = body2.Position - body1.Position;
                            FP x = tsVector2.LengthSquared();
                            if (!(x <= Settings.Epsilon) && !(x > this.MaxRadius * this.MaxRadius) && !(x < this.MinRadius * this.MinRadius))
                            {
                                switch (this.GravityType)
                                {
                                    case GravityType.Linear:
                                        force = this.Strength / FP.Sqrt(x) * body1.Mass * body2.Mass * tsVector2;
                                        break;
                                    case GravityType.DistanceSquared:
                                        force = this.Strength / x * body1.Mass * body2.Mass * tsVector2;
                                        break;
                                }
                                body1.ApplyForce(ref force);
                            }
                        }
                    }
                    foreach (TSVector2 point in this.Points)
                    {
                        TSVector2 tsVector2 = point - body1.Position;
                        FP x = tsVector2.LengthSquared();
                        if (!(x <= Settings.Epsilon) && !(x > this.MaxRadius * this.MaxRadius) && !(x < this.MinRadius * this.MinRadius))
                        {
                            switch (this.GravityType)
                            {
                                case GravityType.Linear:
                                    force = this.Strength / FP.Sqrt(x) * body1.Mass * tsVector2;
                                    break;
                                case GravityType.DistanceSquared:
                                    force = this.Strength / x * body1.Mass * tsVector2;
                                    break;
                            }
                            body1.ApplyForce(ref force);
                        }
                    }
                }
            }
        }

        public void AddBody(Body body)
        {
            this.Bodies.Add(body);
        }

        public void AddPoint(TSVector2 point)
        {
            this.Points.Add(point);
        }
    }
}
