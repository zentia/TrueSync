// Decompiled with JetBrains decompiler
// Type: TrueSync.World
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections.Generic;

namespace TrueSync
{
    public class World : IWorld
    {
        private ContactSettings contactSettings = new ContactSettings();
        private FP inactiveAngularThresholdSq = FP.EN1;
        private FP inactiveLinearThresholdSq = FP.EN1;
        private FP deactivationTime = (FP)2;
        private FP angularDamping = (FP)85 * FP.EN2;
        private FP linearDamping = (FP)85 * FP.EN2;
        private int contactIterations = 6;
        private int smallIterations = 3;
        private FP timestep = FP.Zero;
        public IslandManager islands = new IslandManager();
        public HashList<OverlapPairContact> initialCollisions = new HashList<OverlapPairContact>();
        public HashList<OverlapPairContact> initialTriggers = new HashList<OverlapPairContact>();
        private OverlapPairContact cacheOverPairContact = new OverlapPairContact((IBroadphaseEntity)null, (IBroadphaseEntity)null);
        internal HashList<RigidBody> rigidBodies = new HashList<RigidBody>();
        internal HashList<Constraint> constraints = new HashList<Constraint>();
        internal HashList<SoftBody> softbodies = new HashList<SoftBody>();
        private World.WorldEvents events = new World.WorldEvents();
        private ThreadManager threadManager = ThreadManager.Instance;
        public Queue<Arbiter> removedArbiterQueue = new Queue<Arbiter>();
        public Queue<Arbiter> addedArbiterQueue = new Queue<Arbiter>();
        private TSVector gravity = new TSVector((FP)0, (FP) - 981 * FP.EN2, (FP)0);
        private FP currentLinearDampFactor = FP.One;
        private FP currentAngularDampFactor = FP.One;
        public FP accumulatedTime = FP.Zero;
        public Stack<Arbiter> removedArbiterStack = new Stack<Arbiter>();
        private ArbiterMap arbiterMap;
        private ArbiterMap arbiterTriggerMap;
        private Action<object> arbiterCallback;
        private Action<object> integrateCallback;
        private CollisionDetectedHandler collisionDetectionHandler;
        public IPhysicsManager physicsManager;

        public ReadOnlyHashset<RigidBody> RigidBodies { get; private set; }

        public ReadOnlyHashset<Constraint> Constraints { get; private set; }

        public ReadOnlyHashset<SoftBody> SoftBodies { get; private set; }

        public World.WorldEvents Events
        {
            get
            {
                return this.events;
            }
        }

        public ArbiterMap ArbiterMap
        {
            get
            {
                return this.arbiterMap;
            }
        }

        public ArbiterMap ArbiterTriggerMap
        {
            get
            {
                return this.arbiterTriggerMap;
            }
        }

        public ContactSettings ContactSettings
        {
            get
            {
                return this.contactSettings;
            }
        }

        public List<CollisionIsland> Islands
        {
            get
            {
                return (List<CollisionIsland>)this.islands;
            }
        }

        public World(CollisionSystem collision)
        {
            if (collision == null)
                throw new ArgumentNullException("The CollisionSystem can't be null.", nameof(collision));
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
            this.arbiterMap = new ArbiterMap();
            this.arbiterTriggerMap = new ArbiterMap();
            this.AllowDeactivation = false;
        }

