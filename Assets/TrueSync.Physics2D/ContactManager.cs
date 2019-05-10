using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public class ContactManager
	{
		public BeginContactDelegate BeginContact;

		public IBroadPhase BroadPhase;

		public CollisionFilterDelegate ContactFilter;

		public List<Contact> ContactList = new List<Contact>(128);

		public EndContactDelegate StayContact;

		public EndContactDelegate EndContact;

		public BroadphaseDelegate OnBroadphaseCollision;

		public PostSolveDelegate PostSolve;

		public PreSolveDelegate PreSolve;

		public static IPhysicsManager physicsManager;

		internal ContactManager(IBroadPhase broadPhase)
		{
			this.BroadPhase = broadPhase;
			this.OnBroadphaseCollision = new BroadphaseDelegate(this.AddPair);
		}

		private void AddPair(ref FixtureProxy proxyA, ref FixtureProxy proxyB)
		{
			Fixture fixture = proxyA.Fixture;
			Fixture fixture2 = proxyB.Fixture;
			int childIndex = proxyA.ChildIndex;
			int childIndex2 = proxyB.ChildIndex;
			Body body = fixture.Body;
			Body body2 = fixture2.Body;
			bool flag = body == body2;
			if (!flag)
			{
				for (ContactEdge contactEdge = body2.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
				{
					bool flag2 = contactEdge.Other == body;
					if (flag2)
					{
						Fixture fixtureA = contactEdge.Contact.FixtureA;
						Fixture fixtureB = contactEdge.Contact.FixtureB;
						int childIndexA = contactEdge.Contact.ChildIndexA;
						int childIndexB = contactEdge.Contact.ChildIndexB;
						bool flag3 = fixtureA == fixture && fixtureB == fixture2 && childIndexA == childIndex && childIndexB == childIndex2;
						if (flag3)
						{
							return;
						}
						bool flag4 = fixtureA == fixture2 && fixtureB == fixture && childIndexA == childIndex2 && childIndexB == childIndex;
						if (flag4)
						{
							return;
						}
					}
				}
				bool flag5 = !body2.ShouldCollide(body);
				if (!flag5)
				{
					bool flag6 = !ContactManager.ShouldCollide(fixture, fixture2);
					if (!flag6)
					{
						bool flag7 = this.ContactFilter != null && !this.ContactFilter(fixture, fixture2);
						if (!flag7)
						{
							bool flag8 = fixture.BeforeCollision != null && !fixture.BeforeCollision(fixture, fixture2);
							if (!flag8)
							{
								bool flag9 = fixture2.BeforeCollision != null && !fixture2.BeforeCollision(fixture2, fixture);
								if (!flag9)
								{
									Body body3 = null;
									Body item = null;
									bool flag10 = body.SpecialSensor > BodySpecialSensor.None;
									if (flag10)
									{
										body3 = body;
										item = body2;
									}
									else
									{
										bool flag11 = body2.SpecialSensor > BodySpecialSensor.None;
										if (flag11)
										{
											body3 = body2;
											item = body;
										}
									}
									bool flag12 = body3 != null;
									if (flag12)
									{
										bool flag13 = Collision.TestOverlap(body.FixtureList[0].Shape, childIndex, body2.FixtureList[0].Shape, childIndex2, ref body._xf, ref body2._xf);
										if (!flag13)
										{
											return;
										}
										body3._specialSensorResults.Add(item);
										bool flag14 = body3.SpecialSensor == BodySpecialSensor.ActiveOnce;
										if (flag14)
										{
											body3.disabled = true;
											return;
										}
									}
									Contact contact = Contact.Create(fixture, childIndex, fixture2, childIndex2);
									bool flag15 = contact == null;
									if (!flag15)
									{
										fixture = contact.FixtureA;
										fixture2 = contact.FixtureB;
										body = fixture.Body;
										body2 = fixture2.Body;
										this.ContactList.Add(contact);
										contact._nodeA.Contact = contact;
										contact._nodeA.Other = body2;
										contact._nodeA.Prev = null;
										contact._nodeA.Next = body.ContactList;
										bool flag16 = body.ContactList != null;
										if (flag16)
										{
											body.ContactList.Prev = contact._nodeA;
										}
										body.ContactList = contact._nodeA;
										contact._nodeB.Contact = contact;
										contact._nodeB.Other = body;
										contact._nodeB.Prev = null;
										contact._nodeB.Next = body2.ContactList;
										bool flag17 = body2.ContactList != null;
										if (flag17)
										{
											body2.ContactList.Prev = contact._nodeB;
										}
										body2.ContactList = contact._nodeB;
										bool flag18 = !fixture.IsSensor && !fixture2.IsSensor;
										if (flag18)
										{
											body.Awake = true;
											body2.Awake = true;
										}
									}
								}
							}
						}
					}
				}
			}
		}

		internal void FindNewContacts()
		{
			this.BroadPhase.UpdatePairs(this.OnBroadphaseCollision);
		}

		internal void Destroy(Contact contact)
		{
			Fixture fixtureA = contact.FixtureA;
			Fixture fixtureB = contact.FixtureB;
			Body body = fixtureA.Body;
			Body body2 = fixtureB.Body;
			bool isTouching = contact.IsTouching;
			if (isTouching)
			{
				bool flag = fixtureA != null && fixtureA.OnSeparation != null;
				if (flag)
				{
					fixtureA.OnSeparation(fixtureA, fixtureB);
				}
				bool flag2 = fixtureB != null && fixtureB.OnSeparation != null;
				if (flag2)
				{
					fixtureB.OnSeparation(fixtureB, fixtureA);
				}
				bool flag3 = this.EndContact != null;
				if (flag3)
				{
					this.EndContact(contact);
				}
			}
			this.ContactList.Remove(contact);
			bool flag4 = contact._nodeA.Prev != null;
			if (flag4)
			{
				contact._nodeA.Prev.Next = contact._nodeA.Next;
			}
			bool flag5 = contact._nodeA.Next != null;
			if (flag5)
			{
				contact._nodeA.Next.Prev = contact._nodeA.Prev;
			}
			bool flag6 = contact._nodeA == body.ContactList;
			if (flag6)
			{
				body.ContactList = contact._nodeA.Next;
			}
			bool flag7 = contact._nodeB.Prev != null;
			if (flag7)
			{
				contact._nodeB.Prev.Next = contact._nodeB.Next;
			}
			bool flag8 = contact._nodeB.Next != null;
			if (flag8)
			{
				contact._nodeB.Next.Prev = contact._nodeB.Prev;
			}
			bool flag9 = contact._nodeB == body2.ContactList;
			if (flag9)
			{
				body2.ContactList = contact._nodeB.Next;
			}
			contact.Destroy();
		}

		internal void Collide()
		{
			for (int i = 0; i < this.ContactList.Count; i++)
			{
				Contact contact = this.ContactList[i];
				Fixture fixtureA = contact.FixtureA;
				Fixture fixtureB = contact.FixtureB;
				int childIndexA = contact.ChildIndexA;
				int childIndexB = contact.ChildIndexB;
				Body body = fixtureA.Body;
				Body body2 = fixtureB.Body;
				bool flag = !body.Enabled || !body2.Enabled;
				if (!flag)
				{
					bool filterFlag = contact.FilterFlag;
					if (filterFlag)
					{
						bool flag2 = !body2.ShouldCollide(body);
						if (flag2)
						{
							Contact contact2 = contact;
							this.Destroy(contact2);
							goto IL_198;
						}
						bool flag3 = !ContactManager.ShouldCollide(fixtureA, fixtureB);
						if (flag3)
						{
							Contact contact3 = contact;
							this.Destroy(contact3);
							goto IL_198;
						}
						bool flag4 = this.ContactFilter != null && !this.ContactFilter(fixtureA, fixtureB);
						if (flag4)
						{
							Contact contact4 = contact;
							this.Destroy(contact4);
							goto IL_198;
						}
						contact.FilterFlag = false;
					}
					bool flag5 = body.Awake && body.BodyType > BodyType.Static;
					bool flag6 = body2.Awake && body2.BodyType > BodyType.Static;
					bool flag7 = !flag5 && !flag6;
					if (!flag7)
					{
						int proxyId = fixtureA.Proxies[childIndexA].ProxyId;
						int proxyId2 = fixtureB.Proxies[childIndexB].ProxyId;
						bool flag8 = this.BroadPhase.TestOverlap(proxyId, proxyId2);
						bool flag9 = !flag8;
						if (flag9)
						{
							Contact contact5 = contact;
							this.Destroy(contact5);
						}
						else
						{
							contact.Update(this);
						}
					}
				}
				IL_198:;
			}
		}

		public static bool CheckCollisionConditions(Fixture fixtureA, Fixture fixtureB)
		{
			Body body = fixtureA.Body;
			Body body2 = fixtureB.Body;
			bool flag = body.disabled || body2.disabled || !ContactManager.physicsManager.IsCollisionEnabled(body, body2);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool flag2 = body._specialSensor != BodySpecialSensor.None || body2._specialSensor > BodySpecialSensor.None;
				if (flag2)
				{
					result = true;
				}
				else
				{
					bool flag3 = body.IsStatic && body2.IsStatic;
					if (flag3)
					{
						result = false;
					}
					else
					{
						bool flag4 = fixtureA.IsSensor || fixtureB.IsSensor;
						bool flag5 = !flag4;
						if (flag5)
						{
							bool flag6 = (body.IsKinematic && body2.IsKinematic) || (body.IsKinematic && body2.IsStatic) || (body2.IsKinematic && body.IsStatic);
							if (flag6)
							{
								result = false;
								return result;
							}
						}
						result = true;
					}
				}
			}
			return result;
		}

		private static bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
		{
			bool flag = !ContactManager.CheckCollisionConditions(fixtureA, fixtureB);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				bool useFPECollisionCategories = Settings.UseFPECollisionCategories;
				if (useFPECollisionCategories)
				{
					bool flag2 = fixtureA.CollisionGroup == fixtureB.CollisionGroup && fixtureA.CollisionGroup != 0 && fixtureB.CollisionGroup != 0;
					if (flag2)
					{
						result = false;
					}
					else
					{
						bool flag3 = (fixtureA.CollisionCategories & fixtureB.CollidesWith) == Category.None & (fixtureB.CollisionCategories & fixtureA.CollidesWith) == Category.None;
						if (flag3)
						{
							result = false;
						}
						else
						{
							bool flag4 = fixtureA.IsFixtureIgnored(fixtureB) || fixtureB.IsFixtureIgnored(fixtureA);
							result = !flag4;
						}
					}
				}
				else
				{
					bool flag5 = fixtureA.CollisionGroup == fixtureB.CollisionGroup && fixtureA.CollisionGroup != 0;
					if (flag5)
					{
						result = (fixtureA.CollisionGroup > 0);
					}
					else
					{
						bool flag6 = (fixtureA.CollidesWith & fixtureB.CollisionCategories) != Category.None && (fixtureA.CollisionCategories & fixtureB.CollidesWith) > Category.None;
						bool flag7 = flag6;
						if (flag7)
						{
							bool flag8 = fixtureA.IsFixtureIgnored(fixtureB) || fixtureB.IsFixtureIgnored(fixtureA);
							if (flag8)
							{
								result = false;
								return result;
							}
						}
						result = flag6;
					}
				}
			}
			return result;
		}

		internal void UpdateContacts(ContactEdge contactEdge, bool value)
		{
		}
	}
}
