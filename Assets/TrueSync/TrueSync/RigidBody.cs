// Decompiled with JetBrains decompiler
// Type: TrueSync.RigidBody
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections.Generic;

namespace TrueSync
{
    public class RigidBody : IBroadphaseEntity, IDebugDrawable, IEquatable<RigidBody>, IComparable, IBody3D, IBody
    {
        internal static int instanceCount = 0;
        internal FP inactiveTime = FP.Zero;
        internal bool isActive = true;
        internal bool isStatic = false;
        internal bool isKinematic = false;
        internal bool affectedByGravity = true;
        internal bool isColliderOnly = false;
        internal int internalIndex = 0;
        internal List<RigidBody> connections = new List<RigidBody>();
        internal HashList<Arbiter> arbiters = new HashList<Arbiter>();
        internal HashList<Arbiter> arbitersTrigger = new HashList<Arbiter>();
        internal HashList<Constraint> constraints = new HashList<Constraint>();
        internal int marker = 0;
        internal bool disabled = false;
        internal TSRigidBodyConstraints _freezeConstraints = TSRigidBodyConstraints.None;
        internal TSVector _freezePosition = TSVector.zero;
        internal TSMatrix _freezeRotation = TSMatrix.Identity;
        internal TSQuaternion _freezeRotationQuat = TSQuaternion.identity;
        internal bool isParticle = false;
        protected bool useShapeMassProperties = true;
        private RigidBody.DampingType damping = RigidBody.DampingType.Angular | RigidBody.DampingType.Linear;
        internal TSVector sweptDirection = TSVector.zero;
        private bool enableDebugDraw = false;
        private List<TSVector> hullPoints = new List<TSVector>();
        internal TSMatrix inertia;
        internal TSMatrix invInertia;
        internal TSMatrix invInertiaWorld;
        internal TSMatrix orientation;
        internal TSMatrix invOrientation;
        internal TSVector position;
        internal TSVector linearVelocity;
        internal TSVector angularVelocity;
        internal BodyMaterial material;
        internal TSBBox boundingBox;
        internal CollisionIsland island;
        internal FP inverseMass;
        internal TSVector force;
        internal TSVector torque;
        private int hashCode;
        private ShapeUpdatedHandler updatedHandler;
        private ReadOnlyHashset<Arbiter> readOnlyArbiters;
        private ReadOnlyHashset<Constraint> readOnlyConstraints;
        private int instance;
        internal Shape shape;

        public TSRigidBodyConstraints FreezeConstraints
        {
            get
            {
                return this._freezeConstraints;
            }
            set
            {
                if (this._freezeConstraints == value)
                    return;
                this._freezeConstraints = value;
                this._freezePosition = this.position;
                if (this._freezeRotation != this.orientation)
                {
                    this._freezeRotation = this.orientation;
                    this._freezeRotationQuat = TSQuaternion.CreateFromMatrix(this._freezeRotation);
                }
            }
        }

        public bool Disabled
        {
            get
            {
                return this.disabled;
            }
        }

        public RigidBody(Shape shape)
          : this(shape, new BodyMaterial(), false)
        {
        }

        public bool IsParticle
        {
            get
            {
                return this.isParticle;
            }
            set
            {
                if (this.isParticle && !value)
                {
                    this.updatedHandler = new ShapeUpdatedHandler(this.ShapeUpdated);
                    this.Shape.ShapeUpdated += this.updatedHandler;
                    this.SetMassProperties();
                    this.isParticle = false;
                }
                else if (!this.isParticle & value)
                {
                    this.inertia = TSMatrix.Zero;
                    this.invInertia = this.invInertiaWorld = TSMatrix.Zero;
                    this.invOrientation = this.orientation = TSMatrix.Identity;
                    this.inverseMass = FP.One;
                    this.Shape.ShapeUpdated -= this.updatedHandler;
                    this.isParticle = true;
                }
                this.Update();
            }
        }

