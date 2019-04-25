namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Threading;

    public class World : IWorld
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <AllowDeactivation>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.CollisionSystem <CollisionSystem>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ReadOnlyHashset<Constraint> <Constraints>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ReadOnlyHashset<RigidBody> <RigidBodies>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ReadOnlyHashset<SoftBody> <SoftBodies>k__BackingField;
        public FP accumulatedTime = FP.Zero;
        public Queue<Arbiter> addedArbiterQueue = new Queue<Arbiter>();
        private FP angularDamping = (0x55 * FP.EN2);
        private Action<object> arbiterCallback;
        private TrueSync.ArbiterMap arbiterMap;
        private TrueSync.ArbiterMap arbiterTriggerMap;
        private OverlapPairContact cacheOverPairContact = new OverlapPairContact(null, null);
        private CollisionDetectedHandler collisionDetectionHandler;
        internal HashList<Constraint> constraints = new HashList<Constraint>();
        private int contactIterations = 6;
        private TrueSync.ContactSettings contactSettings = new TrueSync.ContactSettings();
        private FP currentAngularDampFactor = FP.One;
        private FP currentLinearDampFactor = FP.One;
        private FP deactivationTime = 2;
        private WorldEvents events = new WorldEvents();
        private TSVector gravity = new TSVector(0, -981 * FP.EN2, 0);
        private FP inactiveAngularThresholdSq = FP.EN1;
        private FP inactiveLinearThresholdSq = FP.EN1;
        public HashList<OverlapPairContact> initialCollisions = new HashList<OverlapPairContact>();
        public HashList<OverlapPairContact> initialTriggers = new HashList<OverlapPairContact>();
        private Action<object> integrateCallback;
        public IslandManager islands = new IslandManager();
        private FP linearDamping = (0x55 * FP.EN2);
        public IPhysicsManager physicsManager;
        public Queue<Arbiter> removedArbiterQueue = new Queue<Arbiter>();
        public Stack<Arbiter> removedArbiterStack = new Stack<Arbiter>();
        internal HashList<RigidBody> rigidBodies = new HashList<RigidBody>();
        private int smallIterations = 3;
        internal HashList<SoftBody> softbodies = new HashList<SoftBody>();
        private ThreadManager threadManager = ThreadManager.Instance;
        private FP timestep = FP.Zero;

        public World(TrueSync.CollisionSystem collision)
        {
            if (collision == null)
            {
                throw new ArgumentNullException("The CollisionSystem can't be null.", "collision");
            }
            RigidBody.instanceCount = 0;
            Constraint.instanceCount = 0;
            this.arbiterCallback = new Action<object>(this.ArbiterCallback);
            this.integrateCallback = new Action<object>(this.IntegrateCallback);
            this.RigidBodies = new ReadOnlyHashset<RigidBody>(this.rigidBodies);
            this.Constraints = new ReadOnlyHashset<Constraint>(this.constraints);
            this.SoftBodies = new ReadOnlyHashset<SoftBody>(this.softbodies);
            this.CollisionSystem = collision;
            this.collisionDetectionHandler = new CollisionDetectedHandler(this.CollisionDetected);
            this.CollisionSystem.CollisionDetected += this.collisionDetectionHandler;
            this.arbiterMap = new TrueSync.ArbiterMap();
            this.arbiterTriggerMap = new TrueSync.ArbiterMap();
            this.AllowDeactivation = false;
        }

        public void AddBody(RigidBody body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body", "body can't be null.");
            }
            if (this.rigidBodies.Contains(body))
            {
                throw new ArgumentException("The body was already added to the world.", "body");
            }
            this.events.RaiseAddedRigidBody(body);
            this.CollisionSystem.AddEntity(body);
            this.rigidBodies.Add(body);
        }

        public void AddBody(SoftBody body)
        {
            if (body == null)
            {
                throw new ArgumentNullException("body", "body can't be null.");
            }
            if (this.softbodies.Contains(body))
            {
                throw new ArgumentException("The body was already added to the world.", "body");
            }
            this.softbodies.Add(body);
            this.CollisionSystem.AddEntity(body);
            this.events.RaiseAddedSoftBody(body);
            foreach (Constraint constraint in body.EdgeSprings)
            {
                this.AddConstraint(constraint);
            }
            foreach (SoftBody.MassPoint point in body.VertexBodies)
            {
                this.events.RaiseAddedRigidBody(point);
                this.rigidBodies.Add(point);
            }
        }

        public void AddConstraint(Constraint constraint)
        {
            if (this.constraints.Contains(constraint))
            {
                throw new ArgumentException("The constraint was already added to the world.", "constraint");
            }
            this.constraints.Add(constraint);
            this.islands.ConstraintCreated(constraint);
            this.events.RaiseAddedConstraint(constraint);
        }

        private void ArbiterCallback(object obj)
        {
            int contactIterations;
            CollisionIsland island = obj as CollisionIsland;
            if ((island.Bodies.Count + island.Constraints.Count) > 3)
            {
                contactIterations = this.contactIterations;
            }
            else
            {
                contactIterations = this.smallIterations;
            }
            for (int i = -1; i < contactIterations; i++)
            {
                int num3 = 0;
                int count = island.arbiter.Count;
                while (num3 < count)
                {
                    Arbiter arbiter = island.arbiter[num3];
                    int num5 = arbiter.contactList.Count;
                    for (int j = 0; j < num5; j++)
                    {
                        if (i == -1)
                        {
                            arbiter.contactList[j].PrepareForIteration(this.timestep);
                        }
                        else
                        {
                            arbiter.contactList[j].Iterate();
                        }
                    }
                    num3++;
                }
                int num7 = 0;
                int num8 = island.constraints.Count;
                while (num7 < num8)
                {
                    Constraint constraint = island.constraints[num7];
                    if ((((constraint.body1 == null) || constraint.body1.IsActive) || (constraint.body2 == null)) || constraint.body2.IsActive)
                    {
                        if (i == -1)
                        {
                            constraint.PrepareForIteration(this.timestep);
                        }
                        else
                        {
                            constraint.Iterate();
                        }
                    }
                    num7++;
                }
            }
        }

        public List<IBody> Bodies()
        {
            List<IBody> list = new List<IBody>();
            for (int i = 0; i < this.rigidBodies.Count; i++)
            {
                list.Add(this.rigidBodies[i]);
            }
            return list;
        }

        internal bool CanBodiesCollide(RigidBody body1, RigidBody body2)
        {
            if ((body1.disabled || body2.disabled) || !this.physicsManager.IsCollisionEnabled(body1, body2))
            {
                return false;
            }
            if (body1.IsStaticNonKinematic && body2.IsStaticNonKinematic)
            {
                return false;
            }
            if (body1.IsColliderOnly || body2.IsColliderOnly)
            {
                if (((body1.IsColliderOnly && body1.IsStaticNonKinematic) && body2.IsStaticNonKinematic) || ((body2.IsColliderOnly && body2.IsStaticNonKinematic) && body1.IsStaticNonKinematic))
                {
                    return false;
                }
            }
            else if ((body1.isKinematic && body2.isStatic) || (body2.isKinematic && body1.isStatic))
            {
                return false;
            }
            return true;
        }

        private void CheckDeactivation()
        {
            if (this.AllowDeactivation)
            {
                foreach (CollisionIsland island in this.islands)
                {
                    bool flag2 = true;
                    if (!this.AllowDeactivation)
                    {
                        flag2 = false;
                    }
                    else
                    {
                        int num = 0;
                        int num2 = island.bodies.Count;
                        while (num < num2)
                        {
                            RigidBody body = island.bodies[num];
                            if (body.AllowDeactivation && ((body.angularVelocity.sqrMagnitude < this.inactiveAngularThresholdSq) && (body.linearVelocity.sqrMagnitude < this.inactiveLinearThresholdSq)))
                            {
                                body.inactiveTime += this.timestep;
                                if (body.inactiveTime < this.deactivationTime)
                                {
                                    flag2 = false;
                                }
                            }
                            else
                            {
                                body.inactiveTime = FP.Zero;
                                flag2 = false;
                            }
                            num++;
                        }
                    }
                    int num3 = 0;
                    int count = island.bodies.Count;
                    while (num3 < count)
                    {
                        RigidBody body2 = island.bodies[num3];
                        if (body2.isActive == flag2)
                        {
                            if (body2.isActive)
                            {
                                body2.IsActive = false;
                                this.events.RaiseDeactivatedBody(body2);
                            }
                            else
                            {
                                body2.IsActive = true;
                                this.events.RaiseActivatedBody(body2);
                            }
                        }
                        num3++;
                    }
                }
            }
        }

        public void Clear()
        {
            int num = 0;
            int count = rigidBodies.Count;
            while (num < count)
            {
                RigidBody body = this.rigidBodies[num];
                CollisionSystem.RemoveEntity(body);
                if (body.island != null)
                {
                    body.island.ClearLists();
                    body.island = null;
                }
                body.connections.Clear();
                body.arbiters.Clear();
                body.constraints.Clear();
                this.events.RaiseRemovedRigidBody(body);
                num++;
            }
            int num3 = 0;
            int num4 = this.softbodies.Count;
            while (num3 < num4)
            {
                SoftBody body2 = this.softbodies[num3];
                this.CollisionSystem.RemoveEntity(body2);
                num3++;
            }
            this.rigidBodies.Clear();
            int num5 = 0;
            int num6 = this.constraints.Count;
            while (num5 < num6)
            {
                Constraint constraint = this.constraints[num5];
                this.events.RaiseRemovedConstraint(constraint);
                num5++;
            }
            this.constraints.Clear();
            this.softbodies.Clear();
            this.islands.RemoveAll();
            this.arbiterMap.Clear();
            this.arbiterTriggerMap.Clear();
            ResetResourcePools();
        }

        private void CollisionDetected(RigidBody body1, RigidBody body2, TSVector point1, TSVector point2, TSVector normal, FP penetration)
        {
            bool flag = body1.IsColliderOnly || body2.IsColliderOnly;
            Arbiter arbiter = null;
            TrueSync.ArbiterMap arbiterTriggerMap = null;
            if (flag)
            {
                arbiterTriggerMap = this.arbiterTriggerMap;
            }
            else
            {
                arbiterTriggerMap = this.arbiterMap;
            }
            bool flag2 = false;
            TrueSync.ArbiterMap map2 = arbiterTriggerMap;
            lock (map2)
            {
                arbiterTriggerMap.LookUpArbiter(body1, body2, out arbiter);
                if (arbiter == null)
                {
                    arbiter = Arbiter.Pool.GetNew();
                    arbiter.body1 = body1;
                    arbiter.body2 = body2;
                    arbiterTriggerMap.Add(new ArbiterKey(body1, body2), arbiter);
                    flag2 = true;
                }
            }
            Contact contact = null;
            if (arbiter.body1 == body1)
            {
                TSVector.Negate(ref normal, out normal);
                contact = arbiter.AddContact(point1, point2, normal, penetration, this.contactSettings);
            }
            else
            {
                contact = arbiter.AddContact(point2, point1, normal, penetration, this.contactSettings);
            }
            if (flag2)
            {
                if (flag)
                {
                    this.events.RaiseTriggerBeginCollide(contact);
                    body1.arbitersTrigger.Add(arbiter);
                    body2.arbitersTrigger.Add(arbiter);
                    OverlapPairContact item = new OverlapPairContact(body1, body2) {
                        contact = contact
                    };
                    this.initialTriggers.Add(item);
                }
                else
                {
                    this.events.RaiseBodiesBeginCollide(contact);
                    this.addedArbiterQueue.Enqueue(arbiter);
                    OverlapPairContact contact3 = new OverlapPairContact(body1, body2) {
                        contact = contact
                    };
                    this.initialCollisions.Add(contact3);
                }
            }
            if (!flag && (contact > null))
            {
                this.events.RaiseContactCreated(contact);
            }
        }

        private void HandleArbiter(int iterations, bool multiThreaded)
        {
            if (multiThreaded)
            {
                for (int i = 0; i < this.islands.Count; i++)
                {
                    if (this.islands[i].IsActive())
                    {
                        this.threadManager.AddTask(this.arbiterCallback, this.islands[i]);
                    }
                }
                this.threadManager.Execute();
            }
            else
            {
                for (int j = 0; j < this.islands.Count; j++)
                {
                    if (this.islands[j].IsActive())
                    {
                        this.arbiterCallback(this.islands[j]);
                    }
                }
            }
        }

        private void Integrate(bool multithread)
        {
            if (multithread)
            {
                int num = 0;
                int count = this.rigidBodies.Count;
                while (num < count)
                {
                    RigidBody param = this.rigidBodies[num];
                    if (!(param.isStatic || !param.IsActive))
                    {
                        this.threadManager.AddTask(this.integrateCallback, param);
                    }
                    num++;
                }
                this.threadManager.Execute();
            }
            else
            {
                int num3 = 0;
                int num4 = this.rigidBodies.Count;
                while (num3 < num4)
                {
                    RigidBody body2 = this.rigidBodies[num3];
                    if (!(body2.isStatic || !body2.IsActive))
                    {
                        this.integrateCallback(body2);
                    }
                    num3++;
                }
            }
        }

        private void IntegrateCallback(object obj)
        {
            TSVector vector;
            RigidBody body = obj as RigidBody;
            TSVector.Multiply(ref body.linearVelocity, this.timestep, out vector);
            TSVector.Add(ref vector, ref body.position, out body.position);
            if (!body.isParticle)
            {
                TSVector vector2;
                TSQuaternion quaternion2;
                FP magnitude = body.angularVelocity.magnitude;
                if (magnitude < FP.EN3)
                {
                    TSVector.Multiply(ref body.angularVelocity, (FP.Half * this.timestep) - (((((this.timestep * this.timestep) * this.timestep) * (0x822 * FP.EN6)) * magnitude) * magnitude), out vector2);
                }
                else
                {
                    TSVector.Multiply(ref body.angularVelocity, FP.Sin((FP.Half * magnitude) * this.timestep) / magnitude, out vector2);
                }
                TSQuaternion quaternion = new TSQuaternion(vector2.x, vector2.y, vector2.z, FP.Cos((magnitude * this.timestep) * FP.Half));
                TSQuaternion.CreateFromMatrix(ref body.orientation, out quaternion2);
                TSQuaternion.Multiply(ref quaternion, ref quaternion2, out quaternion);
                quaternion.Normalize();
                TSMatrix.CreateFromQuaternion(ref quaternion, out body.orientation);
            }
            if ((body.Damping & RigidBody.DampingType.Linear) > RigidBody.DampingType.None)
            {
                TSVector.Multiply(ref body.linearVelocity, this.currentLinearDampFactor, out body.linearVelocity);
            }
            if ((body.Damping & RigidBody.DampingType.Angular) > RigidBody.DampingType.None)
            {
                TSVector.Multiply(ref body.angularVelocity, this.currentAngularDampFactor, out body.angularVelocity);
            }
            body.Update();
            if (this.CollisionSystem.EnableSpeculativeContacts || body.EnableSpeculativeContacts)
            {
                body.SweptExpandBoundingBox(this.timestep);
            }
        }

        private void IntegrateForces()
        {
            int num = 0;
            int count = this.rigidBodies.Count;
            while (num < count)
            {
                RigidBody body = this.rigidBodies[num];
                if (!body.isStatic && body.IsActive)
                {
                    TSVector vector;
                    TSVector.Multiply(ref body.force, body.inverseMass * this.timestep, out vector);
                    TSVector.Add(ref vector, ref body.linearVelocity, out body.linearVelocity);
                    if (!body.isParticle)
                    {
                        TSVector.Multiply(ref body.torque, this.timestep, out vector);
                        TSVector.Transform(ref vector, ref body.invInertiaWorld, out vector);
                        TSVector.Add(ref vector, ref body.angularVelocity, out body.angularVelocity);
                    }
                    if (body.affectedByGravity)
                    {
                        TSVector.Multiply(ref this.gravity, this.timestep, out vector);
                        TSVector.Add(ref body.linearVelocity, ref vector, out body.linearVelocity);
                    }
                }
                body.force.MakeZero();
                body.torque.MakeZero();
                num++;
            }
        }

        public bool RemoveBody(RigidBody body)
        {
            return this.RemoveBody(body, false);
        }

        public bool RemoveBody(SoftBody body)
        {
            if (!this.softbodies.Remove(body))
            {
                return false;
            }
            this.CollisionSystem.RemoveEntity(body);
            this.events.RaiseRemovedSoftBody(body);
            foreach (Constraint constraint in body.EdgeSprings)
            {
                this.RemoveConstraint(constraint);
            }
            foreach (SoftBody.MassPoint point in body.VertexBodies)
            {
                this.RemoveBody(point, true);
            }
            return true;
        }

        private bool RemoveBody(RigidBody body, bool removeMassPoints)
        {
            if (!removeMassPoints && body.IsParticle)
            {
                return false;
            }
            if (!this.rigidBodies.Remove(body))
            {
                return false;
            }
            int num = 0;
            int count = body.arbiters.Count;
            while (num < count)
            {
                Arbiter arbiter = body.arbiters[num];
                this.arbiterMap.Remove(arbiter);
                this.events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
                this.cacheOverPairContact.SetBodies(arbiter.body1, arbiter.body2);
                this.initialCollisions.Remove(this.cacheOverPairContact);
                num++;
            }
            int num3 = 0;
            int num4 = body.arbitersTrigger.Count;
            while (num3 < num4)
            {
                Arbiter arbiter2 = body.arbitersTrigger[num3];
                this.arbiterTriggerMap.Remove(arbiter2);
                if (arbiter2.body1.isColliderOnly)
                {
                    this.events.RaiseTriggerEndCollide(arbiter2.body1, arbiter2.body2);
                }
                else
                {
                    this.events.RaiseTriggerEndCollide(arbiter2.body2, arbiter2.body1);
                }
                this.cacheOverPairContact.SetBodies(arbiter2.body1, arbiter2.body2);
                this.initialTriggers.Remove(this.cacheOverPairContact);
                num3++;
            }
            int num5 = 0;
            int num6 = body.constraints.Count;
            while (num5 < num6)
            {
                Constraint item = body.constraints[num5];
                this.constraints.Remove(item);
                this.events.RaiseRemovedConstraint(item);
                num5++;
            }
            this.CollisionSystem.RemoveEntity(body);
            this.islands.RemoveBody(body);
            this.events.RaiseRemovedRigidBody(body);
            return true;
        }

        public bool RemoveConstraint(Constraint constraint)
        {
            if (!this.constraints.Remove(constraint))
            {
                return false;
            }
            this.events.RaiseRemovedConstraint(constraint);
            this.islands.ConstraintRemoved(constraint);
            return true;
        }

        public void ResetResourcePools()
        {
            IslandManager.Pool.ResetResourcePool();
            Arbiter.Pool.ResetResourcePool();
            Contact.Pool.ResetResourcePool();
        }

        public void SetDampingFactors(FP angularDamping, FP linearDamping)
        {
            if ((angularDamping < FP.Zero) || (angularDamping > FP.One))
            {
                throw new ArgumentException("Angular damping factor has to be between 0.0 and 1.0", "angularDamping");
            }
            if ((linearDamping < FP.Zero) || (linearDamping > FP.One))
            {
                throw new ArgumentException("Linear damping factor has to be between 0.0 and 1.0", "linearDamping");
            }
            this.angularDamping = angularDamping;
            this.linearDamping = linearDamping;
        }

        public void SetInactivityThreshold(FP angularVelocity, FP linearVelocity, FP time)
        {
            if (angularVelocity < FP.Zero)
            {
                throw new ArgumentException("Angular velocity threshold has to be larger than zero", "angularVelocity");
            }
            if (linearVelocity < FP.Zero)
            {
                throw new ArgumentException("Linear velocity threshold has to be larger than zero", "linearVelocity");
            }
            if (time < FP.Zero)
            {
                throw new ArgumentException("Deactivation time threshold has to be larger than zero", "time");
            }
            this.inactiveAngularThresholdSq = angularVelocity * angularVelocity;
            this.inactiveLinearThresholdSq = linearVelocity * linearVelocity;
            this.deactivationTime = time;
        }

        public void SetIterations(int iterations, int smallIterations)
        {
            if (iterations < 1)
            {
                throw new ArgumentException("The number of collision iterations has to be larger than zero", "iterations");
            }
            if (smallIterations < 1)
            {
                throw new ArgumentException("The number of collision iterations has to be larger than zero", "smallIterations");
            }
            this.contactIterations = iterations;
            this.smallIterations = smallIterations;
        }

        public void Step(FP timestep, bool multithread)
        {
            this.timestep = timestep;
            if (timestep != FP.Zero)
            {
                if (timestep < FP.Zero)
                {
                    throw new ArgumentException("The timestep can't be negative.", "timestep");
                }
                this.currentAngularDampFactor = FP.One;
                this.currentLinearDampFactor = FP.One;
                this.events.RaiseWorldPreStep(timestep);
                this.UpdateContacts();
                int num = 0;
                int count = this.initialCollisions.Count;
                while (num < count)
                {
                    OverlapPairContact contact = this.initialCollisions[num];
                    this.events.RaiseBodiesStayCollide(contact.contact);
                    num++;
                }
                int num3 = 0;
                int num4 = this.initialTriggers.Count;
                while (num3 < num4)
                {
                    OverlapPairContact contact2 = this.initialTriggers[num3];
                    this.events.RaiseTriggerStayCollide(contact2.contact);
                    num3++;
                }
                while (this.removedArbiterQueue.Count > 0)
                {
                    this.islands.ArbiterRemoved(this.removedArbiterQueue.Dequeue());
                }
                int num5 = 0;
                int num6 = this.softbodies.Count;
                while (num5 < num6)
                {
                    SoftBody body = this.softbodies[num5];
                    body.Update(timestep);
                    body.DoSelfCollision(this.collisionDetectionHandler);
                    num5++;
                }
                this.CollisionSystem.Detect(multithread);
                while (this.addedArbiterQueue.Count > 0)
                {
                    this.islands.ArbiterCreated(this.addedArbiterQueue.Dequeue());
                }
                this.CheckDeactivation();
                this.IntegrateForces();
                this.HandleArbiter(this.contactIterations, multithread);
                this.Integrate(multithread);
                int num7 = 0;
                int num8 = this.rigidBodies.Count;
                while (num7 < num8)
                {
                    RigidBody body2 = this.rigidBodies[num7];
                    body2.PostStep();
                    int num9 = 0;
                    int num10 = body2.constraints.Count;
                    while (num9 < num10)
                    {
                        body2.constraints[num9].PostStep();
                        num9++;
                    }
                    num7++;
                }
                this.events.RaiseWorldPostStep(timestep);
            }
        }

        public void Step(FP totalTime, bool multithread, FP timestep, int maxSteps)
        {
            int num = 0;
            this.accumulatedTime += totalTime;
            while (this.accumulatedTime > timestep)
            {
                this.Step(timestep, multithread);
                this.accumulatedTime -= timestep;
                num++;
                if (num > maxSteps)
                {
                    this.accumulatedTime = FP.Zero;
                    break;
                }
            }
        }

        private void UpdateArbiterContacts(Arbiter arbiter)
        {
            if (arbiter.contactList.Count == 0)
            {
                Stack<Arbiter> removedArbiterStack = this.removedArbiterStack;
                lock (removedArbiterStack)
                {
                    this.removedArbiterStack.Push(arbiter);
                }
            }
            else
            {
                for (int i = arbiter.contactList.Count - 1; i >= 0; i--)
                {
                    Contact contact = arbiter.contactList[i];
                    contact.UpdatePosition();
                    if (contact.penetration < -this.contactSettings.breakThreshold)
                    {
                        Contact.Pool.GiveBack(contact);
                        arbiter.contactList.RemoveAt(i);
                    }
                    else
                    {
                        TSVector vector;
                        TSVector.Subtract(ref contact.p1, ref contact.p2, out vector);
                        FP fp = TSVector.Dot(ref vector, ref contact.normal);
                        vector -= fp * contact.normal;
                        if (vector.sqrMagnitude > ((this.contactSettings.breakThreshold * this.contactSettings.breakThreshold) * 100))
                        {
                            Contact.Pool.GiveBack(contact);
                            arbiter.contactList.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private void UpdateContacts()
        {
            this.UpdateContacts(this.arbiterMap);
            this.UpdateContacts(this.arbiterTriggerMap);
        }

        private void UpdateContacts(TrueSync.ArbiterMap selectedArbiterMap)
        {
            foreach (Arbiter arbiter in selectedArbiterMap.Arbiters)
            {
                this.UpdateArbiterContacts(arbiter);
            }
            while (this.removedArbiterStack.Count > 0)
            {
                Arbiter arbiter2 = this.removedArbiterStack.Pop();
                Arbiter.Pool.GiveBack(arbiter2);
                selectedArbiterMap.Remove(arbiter2);
                if (selectedArbiterMap == this.arbiterMap)
                {
                    this.removedArbiterQueue.Enqueue(arbiter2);
                    this.events.RaiseBodiesEndCollide(arbiter2.body1, arbiter2.body2);
                    this.cacheOverPairContact.SetBodies(arbiter2.body1, arbiter2.body2);
                    this.initialCollisions.Remove(this.cacheOverPairContact);
                }
                else
                {
                    if (arbiter2.body1.isColliderOnly)
                    {
                        this.events.RaiseTriggerEndCollide(arbiter2.body1, arbiter2.body2);
                    }
                    else
                    {
                        this.events.RaiseTriggerEndCollide(arbiter2.body2, arbiter2.body1);
                    }
                    this.cacheOverPairContact.SetBodies(arbiter2.body1, arbiter2.body2);
                    this.initialTriggers.Remove(this.cacheOverPairContact);
                }
            }
        }

        public bool AllowDeactivation { get; set; }

        public TrueSync.ArbiterMap ArbiterMap
        {
            get
            {
                return this.arbiterMap;
            }
        }

        public TrueSync.ArbiterMap ArbiterTriggerMap
        {
            get
            {
                return this.arbiterTriggerMap;
            }
        }

        public TrueSync.CollisionSystem CollisionSystem { get; set; }

        public ReadOnlyHashset<Constraint> Constraints { get; private set; }

        public TrueSync.ContactSettings ContactSettings
        {
            get
            {
                return this.contactSettings;
            }
        }

        public WorldEvents Events
        {
            get
            {
                return this.events;
            }
        }

        public TSVector Gravity
        {
            get
            {
                return this.gravity;
            }
            set
            {
                this.gravity = value;
            }
        }

        public List<CollisionIsland> Islands
        {
            get
            {
                return this.islands;
            }
        }

        public ReadOnlyHashset<RigidBody> RigidBodies { get; private set; }

        public ReadOnlyHashset<SoftBody> SoftBodies { get; private set; }

        public class WorldEvents
        {
            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<RigidBody> ActivatedBody;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<Constraint> AddedConstraint;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<RigidBody> AddedRigidBody;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<SoftBody> AddedSoftBody;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<Contact> BodiesBeginCollide;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<RigidBody, RigidBody> BodiesEndCollide;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<Contact> BodiesStayCollide;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<Contact> ContactCreated;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<RigidBody> DeactivatedBody;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event World.WorldStep PostStep;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event World.WorldStep PreStep;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<Constraint> RemovedConstraint;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<RigidBody> RemovedRigidBody;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<SoftBody> RemovedSoftBody;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<Contact> TriggerBeginCollide;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<RigidBody, RigidBody> TriggerEndCollide;

            [field: CompilerGenerated, DebuggerBrowsable(0)]
            public event Action<Contact> TriggerStayCollide;

            internal WorldEvents()
            {
            }

            internal void RaiseActivatedBody(RigidBody body)
            {
                if (this.ActivatedBody > null)
                {
                    this.ActivatedBody(body);
                }
            }

            internal void RaiseAddedConstraint(Constraint constraint)
            {
                if (this.AddedConstraint > null)
                {
                    this.AddedConstraint(constraint);
                }
            }

            internal void RaiseAddedRigidBody(RigidBody body)
            {
                if (this.AddedRigidBody > null)
                {
                    this.AddedRigidBody(body);
                }
            }

            internal void RaiseAddedSoftBody(SoftBody body)
            {
                if (this.AddedSoftBody > null)
                {
                    this.AddedSoftBody(body);
                }
            }

            internal void RaiseBodiesBeginCollide(Contact contact)
            {
                if (this.BodiesBeginCollide > null)
                {
                    this.BodiesBeginCollide(contact);
                }
            }

            internal void RaiseBodiesEndCollide(RigidBody body1, RigidBody body2)
            {
                if (this.BodiesEndCollide > null)
                {
                    this.BodiesEndCollide(body1, body2);
                }
            }

            internal void RaiseBodiesStayCollide(Contact contact)
            {
                if (this.BodiesStayCollide > null)
                {
                    this.BodiesStayCollide(contact);
                }
            }

            internal void RaiseContactCreated(Contact contact)
            {
                if (this.ContactCreated > null)
                {
                    this.ContactCreated(contact);
                }
            }

            internal void RaiseDeactivatedBody(RigidBody body)
            {
                if (this.DeactivatedBody > null)
                {
                    this.DeactivatedBody(body);
                }
            }

            internal void RaiseRemovedConstraint(Constraint constraint)
            {
                if (this.RemovedConstraint > null)
                {
                    this.RemovedConstraint(constraint);
                }
            }

            internal void RaiseRemovedRigidBody(RigidBody body)
            {
                if (this.RemovedRigidBody > null)
                {
                    this.RemovedRigidBody(body);
                }
            }

            internal void RaiseRemovedSoftBody(SoftBody body)
            {
                if (this.RemovedSoftBody > null)
                {
                    this.RemovedSoftBody(body);
                }
            }

            internal void RaiseTriggerBeginCollide(Contact contact)
            {
                if (this.TriggerBeginCollide > null)
                {
                    this.TriggerBeginCollide(contact);
                }
            }

            internal void RaiseTriggerEndCollide(RigidBody body1, RigidBody body2)
            {
                if (this.TriggerEndCollide > null)
                {
                    this.TriggerEndCollide(body1, body2);
                }
            }

            internal void RaiseTriggerStayCollide(Contact contact)
            {
                if (this.TriggerStayCollide > null)
                {
                    this.TriggerStayCollide(contact);
                }
            }

            internal void RaiseWorldPostStep(FP timestep)
            {
                if (this.PostStep > null)
                {
                    this.PostStep(timestep);
                }
            }

            internal void RaiseWorldPreStep(FP timestep)
            {
                if (this.PreStep > null)
                {
                    this.PreStep(timestep);
                }
            }
        }

        public delegate void WorldStep(FP timestep);
    }
}

