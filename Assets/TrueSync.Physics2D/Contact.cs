using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class Contact
	{
		internal enum ContactType
		{
			NotSupported,
			Polygon,
			PolygonAndCircle,
			Circle,
			EdgeAndPolygon,
			EdgeAndCircle,
			ChainAndPolygon,
			ChainAndCircle
		}

		internal Contact.ContactType _type;

		private static EdgeShape _edge = new EdgeShape();

		private static Contact.ContactType[,] _registers;

		internal ContactEdge _nodeA = new ContactEdge();

		internal ContactEdge _nodeB = new ContactEdge();

		internal int _toiCount;

		internal FP _toi;

		public Fixture FixtureA;

		public Fixture FixtureB;

		public Manifold Manifold;

		public FP Friction
		{
			get;
			set;
		}

		public FP Restitution
		{
			get;
			set;
		}

		internal string Key
		{
			get
			{
				return this.FixtureA.FixtureId + "_" + this.FixtureB.FixtureId;
			}
		}

		public FP TangentSpeed
		{
			get;
			set;
		}

		public bool Enabled
		{
			get;
			set;
		}

		public int ChildIndexA
		{
			get;
			internal set;
		}

		public int ChildIndexB
		{
			get;
			internal set;
		}

		public bool IsTouching
		{
			get;
			set;
		}

		internal bool IslandFlag
		{
			get;
			set;
		}

		internal bool TOIFlag
		{
			get;
			set;
		}

		internal bool FilterFlag
		{
			get;
			set;
		}

		public void ResetRestitution()
		{
			this.Restitution = Settings.MixRestitution(this.FixtureA.Restitution, this.FixtureB.Restitution);
		}

		public void ResetFriction()
		{
			this.Friction = Settings.MixFriction(this.FixtureA.Friction, this.FixtureB.Friction);
		}

		internal Contact()
		{
		}

		internal Contact(Fixture fA, int indexA, Fixture fB, int indexB)
		{
			this.Reset(fA, indexA, fB, indexB);
		}

		public TSVector2 CalculateRelativeVelocity()
		{
			TSVector2 result;
			result.x = this.FixtureA.Body.LinearVelocity.x - this.FixtureB.Body.LinearVelocity.x;
			result.y = this.FixtureA.Body.LinearVelocity.y - this.FixtureB.Body.LinearVelocity.y;
			return result;
		}

		public void GetWorldManifold(out TSVector2 normal, out FixedArray2<TSVector2> points)
		{
			Body body = this.FixtureA.Body;
			Body body2 = this.FixtureB.Body;
			Shape shape = this.FixtureA.Shape;
			Shape shape2 = this.FixtureB.Shape;
			ContactSolver.WorldManifold.Initialize(ref this.Manifold, ref body._xf, shape.Radius, ref body2._xf, shape2.Radius, out normal, out points);
		}

		private void Reset(Fixture fA, int indexA, Fixture fB, int indexB)
		{
			this.Enabled = true;
			this.IsTouching = false;
			this.IslandFlag = false;
			this.FilterFlag = false;
			this.TOIFlag = false;
			this.FixtureA = fA;
			this.FixtureB = fB;
			this.ChildIndexA = indexA;
			this.ChildIndexB = indexB;
			this.Manifold.PointCount = 0;
			this._nodeA.Contact = null;
			this._nodeA.Prev = null;
			this._nodeA.Next = null;
			this._nodeA.Other = null;
			this._nodeB.Contact = null;
			this._nodeB.Prev = null;
			this._nodeB.Next = null;
			this._nodeB.Other = null;
			this._toiCount = 0;
			bool flag = this.FixtureA != null && this.FixtureB != null;
			if (flag)
			{
				this.Friction = Settings.MixFriction(this.FixtureA.Friction, this.FixtureB.Friction);
				this.Restitution = Settings.MixRestitution(this.FixtureA.Restitution, this.FixtureB.Restitution);
			}
			this.TangentSpeed = 0;
		}

		internal void Update(ContactManager contactManager)
		{
			Body body = this.FixtureA.Body;
			Body body2 = this.FixtureB.Body;
			bool flag = this.FixtureA == null || this.FixtureB == null;
			if (!flag)
			{
				bool flag2 = !ContactManager.CheckCollisionConditions(this.FixtureA, this.FixtureB);
				if (flag2)
				{
					this.Enabled = false;
				}
				else
				{
					Manifold manifold = this.Manifold;
					this.Enabled = true;
					bool isTouching = this.IsTouching;
					bool flag3 = this.FixtureA.IsSensor || this.FixtureB.IsSensor;
					bool flag4 = flag3;
					bool flag5;
					if (flag4)
					{
						Shape shape = this.FixtureA.Shape;
						Shape shape2 = this.FixtureB.Shape;
						flag5 = Collision.TestOverlap(shape, this.ChildIndexA, shape2, this.ChildIndexB, ref body._xf, ref body2._xf);
						this.Manifold.PointCount = 0;
					}
					else
					{
						this.Evaluate(ref this.Manifold, ref body._xf, ref body2._xf);
						flag5 = (this.Manifold.PointCount > 0);
						for (int i = 0; i < this.Manifold.PointCount; i++)
						{
							ManifoldPoint manifoldPoint = this.Manifold.Points[i];
							manifoldPoint.NormalImpulse = 0f;
							manifoldPoint.TangentImpulse = 0f;
							ContactID id = manifoldPoint.Id;
							for (int j = 0; j < manifold.PointCount; j++)
							{
								ManifoldPoint manifoldPoint2 = manifold.Points[j];
								bool flag6 = manifoldPoint2.Id.Key == id.Key;
								if (flag6)
								{
									manifoldPoint.NormalImpulse = manifoldPoint2.NormalImpulse;
									manifoldPoint.TangentImpulse = manifoldPoint2.TangentImpulse;
									break;
								}
							}
							this.Manifold.Points[i] = manifoldPoint;
						}
						bool flag7 = flag5 != isTouching;
						if (flag7)
						{
							body.Awake = true;
							body2.Awake = true;
						}
					}
					this.IsTouching = flag5;
					bool flag8 = !isTouching;
					if (flag8)
					{
						bool flag9 = flag5;
						if (flag9)
						{
							bool flag10 = true;
							bool flag11 = true;
							bool flag12 = this.FixtureA.OnCollision != null;
							if (flag12)
							{
								Delegate[] invocationList = this.FixtureA.OnCollision.GetInvocationList();
								for (int k = 0; k < invocationList.Length; k++)
								{
									OnCollisionEventHandler onCollisionEventHandler = (OnCollisionEventHandler)invocationList[k];
									flag10 = (onCollisionEventHandler(this.FixtureA, this.FixtureB, this) & flag10);
								}
							}
							bool flag13 = this.FixtureB.OnCollision != null;
							if (flag13)
							{
								Delegate[] invocationList2 = this.FixtureB.OnCollision.GetInvocationList();
								for (int l = 0; l < invocationList2.Length; l++)
								{
									OnCollisionEventHandler onCollisionEventHandler2 = (OnCollisionEventHandler)invocationList2[l];
									flag11 = (onCollisionEventHandler2(this.FixtureB, this.FixtureA, this) & flag11);
								}
							}
							this.Enabled = (flag10 & flag11);
							bool flag14 = (flag10 & flag11) && contactManager.BeginContact != null;
							if (flag14)
							{
								this.Enabled = contactManager.BeginContact(this);
							}
							bool flag15 = !this.Enabled;
							if (flag15)
							{
								this.IsTouching = false;
							}
						}
					}
					else
					{
						bool flag16 = !flag5;
						if (flag16)
						{
							bool flag17 = this.FixtureA != null && this.FixtureA.OnSeparation != null;
							if (flag17)
							{
								this.FixtureA.OnSeparation(this.FixtureA, this.FixtureB);
							}
							bool flag18 = this.FixtureB != null && this.FixtureB.OnSeparation != null;
							if (flag18)
							{
								this.FixtureB.OnSeparation(this.FixtureB, this.FixtureA);
							}
							bool flag19 = contactManager.EndContact != null;
							if (flag19)
							{
								contactManager.EndContact(this);
							}
						}
						else
						{
							bool flag20 = contactManager.StayContact != null;
							if (flag20)
							{
								contactManager.StayContact(this);
							}
						}
					}
					bool flag21 = flag3;
					if (!flag21)
					{
						bool flag22 = contactManager.PreSolve != null;
						if (flag22)
						{
							contactManager.PreSolve(this, ref manifold);
						}
					}
				}
			}
		}

		private void Evaluate(ref Manifold manifold, ref Transform transformA, ref Transform transformB)
		{
			switch (this._type)
			{
			case Contact.ContactType.Polygon:
				Collision.CollidePolygons(ref manifold, (PolygonShape)this.FixtureA.Shape, ref transformA, (PolygonShape)this.FixtureB.Shape, ref transformB);
				break;
			case Contact.ContactType.PolygonAndCircle:
				Collision.CollidePolygonAndCircle(ref manifold, (PolygonShape)this.FixtureA.Shape, ref transformA, (CircleShape)this.FixtureB.Shape, ref transformB);
				break;
			case Contact.ContactType.Circle:
				Collision.CollideCircles(ref manifold, (CircleShape)this.FixtureA.Shape, ref transformA, (CircleShape)this.FixtureB.Shape, ref transformB);
				break;
			case Contact.ContactType.EdgeAndPolygon:
				Collision.CollideEdgeAndPolygon(ref manifold, (EdgeShape)this.FixtureA.Shape, ref transformA, (PolygonShape)this.FixtureB.Shape, ref transformB);
				break;
			case Contact.ContactType.EdgeAndCircle:
				Collision.CollideEdgeAndCircle(ref manifold, (EdgeShape)this.FixtureA.Shape, ref transformA, (CircleShape)this.FixtureB.Shape, ref transformB);
				break;
			case Contact.ContactType.ChainAndPolygon:
			{
				ChainShape chainShape = (ChainShape)this.FixtureA.Shape;
				chainShape.GetChildEdge(Contact._edge, this.ChildIndexA);
				Collision.CollideEdgeAndPolygon(ref manifold, Contact._edge, ref transformA, (PolygonShape)this.FixtureB.Shape, ref transformB);
				break;
			}
			case Contact.ContactType.ChainAndCircle:
			{
				ChainShape chainShape2 = (ChainShape)this.FixtureA.Shape;
				chainShape2.GetChildEdge(Contact._edge, this.ChildIndexA);
				Collision.CollideEdgeAndCircle(ref manifold, Contact._edge, ref transformA, (CircleShape)this.FixtureB.Shape, ref transformB);
				break;
			}
			}
		}

		internal static Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
		{
			ShapeType shapeType = fixtureA.Shape.ShapeType;
			ShapeType shapeType2 = fixtureB.Shape.ShapeType;
			Debug.Assert(ShapeType.Unknown < shapeType && shapeType < ShapeType.TypeCount);
			Debug.Assert(ShapeType.Unknown < shapeType2 && shapeType2 < ShapeType.TypeCount);
			Queue<Contact> contactPool = fixtureA.Body._world._contactPool;
			bool flag = contactPool.Count > 0;
			Contact contact;
			if (flag)
			{
				contact = contactPool.Dequeue();
				bool flag2 = (shapeType >= shapeType2 || (shapeType == ShapeType.Edge && shapeType2 == ShapeType.Polygon)) && (shapeType2 != ShapeType.Edge || shapeType != ShapeType.Polygon);
				if (flag2)
				{
					contact.Reset(fixtureA, indexA, fixtureB, indexB);
				}
				else
				{
					contact.Reset(fixtureB, indexB, fixtureA, indexA);
				}
			}
			else
			{
				bool flag3 = (shapeType >= shapeType2 || (shapeType == ShapeType.Edge && shapeType2 == ShapeType.Polygon)) && (shapeType2 != ShapeType.Edge || shapeType != ShapeType.Polygon);
				if (flag3)
				{
					contact = new Contact(fixtureA, indexA, fixtureB, indexB);
				}
				else
				{
					contact = new Contact(fixtureB, indexB, fixtureA, indexA);
				}
			}
			contact._type = Contact._registers[(int)shapeType, (int)shapeType2];
			return contact;
		}

		internal void Destroy()
		{
			this.FixtureA.Body._world._contactPool.Enqueue(this);
			bool flag = this.Manifold.PointCount > 0 && !this.FixtureA.IsSensor && !this.FixtureB.IsSensor;
			if (flag)
			{
				this.FixtureA.Body.Awake = true;
				this.FixtureB.Body.Awake = true;
			}
			this.Reset(null, 0, null, 0);
		}

		static Contact()
		{
			// Note: this type is marked as 'beforefieldinit'.
			Contact.ContactType[,] expr_11 = new Contact.ContactType[4, 4];
			expr_11[0, 0] = Contact.ContactType.Circle;
			expr_11[0, 1] = Contact.ContactType.EdgeAndCircle;
			expr_11[0, 2] = Contact.ContactType.PolygonAndCircle;
			expr_11[0, 3] = Contact.ContactType.ChainAndCircle;
			expr_11[1, 0] = Contact.ContactType.EdgeAndCircle;
			expr_11[1, 2] = Contact.ContactType.EdgeAndPolygon;
			expr_11[2, 0] = Contact.ContactType.PolygonAndCircle;
			expr_11[2, 1] = Contact.ContactType.EdgeAndPolygon;
			expr_11[2, 2] = Contact.ContactType.Polygon;
			expr_11[2, 3] = Contact.ContactType.ChainAndPolygon;
			expr_11[3, 0] = Contact.ContactType.ChainAndCircle;
			expr_11[3, 2] = Contact.ContactType.ChainAndPolygon;
			Contact._registers = expr_11;
		}
	}
}
