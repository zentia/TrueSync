// Decompiled with JetBrains decompiler
// Type: TrueSync.SoftBody
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace TrueSync
{
    public class SoftBody : IBroadphaseEntity, IComparable
    {
        private SphereShape sphere = new SphereShape(FP.EN1);
        protected List<SoftBody.Spring> springs = new List<SoftBody.Spring>();
        protected List<SoftBody.MassPoint> points = new List<SoftBody.MassPoint>();
        protected List<SoftBody.Triangle> triangles = new List<SoftBody.Triangle>();
        protected FP triangleExpansion = FP.EN1;
        private bool selfCollision = false;
        private FP volume = FP.One;
        private FP mass = FP.One;
        internal TrueSync.DynamicTree<SoftBody.Triangle> dynamicTree = new TrueSync.DynamicTree<SoftBody.Triangle>();
        private BodyMaterial material = new BodyMaterial();
        private TSBBox box = new TSBBox();
        private bool active = true;
        private FP pressure = FP.Zero;
        private List<int> queryList = new List<int>();

        public int CompareTo(object otherObj)
        {
            SoftBody softBody = (SoftBody)otherObj;
            if (softBody.volume < this.volume)
                return -1;
            return softBody.volume > this.volume ? 1 : 0;
        }

        public ReadOnlyCollection<SoftBody.Spring> EdgeSprings { get; private set; }

        public ReadOnlyCollection<SoftBody.MassPoint> VertexBodies { get; private set; }

        public ReadOnlyCollection<SoftBody.Triangle> Triangles { private set; get; }

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

        public TrueSync.DynamicTree<SoftBody.Triangle> DynamicTree
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

        public SoftBody()
        {
        }

        public SoftBody(int sizeX, int sizeY, FP scale)
        {
            List<TriangleVertexIndices> indices = new List<TriangleVertexIndices>();
            List<TSVector> vertices = new List<TSVector>();
            for (int x = 0; x < sizeY; ++x)
            {
                for (int z = 0; z < sizeX; ++z)
                    vertices.Add(new TSVector(x, 0, z) * scale);
            }
            for (int index1 = 0; index1 < sizeX - 1; ++index1)
            {
                for (int index2 = 0; index2 < sizeY - 1; ++index2)
                {
                    TriangleVertexIndices triangleVertexIndices = new TriangleVertexIndices();
                    triangleVertexIndices.I0 = index2 * sizeX + index1;
                    triangleVertexIndices.I1 = index2 * sizeX + index1 + 1;
                    triangleVertexIndices.I2 = (index2 + 1) * sizeX + index1 + 1;
                    indices.Add(triangleVertexIndices);
                    triangleVertexIndices.I0 = index2 * sizeX + index1;
                    triangleVertexIndices.I1 = (index2 + 1) * sizeX + index1 + 1;
                    triangleVertexIndices.I2 = (index2 + 1) * sizeX + index1;
                    indices.Add(triangleVertexIndices);
                }
            }
            this.EdgeSprings = this.springs.AsReadOnly();
            this.VertexBodies = this.points.AsReadOnly();
            this.Triangles = this.triangles.AsReadOnly();
            this.AddPointsAndSprings(indices, vertices);
            for (int index1 = 0; index1 < sizeX - 1; ++index1)
            {
                for (int index2 = 0; index2 < sizeY - 1; ++index2)
                    this.springs.Add(new SoftBody.Spring((RigidBody)this.points[index2 * sizeX + index1 + 1], (RigidBody)this.points[(index2 + 1) * sizeX + index1])
                    {
                        Softness = FP.EN2,
                        BiasFactor = FP.EN1
                    });
            }
            foreach (SoftBody.Spring spring in this.springs)
            {
                TSVector tsVector = spring.body1.position - spring.body2.position;
                int num = !(tsVector.z != FP.Zero) ? 0 : (tsVector.x != FP.Zero ? 1 : 0);
                spring.SpringType = num == 0 ? SoftBody.SpringType.EdgeSpring : SoftBody.SpringType.ShearSpring;
            }
            for (int index1 = 0; index1 < sizeX - 2; ++index1)
            {
                for (int index2 = 0; index2 < sizeY - 2; ++index2)
                {
                    SoftBody.Spring spring1 = new SoftBody.Spring((RigidBody)this.points[index2 * sizeX + index1], (RigidBody)this.points[index2 * sizeX + index1 + 2]);
                    spring1.Softness = FP.EN2;
                    spring1.BiasFactor = FP.EN1;
                    SoftBody.Spring spring2 = new SoftBody.Spring((RigidBody)this.points[index2 * sizeX + index1], (RigidBody)this.points[(index2 + 2) * sizeX + index1]);
                    spring2.Softness = FP.EN2;
                    spring2.BiasFactor = FP.EN1;
                    spring1.SpringType = SoftBody.SpringType.BendSpring;
                    spring2.SpringType = SoftBody.SpringType.BendSpring;
                    this.springs.Add(spring1);
                    this.springs.Add(spring2);
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

        private void AddPressureForces(FP timeStep)
        {
            if (this.pressure == FP.Zero || this.volume == FP.Zero)
                return;
            FP fp = FP.One / this.volume;
            foreach (SoftBody.Triangle triangle in this.triangles)
            {
                TSVector position1 = this.points[triangle.indices.I0].position;
                TSVector position2 = this.points[triangle.indices.I1].position;
                TSVector tsVector = (this.points[triangle.indices.I2].position - position1) % (position2 - position1);
                this.points[triangle.indices.I0].AddForce(fp * tsVector * this.pressure);
                this.points[triangle.indices.I1].AddForce(fp * tsVector * this.pressure);
                this.points[triangle.indices.I2].AddForce(fp * tsVector * this.pressure);
            }
        }

        public void Translate(TSVector position)
        {
            foreach (SoftBody.MassPoint point in this.points)
            {
                TSVector tsVector = point.Position + position;
                point.Position = tsVector;
            }
            this.Update(FP.Epsilon);
        }

        public void AddForce(TSVector force)
        {
            throw new NotImplementedException();
        }

        public void Rotate(TSMatrix orientation, TSVector center)
        {
            for (int index = 0; index < this.points.Count; ++index)
                this.points[index].position = TSVector.Transform(this.points[index].position - center, orientation);
        }

        public TSVector CalculateCenter()
        {
            throw new NotImplementedException();
        }

        private HashSet<SoftBody.Edge> GetEdges(List<TriangleVertexIndices> indices)
        {
            HashSet<SoftBody.Edge> edgeSet = new HashSet<SoftBody.Edge>();
            for (int index = 0; index < indices.Count; ++index)
            {
                SoftBody.Edge edge = new SoftBody.Edge(indices[index].I0, indices[index].I1);
                if (!edgeSet.Contains(edge))
                    edgeSet.Add(edge);
                edge = new SoftBody.Edge(indices[index].I1, indices[index].I2);
                if (!edgeSet.Contains(edge))
                    edgeSet.Add(edge);
                edge = new SoftBody.Edge(indices[index].I2, indices[index].I0);
                if (!edgeSet.Contains(edge))
                    edgeSet.Add(edge);
            }
            return edgeSet;
        }

        public virtual void DoSelfCollision(CollisionDetectedHandler collision)
        {
            if (!this.selfCollision)
                return;
            for (int index1 = 0; index1 < this.points.Count; ++index1)
            {
                this.queryList.Clear();
                this.dynamicTree.Query(this.queryList, ref this.points[index1].boundingBox);
                for (int index2 = 0; index2 < this.queryList.Count; ++index2)
                {
                    SoftBody.Triangle userData = this.dynamicTree.GetUserData(this.queryList[index2]);
                    TSVector point;
                    TSVector normal;
                    FP penetration;
                    if (userData.VertexBody1 != this.points[index1] && userData.VertexBody2 != this.points[index1] && userData.VertexBody3 != this.points[index1] && XenoCollide.Detect((ISupportMappable)this.points[index1].Shape, (ISupportMappable)userData, ref this.points[index1].orientation, ref TSMatrix.InternalIdentity, ref this.points[index1].position, ref TSVector.InternalZero, out point, out normal, out penetration))
                    {
                        int nearestTrianglePoint = CollisionSystem.FindNearestTrianglePoint(this, this.queryList[index2], ref point);
                        collision((RigidBody)this.points[index1], (RigidBody)this.points[nearestTrianglePoint], point, point, normal, penetration);
                    }
                }
            }
        }

        private void AddPointsAndSprings(List<TriangleVertexIndices> indices, List<TSVector> vertices)
        {
            for (int index = 0; index < vertices.Count; ++index)
            {
                SoftBody.MassPoint massPoint = new SoftBody.MassPoint((Shape)this.sphere, this, this.material);
                massPoint.Position = vertices[index];
                massPoint.Mass = FP.EN1;
                this.points.Add(massPoint);
            }
            for (int index1 = 0; index1 < indices.Count; ++index1)
            {
                TriangleVertexIndices index2 = indices[index1];
                SoftBody.Triangle userData = new SoftBody.Triangle(this);
                userData.indices = index2;
                this.triangles.Add(userData);
                userData.boundingBox = TSBBox.SmallBox;
                userData.boundingBox.AddPoint(this.points[userData.indices.I0].position);
                userData.boundingBox.AddPoint(this.points[userData.indices.I1].position);
                userData.boundingBox.AddPoint(this.points[userData.indices.I2].position);
                userData.dynamicTreeID = this.dynamicTree.AddProxy(ref userData.boundingBox, userData);
            }
            HashSet<SoftBody.Edge> edges = this.GetEdges(indices);
            int num = 0;
            foreach (SoftBody.Edge edge in edges)
            {
                this.springs.Add(new SoftBody.Spring((RigidBody)this.points[edge.Index1], (RigidBody)this.points[edge.Index2])
                {
                    Softness = FP.EN2,
                    BiasFactor = FP.EN1,
                    SpringType = SoftBody.SpringType.EdgeSpring
                });
                ++num;
            }
        }

        public void SetSpringValues(FP bias, FP softness)
        {
            this.SetSpringValues(SoftBody.SpringType.EdgeSpring | SoftBody.SpringType.ShearSpring | SoftBody.SpringType.BendSpring, bias, softness);
        }

        public void SetSpringValues(SoftBody.SpringType type, FP bias, FP softness)
        {
            for (int index = 0; index < this.springs.Count; ++index)
            {
                if ((uint)(this.springs[index].SpringType & type) > 0U)
                {
                    this.springs[index].Softness = softness;
                    this.springs[index].BiasFactor = bias;
                }
            }
        }

        public virtual void Update(FP timestep)
        {
            this.active = false;
            foreach (SoftBody.MassPoint point in this.points)
            {
                if (point.isActive && !point.isStatic)
                {
                    this.active = true;
                    break;
                }
            }
            if (!this.active)
                return;
            this.box = TSBBox.SmallBox;
            this.volume = FP.Zero;
            this.mass = FP.Zero;
            foreach (SoftBody.MassPoint point in this.points)
            {
                this.mass = this.mass + point.Mass;
                this.box.AddPoint(point.position);
            }
            this.box.min -= new TSVector(this.TriangleExpansion);
            this.box.max += new TSVector(this.TriangleExpansion);
            foreach (SoftBody.Triangle triangle in this.triangles)
            {
                triangle.UpdateBoundingBox();
                TSVector tsVector = (triangle.VertexBody1.linearVelocity + triangle.VertexBody2.linearVelocity + triangle.VertexBody3.linearVelocity) * (FP.One / ((FP)3 * FP.One));
                this.dynamicTree.MoveProxy(triangle.dynamicTreeID, ref triangle.boundingBox, tsVector * timestep);
                TSVector position1 = this.points[triangle.indices.I0].position;
                TSVector position2 = this.points[triangle.indices.I1].position;
                TSVector position3 = this.points[triangle.indices.I2].position;
                this.volume = this.volume - ((position2.y - position1.y) * (position3.z - position1.z) - (position2.z - position1.z) * (position3.y - position1.y)) * (position1.x + position2.x + position3.x);
            }
            this.volume = this.volume / (FP)6;
            this.AddPressureForces(timestep);
        }

        public FP Mass
        {
            get
            {
                return this.mass;
            }
            set
            {
                for (int index = 0; index < this.points.Count; ++index)
                    this.points[index].Mass = value / (FP)this.points.Count;
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

        public int BroadphaseTag { get; set; }

        public object Tag { get; set; }

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

        [Flags]
        public enum SpringType
        {
            EdgeSpring = 2,
            ShearSpring = 4,
            BendSpring = 8,
        }

        public class Spring : Constraint
        {
            private FP biasFactor = FP.EN1;
            private FP softness = FP.EN2;
            private SoftBody.Spring.DistanceBehavior behavior = SoftBody.Spring.DistanceBehavior.LimitDistance;
            private FP effectiveMass = FP.Zero;
            private FP accumulatedImpulse = FP.Zero;
            private TSVector[] jacobian = new TSVector[2];
            private bool skipConstraint = false;
            private FP distance;
            private FP bias;
            private FP softnessOverDt;

            public SoftBody.SpringType SpringType { get; set; }

            public Spring(RigidBody body1, RigidBody body2)
              : base(body1, body2)
            {
                this.distance = (body1.position - body2.position).magnitude;
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

            public override void PrepareForIteration(FP timestep)
            {
                TSVector result;
                TSVector.Subtract(ref this.body2.position, ref this.body1.position, out result);
                FP fp = result.magnitude - this.distance;
                if (this.behavior == SoftBody.Spring.DistanceBehavior.LimitMaximumDistance && fp <= FP.Zero)
                    this.skipConstraint = true;
                else if (this.behavior == SoftBody.Spring.DistanceBehavior.LimitMinimumDistance && fp >= FP.Zero)
                {
                    this.skipConstraint = true;
                }
                else
                {
                    this.skipConstraint = false;
                    TSVector tsVector = result;
                    if (tsVector.sqrMagnitude != FP.Zero)
                        tsVector.Normalize();
                    this.jacobian[0] = -FP.One * tsVector;
                    this.jacobian[1] = FP.One * tsVector;
                    this.effectiveMass = this.body1.inverseMass + this.body2.inverseMass;
                    this.softnessOverDt = this.softness / timestep;
                    this.effectiveMass = this.effectiveMass + this.softnessOverDt;
                    this.effectiveMass = FP.One / this.effectiveMass;
                    this.bias = fp * this.biasFactor * (FP.One / timestep);
                    if (!this.body1.isStatic)
                        this.body1.linearVelocity += this.body1.inverseMass * this.accumulatedImpulse * this.jacobian[0];
                    if (!this.body2.isStatic)
                        this.body2.linearVelocity += this.body2.inverseMass * this.accumulatedImpulse * this.jacobian[1];
                }
            }

            public override void Iterate()
            {
                if (this.skipConstraint)
                    return;
                FP fp = -this.effectiveMass * (TSVector.Dot(ref this.body1.linearVelocity, ref this.jacobian[0]) + TSVector.Dot(ref this.body2.linearVelocity, ref this.jacobian[1]) + this.bias + this.accumulatedImpulse * this.softnessOverDt);
                if (this.behavior == SoftBody.Spring.DistanceBehavior.LimitMinimumDistance)
                {
                    FP accumulatedImpulse = this.accumulatedImpulse;
                    this.accumulatedImpulse = TSMath.Max(this.accumulatedImpulse + fp, (FP)0);
                    fp = this.accumulatedImpulse - accumulatedImpulse;
                }
                else if (this.behavior == SoftBody.Spring.DistanceBehavior.LimitMaximumDistance)
                {
                    FP accumulatedImpulse = this.accumulatedImpulse;
                    this.accumulatedImpulse = TSMath.Min(this.accumulatedImpulse + fp, (FP)0);
                    fp = this.accumulatedImpulse - accumulatedImpulse;
                }
                else
                    this.accumulatedImpulse = this.accumulatedImpulse + fp;
                TSVector result;
                if (!this.body1.isStatic)
                {
                    TSVector.Multiply(ref this.jacobian[0], fp * this.body1.inverseMass, out result);
                    TSVector.Add(ref result, ref this.body1.linearVelocity, out this.body1.linearVelocity);
                }
                if (this.body2.isStatic)
                    return;
                TSVector.Multiply(ref this.jacobian[1], fp * this.body2.inverseMass, out result);
                TSVector.Add(ref result, ref this.body2.linearVelocity, out this.body2.linearVelocity);
            }

            public override void DebugDraw(IDebugDrawer drawer)
            {
                drawer.DrawLine(this.body1.position, this.body2.position);
            }

            public enum DistanceBehavior
            {
                LimitDistance,
                LimitMaximumDistance,
                LimitMinimumDistance,
            }
        }

        public class MassPoint : RigidBody
        {
            public SoftBody SoftBody { get; private set; }

            public MassPoint(Shape shape, SoftBody owner, BodyMaterial material)
              : base(shape, material, true)
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
                TSVector result;
                TSVector.Subtract(ref this.owner.points[this.indices.I1].position, ref this.owner.points[this.indices.I0].position, out result);
                TSVector.Subtract(ref this.owner.points[this.indices.I2].position, ref this.owner.points[this.indices.I0].position, out normal);
                TSVector.Cross(ref result, ref normal, out normal);
            }

            public void UpdateBoundingBox()
            {
                this.boundingBox = TSBBox.SmallBox;
                this.boundingBox.AddPoint(ref this.owner.points[this.indices.I0].position);
                this.boundingBox.AddPoint(ref this.owner.points[this.indices.I1].position);
                this.boundingBox.AddPoint(ref this.owner.points[this.indices.I2].position);
                this.boundingBox.min -= new TSVector(this.owner.triangleExpansion);
                this.boundingBox.max += new TSVector(this.owner.triangleExpansion);
            }

            public FP CalculateArea()
            {
                return ((this.owner.points[this.indices.I1].position - this.owner.points[this.indices.I0].position) % (this.owner.points[this.indices.I2].position - this.owner.points[this.indices.I0].position)).magnitude;
            }

            public void SupportMapping(ref TSVector direction, out TSVector result)
            {
                FP fp1 = TSVector.Dot(ref this.owner.points[this.indices.I0].position, ref direction);
                FP fp2 = TSVector.Dot(ref this.owner.points[this.indices.I1].position, ref direction);
                TSVector position = this.owner.points[this.indices.I0].position;
                if (fp2 > fp1)
                {
                    fp1 = fp2;
                    position = this.owner.points[this.indices.I1].position;
                }
                if (TSVector.Dot(ref this.owner.points[this.indices.I2].position, ref direction) > fp1)
                    position = this.owner.points[this.indices.I2].position;
                TSVector result1;
                TSVector.Normalize(ref direction, out result1);
                result1 *= this.owner.triangleExpansion;
                result = position + result1;
            }

            public void SupportCenter(out TSVector center)
            {
                center = this.owner.points[this.indices.I0].position;
                TSVector.Add(ref center, ref this.owner.points[this.indices.I1].position, out center);
                TSVector.Add(ref center, ref this.owner.points[this.indices.I2].position, out center);
                TSVector.Multiply(ref center, FP.One / ((FP)3 * FP.One), out center);
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
                return edge.Index1 == this.Index1 && edge.Index2 == this.Index2 || edge.Index1 == this.Index2 && edge.Index2 == this.Index1;
            }
        }
    }
}
