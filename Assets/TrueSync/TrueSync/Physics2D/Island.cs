namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using TrueSync;

    public class Island
    {
        internal ContactManager _contactManager;
        internal TrueSync.Physics2D.Contact[] _contacts;
        internal ContactSolver _contactSolver = new ContactSolver();
        internal TrueSync.Physics2D.Joint[] _joints;
        public Position[] _positions;
        public Velocity[] _velocities;
        private static readonly FP AngTolSqr = (Settings.AngularSleepTolerance * Settings.AngularSleepTolerance);
        public Body[] Bodies;
        public int BodyCapacity;
        public int BodyCount;
        public int ContactCapacity;
        public int ContactCount;
        public int JointCapacity;
        public int JointCount;
        private static readonly FP LinTolSqr = (Settings.LinearSleepTolerance * Settings.LinearSleepTolerance);

        public void Add(Body body)
        {
            Debug.Assert(this.BodyCount < this.BodyCapacity);
            body.IslandIndex = this.BodyCount;
            int bodyCount = this.BodyCount;
            this.BodyCount = bodyCount + 1;
            this.Bodies[bodyCount] = body;
        }

        public void Add(TrueSync.Physics2D.Contact contact)
        {
            Debug.Assert(this.ContactCount < this.ContactCapacity);
            int contactCount = this.ContactCount;
            this.ContactCount = contactCount + 1;
            this._contacts[contactCount] = contact;
        }

        public void Add(TrueSync.Physics2D.Joint joint)
        {
            Debug.Assert(this.JointCount < this.JointCapacity);
            int jointCount = this.JointCount;
            this.JointCount = jointCount + 1;
            this._joints[jointCount] = joint;
        }

        public void Clear()
        {
            this.BodyCount = 0;
            this.ContactCount = 0;
            this.JointCount = 0;
        }

        private void Report(ContactVelocityConstraint[] constraints)
        {
            if (this._contactManager != null)
            {
                for (int i = 0; i < this.ContactCount; i++)
                {
                    TrueSync.Physics2D.Contact contact = this._contacts[i];
                    if (contact.FixtureA.AfterCollision > null)
                    {
                        contact.FixtureA.AfterCollision(contact.FixtureA, contact.FixtureB, contact, constraints[i]);
                    }
                    if (contact.FixtureB.AfterCollision > null)
                    {
                        contact.FixtureB.AfterCollision(contact.FixtureB, contact.FixtureA, contact, constraints[i]);
                    }
                    if (this._contactManager.PostSolve > null)
                    {
                        this._contactManager.PostSolve(contact, constraints[i]);
                    }
                }
            }
        }

        public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
        {
            this.BodyCapacity = bodyCapacity;
            this.ContactCapacity = contactCapacity;
            this.JointCapacity = jointCapacity;
            this.BodyCount = 0;
            this.ContactCount = 0;
            this.JointCount = 0;
            this._contactManager = contactManager;
            if ((this.Bodies == null) || (this.Bodies.Length < bodyCapacity))
            {
                this.Bodies = new Body[bodyCapacity];
                this._velocities = new Velocity[bodyCapacity];
                this._positions = new Position[bodyCapacity];
            }
            if ((this._contacts == null) || (this._contacts.Length < contactCapacity))
            {
                this._contacts = new TrueSync.Physics2D.Contact[contactCapacity * 2];
            }
            if ((this._joints == null) || (this._joints.Length < jointCapacity))
            {
                this._joints = new TrueSync.Physics2D.Joint[jointCapacity * 2];
            }
        }

        public void Solve(ref TimeStep step, ref TSVector2 gravity)
        {
            FP dt = step.dt;
            for (int i = 0; i < this.BodyCount; i++)
            {
                Body body = this.Bodies[i];
                TSVector2 c = body._sweep.C;
                FP a = body._sweep.A;
                TSVector2 vector2 = body._linearVelocity;
                FP fp3 = body._angularVelocity;
                body._sweep.C0 = body._sweep.C;
                body._sweep.A0 = body._sweep.A;
                if (body.BodyType == BodyType.Dynamic)
                {
                    if (body.IgnoreGravity)
                    {
                        vector2 += dt * (body._invMass * body._force);
                    }
                    else
                    {
                        vector2 += dt * ((body.GravityScale * gravity) + (body._invMass * body._force));
                    }
                    fp3 += (dt * body._invI) * body._torque;
                    vector2 *= MathUtils.Clamp(1f - (dt * body.LinearDamping), 0f, 1f);
                    fp3 *= MathUtils.Clamp(1f - (dt * body.AngularDamping), 0f, 1f);
                }
                this._positions[i].c = c;
                this._positions[i].a = a;
                this._velocities[i].v = vector2;
                this._velocities[i].w = fp3;
            }
            SolverData data = new SolverData {
                step = step,
                positions = this._positions,
                velocities = this._velocities
            };
            this._contactSolver.Reset(step, this.ContactCount, this._contacts, this._positions, this._velocities, true);
            this._contactSolver.InitializeVelocityConstraints();
            this._contactSolver.WarmStart();
            for (int j = 0; j < this.JointCount; j++)
            {
                if (this._joints[j].Enabled)
                {
                    this._joints[j].InitVelocityConstraints(ref data);
                }
            }
            for (int k = 0; k < Settings.VelocityIterations; k++)
            {
                for (int num4 = 0; num4 < this.JointCount; num4++)
                {
                    TrueSync.Physics2D.Joint joint = this._joints[num4];
                    if (joint.Enabled)
                    {
                        joint.SolveVelocityConstraints(ref data);
                        joint.Validate(step.inv_dt);
                    }
                }
                this._contactSolver.SolveVelocityConstraints();
            }
            this._contactSolver.StoreImpulses();
            for (int m = 0; m < this.BodyCount; m++)
            {
                TSVector2 vector3 = this._positions[m].c;
                FP fp4 = this._positions[m].a;
                TSVector2 v = this._velocities[m].v;
                FP w = this._velocities[m].w;
                TSVector2 vector5 = (TSVector2) (dt * v);
                if (TSVector2.Dot(vector5, vector5) > Settings.MaxTranslationSquared)
                {
                    FP fp7 = Settings.MaxTranslation / vector5.magnitude;
                    v *= fp7;
                }
                FP fp6 = dt * w;
                if ((fp6 * fp6) > Settings.MaxRotationSquared)
                {
                    FP fp8 = Settings.MaxRotation / FP.Abs(fp6);
                    w *= fp8;
                }
                vector3 += dt * v;
                fp4 += dt * w;
                this._positions[m].c = vector3;
                this._positions[m].a = fp4;
                this._velocities[m].v = v;
                this._velocities[m].w = w;
            }
            bool flag = false;
            for (int n = 0; n < Settings.PositionIterations; n++)
            {
                bool flag14 = this._contactSolver.SolvePositionConstraints();
                bool flag15 = true;
                for (int num7 = 0; num7 < this.JointCount; num7++)
                {
                    TrueSync.Physics2D.Joint joint2 = this._joints[num7];
                    if (joint2.Enabled)
                    {
                        bool flag16 = joint2.SolvePositionConstraints(ref data);
                        flag15 &= flag16;
                    }
                }
                if (flag14 & flag15)
                {
                    flag = true;
                    break;
                }
            }
            for (int num8 = 0; num8 < this.BodyCount; num8++)
            {
                Body body2 = this.Bodies[num8];
                body2._sweep.C = this._positions[num8].c;
                body2._sweep.A = this._positions[num8].a;
                body2._linearVelocity = this._velocities[num8].v;
                body2._angularVelocity = this._velocities[num8].w;
                body2.SynchronizeTransform();
            }
            this.Report(this._contactSolver._velocityConstraints);
            if (Settings.AllowSleep)
            {
                FP maxFP = Settings.MaxFP;
                for (int num9 = 0; num9 < this.BodyCount; num9++)
                {
                    Body body3 = this.Bodies[num9];
                    if (body3.BodyType != BodyType.Static)
                    {
                        if ((!body3.SleepingAllowed || ((body3._angularVelocity * body3._angularVelocity) > AngTolSqr)) || (TSVector2.Dot(body3._linearVelocity, body3._linearVelocity) > LinTolSqr))
                        {
                            body3._sleepTime = 0f;
                            maxFP = 0f;
                        }
                        else
                        {
                            body3._sleepTime += dt;
                            maxFP = TSMath.Min(maxFP, body3._sleepTime);
                        }
                    }
                }
                if ((maxFP >= Settings.TimeToSleep) & flag)
                {
                    for (int num10 = 0; num10 < this.BodyCount; num10++)
                    {
                        Body body4 = this.Bodies[num10];
                        body4.Awake = false;
                    }
                }
            }
        }

        internal void SolveTOI(ref TimeStep subStep, int toiIndexA, int toiIndexB, bool warmstarting)
        {
            Debug.Assert(toiIndexA < this.BodyCount);
            Debug.Assert(toiIndexB < this.BodyCount);
            for (int i = 0; i < this.BodyCount; i++)
            {
                Body body = this.Bodies[i];
                this._positions[i].c = body._sweep.C;
                this._positions[i].a = body._sweep.A;
                this._velocities[i].v = body._linearVelocity;
                this._velocities[i].w = body._angularVelocity;
            }
            this._contactSolver.Reset(subStep, this.ContactCount, this._contacts, this._positions, this._velocities, warmstarting);
            for (int j = 0; j < Settings.TOIPositionIterations; j++)
            {
                if (this._contactSolver.SolveTOIPositionConstraints(toiIndexA, toiIndexB))
                {
                    break;
                }
            }
            this.Bodies[toiIndexA]._sweep.C0 = this._positions[toiIndexA].c;
            this.Bodies[toiIndexA]._sweep.A0 = this._positions[toiIndexA].a;
            this.Bodies[toiIndexB]._sweep.C0 = this._positions[toiIndexB].c;
            this.Bodies[toiIndexB]._sweep.A0 = this._positions[toiIndexB].a;
            this._contactSolver.InitializeVelocityConstraints();
            for (int k = 0; k < Settings.TOIVelocityIterations; k++)
            {
                this._contactSolver.SolveVelocityConstraints();
            }
            FP dt = subStep.dt;
            for (int m = 0; m < this.BodyCount; m++)
            {
                TSVector2 c = this._positions[m].c;
                FP a = this._positions[m].a;
                TSVector2 v = this._velocities[m].v;
                FP w = this._velocities[m].w;
                TSVector2 vector3 = (TSVector2) (dt * v);
                if (TSVector2.Dot(vector3, vector3) > Settings.MaxTranslationSquared)
                {
                    FP fp5 = Settings.MaxTranslation / vector3.magnitude;
                    v *= fp5;
                }
                FP fp4 = dt * w;
                if ((fp4 * fp4) > Settings.MaxRotationSquared)
                {
                    FP fp6 = Settings.MaxRotation / FP.Abs(fp4);
                    w *= fp6;
                }
                c += dt * v;
                a += dt * w;
                this._positions[m].c = c;
                this._positions[m].a = a;
                this._velocities[m].v = v;
                this._velocities[m].w = w;
                Body body2 = this.Bodies[m];
                body2._sweep.C = c;
                body2._sweep.A = a;
                body2._linearVelocity = v;
                body2._angularVelocity = w;
                body2.SynchronizeTransform();
            }
            this.Report(this._contactSolver._velocityConstraints);
        }
    }
}

