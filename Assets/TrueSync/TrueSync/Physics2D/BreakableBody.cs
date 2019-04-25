namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    public class BreakableBody
    {
        private FP[] _angularVelocitiesCache;
        private bool _break;
        private TSVector2[] _velocitiesCache;
        private TrueSync.Physics2D.World _world;
        public bool Broken;
        public Body MainBody;
        public List<Fixture> Parts;
        public FP Strength;

        public BreakableBody(IEnumerable<TrueSync.Physics2D.Shape> shapes, TrueSync.Physics2D.World world)
        {
            this._angularVelocitiesCache = new FP[8];
            this._velocitiesCache = new TSVector2[8];
            this.Parts = new List<Fixture>(8);
            this.Strength = 500f;
            this._world = world;
            ContactManager contactManager = this._world.ContactManager;
            contactManager.PostSolve = (PostSolveDelegate) Delegate.Combine(contactManager.PostSolve, new PostSolveDelegate(this.PostSolve));
            this.MainBody = new Body(this._world, null, 0, null);
            this.MainBody.BodyType = BodyType.Dynamic;
            foreach (TrueSync.Physics2D.Shape shape in shapes)
            {
                Fixture item = this.MainBody.CreateFixture(shape, null);
                this.Parts.Add(item);
            }
        }

        public BreakableBody(IEnumerable<Vertices> vertices, TrueSync.Physics2D.World world, FP density)
        {
            this._angularVelocitiesCache = new FP[8];
            this._velocitiesCache = new TSVector2[8];
            this.Parts = new List<Fixture>(8);
            this.Strength = 500f;
            this._world = world;
            ContactManager contactManager = this._world.ContactManager;
            contactManager.PostSolve = (PostSolveDelegate) Delegate.Combine(contactManager.PostSolve, new PostSolveDelegate(this.PostSolve));
            this.MainBody = new Body(this._world, null, 0, null);
            this.MainBody.BodyType = BodyType.Dynamic;
            foreach (Vertices vertices2 in vertices)
            {
                PolygonShape shape = new PolygonShape(vertices2, density);
                Fixture item = this.MainBody.CreateFixture(shape, null);
                this.Parts.Add(item);
            }
        }

        public void Break()
        {
            this._break = true;
        }

        private void Decompose()
        {
            ContactManager contactManager = this._world.ContactManager;
            contactManager.PostSolve = (PostSolveDelegate) Delegate.Remove(contactManager.PostSolve, new PostSolveDelegate(this.PostSolve));
            for (int i = 0; i < this.Parts.Count; i++)
            {
                Fixture fixture = this.Parts[i];
                TrueSync.Physics2D.Shape shape = fixture.Shape.Clone();
                object userData = fixture.UserData;
                this.MainBody.DestroyFixture(fixture);
                Body body = BodyFactory.CreateBody(this._world, null);
                body.BodyType = BodyType.Dynamic;
                body.Position = this.MainBody.Position;
                body.Rotation = this.MainBody.Rotation;
                body.UserData = this.MainBody.UserData;
                Fixture fixture2 = body.CreateFixture(shape, null);
                fixture2.UserData = userData;
                this.Parts[i] = fixture2;
                body.AngularVelocity = this._angularVelocitiesCache[i];
                body.LinearVelocity = this._velocitiesCache[i];
            }
            this._world.RemoveBody(this.MainBody);
            this._world.RemoveBreakableBody(this);
        }

        private void PostSolve(TrueSync.Physics2D.Contact contact, ContactVelocityConstraint impulse)
        {
            if (!this.Broken && (this.Parts.Contains(contact.FixtureA) || this.Parts.Contains(contact.FixtureB)))
            {
                FP fp = 0f;
                int pointCount = contact.Manifold.PointCount;
                for (int i = 0; i < pointCount; i++)
                {
                    fp = TSMath.Max(fp, impulse.points[i].normalImpulse);
                }
                if (fp > this.Strength)
                {
                    this._break = true;
                }
            }
        }

        public void Update()
        {
            if (this._break)
            {
                this.Decompose();
                this.Broken = true;
                this._break = false;
            }
            if (!this.Broken)
            {
                if (this.Parts.Count > this._angularVelocitiesCache.Length)
                {
                    this._velocitiesCache = new TSVector2[this.Parts.Count];
                    this._angularVelocitiesCache = new FP[this.Parts.Count];
                }
                for (int i = 0; i < this.Parts.Count; i++)
                {
                    this._velocitiesCache[i] = this.Parts[i].Body.LinearVelocity;
                    this._angularVelocitiesCache[i] = this.Parts[i].Body.AngularVelocity;
                }
            }
        }
    }
}

