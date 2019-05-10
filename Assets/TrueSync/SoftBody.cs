using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TrueSync
{
	public class SoftBody : IBroadphaseEntity, IComparable
	{
		[Flags]
		public enum SpringType
		{
			EdgeSpring = 2,
			ShearSpring = 4,
			BendSpring = 8
		}

		public class Spring : Constraint
		{
			public enum DistanceBehavior
			{
				LimitDistance,
				LimitMaximumDistance,
				LimitMinimumDistance
			}

			private FP biasFactor = FP.EN1;

			private FP softness = FP.EN2;

			private FP distance;

			private SoftBody.Spring.DistanceBehavior behavior = SoftBody.Spring.DistanceBehavior.LimitDistance;

			private FP effectiveMass = FP.Zero;

			private FP accumulatedImpulse = FP.Zero;

			private FP bias;

			private FP softnessOverDt;

			private TSVector[] jacobian = new TSVector[2];

			private bool skipConstraint = false;

			public SoftBody.SpringType SpringType
			{
				get;
				set;
			}

			public FP AppliedImpulse
			{
				get
				{
					return this.accumulatedImpulse;
				}
			}

			public FP Distance
			{
				get
				{
					return this.distance;
				}
				set
				{
					this.distance = value;
				}
			}

			public SoftBody.Spring.DistanceBehavior Behavior
			{
				get
				{
					return this.behavior;
				}
				set
				{
					this.behavior = value;
				}
			}

			public FP Softness
			{
				get
				{
					return this.softness;
				}
				set
				{
					this.softness = value;
				}
			}

			public FP BiasFactor
			{
				get
				{
					return this.biasFactor;
				}
				set
				{
					this.biasFactor = value;
				}
			}

			public Spring(RigidBody body1, RigidBody body2) : base(body1, body2)
			{
				this.distance = (body1.position - body2.position).magnitude;
			}

			public override void PrepareForIteration(FP timestep)
			{
				TSVector tSVector;
				TSVector.Subtract(ref this.body2.position, ref this.body1.position, out tSVector);
				FP x = tSVector.magnitude - this.distance;
				bool flag = this.behavior == SoftBody.Spring.DistanceBehavior.LimitMaximumDistance && x <= FP.Zero;
				if (flag)
				{
					this.skipConstraint = true;
				}
				else
				{
					bool flag2 = this.behavior == SoftBody.Spring.DistanceBehavior.LimitMinimumDistance && x >= FP.Zero;
					if (flag2)
					{
						this.skipConstraint = true;
					}
					else
					{
						this.skipConstraint = false;
						TSVector value = tSVector;
						bool flag3 = value.sqrMagnitude != FP.Zero;
						if (flag3)
						{
							value.Normalize();
						}
						this.jacobian[0] = -FP.One * value;
						this.jacobian[1] = FP.One * value;
						this.effectiveMass = this.body1.inverseMass + this.body2.inverseMass;
						this.softnessOverDt = this.softness / timestep;
						this.effectiveMass += this.softnessOverDt;
						this.effectiveMass = FP.One / this.effectiveMass;
						this.bias = x * this.biasFactor * (FP.One / timestep);
						bool flag4 = !this.body1.isStatic;
						if (flag4)
						{
							this.body1.linearVelocity += this.body1.inverseMass * this.accumulatedImpulse * this.jacobian[0];
						}
						bool flag5 = !this.body2.isStatic;
						if (flag5)
						{
							this.body2.linearVelocity += this.body2.inverseMass * this.accumulatedImpulse * this.jacobian[1];
						}
					}
				}
			}

			public override void Iterate()
			{
				bool flag = this.skipConstraint;
				if (!flag)
				{
					FP x = TSVector.Dot(ref this.body1.linearVelocity, ref this.jacobian[0]);
					x += TSVector.Dot(ref this.body2.linearVelocity, ref this.jacobian[1]);
					FP y = this.accumulatedImpulse * this.softnessOverDt;
					FP fP = -this.effectiveMass * (x + this.bias + y);
					bool flag2 = this.behavior == SoftBody.Spring.DistanceBehavior.LimitMinimumDistance;
					if (flag2)
					{
						FP y2 = this.accumulatedImpulse;
						this.accumulatedImpulse = TSMath.Max(this.accumulatedImpulse + fP, 0);
						fP = this.accumulatedImpulse - y2;
					}
					else
					{
						bool flag3 = this.behavior == SoftBody.Spring.DistanceBehavior.LimitMaximumDistance;
						if (flag3)
						{
							FP y3 = this.accumulatedImpulse;
							this.accumulatedImpulse = TSMath.Min(this.accumulatedImpulse + fP, 0);
							fP = this.accumulatedImpulse - y3;
						}
						else
						{
							this.accumulatedImpulse += fP;
						}
					}
					bool flag4 = !this.body1.isStatic;
					if (flag4)
					{
						TSVector tSVector;
						TSVector.Multiply(ref this.jacobian[0], fP * this.body1.inverseMass, out tSVector);
						TSVector.Add(ref tSVector, ref this.body1.linearVelocity, out this.body1.linearVelocity);
					}
					bool flag5 = !this.body2.isStatic;
					if (flag5)
					{
						TSVector tSVector;
						TSVector.Multiply(ref this.jacobian[1], fP * this.body2.inverseMass, out tSVector);
						TSVector.Add(ref tSVector, ref this.body2.linearVelocity, out this.body2.linearVelocity);
					}
				}
			}

			public override void DebugDraw(IDebugDrawer drawer)
			{
				drawer.DrawLine(this.body1.position, this.body2.position);
			}
		}

		public class MassPoint : RigidBody
		{
			public SoftBody SoftBody
			{
				get;
				private set;
			}

			public MassPoint(Shape shape, SoftBody owner, BodyMaterial material) : base(shape, material, true)
			{
				this.SoftBody = owner;
			}
		}

		public class Triangle : ISupportMappable
		{
			private SoftBody owner;

			internal TSBBox boundingBox;

			internal int dynamicTreeID;

			internal TriangleVertexIndices indices;

			public SoftBody Owner
			{
				get
				{
					return this.owner;
				}
			}

			public TSBBox BoundingBox
			{
				get
				{
					return this.boundingBox;
				}
			}

			public int DynamicTreeID
			{
				get
				{
					return this.dynamicTreeID;
				}
			}

			public TriangleVertexIndices Indices
			{
				get
				{
					return this.indices;
				}
			}

			public SoftBody.MassPoint VertexBody1
			{
				get
				{
					return this.owner.points[this.indices.I0];
				}
			}

			public SoftBody.MassPoint VertexBody2
			{
				get
				{
					return this.owner.points[this.indices.I1];
				}
			}

			public SoftBody.MassPoint VertexBody3
			{
				get
				{
					return this.owner.points[this.indices.I2];
				}
			}

			public Triangle(SoftBody owner)
			{
				this.owner = owner;
			}

			public void GetNormal(out TSVector normal)
			{
				TSVector tSVector;
				TSVector.Subtract(ref this.owner.points[this.indices.I1].position, ref this.owner.points[this.indices.I0].position, out tSVector);
				TSVector.Subtract(ref this.owner.points[this.indices.I2].position, ref this.owner.points[this.indices.I0].position, out normal);
				TSVector.Cross(ref tSVector, ref normal, out normal);
			}

			public void UpdateBoundingBox()
			{
				this.boundingBox = TSBBox.SmallBox;
				this.boundingBox.AddPoint(ref this.owner.points[this.indices.I0].position);
				this.boundingBox.AddPoint(ref this.owner.points[this.indices.I1].position);
				this.boundingBox.AddPoint(ref this.owner.points[this.indices.I2].position);
				this.boundingBox.min = this.boundingBox.min - new TSVector(this.owner.triangleExpansion);
				this.boundingBox.max = this.boundingBox.max + new TSVector(this.owner.triangleExpansion);
			}

			public FP CalculateArea()
			{
				return ((this.owner.points[this.indices.I1].position - this.owner.points[this.indices.I0].position) % (this.owner.points[this.indices.I2].position - this.owner.points[this.indices.I0].position)).magnitude;
			}

			public void SupportMapping(ref TSVector direction, out TSVector result)
			{
				FP y = TSVector.Dot(ref this.owner.points[this.indices.I0].position, ref direction);
				FP fP = TSVector.Dot(ref this.owner.points[this.indices.I1].position, ref direction);
				TSVector position = this.owner.points[this.indices.I0].position;
				bool flag = fP > y;
				if (flag)
				{
					y = fP;
					position = this.owner.points[this.indices.I1].position;
				}
				fP = TSVector.Dot(ref this.owner.points[this.indices.I2].position, ref direction);
				bool flag2 = fP > y;
				if (flag2)
				{
					position = this.owner.points[this.indices.I2].position;
				}
				TSVector tSVector;
				TSVector.Normalize(ref direction, out tSVector);
				tSVector *= this.owner.triangleExpansion;
				result = position + tSVector;
			}

			public void SupportCenter(out TSVector center)
			{
				center = this.owner.points[this.indices.I0].position;
				TSVector.Add(ref center, ref this.owner.points[this.indices.I1].position, out center);
				TSVector.Add(ref center, ref this.owner.points[this.indices.I2].position, out center);
				TSVector.Multiply(ref center, FP.One / (3 * FP.One), out center);
			}
		}

		private struct Edge
		{
			public int Index1;

			public int Index2;

			public Edge(int index1, int index2)
			{
				this.Index1 = index1;
				this.Index2 = index2;
			}

			public override int GetHashCode()
			{
				return this.Index1.GetHashCode() + this.Index2.GetHashCode();
			}

			public override bool Equals(object obj)
			{
				SoftBody.Edge edge = (SoftBody.Edge)obj;
				return (edge.Index1 == this.Index1 && edge.Index2 == this.Index2) || (edge.Index1 == this.Index2 && edge.Index2 == this.Index1);
			}
		}

		private SphereShape sphere = new SphereShape(FP.EN1);

		protected List<SoftBody.Spring> springs = new List<SoftBody.Spring>();

		protected List<SoftBody.MassPoint> points = new List<SoftBody.MassPoint>();

		protected List<SoftBody.Triangle> triangles = new List<SoftBody.Triangle>();

		protected FP triangleExpansion = FP.EN1;

		private bool selfCollision = false;

		private FP volume = FP.One;

		private FP mass = FP.One;

		internal DynamicTree<SoftBody.Triangle> dynamicTree = new DynamicTree<SoftBody.Triangle>();

		private BodyMaterial material = new BodyMaterial();

		private TSBBox box = default(TSBBox);

		private bool active = true;

		private FP pressure = FP.Zero;

		private List<int> queryList = new List<int>();

		public ReadOnlyCollection<SoftBody.Spring> EdgeSprings
		{
			get;
			private set;
		}

		public ReadOnlyCollection<SoftBody.MassPoint> VertexBodies
		{
			get;
			private set;
		}

		public ReadOnlyCollection<SoftBody.Triangle> Triangles
		{
			get;
			private set;
		}

		public bool SelfCollision
		{
			get
			{
				return this.selfCollision;
			}
			set
			{
				this.selfCollision = value;
			}
		}

		public FP TriangleExpansion
		{
			get
			{
				return this.triangleExpansion;
			}
			set
			{
				this.triangleExpansion = value;
			}
		}

		public FP VertexExpansion
		{
			get
			{
				return this.sphere.Radius;
			}
			set
			{
				this.sphere.Radius = value;
			}
		}

		public DynamicTree<SoftBody.Triangle> DynamicTree
		{
			get
			{
				return this.dynamicTree;
			}
		}

		public BodyMaterial Material
		{
			get
			{
				return this.material;
			}
		}

		public FP Pressure
		{
			get
			{
				return this.pressure;
			}
			set
			{
				this.pressure = value;
			}
		}

		public FP Mass
		{
			get
			{
				return this.mass;
			}
			set
			{
				for (int i = 0; i < this.points.Count; i++)
				{
					this.points[i].Mass = value / this.points.Count;
				}
			}
		}

		public FP Volume
		{
			get
			{
				return this.volume;
			}
		}

		public TSBBox BoundingBox
		{
			get
			{
				return this.box;
			}
		}

		public int BroadphaseTag
		{
			get;
			set;
		}

		public object Tag
		{
			get;
			set;
		}

		public bool IsStaticOrInactive
		{
			get
			{
				return !this.active;
			}
		}

		public bool IsStaticNonKinematic
		{
			get
			{
				return !this.active;
			}
		}

		public int CompareTo(object otherObj)
		{
			SoftBody softBody = (SoftBody)otherObj;
			bool flag = softBody.volume < this.volume;
			int result;
			if (flag)
			{
				result = -1;
			}
			else
			{
				bool flag2 = softBody.volume > this.volume;
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

		public SoftBody()
		{
		}

		public SoftBody(int sizeX, int sizeY, FP scale)
		{
			List<TriangleVertexIndices> list = new List<TriangleVertexIndices>();
			List<TSVector> list2 = new List<TSVector>();
			for (int i = 0; i < sizeY; i++)
			{
				for (int j = 0; j < sizeX; j++)
				{
					list2.Add(new TSVector(i, 0, j) * scale);
				}
			}
			for (int k = 0; k < sizeX - 1; k++)
			{
				for (int l = 0; l < sizeY - 1; l++)
				{
					TriangleVertexIndices item = default(TriangleVertexIndices);
					item.I0 = l * sizeX + k;
					item.I1 = l * sizeX + k + 1;
					item.I2 = (l + 1) * sizeX + k + 1;
					list.Add(item);
					item.I0 = l * sizeX + k;
					item.I1 = (l + 1) * sizeX + k + 1;
					item.I2 = (l + 1) * sizeX + k;
					list.Add(item);
				}
			}
			this.EdgeSprings = this.springs.AsReadOnly();
			this.VertexBodies = this.points.AsReadOnly();
			this.Triangles = this.triangles.AsReadOnly();
			this.AddPointsAndSprings(list, list2);
			for (int m = 0; m < sizeX - 1; m++)
			{
				for (int n = 0; n < sizeY - 1; n++)
				{
					SoftBody.Spring spring = new SoftBody.Spring(this.points[n * sizeX + m + 1], this.points[(n + 1) * sizeX + m]);
					spring.Softness = FP.EN2;
					spring.BiasFactor = FP.EN1;
					this.springs.Add(spring);
				}
			}
			foreach (SoftBody.Spring current in this.springs)
			{
				TSVector tSVector = current.body1.position - current.body2.position;
				bool flag = tSVector.z != FP.Zero && tSVector.x != FP.Zero;
				if (flag)
				{
					current.SpringType = SoftBody.SpringType.ShearSpring;
				}
				else
				{
					current.SpringType = SoftBody.SpringType.EdgeSpring;
				}
			}
			for (int num = 0; num < sizeX - 2; num++)
			{
				for (int num2 = 0; num2 < sizeY - 2; num2++)
				{
					SoftBody.Spring spring2 = new SoftBody.Spring(this.points[num2 * sizeX + num], this.points[num2 * sizeX + num + 2]);
					spring2.Softness = FP.EN2;
					spring2.BiasFactor = FP.EN1;
					SoftBody.Spring spring3 = new SoftBody.Spring(this.points[num2 * sizeX + num], this.points[(num2 + 2) * sizeX + num]);
					spring3.Softness = FP.EN2;
					spring3.BiasFactor = FP.EN1;
					spring2.SpringType = SoftBody.SpringType.BendSpring;
					spring3.SpringType = SoftBody.SpringType.BendSpring;
					this.springs.Add(spring2);
					this.springs.Add(spring3);
				}
			}
		}

		public SoftBody(List<TriangleVertexIndices> indices, List<TSVector> vertices)
		{
			this.EdgeSprings = this.springs.AsReadOnly();
			this.VertexBodies = this.points.AsReadOnly();
			this.AddPointsAndSprings(indices, vertices);
			this.Triangles = this.triangles.AsReadOnly();
		}

		private void AddPressureForces(FP timeStep)
		{
			bool flag = this.pressure == FP.Zero || this.volume == FP.Zero;
			if (!flag)
			{
				FP value = FP.One / this.volume;
				foreach (SoftBody.Triangle current in this.triangles)
				{
					TSVector position = this.points[current.indices.I0].position;
					TSVector position2 = this.points[current.indices.I1].position;
					TSVector position3 = this.points[current.indices.I2].position;
					TSVector value2 = (position3 - position) % (position2 - position);
					this.points[current.indices.I0].AddForce(value * value2 * this.pressure);
					this.points[current.indices.I1].AddForce(value * value2 * this.pressure);
					this.points[current.indices.I2].AddForce(value * value2 * this.pressure);
				}
			}
		}

		public void Translate(TSVector position)
		{
			foreach (SoftBody.MassPoint current in this.points)
			{
				current.Position += position;
			}
			this.Update(FP.Epsilon);
		}

		public void AddForce(TSVector force)
		{
			throw new NotImplementedException();
		}

		public void Rotate(TSMatrix orientation, TSVector center)
		{
			for (int i = 0; i < this.points.Count; i++)
			{
				this.points[i].position = TSVector.Transform(this.points[i].position - center, orientation);
			}
		}

		public TSVector CalculateCenter()
		{
			throw new NotImplementedException();
		}

		private HashSet<SoftBody.Edge> GetEdges(List<TriangleVertexIndices> indices)
		{
			HashSet<SoftBody.Edge> hashSet = new HashSet<SoftBody.Edge>();
			for (int i = 0; i < indices.Count; i++)
			{
				SoftBody.Edge item = new SoftBody.Edge(indices[i].I0, indices[i].I1);
				bool flag = !hashSet.Contains(item);
				if (flag)
				{
					hashSet.Add(item);
				}
				item = new SoftBody.Edge(indices[i].I1, indices[i].I2);
				bool flag2 = !hashSet.Contains(item);
				if (flag2)
				{
					hashSet.Add(item);
				}
				item = new SoftBody.Edge(indices[i].I2, indices[i].I0);
				bool flag3 = !hashSet.Contains(item);
				if (flag3)
				{
					hashSet.Add(item);
				}
			}
			return hashSet;
		}

		public virtual void DoSelfCollision(CollisionDetectedHandler collision)
		{
			bool flag = !this.selfCollision;
			if (!flag)
			{
				for (int i = 0; i < this.points.Count; i++)
				{
					this.queryList.Clear();
					this.dynamicTree.Query(this.queryList, ref this.points[i].boundingBox);
					for (int j = 0; j < this.queryList.Count; j++)
					{
						SoftBody.Triangle userData = this.dynamicTree.GetUserData(this.queryList[j]);
						bool flag2 = userData.VertexBody1 != this.points[i] && userData.VertexBody2 != this.points[i] && userData.VertexBody3 != this.points[i];
						if (flag2)
						{
							TSVector tSVector;
							TSVector normal;
							FP penetration;
							bool flag3 = XenoCollide.Detect(this.points[i].Shape, userData, ref this.points[i].orientation, ref TSMatrix.InternalIdentity, ref this.points[i].position, ref TSVector.InternalZero, out tSVector, out normal, out penetration);
							if (flag3)
							{
								int index = CollisionSystem.FindNearestTrianglePoint(this, this.queryList[j], ref tSVector);
								collision(this.points[i], this.points[index], tSVector, tSVector, normal, penetration);
							}
						}
					}
				}
			}
		}

		private void AddPointsAndSprings(List<TriangleVertexIndices> indices, List<TSVector> vertices)
		{
			for (int i = 0; i < vertices.Count; i++)
			{
				SoftBody.MassPoint massPoint = new SoftBody.MassPoint(this.sphere, this, this.material);
				massPoint.Position = vertices[i];
				massPoint.Mass = FP.EN1;
				this.points.Add(massPoint);
			}
			for (int j = 0; j < indices.Count; j++)
			{
				TriangleVertexIndices indices2 = indices[j];
				SoftBody.Triangle triangle = new SoftBody.Triangle(this);
				triangle.indices = indices2;
				this.triangles.Add(triangle);
				triangle.boundingBox = TSBBox.SmallBox;
				triangle.boundingBox.AddPoint(this.points[triangle.indices.I0].position);
				triangle.boundingBox.AddPoint(this.points[triangle.indices.I1].position);
				triangle.boundingBox.AddPoint(this.points[triangle.indices.I2].position);
				triangle.dynamicTreeID = this.dynamicTree.AddProxy(ref triangle.boundingBox, triangle);
			}
			HashSet<SoftBody.Edge> edges = this.GetEdges(indices);
			int num = 0;
			foreach (SoftBody.Edge current in edges)
			{
				SoftBody.Spring spring = new SoftBody.Spring(this.points[current.Index1], this.points[current.Index2]);
				spring.Softness = FP.EN2;
				spring.BiasFactor = FP.EN1;
				spring.SpringType = SoftBody.SpringType.EdgeSpring;
				this.springs.Add(spring);
				num++;
			}
		}

		public void SetSpringValues(FP bias, FP softness)
		{
			this.SetSpringValues(SoftBody.SpringType.EdgeSpring | SoftBody.SpringType.ShearSpring | SoftBody.SpringType.BendSpring, bias, softness);
		}

		public void SetSpringValues(SoftBody.SpringType type, FP bias, FP softness)
		{
			for (int i = 0; i < this.springs.Count; i++)
			{
				bool flag = (this.springs[i].SpringType & type) > (SoftBody.SpringType)0;
				if (flag)
				{
					this.springs[i].Softness = softness;
					this.springs[i].BiasFactor = bias;
				}
			}
		}

		public virtual void Update(FP timestep)
		{
			this.active = false;
			foreach (SoftBody.MassPoint current in this.points)
			{
				bool flag = current.isActive && !current.isStatic;
				if (flag)
				{
					this.active = true;
					break;
				}
			}
			bool flag2 = !this.active;
			if (!flag2)
			{
				this.box = TSBBox.SmallBox;
				this.volume = FP.Zero;
				this.mass = FP.Zero;
				foreach (SoftBody.MassPoint current2 in this.points)
				{
					this.mass += current2.Mass;
					this.box.AddPoint(current2.position);
				}
				this.box.min = this.box.min - new TSVector(this.TriangleExpansion);
				this.box.max = this.box.max + new TSVector(this.TriangleExpansion);
				foreach (SoftBody.Triangle current3 in this.triangles)
				{
					current3.UpdateBoundingBox();
					TSVector value = current3.VertexBody1.linearVelocity + current3.VertexBody2.linearVelocity + current3.VertexBody3.linearVelocity;
					value *= FP.One / (3 * FP.One);
					this.dynamicTree.MoveProxy(current3.dynamicTreeID, ref current3.boundingBox, value * timestep);
					TSVector position = this.points[current3.indices.I0].position;
					TSVector position2 = this.points[current3.indices.I1].position;
					TSVector position3 = this.points[current3.indices.I2].position;
					this.volume -= ((position2.y - position.y) * (position3.z - position.z) - (position2.z - position.z) * (position3.y - position.y)) * (position.x + position2.x + position3.x);
				}
				this.volume /= 6;
				this.AddPressureForces(timestep);
			}
		}
	}
}
