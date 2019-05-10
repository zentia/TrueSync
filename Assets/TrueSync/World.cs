using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace TrueSync
{
	public class World : IWorld
	{
		public delegate void WorldStep(FP timestep);

		public class WorldEvents
		{
			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event World.WorldStep PreStep;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event World.WorldStep PostStep;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<RigidBody> AddedRigidBody;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<RigidBody> RemovedRigidBody;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<Constraint> AddedConstraint;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<Constraint> RemovedConstraint;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<SoftBody> AddedSoftBody;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<SoftBody> RemovedSoftBody;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<Contact> BodiesBeginCollide;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<Contact> BodiesStayCollide;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<RigidBody, RigidBody> BodiesEndCollide;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<Contact> TriggerBeginCollide;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<Contact> TriggerStayCollide;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<RigidBody, RigidBody> TriggerEndCollide;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<Contact> ContactCreated;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<RigidBody> DeactivatedBody;

			[method: CompilerGenerated]
			[DebuggerBrowsable(DebuggerBrowsableState.Never), CompilerGenerated]
			public event Action<RigidBody> ActivatedBody;

			internal WorldEvents()
			{
			}

			internal void RaiseWorldPreStep(FP timestep)
			{
				bool flag = this.PreStep != null;
				if (flag)
				{
					this.PreStep(timestep);
				}
			}

			internal void RaiseWorldPostStep(FP timestep)
			{
				bool flag = this.PostStep != null;
				if (flag)
				{
					this.PostStep(timestep);
				}
			}

			internal void RaiseAddedRigidBody(RigidBody body)
			{
				bool flag = this.AddedRigidBody != null;
				if (flag)
				{
					this.AddedRigidBody(body);
				}
			}

			internal void RaiseRemovedRigidBody(RigidBody body)
			{
				bool flag = this.RemovedRigidBody != null;
				if (flag)
				{
					this.RemovedRigidBody(body);
				}
			}

			internal void RaiseAddedConstraint(Constraint constraint)
			{
				bool flag = this.AddedConstraint != null;
				if (flag)
				{
					this.AddedConstraint(constraint);
				}
			}

			internal void RaiseRemovedConstraint(Constraint constraint)
			{
				bool flag = this.RemovedConstraint != null;
				if (flag)
				{
					this.RemovedConstraint(constraint);
				}
			}

			internal void RaiseAddedSoftBody(SoftBody body)
			{
				bool flag = this.AddedSoftBody != null;
				if (flag)
				{
					this.AddedSoftBody(body);
				}
			}

			internal void RaiseRemovedSoftBody(SoftBody body)
			{
				bool flag = this.RemovedSoftBody != null;
				if (flag)
				{
					this.RemovedSoftBody(body);
				}
			}

			internal void RaiseBodiesBeginCollide(Contact contact)
			{
				bool flag = this.BodiesBeginCollide != null;
				if (flag)
				{
					this.BodiesBeginCollide(contact);
				}
			}

			internal void RaiseBodiesStayCollide(Contact contact)
			{
				bool flag = this.BodiesStayCollide != null;
				if (flag)
				{
					this.BodiesStayCollide(contact);
				}
			}

			internal void RaiseBodiesEndCollide(RigidBody body1, RigidBody body2)
			{
				bool flag = this.BodiesEndCollide != null;
				if (flag)
				{
					this.BodiesEndCollide(body1, body2);
				}
			}

			internal void RaiseTriggerBeginCollide(Contact contact)
			{
				bool flag = this.TriggerBeginCollide != null;
				if (flag)
				{
					this.TriggerBeginCollide(contact);
				}
			}

			internal void RaiseTriggerStayCollide(Contact contact)
			{
				bool flag = this.TriggerStayCollide != null;
				if (flag)
				{
					this.TriggerStayCollide(contact);
				}
			}

			internal void RaiseTriggerEndCollide(RigidBody body1, RigidBody body2)
			{
				bool flag = this.TriggerEndCollide != null;
				if (flag)
				{
					this.TriggerEndCollide(body1, body2);
				}
			}

			internal void RaiseActivatedBody(RigidBody body)
			{
				bool flag = this.ActivatedBody != null;
				if (flag)
				{
					this.ActivatedBody(body);
				}
			}

			internal void RaiseDeactivatedBody(RigidBody body)
			{
				bool flag = this.DeactivatedBody != null;
				if (flag)
				{
					this.DeactivatedBody(body);
				}
			}

			internal void RaiseContactCreated(Contact contact)
			{
				bool flag = this.ContactCreated != null;
				if (flag)
				{
					this.ContactCreated(contact);
				}
			}
		}

		private ContactSettings contactSettings = new ContactSettings();

		private FP inactiveAngularThresholdSq = FP.EN1;

		private FP inactiveLinearThresholdSq = FP.EN1;

		private FP deactivationTime = 2;

		private FP angularDamping = 85 * FP.EN2;

		private FP linearDamping = 85 * FP.EN2;

		private int contactIterations = 6;

		private int smallIterations = 3;

		private FP timestep = FP.Zero;

		public IslandManager islands = new IslandManager();

		public HashList<OverlapPairContact> initialCollisions = new HashList<OverlapPairContact>();

		public HashList<OverlapPairContact> initialTriggers = new HashList<OverlapPairContact>();

		private OverlapPairContact cacheOverPairContact = new OverlapPairContact(null, null);

		internal HashList<RigidBody> rigidBodies = new HashList<RigidBody>();

		internal HashList<Constraint> constraints = new HashList<Constraint>();

		internal HashList<SoftBody> softbodies = new HashList<SoftBody>();

		private World.WorldEvents events = new World.WorldEvents();

		private ThreadManager threadManager = ThreadManager.Instance;

		private ArbiterMap arbiterMap;

		private ArbiterMap arbiterTriggerMap;

		public Queue<Arbiter> removedArbiterQueue = new Queue<Arbiter>();

		public Queue<Arbiter> addedArbiterQueue = new Queue<Arbiter>();

		private TSVector gravity = new TSVector(0, -981 * FP.EN2, 0);

		private Action<object> arbiterCallback;

		private Action<object> integrateCallback;

		private CollisionDetectedHandler collisionDetectionHandler;

		public IPhysicsManager physicsManager;

		private FP currentLinearDampFactor = FP.One;

		private FP currentAngularDampFactor = FP.One;

		public FP accumulatedTime = FP.Zero;

		public Stack<Arbiter> removedArbiterStack = new Stack<Arbiter>();

		public ReadOnlyHashset<RigidBody> RigidBodies
		{
			get;
			private set;
		}

		public ReadOnlyHashset<Constraint> Constraints
		{
			get;
			private set;
		}

		public ReadOnlyHashset<SoftBody> SoftBodies
		{
			get;
			private set;
		}

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
				return this.islands;
			}
		}

		public CollisionSystem CollisionSystem
		{
			get;
			set;
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

		public bool AllowDeactivation
		{
			get;
			set;
		}

		public World(CollisionSystem collision)
		{
			bool flag = collision == null;
			if (flag)
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
			this.arbiterMap = new ArbiterMap();
			this.arbiterTriggerMap = new ArbiterMap();
			this.AllowDeactivation = false;
		}

		public void AddBody(SoftBody body)
		{
			bool flag = body == null;
			if (flag)
			{
				throw new ArgumentNullException("body", "body can't be null.");
			}
			bool flag2 = this.softbodies.Contains(body);
			if (flag2)
			{
				throw new ArgumentException("The body was already added to the world.", "body");
			}
			this.softbodies.Add(body);
			this.CollisionSystem.AddEntity(body);
			this.events.RaiseAddedSoftBody(body);
			foreach (Constraint current in body.EdgeSprings)
			{
				this.AddConstraint(current);
			}
			foreach (SoftBody.MassPoint current2 in body.VertexBodies)
			{
				this.events.RaiseAddedRigidBody(current2);
				this.rigidBodies.Add(current2);
			}
		}

		public bool RemoveBody(SoftBody body)
		{
			bool flag = !this.softbodies.Remove(body);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				this.CollisionSystem.RemoveEntity(body);
				this.events.RaiseRemovedSoftBody(body);
				foreach (Constraint current in body.EdgeSprings)
				{
					this.RemoveConstraint(current);
				}
				foreach (SoftBody.MassPoint current2 in body.VertexBodies)
				{
					this.RemoveBody(current2, true);
				}
				result = true;
			}
			return result;
		}

		public void ResetResourcePools()
		{
			IslandManager.Pool.ResetResourcePool();
			Arbiter.Pool.ResetResourcePool();
			Contact.Pool.ResetResourcePool();
		}

		public void Clear()
		{
			int i = 0;
			int count = this.rigidBodies.Count;
			while (i < count)
			{
				RigidBody rigidBody = this.rigidBodies[i];
				this.CollisionSystem.RemoveEntity(rigidBody);
				bool flag = rigidBody.island != null;
				if (flag)
				{
					rigidBody.island.ClearLists();
					rigidBody.island = null;
				}
				rigidBody.connections.Clear();
				rigidBody.arbiters.Clear();
				rigidBody.constraints.Clear();
				this.events.RaiseRemovedRigidBody(rigidBody);
				i++;
			}
			int j = 0;
			int count2 = this.softbodies.Count;
			while (j < count2)
			{
				SoftBody body = this.softbodies[j];
				this.CollisionSystem.RemoveEntity(body);
				j++;
			}
			this.rigidBodies.Clear();
			int k = 0;
			int count3 = this.constraints.Count;
			while (k < count3)
			{
				Constraint constraint = this.constraints[k];
				this.events.RaiseRemovedConstraint(constraint);
				k++;
			}
			this.constraints.Clear();
			this.softbodies.Clear();
			this.islands.RemoveAll();
			this.arbiterMap.Clear();
			this.arbiterTriggerMap.Clear();
			this.ResetResourcePools();
		}

		public void SetDampingFactors(FP angularDamping, FP linearDamping)
		{
			bool flag = angularDamping < FP.Zero || angularDamping > FP.One;
			if (flag)
			{
				throw new ArgumentException("Angular damping factor has to be between 0.0 and 1.0", "angularDamping");
			}
			bool flag2 = linearDamping < FP.Zero || linearDamping > FP.One;
			if (flag2)
			{
				throw new ArgumentException("Linear damping factor has to be between 0.0 and 1.0", "linearDamping");
			}
			this.angularDamping = angularDamping;
			this.linearDamping = linearDamping;
		}

		public void SetInactivityThreshold(FP angularVelocity, FP linearVelocity, FP time)
		{
			bool flag = angularVelocity < FP.Zero;
			if (flag)
			{
				throw new ArgumentException("Angular velocity threshold has to be larger than zero", "angularVelocity");
			}
			bool flag2 = linearVelocity < FP.Zero;
			if (flag2)
			{
				throw new ArgumentException("Linear velocity threshold has to be larger than zero", "linearVelocity");
			}
			bool flag3 = time < FP.Zero;
			if (flag3)
			{
				throw new ArgumentException("Deactivation time threshold has to be larger than zero", "time");
			}
			this.inactiveAngularThresholdSq = angularVelocity * angularVelocity;
			this.inactiveLinearThresholdSq = linearVelocity * linearVelocity;
			this.deactivationTime = time;
		}

		public void SetIterations(int iterations, int smallIterations)
		{
			bool flag = iterations < 1;
			if (flag)
			{
				throw new ArgumentException("The number of collision iterations has to be larger than zero", "iterations");
			}
			bool flag2 = smallIterations < 1;
			if (flag2)
			{
				throw new ArgumentException("The number of collision iterations has to be larger than zero", "smallIterations");
			}
			this.contactIterations = iterations;
			this.smallIterations = smallIterations;
		}

		public bool RemoveBody(RigidBody body)
		{
			return this.RemoveBody(body, false);
		}

		private bool RemoveBody(RigidBody body, bool removeMassPoints)
		{
			bool flag = !removeMassPoints && body.IsParticle;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = !this.rigidBodies.Remove(body);
				if (flag2)
				{
					result = false;
				}
				else
				{
					int i = 0;
					int count = body.arbiters.Count;
					while (i < count)
					{
						Arbiter arbiter = body.arbiters[i];
						this.arbiterMap.Remove(arbiter);
						this.events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
						this.cacheOverPairContact.SetBodies(arbiter.body1, arbiter.body2);
						this.initialCollisions.Remove(this.cacheOverPairContact);
						i++;
					}
					int j = 0;
					int count2 = body.arbitersTrigger.Count;
					while (j < count2)
					{
						Arbiter arbiter2 = body.arbitersTrigger[j];
						this.arbiterTriggerMap.Remove(arbiter2);
						bool isColliderOnly = arbiter2.body1.isColliderOnly;
						if (isColliderOnly)
						{
							this.events.RaiseTriggerEndCollide(arbiter2.body1, arbiter2.body2);
						}
						else
						{
							this.events.RaiseTriggerEndCollide(arbiter2.body2, arbiter2.body1);
						}
						this.cacheOverPairContact.SetBodies(arbiter2.body1, arbiter2.body2);
						this.initialTriggers.Remove(this.cacheOverPairContact);
						j++;
					}
					int k = 0;
					int count3 = body.constraints.Count;
					while (k < count3)
					{
						Constraint constraint = body.constraints[k];
						this.constraints.Remove(constraint);
						this.events.RaiseRemovedConstraint(constraint);
						k++;
					}
					this.CollisionSystem.RemoveEntity(body);
					this.islands.RemoveBody(body);
					this.events.RaiseRemovedRigidBody(body);
					result = true;
				}
			}
			return result;
		}

		public void AddBody(RigidBody body)
		{
			bool flag = body == null;
			if (flag)
			{
				throw new ArgumentNullException("body", "body can't be null.");
			}
			bool flag2 = this.rigidBodies.Contains(body);
			if (flag2)
			{
				throw new ArgumentException("The body was already added to the world.", "body");
			}
			this.events.RaiseAddedRigidBody(body);
			this.CollisionSystem.AddEntity(body);
			this.rigidBodies.Add(body);
		}

		public bool RemoveConstraint(Constraint constraint)
		{
			bool flag = !this.constraints.Remove(constraint);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				this.events.RaiseRemovedConstraint(constraint);
				this.islands.ConstraintRemoved(constraint);
				result = true;
			}
			return result;
		}

		public void AddConstraint(Constraint constraint)
		{
			bool flag = this.constraints.Contains(constraint);
			if (flag)
			{
				throw new ArgumentException("The constraint was already added to the world.", "constraint");
			}
			this.constraints.Add(constraint);
			this.islands.ConstraintCreated(constraint);
			this.events.RaiseAddedConstraint(constraint);
		}

		public void Step(FP timestep, bool multithread)
		{
			this.timestep = timestep;
			bool flag = timestep == FP.Zero;
			if (!flag)
			{
				bool flag2 = timestep < FP.Zero;
				if (flag2)
				{
					throw new ArgumentException("The timestep can't be negative.", "timestep");
				}
				this.currentAngularDampFactor = FP.One;
				this.currentLinearDampFactor = FP.One;
				this.events.RaiseWorldPreStep(timestep);
				this.UpdateContacts();
				int i = 0;
				int count = this.initialCollisions.Count;
				while (i < count)
				{
					OverlapPairContact overlapPairContact = this.initialCollisions[i];
					this.events.RaiseBodiesStayCollide(overlapPairContact.contact);
					i++;
				}
				int j = 0;
				int count2 = this.initialTriggers.Count;
				while (j < count2)
				{
					OverlapPairContact overlapPairContact2 = this.initialTriggers[j];
					this.events.RaiseTriggerStayCollide(overlapPairContact2.contact);
					j++;
				}
				while (this.removedArbiterQueue.Count > 0)
				{
					this.islands.ArbiterRemoved(this.removedArbiterQueue.Dequeue());
				}
				int k = 0;
				int count3 = this.softbodies.Count;
				while (k < count3)
				{
					SoftBody softBody = this.softbodies[k];
					softBody.Update(timestep);
					softBody.DoSelfCollision(this.collisionDetectionHandler);
					k++;
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
				int l = 0;
				int count4 = this.rigidBodies.Count;
				while (l < count4)
				{
					RigidBody rigidBody = this.rigidBodies[l];
					rigidBody.PostStep();
					int m = 0;
					int count5 = rigidBody.constraints.Count;
					while (m < count5)
					{
						rigidBody.constraints[m].PostStep();
						m++;
					}
					l++;
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
				bool flag = num > maxSteps;
				if (flag)
				{
					this.accumulatedTime = FP.Zero;
					break;
				}
			}
		}

		private void UpdateArbiterContacts(Arbiter arbiter)
		{
			bool flag = arbiter.contactList.Count == 0;
			if (flag)
			{
				Stack<Arbiter> obj = this.removedArbiterStack;
				lock (obj)
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
					bool flag2 = contact.penetration < -this.contactSettings.breakThreshold;
					if (flag2)
					{
						Contact.Pool.GiveBack(contact);
						arbiter.contactList.RemoveAt(i);
					}
					else
					{
						TSVector value;
						TSVector.Subtract(ref contact.p1, ref contact.p2, out value);
						FP fP = TSVector.Dot(ref value, ref contact.normal);
						value -= fP * contact.normal;
						fP = value.sqrMagnitude;
						bool flag3 = fP > this.contactSettings.breakThreshold * this.contactSettings.breakThreshold * 100;
						if (flag3)
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

		private void UpdateContacts(ArbiterMap selectedArbiterMap)
		{
			foreach (Arbiter current in selectedArbiterMap.Arbiters)
			{
				this.UpdateArbiterContacts(current);
			}
			while (this.removedArbiterStack.Count > 0)
			{
				Arbiter arbiter = this.removedArbiterStack.Pop();
				Arbiter.Pool.GiveBack(arbiter);
				selectedArbiterMap.Remove(arbiter);
				bool flag = selectedArbiterMap == this.arbiterMap;
				if (flag)
				{
					this.removedArbiterQueue.Enqueue(arbiter);
					this.events.RaiseBodiesEndCollide(arbiter.body1, arbiter.body2);
					this.cacheOverPairContact.SetBodies(arbiter.body1, arbiter.body2);
					this.initialCollisions.Remove(this.cacheOverPairContact);
				}
				else
				{
					bool isColliderOnly = arbiter.body1.isColliderOnly;
					if (isColliderOnly)
					{
						this.events.RaiseTriggerEndCollide(arbiter.body1, arbiter.body2);
					}
					else
					{
						this.events.RaiseTriggerEndCollide(arbiter.body2, arbiter.body1);
					}
					this.cacheOverPairContact.SetBodies(arbiter.body1, arbiter.body2);
					this.initialTriggers.Remove(this.cacheOverPairContact);
				}
			}
		}

		private void ArbiterCallback(object obj)
		{
			CollisionIsland collisionIsland = obj as CollisionIsland;
			bool flag = collisionIsland.Bodies.Count + collisionIsland.Constraints.Count > 3;
			int num;
			if (flag)
			{
				num = this.contactIterations;
			}
			else
			{
				num = this.smallIterations;
			}
			for (int i = -1; i < num; i++)
			{
				int j = 0;
				int count = collisionIsland.arbiter.Count;
				while (j < count)
				{
					Arbiter arbiter = collisionIsland.arbiter[j];
					int count2 = arbiter.contactList.Count;
					for (int k = 0; k < count2; k++)
					{
						bool flag2 = i == -1;
						if (flag2)
						{
							arbiter.contactList[k].PrepareForIteration(this.timestep);
						}
						else
						{
							arbiter.contactList[k].Iterate();
						}
					}
					j++;
				}
				int l = 0;
				int count3 = collisionIsland.constraints.Count;
				while (l < count3)
				{
					Constraint constraint = collisionIsland.constraints[l];
					bool flag3 = constraint.body1 != null && !constraint.body1.IsActive && constraint.body2 != null && !constraint.body2.IsActive;
					if (!flag3)
					{
						bool flag4 = i == -1;
						if (flag4)
						{
							constraint.PrepareForIteration(this.timestep);
						}
						else
						{
							constraint.Iterate();
						}
					}
					l++;
				}
			}
		}

		private void HandleArbiter(int iterations, bool multiThreaded)
		{
			if (multiThreaded)
			{
				for (int i = 0; i < this.islands.Count; i++)
				{
					bool flag = this.islands[i].IsActive();
					if (flag)
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
					bool flag2 = this.islands[j].IsActive();
					if (flag2)
					{
						this.arbiterCallback(this.islands[j]);
					}
				}
			}
		}

		private void IntegrateForces()
		{
			int i = 0;
			int count = this.rigidBodies.Count;
			while (i < count)
			{
				RigidBody rigidBody = this.rigidBodies[i];
				bool flag = !rigidBody.isStatic && rigidBody.IsActive;
				if (flag)
				{
					TSVector tSVector;
					TSVector.Multiply(ref rigidBody.force, rigidBody.inverseMass * this.timestep, out tSVector);
					TSVector.Add(ref tSVector, ref rigidBody.linearVelocity, out rigidBody.linearVelocity);
					bool flag2 = !rigidBody.isParticle;
					if (flag2)
					{
						TSVector.Multiply(ref rigidBody.torque, this.timestep, out tSVector);
						TSVector.Transform(ref tSVector, ref rigidBody.invInertiaWorld, out tSVector);
						TSVector.Add(ref tSVector, ref rigidBody.angularVelocity, out rigidBody.angularVelocity);
					}
					bool affectedByGravity = rigidBody.affectedByGravity;
					if (affectedByGravity)
					{
						TSVector.Multiply(ref this.gravity, this.timestep, out tSVector);
						TSVector.Add(ref rigidBody.linearVelocity, ref tSVector, out rigidBody.linearVelocity);
					}
				}
				rigidBody.force.MakeZero();
				rigidBody.torque.MakeZero();
				i++;
			}
		}

		private void IntegrateCallback(object obj)
		{
			RigidBody rigidBody = obj as RigidBody;
			TSVector tSVector;
			TSVector.Multiply(ref rigidBody.linearVelocity, this.timestep, out tSVector);
			TSVector.Add(ref tSVector, ref rigidBody.position, out rigidBody.position);
			bool flag = !rigidBody.isParticle;
			if (flag)
			{
				FP magnitude = rigidBody.angularVelocity.magnitude;
				bool flag2 = magnitude < FP.EN3;
				TSVector tSVector2;
				if (flag2)
				{
					TSVector.Multiply(ref rigidBody.angularVelocity, FP.Half * this.timestep - this.timestep * this.timestep * this.timestep * (2082 * FP.EN6) * magnitude * magnitude, out tSVector2);
				}
				else
				{
					TSVector.Multiply(ref rigidBody.angularVelocity, FP.Sin(FP.Half * magnitude * this.timestep) / magnitude, out tSVector2);
				}
				TSQuaternion tSQuaternion = new TSQuaternion(tSVector2.x, tSVector2.y, tSVector2.z, FP.Cos(magnitude * this.timestep * FP.Half));
				TSQuaternion tSQuaternion2;
				TSQuaternion.CreateFromMatrix(ref rigidBody.orientation, out tSQuaternion2);
				TSQuaternion.Multiply(ref tSQuaternion, ref tSQuaternion2, out tSQuaternion);
				tSQuaternion.Normalize();
				TSMatrix.CreateFromQuaternion(ref tSQuaternion, out rigidBody.orientation);
			}
			bool flag3 = (rigidBody.Damping & RigidBody.DampingType.Linear) > RigidBody.DampingType.None;
			if (flag3)
			{
				TSVector.Multiply(ref rigidBody.linearVelocity, this.currentLinearDampFactor, out rigidBody.linearVelocity);
			}
			bool flag4 = (rigidBody.Damping & RigidBody.DampingType.Angular) > RigidBody.DampingType.None;
			if (flag4)
			{
				TSVector.Multiply(ref rigidBody.angularVelocity, this.currentAngularDampFactor, out rigidBody.angularVelocity);
			}
			rigidBody.Update();
			bool flag5 = this.CollisionSystem.EnableSpeculativeContacts || rigidBody.EnableSpeculativeContacts;
			if (flag5)
			{
				rigidBody.SweptExpandBoundingBox(this.timestep);
			}
		}

		private void Integrate(bool multithread)
		{
			if (multithread)
			{
				int i = 0;
				int count = this.rigidBodies.Count;
				while (i < count)
				{
					RigidBody rigidBody = this.rigidBodies[i];
					bool flag = rigidBody.isStatic || !rigidBody.IsActive;
					if (!flag)
					{
						this.threadManager.AddTask(this.integrateCallback, rigidBody);
					}
					i++;
				}
				this.threadManager.Execute();
			}
			else
			{
				int j = 0;
				int count2 = this.rigidBodies.Count;
				while (j < count2)
				{
					RigidBody rigidBody2 = this.rigidBodies[j];
					bool flag2 = rigidBody2.isStatic || !rigidBody2.IsActive;
					if (!flag2)
					{
						this.integrateCallback(rigidBody2);
					}
					j++;
				}
			}
		}

		internal bool CanBodiesCollide(RigidBody body1, RigidBody body2)
		{
			bool flag = body1.disabled || body2.disabled || !this.physicsManager.IsCollisionEnabled(body1, body2);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = body1.IsStaticNonKinematic && body2.IsStaticNonKinematic;
				if (flag2)
				{
					result = false;
				}
				else
				{
					bool flag3 = body1.IsColliderOnly || body2.IsColliderOnly;
					bool flag4 = flag3;
					if (flag4)
					{
						bool flag5 = (body1.IsColliderOnly && body1.IsStaticNonKinematic && body2.IsStaticNonKinematic) || (body2.IsColliderOnly && body2.IsStaticNonKinematic && body1.IsStaticNonKinematic);
						if (flag5)
						{
							result = false;
							return result;
						}
					}
					else
					{
						bool flag6 = (body1.isKinematic && body2.isStatic) || (body2.isKinematic && body1.isStatic);
						if (flag6)
						{
							result = false;
							return result;
						}
					}
					result = true;
				}
			}
			return result;
		}

		private void CollisionDetected(RigidBody body1, RigidBody body2, TSVector point1, TSVector point2, TSVector normal, FP penetration)
		{
			bool flag = body1.IsColliderOnly || body2.IsColliderOnly;
			Arbiter arbiter = null;
			bool flag2 = flag;
			ArbiterMap arbiterMap;
			if (flag2)
			{
				arbiterMap = this.arbiterTriggerMap;
			}
			else
			{
				arbiterMap = this.arbiterMap;
			}
			bool flag3 = false;
			ArbiterMap obj = arbiterMap;
			lock (obj)
			{
				arbiterMap.LookUpArbiter(body1, body2, out arbiter);
				bool flag4 = arbiter == null;
				if (flag4)
				{
					arbiter = Arbiter.Pool.GetNew();
					arbiter.body1 = body1;
					arbiter.body2 = body2;
					arbiterMap.Add(new ArbiterKey(body1, body2), arbiter);
					flag3 = true;
				}
			}
			bool flag5 = arbiter.body1 == body1;
			Contact contact;
			if (flag5)
			{
				TSVector.Negate(ref normal, out normal);
				contact = arbiter.AddContact(point1, point2, normal, penetration, this.contactSettings);
			}
			else
			{
				contact = arbiter.AddContact(point2, point1, normal, penetration, this.contactSettings);
			}
			bool flag6 = flag3;
			if (flag6)
			{
				bool flag7 = flag;
				if (flag7)
				{
					this.events.RaiseTriggerBeginCollide(contact);
					body1.arbitersTrigger.Add(arbiter);
					body2.arbitersTrigger.Add(arbiter);
					OverlapPairContact overlapPairContact = new OverlapPairContact(body1, body2);
					overlapPairContact.contact = contact;
					this.initialTriggers.Add(overlapPairContact);
				}
				else
				{
					this.events.RaiseBodiesBeginCollide(contact);
					this.addedArbiterQueue.Enqueue(arbiter);
					OverlapPairContact overlapPairContact2 = new OverlapPairContact(body1, body2);
					overlapPairContact2.contact = contact;
					this.initialCollisions.Add(overlapPairContact2);
				}
			}
			bool flag8 = !flag && contact != null;
			if (flag8)
			{
				this.events.RaiseContactCreated(contact);
			}
		}

		private void CheckDeactivation()
		{
			bool flag = !this.AllowDeactivation;
			if (!flag)
			{
				foreach (CollisionIsland current in this.islands)
				{
					bool flag2 = true;
					bool flag3 = !this.AllowDeactivation;
					if (flag3)
					{
						flag2 = false;
					}
					else
					{
						int i = 0;
						int count = current.bodies.Count;
						while (i < count)
						{
							RigidBody rigidBody = current.bodies[i];
							bool flag4 = rigidBody.AllowDeactivation && rigidBody.angularVelocity.sqrMagnitude < this.inactiveAngularThresholdSq && rigidBody.linearVelocity.sqrMagnitude < this.inactiveLinearThresholdSq;
							if (flag4)
							{
								rigidBody.inactiveTime += this.timestep;
								bool flag5 = rigidBody.inactiveTime < this.deactivationTime;
								if (flag5)
								{
									flag2 = false;
								}
							}
							else
							{
								rigidBody.inactiveTime = FP.Zero;
								flag2 = false;
							}
							i++;
						}
					}
					int j = 0;
					int count2 = current.bodies.Count;
					while (j < count2)
					{
						RigidBody rigidBody2 = current.bodies[j];
						bool flag6 = rigidBody2.isActive == flag2;
						if (flag6)
						{
							bool isActive = rigidBody2.isActive;
							if (isActive)
							{
								rigidBody2.IsActive = false;
								this.events.RaiseDeactivatedBody(rigidBody2);
							}
							else
							{
								rigidBody2.IsActive = true;
								this.events.RaiseActivatedBody(rigidBody2);
							}
						}
						j++;
					}
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
	}
}
