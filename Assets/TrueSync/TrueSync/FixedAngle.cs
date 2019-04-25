namespace TrueSync
{
    using System;

    public class FixedAngle : Constraint
    {
        private TSVector accumulatedImpulse;
        private TSVector bias;
        private FP biasFactor;
        private TSMatrix effectiveMass;
        private TSMatrix initialOrientation1;
        private TSMatrix initialOrientation2;
        private FP softness;
        private FP softnessOverDt;

        public FixedAngle(RigidBody body1, RigidBody body2) : base(body1, body2)
        {
            this.biasFactor = 5 * FP.EN2;
            this.softness = FP.Zero;
            this.initialOrientation1 = body1.orientation;
            this.initialOrientation2 = body2.orientation;
        }

        public override void Iterate()
        {
            TSVector vector = base.body1.angularVelocity - base.body2.angularVelocity;
            TSVector vector2 = this.accumulatedImpulse * this.softnessOverDt;
            TSVector position = (TSVector) (-FP.One * TSVector.Transform((vector + this.bias) + vector2, this.effectiveMass));
            this.accumulatedImpulse += position;
            if (!base.body1.IsStatic)
            {
                base.body1.angularVelocity += TSVector.Transform(position, base.body1.invInertiaWorld);
            }
            if (!base.body2.IsStatic)
            {
                base.body2.angularVelocity += TSVector.Transform((TSVector) (-FP.One * position), base.body2.invInertiaWorld);
            }
        }

        public override void PrepareForIteration(FP timestep)
        {
            TSMatrix matrix;
            this.effectiveMass = base.body1.invInertiaWorld + base.body2.invInertiaWorld;
            this.softnessOverDt = this.softness / timestep;
            this.effectiveMass.M11 += this.softnessOverDt;
            this.effectiveMass.M22 += this.softnessOverDt;
            this.effectiveMass.M33 += this.softnessOverDt;
            TSMatrix.Inverse(ref this.effectiveMass, out this.effectiveMass);
            TSMatrix.Multiply(ref this.initialOrientation1, ref this.initialOrientation2, out matrix);
            TSMatrix.Transpose(ref matrix, out matrix);
            TSMatrix matrix2 = (matrix * base.body2.invOrientation) * base.body1.orientation;
            FP x = matrix2.M32 - matrix2.M23;
            FP y = matrix2.M13 - matrix2.M31;
            FP z = matrix2.M21 - matrix2.M12;
            FP fp4 = TSMath.Sqrt(((x * x) + (y * y)) + (z * z));
            FP fp5 = (matrix2.M11 + matrix2.M22) + matrix2.M33;
            FP fp6 = FP.Atan2(fp4, fp5 - 1);
            TSVector vector = new TSVector(x, y, z) * fp6;
            if (fp4 != FP.Zero)
            {
                vector *= FP.One / fp4;
            }
            this.bias = (vector * this.biasFactor) * (-FP.One / timestep);
            if (!base.body1.IsStatic)
            {
                base.body1.angularVelocity += TSVector.Transform(this.accumulatedImpulse, base.body1.invInertiaWorld);
            }
            if (!base.body2.IsStatic)
            {
                base.body2.angularVelocity += TSVector.Transform((TSVector) (-FP.One * this.accumulatedImpulse), base.body2.invInertiaWorld);
            }
        }

        public TSVector AppliedImpulse
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

        public TSMatrix InitialOrientationBody1
        {
            get
            {
                return this.initialOrientation1;
            }
            set
            {
                this.initialOrientation1 = value;
            }
        }

        public TSMatrix InitialOrientationBody2
        {
            get
            {
                return this.initialOrientation2;
            }
            set
            {
                this.initialOrientation2 = value;
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

