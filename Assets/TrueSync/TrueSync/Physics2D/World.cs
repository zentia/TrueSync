using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
    public class World : IWorld
    {
        internal Body[] _stack = new Body[64];
        internal List<Body> _bodyAddList = new List<Body>();
        internal List<Body> _bodyRemoveList = new List<Body>();
        internal List<Joint> _jointAddList = new List<Joint>();
        internal List<Joint> _jointRemoveList = new List<Joint>();
        internal TOIInput _input = new TOIInput();
        internal Queue<Contact> _contactPool = new Queue<Contact>(256);
        internal FP _invDt0;
        internal bool _stepComplete;
        internal Func<Fixture, bool> _queryAABBCallback;
        private Func<int, bool> _queryAABBCallbackWrapper;
        internal Fixture _myFixture;
        private TSVector2 _point1;
        internal TSVector2 _point2;
        internal List<Fixture> _testPointAllFixtures;
        private Func<Fixture, TSVector2, TSVector2, FP, FP> _rayCastCallback;
        private Func<RayCastInput, int, FP> _rayCastCallbackWrapper;
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
            this.ContactManager = new ContactManager((IBroadPhase)new DynamicTreeBroadPhase());
            this.Gravity = gravity;
        }

        private void ProcessRemovedJoints()
        {
            if (this._jointRemoveList.Count <= 0)
                return;
            foreach (Joint jointRemove in this._jointRemoveList)
            {
                bool collideConnected = jointRemove.CollideConnected;
                this.JointList.Remove(jointRemove);
                Body bodyA = jointRemove.BodyA;
                Body bodyB = jointRemove.BodyB;
                bodyA.Awake = true;
                if (!jointRemove.IsFixedType())
                    bodyB.Awake = true;
                if (jointRemove.EdgeA.Prev != null)
                    jointRemove.EdgeA.Prev.Next = jointRemove.EdgeA.Next;
                if (jointRemove.EdgeA.Next != null)
                    jointRemove.EdgeA.Next.Prev = jointRemove.EdgeA.Prev;
                if (jointRemove.EdgeA == bodyA.JointList)
                    bodyA.JointList = jointRemove.EdgeA.Next;
                jointRemove.EdgeA.Prev = (JointEdge)null;
                jointRemove.EdgeA.Next = (JointEdge)null;
                if (!jointRemove.IsFixedType())
                {
                    if (jointRemove.EdgeB.Prev != null)
                        jointRemove.EdgeB.Prev.Next = jointRemove.EdgeB.Next;
                    if (jointRemove.EdgeB.Next != null)
                        jointRemove.EdgeB.Next.Prev = jointRemove.EdgeB.Prev;
                    if (jointRemove.EdgeB == bodyB.JointList)
                        bodyB.JointList = jointRemove.EdgeB.Next;
                    jointRemove.EdgeB.Prev = (JointEdge)null;
                    jointRemove.EdgeB.Next = (JointEdge)null;
                }
                if (!jointRemove.IsFixedType() && !collideConnected)
                {
                    for (ContactEdge contactEdge = bodyB.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
                    {
                        if (contactEdge.Other == bodyA)
                            contactEdge.Contact.FilterFlag = true;
                    }
                }
                if (this.JointRemoved != null)
                    this.JointRemoved(jointRemove);
            }
            this._jointRemoveList.Clear();
        }

        private void ProcessAddedJoints()
        {
            if (this._jointAddList.Count <= 0)
                return;
            foreach (Joint jointAdd in this._jointAddList)
            {
                this.JointList.Add(jointAdd);
                jointAdd.EdgeA.Joint = jointAdd;
                jointAdd.EdgeA.Other = jointAdd.BodyB;
                jointAdd.EdgeA.Prev = (JointEdge)null;
                jointAdd.EdgeA.Next = jointAdd.BodyA.JointList;
                if (jointAdd.BodyA.JointList != null)
                    jointAdd.BodyA.JointList.Prev = jointAdd.EdgeA;
                jointAdd.BodyA.JointList = jointAdd.EdgeA;
                if (!jointAdd.IsFixedType())
                {
                    jointAdd.EdgeB.Joint = jointAdd;
                    jointAdd.EdgeB.Other = jointAdd.BodyA;
                    jointAdd.EdgeB.Prev = (JointEdge)null;
                    jointAdd.EdgeB.Next = jointAdd.BodyB.JointList;
                    if (jointAdd.BodyB.JointList != null)
                        jointAdd.BodyB.JointList.Prev = jointAdd.EdgeB;
                    jointAdd.BodyB.JointList = jointAdd.EdgeB;
                    Body bodyA = jointAdd.BodyA;
                    Body bodyB = jointAdd.BodyB;
                    if (!jointAdd.CollideConnected)
                    {
                        for (ContactEdge contactEdge = bodyB.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
                        {
                            if (contactEdge.Other == bodyA)
                                contactEdge.Contact.FilterFlag = true;
                        }
                    }
                }
                if (this.JointAdded != null)
                    this.JointAdded(jointAdd);
            }
            this._jointAddList.Clear();
        }

        public void ProcessAddedBodies()
        {
            if (this._bodyAddList.Count <= 0)
                return;
            foreach (Body bodyAdd in this._bodyAddList)
            {
                this.BodyList.Add(bodyAdd);
                if (this.BodyAdded != null)
                    this.BodyAdded(bodyAdd);
            }
            this._bodyAddList.Clear();
        }

        public void ProcessRemovedBodies()
        {
            if (this._bodyRemoveList.Count <= 0)
                return;
            foreach (Body bodyRemove in this._bodyRemoveList)
            {
                Debug.Assert(this.BodyList.Count > 0);
                bool flag = this._bodyAddList.Contains(bodyRemove);
                if (this.BodyList.Contains(bodyRemove) || flag)
                {
                    JointEdge jointEdge1 = bodyRemove.JointList;
                    while (jointEdge1 != null)
                    {
                        JointEdge jointEdge2 = jointEdge1;
                        jointEdge1 = jointEdge1.Next;
                        this.RemoveJoint(jointEdge2.Joint, false);
                    }
                    bodyRemove.JointList = (JointEdge)null;
                    ContactEdge contactEdge1 = bodyRemove.ContactList;
                    while (contactEdge1 != null)
                    {
                        ContactEdge contactEdge2 = contactEdge1;
                        contactEdge1 = contactEdge1.Next;
                        this.ContactManager.Destroy(contactEdge2.Contact);
                    }
                    bodyRemove.ContactList = (ContactEdge)null;
                    for (int index = 0; index < bodyRemove.FixtureList.Count; ++index)
                    {
                        bodyRemove.FixtureList[index].DestroyProxies(this.ContactManager.BroadPhase);
                        bodyRemove.FixtureList[index].Destroy();
                    }
                    bodyRemove.FixtureList = (List<Fixture>)null;
                    if (flag)
                        this._bodyAddList.Remove(bodyRemove);
                    else
                        this.BodyList.Remove(bodyRemove);
                    if (this.BodyRemoved != null && bodyRemove._specialSensor == BodySpecialSensor.None)
                        this.BodyRemoved(bodyRemove);
                }
            }
            this._bodyRemoveList.Clear();
        }

        private bool QueryAABBCallbackWrapper(int proxyId)
        {
            return this._queryAABBCallback(this.ContactManager.BroadPhase.GetProxy(proxyId).Fixture);
        }

        private FP RayCastCallbackWrapper(RayCastInput rayCastInput, int proxyId)
        {
            FixtureProxy proxy = this.ContactManager.BroadPhase.GetProxy(proxyId);
            Fixture fixture = proxy.Fixture;
            int childIndex = proxy.ChildIndex;
            RayCastOutput output;
            if (!fixture.RayCast(out output, ref rayCastInput, childIndex))
                return rayCastInput.MaxFraction;
            FP fraction = output.Fraction;
            TSVector2 tsVector2 = ((FP)1f - fraction) * rayCastInput.Point1 + fraction * rayCastInput.Point2;
            return this._rayCastCallback(fixture, tsVector2, output.Normal, fraction);
        }

        private void Solve(ref TimeStep step)
        {
            this.Island.Reset(this.BodyList.Count, this.ContactManager.ContactList.Count, this.JointList.Count, this.ContactManager);
            foreach (Body body in this.BodyList)
                body._island = false;
            foreach (Contact contact in this.ContactManager.ContactList)
                contact.IslandFlag = false;
            foreach (Joint joint in this.JointList)
                joint.IslandFlag = false;
            int count = this.BodyList.Count;
            if (count > this._stack.Length)
                this._stack = new Body[Math.Max(this._stack.Length * 2, count)];
            for (int index1 = this.BodyList.Count - 1; index1 >= 0; --index1)
            {
                Body body1 = this.BodyList[index1];
                if (!body1._island && (body1.Awake && body1.Enabled && body1.BodyType != BodyType.Static))
                {
                    this.Island.Clear();
                    int num1 = 0;
                    Body[] stack = this._stack;
                    int index2 = num1;
                    int num2 = 1;
                    int num3 = index2 + num2;
                    Body body2 = body1;
                    stack[index2] = body2;
                    body1._island = true;
                    while (num3 > 0)
                    {
                        Body body3 = this._stack[--num3];
                        Debug.Assert(body3.Enabled);
                        this.Island.Add(body3);
                        body3.Awake = true;
                        if (body3.BodyType != BodyType.Static)
                        {
                            for (ContactEdge contactEdge = body3.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
                            {
                                Contact contact = contactEdge.Contact;
                                if (!contact.IslandFlag && (contactEdge.Contact.Enabled && contactEdge.Contact.IsTouching && !(contact.FixtureA.IsSensor | contact.FixtureB.IsSensor)))
                                {
                                    this.Island.Add(contact);
                                    contact.IslandFlag = true;
                                    Body other = contactEdge.Other;
                                    if (!other._island)
                                    {
                                        Debug.Assert(num3 < count);
                                        this._stack[num3++] = other;
                                        other._island = true;
                                    }
                                }
                            }
                            for (JointEdge jointEdge = body3.JointList; jointEdge != null; jointEdge = jointEdge.Next)
                            {
                                if (!jointEdge.Joint.IslandFlag)
                                {
                                    Body other = jointEdge.Other;
                                    if (other != null)
                                    {
                                        if (other.Enabled)
                                        {
                                            this.Island.Add(jointEdge.Joint);
                                            jointEdge.Joint.IslandFlag = true;
                                            if (!other._island)
                                            {
                                                Debug.Assert(num3 < count);
                                                this._stack[num3++] = other;
                                                other._island = true;
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
                    for (int index3 = 0; index3 < this.Island.BodyCount; ++index3)
                    {
                        Body body3 = this.Island.Bodies[index3];
                        if (body3.BodyType == BodyType.Static)
                            body3._island = false;
                    }
                }
            }
            foreach (Body body in this.BodyList)
            {
                if (body._island && body.BodyType != BodyType.Static)
                    body.SynchronizeFixtures();
            }
            this.ContactManager.FindNewContacts();
        }

        private void SolveTOI(ref TimeStep step)
        {
            this.Island.Reset(64, 32, 0, this.ContactManager);
            if (this._stepComplete)
            {
                for (int index = 0; index < this.BodyList.Count; ++index)
                {
                    this.BodyList[index]._island = false;
                    this.BodyList[index]._sweep.Alpha0 = (FP)0.0f;
                }
                for (int index = 0; index < this.ContactManager.ContactList.Count; ++index)
                {
                    Contact contact = this.ContactManager.ContactList[index];
                    contact.IslandFlag = false;
                    contact.TOIFlag = false;
                    contact._toiCount = 0;
                    contact._toi = (FP)1f;
                }
            }
            while (true)
            {
                Contact contact1 = (Contact)null;
                FP alpha = (FP)1f;
                for (int index = 0; index < this.ContactManager.ContactList.Count; ++index)
                {
                    Contact contact2 = this.ContactManager.ContactList[index];
                    if (contact2.Enabled && contact2._toiCount <= 8)
                    {
                        FP fp;
                        if (contact2.TOIFlag)
                        {
                            fp = contact2._toi;
                        }
                        else
                        {
                            Fixture fixtureA = contact2.FixtureA;
                            Fixture fixtureB = contact2.FixtureB;
                            if (!fixtureA.IsSensor && !fixtureB.IsSensor)
                            {
                                Body body1 = fixtureA.Body;
                                Body body2 = fixtureB.Body;
                                BodyType bodyType1 = body1.BodyType;
                                BodyType bodyType2 = body2.BodyType;
                                Debug.Assert(bodyType1 == BodyType.Dynamic || bodyType2 == BodyType.Dynamic);
                                if ((body1.Awake && (uint)bodyType1 > 0U || body2.Awake && (uint)bodyType2 > 0U) && ((body1.IsBullet || bodyType1 != BodyType.Dynamic) && (fixtureA.IgnoreCCDWith & fixtureB.CollisionCategories) == Category.None && !body1.IgnoreCCD || (body2.IsBullet || bodyType2 != BodyType.Dynamic) && (fixtureB.IgnoreCCDWith & fixtureA.CollisionCategories) == Category.None && !body2.IgnoreCCD))
                                {
                                    FP alpha0 = body1._sweep.Alpha0;
                                    if (body1._sweep.Alpha0 < body2._sweep.Alpha0)
                                    {
                                        alpha0 = body2._sweep.Alpha0;
                                        body1._sweep.Advance(alpha0);
                                    }
                                    else if (body2._sweep.Alpha0 < body1._sweep.Alpha0)
                                    {
                                        alpha0 = body1._sweep.Alpha0;
                                        body2._sweep.Advance(alpha0);
                                    }
                                    Debug.Assert(alpha0 < (FP)1f);
                                    this._input.ProxyA.Set(fixtureA.Shape, contact2.ChildIndexA);
                                    this._input.ProxyB.Set(fixtureB.Shape, contact2.ChildIndexB);
                                    this._input.SweepA = body1._sweep;
                                    this._input.SweepB = body2._sweep;
                                    this._input.TMax = (FP)1f;
                                    TOIOutput output;
                                    TimeOfImpact.CalculateTimeOfImpact(out output, this._input);
                                    FP t = output.T;
                                    fp = output.State != TOIOutputState.Touching ? (FP)1f : TSMath.Min(alpha0 + ((FP)1f - alpha0) * t, (FP)1f);
                                    contact2._toi = fp;
                                    contact2.TOIFlag = true;
                                }
                                else
                                    continue;
                            }
                            else
                                continue;
                        }
                        if (fp < alpha)
                        {
                            contact1 = contact2;
                            alpha = fp;
                        }
                    }
                }
                if (contact1 != null && !((FP)1f - (FP)10f * Settings.Epsilon < alpha))
                {
                    Fixture fixtureA = contact1.FixtureA;
                    Fixture fixtureB = contact1.FixtureB;
                    Body body1 = fixtureA.Body;
                    Body body2 = fixtureB.Body;
                    Sweep sweep1 = body1._sweep;
                    Sweep sweep2 = body2._sweep;
                    body1.Advance(alpha);
                    body2.Advance(alpha);
                    contact1.Update(this.ContactManager);
                    contact1.TOIFlag = false;
                    ++contact1._toiCount;
                    if (!contact1.Enabled || !contact1.IsTouching)
                    {
                        contact1.Enabled = false;
                        body1._sweep = sweep1;
                        body2._sweep = sweep2;
                        body1.SynchronizeTransform();
                        body2.SynchronizeTransform();
                    }
                    else
                    {
                        body1.Awake = true;
                        body2.Awake = true;
                        this.Island.Clear();
                        this.Island.Add(body1);
                        this.Island.Add(body2);
                        this.Island.Add(contact1);
                        body1._island = true;
                        body2._island = true;
                        contact1.IslandFlag = true;
                        Body[] bodyArray = new Body[2]
                        {
              body1,
              body2
                        };
                        for (int index = 0; index < 2; ++index)
                        {
                            Body body3 = bodyArray[index];
                            if (body3.BodyType == BodyType.Dynamic)
                            {
                                for (ContactEdge contactEdge = body3.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
                                {
                                    Contact contact2 = contactEdge.Contact;
                                    if (this.Island.BodyCount != this.Island.BodyCapacity && this.Island.ContactCount != this.Island.ContactCapacity)
                                    {
                                        if (!contact2.IslandFlag)
                                        {
                                            Body other = contactEdge.Other;
                                            if ((other.BodyType != BodyType.Dynamic || body3.IsBullet || other.IsBullet) && (!contact2.FixtureA.IsSensor && !contact2.FixtureB.IsSensor))
                                            {
                                                Sweep sweep3 = other._sweep;
                                                if (!other._island)
                                                    other.Advance(alpha);
                                                contact2.Update(this.ContactManager);
                                                if (!contact2.Enabled)
                                                {
                                                    other._sweep = sweep3;
                                                    other.SynchronizeTransform();
                                                }
                                                else if (!contact2.IsTouching)
                                                {
                                                    other._sweep = sweep3;
                                                    other.SynchronizeTransform();
                                                }
                                                else
                                                {
                                                    contact2.IslandFlag = true;
                                                    this.Island.Add(contact2);
                                                    if (!other._island)
                                                    {
                                                        other._island = true;
                                                        if ((uint)other.BodyType > 0U)
                                                            other.Awake = true;
                                                        this.Island.Add(other);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                        TimeStep subStep;
                        subStep.dt = ((FP)1f - alpha) * step.dt;
                        subStep.inv_dt = (FP)1f / subStep.dt;
                        subStep.dtRatio = (FP)1f;
                        this.Island.SolveTOI(ref subStep, body1.IslandIndex, body2.IslandIndex, false);
                        for (int index = 0; index < this.Island.BodyCount; ++index)
                        {
                            Body body3 = this.Island.Bodies[index];
                            body3._island = false;
                            if (body3.BodyType == BodyType.Dynamic)
                            {
                                body3.SynchronizeFixtures();
                                for (ContactEdge contactEdge = body3.ContactList; contactEdge != null; contactEdge = contactEdge.Next)
                                {
                                    contactEdge.Contact.TOIFlag = false;
                                    contactEdge.Contact.IslandFlag = false;
                                }
                            }
                        }
                        this.ContactManager.FindNewContacts();
                    }
                }
                else
                    break;
            }
            this._stepComplete = true;
        }

        public List<Controller> ControllerList { get; private set; }

        public List<BreakableBody> BreakableBodyList { get; private set; }

        public int ProxyCount
        {
            get
            {
                return this.ContactManager.BroadPhase.ProxyCount;
            }
        }

        public ContactManager ContactManager { get; private set; }

        public List<Body> BodyList { get; private set; }

        public List<Joint> JointList { get; private set; }

        public List<Contact> ContactList
        {
            get
            {
                return this.ContactManager.ContactList;
            }
        }

        public bool Enabled { get; set; }

        public Island Island { get; private set; }

        internal void AddBody(Body body)
        {
            Debug.Assert(!this._bodyAddList.Contains(body), "You are adding the same body more than once.");
            if (this._bodyAddList.Contains(body))
                return;
            this._bodyAddList.Add(body);
        }

        public void RemoveBody(Body body)
        {
            Debug.Assert(!this._bodyRemoveList.Contains(body), "The body is already marked for removal. You are removing the body more than once.");
            if (this._bodyRemoveList.Contains(body))
                return;
            this._bodyRemoveList.Add(body);
        }

        public void AddJoint(Joint joint)
        {
            Debug.Assert(!this._jointAddList.Contains(joint), "You are adding the same joint more than once.");
            if (this._jointAddList.Contains(joint))
                return;
            this._jointAddList.Add(joint);
        }

        private void RemoveJoint(Joint joint, bool doCheck)
        {
            if (doCheck)
                Debug.Assert(!this._jointRemoveList.Contains(joint), "The joint is already marked for removal. You are removing the joint more than once.");
            if (this._jointRemoveList.Contains(joint))
                return;
            this._jointRemoveList.Add(joint);
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
            if (!this.Enabled)
                return;
            this.ProcessChanges();
            if (this._worldHasNewFixture)
            {
                this.ContactManager.FindNewContacts();
                this._worldHasNewFixture = false;
            }
            TimeStep step;
            step.inv_dt = dt > (FP)0.0f ? (FP)1f / dt : (FP)0.0f;
            step.dt = dt;
            step.dtRatio = this._invDt0 * dt;
            for (int index = 0; index < this.ControllerList.Count; ++index)
                this.ControllerList[index].Update(dt);
            this.ContactManager.Collide();
            this.Solve(ref step);
            if (Settings.ContinuousPhysics)
                this.SolveTOI(ref step);
            this.ClearForces();
            for (int index = 0; index < this.BreakableBodyList.Count; ++index)
                this.BreakableBodyList[index].Update();
            for (int index1 = 0; index1 < this.BodyList.Count; ++index1)
            {
                List<IBodyConstraint> bodyConstraints = this.BodyList[index1].bodyConstraints;
                for (int index2 = 0; index2 < bodyConstraints.Count; ++index2)
                    bodyConstraints[index2].PostStep();
            }
            this._invDt0 = step.inv_dt;
        }

        public void ClearForces()
        {
            for (int index = 0; index < this.BodyList.Count; ++index)
            {
                Body body = this.BodyList[index];
                body._force = TSVector2.zero;
                body._torque = (FP)0.0f;
            }
        }

        public void QueryAABB(Func<Fixture, bool> callback, ref AABB aabb)
        {
            this._queryAABBCallback = callback;
            this.ContactManager.BroadPhase.Query(this._queryAABBCallbackWrapper, ref aabb);
            this._queryAABBCallback = (Func<Fixture, bool>)null;
        }

        public List<Fixture> QueryAABB(ref AABB aabb)
        {
            List<Fixture> affected = new List<Fixture>();
            this.QueryAABB((Func<Fixture, bool>)(fixture =>
            {
                affected.Add(fixture);
                return true;
            }), ref aabb);
            return affected;
        }

        public void RayCast(Func<Fixture, TSVector2, TSVector2, FP, FP> callback, TSVector2 point1, TSVector2 point2)
        {
            RayCastInput input = new RayCastInput();
            input.MaxFraction = (FP)1f;
            input.Point1 = point1;
            input.Point2 = point2;
            this._rayCastCallback = callback;
            this.ContactManager.BroadPhase.RayCast(this._rayCastCallbackWrapper, ref input);
            this._rayCastCallback = (Func<Fixture, TSVector2, TSVector2, FP, FP>)null;
        }

        public List<Fixture> RayCast(TSVector2 point1, TSVector2 point2)
        {
            List<Fixture> affected = new List<Fixture>();
            this.RayCast((Func<Fixture, TSVector2, TSVector2, FP, FP>)((f, p, n, fr) =>
            {
                affected.Add(f);
                return (FP)1;
            }), point1, point2);
            return affected;
        }

        public void AddController(Controller controller)
        {
            Debug.Assert(!this.ControllerList.Contains(controller), "You are adding the same controller more than once.");
            controller.World = this;
            this.ControllerList.Add(controller);
            if (this.ControllerAdded == null)
                return;
            this.ControllerAdded(controller);
        }

        public void RemoveController(Controller controller)
        {
            Debug.Assert(this.ControllerList.Contains(controller), "You are removing a controller that is not in the simulation.");
            if (!this.ControllerList.Contains(controller))
                return;
            this.ControllerList.Remove(controller);
            if (this.ControllerRemoved != null)
                this.ControllerRemoved(controller);
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
            TSVector2 tsVector2 = new TSVector2(Settings.Epsilon, Settings.Epsilon);
            AABB aabb;
            aabb.LowerBound = point - tsVector2;
            aabb.UpperBound = point + tsVector2;
            this._myFixture = (Fixture)null;
            this._point1 = point;
            this.QueryAABB(new Func<Fixture, bool>(this.TestPointCallback), ref aabb);
            return this._myFixture;
        }

        private bool TestPointCallback(Fixture fixture)
        {
            if (!fixture.TestPoint(ref this._point1))
                return true;
            this._myFixture = fixture;
            return false;
        }

        public List<Fixture> TestPointAll(TSVector2 point)
        {
            TSVector2 tsVector2 = new TSVector2(Settings.Epsilon, Settings.Epsilon);
            AABB aabb;
            aabb.LowerBound = point - tsVector2;
            aabb.UpperBound = point + tsVector2;
            this._point2 = point;
            this._testPointAllFixtures = new List<Fixture>();
            this.QueryAABB(new Func<Fixture, bool>(this.TestPointAllCallback), ref aabb);
            return this._testPointAllFixtures;
        }

        private bool TestPointAllCallback(Fixture fixture)
        {
            if (fixture.TestPoint(ref this._point2))
                this._testPointAllFixtures.Add(fixture);
            return true;
        }

        public void ShiftOrigin(TSVector2 newOrigin)
        {
            foreach (Body body in this.BodyList)
            {
                body._xf.p -= newOrigin;
                body._sweep.C0 -= newOrigin;
                body._sweep.C -= newOrigin;
            }
            this.ContactManager.BroadPhase.ShiftOrigin(newOrigin);
        }

        public void Clear()
        {
            this.ProcessChanges();
            for (int index = this.BodyList.Count - 1; index >= 0; --index)
                this.RemoveBody(this.BodyList[index]);
            for (int index = this.ControllerList.Count - 1; index >= 0; --index)
                this.RemoveController(this.ControllerList[index]);
            for (int index = this.BreakableBodyList.Count - 1; index >= 0; --index)
                this.RemoveBreakableBody(this.BreakableBodyList[index]);
            this.ProcessChanges();
        }

        public List<IBody> Bodies()
        {
            List<IBody> bodyList = new List<IBody>();
            for (int index = 0; index < this.BodyList.Count; ++index)
                bodyList.Add((IBody)this.BodyList[index]);
            return bodyList;
        }
    }
}
