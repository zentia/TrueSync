namespace TrueSync
{
    using System;

    public class PointOnPoint : Constraint
    {
        private FP accumulatedImpulse;
        private FP bias;
        private FP biasFactor;
        private FP effectiveMass;
        private TSVector[] jacobian;
        private TSVector localAnchor1;
        private TSVector localAnchor2;
        private TSVector r1;
        private TSVector r2;
        private FP softness;
        private FP softnessOverDt;

        public PointOnPoint(RigidBody body1, RigidBody body2, TSVector anchor) : base(body1, body2)
        {
            this.biasFactor = 5 * FP.EN2;
            this.softness = FP.EN2;
            this.effectiveMass = FP.Zero;
            this.accumulatedImpulse = FP.Zero;
            this.jacobian = new TSVector[4];
            TSVector.Subtract(ref anchor, ref body1.position, out this.localAnchor1);
            TSVector.Subtract(ref anchor, ref body2.position, out this.localAnchor2);
            TSVector.Transform(ref this.localAnchor1, ref body1.invOrientation, out this.localAnchor1);
            TSVector.Transform(ref this.localAnchor2, ref body2.invOrientation, out this.localAnchor2);
        }

        public override void DebugDraw(IDebugDrawer drawer)
        {
            drawer.DrawLine(base.body1.position, base.body1.position + this.r1);
            drawer.DrawLine(base.body2.position, base.body2.position + this.r2);
        }

        public override void Iterate()
        {
            FP fp = (FP) ((((base.body1.linearVelocity * this.jacobian[0]) + (base.body1.angularVelocity * this.jacobian[1])) + (base.body2.linearVelocity * this.jacobian[2])) + (base.body2.angularVelocity * this.jacobian[3]));
            FP fp2 = this.accumulatedImpulse * this.softnessOverDt;
            FP fp3 = -this.effectiveMass * ((fp + this.bias) + fp2);
            this.accumulatedImpulse += fp3;
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
            FP magnitude = vector3.magnitude;
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
            this.bias = (magnitude * this.biasFactor) * (FP.One / timestep);
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

        public FP AppliedImpulse
        {
            get
            {
                return this.accumulatedImpulse;
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
    }
}

