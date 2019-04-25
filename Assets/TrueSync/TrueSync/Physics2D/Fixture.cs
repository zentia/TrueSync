// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.Fixture
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
    public class Fixture : IDisposable
    {
        [ThreadStatic]
        internal static int _fixtureIdCounter;
        private bool _isSensor;
        private FP _friction;
        private FP _restitution;
        internal Category _collidesWith;
        internal Category _collisionCategories;
        internal short _collisionGroup;
        internal HashSet<int> _collisionIgnores;
        public FixtureProxy[] Proxies;
        public int ProxyCount;
        public Category IgnoreCCDWith;
        public AfterCollisionEventHandler AfterCollision;
        public BeforeCollisionEventHandler BeforeCollision;
        public OnCollisionEventHandler OnCollision;
        public OnSeparationEventHandler OnSeparation;

        internal Fixture()
        {
            this.FixtureId = Fixture._fixtureIdCounter++;
            this._collisionCategories = Settings.DefaultFixtureCollisionCategories;
            this._collidesWith = Settings.DefaultFixtureCollidesWith;
            this._collisionGroup = (short)0;
            this._collisionIgnores = new HashSet<int>();
            this.IgnoreCCDWith = Settings.DefaultFixtureIgnoreCCDWith;
            this.Friction = (FP)0.2f;
            this.Restitution = (FP)0;
        }

        internal Fixture(Body body, Shape shape, object userData = null)
          : this()
        {
            if (shape.ShapeType == ShapeType.Polygon)
                ((PolygonShape)shape).Vertices.AttachedToBody = true;
            this.Body = body;
            this.UserData = userData;
            this.Shape = shape.Clone();
            this.RegisterFixture();
        }

        public short CollisionGroup
        {
            set
            {
                if ((int)this._collisionGroup == (int)value)
                    return;
                this._collisionGroup = value;
                this.Refilter();
            }
            get
            {
                return this._collisionGroup;
            }
        }

        public Category CollidesWith
        {
            get
            {
                return this._collidesWith;
            }
            set
            {
                if (this._collidesWith == value)
                    return;
                this._collidesWith = value;
                this.Refilter();
            }
        }

        public Category CollisionCategories
        {
            get
            {
                return this._collisionCategories;
            }
            set
            {
                if (this._collisionCategories == value)
                    return;
                this._collisionCategories = value;
                this.Refilter();
            }
        }

        public Shape Shape { get; internal set; }

        public bool IsSensor
        {
            get
            {
                return this._isSensor;
            }
            set
            {
                if (this.Body != null)
                    this.Body.Awake = true;
                this._isSensor = value;
            }
        }

        public Body Body { get; internal set; }

        public object UserData { get; set; }

        public FP Friction
        {
            get
            {
                return this._friction;
            }
            set
            {
                Debug.Assert(!FP.IsNaN(value));
                this._friction = value;
            }
        }

        public FP Restitution
        {
            get
            {
                return this._restitution;
            }
            set
            {
                Debug.Assert(!FP.IsNaN(value));
                this._restitution = value;
            }
        }

        public int FixtureId { get; internal set; }

        public bool IsDisposed { get; set; }

        public void Dispose()
        {
            if (this.IsDisposed)
                return;
            this.Body.DestroyFixture(this);
            this.IsDisposed = true;
            GC.SuppressFinalize((object)this);
        }

        public void RestoreCollisionWith(Fixture fixture)
        {
            if (!this._collisionIgnores.Contains(fixture.FixtureId))
                return;
            this._collisionIgnores.Remove(fixture.FixtureId);
            this.Refilter();
        }

        public void IgnoreCollisionWith(Fixture fixture)
        {
            if (this._collisionIgnores.Contains(fixture.FixtureId))
                return;
            this._collisionIgnores.Add(fixture.FixtureId);
            this.Refilter();
        }

        public bool IsFixtureIgnored(Fixture fixture)
        {
            return this._collisionIgnores.Contains(fixture.FixtureId);
        }

        private void Refilter()
        {
            for (ContactEdge contactEdge = this.Body.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
            {
                Contact contact = contactEdge.Contact;
                if (contact.FixtureA == this || contact.FixtureB == this)
                    contact.FilterFlag = true;
            }
            World world = this.Body._world;
            if (world == null)
                return;
            IBroadPhase broadPhase = world.ContactManager.BroadPhase;
            for (int index = 0; index < this.ProxyCount; ++index)
                broadPhase.TouchProxy(this.Proxies[index].ProxyId);
        }

        private void RegisterFixture()
        {
            this.Proxies = new FixtureProxy[this.Shape.ChildCount];
            this.ProxyCount = 0;
            if (this.Body.Enabled)
                this.CreateProxies(this.Body._world.ContactManager.BroadPhase, ref this.Body._xf);
            this.Body.FixtureList.Add(this);
            if (this.Shape._density > (FP)0.0f)
                this.Body.ResetMassData();
            this.Body._world._worldHasNewFixture = true;
            if (this.Body._world.FixtureAdded == null)
                return;
            this.Body._world.FixtureAdded(this);
        }

        public bool TestPoint(ref TSVector2 point)
        {
            return this.Shape.TestPoint(ref this.Body._xf, ref point);
        }

        public bool RayCast(out RayCastOutput output, ref RayCastInput input, int childIndex)
        {
            return this.Shape.RayCast(out output, ref input, ref this.Body._xf, childIndex);
        }

        public void GetAABB(out AABB aabb, int childIndex)
        {
            Debug.Assert(0 <= childIndex && childIndex < this.ProxyCount);
            aabb = this.Proxies[childIndex].AABB;
        }

        internal void Destroy()
        {
            if (this.Shape.ShapeType == ShapeType.Polygon)
                ((PolygonShape)this.Shape).Vertices.AttachedToBody = false;
            Debug.Assert(this.ProxyCount == 0);
            this.Proxies = (FixtureProxy[])null;
            this.Shape = (Shape)null;
            this.UserData = (object)null;
            this.BeforeCollision = (BeforeCollisionEventHandler)null;
            this.OnCollision = (OnCollisionEventHandler)null;
            this.OnSeparation = (OnSeparationEventHandler)null;
            this.AfterCollision = (AfterCollisionEventHandler)null;
            if (this.Body._world.FixtureRemoved != null)
                this.Body._world.FixtureRemoved(this);
            this.Body._world.FixtureAdded = (FixtureDelegate)null;
            this.Body._world.FixtureRemoved = (FixtureDelegate)null;
            this.OnSeparation = (OnSeparationEventHandler)null;
            this.OnCollision = (OnCollisionEventHandler)null;
        }

        internal void CreateProxies(IBroadPhase broadPhase, ref Transform xf)
        {
            Debug.Assert(this.ProxyCount == 0);
            this.ProxyCount = this.Shape.ChildCount;
            for (int childIndex = 0; childIndex < this.ProxyCount; ++childIndex)
            {
                FixtureProxy proxy = new FixtureProxy();
                this.Shape.ComputeAABB(out proxy.AABB, ref xf, childIndex);
                proxy.Fixture = this;
                proxy.ChildIndex = childIndex;
                proxy.ProxyId = broadPhase.AddProxy(ref proxy);
                this.Proxies[childIndex] = proxy;
            }
        }

        internal void DestroyProxies(IBroadPhase broadPhase)
        {
            for (int index = 0; index < this.ProxyCount; ++index)
            {
                broadPhase.RemoveProxy(this.Proxies[index].ProxyId);
                this.Proxies[index].ProxyId = -1;
            }
            this.ProxyCount = 0;
        }

        internal void Synchronize(IBroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
        {
            if (this.ProxyCount == 0)
                return;
            for (int index = 0; index < this.ProxyCount; ++index)
            {
                FixtureProxy proxy = this.Proxies[index];
                AABB aabb1;
                this.Shape.ComputeAABB(out aabb1, ref transform1, proxy.ChildIndex);
                AABB aabb2;
                this.Shape.ComputeAABB(out aabb2, ref transform2, proxy.ChildIndex);
                proxy.AABB.Combine(ref aabb1, ref aabb2);
                TSVector2 displacement = transform2.p - transform1.p;
                broadPhase.MoveProxy(proxy.ProxyId, ref proxy.AABB, displacement);
            }
        }

        internal bool CompareTo(Fixture fixture)
        {
            return this._collidesWith == fixture._collidesWith && this._collisionCategories == fixture._collisionCategories && ((int)this._collisionGroup == (int)fixture._collisionGroup && this.Friction == fixture.Friction) && (this.IsSensor == fixture.IsSensor && this.Restitution == fixture.Restitution && (this.UserData == fixture.UserData && this.IgnoreCCDWith == fixture.IgnoreCCDWith)) && this.SequenceEqual<int>(this._collisionIgnores, fixture._collisionIgnores);
        }

        private bool SequenceEqual<T>(HashSet<T> first, HashSet<T> second)
        {
            if (first.Count != second.Count)
                return false;
            using (IEnumerator<T> enumerator1 = (IEnumerator<T>)first.GetEnumerator())
            {
                using (IEnumerator<T> enumerator2 = (IEnumerator<T>)second.GetEnumerator())
                {
                    while (enumerator1.MoveNext())
                    {
                        if (!enumerator2.MoveNext() || !object.Equals((object)enumerator1.Current, (object)enumerator2.Current))
                            return false;
                    }
                    if (enumerator2.MoveNext())
                        return false;
                }
            }
            return true;
        }

        public Fixture CloneOnto(Body body)
        {
            Fixture fixture = new Fixture();
            fixture.Body = body;
            fixture.Shape = this.Shape.Clone();
            fixture.UserData = this.UserData;
            fixture.Restitution = this.Restitution;
            fixture.Friction = this.Friction;
            fixture.IsSensor = this.IsSensor;
            fixture._collisionGroup = this._collisionGroup;
            fixture._collisionCategories = this._collisionCategories;
            fixture._collidesWith = this._collidesWith;
            fixture.IgnoreCCDWith = this.IgnoreCCDWith;
            foreach (int collisionIgnore in this._collisionIgnores)
                fixture._collisionIgnores.Add(collisionIgnore);
            fixture.RegisterFixture();
            return fixture;
        }
    }
}
