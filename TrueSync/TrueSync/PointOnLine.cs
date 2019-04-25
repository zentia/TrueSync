namespace TrueSync
{
    using System;

    public class PointOnLine : Constraint
    {
        private FP accumulatedImpulse;
        private FP bias;
        private FP biasFactor;
        private FP effectiveMass;
        private TSVector[] jacobian;
        private TSVector lineNormal;
        private TSVector localAnchor1;
        private TSVector localAnchor2;
        private TSVector r1;
        private TSVector r2;
        private FP softness;
        private FP softnessOverDt;

        public PointOnLine(RigidBody body1, RigidBody body2, TSVector lineStartPointBody1, TSVector pointBody2) : base(body1, body2)
        {
            this.biasFactor = FP.Half;
            this.softness = FP.Zero;
            this.effectiveMass = FP.Zero;
            this.accumulatedImpulse = FP.Zero;
            this.jacobian = new TSVector[4];
            TSVector.Subtract(ref lineStartPointBody1, ref body1.position, out this.localAnchor1);
            TSVector.Subtract(ref pointBody2, ref body2.position, out this.localAnchor2);
            TSVector.Transform(ref this.localAnchor1, ref body1.invOrientation, out this.localAnchor1);
            TSVector.Transform(ref this.localAnchor2, ref body2.invOrientation, out this.localAnchor2);
            this.lineNormal = TSVector.Normalize(lineStartPointBody1 - pointBody2);
        }

        public override void DebugDraw(IDebugDrawer drawer)
        {
            drawer.DrawLine(base.body1.position + this.r1, (base.body1.position + this.r1) + (TSVector.Transform(this.lineNormal, base.body1.orientation) * 100));
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
            TSVector vector4 = TSVector.Transform(this.lineNormal, base.body1.orientation);
            vector4.Normalize();
            TSVector vector5 = (vector - vector2) % vector4;
            if (vector5.sqrMagnitude != FP.Zero)
            {
                vector5.Normalize();
            }
            vector5 = vector5 % vector4;
            this.jacobian[0] = vector5;
            this.jacobian[1] = ((this.r1 + vector2) - vector) % vector5;
            this.jacobian[2] = (TSVector) (-FP.One * vector5);
            this.jacobian[3] = (TSVector) ((-FP.One * this.r2) % vector5);
            this.effectiveMass = ((base.body1.inverseMass + base.body2.inverseMass) + (TSVector.Transform(this.jacobian[1], base.body1.invInertiaWorld) * this.jacobian[1])) + (TSVector.Transform(this.jacobian[3], base.body2.invInertiaWorld) * this.jacobian[3]);
            this.softnessOverDt = this.softness / timestep;
            this.effectiveMass += this.softnessOverDt;
            if (this.effectiveMass != 0)
            {
                this.effectiveMass = FP.One / this.effectiveMass;
            }
            TSVector vector6 = vector4 % (vector2 - vector);
            this.bias = (-vector6.magnitude * this.biasFactor) * (FP.One / timestep);
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

