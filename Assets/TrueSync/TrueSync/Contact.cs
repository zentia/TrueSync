namespace TrueSync
{
    using System;

    public class Contact : IConstraint
    {
        public FP accumulatedNormalImpulse = FP.Zero;
        public FP accumulatedTangentImpulse = FP.Zero;
        public RigidBody body1;
        public bool body1IsMassPoint;
        public RigidBody body2;
        public bool body2IsMassPoint;
        public FP dynamicFriction;
        public FP friction = FP.Zero;
        public FP initialPen = FP.Zero;
        public FP lastTimeStep = FP.PositiveInfinity;
        public FP lostSpeculativeBounce = FP.Zero;
        public FP massNormal = FP.Zero;
        public FP massTangent = FP.Zero;
        public bool newContact = false;
        public TSVector normal;
        public TSVector p1;
        public TSVector p2;
        public FP penetration = FP.Zero;
        public static readonly ResourcePool<Contact> Pool = new ResourcePool<Contact>();
        public TSVector realRelPos1;
        public TSVector realRelPos2;
        public TSVector relativePos1;
        public TSVector relativePos2;
        public FP restitution;
        public FP restitutionBias = FP.Zero;
        public ContactSettings settings;
        public FP speculativeVelocity = FP.Zero;
        public FP staticFriction;
        public TSVector tangent;
        public bool treatBody1AsStatic = false;
        public bool treatBody2AsStatic = false;

        public void ApplyImpulse(ref TSVector impulse)
        {
            if (!this.treatBody1AsStatic)
            {
                this.body1.linearVelocity.x -= impulse.x * this.body1.inverseMass;
                this.body1.linearVelocity.y -= impulse.y * this.body1.inverseMass;
                this.body1.linearVelocity.z -= impulse.z * this.body1.inverseMass;
                FP fp = (this.relativePos1.y * impulse.z) - (this.relativePos1.z * impulse.y);
                FP fp2 = (this.relativePos1.z * impulse.x) - (this.relativePos1.x * impulse.z);
                FP fp3 = (this.relativePos1.x * impulse.y) - (this.relativePos1.y * impulse.x);
                FP fp4 = ((fp * this.body1.invInertiaWorld.M11) + (fp2 * this.body1.invInertiaWorld.M21)) + (fp3 * this.body1.invInertiaWorld.M31);
                FP fp5 = ((fp * this.body1.invInertiaWorld.M12) + (fp2 * this.body1.invInertiaWorld.M22)) + (fp3 * this.body1.invInertiaWorld.M32);
                FP fp6 = ((fp * this.body1.invInertiaWorld.M13) + (fp2 * this.body1.invInertiaWorld.M23)) + (fp3 * this.body1.invInertiaWorld.M33);
                this.body1.angularVelocity.x -= fp4;
                this.body1.angularVelocity.y -= fp5;
                this.body1.angularVelocity.z -= fp6;
            }
            if (!this.treatBody2AsStatic)
            {
                this.body2.linearVelocity.x += impulse.x * this.body2.inverseMass;
                this.body2.linearVelocity.y += impulse.y * this.body2.inverseMass;
                this.body2.linearVelocity.z += impulse.z * this.body2.inverseMass;
                FP fp7 = (this.relativePos2.y * impulse.z) - (this.relativePos2.z * impulse.y);
                FP fp8 = (this.relativePos2.z * impulse.x) - (this.relativePos2.x * impulse.z);
                FP fp9 = (this.relativePos2.x * impulse.y) - (this.relativePos2.y * impulse.x);
                FP fp10 = ((fp7 * this.body2.invInertiaWorld.M11) + (fp8 * this.body2.invInertiaWorld.M21)) + (fp9 * this.body2.invInertiaWorld.M31);
                FP fp11 = ((fp7 * this.body2.invInertiaWorld.M12) + (fp8 * this.body2.invInertiaWorld.M22)) + (fp9 * this.body2.invInertiaWorld.M32);
                FP fp12 = ((fp7 * this.body2.invInertiaWorld.M13) + (fp8 * this.body2.invInertiaWorld.M23)) + (fp9 * this.body2.invInertiaWorld.M33);
                this.body2.angularVelocity.x += fp10;
                this.body2.angularVelocity.y += fp11;
                this.body2.angularVelocity.z += fp12;
            }
        }

        public void ApplyImpulse(TSVector impulse)
        {
            if (!this.treatBody1AsStatic)
            {
                this.body1.linearVelocity.x -= impulse.x * this.body1.inverseMass;
                this.body1.linearVelocity.y -= impulse.y * this.body1.inverseMass;
                this.body1.linearVelocity.z -= impulse.z * this.body1.inverseMass;
                FP fp = (this.relativePos1.y * impulse.z) - (this.relativePos1.z * impulse.y);
                FP fp2 = (this.relativePos1.z * impulse.x) - (this.relativePos1.x * impulse.z);
                FP fp3 = (this.relativePos1.x * impulse.y) - (this.relativePos1.y * impulse.x);
                FP fp4 = ((fp * this.body1.invInertiaWorld.M11) + (fp2 * this.body1.invInertiaWorld.M21)) + (fp3 * this.body1.invInertiaWorld.M31);
                FP fp5 = ((fp * this.body1.invInertiaWorld.M12) + (fp2 * this.body1.invInertiaWorld.M22)) + (fp3 * this.body1.invInertiaWorld.M32);
                FP fp6 = ((fp * this.body1.invInertiaWorld.M13) + (fp2 * this.body1.invInertiaWorld.M23)) + (fp3 * this.body1.invInertiaWorld.M33);
                this.body1.angularVelocity.x -= fp4;
                this.body1.angularVelocity.y -= fp5;
                this.body1.angularVelocity.z -= fp6;
            }
            if (!this.treatBody2AsStatic)
            {
                this.body2.linearVelocity.x += impulse.x * this.body2.inverseMass;
                this.body2.linearVelocity.y += impulse.y * this.body2.inverseMass;
                this.body2.linearVelocity.z += impulse.z * this.body2.inverseMass;
                FP fp7 = (this.relativePos2.y * impulse.z) - (this.relativePos2.z * impulse.y);
                FP fp8 = (this.relativePos2.z * impulse.x) - (this.relativePos2.x * impulse.z);
                FP fp9 = (this.relativePos2.x * impulse.y) - (this.relativePos2.y * impulse.x);
                FP fp10 = ((fp7 * this.body2.invInertiaWorld.M11) + (fp8 * this.body2.invInertiaWorld.M21)) + (fp9 * this.body2.invInertiaWorld.M31);
                FP fp11 = ((fp7 * this.body2.invInertiaWorld.M12) + (fp8 * this.body2.invInertiaWorld.M22)) + (fp9 * this.body2.invInertiaWorld.M32);
                FP fp12 = ((fp7 * this.body2.invInertiaWorld.M13) + (fp8 * this.body2.invInertiaWorld.M23)) + (fp9 * this.body2.invInertiaWorld.M33);
                this.body2.angularVelocity.x += fp10;
                this.body2.angularVelocity.y += fp11;
                this.body2.angularVelocity.z += fp12;
            }
        }

        public TSVector CalculateRelativeVelocity()
        {
            TSVector vector;
            FP fp = ((this.body2.angularVelocity.y * this.relativePos2.z) - (this.body2.angularVelocity.z * this.relativePos2.y)) + this.body2.linearVelocity.x;
            FP fp2 = ((this.body2.angularVelocity.z * this.relativePos2.x) - (this.body2.angularVelocity.x * this.relativePos2.z)) + this.body2.linearVelocity.y;
            FP fp3 = ((this.body2.angularVelocity.x * this.relativePos2.y) - (this.body2.angularVelocity.y * this.relativePos2.x)) + this.body2.linearVelocity.z;
            vector.x = ((fp - (this.body1.angularVelocity.y * this.relativePos1.z)) + (this.body1.angularVelocity.z * this.relativePos1.y)) - this.body1.linearVelocity.x;
            vector.y = ((fp2 - (this.body1.angularVelocity.z * this.relativePos1.x)) + (this.body1.angularVelocity.x * this.relativePos1.z)) - this.body1.linearVelocity.y;
            vector.z = ((fp3 - (this.body1.angularVelocity.x * this.relativePos1.y)) + (this.body1.angularVelocity.y * this.relativePos1.x)) - this.body1.linearVelocity.z;
            return vector;
        }

        public void Initialize(RigidBody body1, RigidBody body2, ref TSVector point1, ref TSVector point2, ref TSVector n, FP penetration, bool newContact, ContactSettings settings)
        {
            this.body1 = body1;
            this.body2 = body2;
            this.normal = n;
            this.normal.Normalize();
            this.p1 = point1;
            this.p2 = point2;
            this.newContact = newContact;
            TSVector.Subtract(ref this.p1, ref body1.position, out this.relativePos1);
            TSVector.Subtract(ref this.p2, ref body2.position, out this.relativePos2);
            TSVector.Transform(ref this.relativePos1, ref body1.invOrientation, out this.realRelPos1);
            TSVector.Transform(ref this.relativePos2, ref body2.invOrientation, out this.realRelPos2);
            this.initialPen = penetration;
            this.penetration = penetration;
            this.body1IsMassPoint = body1.isParticle;
            this.body2IsMassPoint = body2.isParticle;
            if (newContact)
            {
                this.treatBody1AsStatic = body1.isStatic;
                this.treatBody2AsStatic = body2.isStatic;
                this.accumulatedNormalImpulse = FP.Zero;
                this.accumulatedTangentImpulse = FP.Zero;
                this.lostSpeculativeBounce = FP.Zero;
                switch (settings.MaterialCoefficientMixing)
                {
                    case ContactSettings.MaterialCoefficientMixingType.TakeMaximum:
                        this.staticFriction = TSMath.Max(body1.material.staticFriction, body2.material.staticFriction);
                        this.dynamicFriction = TSMath.Max(body1.material.kineticFriction, body2.material.kineticFriction);
                        this.restitution = TSMath.Max(body1.material.restitution, body2.material.restitution);
                        break;

                    case ContactSettings.MaterialCoefficientMixingType.TakeMinimum:
                        this.staticFriction = TSMath.Min(body1.material.staticFriction, body2.material.staticFriction);
                        this.dynamicFriction = TSMath.Min(body1.material.kineticFriction, body2.material.kineticFriction);
                        this.restitution = TSMath.Min(body1.material.restitution, body2.material.restitution);
                        break;

                    case ContactSettings.MaterialCoefficientMixingType.UseAverage:
                        this.staticFriction = (body1.material.staticFriction + body2.material.staticFriction) / (2 * FP.One);
                        this.dynamicFriction = (body1.material.kineticFriction + body2.material.kineticFriction) / (2 * FP.One);
                        this.restitution = (body1.material.restitution + body2.material.restitution) / (2 * FP.One);
                        break;
                }
            }
            this.settings = settings;
        }

        public void Iterate()
        {
            if (!this.treatBody1AsStatic || !this.treatBody2AsStatic)
            {
                FP fp = this.body2.linearVelocity.x - this.body1.linearVelocity.x;
                FP fp2 = this.body2.linearVelocity.y - this.body1.linearVelocity.y;
                FP fp3 = this.body2.linearVelocity.z - this.body1.linearVelocity.z;
                if (!this.body1IsMassPoint)
                {
                    fp = (fp - (this.body1.angularVelocity.y * this.relativePos1.z)) + (this.body1.angularVelocity.z * this.relativePos1.y);
                    fp2 = (fp2 - (this.body1.angularVelocity.z * this.relativePos1.x)) + (this.body1.angularVelocity.x * this.relativePos1.z);
                    fp3 = (fp3 - (this.body1.angularVelocity.x * this.relativePos1.y)) + (this.body1.angularVelocity.y * this.relativePos1.x);
                }
                if (!this.body2IsMassPoint)
                {
                    fp = (fp + (this.body2.angularVelocity.y * this.relativePos2.z)) - (this.body2.angularVelocity.z * this.relativePos2.y);
                    fp2 = (fp2 + (this.body2.angularVelocity.z * this.relativePos2.x)) - (this.body2.angularVelocity.x * this.relativePos2.z);
                    fp3 = (fp3 + (this.body2.angularVelocity.x * this.relativePos2.y)) - (this.body2.angularVelocity.y * this.relativePos2.x);
                }
                if ((((fp * fp) + (fp2 * fp2)) + (fp3 * fp3)) >= (this.settings.minVelocity * this.settings.minVelocity))
                {
                    TSVector vector;
                    FP fp4 = ((this.normal.x * fp) + (this.normal.y * fp2)) + (this.normal.z * fp3);
                    FP fp5 = this.massNormal * ((-fp4 + this.restitutionBias) + this.speculativeVelocity);
                    FP accumulatedNormalImpulse = this.accumulatedNormalImpulse;
                    this.accumulatedNormalImpulse = accumulatedNormalImpulse + fp5;
                    if (this.accumulatedNormalImpulse < FP.Zero)
                    {
                        this.accumulatedNormalImpulse = FP.Zero;
                    }
                    fp5 = this.accumulatedNormalImpulse - accumulatedNormalImpulse;
                    FP fp7 = ((fp * this.tangent.x) + (fp2 * this.tangent.y)) + (fp3 * this.tangent.z);
                    FP fp8 = this.friction * this.accumulatedNormalImpulse;
                    FP fp9 = this.massTangent * -fp7;
                    FP accumulatedTangentImpulse = this.accumulatedTangentImpulse;
                    this.accumulatedTangentImpulse = accumulatedTangentImpulse + fp9;
                    if (this.accumulatedTangentImpulse < -fp8)
                    {
                        this.accumulatedTangentImpulse = -fp8;
                    }
                    else if (this.accumulatedTangentImpulse > fp8)
                    {
                        this.accumulatedTangentImpulse = fp8;
                    }
                    fp9 = this.accumulatedTangentImpulse - accumulatedTangentImpulse;
                    vector.x = (this.normal.x * fp5) + (this.tangent.x * fp9);
                    vector.y = (this.normal.y * fp5) + (this.tangent.y * fp9);
                    vector.z = (this.normal.z * fp5) + (this.tangent.z * fp9);
                    if (!this.treatBody1AsStatic)
                    {
                        this.body1.linearVelocity.x -= vector.x * this.body1.inverseMass;
                        this.body1.linearVelocity.y -= vector.y * this.body1.inverseMass;
                        this.body1.linearVelocity.z -= vector.z * this.body1.inverseMass;
                        if (!this.body1IsMassPoint)
                        {
                            FP fp11 = (this.relativePos1.y * vector.z) - (this.relativePos1.z * vector.y);
                            FP fp12 = (this.relativePos1.z * vector.x) - (this.relativePos1.x * vector.z);
                            FP fp13 = (this.relativePos1.x * vector.y) - (this.relativePos1.y * vector.x);
                            FP fp14 = ((fp11 * this.body1.invInertiaWorld.M11) + (fp12 * this.body1.invInertiaWorld.M21)) + (fp13 * this.body1.invInertiaWorld.M31);
                            FP fp15 = ((fp11 * this.body1.invInertiaWorld.M12) + (fp12 * this.body1.invInertiaWorld.M22)) + (fp13 * this.body1.invInertiaWorld.M32);
                            FP fp16 = ((fp11 * this.body1.invInertiaWorld.M13) + (fp12 * this.body1.invInertiaWorld.M23)) + (fp13 * this.body1.invInertiaWorld.M33);
                            this.body1.angularVelocity.x -= fp14;
                            this.body1.angularVelocity.y -= fp15;
                            this.body1.angularVelocity.z -= fp16;
                        }
                    }
                    if (!this.treatBody2AsStatic)
                    {
                        this.body2.linearVelocity.x += vector.x * this.body2.inverseMass;
                        this.body2.linearVelocity.y += vector.y * this.body2.inverseMass;
                        this.body2.linearVelocity.z += vector.z * this.body2.inverseMass;
                        if (!this.body2IsMassPoint)
                        {
                            FP fp17 = (this.relativePos2.y * vector.z) - (this.relativePos2.z * vector.y);
                            FP fp18 = (this.relativePos2.z * vector.x) - (this.relativePos2.x * vector.z);
                            FP fp19 = (this.relativePos2.x * vector.y) - (this.relativePos2.y * vector.x);
                            FP fp20 = ((fp17 * this.body2.invInertiaWorld.M11) + (fp18 * this.body2.invInertiaWorld.M21)) + (fp19 * this.body2.invInertiaWorld.M31);
                            FP fp21 = ((fp17 * this.body2.invInertiaWorld.M12) + (fp18 * this.body2.invInertiaWorld.M22)) + (fp19 * this.body2.invInertiaWorld.M32);
                            FP fp22 = ((fp17 * this.body2.invInertiaWorld.M13) + (fp18 * this.body2.invInertiaWorld.M23)) + (fp19 * this.body2.invInertiaWorld.M33);
                            this.body2.angularVelocity.x += fp20;
                            this.body2.angularVelocity.y += fp21;
                            this.body2.angularVelocity.z += fp22;
                        }
                    }
                }
            }
        }

        public void PrepareForIteration(FP timestep)
        {
            TSVector vector3;
            FP fp = ((this.body2.angularVelocity.y * this.relativePos2.z) - (this.body2.angularVelocity.z * this.relativePos2.y)) + this.body2.linearVelocity.x;
            FP fp2 = ((this.body2.angularVelocity.z * this.relativePos2.x) - (this.body2.angularVelocity.x * this.relativePos2.z)) + this.body2.linearVelocity.y;
            FP fp3 = ((this.body2.angularVelocity.x * this.relativePos2.y) - (this.body2.angularVelocity.y * this.relativePos2.x)) + this.body2.linearVelocity.z;
            fp = ((fp - (this.body1.angularVelocity.y * this.relativePos1.z)) + (this.body1.angularVelocity.z * this.relativePos1.y)) - this.body1.linearVelocity.x;
            fp2 = ((fp2 - (this.body1.angularVelocity.z * this.relativePos1.x)) + (this.body1.angularVelocity.x * this.relativePos1.z)) - this.body1.linearVelocity.y;
            fp3 = ((fp3 - (this.body1.angularVelocity.x * this.relativePos1.y)) + (this.body1.angularVelocity.y * this.relativePos1.x)) - this.body1.linearVelocity.z;
            FP zero = FP.Zero;
            TSVector vector = TSVector.zero;
            if (!this.treatBody1AsStatic)
            {
                zero += this.body1.inverseMass;
                if (!this.body1IsMassPoint)
                {
                    vector.x = (this.relativePos1.y * this.normal.z) - (this.relativePos1.z * this.normal.y);
                    vector.y = (this.relativePos1.z * this.normal.x) - (this.relativePos1.x * this.normal.z);
                    vector.z = (this.relativePos1.x * this.normal.y) - (this.relativePos1.y * this.normal.x);
                    FP fp9 = ((vector.x * this.body1.invInertiaWorld.M11) + (vector.y * this.body1.invInertiaWorld.M21)) + (vector.z * this.body1.invInertiaWorld.M31);
                    FP fp10 = ((vector.x * this.body1.invInertiaWorld.M12) + (vector.y * this.body1.invInertiaWorld.M22)) + (vector.z * this.body1.invInertiaWorld.M32);
                    FP fp11 = ((vector.x * this.body1.invInertiaWorld.M13) + (vector.y * this.body1.invInertiaWorld.M23)) + (vector.z * this.body1.invInertiaWorld.M33);
                    vector.x = fp9;
                    vector.y = fp10;
                    vector.z = fp11;
                    fp9 = (vector.y * this.relativePos1.z) - (vector.z * this.relativePos1.y);
                    fp10 = (vector.z * this.relativePos1.x) - (vector.x * this.relativePos1.z);
                    fp11 = (vector.x * this.relativePos1.y) - (vector.y * this.relativePos1.x);
                    vector.x = fp9;
                    vector.y = fp10;
                    vector.z = fp11;
                }
            }
            TSVector vector2 = TSVector.zero;
            if (!this.treatBody2AsStatic)
            {
                zero += this.body2.inverseMass;
                if (!this.body2IsMassPoint)
                {
                    vector2.x = (this.relativePos2.y * this.normal.z) - (this.relativePos2.z * this.normal.y);
                    vector2.y = (this.relativePos2.z * this.normal.x) - (this.relativePos2.x * this.normal.z);
                    vector2.z = (this.relativePos2.x * this.normal.y) - (this.relativePos2.y * this.normal.x);
                    FP fp12 = ((vector2.x * this.body2.invInertiaWorld.M11) + (vector2.y * this.body2.invInertiaWorld.M21)) + (vector2.z * this.body2.invInertiaWorld.M31);
                    FP fp13 = ((vector2.x * this.body2.invInertiaWorld.M12) + (vector2.y * this.body2.invInertiaWorld.M22)) + (vector2.z * this.body2.invInertiaWorld.M32);
                    FP fp14 = ((vector2.x * this.body2.invInertiaWorld.M13) + (vector2.y * this.body2.invInertiaWorld.M23)) + (vector2.z * this.body2.invInertiaWorld.M33);
                    vector2.x = fp12;
                    vector2.y = fp13;
                    vector2.z = fp14;
                    fp12 = (vector2.y * this.relativePos2.z) - (vector2.z * this.relativePos2.y);
                    fp13 = (vector2.z * this.relativePos2.x) - (vector2.x * this.relativePos2.z);
                    fp14 = (vector2.x * this.relativePos2.y) - (vector2.y * this.relativePos2.x);
                    vector2.x = fp12;
                    vector2.y = fp13;
                    vector2.z = fp14;
                }
            }
            if (!this.treatBody1AsStatic)
            {
                zero += ((vector.x * this.normal.x) + (vector.y * this.normal.y)) + (vector.z * this.normal.z);
            }
            if (!this.treatBody2AsStatic)
            {
                zero += ((vector2.x * this.normal.x) + (vector2.y * this.normal.y)) + (vector2.z * this.normal.z);
            }
            this.massNormal = FP.One / zero;
            FP x = ((fp * this.normal.x) + (fp2 * this.normal.y)) + (fp3 * this.normal.z);
            this.tangent.x = fp - (this.normal.x * x);
            this.tangent.y = fp2 - (this.normal.y * x);
            this.tangent.z = fp3 - (this.normal.z * x);
            x = ((this.tangent.x * this.tangent.x) + (this.tangent.y * this.tangent.y)) + (this.tangent.z * this.tangent.z);
            if (x != FP.Zero)
            {
                x = FP.Sqrt(x);
                this.tangent.x /= x;
                this.tangent.y /= x;
                this.tangent.z /= x;
            }
            FP fp6 = FP.Zero;
            if (this.treatBody1AsStatic)
            {
                vector.MakeZero();
            }
            else
            {
                fp6 += this.body1.inverseMass;
                if (!this.body1IsMassPoint)
                {
                    vector.x = (this.relativePos1.y * this.tangent.z) - (this.relativePos1.z * this.tangent.y);
                    vector.y = (this.relativePos1.z * this.tangent.x) - (this.relativePos1.x * this.tangent.z);
                    vector.z = (this.relativePos1.x * this.tangent.y) - (this.relativePos1.y * this.tangent.x);
                    FP fp15 = ((vector.x * this.body1.invInertiaWorld.M11) + (vector.y * this.body1.invInertiaWorld.M21)) + (vector.z * this.body1.invInertiaWorld.M31);
                    FP fp16 = ((vector.x * this.body1.invInertiaWorld.M12) + (vector.y * this.body1.invInertiaWorld.M22)) + (vector.z * this.body1.invInertiaWorld.M32);
                    FP fp17 = ((vector.x * this.body1.invInertiaWorld.M13) + (vector.y * this.body1.invInertiaWorld.M23)) + (vector.z * this.body1.invInertiaWorld.M33);
                    vector.x = fp15;
                    vector.y = fp16;
                    vector.z = fp17;
                    fp15 = (vector.y * this.relativePos1.z) - (vector.z * this.relativePos1.y);
                    fp16 = (vector.z * this.relativePos1.x) - (vector.x * this.relativePos1.z);
                    fp17 = (vector.x * this.relativePos1.y) - (vector.y * this.relativePos1.x);
                    vector.x = fp15;
                    vector.y = fp16;
                    vector.z = fp17;
                }
            }
            if (this.treatBody2AsStatic)
            {
                vector2.MakeZero();
            }
            else
            {
                fp6 += this.body2.inverseMass;
                if (!this.body2IsMassPoint)
                {
                    vector2.x = (this.relativePos2.y * this.tangent.z) - (this.relativePos2.z * this.tangent.y);
                    vector2.y = (this.relativePos2.z * this.tangent.x) - (this.relativePos2.x * this.tangent.z);
                    vector2.z = (this.relativePos2.x * this.tangent.y) - (this.relativePos2.y * this.tangent.x);
                    FP fp18 = ((vector2.x * this.body2.invInertiaWorld.M11) + (vector2.y * this.body2.invInertiaWorld.M21)) + (vector2.z * this.body2.invInertiaWorld.M31);
                    FP fp19 = ((vector2.x * this.body2.invInertiaWorld.M12) + (vector2.y * this.body2.invInertiaWorld.M22)) + (vector2.z * this.body2.invInertiaWorld.M32);
                    FP fp20 = ((vector2.x * this.body2.invInertiaWorld.M13) + (vector2.y * this.body2.invInertiaWorld.M23)) + (vector2.z * this.body2.invInertiaWorld.M33);
                    vector2.x = fp18;
                    vector2.y = fp19;
                    vector2.z = fp20;
                    fp18 = (vector2.y * this.relativePos2.z) - (vector2.z * this.relativePos2.y);
                    fp19 = (vector2.z * this.relativePos2.x) - (vector2.x * this.relativePos2.z);
                    fp20 = (vector2.x * this.relativePos2.y) - (vector2.y * this.relativePos2.x);
                    vector2.x = fp18;
                    vector2.y = fp19;
                    vector2.z = fp20;
                }
            }
            if (!this.treatBody1AsStatic)
            {
                fp6 += TSVector.Dot(ref vector, ref this.tangent);
            }
            if (!this.treatBody2AsStatic)
            {
                fp6 += TSVector.Dot(ref vector2, ref this.tangent);
            }
            this.massTangent = FP.One / fp6;
            this.restitutionBias = this.lostSpeculativeBounce;
            this.speculativeVelocity = FP.Zero;
            FP fp7 = ((this.normal.x * fp) + (this.normal.y * fp2)) + (this.normal.z * fp3);
            if (this.Penetration > this.settings.allowedPenetration)
            {
                this.restitutionBias = (this.settings.bias * (FP.One / timestep)) * TSMath.Max(FP.Zero, this.Penetration - this.settings.allowedPenetration);
                this.restitutionBias = TSMath.Clamp(this.restitutionBias, FP.Zero, this.settings.maximumBias);
            }
            FP fp8 = timestep / this.lastTimeStep;
            this.accumulatedNormalImpulse *= fp8;
            this.accumulatedTangentImpulse *= fp8;
            FP fp21 = -(((this.tangent.x * fp) + (this.tangent.y * fp2)) + (this.tangent.z * fp3));
            FP fp22 = this.massTangent * fp21;
            FP fp23 = -this.staticFriction * this.accumulatedNormalImpulse;
            if (fp22 < fp23)
            {
                this.friction = this.dynamicFriction;
            }
            else
            {
                this.friction = this.staticFriction;
            }
            this.restitutionBias = TSMath.Max(-this.restitution * fp7, this.restitutionBias);
            if (this.penetration < -this.settings.allowedPenetration)
            {
                this.speculativeVelocity = this.penetration / timestep;
                this.lostSpeculativeBounce = this.restitutionBias;
                this.restitutionBias = FP.Zero;
            }
            else
            {
                this.lostSpeculativeBounce = FP.Zero;
            }
            vector3.x = (this.normal.x * this.accumulatedNormalImpulse) + (this.tangent.x * this.accumulatedTangentImpulse);
            vector3.y = (this.normal.y * this.accumulatedNormalImpulse) + (this.tangent.y * this.accumulatedTangentImpulse);
            vector3.z = (this.normal.z * this.accumulatedNormalImpulse) + (this.tangent.z * this.accumulatedTangentImpulse);
            if (!this.treatBody1AsStatic)
            {
                this.body1.linearVelocity.x -= vector3.x * this.body1.inverseMass;
                this.body1.linearVelocity.y -= vector3.y * this.body1.inverseMass;
                this.body1.linearVelocity.z -= vector3.z * this.body1.inverseMass;
                if (!this.body1IsMassPoint)
                {
                    FP fp24 = (this.relativePos1.y * vector3.z) - (this.relativePos1.z * vector3.y);
                    FP fp25 = (this.relativePos1.z * vector3.x) - (this.relativePos1.x * vector3.z);
                    FP fp26 = (this.relativePos1.x * vector3.y) - (this.relativePos1.y * vector3.x);
                    FP fp27 = ((fp24 * this.body1.invInertiaWorld.M11) + (fp25 * this.body1.invInertiaWorld.M21)) + (fp26 * this.body1.invInertiaWorld.M31);
                    FP fp28 = ((fp24 * this.body1.invInertiaWorld.M12) + (fp25 * this.body1.invInertiaWorld.M22)) + (fp26 * this.body1.invInertiaWorld.M32);
                    FP fp29 = ((fp24 * this.body1.invInertiaWorld.M13) + (fp25 * this.body1.invInertiaWorld.M23)) + (fp26 * this.body1.invInertiaWorld.M33);
                    this.body1.angularVelocity.x -= fp27;
                    this.body1.angularVelocity.y -= fp28;
                    this.body1.angularVelocity.z -= fp29;
                }
            }
            if (!this.treatBody2AsStatic)
            {
                this.body2.linearVelocity.x += vector3.x * this.body2.inverseMass;
                this.body2.linearVelocity.y += vector3.y * this.body2.inverseMass;
                this.body2.linearVelocity.z += vector3.z * this.body2.inverseMass;
                if (!this.body2IsMassPoint)
                {
                    FP fp30 = (this.relativePos2.y * vector3.z) - (this.relativePos2.z * vector3.y);
                    FP fp31 = (this.relativePos2.z * vector3.x) - (this.relativePos2.x * vector3.z);
                    FP fp32 = (this.relativePos2.x * vector3.y) - (this.relativePos2.y * vector3.x);
                    FP fp33 = ((fp30 * this.body2.invInertiaWorld.M11) + (fp31 * this.body2.invInertiaWorld.M21)) + (fp32 * this.body2.invInertiaWorld.M31);
                    FP fp34 = ((fp30 * this.body2.invInertiaWorld.M12) + (fp31 * this.body2.invInertiaWorld.M22)) + (fp32 * this.body2.invInertiaWorld.M32);
                    FP fp35 = ((fp30 * this.body2.invInertiaWorld.M13) + (fp31 * this.body2.invInertiaWorld.M23)) + (fp32 * this.body2.invInertiaWorld.M33);
                    this.body2.angularVelocity.x += fp33;
                    this.body2.angularVelocity.y += fp34;
                    this.body2.angularVelocity.z += fp35;
                }
            }
            this.lastTimeStep = timestep;
            this.newContact = false;
        }

        public void TreatBodyAsStatic(RigidBodyIndex index)
        {
            if (index == RigidBodyIndex.RigidBody1)
            {
                this.treatBody1AsStatic = true;
            }
            else
            {
                this.treatBody2AsStatic = true;
            }
        }

        public void UpdatePosition()
        {
            TSVector vector;
            if (this.body1IsMassPoint)
            {
                TSVector.Add(ref this.realRelPos1, ref this.body1.position, out this.p1);
            }
            else
            {
                TSVector.Transform(ref this.realRelPos1, ref this.body1.orientation, out this.p1);
                TSVector.Add(ref this.p1, ref this.body1.position, out this.p1);
            }
            if (this.body2IsMassPoint)
            {
                TSVector.Add(ref this.realRelPos2, ref this.body2.position, out this.p2);
            }
            else
            {
                TSVector.Transform(ref this.realRelPos2, ref this.body2.orientation, out this.p2);
                TSVector.Add(ref this.p2, ref this.body2.position, out this.p2);
            }
            TSVector.Subtract(ref this.p1, ref this.p2, out vector);
            this.penetration = TSVector.Dot(ref vector, ref this.normal);
        }

        public FP AppliedNormalImpulse
        {
            get
            {
                return this.accumulatedNormalImpulse;
            }
        }

        public FP AppliedTangentImpulse
        {
            get
            {
                return this.accumulatedTangentImpulse;
            }
        }

        public RigidBody Body1
        {
            get
            {
                return this.body1;
            }
        }

        public RigidBody Body2
        {
            get
            {
                return this.body2;
            }
        }

        public FP DynamicFriction
        {
            get
            {
                return this.dynamicFriction;
            }
            set
            {
                this.dynamicFriction = value;
            }
        }

        public TSVector Normal
        {
            get
            {
                return this.normal;
            }
        }

        public FP Penetration
        {
            get
            {
                return this.penetration;
            }
        }

        public TSVector Position1
        {
            get
            {
                return this.p1;
            }
        }

        public TSVector Position2
        {
            get
            {
                return this.p2;
            }
        }

        public FP Restitution
        {
            get
            {
                return this.restitution;
            }
            set
            {
                this.restitution = value;
            }
        }

        public FP StaticFriction
        {
            get
            {
                return this.staticFriction;
            }
            set
            {
                this.staticFriction = value;
            }
        }

        public TSVector Tangent
        {
            get
            {
                return this.tangent;
            }
        }
    }
}

