using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class RigidBody : IBroadphaseEntity, IDebugDrawable, IEquatable<RigidBody>, IComparable, IBody3D, IBody
	{
		[Flags]
		public enum DampingType
		{
			None = 0,
			Angular = 1,
			Linear = 2
		}

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

		internal FP inactiveTime = FP.Zero;

		internal bool isActive = true;

		internal bool isStatic = false;

		internal bool isKinematic = false;

		internal bool affectedByGravity = true;

		internal bool isColliderOnly = false;

		internal CollisionIsland island;

		internal FP inverseMass;

		internal TSVector force;

		internal TSVector torque;

		private int hashCode;

		internal int internalIndex = 0;

		private ShapeUpdatedHandler updatedHandler;

		internal List<RigidBody> connections = new List<RigidBody>();

		internal HashList<Arbiter> arbiters = new HashList<Arbiter>();

		internal HashList<Arbiter> arbitersTrigger = new HashList<Arbiter>();

		internal HashList<Constraint> constraints = new HashList<Constraint>();

		private ReadOnlyHashset<Arbiter> readOnlyArbiters;

		private ReadOnlyHashset<Constraint> readOnlyConstraints;

		internal int marker = 0;

		internal bool disabled = false;

		internal TSRigidBodyConstraints _freezeConstraints = TSRigidBodyConstraints.None;

		internal TSVector _freezePosition = TSVector.zero;

		internal TSMatrix _freezeRotation = TSMatrix.Identity;

		internal TSQuaternion _freezeRotationQuat = TSQuaternion.identity;

		internal bool isParticle = false;

		internal static int instanceCount = 0;

		private int instance;

		protected bool useShapeMassProperties = true;

		internal Shape shape;

		private RigidBody.DampingType damping = RigidBody.DampingType.Angular | RigidBody.DampingType.Linear;

		internal TSVector sweptDirection = TSVector.zero;

		private bool enableDebugDraw = false;

		private List<TSVector> hullPoints = new List<TSVector>();

		public TSRigidBodyConstraints FreezeConstraints
		{
			get
			{
				return this._freezeConstraints;
			}
			set
			{
				bool flag = this._freezeConstraints != value;
				if (flag)
				{
					this._freezeConstraints = value;
					this._freezePosition = this.position;
					bool flag2 = this._freezeRotation != this.orientation;
					if (flag2)
					{
						this._freezeRotation = this.orientation;
						this._freezeRotationQuat = TSQuaternion.CreateFromMatrix(this._freezeRotation);
					}
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

		public bool IsParticle
		{
			get
			{
				return this.isParticle;
			}
			set
			{
				bool flag = this.isParticle && !value;
				if (flag)
				{
					this.updatedHandler = new ShapeUpdatedHandler(this.ShapeUpdated);
					this.Shape.ShapeUpdated += this.updatedHandler;
					this.SetMassProperties();
					this.isParticle = false;
				}
				else
				{
					bool flag2 = !this.isParticle & value;
					if (flag2)
					{
						this.inertia = TSMatrix.Zero;
						this.invInertia = (this.invInertiaWorld = TSMatrix.Zero);
						this.invOrientation = (this.orientation = TSMatrix.Identity);
						this.inverseMass = FP.One;
						this.Shape.ShapeUpdated -= this.updatedHandler;
						this.isParticle = true;
					}
				}
				this.Update();
			}
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

		internal bool AllowDeactivation
		{
			get;
			set;
		}

		internal bool EnableSpeculativeContacts
		{
			get;
			set;
		}

		public TSBBox BoundingBox
		{
			get
			{
				return this.boundingBox;
			}
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
				bool flag = !this.isActive & value;
				if (flag)
				{
					this.inactiveTime = FP.Zero;
				}
				else
				{
					bool flag2 = this.isActive && !value;
					if (flag2)
					{
						this.inactiveTime = FP.PositiveInfinity;
						this.angularVelocity.MakeZero();
						this.linearVelocity.MakeZero();
					}
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

		public Shape Shape
		{
			get
			{
				return this.shape;
			}
			set
			{
				bool flag = this.shape != null;
				if (flag)
				{
					this.shape.ShapeUpdated -= this.updatedHandler;
				}
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
				bool flag = this.isStatic;
				if (flag)
				{
					throw new InvalidOperationException("Can't set a velocity to a static body.");
				}
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
				bool flag = this.isStatic;
				if (flag)
				{
					throw new InvalidOperationException("Can't set a velocity to a static body.");
				}
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
				bool flag = value && !this.isStatic;
				if (flag)
				{
					bool flag2 = this.island != null;
					if (flag2)
					{
						this.island.islandManager.MakeBodyStatic(this);
					}
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
				bool flag = this.isKinematic;
				if (flag)
				{
					this.AffectedByGravity = false;
					this.IsStatic = true;
				}
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
				bool flag = value <= FP.Zero;
				if (flag)
				{
					throw new ArgumentException("Mass can't be less or equal zero.");
				}
				bool flag2 = !this.isParticle;
				if (flag2)
				{
					TSMatrix.Multiply(ref this.Shape.inertia, value / this.Shape.mass, out this.inertia);
					TSMatrix.Inverse(ref this.inertia, out this.invInertia);
				}
				this.inverseMass = FP.One / value;
			}
		}

		internal int BroadphaseTag
		{
			get;
			set;
		}

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
				return !this.isActive || (this.isStatic && !this.isKinematic);
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

		public RigidBody(Shape shape) : this(shape, new BodyMaterial(), false)
		{
		}

		public RigidBody(Shape shape, BodyMaterial material) : this(shape, material, false)
		{
		}

		public RigidBody(Shape shape, BodyMaterial material, bool isParticle)
		{
			this.readOnlyArbiters = new ReadOnlyHashset<Arbiter>(this.arbiters);
			this.readOnlyConstraints = new ReadOnlyHashset<Constraint>(this.constraints);
			RigidBody.instanceCount++;
			this.instance = RigidBody.instanceCount;
			this.hashCode = this.CalculateHash(this.instance);
			this.Shape = shape;
			this.orientation = TSMatrix.Identity;
			bool flag = !isParticle;
			if (flag)
			{
				this.updatedHandler = new ShapeUpdatedHandler(this.ShapeUpdated);
				this.Shape.ShapeUpdated += this.updatedHandler;
				this.SetMassProperties();
			}
			else
			{
				this.inertia = TSMatrix.Zero;
				this.invInertia = (this.invInertiaWorld = TSMatrix.Zero);
				this.invOrientation = (this.orientation = TSMatrix.Identity);
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

		private int CalculateHash(int a)
		{
			a = (a ^ 61 ^ a >> 16);
			a += a << 3;
			a ^= a >> 4;
			a *= 668265261;
			a ^= a >> 15;
			return a;
		}

		public void ApplyImpulse(TSVector impulse)
		{
			bool flag = this.isStatic;
			if (flag)
			{
				throw new InvalidOperationException("Can't apply an impulse to a static body.");
			}
			TSVector tSVector;
			TSVector.Multiply(ref impulse, this.inverseMass, out tSVector);
			TSVector.Add(ref this.linearVelocity, ref tSVector, out this.linearVelocity);
		}

		public void ApplyImpulse(TSVector impulse, TSVector relativePosition)
		{
			bool flag = this.isStatic;
			if (flag)
			{
				throw new InvalidOperationException("Can't apply an impulse to a static body.");
			}
			TSVector tSVector;
			TSVector.Multiply(ref impulse, this.inverseMass, out tSVector);
			TSVector.Add(ref this.linearVelocity, ref tSVector, out this.linearVelocity);
			TSVector.Cross(ref relativePosition, ref impulse, out tSVector);
			TSVector.Transform(ref tSVector, ref this.invInertiaWorld, out tSVector);
			TSVector.Add(ref this.angularVelocity, ref tSVector, out this.angularVelocity);
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
				bool flag = !this.isParticle;
				if (flag)
				{
					this.invInertia = inertia;
					TSMatrix.Inverse(ref inertia, out this.inertia);
				}
				this.inverseMass = mass;
			}
			else
			{
				bool flag2 = !this.isParticle;
				if (flag2)
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
			bool flag = this.useShapeMassProperties;
			if (flag)
			{
				this.SetMassProperties();
			}
			this.Update();
			this.UpdateHullData();
		}

		internal void SweptExpandBoundingBox(FP timestep)
		{
			this.sweptDirection = this.linearVelocity * timestep;
			bool flag = this.sweptDirection.x < FP.Zero;
			if (flag)
			{
				this.boundingBox.min.x = this.boundingBox.min.x + this.sweptDirection.x;
			}
			else
			{
				this.boundingBox.max.x = this.boundingBox.max.x + this.sweptDirection.x;
			}
			bool flag2 = this.sweptDirection.y < FP.Zero;
			if (flag2)
			{
				this.boundingBox.min.y = this.boundingBox.min.y + this.sweptDirection.y;
			}
			else
			{
				this.boundingBox.max.y = this.boundingBox.max.y + this.sweptDirection.y;
			}
			bool flag3 = this.sweptDirection.z < FP.Zero;
			if (flag3)
			{
				this.boundingBox.min.z = this.boundingBox.min.z + this.sweptDirection.z;
			}
			else
			{
				this.boundingBox.max.z = this.boundingBox.max.z + this.sweptDirection.z;
			}
		}

		public virtual void Update()
		{
			bool flag = this.isParticle;
			if (flag)
			{
				this.inertia = TSMatrix.Zero;
				this.invInertia = (this.invInertiaWorld = TSMatrix.Zero);
				this.invOrientation = (this.orientation = TSMatrix.Identity);
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
				bool flag2 = !this.isStatic;
				if (flag2)
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
			bool flag = rigidBody.instance < this.instance;
			int result;
			if (flag)
			{
				result = -1;
			}
			else
			{
				bool flag2 = rigidBody.instance > this.instance;
				if (flag2)
				{
					result = 1;
				}
				else
				{
					result = 0;
				}
			}
			return result;
		}

		private void UpdateHullData()
		{
			this.hullPoints.Clear();
			bool flag = this.enableDebugDraw;
			if (flag)
			{
				this.shape.MakeHull(ref this.hullPoints, 3);
			}
		}

		public void DebugDraw(IDebugDrawer drawer)
		{
			for (int i = 0; i < this.hullPoints.Count; i += 3)
			{
				TSVector pos = this.hullPoints[i];
				TSVector pos2 = this.hullPoints[i + 1];
				TSVector pos3 = this.hullPoints[i + 2];
				TSVector.Transform(ref pos, ref this.orientation, out pos);
				TSVector.Add(ref pos, ref this.position, out pos);
				TSVector.Transform(ref pos2, ref this.orientation, out pos2);
				TSVector.Add(ref pos2, ref this.position, out pos2);
				TSVector.Transform(ref pos3, ref this.orientation, out pos3);
				TSVector.Add(ref pos3, ref this.position, out pos3);
				drawer.DrawTriangle(pos, pos2, pos3);
			}
		}

		internal int GetInstance()
		{
			return this.instance;
		}

		internal void PostStep()
		{
			bool flag = this._freezeConstraints > TSRigidBodyConstraints.None;
			if (flag)
			{
				bool flag2 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionX) == TSRigidBodyConstraints.FreezePositionX;
				bool flag3 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionY) == TSRigidBodyConstraints.FreezePositionY;
				bool flag4 = (this._freezeConstraints & TSRigidBodyConstraints.FreezePositionZ) == TSRigidBodyConstraints.FreezePositionZ;
				bool flag5 = flag2;
				if (flag5)
				{
					this.position.x = this._freezePosition.x;
				}
				bool flag6 = flag3;
				if (flag6)
				{
					this.position.y = this._freezePosition.y;
				}
				bool flag7 = flag4;
				if (flag7)
				{
					this.position.z = this._freezePosition.z;
				}
				bool flag8 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationX) == TSRigidBodyConstraints.FreezeRotationX;
				bool flag9 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationY) == TSRigidBodyConstraints.FreezeRotationY;
				bool flag10 = (this._freezeConstraints & TSRigidBodyConstraints.FreezeRotationZ) == TSRigidBodyConstraints.FreezeRotationZ;
				bool flag11 = flag8 | flag9 | flag10;
				if (flag11)
				{
					TSQuaternion quaternion = TSQuaternion.CreateFromMatrix(this.Orientation);
					bool flag12 = flag8;
					if (flag12)
					{
						quaternion.x = this._freezeRotationQuat.x;
					}
					bool flag13 = flag9;
					if (flag13)
					{
						quaternion.y = this._freezeRotationQuat.y;
					}
					bool flag14 = flag10;
					if (flag14)
					{
						quaternion.z = this._freezeRotationQuat.z;
					}
					quaternion.Normalize();
					this.Orientation = TSMatrix.CreateFromQuaternion(quaternion);
				}
			}
		}

		public string Checkum()
		{
			return string.Format("{0}|{1}", this.position, this.orientation);
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
	}
}
