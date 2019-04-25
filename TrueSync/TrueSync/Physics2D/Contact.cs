namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class Contact
    {
        private static EdgeShape _edge = new EdgeShape();
        internal ContactEdge _nodeA;
        internal ContactEdge _nodeB;
        private static ContactType[,] _registers;
        internal FP _toi;
        internal int _toiCount;
        internal ContactType _type;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <ChildIndexA>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <ChildIndexB>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <Enabled>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <FilterFlag>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Friction>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IslandFlag>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <IsTouching>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <Restitution>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private FP <TangentSpeed>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <TOIFlag>k__BackingField;
        public Fixture FixtureA;
        public Fixture FixtureB;
        public TrueSync.Physics2D.Manifold Manifold;

        static Contact()
        {
            ContactType[] typeArray1 = new ContactType[4, 4];
            typeArray1[0, 0] = ContactType.Circle;
            typeArray1[0, 1] = ContactType.EdgeAndCircle;
            typeArray1[0, 2] = ContactType.PolygonAndCircle;
            typeArray1[0, 3] = ContactType.ChainAndCircle;
            typeArray1[1, 0] = ContactType.EdgeAndCircle;
            typeArray1[1, 2] = ContactType.EdgeAndPolygon;
            typeArray1[2, 0] = ContactType.PolygonAndCircle;
            typeArray1[2, 1] = ContactType.EdgeAndPolygon;
            typeArray1[2, 2] = ContactType.Polygon;
            typeArray1[2, 3] = ContactType.ChainAndPolygon;
            typeArray1[3, 0] = ContactType.ChainAndCircle;
            typeArray1[3, 2] = ContactType.ChainAndPolygon;
            _registers = typeArray1;
        }

        internal Contact()
        {
            this._nodeA = new ContactEdge();
            this._nodeB = new ContactEdge();
        }

        internal Contact(Fixture fA, int indexA, Fixture fB, int indexB)
        {
            this._nodeA = new ContactEdge();
            this._nodeB = new ContactEdge();
            this.Reset(fA, indexA, fB, indexB);
        }

        public TSVector2 CalculateRelativeVelocity()
        {
            TSVector2 vector;
            vector.x = this.FixtureA.Body.LinearVelocity.x - this.FixtureB.Body.LinearVelocity.x;
            vector.y = this.FixtureA.Body.LinearVelocity.y - this.FixtureB.Body.LinearVelocity.y;
            return vector;
        }

        internal static TrueSync.Physics2D.Contact Create(Fixture fixtureA, int indexA, Fixture fixtureB, int indexB)
        {
            TrueSync.Physics2D.Contact contact;
            ShapeType shapeType = fixtureA.Shape.ShapeType;
            ShapeType type2 = fixtureB.Shape.ShapeType;
            Debug.Assert((ShapeType.Unknown < shapeType) && (shapeType < ShapeType.TypeCount));
            Debug.Assert((ShapeType.Unknown < type2) && (type2 < ShapeType.TypeCount));
            Queue<TrueSync.Physics2D.Contact> queue = fixtureA.Body._world._contactPool;
            if (queue.Count > 0)
            {
                contact = queue.Dequeue();
                if (((shapeType >= type2) || ((shapeType == ShapeType.Edge) && (type2 == ShapeType.Polygon))) && ((type2 != ShapeType.Edge) || (shapeType != ShapeType.Polygon)))
                {
                    contact.Reset(fixtureA, indexA, fixtureB, indexB);
                }
                else
                {
                    contact.Reset(fixtureB, indexB, fixtureA, indexA);
                }
            }
            else if (((shapeType >= type2) || ((shapeType == ShapeType.Edge) && (type2 == ShapeType.Polygon))) && ((type2 != ShapeType.Edge) || (shapeType != ShapeType.Polygon)))
            {
                contact = new TrueSync.Physics2D.Contact(fixtureA, indexA, fixtureB, indexB);
            }
            else
            {
                contact = new TrueSync.Physics2D.Contact(fixtureB, indexB, fixtureA, indexA);
            }
            contact._type = _registers[(int) shapeType, (int) type2];
            return contact;
        }

        internal void Destroy()
        {
            this.FixtureA.Body._world._contactPool.Enqueue(this);
            if (((this.Manifold.PointCount > 0) && !this.FixtureA.IsSensor) && !this.FixtureB.IsSensor)
            {
                this.FixtureA.Body.Awake = true;
                this.FixtureB.Body.Awake = true;
            }
            this.Reset(null, 0, null, 0);
        }

        private void Evaluate(ref TrueSync.Physics2D.Manifold manifold, ref Transform transformA, ref Transform transformB)
        {
            switch (this._type)
            {
                case ContactType.Polygon:
                    Collision.CollidePolygons(ref manifold, (PolygonShape) this.FixtureA.Shape, ref transformA, (PolygonShape) this.FixtureB.Shape, ref transformB);
                    break;

                case ContactType.PolygonAndCircle:
                    Collision.CollidePolygonAndCircle(ref manifold, (PolygonShape) this.FixtureA.Shape, ref transformA, (CircleShape) this.FixtureB.Shape, ref transformB);
                    break;

                case ContactType.Circle:
                    Collision.CollideCircles(ref manifold, (CircleShape) this.FixtureA.Shape, ref transformA, (CircleShape) this.FixtureB.Shape, ref transformB);
                    break;

                case ContactType.EdgeAndPolygon:
                    Collision.CollideEdgeAndPolygon(ref manifold, (EdgeShape) this.FixtureA.Shape, ref transformA, (PolygonShape) this.FixtureB.Shape, ref transformB);
                    break;

                case ContactType.EdgeAndCircle:
                    Collision.CollideEdgeAndCircle(ref manifold, (EdgeShape) this.FixtureA.Shape, ref transformA, (CircleShape) this.FixtureB.Shape, ref transformB);
                    break;

                case ContactType.ChainAndPolygon:
                    ((ChainShape) this.FixtureA.Shape).GetChildEdge(_edge, this.ChildIndexA);
                    Collision.CollideEdgeAndPolygon(ref manifold, _edge, ref transformA, (PolygonShape) this.FixtureB.Shape, ref transformB);
                    break;

                case ContactType.ChainAndCircle:
                    ((ChainShape) this.FixtureA.Shape).GetChildEdge(_edge, this.ChildIndexA);
                    Collision.CollideEdgeAndCircle(ref manifold, _edge, ref transformA, (CircleShape) this.FixtureB.Shape, ref transformB);
                    break;
            }
        }

        public void GetWorldManifold(out TSVector2 normal, out FixedArray2<TSVector2> points)
        {
            Body body = this.FixtureA.Body;
            Body body2 = this.FixtureB.Body;
            TrueSync.Physics2D.Shape shape = this.FixtureA.Shape;
            TrueSync.Physics2D.Shape shape2 = this.FixtureB.Shape;
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
            if ((this.FixtureA != null) && (this.FixtureB > null))
            {
                this.Friction = Settings.MixFriction(this.FixtureA.Friction, this.FixtureB.Friction);
                this.Restitution = Settings.MixRestitution(this.FixtureA.Restitution, this.FixtureB.Restitution);
            }
            this.TangentSpeed = 0;
        }

        public void ResetFriction()
        {
            this.Friction = Settings.MixFriction(this.FixtureA.Friction, this.FixtureB.Friction);
        }

        public void ResetRestitution()
        {
            this.Restitution = Settings.MixRestitution(this.FixtureA.Restitution, this.FixtureB.Restitution);
        }

        internal void Update(ContactManager contactManager)
        {
            Body body = this.FixtureA.Body;
            Body body2 = this.FixtureB.Body;
            if ((this.FixtureA != null) && (this.FixtureB != null))
            {
                if (!ContactManager.CheckCollisionConditions(this.FixtureA, this.FixtureB))
                {
                    this.Enabled = false;
                }
                else
                {
                    bool flag;
                    TrueSync.Physics2D.Manifold oldManifold = this.Manifold;
                    this.Enabled = true;
                    bool isTouching = this.IsTouching;
                    bool flag3 = this.FixtureA.IsSensor || this.FixtureB.IsSensor;
                    if (flag3)
                    {
                        TrueSync.Physics2D.Shape shapeA = this.FixtureA.Shape;
                        TrueSync.Physics2D.Shape shape = this.FixtureB.Shape;
                        flag = Collision.TestOverlap(shapeA, this.ChildIndexA, shape, this.ChildIndexB, ref body._xf, ref body2._xf);
                        this.Manifold.PointCount = 0;
                    }
                    else
                    {
                        this.Evaluate(ref this.Manifold, ref body._xf, ref body2._xf);
                        flag = this.Manifold.PointCount > 0;
                        for (int i = 0; i < this.Manifold.PointCount; i++)
                        {
                            ManifoldPoint point = this.Manifold.Points[i];
                            point.NormalImpulse = 0f;
                            point.TangentImpulse = 0f;
                            ContactID id = point.Id;
                            for (int j = 0; j < oldManifold.PointCount; j++)
                            {
                                ManifoldPoint point2 = oldManifold.Points[j];
                                if (point2.Id.Key == id.Key)
                                {
                                    point.NormalImpulse = point2.NormalImpulse;
                                    point.TangentImpulse = point2.TangentImpulse;
                                    break;
                                }
                            }
                            this.Manifold.Points[i] = point;
                        }
                        if (flag != isTouching)
                        {
                            body.Awake = true;
                            body2.Awake = true;
                        }
                    }
                    this.IsTouching = flag;
                    if (!isTouching)
                    {
                        if (flag)
                        {
                            bool flag14 = true;
                            bool flag15 = true;
                            if (this.FixtureA.OnCollision > null)
                            {
                                foreach (OnCollisionEventHandler handler in this.FixtureA.OnCollision.GetInvocationList())
                                {
                                    flag14 = handler(this.FixtureA, this.FixtureB, this) & flag14;
                                }
                            }
                            if (this.FixtureB.OnCollision > null)
                            {
                                foreach (OnCollisionEventHandler handler2 in this.FixtureB.OnCollision.GetInvocationList())
                                {
                                    flag15 = handler2(this.FixtureB, this.FixtureA, this) & flag15;
                                }
                            }
                            this.Enabled = flag14 & flag15;
                            if ((flag14 & flag15) && (contactManager.BeginContact > null))
                            {
                                this.Enabled = contactManager.BeginContact(this);
                            }
                            if (!this.Enabled)
                            {
                                this.IsTouching = false;
                            }
                        }
                    }
                    else if (!flag)
                    {
                        if ((this.FixtureA != null) && (this.FixtureA.OnSeparation > null))
                        {
                            this.FixtureA.OnSeparation(this.FixtureA, this.FixtureB);
                        }
                        if ((this.FixtureB != null) && (this.FixtureB.OnSeparation > null))
                        {
                            this.FixtureB.OnSeparation(this.FixtureB, this.FixtureA);
                        }
                        if (contactManager.EndContact > null)
                        {
                            contactManager.EndContact(this);
                        }
                    }
                    else if (contactManager.StayContact > null)
                    {
                        contactManager.StayContact(this);
                    }
                    if (!flag3 && (contactManager.PreSolve > null))
                    {
                        contactManager.PreSolve(this, ref oldManifold);
                    }
                }
            }
        }

        public int ChildIndexA { get; internal set; }

        public int ChildIndexB { get; internal set; }

        public bool Enabled { get; set; }

        internal bool FilterFlag { get; set; }

        public FP Friction { get; set; }

        internal bool IslandFlag { get; set; }

        public bool IsTouching { get; set; }

        internal string Key
        {
            get
            {
                return (this.FixtureA.FixtureId + "_" + this.FixtureB.FixtureId);
            }
        }

        public FP Restitution { get; set; }

        public FP TangentSpeed { get; set; }

        internal bool TOIFlag { get; set; }

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
    }
}

