namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    public sealed class BuoyancyController : Controller
    {
        private AABB _container;
        private TSVector2 _gravity;
        private TSVector2 _normal;
        private FP _offset;
        private Dictionary<int, Body> _uniqueBodies;
        public FP AngularDragCoefficient;
        public FP Density;
        public FP LinearDragCoefficient;
        public TSVector2 Velocity;

        public BuoyancyController(AABB container, FP density, FP linearDragCoefficient, FP rotationalDragCoefficient, TSVector2 gravity) : base(ControllerType.BuoyancyController)
        {
            this._uniqueBodies = new Dictionary<int, Body>();
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
            base.World.QueryAABB(delegate (Fixture fixture) {
                if ((!fixture.Body.IsStatic && fixture.Body.Awake) && !this._uniqueBodies.ContainsKey(fixture.Body.BodyId))
                {
                    this._uniqueBodies.Add(fixture.Body.BodyId, fixture.Body);
                }
                return true;
            }, ref this._container);
            foreach (KeyValuePair<int, Body> pair in this._uniqueBodies)
            {
                Body body = pair.Value;
                TSVector2 zero = TSVector2.zero;
                TSVector2 point = TSVector2.zero;
                FP fp = 0;
                FP fp2 = 0;
                for (int i = 0; i < body.FixtureList.Count; i++)
                {
                    Fixture fixture = body.FixtureList[i];
                    if ((fixture.Shape.ShapeType == ShapeType.Polygon) || (fixture.Shape.ShapeType <= ShapeType.Circle))
                    {
                        TSVector2 vector5;
                        TrueSync.Physics2D.Shape shape = fixture.Shape;
                        FP fp3 = shape.ComputeSubmergedArea(ref this._normal, this._offset, ref body._xf, out vector5);
                        fp += fp3;
                        zero.x += fp3 * vector5.x;
                        zero.y += fp3 * vector5.y;
                        fp2 += fp3 * shape.Density;
                        point.x += (fp3 * vector5.x) * shape.Density;
                        point.y += (fp3 * vector5.y) * shape.Density;
                    }
                }
                zero.x /= fp;
                zero.y /= fp;
                point.x /= fp2;
                point.y /= fp2;
                if (fp >= Settings.Epsilon)
                {
                    TSVector2 force = (TSVector2) ((-this.Density * fp) * this._gravity);
                    body.ApplyForce(force, point);
                    TSVector2 vector4 = body.GetLinearVelocityFromWorldPoint(zero) - this.Velocity;
                    vector4 *= -this.LinearDragCoefficient * fp;
                    body.ApplyForce(vector4, zero);
                    body.ApplyTorque((((-body.Inertia / body.Mass) * fp) * body.AngularVelocity) * this.AngularDragCoefficient);
                }
            }
        }

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
    }
}

