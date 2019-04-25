namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class RigidBodyClone
    {
        public TSVector _freezePosition;
        public TSMatrix _freezeRotation;
        public TSQuaternion _freezeRotationQuat;
        public bool affectedByGravity;
        public TSVector angularVelocity;
        public TSBBox boundingBox;
        public List<RigidBody> connections = new List<RigidBody>();
        public List<Constraint> constraints = new List<Constraint>();
        public bool disabled;
        public TSVector force;
        public TSRigidBodyConstraints freezeConstraint;
        public FP inactiveTime;
        private int index;
        public TSMatrix inertia;
        public int internalIndex;
        public FP inverseMass;
        public TSMatrix invInertia;
        public TSMatrix invInertiaWorld;
        public TSMatrix invOrientation;
        public bool isActive;
        public bool isColliderOnly;
        public bool isKinematic;
        public bool isStatic;
        private int length;
        public TSVector linearVelocity;
        public int marker;
        public TSMatrix orientation;
        public static ResourcePoolGenericShapeClone poolGenericShapeClone = new ResourcePoolGenericShapeClone();
        public TSVector position;
        public GenericShapeClone shapeClone;
        public TSVector sweptDirection;
        public TSVector torque;

        public void Clone(RigidBody rb)
        {
            this.marker = rb.marker;
            this.affectedByGravity = rb.affectedByGravity;
            this.boundingBox = rb.boundingBox;
            this.internalIndex = rb.internalIndex;
            this.inverseMass = rb.inverseMass;
            this.isColliderOnly = rb.isColliderOnly;
            this.isStatic = rb.isStatic;
            this.isKinematic = rb.isKinematic;
            this.sweptDirection = rb.sweptDirection;
            this.position = rb.Position;
            this.orientation = rb.Orientation;
            this.linearVelocity = rb.LinearVelocity;
            this.angularVelocity = rb.AngularVelocity;
            this.inertia = rb.Inertia;
            this.invInertia = rb.InverseInertia;
            this.invInertiaWorld = rb.InverseInertiaWorld;
            this.invOrientation = rb.invOrientation;
            this.force = rb.Force;
            this.torque = rb.Torque;
            this.shapeClone = poolGenericShapeClone.GetNew();
            this.shapeClone.Clone(rb.Shape);
            this.connections.Clear();
            this.index = 0;
            this.length = rb.connections.Count;
            while (this.index < this.length)
            {
                this.connections.Add(rb.connections[this.index]);
                this.index++;
            }
            this.constraints.Clear();
            this.index = 0;
            this.length = rb.constraints.Count;
            while (this.index < this.length)
            {
                this.constraints.Add(rb.constraints[this.index]);
                this.index++;
            }
            this.isActive = rb.IsActive;
            this.inactiveTime = rb.inactiveTime;
            this.marker = rb.marker;
            this.disabled = rb.disabled;
            this.freezeConstraint = rb._freezeConstraints;
            this._freezePosition = rb._freezePosition;
            this._freezeRotation = rb._freezeRotation;
            this._freezeRotationQuat = rb._freezeRotationQuat;
        }

        public void Reset()
        {
            if (this.shapeClone > null)
            {
                poolGenericShapeClone.GiveBack(this.shapeClone);
            }
        }

        public void Restore(World world, RigidBody rb)
        {
            rb.marker = this.marker;
            rb.affectedByGravity = this.affectedByGravity;
            rb.boundingBox = this.boundingBox;
            rb.internalIndex = this.internalIndex;
            rb.inverseMass = this.inverseMass;
            rb.isColliderOnly = this.isColliderOnly;
            rb.isStatic = this.isStatic;
            rb.isKinematic = this.isKinematic;
            rb.sweptDirection = this.sweptDirection;
            rb.position = this.position;
            rb.orientation = this.orientation;
            rb.inertia = this.inertia;
            rb.invInertia = this.invInertia;
            rb.invInertiaWorld = this.invInertiaWorld;
            rb.invOrientation = this.invOrientation;
            rb.force = this.force;
            rb.torque = this.torque;
            this.shapeClone.Restore(rb.Shape);
            rb.connections.Clear();
            rb.connections.AddRange(this.connections);
            rb.arbiters.Clear();
            rb.constraints.Clear();
            rb.constraints.AddRange(this.constraints);
            rb.isActive = this.isActive;
            rb.inactiveTime = this.inactiveTime;
            rb.marker = this.marker;
            bool disabled = rb.disabled;
            rb.disabled = this.disabled;
            rb._freezeConstraints = this.freezeConstraint;
            rb._freezePosition = this._freezePosition;
            rb._freezeRotation = this._freezeRotation;
            rb._freezeRotationQuat = this._freezeRotationQuat;
            if (disabled && !rb.disabled)
            {
                world.physicsManager.GetGameObject(rb).SetActive(true);
            }
            if (!rb.IsStatic)
            {
                rb.linearVelocity = this.linearVelocity;
                rb.angularVelocity = this.angularVelocity;
            }
        }
    }
}

