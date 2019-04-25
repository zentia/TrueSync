namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class Fixture : IDisposable
    {
        internal Category _collidesWith;
        internal Category _collisionCategories;
        internal short _collisionGroup;
        internal HashSet<int> _collisionIgnores;
        [ThreadStatic]
        internal static int _fixtureIdCounter;
        private FP _friction;
        private bool _isSensor;
        private FP _restitution;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.Body <Body>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <FixtureId>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IsDisposed>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.Shape <Shape>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object <UserData>k__BackingField;
        public AfterCollisionEventHandler AfterCollision;
        public BeforeCollisionEventHandler BeforeCollision;
        public Category IgnoreCCDWith;
        public OnCollisionEventHandler OnCollision;
        public OnSeparationEventHandler OnSeparation;
        public FixtureProxy[] Proxies;
        public int ProxyCount;

        internal Fixture()
        {
            this.FixtureId = _fixtureIdCounter++;
            this._collisionCategories = Settings.DefaultFixtureCollisionCategories;
            this._collidesWith = Settings.DefaultFixtureCollidesWith;
            this._collisionGroup = 0;
            this._collisionIgnores = new HashSet<int>();
            this.IgnoreCCDWith = Settings.DefaultFixtureIgnoreCCDWith;
            this.Friction = 0.2f;
            this.Restitution = 0;
        }

        internal Fixture(TrueSync.Physics2D.Body body, TrueSync.Physics2D.Shape shape, object userData = null) : this()
        {
            if (shape.ShapeType == ShapeType.Polygon)
            {
                ((PolygonShape) shape).Vertices.AttachedToBody = true;
            }
            this.Body = body;
            this.UserData = userData;
            this.Shape = shape.Clone();
            this.RegisterFixture();
        }

        public Fixture CloneOnto(TrueSync.Physics2D.Body body)
        {
            Fixture fixture = new Fixture {
                Body = body,
                Shape = this.Shape.Clone(),
                UserData = this.UserData,
                Restitution = this.Restitution,
                Friction = this.Friction,
                IsSensor = this.IsSensor,
                _collisionGroup = this._collisionGroup,
                _collisionCategories = this._collisionCategories,
                _collidesWith = this._collidesWith,
                IgnoreCCDWith = this.IgnoreCCDWith
            };
            foreach (int num in this._collisionIgnores)
            {
                fixture._collisionIgnores.Add(num);
            }
            fixture.RegisterFixture();
            return fixture;
        }

        internal bool CompareTo(Fixture fixture)
        {
            return (((((this._collidesWith == fixture._collidesWith) && (this._collisionCategories == fixture._collisionCategories)) && ((this._collisionGroup == fixture._collisionGroup) && (this.Friction == fixture.Friction))) && (((this.IsSensor == fixture.IsSensor) && (this.Restitution == fixture.Restitution)) && ((this.UserData == fixture.UserData) && (this.IgnoreCCDWith == fixture.IgnoreCCDWith)))) && this.SequenceEqual<int>(this._collisionIgnores, fixture._collisionIgnores));
        }

        internal void CreateProxies(IBroadPhase broadPhase, ref Transform xf)
        {
            Debug.Assert(this.ProxyCount == 0);
            this.ProxyCount = this.Shape.ChildCount;
            for (int i = 0; i < this.ProxyCount; i++)
            {
                FixtureProxy proxy = new FixtureProxy();
                this.Shape.ComputeAABB(out proxy.AABB, ref xf, i);
                proxy.Fixture = this;
                proxy.ChildIndex = i;
                proxy.ProxyId = broadPhase.AddProxy(ref proxy);
                this.Proxies[i] = proxy;
            }
        }

        internal void Destroy()
        {
            if (this.Shape.ShapeType == ShapeType.Polygon)
            {
                ((PolygonShape) this.Shape).Vertices.AttachedToBody = false;
            }
            Debug.Assert(this.ProxyCount == 0);
            this.Proxies = null;
            this.Shape = null;
            this.UserData = null;
            this.BeforeCollision = null;
            this.OnCollision = null;
            this.OnSeparation = null;
            this.AfterCollision = null;
            if (this.Body._world.FixtureRemoved > null)
            {
                this.Body._world.FixtureRemoved(this);
            }
            this.Body._world.FixtureAdded = null;
            this.Body._world.FixtureRemoved = null;
            this.OnSeparation = null;
            this.OnCollision = null;
        }

        internal void DestroyProxies(IBroadPhase broadPhase)
        {
            for (int i = 0; i < this.ProxyCount; i++)
            {
                broadPhase.RemoveProxy(this.Proxies[i].ProxyId);
                this.Proxies[i].ProxyId = -1;
            }
            this.ProxyCount = 0;
        }

        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.Body.DestroyFixture(this);
                this.IsDisposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void GetAABB(out AABB aabb, int childIndex)
        {
            Debug.Assert((0 <= childIndex) && (childIndex < this.ProxyCount));
            aabb = this.Proxies[childIndex].AABB;
        }

        public void IgnoreCollisionWith(Fixture fixture)
        {
            if (!this._collisionIgnores.Contains(fixture.FixtureId))
            {
                this._collisionIgnores.Add(fixture.FixtureId);
                this.Refilter();
            }
        }

        public bool IsFixtureIgnored(Fixture fixture)
        {
            return this._collisionIgnores.Contains(fixture.FixtureId);
        }

        public bool RayCast(out RayCastOutput output, ref RayCastInput input, int childIndex)
        {
            return this.Shape.RayCast(out output, ref input, ref this.Body._xf, childIndex);
        }

        private void Refilter()
        {
            for (ContactEdge edge = this.Body.ContactList; edge > null; edge = edge.Next)
            {
                TrueSync.Physics2D.Contact contact = edge.Contact;
                Fixture fixtureA = contact.FixtureA;
                Fixture fixtureB = contact.FixtureB;
                if ((fixtureA == this) || (fixtureB == this))
                {
                    contact.FilterFlag = true;
                }
            }
            TrueSync.Physics2D.World world = this.Body._world;
            if (world != null)
            {
                IBroadPhase broadPhase = world.ContactManager.BroadPhase;
                for (int i = 0; i < this.ProxyCount; i++)
                {
                    broadPhase.TouchProxy(this.Proxies[i].ProxyId);
                }
            }
        }

        private void RegisterFixture()
        {
            this.Proxies = new FixtureProxy[this.Shape.ChildCount];
            this.ProxyCount = 0;
            if (this.Body.Enabled)
            {
                IBroadPhase broadPhase = this.Body._world.ContactManager.BroadPhase;
                this.CreateProxies(broadPhase, ref this.Body._xf);
            }
            this.Body.FixtureList.Add(this);
            if (this.Shape._density > 0f)
            {
                this.Body.ResetMassData();
            }
            this.Body._world._worldHasNewFixture = true;
            if (this.Body._world.FixtureAdded > null)
            {
                this.Body._world.FixtureAdded(this);
            }
        }

        public void RestoreCollisionWith(Fixture fixture)
        {
            if (this._collisionIgnores.Contains(fixture.FixtureId))
            {
                this._collisionIgnores.Remove(fixture.FixtureId);
                this.Refilter();
            }
        }

        private bool SequenceEqual<T>(HashSet<T> first, HashSet<T> second)
        {
            if (first.Count != second.Count)
            {
                return false;
            }
            using (IEnumerator<T> enumerator = first.GetEnumerator())
            {
                using (IEnumerator<T> enumerator2 = second.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        if (!enumerator2.MoveNext() || !object.Equals(enumerator.Current, enumerator2.Current))
                        {
                            return false;
                        }
                    }
                    if (enumerator2.MoveNext())
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        internal void Synchronize(IBroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
        {
            if (this.ProxyCount != 0)
            {
                for (int i = 0; i < this.ProxyCount; i++)
                {
                    AABB aabb;
                    AABB aabb2;
                    FixtureProxy proxy = this.Proxies[i];
                    this.Shape.ComputeAABB(out aabb, ref transform1, proxy.ChildIndex);
                    this.Shape.ComputeAABB(out aabb2, ref transform2, proxy.ChildIndex);
                    proxy.AABB.Combine(ref aabb, ref aabb2);
                    TSVector2 displacement = transform2.p - transform1.p;
                    broadPhase.MoveProxy(proxy.ProxyId, ref proxy.AABB, displacement);
                }
            }
        }

        public bool TestPoint(ref TSVector2 point)
        {
            return this.Shape.TestPoint(ref this.Body._xf, ref point);
        }

        public TrueSync.Physics2D.Body Body { get; internal set; }

        public Category CollidesWith
        {
            get
            {
                return this._collidesWith;
            }
            set
            {
                if (this._collidesWith != value)
                {
                    this._collidesWith = value;
                    this.Refilter();
                }
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
                if (this._collisionCategories != value)
                {
                    this._collisionCategories = value;
                    this.Refilter();
                }
            }
        }

        public short CollisionGroup
        {
            get
            {
                return this._collisionGroup;
            }
            set
            {
                if (this._collisionGroup != value)
                {
                    this._collisionGroup = value;
                    this.Refilter();
                }
            }
        }

        public int FixtureId { get; internal set; }

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

        public bool IsDisposed { get; set; }

        public bool IsSensor
        {
            get
            {
                return this._isSensor;
            }
            set
            {
                if (this.Body > null)
                {
                    this.Body.Awake = true;
                }
                this._isSensor = value;
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

        public TrueSync.Physics2D.Shape Shape { get; internal set; }

        public object UserData { get; set; }
    }
}

