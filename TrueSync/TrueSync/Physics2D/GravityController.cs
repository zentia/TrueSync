namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public class GravityController : Controller
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Body> <Bodies>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.GravityType <GravityType>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <MaxRadius>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <MinRadius>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<TSVector2> <Points>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Strength>k__BackingField;

        public GravityController(FP strength) : base(ControllerType.GravityController)
        {
            this.Strength = strength;
            this.MaxRadius = FP.MaxValue;
            this.GravityType = TrueSync.Physics2D.GravityType.DistanceSquared;
            this.Points = new List<TSVector2>();
            this.Bodies = new List<Body>();
        }

        public GravityController(FP strength, FP maxRadius, FP minRadius) : base(ControllerType.GravityController)
        {
            this.MinRadius = minRadius;
            this.MaxRadius = maxRadius;
            this.Strength = strength;
            this.GravityType = TrueSync.Physics2D.GravityType.DistanceSquared;
            this.Points = new List<TSVector2>();
            this.Bodies = new List<Body>();
        }

        public void AddBody(Body body)
        {
            this.Bodies.Add(body);
        }

        public void AddPoint(TSVector2 point)
        {
            this.Points.Add(point);
        }

        public override void Update(FP dt)
        {
            TSVector2 zero = TSVector2.zero;
            foreach (Body body in base.World.BodyList)
            {
                if (this.IsActiveOn(body))
                {
                    foreach (Body body2 in this.Bodies)
                    {
                        if (((body == body2) || (body.IsStatic && body2.IsStatic)) || !body2.Enabled)
                        {
                            continue;
                        }
                        TSVector2 vector2 = body2.Position - body.Position;
                        FP x = vector2.LengthSquared();
                        if (((x > Settings.Epsilon) && (x <= (this.MaxRadius * this.MaxRadius))) && (x >= (this.MinRadius * this.MinRadius)))
                        {
                            switch (this.GravityType)
                            {
                                case TrueSync.Physics2D.GravityType.Linear:
                                    zero = (TSVector2) ((((this.Strength / FP.Sqrt(x)) * body.Mass) * body2.Mass) * vector2);
                                    break;

                                case TrueSync.Physics2D.GravityType.DistanceSquared:
                                    zero = (TSVector2) ((((this.Strength / x) * body.Mass) * body2.Mass) * vector2);
                                    break;
                            }
                            body.ApplyForce(ref zero);
                        }
                    }
                    foreach (TSVector2 vector3 in this.Points)
                    {
                        TSVector2 vector4 = vector3 - body.Position;
                        FP fp2 = vector4.LengthSquared();
                        if (((fp2 > Settings.Epsilon) && (fp2 <= (this.MaxRadius * this.MaxRadius))) && (fp2 >= (this.MinRadius * this.MinRadius)))
                        {
                            switch (this.GravityType)
                            {
                                case TrueSync.Physics2D.GravityType.Linear:
                                    zero = (TSVector2) (((this.Strength / FP.Sqrt(fp2)) * body.Mass) * vector4);
                                    break;

                                case TrueSync.Physics2D.GravityType.DistanceSquared:
                                    zero = (TSVector2) (((this.Strength / fp2) * body.Mass) * vector4);
                                    break;
                            }
                            body.ApplyForce(ref zero);
                        }
                    }
                }
            }
        }

        public List<Body> Bodies { get; set; }

        public TrueSync.Physics2D.GravityType GravityType { get; set; }

        public FP MaxRadius { get; set; }

        public FP MinRadius { get; set; }

        public List<TSVector2> Points { get; set; }

        public FP Strength { get; set; }
    }
}

