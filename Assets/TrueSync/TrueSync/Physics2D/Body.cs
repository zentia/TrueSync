// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.Body
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
    public class Body : IDisposable, IBody2D, IBody
    {
        internal BodySpecialSensor _specialSensor = BodySpecialSensor.None;
        internal bool _sleepingAllowed = true;
        internal bool _awake = true;
        internal bool _enabled = true;
        [ThreadStatic]
        internal static int _bodyIdCounter;
        public List<Body> _specialSensorResults;
        internal FP _angularDamping;
        internal BodyType _bodyType;
        internal FP _inertia;
        internal FP _linearDamping;
        internal FP _mass;
        internal bool _fixedRotation;
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

        public BodySpecialSensor SpecialSensor
        {
            get
            {
                return this._specialSensor;
            }
            set
            {
                this._specialSensor = value;
                if ((uint)value <= 0U)
                    return;
                this._specialSensorResults = new List<Body>();
            }
        }

        public Body(World world, TSVector2? position, FP rotation, object userdata = null)
        {
            this.FixtureList = new List<Fixture>();
            this.bodyConstraints = new List<IBodyConstraint>();
            this.BodyId = Body._bodyIdCounter++;
            this._world = world;
            this.UserData = userdata;
            this.GravityScale = (FP)1f;
            this.BodyType = BodyType.Static;
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

        public int BodyId { get; private set; }

        public int IslandIndex { get; set; }

        public FP GravityScale { get; set; }

        public object UserData { get; set; }

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
                if (this._bodyType == value)
                    return;
                this._bodyType = value;
                this.ResetMassData();
                if (this._bodyType == BodyType.Static)
                {
                    this._linearVelocity = TSVector2.zero;
                    this._angularVelocity = (FP)0.0f;
                    this._sweep.A0 = this._sweep.A;
                    this._sweep.C0 = this._sweep.C;
                    this.SynchronizeFixtures();
                }
                this.Awake = true;
                this._force = TSVector2.zero;
                this._torque = (FP)0.0f;
                ContactEdge contactEdge1 = this.ContactList;
                while (contactEdge1 != null)
                {
                    ContactEdge contactEdge2 = contactEdge1;
                    contactEdge1 = contactEdge1.Next;
                    this._world.ContactManager.Destroy(contactEdge2.Contact);
                }
                this.ContactList = (ContactEdge)null;
                IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
                foreach (Fixture fixture in this.FixtureList)
                {
                    int proxyCount = fixture.ProxyCount;
                    for (int index = 0; index < proxyCount; ++index)
                        broadPhase.TouchProxy(fixture.Proxies[index].ProxyId);
                }
            }
        }

        public TSVector2 LinearVelocity
        {
            set
            {
                Debug.Assert(!FP.IsNaN(value.x) && !FP.IsNaN(value.y));
                if (this._bodyType == BodyType.Static)
                    return;
                if (TSVector2.Dot(value, value) > (FP)0.0f)
                    this.Awake = true;
                this._linearVelocity = value;
            }
            get
            {
                return this._linearVelocity;
            }
        }

        public FP AngularVelocity
        {
            set
            {
                Debug.Assert(!FP.IsNaN(value));
                if (this._bodyType == BodyType.Static)
                    return;
                if (value * value > (FP)0.0f)
                    this.Awake = true;
                this._angularVelocity = value;
            }
            get
            {
                return this._angularVelocity;
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

        public bool IsBullet { get; set; }

        public bool SleepingAllowed
        {
            set
            {
                if (!value)
                    this.Awake = true;
                this._sleepingAllowed = value;
            }
            get
            {
                return this._sleepingAllowed;
            }
        }

        public bool Awake
        {
            set
            {
                if (value)
                {
                    if (!this._awake)
                    {
                        this._sleepTime = (FP)0.0f;
                        this._world.ContactManager.UpdateContacts(this.ContactList, true);
                    }
                }
                else
                {
                    this.ResetDynamics();
                    this._sleepTime = (FP)0.0f;
                    this._world.ContactManager.UpdateContacts(this.ContactList, false);
                }
                this._awake = value;
            }
            get
            {
                return this._awake;
            }
        }

        public bool Enabled
        {
            set
            {
                if (value == this._enabled)
                    return;
                if (value)
                {
                    IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
                    for (int index = 0; index < this.FixtureList.Count; ++index)
                        this.FixtureList[index].CreateProxies(broadPhase, ref this._xf);
                }
                else
                {
                    IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
                    for (int index = 0; index < this.FixtureList.Count; ++index)
                        this.FixtureList[index].DestroyProxies(broadPhase);
                    ContactEdge contactEdge1 = this.ContactList;
                    while (contactEdge1 != null)
                    {
                        ContactEdge contactEdge2 = contactEdge1;
                        contactEdge1 = contactEdge1.Next;
                        this._world.ContactManager.Destroy(contactEdge2.Contact);
                    }
                    this.ContactList = (ContactEdge)null;
                }
                this._enabled = value;
            }
            get
            {
                return this._enabled;
            }
        }

        public bool FixedRotation
        {
            set
            {
                if (this._fixedRotation == value)
                    return;
                this._fixedRotation = value;
                this._angularVelocity = (FP)0.0f;
                this.ResetMassData();
            }
            get
            {
                return this._fixedRotation;
            }
        }

        public List<Fixture> FixtureList { get; internal set; }

        public JointEdge JointList { get; internal set; }

        public ContactEdge ContactList { get; internal set; }

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
                this.BodyType = value ? BodyType.Static : BodyType.Dynamic;
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
                this.BodyType = value ? BodyType.Kinematic : BodyType.Dynamic;
            }
        }

        public bool IgnoreGravity { get; set; }

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
                if (this._bodyType != BodyType.Dynamic)
                    return;
                TSVector2 c = this._sweep.C;
                this._sweep.LocalCenter = value;
                this._sweep.C0 = this._sweep.C = MathUtils.Mul(ref this._xf, ref this._sweep.LocalCenter);
                TSVector2 tsVector2 = this._sweep.C - c;
                this._linearVelocity = this._linearVelocity + new TSVector2(-this._angularVelocity * tsVector2.y, this._angularVelocity * tsVector2.x);
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
                if (this._bodyType != BodyType.Dynamic)
                    return;
                this._mass = value;
                if (this._mass <= (FP)0.0f)
                    this._mass = (FP)1f;
                this._invMass = (FP)1f / this._mass;
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
                if (this._bodyType != BodyType.Dynamic || (!(value > (FP)0.0f) || this._fixedRotation))
                    return;
                this._inertia = value - this.Mass * TSVector2.Dot(this.LocalCenter, this.LocalCenter);
                Debug.Assert(this._inertia > (FP)0.0f);
                this._invI = (FP)1f / this._inertia;
            }
        }

        public FP Restitution
        {
            get
            {
                FP fp = (FP)0;
                for (int index = 0; index < this.FixtureList.Count; ++index)
                {
                    Fixture fixture = this.FixtureList[index];
                    fp += fixture.Restitution;
                }
                return this.FixtureList.Count > 0 ? fp / (FP)this.FixtureList.Count : (FP)0;
            }
            set
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].Restitution = value;
            }
        }

        public FP Friction
        {
            get
            {
                FP fp = (FP)0;
                for (int index = 0; index < this.FixtureList.Count; ++index)
                {
                    Fixture fixture = this.FixtureList[index];
                    fp += fixture.Friction;
                }
                return this.FixtureList.Count > 0 ? fp / (FP)this.FixtureList.Count : (FP)0;
            }
            set
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].Friction = value;
            }
        }

        public Category CollisionCategories
        {
            set
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].CollisionCategories = value;
            }
        }

        public Category CollidesWith
        {
            set
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].CollidesWith = value;
            }
        }

        public Category IgnoreCCDWith
        {
            set
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].IgnoreCCDWith = value;
            }
        }

        public short CollisionGroup
        {
            set
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].CollisionGroup = value;
            }
        }

        public bool IsSensor
        {
            set
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].IsSensor = value;
            }
        }

        public bool IgnoreCCD { get; set; }

        public void ResetDynamics()
        {
            this._torque = (FP)0;
            this._angularVelocity = (FP)0;
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
                if (fixture == fixtureA || fixture == fixtureB)
                    this._world.ContactManager.Destroy(contact);
            }
            if (this._enabled)
            {
                IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }
            this.FixtureList.Remove(fixture);
            fixture.Destroy();
            fixture.Body = (Body)null;
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
            for (int index = 0; index < this.FixtureList.Count; ++index)
                this.FixtureList[index].Synchronize(broadPhase, ref this._xf, ref this._xf);
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
            if (this._bodyType != BodyType.Dynamic)
                return;
            if (!this.Awake)
                this.Awake = true;
            this._force = this._force + force;
            this._torque = this._torque + ((point.x - this._sweep.C.x) * force.y - (point.y - this._sweep.C.y) * force.x);
        }

        public void ApplyTorque(FP torque)
        {
            Debug.Assert(!FP.IsNaN(torque));
            if (this._bodyType != BodyType.Dynamic)
                return;
            if (!this.Awake)
                this.Awake = true;
            this._torque = this._torque + torque;
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
            if (this._bodyType != BodyType.Dynamic)
                return;
            if (!this.Awake)
                this.Awake = true;
            this._linearVelocity = this._linearVelocity + this._invMass * impulse;
        }

        public void ApplyLinearImpulse(ref TSVector2 impulse, ref TSVector2 point)
        {
            if (this._bodyType != BodyType.Dynamic)
                return;
            if (!this.Awake)
                this.Awake = true;
            this._linearVelocity = this._linearVelocity + this._invMass * impulse;
            this._angularVelocity = this._angularVelocity + this._invI * ((point.x - this._sweep.C.x) * impulse.y - (point.y - this._sweep.C.y) * impulse.x);
        }

        public void ApplyAngularImpulse(FP impulse)
        {
            if (this._bodyType != BodyType.Dynamic)
                return;
            if (!this.Awake)
                this.Awake = true;
            this._angularVelocity = this._angularVelocity + this._invI * impulse;
        }

        public void ResetMassData()
        {
            this._mass = (FP)0.0f;
            this._invMass = (FP)0.0f;
            this._inertia = (FP)0.0f;
            this._invI = (FP)0.0f;
            this._sweep.LocalCenter = TSVector2.zero;
            if (this.BodyType == BodyType.Kinematic)
            {
                this._sweep.C0 = this._xf.p;
                this._sweep.C = this._xf.p;
                this._sweep.A0 = this._sweep.A;
            }
            else
            {
                Debug.Assert(this.BodyType == BodyType.Dynamic || this.BodyType == BodyType.Static);
                TSVector2 zero = TSVector2.zero;
                foreach (Fixture fixture in this.FixtureList)
                {
                    if (!(fixture.Shape._density == (FP)0))
                    {
                        MassData massData = fixture.Shape.MassData;
                        this._mass = this._mass + massData.Mass;
                        zero += massData.Mass * massData.Centroid;
                        this._inertia = this._inertia + massData.Inertia;
                    }
                }
                if (this.BodyType == BodyType.Static)
                {
                    this._sweep.C0 = this._sweep.C = this._xf.p;
                }
                else
                {
                    if (this._mass > (FP)0.0f)
                    {
                        this._invMass = (FP)1f / this._mass;
                        zero *= this._invMass;
                    }
                    else
                    {
                        this._mass = (FP)1f;
                        this._invMass = (FP)1f;
                    }
                    if (this._inertia > (FP)0.0f && !this._fixedRotation)
                    {
                        this._inertia = this._inertia - this._mass * TSVector2.Dot(zero, zero);
                        Debug.Assert(this._inertia > (FP)0.0f);
                        this._invI = (FP)1f / this._inertia;
                    }
                    else
                    {
                        this._inertia = (FP)0.0f;
                        this._invI = (FP)0.0f;
                    }
                    TSVector2 c = this._sweep.C;
                    this._sweep.LocalCenter = zero;
                    this._sweep.C0 = this._sweep.C = MathUtils.Mul(ref this._xf, ref this._sweep.LocalCenter);
                    TSVector2 tsVector2 = this._sweep.C - c;
                    this._linearVelocity = this._linearVelocity + new TSVector2(-this._angularVelocity * tsVector2.y, this._angularVelocity * tsVector2.x);
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
            Transform transform1 = new Transform();
            transform1.q.Set(this._sweep.A0);
            transform1.p = this._sweep.C0 - MathUtils.Mul(transform1.q, this._sweep.LocalCenter);
            IBroadPhase broadPhase = this._world.ContactManager.BroadPhase;
            for (int index = 0; index < this.FixtureList.Count; ++index)
                this.FixtureList[index].Synchronize(broadPhase, ref transform1, ref this._xf);
        }

        internal void SynchronizeTransform()
        {
            this._xf.q.Set(this._sweep.A);
            this._xf.p = this._sweep.C - MathUtils.Mul(this._xf.q, this._sweep.LocalCenter);
        }

        internal bool ShouldCollide(Body other)
        {
            if (this._specialSensor != BodySpecialSensor.None || (uint)other._specialSensor > 0U)
                return true;
            if (this._bodyType == BodyType.Static && other._bodyType == BodyType.Static)
                return false;
            for (JointEdge jointEdge = this.JointList; jointEdge != null; jointEdge = jointEdge.Next)
            {
                if (jointEdge.Other == other && !jointEdge.Joint.CollideConnected)
                    return false;
            }
            return true;
        }

        internal void Advance(FP alpha)
        {
            this._sweep.Advance(alpha);
            this._sweep.C = this._sweep.C0;
            this._sweep.A = this._sweep.A0;
            this._xf.q.Set(this._sweep.A);
            this._xf.p = this._sweep.C - MathUtils.Mul(this._xf.q, this._sweep.LocalCenter);
        }

        public event OnCollisionEventHandler OnCollision
        {
            add
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].OnCollision += value;
            }
            remove
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].OnCollision -= value;
            }
        }

        public event OnSeparationEventHandler OnSeparation
        {
            add
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].OnSeparation += value;
            }
            remove
            {
                for (int index = 0; index < this.FixtureList.Count; ++index)
                    this.FixtureList[index].OnSeparation -= value;
            }
        }

        public void IgnoreCollisionWith(Body other)
        {
            for (int index1 = 0; index1 < this.FixtureList.Count; ++index1)
            {
                for (int index2 = 0; index2 < other.FixtureList.Count; ++index2)
                    this.FixtureList[index1].IgnoreCollisionWith(other.FixtureList[index2]);
            }
        }

        public void RestoreCollisionWith(Body other)
        {
            for (int index1 = 0; index1 < this.FixtureList.Count; ++index1)
            {
                for (int index2 = 0; index2 < other.FixtureList.Count; ++index2)
                    this.FixtureList[index1].RestoreCollisionWith(other.FixtureList[index2]);
            }
        }

        public bool IsDisposed { get; set; }

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

        public void Dispose()
        {
            if (this.IsDisposed)
                return;
            this._world.RemoveBody(this);
            this.IsDisposed = true;
            GC.SuppressFinalize((object)this);
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
            for (int index = 0; index < count; ++index)
                this.FixtureList[index].CloneOnto(body);
            return body;
        }

        public string Checkum()
        {
            return string.Format("{0}|{1}", (object)this.Position, (object)this.Rotation);
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