        public void AddBody(SoftBody body)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body), "body can't be null.");
            if (this.softbodies.Contains(body))
                throw new ArgumentException("The body was already added to the world.", nameof(body));
            this.softbodies.Add(body);
            this.CollisionSystem.AddEntity((IBroadphaseEntity)body);
            this.events.RaiseAddedSoftBody(body);
            foreach (Constraint edgeSpring in body.EdgeSprings)
                this.AddConstraint(edgeSpring);
            foreach (SoftBody.MassPoint vertexBody in body.VertexBodies)
            {
                this.events.RaiseAddedRigidBody((RigidBody)vertexBody);
                this.rigidBodies.Add((RigidBody)vertexBody);
            }
        }

        public bool RemoveBody(SoftBody body)
        {
            if (!this.softbodies.Remove(body))
                return false;
            this.CollisionSystem.RemoveEntity((IBroadphaseEntity)body);
            this.events.RaiseRemovedSoftBody(body);
            foreach (Constraint edgeSpring in body.EdgeSprings)
                this.RemoveConstraint(edgeSpring);
            foreach (RigidBody vertexBody in body.VertexBodies)
                this.RemoveBody(vertexBody, true);
            return true;
        }

        public CollisionSystem CollisionSystem { set; get; }

        public void ResetResourcePools()
        {
            IslandManager.Pool.ResetResourcePool();
            Arbiter.Pool.ResetResourcePool();
            Contact.Pool.ResetResourcePool();
        }

        public void Clear()
        {
            int index1 = 0;
            for (int count = this.rigidBodies.Count; index1 < count; ++index1)
            {
                RigidBody rigidBody = this.rigidBodies[index1];
                this.CollisionSystem.RemoveEntity((IBroadphaseEntity)rigidBody);
                if (rigidBody.island != null)
                {
                    rigidBody.island.ClearLists();
                    rigidBody.island = (CollisionIsland)null;
                }
                rigidBody.connections.Clear();
                rigidBody.arbiters.Clear();
                rigidBody.constraints.Clear();
                this.events.RaiseRemovedRigidBody(rigidBody);
            }
            int index2 = 0;
            for (int count = this.softbodies.Count; index2 < count; ++index2)
                this.CollisionSystem.RemoveEntity((IBroadphaseEntity)this.softbodies[index2]);
            this.rigidBodies.Clear();
            int index3 = 0;
            for (int count = this.constraints.Count; index3 < count; ++index3)
                this.events.RaiseRemovedConstraint(this.constraints[index3]);
            this.constraints.Clear();
            this.softbodies.Clear();
            this.islands.RemoveAll();
            this.arbiterMap.Clear();
            this.arbiterTriggerMap.Clear();
            this.ResetResourcePools();
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

        public bool AllowDeactivation { get; set; }

        public void SetDampingFactors(FP angularDamping, FP linearDamping)
        {
            if (angularDamping < FP.Zero || angularDamping > FP.One)
                throw new ArgumentException("Angular damping factor has to be between 0.0 and 1.0", nameof(angularDamping));
            if (linearDamping < FP.Zero || linearDamping > FP.One)
                throw new ArgumentException("Linear damping factor has to be between 0.0 and 1.0", nameof(linearDamping));
            this.angularDamping = angularDamping;
            this.linearDamping = linearDamping;
        }

        public void SetInactivityThreshold(FP angularVelocity, FP linearVelocity, FP time)
        {
            if (angularVelocity < FP.Zero)
                throw new ArgumentException("Angular velocity threshold has to be larger than zero", nameof(angularVelocity));
            if (linearVelocity < FP.Zero)
                throw new ArgumentException("Linear velocity threshold has to be larger than zero", nameof(linearVelocity));
            if (time < FP.Zero)
                throw new ArgumentException("Deactivation time threshold has to be larger than zero", nameof(time));
            this.inactiveAngularThresholdSq = angularVelocity * angularVelocity;
            this.inactiveLinearThresholdSq = linearVelocity * linearVelocity;
            this.deactivationTime = time;
        }

        public void SetIterations(int iterations, int smallIterations)
        {
            if (iterations < 1)
                throw new ArgumentException("The number of collision iterations has to be larger than zero", nameof(iterations));
            if (smallIterations < 1)
                throw new ArgumentException("The number of collision iterations has to be larger than zero", nameof(smallIterations));
            this.contactIterations = iterations;
            this.smallIterations = smallIterations;
        }

        public bool RemoveBody(RigidBody body)
        {
            return this.RemoveBody(body, false);
        }

        private bool RemoveBody(RigidBody body, bool removeMassPoints)
        {
            if (!removeMassPoints && body.IsParticle || !this.rigidBodies.Remove(body))
                return false;
            int index1 = 0;
            for (int count = body.arbiters.Count; index1 < count; ++index1)
            {
                Arbiter arbiter = body.arbiters[index1];
                this.arbiterMap.Remove(arbiter);
                this.events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
                this.cacheOverPairContact.SetBodies((IBroadphaseEntity)arbiter.body1, (IBroadphaseEntity)arbiter.body2);
                this.initialCollisions.Remove(this.cacheOverPairContact);
            }
            int index2 = 0;
            for (int count = body.arbitersTrigger.Count; index2 < count; ++index2)
            {
                Arbiter arbiter = body.arbitersTrigger[index2];
                this.arbiterTriggerMap.Remove(arbiter);
                if (arbiter.body1.isColliderOnly)
                    this.events.RaiseTriggerEndCollide(arbiter.body1, arbiter.body2);
                else
                    this.events.RaiseTriggerEndCollide(arbiter.body2, arbiter.body1);
                this.cacheOverPairContact.SetBodies((IBroadphaseEntity)arbiter.body1, (IBroadphaseEntity)arbiter.body2);
                this.initialTriggers.Remove(this.cacheOverPairContact);
            }
            int index3 = 0;
            for (int count = body.constraints.Count; index3 < count; ++index3)
            {
                Constraint constraint = body.constraints[index3];
                this.constraints.Remove(constraint);
                this.events.RaiseRemovedConstraint(constraint);
            }
            this.CollisionSystem.RemoveEntity((IBroadphaseEntity)body);
            this.islands.RemoveBody(body);
            this.events.RaiseRemovedRigidBody(body);
            return true;
        }

        public void AddBody(RigidBody body)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body), "body can't be null.");
            if (this.rigidBodies.Contains(body))
                throw new ArgumentException("The body was already added to the world.", nameof(body));
            this.events.RaiseAddedRigidBody(body);
            this.CollisionSystem.AddEntity((IBroadphaseEntity)body);
            this.rigidBodies.Add(body);
        }

        public bool RemoveConstraint(Constraint constraint)
        {
            if (!this.constraints.Remove(constraint))
                return false;
            this.events.RaiseRemovedConstraint(constraint);
            this.islands.ConstraintRemoved(constraint);
            return true;
        }

        public void AddConstraint(Constraint constraint)
        {
            if (this.constraints.Contains(constraint))
                throw new ArgumentException("The constraint was already added to the world.", nameof(constraint));
            this.constraints.Add(constraint);
            this.islands.ConstraintCreated(constraint);
            this.events.RaiseAddedConstraint(constraint);
        }

        public void Step(FP timestep, bool multithread)
        {
            this.timestep = timestep;
            if (timestep == FP.Zero)
                return;
            if (timestep < FP.Zero)
                throw new ArgumentException("The timestep can't be negative.", nameof(timestep));
            this.currentAngularDampFactor = FP.One;
            this.currentLinearDampFactor = FP.One;
            this.events.RaiseWorldPreStep(timestep);
            this.UpdateContacts();
            int index1 = 0;
            for (int count = this.initialCollisions.Count; index1 < count; ++index1)
                this.events.RaiseBodiesStayCollide(this.initialCollisions[index1].contact);
            int index2 = 0;
            for (int count = this.initialTriggers.Count; index2 < count; ++index2)
                this.events.RaiseTriggerStayCollide(this.initialTriggers[index2].contact);
            while (this.removedArbiterQueue.Count > 0)
                this.islands.ArbiterRemoved(this.removedArbiterQueue.Dequeue());
            int index3 = 0;
            for (int count = this.softbodies.Count; index3 < count; ++index3)
            {
                SoftBody softbody = this.softbodies[index3];
                softbody.Update(timestep);
                softbody.DoSelfCollision(this.collisionDetectionHandler);
            }
            CollisionSystem.Detect(multithread);
            while (this.addedArbiterQueue.Count > 0)
                this.islands.ArbiterCreated(this.addedArbiterQueue.Dequeue());
            this.CheckDeactivation();
            this.IntegrateForces();
            this.HandleArbiter(this.contactIterations, multithread);
            this.Integrate(multithread);
            int index4 = 0;
            for (int count1 = this.rigidBodies.Count; index4 < count1; ++index4)
            {
                RigidBody rigidBody = this.rigidBodies[index4];
                rigidBody.PostStep();
                int index5 = 0;
                for (int count2 = rigidBody.constraints.Count; index5 < count2; ++index5)
                    rigidBody.constraints[index5].PostStep();
            }
            this.events.RaiseWorldPostStep(timestep);
        }

        public void Step(FP totalTime, bool multithread, FP timestep, int maxSteps)
        {
            int num = 0;
            this.accumulatedTime = this.accumulatedTime + totalTime;
            while (this.accumulatedTime > timestep)
            {
                this.Step(timestep, multithread);
                this.accumulatedTime = this.accumulatedTime - timestep;
                ++num;
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
                lock (this.removedArbiterStack)
                    this.removedArbiterStack.Push(arbiter);
            }
            else
            {
                for (int index = arbiter.contactList.Count - 1; index >= 0; --index)
                {
                    Contact contact = arbiter.contactList[index];
                    contact.UpdatePosition();
                    if (contact.penetration < -this.contactSettings.breakThreshold)
                    {
                        Contact.Pool.GiveBack(contact);
                        arbiter.contactList.RemoveAt(index);
                    }
                    else
                    {
                        TSVector result;
                        TSVector.Subtract(ref contact.p1, ref contact.p2, out result);
                        FP fp = TSVector.Dot(ref result, ref contact.normal);
                        result -= fp * contact.normal;
                        if (result.sqrMagnitude > this.contactSettings.breakThreshold * this.contactSettings.breakThreshold * (FP)100)
                        {
                            Contact.Pool.GiveBack(contact);
                            arbiter.contactList.RemoveAt(index);
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

        private void UpdateContacts(ArbiterMap selectedArbiterMap)
        {
            foreach (Arbiter arbiter in selectedArbiterMap.Arbiters)
                this.UpdateArbiterContacts(arbiter);
            while (this.removedArbiterStack.Count > 0)
            {
                Arbiter arbiter = this.removedArbiterStack.Pop();
                Arbiter.Pool.GiveBack(arbiter);
                selectedArbiterMap.Remove(arbiter);
                if (selectedArbiterMap == this.arbiterMap)
                {
                    this.removedArbiterQueue.Enqueue(arbiter);
                    this.events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
                    this.cacheOverPairContact.SetBodies((IBroadphaseEntity)arbiter.body1, (IBroadphaseEntity)arbiter.body2);
                    this.initialCollisions.Remove(this.cacheOverPairContact);
                }
                else
                {
                    if (arbiter.body1.isColliderOnly)
                        this.events.RaiseTriggerEndCollide(arbiter.body1, arbiter.body2);
                    else
                        this.events.RaiseTriggerEndCollide(arbiter.body2, arbiter.body1);
                    this.cacheOverPairContact.SetBodies((IBroadphaseEntity)arbiter.body1, (IBroadphaseEntity)arbiter.body2);
                    this.initialTriggers.Remove(this.cacheOverPairContact);
                }
            }
        }

        private void ArbiterCallback(object obj)
        {
            CollisionIsland collisionIsland = obj as CollisionIsland;
            int num = collisionIsland.Bodies.Count + collisionIsland.Constraints.Count <= 3 ? this.smallIterations : this.contactIterations;
            for (int index1 = -1; index1 < num; ++index1)
            {
                int index2 = 0;
                for (int count1 = collisionIsland.arbiter.Count; index2 < count1; ++index2)
                {
                    Arbiter arbiter = collisionIsland.arbiter[index2];
                    int count2 = arbiter.contactList.Count;
                    for (int index3 = 0; index3 < count2; ++index3)
                    {
                        if (index1 == -1)
                            arbiter.contactList[index3].PrepareForIteration(this.timestep);
                        else
                            arbiter.contactList[index3].Iterate();
                    }
                }
                int index4 = 0;
                for (int count = collisionIsland.constraints.Count; index4 < count; ++index4)
                {
                    Constraint constraint = collisionIsland.constraints[index4];
                    if (constraint.body1 == null || constraint.body1.IsActive || constraint.body2 == null || constraint.body2.IsActive)
                    {
                        if (index1 == -1)
                            constraint.PrepareForIteration(this.timestep);
                        else
                            constraint.Iterate();
                    }
                }
            }
        }

        private void HandleArbiter(int iterations, bool multiThreaded)
        {
            if (multiThreaded)
            {
                for (int index = 0; index < this.islands.Count; ++index)
                {
                    if (this.islands[index].IsActive())
                        this.threadManager.AddTask(this.arbiterCallback, (object)this.islands[index]);
                }
                this.threadManager.Execute();
            }
            else
            {
                for (int index = 0; index < this.islands.Count; ++index)
                {
                    if (this.islands[index].IsActive())
                        this.arbiterCallback((object)this.islands[index]);
                }
            }
        }

        private void IntegrateForces()
        {
            int index = 0;
            for (int count = this.rigidBodies.Count; index < count; ++index)
            {
                RigidBody rigidBody = this.rigidBodies[index];
                if (!rigidBody.isStatic && rigidBody.IsActive)
                {
                    TSVector result;
                    TSVector.Multiply(ref rigidBody.force, rigidBody.inverseMass * this.timestep, out result);
                    TSVector.Add(ref result, ref rigidBody.linearVelocity, out rigidBody.linearVelocity);
                    if (!rigidBody.isParticle)
                    {
                        TSVector.Multiply(ref rigidBody.torque, this.timestep, out result);
                        TSVector.Transform(ref result, ref rigidBody.invInertiaWorld, out result);
                        TSVector.Add(ref result, ref rigidBody.angularVelocity, out rigidBody.angularVelocity);
                    }
                    if (rigidBody.affectedByGravity)
                    {
                        TSVector.Multiply(ref this.gravity, this.timestep, out result);
                        TSVector.Add(ref rigidBody.linearVelocity, ref result, out rigidBody.linearVelocity);
                    }
                }
                rigidBody.force.MakeZero();
                rigidBody.torque.MakeZero();
            }
        }

        private void IntegrateCallback(object obj)
        {
            RigidBody rigidBody = obj as RigidBody;
            TSVector result1;
            TSVector.Multiply(ref rigidBody.linearVelocity, this.timestep, out result1);
            TSVector.Add(ref result1, ref rigidBody.position, out rigidBody.position);
            if (!rigidBody.isParticle)
            {
                FP magnitude = rigidBody.angularVelocity.magnitude;
                TSVector result2;
                if (magnitude < FP.EN3)
                    TSVector.Multiply(ref rigidBody.angularVelocity, FP.Half * this.timestep - this.timestep * this.timestep * this.timestep * ((FP)2082 * FP.EN6) * magnitude * magnitude, out result2);
                else
                    TSVector.Multiply(ref rigidBody.angularVelocity, FP.Sin(FP.Half * magnitude * this.timestep) / magnitude, out result2);
                TSQuaternion result3 = new TSQuaternion(result2.x, result2.y, result2.z, FP.Cos(magnitude * this.timestep * FP.Half));
                TSQuaternion result4;
                TSQuaternion.CreateFromMatrix(ref rigidBody.orientation, out result4);
                TSQuaternion.Multiply(ref result3, ref result4, out result3);
                result3.Normalize();
                TSMatrix.CreateFromQuaternion(ref result3, out rigidBody.orientation);
            }
            if ((uint)(rigidBody.Damping & RigidBody.DampingType.Linear) > 0U)
                TSVector.Multiply(ref rigidBody.linearVelocity, this.currentLinearDampFactor, out rigidBody.linearVelocity);
            if ((uint)(rigidBody.Damping & RigidBody.DampingType.Angular) > 0U)
                TSVector.Multiply(ref rigidBody.angularVelocity, this.currentAngularDampFactor, out rigidBody.angularVelocity);
            rigidBody.Update();
            if (!this.CollisionSystem.EnableSpeculativeContacts && !rigidBody.EnableSpeculativeContacts)
                return;
            rigidBody.SweptExpandBoundingBox(this.timestep);
        }

        private void Integrate(bool multithread)
        {
            if (multithread)
            {
                int index = 0;
                for (int count = this.rigidBodies.Count; index < count; ++index)
                {
                    RigidBody rigidBody = this.rigidBodies[index];
                    if (!rigidBody.isStatic && rigidBody.IsActive)
                        this.threadManager.AddTask(this.integrateCallback, (object)rigidBody);
                }
                threadManager.Execute();
            }
            else
            {
                int index = 0;
                for (int count = this.rigidBodies.Count; index < count; ++index)
                {
                    RigidBody rigidBody = this.rigidBodies[index];
                    if (!rigidBody.isStatic && rigidBody.IsActive)
                        this.integrateCallback((object)rigidBody);
                }
            }
        }

        internal bool CanBodiesCollide(RigidBody body1, RigidBody body2)
        {
            if (body1.disabled || body2.disabled || !this.physicsManager.IsCollisionEnabled((IBody)body1, (IBody)body2) || body1.IsStaticNonKinematic && body2.IsStaticNonKinematic)
                return false;
            if (body1.IsColliderOnly || body2.IsColliderOnly)
            {
                if (body1.IsColliderOnly && body1.IsStaticNonKinematic && body2.IsStaticNonKinematic || body2.IsColliderOnly && body2.IsStaticNonKinematic && body1.IsStaticNonKinematic)
                    return false;
            }
            else if (body1.isKinematic && body2.isStatic || body2.isKinematic && body1.isStatic)
                return false;
            return true;
        }

        private void CollisionDetected(RigidBody body1, RigidBody body2, TSVector point1, TSVector point2, TSVector normal, FP penetration)
        {
            bool flag1 = body1.IsColliderOnly || body2.IsColliderOnly;
            Arbiter arbiter = (Arbiter)null;
            ArbiterMap arbiterMap = !flag1 ? this.arbiterMap : this.arbiterTriggerMap;
            bool flag2 = false;
            lock (arbiterMap)
            {
                arbiterMap.LookUpArbiter(body1, body2, out arbiter);
                if (arbiter == null)
                {
                    arbiter = Arbiter.Pool.GetNew();
                    arbiter.body1 = body1;
                    arbiter.body2 = body2;
                    arbiterMap.Add(new ArbiterKey(body1, body2), arbiter);
                    flag2 = true;
                }
            }
            Contact contact;
            if (arbiter.body1 == body1)
            {
                TSVector.Negate(ref normal, out normal);
                contact = arbiter.AddContact(point1, point2, normal, penetration, this.contactSettings);
            }
            else
                contact = arbiter.AddContact(point2, point1, normal, penetration, this.contactSettings);
            if (flag2)
            {
                if (flag1)
                {
                    this.events.RaiseTriggerBeginCollide(contact);
                    body1.arbitersTrigger.Add(arbiter);
                    body2.arbitersTrigger.Add(arbiter);
                    this.initialTriggers.Add(new OverlapPairContact((IBroadphaseEntity)body1, (IBroadphaseEntity)body2)
                    {
                        contact = contact
                    });
                }
                else
                {
                    this.events.RaiseBodiesBeginCollide(contact);
                    this.addedArbiterQueue.Enqueue(arbiter);
                    this.initialCollisions.Add(new OverlapPairContact((IBroadphaseEntity)body1, (IBroadphaseEntity)body2)
                    {
                        contact = contact
                    });
                }
            }
            if (flag1 || contact == null)
                return;
            this.events.RaiseContactCreated(contact);
        }

        private void CheckDeactivation()
        {
            if (!this.AllowDeactivation)
                return;
            foreach (CollisionIsland island in (List<CollisionIsland>)this.islands)
            {
                bool flag = true;
                if (!this.AllowDeactivation)
                {
                    flag = false;
                }
                else
                {
                    int index = 0;
                    for (int count = island.bodies.Count; index < count; ++index)
                    {
                        RigidBody body = island.bodies[index];
                        if (body.AllowDeactivation && (body.angularVelocity.sqrMagnitude < this.inactiveAngularThresholdSq && body.linearVelocity.sqrMagnitude < this.inactiveLinearThresholdSq))
                        {
                            body.inactiveTime += this.timestep;
                            if (body.inactiveTime < this.deactivationTime)
                                flag = false;
                        }
                        else
                        {
                            body.inactiveTime = FP.Zero;
                            flag = false;
                        }
                    }
                }
                int index1 = 0;
                for (int count = island.bodies.Count; index1 < count; ++index1)
                {
                    RigidBody body = island.bodies[index1];
                    if (body.isActive == flag)
                    {
                        if (body.isActive)
                        {
                            body.IsActive = false;
                            this.events.RaiseDeactivatedBody(body);
                        }
                        else
                        {
                            body.IsActive = true;
                            this.events.RaiseActivatedBody(body);
                        }
                    }
                }
            }
        }

        public List<IBody> Bodies()
        {
            List<IBody> bodyList = new List<IBody>();
            for (int index = 0; index < this.rigidBodies.Count; ++index)
                bodyList.Add((IBody)this.rigidBodies[index]);
            return bodyList;
        }

        public delegate void WorldStep(FP timestep);

        public class WorldEvents
        {
            public event World.WorldStep PreStep;

            public event World.WorldStep PostStep;

            public event Action<RigidBody> AddedRigidBody;

            public event Action<RigidBody> RemovedRigidBody;

            public event Action<Constraint> AddedConstraint;

            public event Action<Constraint> RemovedConstraint;

            public event Action<SoftBody> AddedSoftBody;

            public event Action<SoftBody> RemovedSoftBody;

            public event Action<Contact> BodiesBeginCollide;

            public event Action<Contact> BodiesStayCollide;

            public event Action<RigidBody, RigidBody> BodiesEndCollide;

            public event Action<Contact> TriggerBeginCollide;

            public event Action<Contact> TriggerStayCollide;

            public event Action<RigidBody, RigidBody> TriggerEndCollide;

            public event Action<Contact> ContactCreated;

            public event Action<RigidBody> DeactivatedBody;

            public event Action<RigidBody> ActivatedBody;

            internal WorldEvents()
            {
            }

            internal void RaiseWorldPreStep(FP timestep)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.PreStep == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.PreStep(timestep);
            }

            internal void RaiseWorldPostStep(FP timestep)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.PostStep == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.PostStep(timestep);
            }

            internal void RaiseAddedRigidBody(RigidBody body)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.AddedRigidBody == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.AddedRigidBody(body);
            }

            internal void RaiseRemovedRigidBody(RigidBody body)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.RemovedRigidBody == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.RemovedRigidBody(body);
            }

            internal void RaiseAddedConstraint(Constraint constraint)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.AddedConstraint == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.AddedConstraint(constraint);
            }

            internal void RaiseRemovedConstraint(Constraint constraint)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.RemovedConstraint == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.RemovedConstraint(constraint);
            }

            internal void RaiseAddedSoftBody(SoftBody body)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.AddedSoftBody == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.AddedSoftBody(body);
            }

            internal void RaiseRemovedSoftBody(SoftBody body)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.RemovedSoftBody == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.RemovedSoftBody(body);
            }

            internal void RaiseBodiesBeginCollide(Contact contact)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.BodiesBeginCollide == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.BodiesBeginCollide(contact);
            }

            internal void RaiseBodiesStayCollide(Contact contact)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.BodiesStayCollide == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.BodiesStayCollide(contact);
            }

            internal void RaiseBodiesEndCollide(RigidBody body1, RigidBody body2)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.BodiesEndCollide == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.BodiesEndCollide(body1, body2);
            }

            internal void RaiseTriggerBeginCollide(Contact contact)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.TriggerBeginCollide == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.TriggerBeginCollide(contact);
            }

            internal void RaiseTriggerStayCollide(Contact contact)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.TriggerStayCollide == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.TriggerStayCollide(contact);
            }

            internal void RaiseTriggerEndCollide(RigidBody body1, RigidBody body2)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.TriggerEndCollide == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.TriggerEndCollide(body1, body2);
            }

            internal void RaiseActivatedBody(RigidBody body)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.ActivatedBody == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.ActivatedBody(body);
            }

            internal void RaiseDeactivatedBody(RigidBody body)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.DeactivatedBody == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.DeactivatedBody(body);
            }

            internal void RaiseContactCreated(Contact contact)
            {
                // ISSUE: reference to a compiler-generated field
                if (this.ContactCreated == null)
                    return;
                // ISSUE: reference to a compiler-generated field
                this.ContactCreated(contact);
            }
        }
    }
}
