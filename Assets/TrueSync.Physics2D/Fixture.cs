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

		public short CollisionGroup
		{
			get
			{
				return this._collisionGroup;
			}
			set
			{
				bool flag = this._collisionGroup == value;
				if (!flag)
				{
					this._collisionGroup = value;
					this.Refilter();
				}
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
				bool flag = this._collidesWith == value;
				if (!flag)
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
				bool flag = this._collisionCategories == value;
				if (!flag)
				{
					this._collisionCategories = value;
					this.Refilter();
				}
			}
		}

		public Shape Shape
		{
			get;
			internal set;
		}

		public bool IsSensor
		{
			get
			{
				return this._isSensor;
			}
			set
			{
				bool flag = this.Body != null;
				if (flag)
				{
					this.Body.Awake = true;
				}
				this._isSensor = value;
			}
		}

		public Body Body
		{
			get;
			internal set;
		}

		public object UserData
		{
			get;
			set;
		}

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

		public int FixtureId
		{
			get;
			internal set;
		}

		public bool IsDisposed
		{
			get;
			set;
		}

		internal Fixture()
		{
			this.FixtureId = Fixture._fixtureIdCounter++;
			this._collisionCategories = Settings.DefaultFixtureCollisionCategories;
			this._collidesWith = Settings.DefaultFixtureCollidesWith;
			this._collisionGroup = 0;
			this._collisionIgnores = new HashSet<int>();
			this.IgnoreCCDWith = Settings.DefaultFixtureIgnoreCCDWith;
			this.Friction = 0.2f;
			this.Restitution = 0;
		}

		internal Fixture(Body body, Shape shape, object userData = null) : this()
		{
			bool flag = shape.ShapeType == ShapeType.Polygon;
			if (flag)
			{
				((PolygonShape)shape).Vertices.AttachedToBody = true;
			}
			this.Body = body;
			this.UserData = userData;
			this.Shape = shape.Clone();
			this.RegisterFixture();
		}

		public void Dispose()
		{
			bool flag = !this.IsDisposed;
			if (flag)
			{
				this.Body.DestroyFixture(this);
				this.IsDisposed = true;
				GC.SuppressFinalize(this);
			}
		}

		public void RestoreCollisionWith(Fixture fixture)
		{
			bool flag = this._collisionIgnores.Contains(fixture.FixtureId);
			if (flag)
			{
				this._collisionIgnores.Remove(fixture.FixtureId);
				this.Refilter();
			}
		}

		public void IgnoreCollisionWith(Fixture fixture)
		{
			bool flag = !this._collisionIgnores.Contains(fixture.FixtureId);
			if (flag)
			{
				this._collisionIgnores.Add(fixture.FixtureId);
				this.Refilter();
			}
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
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				bool flag = fixtureA == this || fixtureB == this;
				if (flag)
				{
					contact.FilterFlag = true;
				}
			}
			World world = this.Body._world;
			bool flag2 = world == null;
			if (!flag2)
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
			bool enabled = this.Body.Enabled;
			if (enabled)
			{
				IBroadPhase broadPhase = this.Body._world.ContactManager.BroadPhase;
				this.CreateProxies(broadPhase, ref this.Body._xf);
			}
			this.Body.FixtureList.Add(this);
			bool flag = this.Shape._density > 0f;
			if (flag)
			{
				this.Body.ResetMassData();
			}
			this.Body._world._worldHasNewFixture = true;
			bool flag2 = this.Body._world.FixtureAdded != null;
			if (flag2)
			{
				this.Body._world.FixtureAdded(this);
			}
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
			bool flag = this.Shape.ShapeType == ShapeType.Polygon;
			if (flag)
			{
				((PolygonShape)this.Shape).Vertices.AttachedToBody = false;
			}
			Debug.Assert(this.ProxyCount == 0);
			this.Proxies = null;
			this.Shape = null;
			this.UserData = null;
			this.BeforeCollision = null;
			this.OnCollision = null;
			this.OnSeparation = null;
			this.AfterCollision = null;
			bool flag2 = this.Body._world.FixtureRemoved != null;
			if (flag2)
			{
				this.Body._world.FixtureRemoved(this);
			}
			this.Body._world.FixtureAdded = null;
			this.Body._world.FixtureRemoved = null;
			this.OnSeparation = null;
			this.OnCollision = null;
		}

		internal void CreateProxies(IBroadPhase broadPhase, ref Transform xf)
		{
			Debug.Assert(this.ProxyCount == 0);
			this.ProxyCount = this.Shape.ChildCount;
			for (int i = 0; i < this.ProxyCount; i++)
			{
				FixtureProxy fixtureProxy = default(FixtureProxy);
				this.Shape.ComputeAABB(out fixtureProxy.AABB, ref xf, i);
				fixtureProxy.Fixture = this;
				fixtureProxy.ChildIndex = i;
				fixtureProxy.ProxyId = broadPhase.AddProxy(ref fixtureProxy);
				this.Proxies[i] = fixtureProxy;
			}
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

		internal void Synchronize(IBroadPhase broadPhase, ref Transform transform1, ref Transform transform2)
		{
			bool flag = this.ProxyCount == 0;
			if (!flag)
			{
				for (int i = 0; i < this.ProxyCount; i++)
				{
					FixtureProxy fixtureProxy = this.Proxies[i];
					AABB aABB;
					this.Shape.ComputeAABB(out aABB, ref transform1, fixtureProxy.ChildIndex);
					AABB aABB2;
					this.Shape.ComputeAABB(out aABB2, ref transform2, fixtureProxy.ChildIndex);
					fixtureProxy.AABB.Combine(ref aABB, ref aABB2);
					TSVector2 displacement = transform2.p - transform1.p;
					broadPhase.MoveProxy(fixtureProxy.ProxyId, ref fixtureProxy.AABB, displacement);
				}
			}
		}

		internal bool CompareTo(Fixture fixture)
		{
			return this._collidesWith == fixture._collidesWith && this._collisionCategories == fixture._collisionCategories && this._collisionGroup == fixture._collisionGroup && this.Friction == fixture.Friction && this.IsSensor == fixture.IsSensor && this.Restitution == fixture.Restitution && this.UserData == fixture.UserData && this.IgnoreCCDWith == fixture.IgnoreCCDWith && this.SequenceEqual<int>(this._collisionIgnores, fixture._collisionIgnores);
		}

		private bool SequenceEqual<T>(HashSet<T> first, HashSet<T> second)
		{
			bool flag = first.Count != second.Count;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				using (IEnumerator<T> enumerator = first.GetEnumerator())
				{
					using (IEnumerator<T> enumerator2 = second.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							bool flag2 = !enumerator2.MoveNext() || !object.Equals(enumerator.Current, enumerator2.Current);
							if (flag2)
							{
								result = false;
								return result;
							}
						}
						bool flag3 = enumerator2.MoveNext();
						if (flag3)
						{
							result = false;
							return result;
						}
					}
				}
				result = true;
			}
			return result;
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
			foreach (int current in this._collisionIgnores)
			{
				fixture._collisionIgnores.Add(current);
			}
			fixture.RegisterFixture();
			return fixture;
		}
	}
}
