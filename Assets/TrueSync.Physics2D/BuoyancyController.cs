using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public sealed class BuoyancyController : Controller
	{
		public FP AngularDragCoefficient;

		public FP Density;

		public FP LinearDragCoefficient;

		public TSVector2 Velocity;

		private AABB _container;

		private TSVector2 _gravity;

		private TSVector2 _normal;

		private FP _offset;

		private Dictionary<int, Body> _uniqueBodies = new Dictionary<int, Body>();

		public AABB Container
		{
			get
			{
				return this._container;
			}
			set
			{
				this._container = value;
				this._offset = this._container.UpperBound.y;
			}
		}

		public BuoyancyController(AABB container, FP density, FP linearDragCoefficient, FP rotationalDragCoefficient, TSVector2 gravity) : base(ControllerType.BuoyancyController)
		{
			this.Container = container;
			this._normal = new TSVector2(0, 1);
			this.Density = density;
			this.LinearDragCoefficient = linearDragCoefficient;
			this.AngularDragCoefficient = rotationalDragCoefficient;
			this._gravity = gravity;
		}

		public override void Update(FP dt)
		{
			this._uniqueBodies.Clear();
			this.World.QueryAABB(delegate(Fixture fixture)
			{
				bool flag3 = fixture.Body.IsStatic || !fixture.Body.Awake;
				bool result;
				if (flag3)
				{
					result = true;
				}
				else
				{
					bool flag4 = !this._uniqueBodies.ContainsKey(fixture.Body.BodyId);
					if (flag4)
					{
						this._uniqueBodies.Add(fixture.Body.BodyId, fixture.Body);
					}
					result = true;
				}
				return result;
			}, ref this._container);
			foreach (KeyValuePair<int, Body> current in this._uniqueBodies)
			{
				Body value = current.Value;
				TSVector2 zero = TSVector2.zero;
				TSVector2 zero2 = TSVector2.zero;
				FP fP = 0;
				FP fP2 = 0;
				for (int i = 0; i < value.FixtureList.Count; i++)
				{
					Fixture fixture2 = value.FixtureList[i];
					bool flag = fixture2.Shape.ShapeType != ShapeType.Polygon && fixture2.Shape.ShapeType > ShapeType.Circle;
					if (!flag)
					{
						Shape shape = fixture2.Shape;
						TSVector2 tSVector;
						FP fP3 = shape.ComputeSubmergedArea(ref this._normal, this._offset, ref value._xf, out tSVector);
						fP += fP3;
						zero.x += fP3 * tSVector.x;
						zero.y += fP3 * tSVector.y;
						fP2 += fP3 * shape.Density;
						zero2.x += fP3 * tSVector.x * shape.Density;
						zero2.y += fP3 * tSVector.y * shape.Density;
					}
				}
				zero.x /= fP;
				zero.y /= fP;
				zero2.x /= fP2;
				zero2.y /= fP2;
				bool flag2 = fP < Settings.Epsilon;
				if (!flag2)
				{
					TSVector2 force = -this.Density * fP * this._gravity;
					value.ApplyForce(force, zero2);
					TSVector2 tSVector2 = value.GetLinearVelocityFromWorldPoint(zero) - this.Velocity;
					tSVector2 *= -this.LinearDragCoefficient * fP;
					value.ApplyForce(tSVector2, zero);
					value.ApplyTorque(-value.Inertia / value.Mass * fP * value.AngularVelocity * this.AngularDragCoefficient);
				}
			}
		}
	}
}