        public RigidBody(Shape shape, BodyMaterial material)
          : this(shape, material, false)
        {
        }

        public RigidBody(Shape shape, BodyMaterial material, bool isParticle)
        {
            this.readOnlyArbiters = new ReadOnlyHashset<Arbiter>(this.arbiters);
            this.readOnlyConstraints = new ReadOnlyHashset<Constraint>(this.constraints);
            ++RigidBody.instanceCount;
            this.instance = RigidBody.instanceCount;
            this.hashCode = this.CalculateHash(this.instance);
            this.Shape = shape;
            this.orientation = TSMatrix.Identity;
            if (!isParticle)
            {
                this.updatedHandler = new ShapeUpdatedHandler(this.ShapeUpdated);
                this.Shape.ShapeUpdated += this.updatedHandler;
                this.SetMassProperties();
            }
            else
            {
                this.inertia = TSMatrix.Zero;
                this.invInertia = this.invInertiaWorld = TSMatrix.Zero;
                this.invOrientation = this.orientation = TSMatrix.Identity;
                this.inverseMass = FP.One;
            }
            this.material = material;
            this.AllowDeactivation = true;
            this.EnableSpeculativeContacts = false;
            this.isParticle = isParticle;
            this.Update();
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        internal ReadOnlyHashset<Arbiter> Arbiters
        {
            get
            {
                return this.readOnlyArbiters;
            }
        }

        internal ReadOnlyHashset<Constraint> Constraints
        {
            get
            {
                return this.readOnlyConstraints;
            }
        }

        internal bool AllowDeactivation { get; set; }

        internal bool EnableSpeculativeContacts { get; set; }

        public TSBBox BoundingBox
        {
            get
            {
                return this.boundingBox;
            }
        }

        private int CalculateHash(int a)
        {
            a = a ^ 61 ^ a >> 16;
            a += a << 3;
            a ^= a >> 4;
            a *= 668265261;
            a ^= a >> 15;
            return a;
        }

        internal CollisionIsland CollisionIsland
        {
            get
            {
                return this.island;
            }
        }

        public bool IsActive
        {
            get
            {
                return this.isActive;
            }
            set
            {
                if (!this.isActive & value)
                    this.inactiveTime = FP.Zero;
                else if (this.isActive && !value)
                {
                    this.inactiveTime = FP.PositiveInfinity;
                    this.angularVelocity.MakeZero();
                    this.linearVelocity.MakeZero();
                }
                this.isActive = value;
            }
        }

        public bool IsColliderOnly
        {
            get
            {
                return this.isColliderOnly;
            }
            set
            {
                this.isColliderOnly = value;
            }
        }

        public void ApplyImpulse(TSVector impulse)
        {
            if (this.isStatic)
                throw new InvalidOperationException("Can't apply an impulse to a static body.");
            TSVector result;
            TSVector.Multiply(ref impulse, this.inverseMass, out result);
            TSVector.Add(ref this.linearVelocity, ref result, out this.linearVelocity);
        }

        public void ApplyImpulse(TSVector impulse, TSVector relativePosition)
        {
            if (this.isStatic)
                throw new InvalidOperationException("Can't apply an impulse to a static body.");
            TSVector result;
            TSVector.Multiply(ref impulse, this.inverseMass, out result);
            TSVector.Add(ref this.linearVelocity, ref result, out this.linearVelocity);
            TSVector.Cross(ref relativePosition, ref impulse, out result);
            TSVector.Transform(ref result, ref this.invInertiaWorld, out result);
            TSVector.Add(ref this.angularVelocity, ref result, out this.angularVelocity);
        }

        public void AddForce(TSVector force)
        {
            TSVector.Add(ref force, ref this.force, out this.force);
        }

        public void AddForce(TSVector force, TSVector pos)
        {
            TSVector.Add(ref this.force, ref force, out this.force);
            TSVector.Subtract(ref pos, ref this.position, out pos);
            TSVector.Cross(ref pos, ref force, out pos);
            TSVector.Add(ref pos, ref this.torque, out this.torque);
        }

        public TSVector Torque
        {
            get
            {
                return this.torque;
            }
        }

        public TSVector Force
        {
            get
            {
                return this.force;
            }
        }

        public void AddTorque(TSVector torque)
        {
            TSVector.Add(ref torque, ref this.torque, out this.torque);
        }

        public void SetMassProperties()
        {
            this.inertia = this.Shape.inertia;
            TSMatrix.Inverse(ref this.inertia, out this.invInertia);
            this.inverseMass = FP.One / this.Shape.mass;
            this.useShapeMassProperties = true;
        }

        public void SetMassProperties(TSMatrix inertia, FP mass, bool setAsInverseValues)
        {
            if (setAsInverseValues)
            {
                if (!this.isParticle)
                {
                    this.invInertia = inertia;
                    TSMatrix.Inverse(ref inertia, out this.inertia);
                }
                this.inverseMass = mass;
            }
            else
            {
                if (!this.isParticle)
                {
                    this.inertia = inertia;
                    TSMatrix.Inverse(ref inertia, out this.invInertia);
                }
                this.inverseMass = FP.One / mass;
            }
            this.useShapeMassProperties = false;
            this.Update();
        }

        private void ShapeUpdated()
        {
            if (this.useShapeMassProperties)
                this.SetMassProperties();
            this.Update();
            this.UpdateHullData();
        }

        public Shape Shape
        {
            get
            {
                return this.shape;
            }
            set
            {
                if (this.shape != null)
                    this.shape.ShapeUpdated -= this.updatedHandler;
                this.shape = value;
                this.shape.ShapeUpdated += new ShapeUpdatedHandler(this.ShapeUpdated);
            }
        }

        public RigidBody.DampingType Damping
        {
            get
            {
                return this.damping;
            }
            set
            {
                this.damping = value;
            }
        }

        public BodyMaterial Material
        {
            get
            {
                return this.material;
            }
            set
            {
                this.material = value;
            }
        }

        public TSMatrix Inertia
        {
            get
            {
                return this.inertia;
            }
        }

        public TSMatrix InverseInertia
        {
            get
            {
                return this.invInertia;
            }
        }

        public TSVector LinearVelocity
        {
            get
            {
                return this.linearVelocity;
            }
            set
            {
                if (this.isStatic)
                    throw new InvalidOperationException("Can't set a velocity to a static body.");
                this.linearVelocity = value;
            }
        }

        public TSVector AngularVelocity
        {
            get
            {
                return this.angularVelocity;
            }
            set
            {
                if (this.isStatic)
                    throw new InvalidOperationException("Can't set a velocity to a static body.");
                this.angularVelocity = value;
            }
        }

        public TSVector Position
        {
            get
            {
                return this.position;
            }
            set
            {
                this.position = value;
                this.Update();
            }
        }

        public TSMatrix Orientation
        {
            get
            {
                return this.orientation;
            }
            set
            {
                this.orientation = value;
                this.Update();
            }
        }

        public bool IsStatic
        {
            get
            {
                return this.isStatic;
            }
            set
            {
                if (value && !this.isStatic)
                {
                    if (this.island != null)
                        this.island.islandManager.MakeBodyStatic(this);
                    this.angularVelocity.MakeZero();
                    this.linearVelocity.MakeZero();
                }
                this.isStatic = value;
            }
        }

        public bool TSIsStatic
        {
            get
            {
                return this.IsStatic;
            }
            set
            {
                this.IsStatic = value;
            }
        }

        public bool IsKinematic
        {
            get
            {
                return this.isKinematic;
            }
            set
            {
                this.isKinematic = value;
                if (!this.isKinematic)
                    return;
                this.AffectedByGravity = false;
                this.IsStatic = true;
            }
        }

        public bool AffectedByGravity
        {
            get
            {
                return this.affectedByGravity;
            }
            set
            {
                this.affectedByGravity = value;
            }
        }

        public TSMatrix InverseInertiaWorld
        {
            get
            {
                return this.invInertiaWorld;
            }
        }

        public FP Mass
        {
            get
            {
                return FP.One / this.inverseMass;
            }
            set
            {
                if (value <= FP.Zero)
                    throw new ArgumentException("Mass can't be less or equal zero.");
                if (!this.isParticle)
                {
                    TSMatrix.Multiply(ref this.Shape.inertia, value / this.Shape.mass, out this.inertia);
                    TSMatrix.Inverse(ref this.inertia, out this.invInertia);
                }
                this.inverseMass = FP.One / value;
            }
        }

        internal void SweptExpandBoundingBox(FP timestep)
        {
            this.sweptDirection = this.linearVelocity * timestep;
            if (this.sweptDirection.x < FP.Zero)
                this.boundingBox.min.x += this.sweptDirection.x;
            else
                this.boundingBox.max.x += this.sweptDirection.x;
            if (this.sweptDirection.y < FP.Zero)
                this.boundingBox.min.y += this.sweptDirection.y;
            else
                this.boundingBox.max.y += this.sweptDirection.y;
            if (this.sweptDirection.z < FP.Zero)
                this.boundingBox.min.z += this.sweptDirection.z;
            else
                this.boundingBox.max.z += this.sweptDirection.z;
        }

        public virtual void Update()
        {
            if (this.isParticle)
            {
                this.inertia = TSMatrix.Zero;
                this.invInertia = this.invInertiaWorld = TSMatrix.Zero;
                this.invOrientation = this.orientation = TSMatrix.Identity;
                this.boundingBox = this.shape.boundingBox;
                TSVector.Add(ref this.boundingBox.min, ref this.position, out this.boundingBox.min);
                TSVector.Add(ref this.boundingBox.max, ref this.position, out this.boundingBox.max);
                this.angularVelocity.MakeZero();
            }
            else
            {
                TSMatrix.Transpose(ref this.orientation, out this.invOrientation);
                this.Shape.GetBoundingBox(ref this.orientation, out this.boundingBox);
                TSVector.Add(ref this.boundingBox.min, ref this.position, out this.boundingBox.min);
                TSVector.Add(ref this.boundingBox.max, ref this.position, out this.boundingBox.max);
                if (!this.isStatic)
                {
                    TSMatrix.Multiply(ref this.invOrientation, ref this.invInertia, out this.invInertiaWorld);
                    TSMatrix.Multiply(ref this.invInertiaWorld, ref this.orientation, out this.invInertiaWorld);
                }
            }
        }

        public bool Equals(RigidBody other)
        {
            return other.instance == this.instance;
        }

        public int CompareTo(object otherObj)
        {
            RigidBody rigidBody = (RigidBody)otherObj;
            if (rigidBody.instance < this.instance)
                return -1;
            return rigidBody.instance > this.instance ? 1 : 0;
        }

        internal int BroadphaseTag { get; set; }

        public bool IsStaticOrInactive
        {
            get
            {
                return !this.isActive || this.isStatic;
            }
        }

        public bool IsStaticNonKinematic
        {
            get
            {
                return !this.isActive || this.isStatic && !this.isKinematic;
            }
        }

        internal bool EnableDebugDraw
        {
            get
            {
                return this.enableDebugDraw;
            }
            set
            {
                this.enableDebugDraw = value;
                this.UpdateHullData();
            }
        }

        public bool TSAffectedByGravity
        {
            get
            {
                return this.AffectedByGravity;
            }
            set
            {
                this.AffectedByGravity = value;
            }
        }

        public bool TSIsKinematic
        {
            get
            {
                return this.IsKinematic;
            }
            set
            {
                this.IsKinematic = value;
            }
        }

        public TSVector TSLinearVelocity
        {
            get
            {
                return this.LinearVelocity;
            }
            set
            {
                this.LinearVelocity = value;
            }
        }

        public TSVector TSAngularVelocity
        {
            get
            {
                return this.AngularVelocity;
            }
            set
            {
                this.AngularVelocity = value;
            }
        }

        public bool TSDisabled
        {
            get
            {
                return this.Disabled;
            }
            set
            {
                this.disabled = true;
            }
        }

        public TSVector TSPosition
        {
            get
            {
                return this.Position;
            }
            set
            {
                this.Position = value;
            }
        }

        public TSMatrix TSOrientation
        {
            get
            {
                return this.Orientation;
            }
            set
            {
                this.Orientation = value;
            }
        }

        private void UpdateHullData()
        {
            this.hullPoints.Clear();
            if (!this.enableDebugDraw)
                return;
            this.shape.MakeHull(ref this.hullPoints, 3);
        }

        public void DebugDraw(IDebugDrawer drawer)
        {
            int index = 0;
            while (index < this.hullPoints.Count)
            {
                TSVector result1 = this.hullPoints[index];
                TSVector result2 = this.hullPoints[index + 1];
                TSVector result3 = this.hullPoints[index + 2];
                TSVector.Transform(ref result1, ref this.orientation, out result1);
                TSVector.Add(ref result1, ref this.position, out result1);
                TSVector.Transform(ref result2, ref this.orientation, out result2);
                TSVector.Add(ref result2, ref this.position, out result2);
                TSVector.Transform(ref result3, ref this.orientation, out result3);
                TSVector.Add(ref result3, ref this.position, out result3);
                drawer.DrawTriangle(result1, result2, result3);
                index += 3;
            }
        }

        internal int GetInstance()
        {
            return this.instance;
        }

        internal void PostStep()
        {
            if (this._freezeConstraints <= TSRigidBodyConstraints.None)
                return;
            bool flag1 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionX) == TSRigidBodyConstraints.FreezePositionX;
            bool flag2 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionY) == TSRigidBodyConstraints.FreezePositionY;
            bool flag3 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionZ) == TSRigidBodyConstraints.FreezePositionZ;
            if (flag1)
                this.position.x = this._freezePosition.x;
            if (flag2)
                this.position.y = this._freezePosition.y;
            if (flag3)
                this.position.z = this._freezePosition.z;
            bool flag4 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationX) == TSRigidBodyConstraints.FreezeRotationX;
            bool flag5 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationY) == TSRigidBodyConstraints.FreezeRotationY;
            bool flag6 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationZ) == TSRigidBodyConstraints.FreezeRotationZ;
            if (flag4 | flag5 | flag6)
            {
                TSQuaternion fromMatrix = TSQuaternion.CreateFromMatrix(this.Orientation);
                if (flag4)
                    fromMatrix.x = this._freezeRotationQuat.x;
                if (flag5)
                    fromMatrix.y = this._freezeRotationQuat.y;
                if (flag6)
                    fromMatrix.z = this._freezeRotationQuat.z;
                fromMatrix.Normalize();
                this.Orientation = TSMatrix.CreateFromQuaternion(fromMatrix);
            }
        }

        public string Checkum()
        {
            return string.Format("{0}|{1}", (object)this.position, (object)this.orientation);
        }

        public void TSApplyForce(TSVector force)
        {
            this.AddForce(force);
        }

        public void TSApplyForce(TSVector force, TSVector position)
        {
            this.AddForce(force, position);
        }

        public void TSApplyImpulse(TSVector force)
        {
            this.ApplyImpulse(force);
        }

        public void TSApplyImpulse(TSVector force, TSVector position)
        {
            this.ApplyImpulse(force, position);
        }

        public void TSApplyTorque(TSVector force)
        {
            this.AddTorque(force);
        }

        public void TSUpdate()
        {
            this.Update();
        }

        [Flags]
        public enum DampingType
        {
            None = 0,
            Angular = 1,
            Linear = 2,
        }
    }
}
