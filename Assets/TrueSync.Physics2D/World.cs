using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class World : IWorld
	{
		internal FP _invDt0;

		internal Body[] _stack = new Body[64];

		internal bool _stepComplete;

		internal List<Body> _bodyAddList = new List<Body>();

		internal List<Body> _bodyRemoveList = new List<Body>();

		internal List<Joint> _jointAddList = new List<Joint>();

		internal List<Joint> _jointRemoveList = new List<Joint>();

		internal Func<Fixture, bool> _queryAABBCallback;

		private Func<int, bool> _queryAABBCallbackWrapper;

		internal TOIInput _input = new TOIInput();

		internal Fixture _myFixture;

		private TSVector2 _point1;

		internal TSVector2 _point2;

		internal List<Fixture> _testPointAllFixtures;

		private Func<Fixture, TSVector2, TSVector2, FP, FP> _rayCastCallback;

		private Func<RayCastInput, int, FP> _rayCastCallbackWrapper;

		internal Queue<Contact> _contactPool = new Queue<Contact>(256);

		internal bool _worldHasNewFixture;

		public BodyDelegate BodyAdded;

		public BodyDelegate BodyRemoved;

		public FixtureDelegate FixtureAdded;

		public FixtureDelegate FixtureRemoved;

		public JointDelegate JointAdded;

		public JointDelegate JointRemoved;

		public ControllerDelegate ControllerAdded;

		public ControllerDelegate ControllerRemoved;

		public TSVector2 Gravity;

		public List<Controller> ControllerList
		{
			get;
			private set;
		}

		public List<BreakableBody> BreakableBodyList
		{
			get;
			private set;
		}

		public int ProxyCount
		{
			get
			{
				return this.ContactManager.BroadPhase.ProxyCount;
			}
		}

		public ContactManager ContactManager
		{
			get;
			private set;
		}

		public List<Body> BodyList
		{
			get;
			private set;
		}

		public List<Joint> JointList
		{
			get;
			private set;
		}

		public List<Contact> ContactList
		{
			get
			{
				return this.ContactManager.ContactList;
			}
		}

		public bool Enabled
		{
			get;
			set;
		}

		public Island Island
		{
			get;
			private set;
		}

		public World(TSVector2 gravity)
		{
			this.Island = new Island();
			this.Enabled = true;
			this.ControllerList = new List<Controller>();
			this.BreakableBodyList = new List<BreakableBody>();
			this.BodyList = new List<Body>(32);
			this.JointList = new List<Joint>(32);
			this._queryAABBCallbackWrapper = new Func<int, bool>(this.QueryAABBCallbackWrapper);
			this._rayCastCallbackWrapper = new Func<RayCastInput, int, FP>(this.RayCastCallbackWrapper);
			this.ContactManager = new ContactManager(new DynamicTreeBroadPhase());
			this.Gravity = gravity;
		}

		private void ProcessRemovedJoints()
		{
			bool flag = this._jointRemoveList.Count > 0;
			if (flag)
			{
				foreach (Joint current in this._jointRemoveList)
				{
					bool collideConnected = current.CollideConnected;
					this.JointList.Remove(current);
					Body bodyA = current.BodyA;
					Body bodyB = current.BodyB;
					bodyA.Awake = true;
					bool flag2 = !current.IsFixedType();
					if (flag2)
					{
						bodyB.Awake = true;
					}
					bool flag3 = current.EdgeA.Prev != null;
					if (flag3)
					{
						current.EdgeA.Prev.Next = current.EdgeA.Next;
					}
					bool flag4 = current.EdgeA.Next != null;
					if (flag4)
					{
						current.EdgeA.Next.Prev = current.EdgeA.Prev;
					}
					bool flag5 = current.EdgeA == bodyA.JointList;
					if (flag5)
					{
						bodyA.JointList = current.EdgeA.Next;
					}
					current.EdgeA.Prev = null;
					current.EdgeA.Next = null;
					bool flag6 = !current.IsFixedType();
					if (flag6)
					{
						bool flag7 = current.EdgeB.Prev != null;
						if (flag7)
						{
							current.EdgeB.Prev.Next = current.EdgeB.Next;
						}
						bool flag8 = current.EdgeB.Next != null;
						if (flag8)
						{
							current.EdgeB.Next.Prev = current.EdgeB.Prev;
						}
						bool flag9 = current.EdgeB == bodyB.JointList;
						if (flag9)
						{
							bodyB.JointList = current.EdgeB.Next;
						}
						current.EdgeB.Prev = null;
						current.EdgeB.Next = null;
					}
					bool flag10 = !current.IsFixedType();
					if (flag10)
					{
						bool flag11 = !collideConnected;
						if (flag11)
						{
							for (ContactEdge contactEdge = bodyB.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
							{
								bool flag12 = contactEdge.Other == bodyA;
								if (flag12)
								{
									contactEdge.Contact.FilterFlag = true;
								}
							}
						}
					}
					bool flag13 = this.JointRemoved != null;
					if (flag13)
					{
						this.JointRemoved(current);
					}
				}
				this._jointRemoveList.Clear();
			}
		}

		private void ProcessAddedJoints()
		{
			bool flag = this._jointAddList.Count > 0;
			if (flag)
			{
				foreach (Joint current in this._jointAddList)
				{
					this.JointList.Add(current);
					current.EdgeA.Joint = current;
					current.EdgeA.Other = current.BodyB;
					current.EdgeA.Prev = null;
					current.EdgeA.Next = current.BodyA.JointList;
					bool flag2 = current.BodyA.JointList != null;
					if (flag2)
					{
						current.BodyA.JointList.Prev = current.EdgeA;
					}
					current.BodyA.JointList = current.EdgeA;
					bool flag3 = !current.IsFixedType();
					if (flag3)
					{
						current.EdgeB.Joint = current;
						current.EdgeB.Other = current.BodyA;
						current.EdgeB.Prev = null;
						current.EdgeB.Next = current.BodyB.JointList;
						bool flag4 = current.BodyB.JointList != null;
						if (flag4)
						{
							current.BodyB.JointList.Prev = current.EdgeB;
						}
						current.BodyB.JointList = current.EdgeB;
						Body bodyA = current.BodyA;
						Body bodyB = current.BodyB;
						bool flag5 = !current.CollideConnected;
						if (flag5)
						{
							for (ContactEdge contactEdge = bodyB.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
							{
								bool flag6 = contactEdge.Other == bodyA;
								if (flag6)
								{
									contactEdge.Contact.FilterFlag = true;
								}
							}
						}
					}
					bool flag7 = this.JointAdded != null;
					if (flag7)
					{
						this.JointAdded(current);
					}
				}
				this._jointAddList.Clear();
			}
		}

		public void ProcessAddedBodies()
		{
			bool flag = this._bodyAddList.Count > 0;
			if (flag)
			{
				foreach (Body current in this._bodyAddList)
				{
					this.BodyList.Add(current);
					bool flag2 = this.BodyAdded != null;
					if (flag2)
					{
						this.BodyAdded(current);
					}
				}
				this._bodyAddList.Clear();
			}
		}

		public void ProcessRemovedBodies()
		{
			bool flag = this._bodyRemoveList.Count > 0;
			if (flag)
			{
				foreach (Body current in this._bodyRemoveList)
				{
					Debug.Assert(this.BodyList.Count > 0);
					bool flag2 = this._bodyAddList.Contains(current);
					bool flag3 = !this.BodyList.Contains(current) && !flag2;
					if (!flag3)
					{
						JointEdge jointEdge = current.JointList;
						while (jointEdge != null)
						{
							JointEdge jointEdge2 = jointEdge;
							jointEdge = jointEdge.Next;
							this.RemoveJoint(jointEdge2.Joint, false);
						}
						current.JointList = null;
						ContactEdge contactEdge = current.ContactList;
						while (contactEdge != null)
						{
							ContactEdge contactEdge2 = contactEdge;
							contactEdge = contactEdge.Next;
							this.ContactManager.Destroy(contactEdge2.Contact);
						}
						current.ContactList = null;
						for (int i = 0; i < current.FixtureList.Count; i++)
						{
							current.FixtureList[i].DestroyProxies(this.ContactManager.BroadPhase);
							current.FixtureList[i].Destroy();
						}
						current.FixtureList = null;
						bool flag4 = flag2;
						if (flag4)
						{
							this._bodyAddList.Remove(current);
						}
						else
						{
							this.BodyList.Remove(current);
						}
						bool flag5 = this.BodyRemoved != null && current._specialSensor == BodySpecialSensor.None;
						if (flag5)
						{
							this.BodyRemoved(current);
						}
					}
				}
				this._bodyRemoveList.Clear();
			}
		}

		private bool QueryAABBCallbackWrapper(int proxyId)
		{
			FixtureProxy proxy = this.ContactManager.BroadPhase.GetProxy(proxyId);
			return this._queryAABBCallback(proxy.Fixture);
		}

		private FP RayCastCallbackWrapper(RayCastInput rayCastInput, int proxyId)
		{
			FixtureProxy proxy = this.ContactManager.BroadPhase.GetProxy(proxyId);
			Fixture fixture = proxy.Fixture;
			int childIndex = proxy.ChildIndex;
			RayCastOutput rayCastOutput;
			bool flag = fixture.RayCast(out rayCastOutput, ref rayCastInput, childIndex);
			bool flag2 = flag;
			FP result;
			if (flag2)
			{
				FP fraction = rayCastOutput.Fraction;
				TSVector2 arg = (1f - fraction) * rayCastInput.Point1 + fraction * rayCastInput.Point2;
				result = this._rayCastCallback(fixture, arg, rayCastOutput.Normal, fraction);
			}
			else
			{
				result = rayCastInput.MaxFraction;
			}
			return result;
		}

		private void Solve(ref TimeStep step)
		{
			this.Island.Reset(this.BodyList.Count, this.ContactManager.ContactList.Count, this.JointList.Count, this.ContactManager);
			foreach (Body current in this.BodyList)
			{
				current._island = false;
			}
			foreach (Contact current2 in this.ContactManager.ContactList)
			{
				current2.IslandFlag = false;
			}
			foreach (Joint current3 in this.JointList)
			{
				current3.IslandFlag = false;
			}
			int count = this.BodyList.Count;
			bool flag = count > this._stack.Length;
			if (flag)
			{
				this._stack = new Body[Math.Max(this._stack.Length * 2, count)];
			}
			for (int i = this.BodyList.Count - 1; i >= 0; i--)
			{
				Body body = this.BodyList[i];
				bool island = body._island;
				if (!island)
				{
					bool flag2 = !body.Awake || !body.Enabled;
					if (!flag2)
					{
						bool flag3 = body.BodyType == BodyType.Static;
						if (!flag3)
						{
							this.Island.Clear();
							int j = 0;
							this._stack[j++] = body;
							body._island = true;
							while (j > 0)
							{
								Body body2 = this._stack[--j];
								Debug.Assert(body2.Enabled);
								this.Island.Add(body2);
								body2.Awake = true;
								bool flag4 = body2.BodyType == BodyType.Static;
								if (!flag4)
								{
									for (ContactEdge contactEdge = body2.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
									{
										Contact contact = contactEdge.Contact;
										bool islandFlag = contact.IslandFlag;
										if (!islandFlag)
										{
											bool flag5 = !contactEdge.Contact.Enabled || !contactEdge.Contact.IsTouching;
											if (!flag5)
											{
												bool isSensor = contact.FixtureA.IsSensor;
												bool isSensor2 = contact.FixtureB.IsSensor;
												bool flag6 = isSensor | isSensor2;
												if (!flag6)
												{
													this.Island.Add(contact);
													contact.IslandFlag = true;
													Body other = contactEdge.Other;
													bool island2 = other._island;
													if (!island2)
													{
														Debug.Assert(j < count);
														this._stack[j++] = other;
														other._island = true;
													}
												}
											}
										}
									}
									for (JointEdge jointEdge = body2.JointList; jointEdge != null; jointEdge = jointEdge.Next)
									{
										bool islandFlag2 = jointEdge.Joint.IslandFlag;
										if (!islandFlag2)
										{
											Body other2 = jointEdge.Other;
											bool flag7 = other2 != null;
											if (flag7)
											{
												bool flag8 = !other2.Enabled;
												if (!flag8)
												{
													this.Island.Add(jointEdge.Joint);
													jointEdge.Joint.IslandFlag = true;
													bool island3 = other2._island;
													if (!island3)
													{
														Debug.Assert(j < count);
														this._stack[j++] = other2;
														other2._island = true;
													}
												}
											}
											else
											{
												this.Island.Add(jointEdge.Joint);
												jointEdge.Joint.IslandFlag = true;
											}
										}
									}
								}
							}
							this.Island.Solve(ref step, ref this.Gravity);
							for (int k = 0; k < this.Island.BodyCount; k++)
							{
								Body body3 = this.Island.Bodies[k];
								bool flag9 = body3.BodyType == BodyType.Static;
								if (flag9)
								{
									body3._island = false;
								}
							}
						}
					}
				}
			}
			foreach (Body current4 in this.BodyList)
			{
				bool flag10 = !current4._island;
				if (!flag10)
				{
					bool flag11 = current4.BodyType == BodyType.Static;
					if (!flag11)
					{
						current4.SynchronizeFixtures();
					}
				}
			}
			this.ContactManager.FindNewContacts();
		}

		private void SolveTOI(ref TimeStep step)
		{
			this.Island.Reset(64, 32, 0, this.ContactManager);
			bool stepComplete = this._stepComplete;
			if (stepComplete)
			{
				for (int i = 0; i < this.BodyList.Count; i++)
				{
					this.BodyList[i]._island = false;
					this.BodyList[i]._sweep.Alpha0 = 0f;
				}
				for (int j = 0; j < this.ContactManager.ContactList.Count; j++)
				{
					Contact contact = this.ContactManager.ContactList[j];
					contact.IslandFlag = false;
					contact.TOIFlag = false;
					contact._toiCount = 0;
					contact._toi = 1f;
				}
			}
			while (true)
			{
				Contact contact2 = null;
				FP fP = 1f;
				for (int k = 0; k < this.ContactManager.ContactList.Count; k++)
				{
					Contact contact3 = this.ContactManager.ContactList[k];
					bool flag = !contact3.Enabled;
					if (!flag)
					{
						bool flag2 = contact3._toiCount > 8;
						if (!flag2)
						{
							bool tOIFlag = contact3.TOIFlag;
							FP fP2;
							if (tOIFlag)
							{
								fP2 = contact3._toi;
							}
							else
							{
								Fixture fixtureA = contact3.FixtureA;
								Fixture fixtureB = contact3.FixtureB;
								bool flag3 = fixtureA.IsSensor || fixtureB.IsSensor;
								if (flag3)
								{
									goto IL_424;
								}
								Body body = fixtureA.Body;
								Body body2 = fixtureB.Body;
								BodyType bodyType = body.BodyType;
								BodyType bodyType2 = body2.BodyType;
								Debug.Assert(bodyType == BodyType.Dynamic || bodyType2 == BodyType.Dynamic);
								bool flag4 = body.Awake && bodyType > BodyType.Static;
								bool flag5 = body2.Awake && bodyType2 > BodyType.Static;
								bool flag6 = !flag4 && !flag5;
								if (flag6)
								{
									goto IL_424;
								}
								bool flag7 = (body.IsBullet || bodyType != BodyType.Dynamic) && (fixtureA.IgnoreCCDWith & fixtureB.CollisionCategories) == Category.None && !body.IgnoreCCD;
								bool flag8 = (body2.IsBullet || bodyType2 != BodyType.Dynamic) && (fixtureB.IgnoreCCDWith & fixtureA.CollisionCategories) == Category.None && !body2.IgnoreCCD;
								bool flag9 = !flag7 && !flag8;
								if (flag9)
								{
									goto IL_424;
								}
								FP alpha = body._sweep.Alpha0;
								bool flag10 = body._sweep.Alpha0 < body2._sweep.Alpha0;
								if (flag10)
								{
									alpha = body2._sweep.Alpha0;
									body._sweep.Advance(alpha);
								}
								else
								{
									bool flag11 = body2._sweep.Alpha0 < body._sweep.Alpha0;
									if (flag11)
									{
										alpha = body._sweep.Alpha0;
										body2._sweep.Advance(alpha);
									}
								}
								Debug.Assert(alpha < 1f);
								this._input.ProxyA.Set(fixtureA.Shape, contact3.ChildIndexA);
								this._input.ProxyB.Set(fixtureB.Shape, contact3.ChildIndexB);
								this._input.SweepA = body._sweep;
								this._input.SweepB = body2._sweep;
								this._input.TMax = 1f;
								TOIOutput tOIOutput;
								TimeOfImpact.CalculateTimeOfImpact(out tOIOutput, this._input);
								FP t = tOIOutput.T;
								bool flag12 = tOIOutput.State == TOIOutputState.Touching;
								if (flag12)
								{
									fP2 = TSMath.Min(alpha + (1f - alpha) * t, 1f);
								}
								else
								{
									fP2 = 1f;
								}
								contact3._toi = fP2;
								contact3.TOIFlag = true;
							}
							bool flag13 = fP2 < fP;
							if (flag13)
							{
								contact2 = contact3;
								fP = fP2;
							}
						}
					}
					IL_424:;
				}
				bool flag14 = contact2 == null || 1f - 10f * Settings.Epsilon < fP;
				if (flag14)
				{
					break;
				}
				Fixture fixtureA2 = contact2.FixtureA;
				Fixture fixtureB2 = contact2.FixtureB;
				Body body3 = fixtureA2.Body;
				Body body4 = fixtureB2.Body;
				Sweep sweep = body3._sweep;
				Sweep sweep2 = body4._sweep;
				body3.Advance(fP);
				body4.Advance(fP);
				contact2.Update(this.ContactManager);
				contact2.TOIFlag = false;
				contact2._toiCount++;
				bool flag15 = !contact2.Enabled || !contact2.IsTouching;
				if (flag15)
				{
					contact2.Enabled = false;
					body3._sweep = sweep;
					body4._sweep = sweep2;
					body3.SynchronizeTransform();
					body4.SynchronizeTransform();
				}
				else
				{
					body3.Awake = true;
					body4.Awake = true;
					this.Island.Clear();
					this.Island.Add(body3);
					this.Island.Add(body4);
					this.Island.Add(contact2);
					body3._island = true;
					body4._island = true;
					contact2.IslandFlag = true;
					Body[] array = new Body[]
					{
						body3,
						body4
					};
					for (int l = 0; l < 2; l++)
					{
						Body body5 = array[l];
						bool flag16 = body5.BodyType == BodyType.Dynamic;
						if (flag16)
						{
							for (ContactEdge contactEdge = body5.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
							{
								Contact contact4 = contactEdge.Contact;
								bool flag17 = this.Island.BodyCount == this.Island.BodyCapacity;
								if (flag17)
								{
									break;
								}
								bool flag18 = this.Island.ContactCount == this.Island.ContactCapacity;
								if (flag18)
								{
									break;
								}
								bool islandFlag = contact4.IslandFlag;
								if (!islandFlag)
								{
									Body other = contactEdge.Other;
									bool flag19 = other.BodyType == BodyType.Dynamic && !body5.IsBullet && !other.IsBullet;
									if (!flag19)
									{
										bool flag20 = contact4.FixtureA.IsSensor || contact4.FixtureB.IsSensor;
										if (!flag20)
										{
											Sweep sweep3 = other._sweep;
											bool flag21 = !other._island;
											if (flag21)
											{
												other.Advance(fP);
											}
											contact4.Update(this.ContactManager);
											bool flag22 = !contact4.Enabled;
											if (flag22)
											{
												other._sweep = sweep3;
												other.SynchronizeTransform();
											}
											else
											{
												bool flag23 = !contact4.IsTouching;
												if (flag23)
												{
													other._sweep = sweep3;
													other.SynchronizeTransform();
												}
												else
												{
													contact4.IslandFlag = true;
													this.Island.Add(contact4);
													bool island = other._island;
													if (!island)
													{
														other._island = true;
														bool flag24 = other.BodyType > BodyType.Static;
														if (flag24)
														{
															other.Awake = true;
														}
														this.Island.Add(other);
													}
												}
											}
										}
									}
								}
							}
						}
					}
					TimeStep timeStep;
					timeStep.dt = (1f - fP) * step.dt;
					timeStep.inv_dt = 1f / timeStep.dt;
					timeStep.dtRatio = 1f;
					this.Island.SolveTOI(ref timeStep, body3.IslandIndex, body4.IslandIndex, false);
					for (int m = 0; m < this.Island.BodyCount; m++)
					{
						Body body6 = this.Island.Bodies[m];
						body6._island = false;
						bool flag25 = body6.BodyType != BodyType.Dynamic;
						if (!flag25)
						{
							body6.SynchronizeFixtures();
							for (ContactEdge contactEdge2 = body6.ContactList; contactEdge2 != null; contactEdge2 = contactEdge2.Next)
							{
								contactEdge2.Contact.TOIFlag = false;
								contactEdge2.Contact.IslandFlag = false;
							}
						}
					}
					this.ContactManager.FindNewContacts();
				}
			}
			this._stepComplete = true;
		}

		internal void AddBody(Body body)
		{
			Debug.Assert(!this._bodyAddList.Contains(body), "You are adding the same body more than once.");
			bool flag = !this._bodyAddList.Contains(body);
			if (flag)
			{
				this._bodyAddList.Add(body);
			}
		}

		public void RemoveBody(Body body)
		{
			Debug.Assert(!this._bodyRemoveList.Contains(body), "The body is already marked for removal. You are removing the body more than once.");
			bool flag = !this._bodyRemoveList.Contains(body);
			if (flag)
			{
				this._bodyRemoveList.Add(body);
			}
		}

		public void AddJoint(Joint joint)
		{
			Debug.Assert(!this._jointAddList.Contains(joint), "You are adding the same joint more than once.");
			bool flag = !this._jointAddList.Contains(joint);
			if (flag)
			{
				this._jointAddList.Add(joint);
			}
		}

		private void RemoveJoint(Joint joint, bool doCheck)
		{
			if (doCheck)
			{
				Debug.Assert(!this._jointRemoveList.Contains(joint), "The joint is already marked for removal. You are removing the joint more than once.");
			}
			bool flag = !this._jointRemoveList.Contains(joint);
			if (flag)
			{
				this._jointRemoveList.Add(joint);
			}
		}

		public void RemoveJoint(Joint joint)
		{
			this.RemoveJoint(joint, true);
		}

		public void ProcessChanges()
		{
			this.ProcessAddedBodies();
			this.ProcessAddedJoints();
			this.ProcessRemovedBodies();
			this.ProcessRemovedJoints();
		}

		public void Step(FP dt)
		{
			bool flag = !this.Enabled;
			if (!flag)
			{
				this.ProcessChanges();
				bool worldHasNewFixture = this._worldHasNewFixture;
				if (worldHasNewFixture)
				{
					this.ContactManager.FindNewContacts();
					this._worldHasNewFixture = false;
				}
				TimeStep timeStep;
				timeStep.inv_dt = ((dt > 0f) ? (1f / dt) : 0f);
				timeStep.dt = dt;
				timeStep.dtRatio = this._invDt0 * dt;
				for (int i = 0; i < this.ControllerList.Count; i++)
				{
					this.ControllerList[i].Update(dt);
				}
				this.ContactManager.Collide();
				this.Solve(ref timeStep);
				bool continuousPhysics = Settings.ContinuousPhysics;
				if (continuousPhysics)
				{
					this.SolveTOI(ref timeStep);
				}
				this.ClearForces();
				for (int j = 0; j < this.BreakableBodyList.Count; j++)
				{
					this.BreakableBodyList[j].Update();
				}
				for (int k = 0; k < this.BodyList.Count; k++)
				{
					List<IBodyConstraint> bodyConstraints = this.BodyList[k].bodyConstraints;
					for (int l = 0; l < bodyConstraints.Count; l++)
					{
						bodyConstraints[l].PostStep();
					}
				}
				this._invDt0 = timeStep.inv_dt;
			}
		}

		public void ClearForces()
		{
			for (int i = 0; i < this.BodyList.Count; i++)
			{
				Body body = this.BodyList[i];
				body._force = TSVector2.zero;
				body._torque = 0f;
			}
		}

		public void QueryAABB(Func<Fixture, bool> callback, ref AABB aabb)
		{
			this._queryAABBCallback = callback;
			this.ContactManager.BroadPhase.Query(this._queryAABBCallbackWrapper, ref aabb);
			this._queryAABBCallback = null;
		}

		public List<Fixture> QueryAABB(ref AABB aabb)
		{
			List<Fixture> affected = new List<Fixture>();
			this.QueryAABB(delegate(Fixture fixture)
			{
				affected.Add(fixture);
				return true;
			}, ref aabb);
			return affected;
		}

		public void RayCast(Func<Fixture, TSVector2, TSVector2, FP, FP> callback, TSVector2 point1, TSVector2 point2)
		{
			RayCastInput rayCastInput = default(RayCastInput);
			rayCastInput.MaxFraction = 1f;
			rayCastInput.Point1 = point1;
			rayCastInput.Point2 = point2;
			this._rayCastCallback = callback;
			this.ContactManager.BroadPhase.RayCast(this._rayCastCallbackWrapper, ref rayCastInput);
			this._rayCastCallback = null;
		}

		public List<Fixture> RayCast(TSVector2 point1, TSVector2 point2)
		{
			List<Fixture> affected = new List<Fixture>();
			this.RayCast(delegate(Fixture f, TSVector2 p, TSVector2 n, FP fr)
			{
				affected.Add(f);
				return 1;
			}, point1, point2);
			return affected;
		}

		public void AddController(Controller controller)
		{
			Debug.Assert(!this.ControllerList.Contains(controller), "You are adding the same controller more than once.");
			controller.World = this;
			this.ControllerList.Add(controller);
			bool flag = this.ControllerAdded != null;
			if (flag)
			{
				this.ControllerAdded(controller);
			}
		}

		public void RemoveController(Controller controller)
		{
			Debug.Assert(this.ControllerList.Contains(controller), "You are removing a controller that is not in the simulation.");
			bool flag = this.ControllerList.Contains(controller);
			if (flag)
			{
				this.ControllerList.Remove(controller);
				bool flag2 = this.ControllerRemoved != null;
				if (flag2)
				{
					this.ControllerRemoved(controller);
				}
			}
		}

		public void AddBreakableBody(BreakableBody breakableBody)
		{
			this.BreakableBodyList.Add(breakableBody);
		}

		public void RemoveBreakableBody(BreakableBody breakableBody)
		{
			Debug.Assert(this.BreakableBodyList.Contains(breakableBody));
			this.BreakableBodyList.Remove(breakableBody);
		}

		public Fixture TestPoint(TSVector2 point)
		{
			TSVector2 value = new TSVector2(Settings.Epsilon, Settings.Epsilon);
			AABB aABB;
			aABB.LowerBound = point - value;
			aABB.UpperBound = point + value;
			this._myFixture = null;
			this._point1 = point;
			this.QueryAABB(new Func<Fixture, bool>(this.TestPointCallback), ref aABB);
			return this._myFixture;
		}

		private bool TestPointCallback(Fixture fixture)
		{
			bool flag = fixture.TestPoint(ref this._point1);
			bool flag2 = flag;
			bool result;
			if (flag2)
			{
				this._myFixture = fixture;
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		public List<Fixture> TestPointAll(TSVector2 point)
		{
			TSVector2 value = new TSVector2(Settings.Epsilon, Settings.Epsilon);
			AABB aABB;
			aABB.LowerBound = point - value;
			aABB.UpperBound = point + value;
			this._point2 = point;
			this._testPointAllFixtures = new List<Fixture>();
			this.QueryAABB(new Func<Fixture, bool>(this.TestPointAllCallback), ref aABB);
			return this._testPointAllFixtures;
		}

		private bool TestPointAllCallback(Fixture fixture)
		{
			bool flag = fixture.TestPoint(ref this._point2);
			bool flag2 = flag;
			if (flag2)
			{
				this._testPointAllFixtures.Add(fixture);
			}
			return true;
		}

		public void ShiftOrigin(TSVector2 newOrigin)
		{
			foreach (Body current in this.BodyList)
			{
				Body expr_24_cp_0_cp_0 = current;
				expr_24_cp_0_cp_0._xf.p = expr_24_cp_0_cp_0._xf.p - newOrigin;
				Body expr_40_cp_0_cp_0 = current;
				expr_40_cp_0_cp_0._sweep.C0 = expr_40_cp_0_cp_0._sweep.C0 - newOrigin;
				Body expr_5C_cp_0_cp_0 = current;
				expr_5C_cp_0_cp_0._sweep.C = expr_5C_cp_0_cp_0._sweep.C - newOrigin;
			}
			this.ContactManager.BroadPhase.ShiftOrigin(newOrigin);
		}

		public void Clear()
		{
			this.ProcessChanges();
			for (int i = this.BodyList.Count - 1; i >= 0; i--)
			{
				this.RemoveBody(this.BodyList[i]);
			}
			for (int j = this.ControllerList.Count - 1; j >= 0; j--)
			{
				this.RemoveController(this.ControllerList[j]);
			}
			for (int k = this.BreakableBodyList.Count - 1; k >= 0; k--)
			{
				this.RemoveBreakableBody(this.BreakableBodyList[k]);
			}
			this.ProcessChanges();
		}

		public List<IBody> Bodies()
		{
			List<IBody> list = new List<IBody>();
			for (int i = 0; i < this.BodyList.Count; i++)
			{
				list.Add(this.BodyList[i]);
			}
			return list;
		}
	}
}
