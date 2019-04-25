namespace TrueSync
{
    using System;

    public class PointPointDistance : Constraint
    {
        private FP accumulatedImpulse;
        private DistanceBehavior behavior;
        private FP bias;
        private FP biasFactor;
        private FP distance;
        private FP effectiveMass;
        private TSVector[] jacobian;
        private TSVector localAnchor1;
        private TSVector localAnchor2;
        private TSVector r1;
        private TSVector r2;
        private bool skipConstraint;
        private FP softness;
        private FP softnessOverDt;

        public PointPointDistance(RigidBody body1, RigidBody body2, TSVector anchor1, TSVector anchor2) : base(body1, body2)
        {
            this.biasFactor = FP.EN1;
            this.softness = FP.EN2;
            this.behavior = DistanceBehavior.LimitDistance;
            this.effectiveMass = FP.Zero;
            this.accumulatedImpulse = FP.Zero;
            this.jacobian = new TSVector[4];
            this.skipConstraint = false;
            TSVector.Subtract(ref anchor1, ref body1.position, out this.localAnchor1);
            TSVector.Subtract(ref anchor2, ref body2.position, out this.localAnchor2);
            TSVector.Transform(ref this.localAnchor1, ref body1.invOrientation, out this.localAnchor1);
            TSVector.Transform(ref this.localAnchor2, ref body2.invOrientation, out this.localAnchor2);
            TSVector vector = anchor1 - anchor2;
            this.distance = vector.magnitude;
        }

        public override void DebugDraw(IDebugDrawer drawer)
        {
            drawer.DrawLine(base.body1.position + this.r1, base.body2.position + this.r2);
        }

        public override void Iterate()
        {
            if (!this.skipConstraint)
            {
                FP fp = (FP) ((((base.body1.linearVelocity * this.jacobian[0]) + (base.body1.angularVelocity * this.jacobian[1])) + (base.body2.linearVelocity * this.jacobian[2])) + (base.body2.angularVelocity * this.jacobian[3]));
                FP fp2 = this.accumulatedImpulse * this.softnessOverDt;
                FP fp3 = -this.effectiveMass * ((fp + this.bias) + fp2);
                if (this.behavior == DistanceBehavior.LimitMinimumDistance)
                {
                    FP accumulatedImpulse = this.accumulatedImpulse;
                    this.accumulatedImpulse = TSMath.Max(this.accumulatedImpulse + fp3, 0);
                    fp3 = this.accumulatedImpulse - accumulatedImpulse;
                }
                else if (this.behavior == DistanceBehavior.LimitMaximumDistance)
                {
                    FP fp5 = this.accumulatedImpulse;
                    this.accumulatedImpulse = TSMath.Min(this.accumulatedImpulse + fp3, 0);
                    fp3 = this.accumulatedImpulse - fp5;
                }
                else
                {
                    this.accumulatedImpulse += fp3;
                }
                if (!base.body1.isStatic)
                {
                    base.body1.linearVelocity += (base.body1.inverseMass * fp3) * this.jacobian[0];
                    base.body1.angularVelocity += TSVector.Transform((TSVector) (fp3 * this.jacobian[1]), base.body1.invInertiaWorld);
                }
                if (!base.body2.isStatic)
                {
                    base.body2.linearVelocity += (base.body2.inverseMass * fp3) * this.jacobian[2];
                    base.body2.angularVelocity += TSVector.Transform((TSVector) (fp3 * this.jacobian[3]), base.body2.invInertiaWorld);
                }
            }
        }

        public override void PrepareForIteration(FP timestep)
        {
            TSVector vector;
            TSVector vector2;
            TSVector vector3;
            TSVector.Transform(ref this.localAnchor1, ref base.body1.orientation, out this.r1);
            TSVector.Transform(ref this.localAnchor2, ref base.body2.orientation, out this.r2);
            TSVector.Add(ref base.body1.position, ref this.r1, out vector);
            TSVector.Add(ref base.body2.position, ref this.r2, out vector2);
            TSVector.Subtract(ref vector2, ref vector, out vector3);
            FP fp = vector3.magnitude - this.distance;
            if ((this.behavior == DistanceBehavior.LimitMaximumDistance) && (fp <= FP.Zero))
            {
                this.skipConstraint = true;
            }
            else if ((this.behavior == DistanceBehavior.LimitMinimumDistance) && (fp >= FP.Zero))
            {
                this.skipConstraint = true;
            }
            else
            {
                this.skipConstraint = false;
                TSVector vector4 = vector2 - vector;
                if (vector4.sqrMagnitude != FP.Zero)
                {
                    vector4.Normalize();
                }
                this.jacobian[0] = (TSVector) (-FP.One * vector4);
                this.jacobian[1] = (TSVector) (-FP.One * (this.r1 % vector4));
                this.jacobian[2] = (TSVector) (FP.One * vector4);
                this.jacobian[3] = this.r2 % vector4;
                this.effectiveMass = ((base.body1.inverseMass + base.body2.inverseMass) + (TSVector.Transform(this.jacobian[1], base.body1.invInertiaWorld) * this.jacobian[1])) + (TSVector.Transform(this.jacobian[3], base.body2.invInertiaWorld) * this.jacobian[3]);
                this.softnessOverDt = this.softness / timestep;
                this.effectiveMass += this.softnessOverDt;
                this.effectiveMass = FP.One / this.effectiveMass;
                this.bias = (fp * this.biasFactor) * (FP.One / timestep);
                if (!base.body1.isStatic)
                {
                    base.body1.linearVelocity += (base.body1.inverseMass * this.accumulatedImpulse) * this.jacobian[0];
                    base.body1.angularVelocity += TSVector.Transform((TSVector) (this.accumulatedImpulse * this.jacobian[1]), base.body1.invInertiaWorld);
                }
                if (!base.body2.isStatic)
                {
                    base.body2.linearVelocity += (base.body2.inverseMass * this.accumulatedImpulse) * this.jacobian[2];
                    base.body2.angularVelocity += TSVector.Transform((TSVector) (this.accumulatedImpulse * this.jacobian[3]), base.body2.invInertiaWorld);
                }
            }
        }

        public FP AppliedImpulse
        {
            get
            {
                return this.accumulatedImpulse;
            }
        }

        public DistanceBehavior Behavior
        {
            get
            {
                return this.behavior;
            }
            set
            {
                this.behavior = value;
            }
        }

        public FP BiasFactor
        {
            get
            {
                return this.biasFactor;
            }
            set
            {
                this.biasFactor = value;
            }
        }

        public FP Distance
        {
            get
            {
                return this.distance;
            }
            set
            {
                this.distance = value;
            }
        }

        public TSVector LocalAnchor1
        {
            get
            {
                return this.localAnchor1;
            }
            set
            {
                this.localAnchor1 = value;
            }
        }

        public TSVector LocalAnchor2
        {
            get
            {
                return this.localAnchor2;
            }
            set
            {
                this.localAnchor2 = value;
            }
        }

        public FP Softness
        {
            get
            {
                return this.softness;
            }
            set
            {
                this.softness = value;
            }
        }

        public enum DistanceBehavior
        {
            LimitDistance,
            LimitMaximumDistance,
            LimitMinimumDistance
        }
    }
}

