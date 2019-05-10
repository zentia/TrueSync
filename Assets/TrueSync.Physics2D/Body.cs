using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class Body : IDisposable, IBody2D, IBody
	{
		[ThreadStatic]
		internal static int _bodyIdCounter;

		internal BodySpecialSensor _specialSensor = BodySpecialSensor.None;

		public List<Body> _specialSensorResults;

		internal FP _angularDamping;

		internal BodyType _bodyType;

		internal FP _inertia;

		internal FP _linearDamping;

		internal FP _mass;

		internal bool _sleepingAllowed = true;

		internal bool _awake = true;

		internal bool _fixedRotation;

		internal bool _enabled = true;

		internal FP _angularVelocity;

		internal TSVector2 _linearVelocity;

		internal TSVector2 _force;

		internal FP _invI;

		internal FP _invMass;

		internal FP _sleepTime;

		internal Sweep _sweep;

		internal FP _torque;

		internal World _world;

		internal Transform _xf;

		internal bool _island;

		internal bool disabled;

		public List<IBodyConstraint> bodyConstraints;

		public PhysicsLogicFilter PhysicsLogicFilter;

		public ControllerFilter ControllerFilter;

		public event OnCollisionEventHandler OnCollision
		{
			add
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture expr_12 = this.FixtureList[i];
					expr_12.OnCollision = (OnCollisionEventHandler)Delegate.Combine(expr_12.OnCollision, value);
				}
			}
			remove
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture expr_12 = this.FixtureList[i];
					expr_12.OnCollision = (OnCollisionEventHandler)Delegate.Remove(expr_12.OnCollision, value);
				}
			}
		}

		public event OnSeparationEventHandler OnSeparation
		{
			add
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture expr_12 = this.FixtureList[i];
					expr_12.OnSeparation = (OnSeparationEventHandler)Delegate.Combine(expr_12.OnSeparation, value);
				}
			}
			remove
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture expr_12 = this.FixtureList[i];
					expr_12.OnSeparation = (OnSeparationEventHandler)Delegate.Remove(expr_12.OnSeparation, value);
				}
			}
		}

		public BodySpecialSensor SpecialSensor
		{
			get
			{
				return this._specialSensor;
			}
			set
			{
				this._specialSensor = value;
				bool flag = value > BodySpecialSensor.None;
				if (flag)
				{
					this._specialSensorResults = new List<Body>();
				}
			}
		}

		public int BodyId
		{
			get;
			private set;
		}

		public int IslandIndex
		{
			get;
			set;
		}

		public FP GravityScale
		{
			get;
			set;
		}

		public object UserData
		{
			get;
			set;
		}

		public FP Revolutions
		{
			get
			{
				return this.Rotation / FP.Pi;
			}
		}

		public BodyType BodyType
		{
			get
			{
				return this._bodyType;
			}
			set
			{
				bool flag = this._bodyType == value;
				if (!flag)
				{
					this._bodyType = value;
					this.ResetMassData();
					bool flag2 = this._bodyType == BodyType.Static;
					if (flag2)
					{
						this._linearVelocity = TSVector2.zero;
						this._angularVelocity = 0f;
						this._sweep.A0 = this._sweep.A;
						this._sweep.C0 = this._sweep.C;
						this.SynchronizeFixtures();
					}
					this.Awake = true;
					this._force = TSVector2.zero;
					this._torque = 0f;
					ContactEdge contactEdge = this.ContactList;
					while (contactEdge != null)
					{
						ContactEdge contactEdge2 = contactEdge;
						contactEdge = contactEdge.Next;
						this._world.ContactManager.Destroy(contactEdge2.Contact);
					}
					this.ContactList = null;
					IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
					foreach (Fixture current in this.FixtureList)
					{
						int proxyCount = current.ProxyCount;
						for (int i = 0; i < proxyCount; i++)
						{
							broadPhase.TouchProxy(current.Proxies[i].ProxyId);
						}
					}
				}
			}
		}

		public TSVector2 LinearVelocity
		{
			get
			{
				return this._linearVelocity;
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value.x) && !FP.IsNaN(value.y));
				bool flag = this._bodyType == BodyType.Static;
				if (!flag)
				{
					bool flag2 = TSVector2.Dot(value, value) > 0f;
					if (flag2)
					{
						this.Awake = true;
					}
					this._linearVelocity = value;
				}
			}
		}

		public FP AngularVelocity
		{
			get
			{
				return this._angularVelocity;
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value));
				bool flag = this._bodyType == BodyType.Static;
				if (!flag)
				{
					bool flag2 = value * value > 0f;
					if (flag2)
					{
						this.Awake = true;
					}
					this._angularVelocity = value;
				}
			}
		}

		public FP LinearDamping
		{
			get
			{
				return this._linearDamping;
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value));
				this._linearDamping = value;
			}
		}

		public FP AngularDamping
		{
			get
			{
				return this._angularDamping;
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value));
				this._angularDamping = value;
			}
		}

		public bool IsBullet
		{
			get;
			set;
		}

		public bool SleepingAllowed
		{
			get
			{
				return this._sleepingAllowed;
			}
			set
			{
				bool flag = !value;
				if (flag)
				{
					this.Awake = true;
				}
				this._sleepingAllowed = value;
			}
		}

		public bool Awake
		{
			get
			{
				return this._awake;
			}
			set
			{
				if (value)
				{
					bool flag = !this._awake;
					if (flag)
					{
						this._sleepTime = 0f;
						this._world.ContactManager.UpdateContacts(this.ContactList, true);
					}
				}
				else
				{
					this.ResetDynamics();
					this._sleepTime = 0f;
					this._world.ContactManager.UpdateContacts(this.ContactList, false);
				}
				this._awake = value;
			}
		}

		public bool Enabled
		{
			get
			{
				return this._enabled;
			}
			set
			{
				bool flag = value == this._enabled;
				if (!flag)
				{
					if (value)
					{
						IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
						for (int i = 0; i < this.FixtureList.Count; i++)
						{
							this.FixtureList[i].CreateProxies(broadPhase, ref this._xf);
						}
					}
					else
					{
						IBroadPhase broadPhase2 = this._world.ContactManager.BroadPhase;
						for (int j = 0; j < this.FixtureList.Count; j++)
						{
							this.FixtureList[j].DestroyProxies(broadPhase2);
						}
						ContactEdge contactEdge = this.ContactList;
						while (contactEdge != null)
						{
							ContactEdge contactEdge2 = contactEdge;
							contactEdge = contactEdge.Next;
							this._world.ContactManager.Destroy(contactEdge2.Contact);
						}
						this.ContactList = null;
					}
					this._enabled = value;
				}
			}
		}

		public bool FixedRotation
		{
			get
			{
				return this._fixedRotation;
			}
			set
			{
				bool flag = this._fixedRotation == value;
				if (!flag)
				{
					this._fixedRotation = value;
					this._angularVelocity = 0f;
					this.ResetMassData();
				}
			}
		}

		public List<Fixture> FixtureList
		{
			get;
			internal set;
		}

		public JointEdge JointList
		{
			get;
			internal set;
		}

		public ContactEdge ContactList
		{
			get;
			internal set;
		}

		public TSVector2 Position
		{
			get
			{
				return this._xf.p;
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value.x) && !FP.IsNaN(value.y));
				this.SetTransform(ref value, this.Rotation);
			}
		}

		public FP Rotation
		{
			get
			{
				return this._sweep.A;
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value));
				this.SetTransform(ref this._xf.p, value);
			}
		}

		public bool IsStatic
		{
			get
			{
				return this._bodyType == BodyType.Static;
			}
			set
			{
				this.BodyType = (value ? BodyType.Static : BodyType.Dynamic);
			}
		}

		public bool TSIsStatic
		{
			get
			{
				return this.IsStatic;
			}
			set
			{
				this.IsStatic = value;
			}
		}

		public bool IsKinematic
		{
			get
			{
				return this._bodyType == BodyType.Kinematic;
			}
			set
			{
				this.BodyType = (value ? BodyType.Kinematic : BodyType.Dynamic);
			}
		}

		public bool IgnoreGravity
		{
			get;
			set;
		}

		public TSVector2 WorldCenter
		{
			get
			{
				return this._sweep.C;
			}
		}

		public TSVector2 LocalCenter
		{
			get
			{
				return this._sweep.LocalCenter;
			}
			set
			{
				bool flag = this._bodyType != BodyType.Dynamic;
				if (!flag)
				{
					TSVector2 c = this._sweep.C;
					this._sweep.LocalCenter = value;
					this._sweep.C0 = (this._sweep.C = MathUtils.Mul(ref this._xf, ref this._sweep.LocalCenter));
					TSVector2 tSVector = this._sweep.C - c;
					this._linearVelocity += new TSVector2(-this._angularVelocity * tSVector.y, this._angularVelocity * tSVector.x);
				}
			}
		}

		public FP Mass
		{
			get
			{
				return this._mass;
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value));
				bool flag = this._bodyType != BodyType.Dynamic;
				if (!flag)
				{
					this._mass = value;
					bool flag2 = this._mass <= 0f;
					if (flag2)
					{
						this._mass = 1f;
					}
					this._invMass = 1f / this._mass;
				}
			}
		}

		public FP Inertia
		{
			get
			{
				return this._inertia + this.Mass * TSVector2.Dot(this._sweep.LocalCenter, this._sweep.LocalCenter);
			}
			set
			{
				Debug.Assert(!FP.IsNaN(value));
				bool flag = this._bodyType != BodyType.Dynamic;
				if (!flag)
				{
					bool flag2 = value > 0f && !this._fixedRotation;
					if (flag2)
					{
						this._inertia = value - this.Mass * TSVector2.Dot(this.LocalCenter, this.LocalCenter);
						Debug.Assert(this._inertia > 0f);
						this._invI = 1f / this._inertia;
					}
				}
			}
		}

		public FP Restitution
		{
			get
			{
				FP x = 0;
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					x += fixture.Restitution;
				}
				return (this.FixtureList.Count > 0) ? (x / this.FixtureList.Count) : 0;
			}
			set
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					fixture.Restitution = value;
				}
			}
		}

		public FP Friction
		{
			get
			{
				FP x = 0;
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					x += fixture.Friction;
				}
				return (this.FixtureList.Count > 0) ? (x / this.FixtureList.Count) : 0;
			}
			set
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					fixture.Friction = value;
				}
			}
		}

		public Category CollisionCategories
		{
			set
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					fixture.CollisionCategories = value;
				}
			}
		}

		public Category CollidesWith
		{
			set
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					fixture.CollidesWith = value;
				}
			}
		}

		public Category IgnoreCCDWith
		{
			set
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					fixture.IgnoreCCDWith = value;
				}
			}
		}

		public short CollisionGroup
		{
			set
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					fixture.CollisionGroup = value;
				}
			}
		}

		public bool IsSensor
		{
			set
			{
				for (int i = 0; i < this.FixtureList.Count; i++)
				{
					Fixture fixture = this.FixtureList[i];
					fixture.IsSensor = value;
				}
			}
		}

		public bool IgnoreCCD
		{
			get;
			set;
		}

		public bool IsDisposed
		{
			get;
			set;
		}

		public bool TSAffectedByGravity
		{
			get
			{
				return !this.IgnoreGravity;
			}
			set
			{
				this.IgnoreGravity = !value;
			}
		}

		public TSVector2 TSLinearVelocity
		{
			get
			{
				return this.LinearVelocity;
			}
			set
			{
				this.LinearVelocity = value;
			}
		}

		public FP TSAngularVelocity
		{
			get
			{
				return this.AngularVelocity;
			}
			set
			{
				this.AngularVelocity = value;
			}
		}

		public FP TSLinearDamping
		{
			get
			{
				return this.LinearDamping;
			}
			set
			{
				this.LinearDamping = value;
			}
		}

		public FP TSAngularDamping
		{
			get
			{
				return this.AngularDamping;
			}
			set
			{
				this.AngularDamping = value;
			}
		}

		public bool TSIsKinematic
		{
			get
			{
				return this.IsKinematic;
			}
			set
			{
				this.IsKinematic = value;
			}
		}

		public bool TSDisabled
		{
			get
			{
				return this.disabled;
			}
			set
			{
				this.disabled = value;
			}
		}

		public TSVector2 TSPosition
		{
			get
			{
				return this.Position;
			}
			set
			{
				this.Position = value;
			}
		}

		public FP TSOrientation
		{
			get
			{
				return this.Rotation;
			}
			set
			{
				this.Rotation = value;
			}
		}

		public Body(World world, TSVector2? position, FP rotation, object userdata = null)
		{
			this.FixtureList = new List<Fixture>();
			this.bodyConstraints = new List<IBodyConstraint>();
			this.BodyId = Body._bodyIdCounter++;
			this._world = world;
			this.UserData = userdata;
			this.GravityScale = 1f;
			this.BodyType = BodyType.Static;
			this.Enabled = true;
			this._xf.q.Set(rotation);
			bool hasValue = position.HasValue;
			if (hasValue)
			{
				this._xf.p = position.Value;
				this._sweep.C0 = this._xf.p;
				this._sweep.C = this._xf.p;
				this._sweep.A0 = rotation;
				this._sweep.A = rotation;
			}
			world.AddBody(this);
		}

		public void ResetDynamics()
		{
			this._torque = 0;
			this._angularVelocity = 0;
			this._force = TSVector2.zero;
			this._linearVelocity = TSVector2.zero;
		}

		public Fixture CreateFixture(Shape shape, object userData = null)
		{
			return new Fixture(this, shape, userData);
		}

		public void DestroyFixture(Fixture fixture)
		{
			Debug.Assert(fixture.Body == this);
			Debug.Assert(this.FixtureList.Count > 0);
			Debug.Assert(this.FixtureList.Contains(fixture));
			ContactEdge contactEdge = this.ContactList;
			while (contactEdge != null)
			{
				Contact contact = contactEdge.Contact;
				contactEdge = contactEdge.Next;
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				bool flag = fixture == fixtureA || fixture == fixtureB;
				if (flag)
				{
					this._world.ContactManager.Destroy(contact);
				}
			}
			bool enabled = this._enabled;
			if (enabled)
			{
				IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
				fixture.DestroyProxies(broadPhase);
			}
			this.FixtureList.Remove(fixture);
			fixture.Destroy();
			fixture.Body = null;
			this.ResetMassData();
		}

		public void SetTransform(ref TSVector2 position, FP rotation)
		{
			this.SetTransformIgnoreContacts(ref position, rotation);
			this._world.ContactManager.FindNewContacts();
		}

		public void SetTransform(TSVector2 position, FP rotation)
		{
			this.SetTransform(ref position, rotation);
		}

		public void SetTransformIgnoreContacts(ref TSVector2 position, FP angle)
		{
			this._xf.q.Set(angle);
			this._xf.p = position;
			this._sweep.C = MathUtils.Mul(ref this._xf, this._sweep.LocalCenter);
			this._sweep.A = angle;
			this._sweep.C0 = this._sweep.C;
			this._sweep.A0 = angle;
			IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
			for (int i = 0; i < this.FixtureList.Count; i++)
			{
				this.FixtureList[i].Synchronize(broadPhase, ref this._xf, ref this._xf);
			}
		}

		public void GetTransform(out Transform transform)
		{
			transform = this._xf;
		}

		public void ApplyForce(TSVector2 force, TSVector2 point)
		{
			this.ApplyForce(ref force, ref point);
		}

		public void ApplyForce(ref TSVector2 force)
		{
			this.ApplyForce(ref force, ref this._xf.p);
		}

		public void ApplyForce(TSVector2 force)
		{
			this.ApplyForce(ref force, ref this._xf.p);
		}

		public void ApplyForce(ref TSVector2 force, ref TSVector2 point)
		{
			Debug.Assert(!FP.IsNaN(force.x));
			Debug.Assert(!FP.IsNaN(force.y));
			Debug.Assert(!FP.IsNaN(point.x));
			Debug.Assert(!FP.IsNaN(point.y));
			bool flag = this._bodyType == BodyType.Dynamic;
			if (flag)
			{
				bool flag2 = !this.Awake;
				if (flag2)
				{
					this.Awake = true;
				}
				this._force += force;
				this._torque += (point.x - this._sweep.C.x) * force.y - (point.y - this._sweep.C.y) * force.x;
			}
		}

		public void ApplyTorque(FP torque)
		{
			Debug.Assert(!FP.IsNaN(torque));
			bool flag = this._bodyType == BodyType.Dynamic;
			if (flag)
			{
				bool flag2 = !this.Awake;
				if (flag2)
				{
					this.Awake = true;
				}
				this._torque += torque;
			}
		}

		public void ApplyLinearImpulse(TSVector2 impulse)
		{
			this.ApplyLinearImpulse(ref impulse);
		}

		public void ApplyLinearImpulse(TSVector2 impulse, TSVector2 point)
		{
			this.ApplyLinearImpulse(ref impulse, ref point);
		}

		public void ApplyLinearImpulse(ref TSVector2 impulse)
		{
			bool flag = this._bodyType != BodyType.Dynamic;
			if (!flag)
			{
				bool flag2 = !this.Awake;
				if (flag2)
				{
					this.Awake = true;
				}
				this._linearVelocity += this._invMass * impulse;
			}
		}

		public void ApplyLinearImpulse(ref TSVector2 impulse, ref TSVector2 point)
		{
			bool flag = this._bodyType != BodyType.Dynamic;
			if (!flag)
			{
				bool flag2 = !this.Awake;
				if (flag2)
				{
					this.Awake = true;
				}
				this._linearVelocity += this._invMass * impulse;
				this._angularVelocity += this._invI * ((point.x - this._sweep.C.x) * impulse.y - (point.y - this._sweep.C.y) * impulse.x);
			}
		}

		public void ApplyAngularImpulse(FP impulse)
		{
			bool flag = this._bodyType != BodyType.Dynamic;
			if (!flag)
			{
				bool flag2 = !this.Awake;
				if (flag2)
				{
					this.Awake = true;
				}
				this._angularVelocity += this._invI * impulse;
			}
		}

		public void ResetMassData()
		{
			this._mass = 0f;
			this._invMass = 0f;
			this._inertia = 0f;
			this._invI = 0f;
			this._sweep.LocalCenter = TSVector2.zero;
			bool flag = this.BodyType == BodyType.Kinematic;
			if (flag)
			{
				this._sweep.C0 = this._xf.p;
				this._sweep.C = this._xf.p;
				this._sweep.A0 = this._sweep.A;
			}
			else
			{
				Debug.Assert(this.BodyType == BodyType.Dynamic || this.BodyType == BodyType.Static);
				TSVector2 tSVector = TSVector2.zero;
				foreach (Fixture current in this.FixtureList)
				{
					bool flag2 = current.Shape._density == 0;
					if (!flag2)
					{
						MassData massData = current.Shape.MassData;
						this._mass += massData.Mass;
						tSVector += massData.Mass * massData.Centroid;
						this._inertia += massData.Inertia;
					}
				}
				bool flag3 = this.BodyType == BodyType.Static;
				if (flag3)
				{
					this._sweep.C0 = (this._sweep.C = this._xf.p);
				}
				else
				{
					bool flag4 = this._mass > 0f;
					if (flag4)
					{
						this._invMass = 1f / this._mass;
						tSVector *= this._invMass;
					}
					else
					{
						this._mass = 1f;
						this._invMass = 1f;
					}
					bool flag5 = this._inertia > 0f && !this._fixedRotation;
					if (flag5)
					{
						this._inertia -= this._mass * TSVector2.Dot(tSVector, tSVector);
						Debug.Assert(this._inertia > 0f);
						this._invI = 1f / this._inertia;
					}
					else
					{
						this._inertia = 0f;
						this._invI = 0f;
					}
					TSVector2 c = this._sweep.C;
					this._sweep.LocalCenter = tSVector;
					this._sweep.C0 = (this._sweep.C = MathUtils.Mul(ref this._xf, ref this._sweep.LocalCenter));
					TSVector2 tSVector2 = this._sweep.C - c;
					this._linearVelocity += new TSVector2(-this._angularVelocity * tSVector2.y, this._angularVelocity * tSVector2.x);
				}
			}
		}

		public TSVector2 GetWorldPoint(ref TSVector2 localPoint)
		{
			return MathUtils.Mul(ref this._xf, ref localPoint);
		}

		public TSVector2 GetWorldPoint(TSVector2 localPoint)
		{
			return this.GetWorldPoint(ref localPoint);
		}

		public TSVector2 GetWorldVector(ref TSVector2 localVector)
		{
			return MathUtils.Mul(this._xf.q, localVector);
		}

		public TSVector2 GetWorldVector(TSVector2 localVector)
		{
			return this.GetWorldVector(ref localVector);
		}

		public TSVector2 GetLocalPoint(ref TSVector2 worldPoint)
		{
			return MathUtils.MulT(ref this._xf, worldPoint);
		}

		public TSVector2 GetLocalPoint(TSVector2 worldPoint)
		{
			return this.GetLocalPoint(ref worldPoint);
		}

		public TSVector2 GetLocalVector(ref TSVector2 worldVector)
		{
			return MathUtils.MulT(this._xf.q, worldVector);
		}

		public TSVector2 GetLocalVector(TSVector2 worldVector)
		{
			return this.GetLocalVector(ref worldVector);
		}

		public TSVector2 GetLinearVelocityFromWorldPoint(TSVector2 worldPoint)
		{
			return this.GetLinearVelocityFromWorldPoint(ref worldPoint);
		}

		public TSVector2 GetLinearVelocityFromWorldPoint(ref TSVector2 worldPoint)
		{
			return this._linearVelocity + new TSVector2(-this._angularVelocity * (worldPoint.y - this._sweep.C.y), this._angularVelocity * (worldPoint.x - this._sweep.C.x));
		}

		public TSVector2 GetLinearVelocityFromLocalPoint(TSVector2 localPoint)
		{
			return this.GetLinearVelocityFromLocalPoint(ref localPoint);
		}

		public TSVector2 GetLinearVelocityFromLocalPoint(ref TSVector2 localPoint)
		{
			return this.GetLinearVelocityFromWorldPoint(this.GetWorldPoint(ref localPoint));
		}

		internal void SynchronizeFixtures()
		{
			Transform transform = default(Transform);
			transform.q.Set(this._sweep.A0);
			transform.p = this._sweep.C0 - MathUtils.Mul(transform.q, this._sweep.LocalCenter);
			IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
			for (int i = 0; i < this.FixtureList.Count; i++)
			{
				this.FixtureList[i].Synchronize(broadPhase, ref transform, ref this._xf);
			}
		}

		internal void SynchronizeTransform()
		{
			this._xf.q.Set(this._sweep.A);
			this._xf.p = this._sweep.C - MathUtils.Mul(this._xf.q, this._sweep.LocalCenter);
		}

		internal bool ShouldCollide(Body other)
		{
			bool flag = this._specialSensor != BodySpecialSensor.None || other._specialSensor > BodySpecialSensor.None;
			bool result;
			if (flag)
			{
				result = true;
			}
			else
			{
				bool flag2 = this._bodyType == BodyType.Static && other._bodyType == BodyType.Static;
				if (flag2)
				{
					result = false;
				}
				else
				{
					for (JointEdge jointEdge = this.JointList; jointEdge != null; jointEdge = jointEdge.Next)
					{
						bool flag3 = jointEdge.Other == other;
						if (flag3)
						{
							bool flag4 = !jointEdge.Joint.CollideConnected;
							if (flag4)
							{
								result = false;
								return result;
							}
						}
					}
					result = true;
				}
			}
			return result;
		}

		internal void Advance(FP alpha)
		{
			this._sweep.Advance(alpha);
			this._sweep.C = this._sweep.C0;
			this._sweep.A = this._sweep.A0;
			this._xf.q.Set(this._sweep.A);
			this._xf.p = this._sweep.C - MathUtils.Mul(this._xf.q, this._sweep.LocalCenter);
		}

		public void IgnoreCollisionWith(Body other)
		{
			for (int i = 0; i < this.FixtureList.Count; i++)
			{
				for (int j = 0; j < other.FixtureList.Count; j++)
				{
					this.FixtureList[i].IgnoreCollisionWith(other.FixtureList[j]);
				}
			}
		}

		public void RestoreCollisionWith(Body other)
		{
			for (int i = 0; i < this.FixtureList.Count; i++)
			{
				for (int j = 0; j < other.FixtureList.Count; j++)
				{
					this.FixtureList[i].RestoreCollisionWith(other.FixtureList[j]);
				}
			}
		}

		public void Dispose()
		{
			bool flag = !this.IsDisposed;
			if (flag)
			{
				this._world.RemoveBody(this);
				this.IsDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		public Body Clone(World world = null)
		{
			return new Body(world ?? this._world, new TSVector2?(this.Position), this.Rotation, this.UserData)
			{
				_bodyType = this._bodyType,
				_linearVelocity = this._linearVelocity,
				_angularVelocity = this._angularVelocity,
				GravityScale = this.GravityScale,
				UserData = this.UserData,
				_enabled = this._enabled,
				_fixedRotation = this._fixedRotation,
				_sleepingAllowed = this._sleepingAllowed,
				_linearDamping = this._linearDamping,
				_angularDamping = this._angularDamping,
				_awake = this._awake,
				IsBullet = this.IsBullet,
				IgnoreCCD = this.IgnoreCCD,
				IgnoreGravity = this.IgnoreGravity,
				_torque = this._torque
			};
		}

		public Body DeepClone(World world = null)
		{
			Body body = this.Clone(world ?? this._world);
			int count = this.FixtureList.Count;
			for (int i = 0; i < count; i++)
			{
				this.FixtureList[i].CloneOnto(body);
			}
			return body;
		}

		public string Checkum()
		{
			return string.Format("{0}|{1}", this.Position, this.Rotation);
		}

		public void TSApplyForce(TSVector2 force)
		{
			this.ApplyForce(force);
		}

		public void TSApplyForce(TSVector2 force, TSVector2 position)
		{
			this.ApplyForce(force, position);
		}

		public void TSApplyImpulse(TSVector2 force)
		{
			this.ApplyLinearImpulse(force, force);
		}

		public void TSApplyImpulse(TSVector2 force, TSVector2 position)
		{
			this.ApplyLinearImpulse(force, position);
		}

		public void TSApplyTorque(TSVector2 force)
		{
			throw new NotImplementedException();
		}

		public void TSUpdate()
		{
		}
	}
}
