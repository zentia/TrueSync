namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    public class VelocityLimitController : Controller
    {
        private List<Body> _bodies;
        private FP _maxAngularSqared;
        private FP _maxAngularVelocity;
        private FP _maxLinearSqared;
        private FP _maxLinearVelocity;
        public bool LimitAngularVelocity;
        public bool LimitLinearVelocity;

        public VelocityLimitController() : base(ControllerType.VelocityLimitController)
        {
            this.LimitAngularVelocity = true;
            this.LimitLinearVelocity = true;
            this._bodies = new List<Body>();
            this.MaxLinearVelocity = Settings.MaxTranslation;
            this.MaxAngularVelocity = Settings.MaxRotation;
        }

        public VelocityLimitController(FP maxLinearVelocity, FP maxAngularVelocity) : base(ControllerType.VelocityLimitController)
        {
            this.LimitAngularVelocity = true;
            this.LimitLinearVelocity = true;
            this._bodies = new List<Body>();
            if ((maxLinearVelocity == 0) || (maxLinearVelocity == FP.MaxValue))
            {
                this.LimitLinearVelocity = false;
            }
            if ((maxAngularVelocity == 0) || (maxAngularVelocity == FP.MaxValue))
            {
                this.LimitAngularVelocity = false;
            }
            this.MaxLinearVelocity = maxLinearVelocity;
            this.MaxAngularVelocity = maxAngularVelocity;
        }

        public void AddBody(Body body)
        {
            this._bodies.Add(body);
        }

        public void RemoveBody(Body body)
        {
            this._bodies.Remove(body);
        }

        public override void Update(FP dt)
        {
            foreach (Body body in this._bodies)
            {
                if (this.IsActiveOn(body))
                {
                    if (this.LimitLinearVelocity)
                    {
                        FP fp = dt * body._linearVelocity.x;
                        FP fp2 = dt * body._linearVelocity.y;
                        FP x = (fp * fp) + (fp2 * fp2);
                        if (x > (dt * this._maxLinearSqared))
                        {
                            FP fp4 = FP.Sqrt(x);
                            FP fp5 = this._maxLinearVelocity / fp4;
                            body._linearVelocity.x *= fp5;
                            body._linearVelocity.y *= fp5;
                        }
                    }
                    if (this.LimitAngularVelocity)
                    {
                        FP fp6 = dt * body._angularVelocity;
                        if ((fp6 * fp6) > this._maxAngularSqared)
                        {
                            FP fp7 = this._maxAngularVelocity / FP.Abs(fp6);
                            body._angularVelocity *= fp7;
                        }
                    }
                }
            }
        }

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
    }
}

