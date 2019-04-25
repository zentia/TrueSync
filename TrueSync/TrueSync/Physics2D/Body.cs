namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public class Body : IDisposable, IBody2D, IBody
    {
        internal FP _angularDamping;
        internal FP _angularVelocity;
        internal bool _awake = true;
        [ThreadStatic]
        internal static int _bodyIdCounter;
        internal TrueSync.Physics2D.BodyType _bodyType;
        internal bool _enabled = true;
        internal bool _fixedRotation;
        internal TSVector2 _force;
        internal FP _inertia;
        internal FP _invI;
        internal FP _invMass;
        internal bool _island;
        internal FP _linearDamping;
        internal TSVector2 _linearVelocity;
        internal FP _mass;
        internal bool _sleepingAllowed = true;
        internal FP _sleepTime;
        internal BodySpecialSensor _specialSensor = BodySpecialSensor.None;
        public List<Body> _specialSensorResults;
        internal Sweep _sweep;
        internal FP _torque;
        internal TrueSync.Physics2D.World _world;
        internal Transform _xf;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <BodyId>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ContactEdge <ContactList>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Fixture> <FixtureList>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <GravityScale>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IgnoreCCD>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IgnoreGravity>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IsBullet>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IsDisposed>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <IslandIndex>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private JointEdge <JointList>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object <UserData>k__BackingField;
        public List<IBodyConstraint> bodyConstraints;
        public TrueSync.Physics2D.ControllerFilter ControllerFilter;
        internal bool disabled;
        public TrueSync.Physics2D.PhysicsLogicFilter PhysicsLogicFilter;

        public event OnCollisionEventHandler OnCollision
        {
            add
            {
                for (int i = 0; i < this.FixtureList.Count; i++)
                {
                    Fixture local1 = this.FixtureList[i];
                    local1.OnCollision = (OnCollisionEventHandler) Delegate.Combine(local1.OnCollision, value);
                }
            }
            remove
            {
                for (int i = 0; i < this.FixtureList.Count; i++)
                {
                    Fixture local1 = this.FixtureList[i];
                    local1.OnCollision = (OnCollisionEventHandler) Delegate.Remove(local1.OnCollision, value);
                }
            }
        }

        public event OnSeparationEventHandler OnSeparation
        {
            add
            {
                for (int i = 0; i < this.FixtureList.Count; i++)
                {
                    Fixture local1 = this.FixtureList[i];
                    local1.OnSeparation = (OnSeparationEventHandler) Delegate.Combine(local1.OnSeparation, value);
                }
            }
            remove
            {
                for (int i = 0; i < this.FixtureList.Count; i++)
                {
                    Fixture local1 = this.FixtureList[i];
                    local1.OnSeparation = (OnSeparationEventHandler) Delegate.Remove(local1.OnSeparation, value);
                }
            }
        }

        public Body(TrueSync.Physics2D.World world, TSVector2? position, FP rotation, object userdata = null)
        {
            this.FixtureList = new List<Fixture>();
            this.bodyConstraints = new List<IBodyConstraint>();
            this.BodyId = _bodyIdCounter++;
            this._world = world;
            this.UserData = userdata;
            this.GravityScale = 1f;
            this.BodyType = TrueSync.Physics2D.BodyType.Static;
            this.Enabled = true;
            this._xf.q.Set(rotation);
            if (position.HasValue)
            {
                this._xf.p = position.Value;
                this._sweep.C0 = this._xf.p;
                this._sweep.C = this._xf.p;
                this._sweep.A0 = rotation;
                this._sweep.A = rotation;
            }
            world.AddBody(this);
        }

        internal void Advance(FP alpha)
        {
            this._sweep.Advance(alpha);
            this._sweep.C = this._sweep.C0;
            this._sweep.A = this._sweep.A0;
            this._xf.q.Set(this._sweep.A);
            this._xf.p = this._sweep.C - MathUtils.Mul(this._xf.q, this._sweep.LocalCenter);
        }

        public void ApplyAngularImpulse(FP impulse)
        {
            if (this._bodyType == TrueSync.Physics2D.BodyType.Dynamic)
            {
                if (!this.Awake)
                {
                    this.Awake = true;
                }
                this._angularVelocity += this._invI * impulse;
            }
        }

        public void ApplyForce(ref TSVector2 force)
        {
            this.ApplyForce(ref force, ref this._xf.p);
        }

        public void ApplyForce(TSVector2 force)
        {
            this.ApplyForce(ref force, ref this._xf.p);
        }

        public void ApplyForce(TSVector2 force, TSVector2 point)
        {
            this.ApplyForce(ref force, ref point);
        }

        public void ApplyForce(ref TSVector2 force, ref TSVector2 point)
        {
            Debug.Assert(!FP.IsNaN(force.x));
            Debug.Assert(!FP.IsNaN(force.y));
            Debug.Assert(!FP.IsNaN(point.x));
            Debug.Assert(!FP.IsNaN(point.y));
            if (this._bodyType == TrueSync.Physics2D.BodyType.Dynamic)
            {
                if (!this.Awake)
                {
                    this.Awake = true;
                }
                this._force += force;
                this._torque += ((point.x - this._sweep.C.x) * force.y) - ((point.y - this._sweep.C.y) * force.x);
            }
        }

        public void ApplyLinearImpulse(TSVector2 impulse)
        {
            this.ApplyLinearImpulse(ref impulse);
        }

        public void ApplyLinearImpulse(ref TSVector2 impulse)
        {
            if (this._bodyType == TrueSync.Physics2D.BodyType.Dynamic)
            {
                if (!this.Awake)
                {
                    this.Awake = true;
                }
                this._linearVelocity += this._invMass * impulse;
            }
        }

        public void ApplyLinearImpulse(TSVector2 impulse, TSVector2 point)
        {
            this.ApplyLinearImpulse(ref impulse, ref point);
        }

        public void ApplyLinearImpulse(ref TSVector2 impulse, ref TSVector2 point)
        {
            if (this._bodyType == TrueSync.Physics2D.BodyType.Dynamic)
            {
                if (!this.Awake)
                {
                    this.Awake = true;
                }
                this._linearVelocity += this._invMass * impulse;
                this._angularVelocity += this._invI * (((point.x - this._sweep.C.x) * impulse.y) - ((point.y - this._sweep.C.y) * impulse.x));
            }
        }

        public void ApplyTorque(FP torque)
        {
            Debug.Assert(!FP.IsNaN(torque));
            if (this._bodyType == TrueSync.Physics2D.BodyType.Dynamic)
            {
                if (!this.Awake)
                {
                    this.Awake = true;
                }
                this._torque += torque;
            }
        }

        public string Checkum()
        {
            return string.Format("{0}|{1}", this.Position, this.Rotation);
        }

        public Body Clone(TrueSync.Physics2D.World world = null)
        {
            return new Body(world ?? this._world, new TSVector2?(this.Position), this.Rotation, this.UserData) { _bodyType = this._bodyType, _linearVelocity = this._linearVelocity, _angularVelocity = this._angularVelocity, GravityScale = this.GravityScale, UserData = this.UserData, _enabled = this._enabled, _fixedRotation = this._fixedRotation, _sleepingAllowed = this._sleepingAllowed, _linearDamping = this._linearDamping, _angularDamping = this._angularDamping, _awake = this._awake, IsBullet = this.IsBullet, IgnoreCCD = this.IgnoreCCD, IgnoreGravity = this.IgnoreGravity, _torque = this._torque };
        }

        public Fixture CreateFixture(TrueSync.Physics2D.Shape shape, object userData = null)
        {
            return new Fixture(this, shape, userData);
        }

        public Body DeepClone(TrueSync.Physics2D.World world = null)
        {
            Body body = this.Clone(world ?? this._world);
            int count = this.FixtureList.Count;
            for (int i = 0; i < count; i++)
            {
                this.FixtureList[i].CloneOnto(body);
            }
            return body;
        }

        public void DestroyFixture(Fixture fixture)
        {
            Debug.Assert(fixture.Body == this);
            Debug.Assert(this.FixtureList.Count > 0);
            Debug.Assert(this.FixtureList.Contains(fixture));
            ContactEdge contactList = this.ContactList;
            while (contactList > null)
            {
                TrueSync.Physics2D.Contact contact = contactList.Contact;
                contactList = contactList.Next;
                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                if ((fixture == fixtureA) || (fixture == fixtureB))
                {
                    this._world.ContactManager.Destroy(contact);
                }
            }
            if (this._enabled)
            {
                IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }
            this.FixtureList.Remove(fixture);
            fixture.Destroy();
            fixture.Body = null;
            this.ResetMassData();
        }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this._world.RemoveBody(this);
                this.IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public TSVector2 GetLinearVelocityFromLocalPoint(TSVector2 localPoint)
        {
            return this.GetLinearVelocityFromLocalPoint(ref localPoint);
        }

        public TSVector2 GetLinearVelocityFromLocalPoint(ref TSVector2 localPoint)
        {
            return this.GetLinearVelocityFromWorldPoint(this.GetWorldPoint(ref localPoint));
        }

        public TSVector2 GetLinearVelocityFromWorldPoint(TSVector2 worldPoint)
        {
            return this.GetLinearVelocityFromWorldPoint(ref worldPoint);
        }

        public TSVector2 GetLinearVelocityFromWorldPoint(ref TSVector2 worldPoint)
        {
            return (this._linearVelocity + new TSVector2(-this._angularVelocity * (worldPoint.y - this._sweep.C.y), this._angularVelocity * (worldPoint.x - this._sweep.C.x)));
        }

        public TSVector2 GetLocalPoint(ref TSVector2 worldPoint)
        {
            return MathUtils.MulT(ref this._xf, (TSVector2) worldPoint);
        }

        public TSVector2 GetLocalPoint(TSVector2 worldPoint)
        {
            return this.GetLocalPoint(ref worldPoint);
        }

        public TSVector2 GetLocalVector(ref TSVector2 worldVector)
        {
            return MathUtils.MulT(this._xf.q, (TSVector2) worldVector);
        }

        public TSVector2 GetLocalVector(TSVector2 worldVector)
        {
            return this.GetLocalVector(ref worldVector);
        }

        public void GetTransform(out Transform transform)
        {
            transform = this._xf;
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
            return MathUtils.Mul(this._xf.q, (TSVector2) localVector);
        }

        public TSVector2 GetWorldVector(TSVector2 localVector)
        {
            return this.GetWorldVector(ref localVector);
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

        public void ResetDynamics()
        {
            this._torque = 0;
            this._angularVelocity = 0;
            this._force = TSVector2.zero;
            this._linearVelocity = TSVector2.zero;
        }

        public void ResetMassData()
        {
            this._mass = 0f;
            this._invMass = 0f;
            this._inertia = 0f;
            this._invI = 0f;
            this._sweep.LocalCenter = TSVector2.zero;
            if (this.BodyType == TrueSync.Physics2D.BodyType.Kinematic)
            {
                this._sweep.C0 = this._xf.p;
                this._sweep.C = this._xf.p;
                this._sweep.A0 = this._sweep.A;
            }
            else
            {
                Debug.Assert((this.BodyType == TrueSync.Physics2D.BodyType.Dynamic) || (this.BodyType == TrueSync.Physics2D.BodyType.Static));
                TSVector2 zero = TSVector2.zero;
                foreach (Fixture fixture in this.FixtureList)
                {
                    if (fixture.Shape._density != 0)
                    {
                        MassData massData = fixture.Shape.MassData;
                        this._mass += massData.Mass;
                        zero += massData.Mass * massData.Centroid;
                        this._inertia += massData.Inertia;
                    }
                }
                if (this.BodyType == TrueSync.Physics2D.BodyType.Static)
                {
                    this._sweep.C0 = this._sweep.C = this._xf.p;
                }
                else
                {
                    if (this._mass > 0f)
                    {
                        this._invMass = 1f / this._mass;
                        zero *= this._invMass;
                    }
                    else
                    {
                        this._mass = 1f;
                        this._invMass = 1f;
                    }
                    if ((this._inertia > 0f) && !this._fixedRotation)
                    {
                        this._inertia -= this._mass * TSVector2.Dot(zero, zero);
                        Debug.Assert(this._inertia > 0f);
                        this._invI = 1f / this._inertia;
                    }
                    else
                    {
                        this._inertia = 0f;
                        this._invI = 0f;
                    }
                    TSVector2 c = this._sweep.C;
                    this._sweep.LocalCenter = zero;
                    this._sweep.C0 = this._sweep.C = MathUtils.Mul(ref this._xf, ref this._sweep.LocalCenter);
                    TSVector2 vector3 = this._sweep.C - c;
                    this._linearVelocity += new TSVector2(-this._angularVelocity * vector3.y, this._angularVelocity * vector3.x);
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

        internal bool ShouldCollide(Body other)
        {
            if ((this._specialSensor == BodySpecialSensor.None) && (other._specialSensor <= BodySpecialSensor.None))
            {
                if ((this._bodyType == TrueSync.Physics2D.BodyType.Static) && (other._bodyType == TrueSync.Physics2D.BodyType.Static))
                {
                    return false;
                }
                for (JointEdge edge = this.JointList; edge > null; edge = edge.Next)
                {
                    if ((edge.Other == other) && !edge.Joint.CollideConnected)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal void SynchronizeFixtures()
        {
            Transform transform = new Transform();
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

        public FP AngularVelocity
        {
            get
            {
                return this._angularVelocity;
            }
            set
            {
                Debug.Assert(!FP.IsNaN(value));
                if (this._bodyType != TrueSync.Physics2D.BodyType.Static)
                {
                    if ((value * value) > 0f)
                    {
                        this.Awake = true;
                    }
                    this._angularVelocity = value;
                }
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
                    if (!this._awake)
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

        public int BodyId { get; private set; }

        public TrueSync.Physics2D.BodyType BodyType
        {
            get
            {
                return this._bodyType;
            }
            set
            {
                if (this._bodyType != value)
                {
                    this._bodyType = value;
                    this.ResetMassData();
                    if (this._bodyType == TrueSync.Physics2D.BodyType.Static)
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
                    ContactEdge contactList = this.ContactList;
                    while (contactList > null)
                    {
                        ContactEdge edge2 = contactList;
                        contactList = contactList.Next;
                        this._world.ContactManager.Destroy(edge2.Contact);
                    }
                    this.ContactList = null;
                    IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
                    foreach (Fixture fixture in this.FixtureList)
                    {
                        int proxyCount = fixture.ProxyCount;
                        for (int i = 0; i < proxyCount; i++)
                        {
                            broadPhase.TouchProxy(fixture.Proxies[i].ProxyId);
                        }
                    }
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

        public ContactEdge ContactList { get; internal set; }

        public bool Enabled
        {
            get
            {
                return this._enabled;
            }
            set
            {
                if (value != this._enabled)
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
                        IBroadPhase phase2 = this._world.ContactManager.BroadPhase;
                        for (int j = 0; j < this.FixtureList.Count; j++)
                        {
                            this.FixtureList[j].DestroyProxies(phase2);
                        }
                        ContactEdge contactList = this.ContactList;
                        while (contactList > null)
                        {
                            ContactEdge edge2 = contactList;
                            contactList = contactList.Next;
                            this._world.ContactManager.Destroy(edge2.Contact);
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
                if (this._fixedRotation != value)
                {
                    this._fixedRotation = value;
                    this._angularVelocity = 0f;
                    this.ResetMassData();
                }
            }
        }

        public List<Fixture> FixtureList { get; internal set; }

        public FP Friction
        {
            get
            {
                FP fp = 0;
                for (int i = 0; i < this.FixtureList.Count; i++)
                {
                    Fixture fixture = this.FixtureList[i];
                    fp += fixture.Friction;
                }
                return ((this.FixtureList.Count > 0) ? (fp / this.FixtureList.Count) : 0);
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

        public FP GravityScale { get; set; }

        public bool IgnoreCCD { get; set; }

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

        public bool IgnoreGravity { get; set; }

        public FP Inertia
        {
            get
            {
                return (this._inertia + (this.Mass * TSVector2.Dot(this._sweep.LocalCenter, this._sweep.LocalCenter)));
            }
            set
            {
                Debug.Assert(!FP.IsNaN(value));
                if ((this._bodyType == TrueSync.Physics2D.BodyType.Dynamic) && ((value > 0f) && !this._fixedRotation))
                {
                    this._inertia = value - (this.Mass * TSVector2.Dot(this.LocalCenter, this.LocalCenter));
                    Debug.Assert(this._inertia > 0f);
                    this._invI = 1f / this._inertia;
                }
            }
        }

        public bool IsBullet { get; set; }

        public bool IsDisposed { get; set; }

        public bool IsKinematic
        {
            get
            {
                return (this._bodyType == TrueSync.Physics2D.BodyType.Kinematic);
            }
            set
            {
                this.BodyType = value ? TrueSync.Physics2D.BodyType.Kinematic : TrueSync.Physics2D.BodyType.Dynamic;
            }
        }

        public int IslandIndex { get; set; }

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

        public bool IsStatic
        {
            get
            {
                return (this._bodyType == TrueSync.Physics2D.BodyType.Static);
            }
            set
            {
                this.BodyType = value ? TrueSync.Physics2D.BodyType.Static : TrueSync.Physics2D.BodyType.Dynamic;
            }
        }

        public JointEdge JointList { get; internal set; }

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

        public TSVector2 LinearVelocity
        {
            get
            {
                return this._linearVelocity;
            }
            set
            {
                Debug.Assert(!FP.IsNaN(value.x) && !FP.IsNaN(value.y));
                if (this._bodyType != TrueSync.Physics2D.BodyType.Static)
                {
                    if (TSVector2.Dot(value, value) > 0f)
                    {
                        this.Awake = true;
                    }
                    this._linearVelocity = value;
                }
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
                if (this._bodyType == TrueSync.Physics2D.BodyType.Dynamic)
                {
                    TSVector2 c = this._sweep.C;
                    this._sweep.LocalCenter = value;
                    this._sweep.C0 = this._sweep.C = MathUtils.Mul(ref this._xf, ref this._sweep.LocalCenter);
                    TSVector2 vector2 = this._sweep.C - c;
                    this._linearVelocity += new TSVector2(-this._angularVelocity * vector2.y, this._angularVelocity * vector2.x);
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
                if (this._bodyType == TrueSync.Physics2D.BodyType.Dynamic)
                {
                    this._mass = value;
                    if (this._mass <= 0f)
                    {
                        this._mass = 1f;
                    }
                    this._invMass = 1f / this._mass;
                }
            }
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

        public FP Restitution
        {
            get
            {
                FP fp = 0;
                for (int i = 0; i < this.FixtureList.Count; i++)
                {
                    Fixture fixture = this.FixtureList[i];
                    fp += fixture.Restitution;
                }
                return ((this.FixtureList.Count > 0) ? (fp / this.FixtureList.Count) : 0);
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

        public FP Revolutions
        {
            get
            {
                return (this.Rotation / FP.Pi);
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

        public bool SleepingAllowed
        {
            get
            {
                return this._sleepingAllowed;
            }
            set
            {
                if (!value)
                {
                    this.Awake = true;
                }
                this._sleepingAllowed = value;
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
                if (value > BodySpecialSensor.None)
                {
                    this._specialSensorResults = new List<Body>();
                }
            }
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

        public object UserData { get; set; }

        public TSVector2 WorldCenter
        {
            get
            {
                return this._sweep.C;
            }
        }
    }
}

