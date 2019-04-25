namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;

    public class SoftBody : IBroadphaseEntity, IComparable
    {
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int <BroadphaseTag>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ReadOnlyCollection<Spring> <EdgeSprings>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private object <Tag>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ReadOnlyCollection<Triangle> <Triangles>k__BackingField;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private ReadOnlyCollection<MassPoint> <VertexBodies>k__BackingField;
        private bool active;
        private TSBBox box;
        internal DynamicTree<Triangle> dynamicTree;
        private FP mass;
        private BodyMaterial material;
        protected List<MassPoint> points;
        private FP pressure;
        private List<int> queryList;
        private bool selfCollision;
        private SphereShape sphere;
        protected List<Spring> springs;
        protected FP triangleExpansion;
        protected List<Triangle> triangles;
        private FP volume;

        public SoftBody()
        {
            this.sphere = new SphereShape(FP.EN1);
            this.springs = new List<Spring>();
            this.points = new List<MassPoint>();
            this.triangles = new List<Triangle>();
            this.triangleExpansion = FP.EN1;
            this.selfCollision = false;
            this.volume = FP.One;
            this.mass = FP.One;
            this.dynamicTree = new DynamicTree<Triangle>();
            this.material = new BodyMaterial();
            this.box = new TSBBox();
            this.active = true;
            this.pressure = FP.Zero;
            this.queryList = new List<int>();
        }

        public SoftBody(List<TriangleVertexIndices> indices, List<TSVector> vertices)
        {
            this.sphere = new SphereShape(FP.EN1);
            this.springs = new List<Spring>();
            this.points = new List<MassPoint>();
            this.triangles = new List<Triangle>();
            this.triangleExpansion = FP.EN1;
            this.selfCollision = false;
            this.volume = FP.One;
            this.mass = FP.One;
            this.dynamicTree = new DynamicTree<Triangle>();
            this.material = new BodyMaterial();
            this.box = new TSBBox();
            this.active = true;
            this.pressure = FP.Zero;
            this.queryList = new List<int>();
            this.EdgeSprings = this.springs.AsReadOnly();
            this.VertexBodies = this.points.AsReadOnly();
            this.AddPointsAndSprings(indices, vertices);
            this.Triangles = this.triangles.AsReadOnly();
        }

        public SoftBody(int sizeX, int sizeY, FP scale)
        {
            this.sphere = new SphereShape(FP.EN1);
            this.springs = new List<Spring>();
            this.points = new List<MassPoint>();
            this.triangles = new List<Triangle>();
            this.triangleExpansion = FP.EN1;
            this.selfCollision = false;
            this.volume = FP.One;
            this.mass = FP.One;
            this.dynamicTree = new DynamicTree<Triangle>();
            this.material = new BodyMaterial();
            this.box = new TSBBox();
            this.active = true;
            this.pressure = FP.Zero;
            this.queryList = new List<int>();
            List<TriangleVertexIndices> list = new List<TriangleVertexIndices>();
            List<TSVector> vertices = new List<TSVector>();
            for (int i = 0; i < sizeY; i++)
            {
                for (int n = 0; n < sizeX; n++)
                {
                    vertices.Add(new TSVector(i, 0, n) * scale);
                }
            }
            for (int j = 0; j < (sizeX - 1); j++)
            {
                for (int num4 = 0; num4 < (sizeY - 1); num4++)
                {
                    TriangleVertexIndices item = new TriangleVertexIndices {
                        I0 = (num4 * sizeX) + j,
                        I1 = ((num4 * sizeX) + j) + 1,
                        I2 = (((num4 + 1) * sizeX) + j) + 1
                    };
                    list.Add(item);
                    item.I0 = (num4 * sizeX) + j;
                    item.I1 = (((num4 + 1) * sizeX) + j) + 1;
                    item.I2 = ((num4 + 1) * sizeX) + j;
                    list.Add(item);
                }
            }
            this.EdgeSprings = this.springs.AsReadOnly();
            this.VertexBodies = this.points.AsReadOnly();
            this.Triangles = this.triangles.AsReadOnly();
            this.AddPointsAndSprings(list, vertices);
            for (int k = 0; k < (sizeX - 1); k++)
            {
                for (int num6 = 0; num6 < (sizeY - 1); num6++)
                {
                    Spring spring = new Spring(this.points[((num6 * sizeX) + k) + 1], this.points[((num6 + 1) * sizeX) + k]) {
                        Softness = FP.EN2,
                        BiasFactor = FP.EN1
                    };
                    this.springs.Add(spring);
                }
            }
            foreach (Spring spring2 in this.springs)
            {
                TSVector vector = spring2.body1.position - spring2.body2.position;
                if ((vector.z != FP.Zero) && (vector.x != FP.Zero))
                {
                    spring2.SpringType = SpringType.ShearSpring;
                }
                else
                {
                    spring2.SpringType = SpringType.EdgeSpring;
                }
            }
            for (int m = 0; m < (sizeX - 2); m++)
            {
                for (int num8 = 0; num8 < (sizeY - 2); num8++)
                {
                    Spring spring3 = new Spring(this.points[(num8 * sizeX) + m], this.points[((num8 * sizeX) + m) + 2]) {
                        Softness = FP.EN2,
                        BiasFactor = FP.EN1
                    };
                    Spring spring4 = new Spring(this.points[(num8 * sizeX) + m], this.points[((num8 + 2) * sizeX) + m]) {
                        Softness = FP.EN2,
                        BiasFactor = FP.EN1
                    };
                    spring3.SpringType = SpringType.BendSpring;
                    spring4.SpringType = SpringType.BendSpring;
                    this.springs.Add(spring3);
                    this.springs.Add(spring4);
                }
            }
        }

        public void AddForce(TSVector force)
        {
            throw new NotImplementedException();
        }

        private void AddPointsAndSprings(List<TriangleVertexIndices> indices, List<TSVector> vertices)
        {
            for (int i = 0; i < vertices.Count; i++)
            {
                MassPoint item = new MassPoint(this.sphere, this, this.material) {
                    Position = vertices[i],
                    Mass = FP.EN1
                };
                this.points.Add(item);
            }
            for (int j = 0; j < indices.Count; j++)
            {
                TriangleVertexIndices indices2 = indices[j];
                Triangle triangle = new Triangle(this) {
                    indices = indices2
                };
                this.triangles.Add(triangle);
                triangle.boundingBox = TSBBox.SmallBox;
                triangle.boundingBox.AddPoint(this.points[triangle.indices.I0].position);
                triangle.boundingBox.AddPoint(this.points[triangle.indices.I1].position);
                triangle.boundingBox.AddPoint(this.points[triangle.indices.I2].position);
                triangle.dynamicTreeID = this.dynamicTree.AddProxy(ref triangle.boundingBox, triangle);
            }
            HashSet<Edge> edges = this.GetEdges(indices);
            int num = 0;
            foreach (Edge edge in edges)
            {
                Spring spring = new Spring(this.points[edge.Index1], this.points[edge.Index2]) {
                    Softness = FP.EN2,
                    BiasFactor = FP.EN1,
                    SpringType = SpringType.EdgeSpring
                };
                this.springs.Add(spring);
                num++;
            }
        }

        private void AddPressureForces(FP timeStep)
        {
            if ((this.pressure != FP.Zero) && (this.volume != FP.Zero))
            {
                FP fp = FP.One / this.volume;
                foreach (Triangle triangle in this.triangles)
                {
                    TSVector position = this.points[triangle.indices.I0].position;
                    TSVector vector2 = this.points[triangle.indices.I1].position;
                    TSVector vector4 = (this.points[triangle.indices.I2].position - position) % (vector2 - position);
                    this.points[triangle.indices.I0].AddForce((TSVector) ((fp * vector4) * this.pressure));
                    this.points[triangle.indices.I1].AddForce((TSVector) ((fp * vector4) * this.pressure));
                    this.points[triangle.indices.I2].AddForce((TSVector) ((fp * vector4) * this.pressure));
                }
            }
        }

        public TSVector CalculateCenter()
        {
            throw new NotImplementedException();
        }

        public int CompareTo(object otherObj)
        {
            SoftBody body = (SoftBody) otherObj;
            if (body.volume < this.volume)
            {
                return -1;
            }
            if (body.volume > this.volume)
            {
                return 1;
            }
            return 0;
        }

        public virtual void DoSelfCollision(CollisionDetectedHandler collision)
        {
            if (this.selfCollision)
            {
                for (int i = 0; i < this.points.Count; i++)
                {
                    this.queryList.Clear();
                    this.dynamicTree.Query(this.queryList, ref this.points[i].boundingBox);
                    for (int j = 0; j < this.queryList.Count; j++)
                    {
                        TSVector vector;
                        TSVector vector2;
                        FP fp;
                        Triangle userData = this.dynamicTree.GetUserData(this.queryList[j]);
                        if ((((userData.VertexBody1 != this.points[i]) && (userData.VertexBody2 != this.points[i])) && (userData.VertexBody3 != this.points[i])) && XenoCollide.Detect(this.points[i].Shape, userData, ref this.points[i].orientation, ref TSMatrix.InternalIdentity, ref this.points[i].position, ref TSVector.InternalZero, out vector, out vector2, out fp))
                        {
                            int num3 = CollisionSystem.FindNearestTrianglePoint(this, this.queryList[j], ref vector);
                            collision(this.points[i], this.points[num3], vector, vector, vector2, fp);
                        }
                    }
                }
            }
        }

        private HashSet<Edge> GetEdges(List<TriangleVertexIndices> indices)
        {
            HashSet<Edge> set = new HashSet<Edge>();
            for (int i = 0; i < indices.Count; i++)
            {
                Edge item = new Edge(indices[i].I0, indices[i].I1);
                if (!set.Contains(item))
                {
                    set.Add(item);
                }
                item = new Edge(indices[i].I1, indices[i].I2);
                if (!set.Contains(item))
                {
                    set.Add(item);
                }
                item = new Edge(indices[i].I2, indices[i].I0);
                if (!set.Contains(item))
                {
                    set.Add(item);
                }
            }
            return set;
        }

        public void Rotate(TSMatrix orientation, TSVector center)
        {
            for (int i = 0; i < this.points.Count; i++)
            {
                this.points[i].position = TSVector.Transform(this.points[i].position - center, orientation);
            }
        }

        public void SetSpringValues(FP bias, FP softness)
        {
            this.SetSpringValues(SpringType.BendSpring | SpringType.ShearSpring | SpringType.EdgeSpring, bias, softness);
        }

        public void SetSpringValues(SpringType type, FP bias, FP softness)
        {
            for (int i = 0; i < this.springs.Count; i++)
            {
                if ((this.springs[i].SpringType & type) > 0)
                {
                    this.springs[i].Softness = softness;
                    this.springs[i].BiasFactor = bias;
                }
            }
        }

        public void Translate(TSVector position)
        {
            foreach (MassPoint point in this.points)
            {
                point.Position += position;
            }
            this.Update(FP.Epsilon);
        }

        public virtual void Update(FP timestep)
        {
            this.active = false;
            foreach (MassPoint point in this.points)
            {
                if (point.isActive && !point.isStatic)
                {
                    this.active = true;
                    break;
                }
            }
            if (this.active)
            {
                this.box = TSBBox.SmallBox;
                this.volume = FP.Zero;
                this.mass = FP.Zero;
                foreach (MassPoint point2 in this.points)
                {
                    this.mass += point2.Mass;
                    this.box.AddPoint(point2.position);
                }
                this.box.min -= new TSVector(this.TriangleExpansion);
                this.box.max += new TSVector(this.TriangleExpansion);
                foreach (Triangle triangle in this.triangles)
                {
                    triangle.UpdateBoundingBox();
                    TSVector vector = (triangle.VertexBody1.linearVelocity + triangle.VertexBody2.linearVelocity) + triangle.VertexBody3.linearVelocity;
                    vector *= FP.One / (3 * FP.One);
                    this.dynamicTree.MoveProxy(triangle.dynamicTreeID, ref triangle.boundingBox, vector * timestep);
                    TSVector position = this.points[triangle.indices.I0].position;
                    TSVector vector3 = this.points[triangle.indices.I1].position;
                    TSVector vector4 = this.points[triangle.indices.I2].position;
                    this.volume -= (((vector3.y - position.y) * (vector4.z - position.z)) - ((vector3.z - position.z) * (vector4.y - position.y))) * ((position.x + vector3.x) + vector4.x);
                }
                this.volume /= 6;
                this.AddPressureForces(timestep);
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

        public DynamicTree<Triangle> DynamicTree
        {
            get
            {
                return this.dynamicTree;
            }
        }

        public ReadOnlyCollection<Spring> EdgeSprings { get; private set; }

        public bool IsStaticNonKinematic
        {
            get
            {
                return !this.active;
            }
        }

        public bool IsStaticOrInactive
        {
            get
            {
                return !this.active;
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

        public object Tag { get; set; }

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

        public ReadOnlyCollection<Triangle> Triangles { get; private set; }

        public ReadOnlyCollection<MassPoint> VertexBodies { get; private set; }

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

        public FP Volume
        {
            get
            {
                return this.volume;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
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
                return (this.Index1.GetHashCode() + this.Index2.GetHashCode());
            }

            public override bool Equals(object obj)
            {
                SoftBody.Edge edge = (SoftBody.Edge) obj;
                return (((edge.Index1 == this.Index1) && (edge.Index2 == this.Index2)) || ((edge.Index1 == this.Index2) && (edge.Index2 == this.Index1)));
            }
        }

        public class MassPoint : RigidBody
        {
            [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private TrueSync.SoftBody <SoftBody>k__BackingField;

            public MassPoint(Shape shape, TrueSync.SoftBody owner, BodyMaterial material) : base(shape, material, true)
            {
                this.SoftBody = owner;
            }

            public TrueSync.SoftBody SoftBody { get; private set; }
        }

        public class Spring : Constraint
        {
            [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
            private TrueSync.SoftBody.SpringType <SpringType>k__BackingField;
            private FP accumulatedImpulse;
            private DistanceBehavior behavior;
            private FP bias;
            private FP biasFactor;
            private FP distance;
            private FP effectiveMass;
            private TSVector[] jacobian;
            private bool skipConstraint;
            private FP softness;
            private FP softnessOverDt;

            public Spring(RigidBody body1, RigidBody body2) : base(body1, body2)
            {
                this.biasFactor = FP.EN1;
                this.softness = FP.EN2;
                this.behavior = DistanceBehavior.LimitDistance;
                this.effectiveMass = FP.Zero;
                this.accumulatedImpulse = FP.Zero;
                this.jacobian = new TSVector[2];
                this.skipConstraint = false;
                TSVector vector = body1.position - body2.position;
                this.distance = vector.magnitude;
            }

            public override void DebugDraw(IDebugDrawer drawer)
            {
                drawer.DrawLine(base.body1.position, base.body2.position);
            }

            public override void Iterate()
            {
                if (!this.skipConstraint)
                {
                    TSVector vector;
                    FP introduced11 = TSVector.Dot(ref base.body1.linearVelocity, ref this.jacobian[0]);
                    FP fp = introduced11 + TSVector.Dot(ref base.body2.linearVelocity, ref this.jacobian[1]);
                    FP fp2 = this.accumulatedImpulse * this.softnessOverDt;
                    FP fp3 = -this.effectiveMass * ((fp + this.bias) + fp2);
                    if (this.behavior == DistanceBehavior.LimitMinimumDistance)
                    {
                        FP accumulatedImpulse = this.accumulatedImpulse;
                        this.accumulatedImpulse = TSMath.Max(this.accumulatedImpulse + fp3, 0);
                        fp3 = this.accumulatedImpulse - accumulatedImpulse;
                    }
                    else if (this.behavior == DistanceBehavior.LimitMaximumDistance)
                    {
                        FP fp5 = this.accumulatedImpulse;
                        this.accumulatedImpulse = TSMath.Min(this.accumulatedImpulse + fp3, 0);
                        fp3 = this.accumulatedImpulse - fp5;
                    }
                    else
                    {
                        this.accumulatedImpulse += fp3;
                    }
                    if (!base.body1.isStatic)
                    {
                        TSVector.Multiply(ref this.jacobian[0], fp3 * base.body1.inverseMass, out vector);
                        TSVector.Add(ref vector, ref base.body1.linearVelocity, out base.body1.linearVelocity);
                    }
                    if (!base.body2.isStatic)
                    {
                        TSVector.Multiply(ref this.jacobian[1], fp3 * base.body2.inverseMass, out vector);
                        TSVector.Add(ref vector, ref base.body2.linearVelocity, out base.body2.linearVelocity);
                    }
                }
            }

            public override void PrepareForIteration(FP timestep)
            {
                TSVector vector;
                TSVector.Subtract(ref base.body2.position, ref base.body1.position, out vector);
                FP fp = vector.magnitude - this.distance;
                if ((this.behavior == DistanceBehavior.LimitMaximumDistance) && (fp <= FP.Zero))
                {
                    this.skipConstraint = true;
                }
                else if ((this.behavior == DistanceBehavior.LimitMinimumDistance) && (fp >= FP.Zero))
                {
                    this.skipConstraint = true;
                }
                else
                {
                    this.skipConstraint = false;
                    TSVector vector2 = vector;
                    if (vector2.sqrMagnitude != FP.Zero)
                    {
                        vector2.Normalize();
                    }
                    this.jacobian[0] = (TSVector) (-FP.One * vector2);
                    this.jacobian[1] = (TSVector) (FP.One * vector2);
                    this.effectiveMass = base.body1.inverseMass + base.body2.inverseMass;
                    this.softnessOverDt = this.softness / timestep;
                    this.effectiveMass += this.softnessOverDt;
                    this.effectiveMass = FP.One / this.effectiveMass;
                    this.bias = (fp * this.biasFactor) * (FP.One / timestep);
                    if (!base.body1.isStatic)
                    {
                        base.body1.linearVelocity += (base.body1.inverseMass * this.accumulatedImpulse) * this.jacobian[0];
                    }
                    if (!base.body2.isStatic)
                    {
                        base.body2.linearVelocity += (base.body2.inverseMass * this.accumulatedImpulse) * this.jacobian[1];
                    }
                }
            }

            public FP AppliedImpulse
            {
                get
                {
                    return this.accumulatedImpulse;
                }
            }

            public DistanceBehavior Behavior
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

            public TrueSync.SoftBody.SpringType SpringType { get; set; }

            public enum DistanceBehavior
            {
                LimitDistance,
                LimitMaximumDistance,
                LimitMinimumDistance
            }
        }

        [Flags]
        public enum SpringType
        {
            BendSpring = 8,
            EdgeSpring = 2,
            ShearSpring = 4
        }

        public class Triangle : ISupportMappable
        {
            internal TSBBox boundingBox;
            internal int dynamicTreeID;
            internal TriangleVertexIndices indices;
            private SoftBody owner;

            public Triangle(SoftBody owner)
            {
                this.owner = owner;
            }

            public FP CalculateArea()
            {
                TSVector vector = (this.owner.points[this.indices.I1].position - this.owner.points[this.indices.I0].position) % (this.owner.points[this.indices.I2].position - this.owner.points[this.indices.I0].position);
                return vector.magnitude;
            }

            public void GetNormal(out TSVector normal)
            {
                TSVector vector;
                TSVector.Subtract(ref this.owner.points[this.indices.I1].position, ref this.owner.points[this.indices.I0].position, out vector);
                TSVector.Subtract(ref this.owner.points[this.indices.I2].position, ref this.owner.points[this.indices.I0].position, out normal);
                TSVector.Cross(ref vector, ref normal, out normal);
            }

            public void SupportCenter(out TSVector center)
            {
                center = this.owner.points[this.indices.I0].position;
                TSVector.Add(ref center, ref this.owner.points[this.indices.I1].position, out center);
                TSVector.Add(ref center, ref this.owner.points[this.indices.I2].position, out center);
                TSVector.Multiply(ref center, FP.One / (3 * FP.One), out center);
            }

            public void SupportMapping(ref TSVector direction, out TSVector result)
            {
                TSVector vector2;
                FP fp = TSVector.Dot(ref this.owner.points[this.indices.I0].position, ref direction);
                FP fp2 = TSVector.Dot(ref this.owner.points[this.indices.I1].position, ref direction);
                TSVector position = this.owner.points[this.indices.I0].position;
                if (fp2 > fp)
                {
                    fp = fp2;
                    position = this.owner.points[this.indices.I1].position;
                }
                fp2 = TSVector.Dot(ref this.owner.points[this.indices.I2].position, ref direction);
                if (fp2 > fp)
                {
                    fp = fp2;
                    position = this.owner.points[this.indices.I2].position;
                }
                TSVector.Normalize(ref direction, out vector2);
                vector2 *= this.owner.triangleExpansion;
                result = position + vector2;
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

            public SoftBody Owner
            {
                get
                {
                    return this.owner;
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
        }
    }
}

