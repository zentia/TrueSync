// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.Contact
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
    public class Contact
    {
        private static EdgeShape _edge = new EdgeShape();
        private static Contact.ContactType[,] _registers = new Contact.ContactType[4, 4]
        {
      {
        Contact.ContactType.Circle,
        Contact.ContactType.EdgeAndCircle,
        Contact.ContactType.PolygonAndCircle,
        Contact.ContactType.ChainAndCircle
      },
      {
        Contact.ContactType.EdgeAndCircle,
        Contact.ContactType.NotSupported,
        Contact.ContactType.EdgeAndPolygon,
        Contact.ContactType.NotSupported
      },
      {
        Contact.ContactType.PolygonAndCircle,
        Contact.ContactType.EdgeAndPolygon,
        Contact.ContactType.Polygon,
        Contact.ContactType.ChainAndPolygon
      },
      {
        Contact.ContactType.ChainAndCircle,
        Contact.ContactType.NotSupported,
        Contact.ContactType.ChainAndPolygon,
        Contact.ContactType.NotSupported
      }
        };
        internal ContactEdge _nodeA = new ContactEdge();
        internal ContactEdge _nodeB = new ContactEdge();
        internal Contact.ContactType _type;
        internal int _toiCount;
        internal FP _toi;
        public Fixture FixtureA;
        public Fixture FixtureB;
        public Manifold Manifold;

        public FP Friction { get; set; }

        public FP Restitution { get; set; }

        internal string Key
        {
            get
            {
                return this.FixtureA.FixtureId.ToString() + "_" + (object)this.FixtureB.FixtureId;
            }
        }

        public FP TangentSpeed { get; set; }

        public bool Enabled { get; set; }

        public int ChildIndexA { get; internal set; }

        public int ChildIndexB { get; internal set; }

        public bool IsTouching { get; set; }

        internal bool IslandFlag { get; set; }

        internal bool TOIFlag { get; set; }

        internal bool FilterFlag { get; set; }

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
            TSVector2 tsVector2;
            tsVector2.x = this.FixtureA.Body.LinearVelocity.x - this.FixtureB.Body.LinearVelocity.x;
            tsVector2.y = this.FixtureA.Body.LinearVelocity.y - this.FixtureB.Body.LinearVelocity.y;
            return tsVector2;
        }

        public void GetWorldManifold(out TSVector2 normal, out FixedArray2<TSVector2> points)
        {
            Body body1 = this.FixtureA.Body;
            Body body2 = this.FixtureB.Body;
            Shape shape1 = this.FixtureA.Shape;
            Shape shape2 = this.FixtureB.Shape;
            ContactSolver.WorldManifold.Initialize(ref this.Manifold, ref body1._xf, shape1.Radius, ref body2._xf, shape2.Radius, out normal, out points);
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
            this._nodeA.Contact = (Contact)null;
            this._nodeA.Prev = (ContactEdge)null;
            this._nodeA.Next = (ContactEdge)null;
            this._nodeA.Other = (Body)null;
            this._nodeB.Contact = (Contact)null;
            this._nodeB.Prev = (ContactEdge)null;
            this._nodeB.Next = (ContactEdge)null;
            this._nodeB.Other = (Body)null;
            this._toiCount = 0;
            if (this.FixtureA != null && this.FixtureB != null)
            {
                this.Friction = Settings.MixFriction(this.FixtureA.Friction, this.FixtureB.Friction);
                this.Restitution = Settings.MixRestitution(this.FixtureA.Restitution, this.FixtureB.Restitution);
            }
            this.TangentSpeed = (FP)0;
        }

        internal void Update(ContactManager contactManager)
        {
            Body body1 = this.FixtureA.Body;
            Body body2 = this.FixtureB.Body;
            if (this.FixtureA == null || this.FixtureB == null)
                return;
            if (!ContactManager.CheckCollisionConditions(this.FixtureA, this.FixtureB))
            {
                this.Enabled = false;
            }
            else
            {
                Manifold manifold = this.Manifold;
                this.Enabled = true;
                bool isTouching = this.IsTouching;
                bool flag1 = this.FixtureA.IsSensor || this.FixtureB.IsSensor;
                bool flag2;
                if (flag1)
                {
                    flag2 = Collision.TestOverlap(this.FixtureA.Shape, this.ChildIndexA, this.FixtureB.Shape, this.ChildIndexB, ref body1._xf, ref body2._xf);
                    this.Manifold.PointCount = 0;
                }
                else
                {
                    this.Evaluate(ref this.Manifold, ref body1._xf, ref body2._xf);
                    flag2 = this.Manifold.PointCount > 0;
                    for (int index1 = 0; index1 < this.Manifold.PointCount; ++index1)
                    {
                        ManifoldPoint point1 = this.Manifold.Points[index1];
                        point1.NormalImpulse = (FP)0.0f;
                        point1.TangentImpulse = (FP)0.0f;
                        ContactID id = point1.Id;
                        for (int index2 = 0; index2 < manifold.PointCount; ++index2)
                        {
                            ManifoldPoint point2 = manifold.Points[index2];
                            if ((int)point2.Id.Key == (int)id.Key)
                            {
                                point1.NormalImpulse = point2.NormalImpulse;
                                point1.TangentImpulse = point2.TangentImpulse;
                                break;
                            }
                        }
                        this.Manifold.Points[index1] = point1;
                    }
                    if (flag2 != isTouching)
                    {
                        body1.Awake = true;
                        body2.Awake = true;
                    }
                }
                this.IsTouching = flag2;
                if (!isTouching)
                {
                    if (flag2)
                    {
                        bool flag3 = true;
                        bool flag4 = true;
                        if (this.FixtureA.OnCollision != null)
                        {
                            foreach (OnCollisionEventHandler invocation in this.FixtureA.OnCollision.GetInvocationList())
                                flag3 = invocation(this.FixtureA, this.FixtureB, this) & flag3;
                        }
                        if (this.FixtureB.OnCollision != null)
                        {
                            foreach (OnCollisionEventHandler invocation in this.FixtureB.OnCollision.GetInvocationList())
                                flag4 = invocation(this.FixtureB, this.FixtureA, this) & flag4;
                        }
                        this.Enabled = flag3 & flag4;
                        if (flag3 & flag4 && contactManager.BeginContact != null)
                            this.Enabled = contactManager.BeginContact(this);
                        if (!this.Enabled)
                            this.IsTouching = false;
                    }
                }
                else if (!flag2)
                {
                    if (this.FixtureA != null && this.FixtureA.OnSeparation != null)
                        this.FixtureA.OnSeparation(this.FixtureA, this.FixtureB);
                    if (this.FixtureB != null && this.FixtureB.OnSeparation != null)
                        this.FixtureB.OnSeparation(this.FixtureB, this.FixtureA);
                    if (contactManager.EndContact != null)
                        contactManager.EndContact(this);
                }
                else if (contactManager.StayContact != null)
                    contactManager.StayContact(this);
                if (flag1 || contactManager.PreSolve == null)
                    return;
                contactManager.PreSolve(this, ref manifold);
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
                    ((ChainShape)this.FixtureA.Shape).GetChildEdge(Contact._edge, this.ChildIndexA);
                    Collision.CollideEdgeAndPolygon(ref manifold, Contact._edge, ref transformA, (PolygonShape)this.FixtureB.Shape, ref transformB);
                    break;
                case Contact.ContactType.ChainAndCircle:
                    ((ChainShape)this.FixtureA.Shape).GetChildEdge(Contact._edge, this.ChildIndexA);
                    Collision.CollideEdgeAndCircle(ref manifold, Contact._edge, ref transformA, (CircleShape)this.FixtureB.Shape, ref transformB);
                    break;
            }
        }

        internal static Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
        {
            ShapeType shapeType1 = fixtureA.Shape.ShapeType;
            ShapeType shapeType2 = fixtureB.Shape.ShapeType;
            Debug.Assert(ShapeType.Unknown < shapeType1 && shapeType1 < ShapeType.TypeCount);
            Debug.Assert(ShapeType.Unknown < shapeType2 && shapeType2 < ShapeType.TypeCount);
            Queue<Contact> contactPool = fixtureA.Body._world._contactPool;
            Contact contact;
            if (contactPool.Count > 0)
            {
                contact = contactPool.Dequeue();
                if ((shapeType1 >= shapeType2 || shapeType1 == ShapeType.Edge && shapeType2 == ShapeType.Polygon) && (shapeType2 != ShapeType.Edge || shapeType1 != ShapeType.Polygon))
                    contact.Reset(fixtureA, indexA, fixtureB, indexB);
                else
                    contact.Reset(fixtureB, indexB, fixtureA, indexA);
            }
            else
                contact = shapeType1 < shapeType2 && (shapeType1 != ShapeType.Edge || shapeType2 != ShapeType.Polygon) || shapeType2 == ShapeType.Edge && shapeType1 == ShapeType.Polygon ? new Contact(fixtureB, indexB, fixtureA, indexA) : new Contact(fixtureA, indexA, fixtureB, indexB);
            contact._type = Contact._registers[(int)shapeType1, (int)shapeType2];
            return contact;
        }

        internal void Destroy()
        {
            this.FixtureA.Body._world._contactPool.Enqueue(this);
            if (this.Manifold.PointCount > 0 && !this.FixtureA.IsSensor && !this.FixtureB.IsSensor)
            {
                this.FixtureA.Body.Awake = true;
                this.FixtureB.Body.Awake = true;
            }
            this.Reset((Fixture)null, 0, (Fixture)null, 0);
        }

        internal enum ContactType
        {
            NotSupported,
            Polygon,
            PolygonAndCircle,
            Circle,
            EdgeAndPolygon,
            EdgeAndCircle,
            ChainAndPolygon,
            ChainAndCircle,
        }
    }
}
