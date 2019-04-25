namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using TrueSync;

    public class World : IWorld
    {
        internal List<Body> _bodyAddList = new List<Body>();
        internal List<Body> _bodyRemoveList = new List<Body>();
        internal Queue<TrueSync.Physics2D.Contact> _contactPool = new Queue<TrueSync.Physics2D.Contact>(0x100);
        internal TOIInput _input = new TOIInput();
        internal FP _invDt0;
        internal List<TrueSync.Physics2D.Joint> _jointAddList = new List<TrueSync.Physics2D.Joint>();
        internal List<TrueSync.Physics2D.Joint> _jointRemoveList = new List<TrueSync.Physics2D.Joint>();
        internal Fixture _myFixture;
        private TSVector2 _point1;
        internal TSVector2 _point2;
        internal Func<Fixture, bool> _queryAABBCallback;
        private Func<int, bool> _queryAABBCallbackWrapper;
        private Func<Fixture, TSVector2, TSVector2, FP, FP> _rayCastCallback;
        private Func<RayCastInput, int, FP> _rayCastCallbackWrapper;
        internal Body[] _stack = new Body[0x40];
        internal bool _stepComplete;
        internal List<Fixture> _testPointAllFixtures;
        internal bool _worldHasNewFixture;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Body> <BodyList>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<BreakableBody> <BreakableBodyList>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.ContactManager <ContactManager>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<Controller> <ControllerList>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <Enabled>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private TrueSync.Physics2D.Island <Island>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private List<TrueSync.Physics2D.Joint> <JointList>k__BackingField;
        public BodyDelegate BodyAdded;
        public BodyDelegate BodyRemoved;
        public ControllerDelegate ControllerAdded;
        public ControllerDelegate ControllerRemoved;
        public FixtureDelegate FixtureAdded;
        public FixtureDelegate FixtureRemoved;
        public TSVector2 Gravity;
        public JointDelegate JointAdded;
        public JointDelegate JointRemoved;

        public World(TSVector2 gravity)
        {
            this.Island = new TrueSync.Physics2D.Island();
            this.Enabled = true;
            this.ControllerList = new List<Controller>();
            this.BreakableBodyList = new List<BreakableBody>();
            this.BodyList = new List<Body>(0x20);
            this.JointList = new List<TrueSync.Physics2D.Joint>(0x20);
            this._queryAABBCallbackWrapper = new Func<int, bool>(this.QueryAABBCallbackWrapper);
            this._rayCastCallbackWrapper = new Func<RayCastInput, int, FP>(this.RayCastCallbackWrapper);
            this.ContactManager = new TrueSync.Physics2D.ContactManager(new DynamicTreeBroadPhase());
            this.Gravity = gravity;
        }

        internal void AddBody(Body body)
        {
            Debug.Assert(!this._bodyAddList.Contains(body), "You are adding the same body more than once.");
            if (!this._bodyAddList.Contains(body))
            {
                this._bodyAddList.Add(body);
            }
        }

        public void AddBreakableBody(BreakableBody breakableBody)
        {
            this.BreakableBodyList.Add(breakableBody);
        }

        public void AddController(Controller controller)
        {
            Debug.Assert(!this.ControllerList.Contains(controller), "You are adding the same controller more than once.");
            controller.World = this;
            this.ControllerList.Add(controller);
            if (this.ControllerAdded > null)
            {
                this.ControllerAdded(controller);
            }
        }

        public void AddJoint(TrueSync.Physics2D.Joint joint)
        {
            Debug.Assert(!this._jointAddList.Contains(joint), "You are adding the same joint more than once.");
            if (!this._jointAddList.Contains(joint))
            {
                this._jointAddList.Add(joint);
            }
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

        public void ClearForces()
        {
            for (int i = 0; i < this.BodyList.Count; i++)
            {
                Body body = this.BodyList[i];
                body._force = TSVector2.zero;
                body._torque = 0f;
            }
        }

        public void ProcessAddedBodies()
        {
            if (this._bodyAddList.Count > 0)
            {
                foreach (Body body in this._bodyAddList)
                {
                    this.BodyList.Add(body);
                    if (this.BodyAdded > null)
                    {
                        this.BodyAdded(body);
                    }
                }
                this._bodyAddList.Clear();
            }
        }

        private void ProcessAddedJoints()
        {
            if (this._jointAddList.Count > 0)
            {
                foreach (TrueSync.Physics2D.Joint joint in this._jointAddList)
                {
                    this.JointList.Add(joint);
                    joint.EdgeA.Joint = joint;
                    joint.EdgeA.Other = joint.BodyB;
                    joint.EdgeA.Prev = null;
                    joint.EdgeA.Next = joint.BodyA.JointList;
                    if (joint.BodyA.JointList > null)
                    {
                        joint.BodyA.JointList.Prev = joint.EdgeA;
                    }
                    joint.BodyA.JointList = joint.EdgeA;
                    if (!joint.IsFixedType())
                    {
                        joint.EdgeB.Joint = joint;
                        joint.EdgeB.Other = joint.BodyA;
                        joint.EdgeB.Prev = null;
                        joint.EdgeB.Next = joint.BodyB.JointList;
                        if (joint.BodyB.JointList > null)
                        {
                            joint.BodyB.JointList.Prev = joint.EdgeB;
                        }
                        joint.BodyB.JointList = joint.EdgeB;
                        Body bodyA = joint.BodyA;
                        Body bodyB = joint.BodyB;
                        if (!joint.CollideConnected)
                        {
                            for (ContactEdge edge = bodyB.ContactList; edge > null; edge = edge.Next)
                            {
                                if (edge.Other == bodyA)
                                {
                                    edge.Contact.FilterFlag = true;
                                }
                            }
                        }
                    }
                    if (this.JointAdded > null)
                    {
                        this.JointAdded(joint);
                    }
                }
                this._jointAddList.Clear();
            }
        }

        public void ProcessChanges()
        {
            this.ProcessAddedBodies();
            this.ProcessAddedJoints();
            this.ProcessRemovedBodies();
            this.ProcessRemovedJoints();
        }

        public void ProcessRemovedBodies()
        {
            if (this._bodyRemoveList.Count > 0)
            {
                foreach (Body body in this._bodyRemoveList)
                {
                    Debug.Assert(this.BodyList.Count > 0);
                    bool flag2 = this._bodyAddList.Contains(body);
                    if (this.BodyList.Contains(body) || flag2)
                    {
                        JointEdge jointList = body.JointList;
                        while (jointList > null)
                        {
                            JointEdge edge3 = jointList;
                            jointList = jointList.Next;
                            this.RemoveJoint(edge3.Joint, false);
                        }
                        body.JointList = null;
                        ContactEdge contactList = body.ContactList;
                        while (contactList > null)
                        {
                            ContactEdge edge4 = contactList;
                            contactList = contactList.Next;
                            this.ContactManager.Destroy(edge4.Contact);
                        }
                        body.ContactList = null;
                        for (int i = 0; i < body.FixtureList.Count; i++)
                        {
                            body.FixtureList[i].DestroyProxies(this.ContactManager.BroadPhase);
                            body.FixtureList[i].Destroy();
                        }
                        body.FixtureList = null;
                        if (flag2)
                        {
                            this._bodyAddList.Remove(body);
                        }
                        else
                        {
                            this.BodyList.Remove(body);
                        }
                        if ((this.BodyRemoved != null) && (body._specialSensor == BodySpecialSensor.None))
                        {
                            this.BodyRemoved(body);
                        }
                    }
                }
                this._bodyRemoveList.Clear();
            }
        }

        private void ProcessRemovedJoints()
        {
            if (this._jointRemoveList.Count > 0)
            {
                foreach (TrueSync.Physics2D.Joint joint in this._jointRemoveList)
                {
                    bool collideConnected = joint.CollideConnected;
                    this.JointList.Remove(joint);
                    Body bodyA = joint.BodyA;
                    Body bodyB = joint.BodyB;
                    bodyA.Awake = true;
                    if (!joint.IsFixedType())
                    {
                        bodyB.Awake = true;
                    }
                    if (joint.EdgeA.Prev > null)
                    {
                        joint.EdgeA.Prev.Next = joint.EdgeA.Next;
                    }
                    if (joint.EdgeA.Next > null)
                    {
                        joint.EdgeA.Next.Prev = joint.EdgeA.Prev;
                    }
                    if (joint.EdgeA == bodyA.JointList)
                    {
                        bodyA.JointList = joint.EdgeA.Next;
                    }
                    joint.EdgeA.Prev = null;
                    joint.EdgeA.Next = null;
                    if (!joint.IsFixedType())
                    {
                        if (joint.EdgeB.Prev > null)
                        {
                            joint.EdgeB.Prev.Next = joint.EdgeB.Next;
                        }
                        if (joint.EdgeB.Next > null)
                        {
                            joint.EdgeB.Next.Prev = joint.EdgeB.Prev;
                        }
                        if (joint.EdgeB == bodyB.JointList)
                        {
                            bodyB.JointList = joint.EdgeB.Next;
                        }
                        joint.EdgeB.Prev = null;
                        joint.EdgeB.Next = null;
                    }
                    if (!joint.IsFixedType() && !collideConnected)
                    {
                        for (ContactEdge edge = bodyB.ContactList; edge > null; edge = edge.Next)
                        {
                            if (edge.Other == bodyA)
                            {
                                edge.Contact.FilterFlag = true;
                            }
                        }
                    }
                    if (this.JointRemoved > null)
                    {
                        this.JointRemoved(joint);
                    }
                }
                this._jointRemoveList.Clear();
            }
        }

        public List<Fixture> QueryAABB(ref AABB aabb)
        {
            List<Fixture> affected = new List<Fixture>();
            this.QueryAABB(delegate (Fixture fixture) {
                affected.Add(fixture);
                return true;
            }, ref aabb);
            return affected;
        }

        public void QueryAABB(Func<Fixture, bool> callback, ref AABB aabb)
        {
            this._queryAABBCallback = callback;
            this.ContactManager.BroadPhase.Query(this._queryAABBCallbackWrapper, ref aabb);
            this._queryAABBCallback = null;
        }

        private bool QueryAABBCallbackWrapper(int proxyId)
        {
            FixtureProxy proxy = this.ContactManager.BroadPhase.GetProxy(proxyId);
            return this._queryAABBCallback(proxy.Fixture);
        }

        public List<Fixture> RayCast(TSVector2 point1, TSVector2 point2)
        {
            List<Fixture> affected = new List<Fixture>();
            this.RayCast(delegate (Fixture f, TSVector2 p, TSVector2 n, FP fr) {
                affected.Add(f);
                return 1;
            }, point1, point2);
            return affected;
        }

        public void RayCast(Func<Fixture, TSVector2, TSVector2, FP, FP> callback, TSVector2 point1, TSVector2 point2)
        {
            RayCastInput input = new RayCastInput {
                MaxFraction = 1f,
                Point1 = point1,
                Point2 = point2
            };
            this._rayCastCallback = callback;
            this.ContactManager.BroadPhase.RayCast(this._rayCastCallbackWrapper, ref input);
            this._rayCastCallback = null;
        }

        private FP RayCastCallbackWrapper(RayCastInput rayCastInput, int proxyId)
        {
            RayCastOutput output;
            FixtureProxy proxy = this.ContactManager.BroadPhase.GetProxy(proxyId);
            Fixture fixture = proxy.Fixture;
            int childIndex = proxy.ChildIndex;
            if (fixture.RayCast(out output, ref rayCastInput, childIndex))
            {
                FP fraction = output.Fraction;
                TSVector2 vector = (TSVector2) (((1f - fraction) * rayCastInput.Point1) + (fraction * rayCastInput.Point2));
                return this._rayCastCallback(fixture, vector, output.Normal, fraction);
            }
            return rayCastInput.MaxFraction;
        }

        public void RemoveBody(Body body)
        {
            Debug.Assert(!this._bodyRemoveList.Contains(body), "The body is already marked for removal. You are removing the body more than once.");
            if (!this._bodyRemoveList.Contains(body))
            {
                this._bodyRemoveList.Add(body);
            }
        }

        public void RemoveBreakableBody(BreakableBody breakableBody)
        {
            Debug.Assert(this.BreakableBodyList.Contains(breakableBody));
            this.BreakableBodyList.Remove(breakableBody);
        }

        public void RemoveController(Controller controller)
        {
            Debug.Assert(this.ControllerList.Contains(controller), "You are removing a controller that is not in the simulation.");
            if (this.ControllerList.Contains(controller))
            {
                this.ControllerList.Remove(controller);
                if (this.ControllerRemoved > null)
                {
                    this.ControllerRemoved(controller);
                }
            }
        }

        public void RemoveJoint(TrueSync.Physics2D.Joint joint)
        {
            this.RemoveJoint(joint, true);
        }

        private void RemoveJoint(TrueSync.Physics2D.Joint joint, bool doCheck)
        {
            if (doCheck)
            {
                Debug.Assert(!this._jointRemoveList.Contains(joint), "The joint is already marked for removal. You are removing the joint more than once.");
            }
            if (!this._jointRemoveList.Contains(joint))
            {
                this._jointRemoveList.Add(joint);
            }
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

        private void Solve(ref TimeStep step)
        {
            this.Island.Reset(this.BodyList.Count, this.ContactManager.ContactList.Count, this.JointList.Count, this.ContactManager);
            foreach (Body body in this.BodyList)
            {
                body._island = false;
            }
            foreach (TrueSync.Physics2D.Contact contact in this.ContactManager.ContactList)
            {
                contact.IslandFlag = false;
            }
            foreach (TrueSync.Physics2D.Joint joint in this.JointList)
            {
                joint.IslandFlag = false;
            }
            int count = this.BodyList.Count;
            if (count > this._stack.Length)
            {
                this._stack = new Body[Math.Max(this._stack.Length * 2, count)];
            }
            for (int i = this.BodyList.Count - 1; i >= 0; i--)
            {
                Body body2 = this.BodyList[i];
                if ((!body2._island && (body2.Awake && body2.Enabled)) && (body2.BodyType != BodyType.Static))
                {
                    this.Island.Clear();
                    int num3 = 0;
                    this._stack[num3++] = body2;
                    body2._island = true;
                    while (num3 > 0)
                    {
                        Body body3 = this._stack[--num3];
                        Debug.Assert(body3.Enabled);
                        this.Island.Add(body3);
                        body3.Awake = true;
                        if (body3.BodyType != BodyType.Static)
                        {
                            for (ContactEdge edge = body3.ContactList; edge > null; edge = edge.Next)
                            {
                                TrueSync.Physics2D.Contact contact2 = edge.Contact;
                                if (!contact2.IslandFlag && (edge.Contact.Enabled && edge.Contact.IsTouching))
                                {
                                    bool isSensor = contact2.FixtureA.IsSensor;
                                    bool flag7 = contact2.FixtureB.IsSensor;
                                    if (!(isSensor | flag7))
                                    {
                                        this.Island.Add(contact2);
                                        contact2.IslandFlag = true;
                                        Body other = edge.Other;
                                        if (!other._island)
                                        {
                                            Debug.Assert(num3 < count);
                                            this._stack[num3++] = other;
                                            other._island = true;
                                        }
                                    }
                                }
                            }
                            for (JointEdge edge2 = body3.JointList; edge2 > null; edge2 = edge2.Next)
                            {
                                if (!edge2.Joint.IslandFlag)
                                {
                                    Body body5 = edge2.Other;
                                    if (body5 > null)
                                    {
                                        if (body5.Enabled)
                                        {
                                            this.Island.Add(edge2.Joint);
                                            edge2.Joint.IslandFlag = true;
                                            if (!body5._island)
                                            {
                                                Debug.Assert(num3 < count);
                                                this._stack[num3++] = body5;
                                                body5._island = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        this.Island.Add(edge2.Joint);
                                        edge2.Joint.IslandFlag = true;
                                    }
                                }
                            }
                        }
                    }
                    this.Island.Solve(ref step, ref this.Gravity);
                    for (int j = 0; j < this.Island.BodyCount; j++)
                    {
                        Body body6 = this.Island.Bodies[j];
                        if (body6.BodyType == BodyType.Static)
                        {
                            body6._island = false;
                        }
                    }
                }
            }
            foreach (Body body7 in this.BodyList)
            {
                if (body7._island && (body7.BodyType != BodyType.Static))
                {
                    body7.SynchronizeFixtures();
                }
            }
            this.ContactManager.FindNewContacts();
        }

        private void SolveTOI(ref TimeStep step)
        {
            TrueSync.Physics2D.Contact contact2;
            this.Island.Reset(0x40, 0x20, 0, this.ContactManager);
            if (this._stepComplete)
            {
                for (int j = 0; j < this.BodyList.Count; j++)
                {
                    this.BodyList[j]._island = false;
                    this.BodyList[j]._sweep.Alpha0 = 0f;
                }
                for (int k = 0; k < this.ContactManager.ContactList.Count; k++)
                {
                    TrueSync.Physics2D.Contact contact = this.ContactManager.ContactList[k];
                    contact.IslandFlag = false;
                    contact.TOIFlag = false;
                    contact._toiCount = 0;
                    contact._toi = 1f;
                }
            }
        Label_00DB:
            contact2 = null;
            FP alpha = 1f;
            for (int i = 0; i < this.ContactManager.ContactList.Count; i++)
            {
                TrueSync.Physics2D.Contact contact3 = this.ContactManager.ContactList[i];
                if (contact3.Enabled && (contact3._toiCount <= 8))
                {
                    FP fp2;
                    if (contact3.TOIFlag)
                    {
                        fp2 = contact3._toi;
                    }
                    else
                    {
                        TOIOutput output;
                        Fixture fixtureA = contact3.FixtureA;
                        Fixture fixtureB = contact3.FixtureB;
                        if (fixtureA.IsSensor || fixtureB.IsSensor)
                        {
                            continue;
                        }
                        Body body3 = fixtureA.Body;
                        Body body4 = fixtureB.Body;
                        BodyType bodyType = body3.BodyType;
                        BodyType type2 = body4.BodyType;
                        Debug.Assert((bodyType == BodyType.Dynamic) || (type2 == BodyType.Dynamic));
                        bool flag7 = body3.Awake && (bodyType > BodyType.Static);
                        bool flag8 = body4.Awake && (type2 > BodyType.Static);
                        if (!flag7 && !flag8)
                        {
                            continue;
                        }
                        bool flag9 = ((body3.IsBullet || (bodyType != BodyType.Dynamic)) && ((fixtureA.IgnoreCCDWith & fixtureB.CollisionCategories) == Category.None)) && !body3.IgnoreCCD;
                        bool flag10 = ((body4.IsBullet || (type2 != BodyType.Dynamic)) && ((fixtureB.IgnoreCCDWith & fixtureA.CollisionCategories) == Category.None)) && !body4.IgnoreCCD;
                        if (!flag9 && !flag10)
                        {
                            continue;
                        }
                        FP fp3 = body3._sweep.Alpha0;
                        if (body3._sweep.Alpha0 < body4._sweep.Alpha0)
                        {
                            fp3 = body4._sweep.Alpha0;
                            body3._sweep.Advance(fp3);
                        }
                        else if (body4._sweep.Alpha0 < body3._sweep.Alpha0)
                        {
                            fp3 = body3._sweep.Alpha0;
                            body4._sweep.Advance(fp3);
                        }
                        Debug.Assert(fp3 < 1f);
                        this._input.ProxyA.Set(fixtureA.Shape, contact3.ChildIndexA);
                        this._input.ProxyB.Set(fixtureB.Shape, contact3.ChildIndexB);
                        this._input.SweepA = body3._sweep;
                        this._input.SweepB = body4._sweep;
                        this._input.TMax = 1f;
                        TimeOfImpact.CalculateTimeOfImpact(out output, this._input);
                        FP t = output.T;
                        if (output.State == TOIOutputState.Touching)
                        {
                            fp2 = TSMath.Min(fp3 + ((1f - fp3) * t), 1f);
                        }
                        else
                        {
                            fp2 = 1f;
                        }
                        contact3._toi = fp2;
                        contact3.TOIFlag = true;
                    }
                    if (fp2 < alpha)
                    {
                        contact2 = contact3;
                        alpha = fp2;
                    }
                }
            }
            if ((contact2 == null) || ((1f - (10f * Settings.Epsilon)) < alpha))
            {
                this._stepComplete = true;
            }
            else
            {
                Fixture fixture = contact2.FixtureA;
                Fixture fixture2 = contact2.FixtureB;
                Body body = fixture.Body;
                Body body2 = fixture2.Body;
                Sweep sweep = body._sweep;
                Sweep sweep2 = body2._sweep;
                body.Advance(alpha);
                body2.Advance(alpha);
                contact2.Update(this.ContactManager);
                contact2.TOIFlag = false;
                contact2._toiCount++;
                if (!contact2.Enabled || !contact2.IsTouching)
                {
                    contact2.Enabled = false;
                    body._sweep = sweep;
                    body2._sweep = sweep2;
                    body.SynchronizeTransform();
                    body2.SynchronizeTransform();
                }
                else
                {
                    TimeStep step2;
                    body.Awake = true;
                    body2.Awake = true;
                    this.Island.Clear();
                    this.Island.Add(body);
                    this.Island.Add(body2);
                    this.Island.Add(contact2);
                    body._island = true;
                    body2._island = true;
                    contact2.IslandFlag = true;
                    Body[] bodyArray = new Body[] { body, body2 };
                    for (int m = 0; m < 2; m++)
                    {
                        Body body5 = bodyArray[m];
                        if (body5.BodyType == BodyType.Dynamic)
                        {
                            for (ContactEdge edge = body5.ContactList; edge > null; edge = edge.Next)
                            {
                                TrueSync.Physics2D.Contact contact4 = edge.Contact;
                                if ((this.Island.BodyCount == this.Island.BodyCapacity) || (this.Island.ContactCount == this.Island.ContactCapacity))
                                {
                                    break;
                                }
                                if (!contact4.IslandFlag)
                                {
                                    Body other = edge.Other;
                                    if ((((other.BodyType != BodyType.Dynamic) || body5.IsBullet) || other.IsBullet) && (!contact4.FixtureA.IsSensor && !contact4.FixtureB.IsSensor))
                                    {
                                        Sweep sweep3 = other._sweep;
                                        if (!other._island)
                                        {
                                            other.Advance(alpha);
                                        }
                                        contact4.Update(this.ContactManager);
                                        if (!contact4.Enabled)
                                        {
                                            other._sweep = sweep3;
                                            other.SynchronizeTransform();
                                        }
                                        else if (!contact4.IsTouching)
                                        {
                                            other._sweep = sweep3;
                                            other.SynchronizeTransform();
                                        }
                                        else
                                        {
                                            contact4.IslandFlag = true;
                                            this.Island.Add(contact4);
                                            if (!other._island)
                                            {
                                                other._island = true;
                                                if (other.BodyType > BodyType.Static)
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
                    step2.dt = (1f - alpha) * step.dt;
                    step2.inv_dt = 1f / step2.dt;
                    step2.dtRatio = 1f;
                    this.Island.SolveTOI(ref step2, body.IslandIndex, body2.IslandIndex, false);
                    for (int n = 0; n < this.Island.BodyCount; n++)
                    {
                        Body body7 = this.Island.Bodies[n];
                        body7._island = false;
                        if (body7.BodyType == BodyType.Dynamic)
                        {
                            body7.SynchronizeFixtures();
                            for (ContactEdge edge2 = body7.ContactList; edge2 > null; edge2 = edge2.Next)
                            {
                                edge2.Contact.TOIFlag = false;
                                edge2.Contact.IslandFlag = false;
                            }
                        }
                    }
                    this.ContactManager.FindNewContacts();
                }
                goto Label_00DB;
            }
        }

        public void Step(FP dt)
        {
            if (this.Enabled)
            {
                TimeStep step;
                this.ProcessChanges();
                if (this._worldHasNewFixture)
                {
                    this.ContactManager.FindNewContacts();
                    this._worldHasNewFixture = false;
                }
                step.inv_dt = (dt > 0f) ? (1f / dt) : 0f;
                step.dt = dt;
                step.dtRatio = this._invDt0 * dt;
                for (int i = 0; i < this.ControllerList.Count; i++)
                {
                    this.ControllerList[i].Update(dt);
                }
                this.ContactManager.Collide();
                this.Solve(ref step);
                if (Settings.ContinuousPhysics)
                {
                    this.SolveTOI(ref step);
                }
                this.ClearForces();
                for (int j = 0; j < this.BreakableBodyList.Count; j++)
                {
                    this.BreakableBodyList[j].Update();
                }
                for (int k = 0; k < this.BodyList.Count; k++)
                {
                    List<IBodyConstraint> bodyConstraints = this.BodyList[k].bodyConstraints;
                    for (int m = 0; m < bodyConstraints.Count; m++)
                    {
                        bodyConstraints[m].PostStep();
                    }
                }
                this._invDt0 = step.inv_dt;
            }
        }

        public Fixture TestPoint(TSVector2 point)
        {
            AABB aabb;
            TSVector2 vector = new TSVector2(Settings.Epsilon, Settings.Epsilon);
            aabb.LowerBound = point - vector;
            aabb.UpperBound = point + vector;
            this._myFixture = null;
            this._point1 = point;
            this.QueryAABB(new Func<Fixture, bool>(this.TestPointCallback), ref aabb);
            return this._myFixture;
        }

        public List<Fixture> TestPointAll(TSVector2 point)
        {
            AABB aabb;
            TSVector2 vector = new TSVector2(Settings.Epsilon, Settings.Epsilon);
            aabb.LowerBound = point - vector;
            aabb.UpperBound = point + vector;
            this._point2 = point;
            this._testPointAllFixtures = new List<Fixture>();
            this.QueryAABB(new Func<Fixture, bool>(this.TestPointAllCallback), ref aabb);
            return this._testPointAllFixtures;
        }

        private bool TestPointAllCallback(Fixture fixture)
        {
            if (fixture.TestPoint(ref this._point2))
            {
                this._testPointAllFixtures.Add(fixture);
            }
            return true;
        }

        private bool TestPointCallback(Fixture fixture)
        {
            if (fixture.TestPoint(ref this._point1))
            {
                this._myFixture = fixture;
                return false;
            }
            return true;
        }

        public List<Body> BodyList { get; private set; }

        public List<BreakableBody> BreakableBodyList { get; private set; }

        public List<TrueSync.Physics2D.Contact> ContactList
        {
            get
            {
                return this.ContactManager.ContactList;
            }
        }

        public TrueSync.Physics2D.ContactManager ContactManager { get; private set; }

        public List<Controller> ControllerList { get; private set; }

        public bool Enabled { get; set; }

        public TrueSync.Physics2D.Island Island { get; private set; }

        public List<TrueSync.Physics2D.Joint> JointList { get; private set; }

        public int ProxyCount
        {
            get
            {
                return this.ContactManager.BroadPhase.ProxyCount;
            }
        }
    }
}

