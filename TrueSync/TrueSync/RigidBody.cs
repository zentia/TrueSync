namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    public class RigidBody : IBroadphaseEntity, IDebugDrawable, IEquatable<RigidBody>, IComparable, IBody3D, IBody
    {
        internal TSRigidBodyConstraints _freezeConstraints;
        internal TSVector _freezePosition;
        internal TSMatrix _freezeRotation;
        internal TSQuaternion _freezeRotationQuat;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <AllowDeactivation>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <BroadphaseTag>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <EnableSpeculativeContacts>k__BackingField;
        internal bool affectedByGravity;
        internal TSVector angularVelocity;
        internal HashList<Arbiter> arbiters;
        internal HashList<Arbiter> arbitersTrigger;
        internal TSBBox boundingBox;
        internal List<RigidBody> connections;
        internal HashList<Constraint> constraints;
        private DampingType damping;
        internal bool disabled;
        private bool enableDebugDraw;
        internal TSVector force;
        private int hashCode;
        private List<TSVector> hullPoints;
        internal FP inactiveTime;
        internal TSMatrix inertia;
        private int instance;
        internal static int instanceCount = 0;
        internal int internalIndex;
        internal FP inverseMass;
        internal TSMatrix invInertia;
        internal TSMatrix invInertiaWorld;
        internal TSMatrix invOrientation;
        internal bool isActive;
        internal bool isColliderOnly;
        internal bool isKinematic;
        internal TrueSync.CollisionIsland island;
        internal bool isParticle;
        internal bool isStatic;
        internal TSVector linearVelocity;
        internal int marker;
        internal BodyMaterial material;
        internal TSMatrix orientation;
        internal TSVector position;
        private ReadOnlyHashset<Arbiter> readOnlyArbiters;
        private ReadOnlyHashset<Constraint> readOnlyConstraints;
        internal TrueSync.Shape shape;
        internal TSVector sweptDirection;
        internal TSVector torque;
        private ShapeUpdatedHandler updatedHandler;
        protected bool useShapeMassProperties;

        public RigidBody(TrueSync.Shape shape) : this(shape, new BodyMaterial(), false)
        {
        }

        public RigidBody(TrueSync.Shape shape, BodyMaterial material) : this(shape, material, false)
        {
        }

        public RigidBody(TrueSync.Shape shape, BodyMaterial material, bool isParticle)
        {
            this.inactiveTime = FP.Zero;
            this.isActive = true;
            this.isStatic = false;
            this.isKinematic = false;
            this.affectedByGravity = true;
            this.isColliderOnly = false;
            this.internalIndex = 0;
            this.connections = new List<RigidBody>();
            this.arbiters = new HashList<Arbiter>();
            this.arbitersTrigger = new HashList<Arbiter>();
            this.constraints = new HashList<Constraint>();
            this.marker = 0;
            this.disabled = false;
            this._freezeConstraints = TSRigidBodyConstraints.None;
            this._freezePosition = TSVector.zero;
            this._freezeRotation = TSMatrix.Identity;
            this._freezeRotationQuat = TSQuaternion.identity;
            this.isParticle = false;
            this.useShapeMassProperties = true;
            this.damping = DampingType.Linear | DampingType.Angular;
            this.sweptDirection = TSVector.zero;
            this.enableDebugDraw = false;
            this.hullPoints = new List<TSVector>();
            this.readOnlyArbiters = new ReadOnlyHashset<Arbiter>(this.arbiters);
            this.readOnlyConstraints = new ReadOnlyHashset<Constraint>(this.constraints);
            instanceCount++;
            this.instance = instanceCount;
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

        public void AddTorque(TSVector torque)
        {
            TSVector.Add(ref torque, ref this.torque, out this.torque);
        }

        public void ApplyImpulse(TSVector impulse)
        {
            TSVector vector;
            if (this.isStatic)
            {
                throw new InvalidOperationException("Can't apply an impulse to a static body.");
            }
            TSVector.Multiply(ref impulse, this.inverseMass, out vector);
            TSVector.Add(ref this.linearVelocity, ref vector, out this.linearVelocity);
        }

        public void ApplyImpulse(TSVector impulse, TSVector relativePosition)
        {
            TSVector vector;
            if (this.isStatic)
            {
                throw new InvalidOperationException("Can't apply an impulse to a static body.");
            }
            TSVector.Multiply(ref impulse, this.inverseMass, out vector);
            TSVector.Add(ref this.linearVelocity, ref vector, out this.linearVelocity);
            TSVector.Cross(ref relativePosition, ref impulse, out vector);
            TSVector.Transform(ref vector, ref this.invInertiaWorld, out vector);
            TSVector.Add(ref this.angularVelocity, ref vector, out this.angularVelocity);
        }

        private int CalculateHash(int a)
        {
            a = (a ^ 0x3d) ^ (a >> 0x10);
            a += a << 3;
            a ^= a >> 4;
            a *= 0x27d4eb2d;
            a ^= a >> 15;
            return a;
        }

        public string Checkum()
        {
            return string.Format("{0}|{1}", this.position, this.orientation);
        }

        public int CompareTo(object otherObj)
        {
            RigidBody body = (RigidBody) otherObj;
            if (body.instance < this.instance)
            {
                return -1;
            }
            if (body.instance > this.instance)
            {
                return 1;
            }
            return 0;
        }

        public void DebugDraw(IDebugDrawer drawer)
        {
            for (int i = 0; i < this.hullPoints.Count; i += 3)
            {
                TSVector position = this.hullPoints[i];
                TSVector vector2 = this.hullPoints[i + 1];
                TSVector vector3 = this.hullPoints[i + 2];
                TSVector.Transform(ref position, ref this.orientation, out position);
                TSVector.Add(ref position, ref this.position, out position);
                TSVector.Transform(ref vector2, ref this.orientation, out vector2);
                TSVector.Add(ref vector2, ref this.position, out vector2);
                TSVector.Transform(ref vector3, ref this.orientation, out vector3);
                TSVector.Add(ref vector3, ref this.position, out vector3);
                drawer.DrawTriangle(position, vector2, vector3);
            }
        }

        public bool Equals(RigidBody other)
        {
            return (other.instance == this.instance);
        }

        public override int GetHashCode()
        {
            return this.hashCode;
        }

        internal int GetInstance()
        {
            return this.instance;
        }

        internal void PostStep()
        {
            if (this._freezeConstraints > TSRigidBodyConstraints.None)
            {
                bool flag2 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionX) == TSRigidBodyConstraints.FreezePositionX;
                bool flag3 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionY) == TSRigidBodyConstraints.FreezePositionY;
                bool flag4 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionZ) == TSRigidBodyConstraints.FreezePositionZ;
                if (flag2)
                {
                    this.position.x = this._freezePosition.x;
                }
                if (flag3)
                {
                    this.position.y = this._freezePosition.y;
                }
                if (flag4)
                {
                    this.position.z = this._freezePosition.z;
                }
                bool flag5 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationX) == TSRigidBodyConstraints.FreezeRotationX;
                bool flag6 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationY) == TSRigidBodyConstraints.FreezeRotationY;
                bool flag7 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationZ) == TSRigidBodyConstraints.FreezeRotationZ;
                if ((flag5 | flag6) | flag7)
                {
                    TSQuaternion quaternion = TSQuaternion.CreateFromMatrix(this.Orientation);
                    if (flag5)
                    {
                        quaternion.x = this._freezeRotationQuat.x;
                    }
                    if (flag6)
                    {
                        quaternion.y = this._freezeRotationQuat.y;
                    }
                    if (flag7)
                    {
                        quaternion.z = this._freezeRotationQuat.z;
                    }
                    quaternion.Normalize();
                    this.Orientation = TSMatrix.CreateFromQuaternion(quaternion);
                }
            }
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
            {
                this.SetMassProperties();
            }
            this.Update();
            this.UpdateHullData();
        }

        internal void SweptExpandBoundingBox(FP timestep)
        {
            this.sweptDirection = this.linearVelocity * timestep;
            if (this.sweptDirection.x < FP.Zero)
            {
                this.boundingBox.min.x += this.sweptDirection.x;
            }
            else
            {
                this.boundingBox.max.x += this.sweptDirection.x;
            }
            if (this.sweptDirection.y < FP.Zero)
            {
                this.boundingBox.min.y += this.sweptDirection.y;
            }
            else
            {
                this.boundingBox.max.y += this.sweptDirection.y;
            }
            if (this.sweptDirection.z < FP.Zero)
            {
                this.boundingBox.min.z += this.sweptDirection.z;
            }
            else
            {
                this.boundingBox.max.z += this.sweptDirection.z;
            }
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

        private void UpdateHullData()
        {
            this.hullPoints.Clear();
            if (this.enableDebugDraw)
            {
                this.shape.MakeHull(ref this.hullPoints, 3);
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

        internal bool AllowDeactivation { get; set; }

        public TSVector AngularVelocity
        {
            get
            {
                return this.angularVelocity;
            }
            set
            {
                if (this.isStatic)
                {
                    throw new InvalidOperationException("Can't set a velocity to a static body.");
                }
                this.angularVelocity = value;
            }
        }

        internal ReadOnlyHashset<Arbiter> Arbiters
        {
            get
            {
                return this.readOnlyArbiters;
            }
        }

        public TSBBox BoundingBox
        {
            get
            {
                return this.boundingBox;
            }
        }

        internal int BroadphaseTag { get; set; }

        internal TrueSync.CollisionIsland CollisionIsland
        {
            get
            {
                return this.island;
            }
        }

        internal ReadOnlyHashset<Constraint> Constraints
        {
            get
            {
                return this.readOnlyConstraints;
            }
        }

        public DampingType Damping
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

        public bool Disabled
        {
            get
            {
                return this.disabled;
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

        internal bool EnableSpeculativeContacts { get; set; }

        public TSVector Force
        {
            get
            {
                return this.force;
            }
        }

        public TSRigidBodyConstraints FreezeConstraints
        {
            get
            {
                return this._freezeConstraints;
            }
            set
            {
                if (this._freezeConstraints != value)
                {
                    this._freezeConstraints = value;
                    this._freezePosition = this.position;
                    if (this._freezeRotation != this.orientation)
                    {
                        this._freezeRotation = this.orientation;
                        this._freezeRotationQuat = TSQuaternion.CreateFromMatrix(this._freezeRotation);
                    }
                }
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

        public TSMatrix InverseInertiaWorld
        {
            get
            {
                return this.invInertiaWorld;
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
                {
                    this.inactiveTime = FP.Zero;
                }
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

        public bool IsKinematic
        {
            get
            {
                return this.isKinematic;
            }
            set
            {
                this.isKinematic = value;
                if (this.isKinematic)
                {
                    this.AffectedByGravity = false;
                    this.IsStatic = true;
                }
            }
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
                    if (this.island > null)
                    {
                        this.island.islandManager.MakeBodyStatic(this);
                    }
                    this.angularVelocity.MakeZero();
                    this.linearVelocity.MakeZero();
                }
                this.isStatic = value;
            }
        }

        public bool IsStaticNonKinematic
        {
            get
            {
                return (!this.isActive || (this.isStatic && !this.isKinematic));
            }
        }

        public bool IsStaticOrInactive
        {
            get
            {
                return (!this.isActive || this.isStatic);
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
                {
                    throw new InvalidOperationException("Can't set a velocity to a static body.");
                }
                this.linearVelocity = value;
            }
        }

        public FP Mass
        {
            get
            {
                return (FP.One / this.inverseMass);
            }
            set
            {
                if (value <= FP.Zero)
                {
                    throw new ArgumentException("Mass can't be less or equal zero.");
                }
                if (!this.isParticle)
                {
                    TSMatrix.Multiply(ref this.Shape.inertia, value / this.Shape.mass, out this.inertia);
                    TSMatrix.Inverse(ref this.inertia, out this.invInertia);
                }
                this.inverseMass = FP.One / value;
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

        public TrueSync.Shape Shape
        {
            get
            {
                return this.shape;
            }
            set
            {
                if (this.shape > null)
                {
                    this.shape.ShapeUpdated -= this.updatedHandler;
                }
                this.shape = value;
                this.shape.ShapeUpdated += new ShapeUpdatedHandler(this.ShapeUpdated);
            }
        }

        public TSVector Torque
        {
            get
            {
                return this.torque;
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

        [Flags]
        public enum DampingType
        {
            None,
            Angular,
            Linear
        }
    }
}

