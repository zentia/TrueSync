using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public class BreakableBody
	{
		private FP[] _angularVelocitiesCache = new FP[8];

		private bool _break;

		private TSVector2[] _velocitiesCache = new TSVector2[8];

		private World _world;

		public bool Broken;

		public Body MainBody;

		public List<Fixture> Parts = new List<Fixture>(8);

		public FP Strength = 500f;

		public BreakableBody(IEnumerable<Vertices> vertices, World world, FP density)
		{
			this._world = world;
			ContactManager expr_4E = this._world.ContactManager;
			expr_4E.PostSolve = (PostSolveDelegate)Delegate.Combine(expr_4E.PostSolve, new PostSolveDelegate(this.PostSolve));
			this.MainBody = new Body(this._world, null, 0, null);
			this.MainBody.BodyType = BodyType.Dynamic;
			foreach (Vertices current in vertices)
			{
				PolygonShape shape = new PolygonShape(current, density);
				Fixture item = this.MainBody.CreateFixture(shape, null);
				this.Parts.Add(item);
			}
		}

		public BreakableBody(IEnumerable<Shape> shapes, World world)
		{
			this._world = world;
			ContactManager expr_4E = this._world.ContactManager;
			expr_4E.PostSolve = (PostSolveDelegate)Delegate.Combine(expr_4E.PostSolve, new PostSolveDelegate(this.PostSolve));
			this.MainBody = new Body(this._world, null, 0, null);
			this.MainBody.BodyType = BodyType.Dynamic;
			foreach (Shape current in shapes)
			{
				Fixture item = this.MainBody.CreateFixture(current, null);
				this.Parts.Add(item);
			}
		}

		private void PostSolve(Contact contact, ContactVelocityConstraint impulse)
		{
			bool flag = !this.Broken;
			if (flag)
			{
				bool flag2 = this.Parts.Contains(contact.FixtureA) || this.Parts.Contains(contact.FixtureB);
				if (flag2)
				{
					FP fP = 0f;
					int pointCount = contact.Manifold.PointCount;
					for (int i = 0; i < pointCount; i++)
					{
						fP = TSMath.Max(fP, impulse.points[i].normalImpulse);
					}
					bool flag3 = fP > this.Strength;
					if (flag3)
					{
						this._break = true;
					}
				}
			}
		}

		public void Update()
		{
			bool @break = this._break;
			if (@break)
			{
				this.Decompose();
				this.Broken = true;
				this._break = false;
			}
			bool flag = !this.Broken;
			if (flag)
			{
				bool flag2 = this.Parts.Count > this._angularVelocitiesCache.Length;
				if (flag2)
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

		private void Decompose()
		{
			ContactManager expr_0C = this._world.ContactManager;
			expr_0C.PostSolve = (PostSolveDelegate)Delegate.Remove(expr_0C.PostSolve, new PostSolveDelegate(this.PostSolve));
			for (int i = 0; i < this.Parts.Count; i++)
			{
				Fixture fixture = this.Parts[i];
				Shape shape = fixture.Shape.Clone();
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

		public void Break()
		{
			this._break = true;
		}
	}
}
