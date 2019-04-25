namespace TrueSync
{
    using System;
    using TrueSync.Physics2D;

    internal class BodyClone2D
    {
        public FP _angularDamping;
        public FP _angularVelocity;
        public bool _awake = true;
        public BodyType _bodyType;
        public bool _enabled = true;
        public bool _fixedRotation;
        public TSVector2 _force;
        public FP _inertia;
        public FP _invI;
        public FP _invMass;
        public bool _island;
        public FP _linearDamping;
        public TSVector2 _linearVelocity;
        public FP _mass;
        public bool _sleepingAllowed = true;
        public FP _sleepTime;
        public Sweep _sweep = new Sweep();
        public FP _torque;
        public Transform _xf = new Transform();
        public ContactEdgeClone2D contactEdgeClone;
        public bool disabled;
        public FP GravityScale;
        public bool IgnoreCCD;
        public bool IgnoreGravity;
        public bool IsBullet;
        public GenericShapeClone2D shapeClone = new GenericShapeClone2D();

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
            if (body.ContactList == null)
            {
                this.contactEdgeClone = null;
            }
            else
            {
                this.contactEdgeClone = WorldClone2D.poolContactEdgeClone.GetNew();
                this.contactEdgeClone.Clone(body.ContactList);
            }
        }

        public void Reset()
        {
            if (this.contactEdgeClone > null)
            {
                WorldClone2D.poolContactEdgeClone.GiveBack(this.contactEdgeClone);
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

