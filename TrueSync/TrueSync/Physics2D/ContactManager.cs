namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;

    public class ContactManager
    {
        public BeginContactDelegate BeginContact;
        public IBroadPhase BroadPhase;
        public CollisionFilterDelegate ContactFilter;
        public List<Contact> ContactList = new List<Contact>(0x80);
        public EndContactDelegate EndContact;
        public BroadphaseDelegate OnBroadphaseCollision;
        public static IPhysicsManager physicsManager;
        public PostSolveDelegate PostSolve;
        public PreSolveDelegate PreSolve;
        public EndContactDelegate StayContact;

        internal ContactManager(IBroadPhase broadPhase)
        {
            this.BroadPhase = broadPhase;
            this.OnBroadphaseCollision = new BroadphaseDelegate(this.AddPair);
        }

        private void AddPair(ref FixtureProxy proxyA, ref FixtureProxy proxyB)
        {
            Fixture fixtureA = proxyA.Fixture;
            Fixture fixture = proxyB.Fixture;
            int childIndex = proxyA.ChildIndex;
            int indexB = proxyB.ChildIndex;
            Body other = fixtureA.Body;
            Body body = fixture.Body;
            if (other != body)
            {
                for (ContactEdge edge = body.ContactList; edge > null; edge = edge.Next)
                {
                    if (edge.Other == other)
                    {
                        Fixture fixture3 = edge.Contact.FixtureA;
                        Fixture fixtureB = edge.Contact.FixtureB;
                        int childIndexA = edge.Contact.ChildIndexA;
                        int childIndexB = edge.Contact.ChildIndexB;
                        if (((((fixture3 == fixtureA) && (fixtureB == fixture)) && (childIndexA == childIndex)) && (childIndexB == indexB)) || ((((fixture3 == fixture) && (fixtureB == fixtureA)) && (childIndexA == indexB)) && (childIndexB == childIndex)))
                        {
                            return;
                        }
                    }
                }
                if (((body.ShouldCollide(other) && ShouldCollide(fixtureA, fixture)) && (((this.ContactFilter == null) || this.ContactFilter(fixtureA, fixture)) && ((fixtureA.BeforeCollision == null) || fixtureA.BeforeCollision(fixtureA, fixture)))) && ((fixture.BeforeCollision == null) || fixture.BeforeCollision(fixture, fixtureA)))
                {
                    Body body3 = null;
                    Body item = null;
                    if (other.SpecialSensor > BodySpecialSensor.None)
                    {
                        body3 = other;
                        item = body;
                    }
                    else if (body.SpecialSensor > BodySpecialSensor.None)
                    {
                        body3 = body;
                        item = other;
                    }
                    if (body3 > null)
                    {
                        if (!Collision.TestOverlap(other.FixtureList[0].Shape, childIndex, body.FixtureList[0].Shape, indexB, ref other._xf, ref body._xf))
                        {
                            return;
                        }
                        body3._specialSensorResults.Add(item);
                        if (body3.SpecialSensor == BodySpecialSensor.ActiveOnce)
                        {
                            body3.disabled = true;
                            return;
                        }
                    }
                    Contact contact = Contact.Create(fixtureA, childIndex, fixture, indexB);
                    if (contact != null)
                    {
                        fixtureA = contact.FixtureA;
                        fixture = contact.FixtureB;
                        other = fixtureA.Body;
                        body = fixture.Body;
                        this.ContactList.Add(contact);
                        contact._nodeA.Contact = contact;
                        contact._nodeA.Other = body;
                        contact._nodeA.Prev = null;
                        contact._nodeA.Next = other.ContactList;
                        if (other.ContactList > null)
                        {
                            other.ContactList.Prev = contact._nodeA;
                        }
                        other.ContactList = contact._nodeA;
                        contact._nodeB.Contact = contact;
                        contact._nodeB.Other = other;
                        contact._nodeB.Prev = null;
                        contact._nodeB.Next = body.ContactList;
                        if (body.ContactList > null)
                        {
                            body.ContactList.Prev = contact._nodeB;
                        }
                        body.ContactList = contact._nodeB;
                        if (!fixtureA.IsSensor && !fixture.IsSensor)
                        {
                            other.Awake = true;
                            body.Awake = true;
                        }
                    }
                }
            }
        }

        public static bool CheckCollisionConditions(Fixture fixtureA, Fixture fixtureB)
        {
            Body body = fixtureA.Body;
            Body body2 = fixtureB.Body;
            if ((body.disabled || body2.disabled) || !physicsManager.IsCollisionEnabled(body, body2))
            {
                return false;
            }
            if ((body._specialSensor == BodySpecialSensor.None) && (body2._specialSensor <= BodySpecialSensor.None))
            {
                if (body.IsStatic && body2.IsStatic)
                {
                    return false;
                }
                bool flag = fixtureA.IsSensor || fixtureB.IsSensor;
                if (!flag && (((body.IsKinematic && body2.IsKinematic) || (body.IsKinematic && body2.IsStatic)) || (body2.IsKinematic && body.IsStatic)))
                {
                    return false;
                }
            }
            return true;
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
                Body other = fixtureA.Body;
                Body body = fixtureB.Body;
                if (other.Enabled && body.Enabled)
                {
                    if (contact.FilterFlag)
                    {
                        if (!body.ShouldCollide(other))
                        {
                            Contact contact2 = contact;
                            this.Destroy(contact2);
                            continue;
                        }
                        if (!ShouldCollide(fixtureA, fixtureB))
                        {
                            Contact contact3 = contact;
                            this.Destroy(contact3);
                            continue;
                        }
                        if ((this.ContactFilter != null) && !this.ContactFilter(fixtureA, fixtureB))
                        {
                            Contact contact4 = contact;
                            this.Destroy(contact4);
                            continue;
                        }
                        contact.FilterFlag = false;
                    }
                    bool flag = other.Awake && (other.BodyType > BodyType.Static);
                    bool flag2 = body.Awake && (body.BodyType > BodyType.Static);
                    if (flag || flag2)
                    {
                        int proxyId = fixtureA.Proxies[childIndexA].ProxyId;
                        int proxyIdB = fixtureB.Proxies[childIndexB].ProxyId;
                        if (!this.BroadPhase.TestOverlap(proxyId, proxyIdB))
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
            }
        }

        internal void Destroy(Contact contact)
        {
            Fixture fixtureA = contact.FixtureA;
            Fixture fixtureB = contact.FixtureB;
            Body body = fixtureA.Body;
            Body body2 = fixtureB.Body;
            if (contact.IsTouching)
            {
                if ((fixtureA != null) && (fixtureA.OnSeparation > null))
                {
                    fixtureA.OnSeparation(fixtureA, fixtureB);
                }
                if ((fixtureB != null) && (fixtureB.OnSeparation > null))
                {
                    fixtureB.OnSeparation(fixtureB, fixtureA);
                }
                if (this.EndContact > null)
                {
                    this.EndContact(contact);
                }
            }
            this.ContactList.Remove(contact);
            if (contact._nodeA.Prev > null)
            {
                contact._nodeA.Prev.Next = contact._nodeA.Next;
            }
            if (contact._nodeA.Next > null)
            {
                contact._nodeA.Next.Prev = contact._nodeA.Prev;
            }
            if (contact._nodeA == body.ContactList)
            {
                body.ContactList = contact._nodeA.Next;
            }
            if (contact._nodeB.Prev > null)
            {
                contact._nodeB.Prev.Next = contact._nodeB.Next;
            }
            if (contact._nodeB.Next > null)
            {
                contact._nodeB.Next.Prev = contact._nodeB.Prev;
            }
            if (contact._nodeB == body2.ContactList)
            {
                body2.ContactList = contact._nodeB.Next;
            }
            contact.Destroy();
        }

        internal void FindNewContacts()
        {
            this.BroadPhase.UpdatePairs(this.OnBroadphaseCollision);
        }

        private static bool ShouldCollide(Fixture fixtureA, Fixture fixtureB)
        {
            if (!CheckCollisionConditions(fixtureA, fixtureB))
            {
                return false;
            }
            if (Settings.UseFPECollisionCategories)
            {
                if (((fixtureA.CollisionGroup == fixtureB.CollisionGroup) && (fixtureA.CollisionGroup != 0)) && (fixtureB.CollisionGroup > 0))
                {
                    return false;
                }
                if (((fixtureA.CollisionCategories & fixtureB.CollidesWith) == Category.None) & ((fixtureB.CollisionCategories & fixtureA.CollidesWith) == Category.None))
                {
                    return false;
                }
                if (fixtureA.IsFixtureIgnored(fixtureB) || fixtureB.IsFixtureIgnored(fixtureA))
                {
                    return false;
                }
                return true;
            }
            if ((fixtureA.CollisionGroup == fixtureB.CollisionGroup) && (fixtureA.CollisionGroup > 0))
            {
                return (fixtureA.CollisionGroup > 0);
            }
            bool flag = ((fixtureA.CollidesWith & fixtureB.CollisionCategories) != Category.None) && ((fixtureA.CollisionCategories & fixtureB.CollidesWith) > Category.None);
            if (flag && (fixtureA.IsFixtureIgnored(fixtureB) || fixtureB.IsFixtureIgnored(fixtureA)))
            {
                return false;
            }
            return flag;
        }

        internal void UpdateContacts(ContactEdge contactEdge, bool value)
        {
        }
    }
}

