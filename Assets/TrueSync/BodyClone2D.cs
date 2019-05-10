using System;
using TrueSync.Physics2D;

namespace TrueSync
{
	internal class BodyClone2D
	{
		public FP _angularDamping;

		public BodyType _bodyType;

		public FP _inertia;

		public FP _linearDamping;

		public FP _mass;

		public bool _sleepingAllowed = true;

		public bool _awake = true;

		public bool _fixedRotation;

		public bool _enabled = true;

		public FP _angularVelocity;

		public TSVector2 _linearVelocity;

		public TSVector2 _force;

		public FP _invI;

		public FP _invMass;

		public FP _sleepTime;

		public Sweep _sweep = default(Sweep);

		public FP _torque;

		public Transform _xf = default(Transform);

		public bool _island;

		public bool disabled;

		public FP GravityScale;

		public bool IsBullet;

		public bool IgnoreCCD;

		public bool IgnoreGravity;

		public ContactEdgeClone2D contactEdgeClone;

		public GenericShapeClone2D shapeClone = new GenericShapeClone2D();

		public void Reset()
		{
			bool flag = this.contactEdgeClone != null;
			if (flag)
			{
				WorldClone2D.poolContactEdgeClone.GiveBack(this.contactEdgeClone);
			}
		}

		public void Clone(Body body)
		{
			this._angularDamping = body._angularDamping;
			this._bodyType = body._bodyType;
			this._inertia = body._inertia;
			this._linearDamping = body._linearDamping;
			this._mass = body._mass;
			this._sleepingAllowed = body._sleepingAllowed;
			this._awake = body._awake;
			this._fixedRotation = body._fixedRotation;
			this._enabled = body._enabled;
			this._angularVelocity = body._angularVelocity;
			this._linearVelocity = body._linearVelocity;
			this._force = body._force;
			this._invI = body._invI;
			this._invMass = body._invMass;
			this._sleepTime = body._sleepTime;
			this._sweep.A = body._sweep.A;
			this._sweep.A0 = body._sweep.A0;
			this._sweep.Alpha0 = body._sweep.Alpha0;
			this._sweep.C = body._sweep.C;
			this._sweep.C0 = body._sweep.C0;
			this._sweep.LocalCenter = body._sweep.LocalCenter;
			this._torque = body._torque;
			this._xf.p = body._xf.p;
			this._xf.q = body._xf.q;
			this._island = body._island;
			this.disabled = body.disabled;
			this.GravityScale = body.GravityScale;
			this.IsBullet = body.IsBullet;
			this.IgnoreCCD = body.IgnoreCCD;
			this.IgnoreGravity = body.IgnoreGravity;
			this.shapeClone.Clone(body.FixtureList[0].Shape);
			bool flag = body.ContactList == null;
			if (flag)
			{
				this.contactEdgeClone = null;
			}
			else
			{
				this.contactEdgeClone = WorldClone2D.poolContactEdgeClone.GetNew();
				this.contactEdgeClone.Clone(body.ContactList);
			}
		}

		public void Restore(Body body)
		{
			body._angularDamping = this._angularDamping;
			body._bodyType = this._bodyType;
			body._inertia = this._inertia;
			body._linearDamping = this._linearDamping;
			body._mass = this._mass;
			body._sleepingAllowed = this._sleepingAllowed;
			body._awake = this._awake;
			body._fixedRotation = this._fixedRotation;
			body._enabled = this._enabled;
			body._angularVelocity = this._angularVelocity;
			body._linearVelocity = this._linearVelocity;
			body._force = this._force;
			body._invI = this._invI;
			body._invMass = this._invMass;
			body._sleepTime = this._sleepTime;
			body._sweep = this._sweep;
			body._torque = this._torque;
			body._xf.p = this._xf.p;
			body._xf.q = this._xf.q;
			body._island = this._island;
			body.disabled = this.disabled;
			body.GravityScale = body.GravityScale;
			body.IsBullet = body.IsBullet;
			body.IgnoreCCD = body.IgnoreCCD;
			body.IgnoreGravity = body.IgnoreGravity;
			this.shapeClone.Restore(body.FixtureList[0].Shape);
		}
	}
}
