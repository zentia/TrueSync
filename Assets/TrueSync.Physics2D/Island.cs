using System;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class Island
	{
		internal ContactManager _contactManager;

		internal ContactSolver _contactSolver = new ContactSolver();

		internal Contact[] _contacts;

		internal Joint[] _joints;

		private static readonly FP LinTolSqr = Settings.LinearSleepTolerance * Settings.LinearSleepTolerance;

		private static readonly FP AngTolSqr = Settings.AngularSleepTolerance * Settings.AngularSleepTolerance;

		public Body[] Bodies;

		public int BodyCount;

		public int ContactCount;

		public int JointCount;

		public Velocity[] _velocities;

		public Position[] _positions;

		public int BodyCapacity;

		public int ContactCapacity;

		public int JointCapacity;

		public void Reset(int bodyCapacity, int contactCapacity, int jointCapacity, ContactManager contactManager)
		{
			this.BodyCapacity = bodyCapacity;
			this.ContactCapacity = contactCapacity;
			this.JointCapacity = jointCapacity;
			this.BodyCount = 0;
			this.ContactCount = 0;
			this.JointCount = 0;
			this._contactManager = contactManager;
			bool flag = this.Bodies == null || this.Bodies.Length < bodyCapacity;
			if (flag)
			{
				this.Bodies = new Body[bodyCapacity];
				this._velocities = new Velocity[bodyCapacity];
				this._positions = new Position[bodyCapacity];
			}
			bool flag2 = this._contacts == null || this._contacts.Length < contactCapacity;
			if (flag2)
			{
				this._contacts = new Contact[contactCapacity * 2];
			}
			bool flag3 = this._joints == null || this._joints.Length < jointCapacity;
			if (flag3)
			{
				this._joints = new Joint[jointCapacity * 2];
			}
		}

		public void Clear()
		{
			this.BodyCount = 0;
			this.ContactCount = 0;
			this.JointCount = 0;
		}

		public void Solve(ref TimeStep step, ref TSVector2 gravity)
		{
			FP dt = step.dt;
			for (int i = 0; i < this.BodyCount; i++)
			{
				Body body = this.Bodies[i];
				TSVector2 c = body._sweep.C;
				FP a = body._sweep.A;
				TSVector2 tSVector = body._linearVelocity;
				FP fP = body._angularVelocity;
				body._sweep.C0 = body._sweep.C;
				body._sweep.A0 = body._sweep.A;
				bool flag = body.BodyType == BodyType.Dynamic;
				if (flag)
				{
					bool ignoreGravity = body.IgnoreGravity;
					if (ignoreGravity)
					{
						tSVector += dt * (body._invMass * body._force);
					}
					else
					{
						tSVector += dt * (body.GravityScale * gravity + body._invMass * body._force);
					}
					fP += dt * body._invI * body._torque;
					tSVector *= MathUtils.Clamp(1f - dt * body.LinearDamping, 0f, 1f);
					fP *= MathUtils.Clamp(1f - dt * body.AngularDamping, 0f, 1f);
				}
				this._positions[i].c = c;
				this._positions[i].a = a;
				this._velocities[i].v = tSVector;
				this._velocities[i].w = fP;
			}
			SolverData solverData = default(SolverData);
			solverData.step = step;
			solverData.positions = this._positions;
			solverData.velocities = this._velocities;
			this._contactSolver.Reset(step, this.ContactCount, this._contacts, this._positions, this._velocities, true);
			this._contactSolver.InitializeVelocityConstraints();
			this._contactSolver.WarmStart();
			for (int j = 0; j < this.JointCount; j++)
			{
				bool enabled = this._joints[j].Enabled;
				if (enabled)
				{
					this._joints[j].InitVelocityConstraints(ref solverData);
				}
			}
			for (int k = 0; k < Settings.VelocityIterations; k++)
			{
				for (int l = 0; l < this.JointCount; l++)
				{
					Joint joint = this._joints[l];
					bool flag2 = !joint.Enabled;
					if (!flag2)
					{
						joint.SolveVelocityConstraints(ref solverData);
						joint.Validate(step.inv_dt);
					}
				}
				this._contactSolver.SolveVelocityConstraints();
			}
			this._contactSolver.StoreImpulses();
			for (int m = 0; m < this.BodyCount; m++)
			{
				TSVector2 tSVector2 = this._positions[m].c;
				FP fP2 = this._positions[m].a;
				TSVector2 tSVector3 = this._velocities[m].v;
				FP fP3 = this._velocities[m].w;
				TSVector2 tSVector4 = dt * tSVector3;
				bool flag3 = TSVector2.Dot(tSVector4, tSVector4) > Settings.MaxTranslationSquared;
				if (flag3)
				{
					FP scaleFactor = Settings.MaxTranslation / tSVector4.magnitude;
					tSVector3 *= scaleFactor;
				}
				FP fP4 = dt * fP3;
				bool flag4 = fP4 * fP4 > Settings.MaxRotationSquared;
				if (flag4)
				{
					FP y = Settings.MaxRotation / FP.Abs(fP4);
					fP3 *= y;
				}
				tSVector2 += dt * tSVector3;
				fP2 += dt * fP3;
				this._positions[m].c = tSVector2;
				this._positions[m].a = fP2;
				this._velocities[m].v = tSVector3;
				this._velocities[m].w = fP3;
			}
			bool flag5 = false;
			for (int n = 0; n < Settings.PositionIterations; n++)
			{
				bool flag6 = this._contactSolver.SolvePositionConstraints();
				bool flag7 = true;
				for (int num = 0; num < this.JointCount; num++)
				{
					Joint joint2 = this._joints[num];
					bool flag8 = !joint2.Enabled;
					if (!flag8)
					{
						bool flag9 = joint2.SolvePositionConstraints(ref solverData);
						flag7 &= flag9;
					}
				}
				bool flag10 = flag6 & flag7;
				if (flag10)
				{
					flag5 = true;
					break;
				}
			}
			for (int num2 = 0; num2 < this.BodyCount; num2++)
			{
				Body body2 = this.Bodies[num2];
				body2._sweep.C = this._positions[num2].c;
				body2._sweep.A = this._positions[num2].a;
				body2._linearVelocity = this._velocities[num2].v;
				body2._angularVelocity = this._velocities[num2].w;
				body2.SynchronizeTransform();
			}
			this.Report(this._contactSolver._velocityConstraints);
			bool allowSleep = Settings.AllowSleep;
			if (allowSleep)
			{
				FP fP5 = Settings.MaxFP;
				for (int num3 = 0; num3 < this.BodyCount; num3++)
				{
					Body body3 = this.Bodies[num3];
					bool flag11 = body3.BodyType == BodyType.Static;
					if (!flag11)
					{
						bool flag12 = !body3.SleepingAllowed || body3._angularVelocity * body3._angularVelocity > Island.AngTolSqr || TSVector2.Dot(body3._linearVelocity, body3._linearVelocity) > Island.LinTolSqr;
						if (flag12)
						{
							body3._sleepTime = 0f;
							fP5 = 0f;
						}
						else
						{
							body3._sleepTime += dt;
							fP5 = TSMath.Min(fP5, body3._sleepTime);
						}
					}
				}
				bool flag13 = fP5 >= Settings.TimeToSleep & flag5;
				if (flag13)
				{
					for (int num4 = 0; num4 < this.BodyCount; num4++)
					{
						Body body4 = this.Bodies[num4];
						body4.Awake = false;
					}
				}
			}
		}

		internal void SolveTOI(ref TimeStep subStep, int toiIndexA, int toiIndexB, bool warmstarting)
		{
			Debug.Assert(toiIndexA < this.BodyCount);
			Debug.Assert(toiIndexB < this.BodyCount);
			for (int i = 0; i < this.BodyCount; i++)
			{
				Body body = this.Bodies[i];
				this._positions[i].c = body._sweep.C;
				this._positions[i].a = body._sweep.A;
				this._velocities[i].v = body._linearVelocity;
				this._velocities[i].w = body._angularVelocity;
			}
			this._contactSolver.Reset(subStep, this.ContactCount, this._contacts, this._positions, this._velocities, warmstarting);
			for (int j = 0; j < Settings.TOIPositionIterations; j++)
			{
				bool flag = this._contactSolver.SolveTOIPositionConstraints(toiIndexA, toiIndexB);
				bool flag2 = flag;
				if (flag2)
				{
					break;
				}
			}
			this.Bodies[toiIndexA]._sweep.C0 = this._positions[toiIndexA].c;
			this.Bodies[toiIndexA]._sweep.A0 = this._positions[toiIndexA].a;
			this.Bodies[toiIndexB]._sweep.C0 = this._positions[toiIndexB].c;
			this.Bodies[toiIndexB]._sweep.A0 = this._positions[toiIndexB].a;
			this._contactSolver.InitializeVelocityConstraints();
			for (int k = 0; k < Settings.TOIVelocityIterations; k++)
			{
				this._contactSolver.SolveVelocityConstraints();
			}
			FP dt = subStep.dt;
			for (int l = 0; l < this.BodyCount; l++)
			{
				TSVector2 tSVector = this._positions[l].c;
				FP fP = this._positions[l].a;
				TSVector2 tSVector2 = this._velocities[l].v;
				FP fP2 = this._velocities[l].w;
				TSVector2 tSVector3 = dt * tSVector2;
				bool flag3 = TSVector2.Dot(tSVector3, tSVector3) > Settings.MaxTranslationSquared;
				if (flag3)
				{
					FP scaleFactor = Settings.MaxTranslation / tSVector3.magnitude;
					tSVector2 *= scaleFactor;
				}
				FP fP3 = dt * fP2;
				bool flag4 = fP3 * fP3 > Settings.MaxRotationSquared;
				if (flag4)
				{
					FP y = Settings.MaxRotation / FP.Abs(fP3);
					fP2 *= y;
				}
				tSVector += dt * tSVector2;
				fP += dt * fP2;
				this._positions[l].c = tSVector;
				this._positions[l].a = fP;
				this._velocities[l].v = tSVector2;
				this._velocities[l].w = fP2;
				Body body2 = this.Bodies[l];
				body2._sweep.C = tSVector;
				body2._sweep.A = fP;
				body2._linearVelocity = tSVector2;
				body2._angularVelocity = fP2;
				body2.SynchronizeTransform();
			}
			this.Report(this._contactSolver._velocityConstraints);
		}

		public void Add(Body body)
		{
			Debug.Assert(this.BodyCount < this.BodyCapacity);
			body.IslandIndex = this.BodyCount;
			Body[] arg_3A_0 = this.Bodies;
			int bodyCount = this.BodyCount;
			this.BodyCount = bodyCount + 1;
			arg_3A_0[bodyCount] = body;
		}

		public void Add(Contact contact)
		{
			Debug.Assert(this.ContactCount < this.ContactCapacity);
			Contact[] arg_2D_0 = this._contacts;
			int contactCount = this.ContactCount;
			this.ContactCount = contactCount + 1;
			arg_2D_0[contactCount] = contact;
		}

		public void Add(Joint joint)
		{
			Debug.Assert(this.JointCount < this.JointCapacity);
			Joint[] arg_2D_0 = this._joints;
			int jointCount = this.JointCount;
			this.JointCount = jointCount + 1;
			arg_2D_0[jointCount] = joint;
		}

		private void Report(ContactVelocityConstraint[] constraints)
		{
			bool flag = this._contactManager == null;
			if (!flag)
			{
				for (int i = 0; i < this.ContactCount; i++)
				{
					Contact contact = this._contacts[i];
					bool flag2 = contact.FixtureA.AfterCollision != null;
					if (flag2)
					{
						contact.FixtureA.AfterCollision(contact.FixtureA, contact.FixtureB, contact, constraints[i]);
					}
					bool flag3 = contact.FixtureB.AfterCollision != null;
					if (flag3)
					{
						contact.FixtureB.AfterCollision(contact.FixtureB, contact.FixtureA, contact, constraints[i]);
					}
					bool flag4 = this._contactManager.PostSolve != null;
					if (flag4)
					{
						this._contactManager.PostSolve(contact, constraints[i]);
					}
				}
			}
		}
	}
}
