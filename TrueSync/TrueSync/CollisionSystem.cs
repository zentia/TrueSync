namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    public abstract class CollisionSystem
    {
        private ResourcePool<List<int>> potentialTriangleLists = new ResourcePool<List<int>>();
        private bool speculativeContacts = false;
        protected ThreadManager threadManager = ThreadManager.Instance;
        internal bool useTerrainNormal = true;
        internal bool useTriangleMeshNormal = true;
        public World world;

        public event CollisionDetectedHandler CollisionDetected;

        [field: CompilerGenerated, DebuggerBrowsable(0)]
        public event PassedBroadphaseHandler PassedBroadphase;

        public abstract void AddEntity(IBroadphaseEntity body);
        public bool CheckBothStaticNonKinematic(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            return !this.world.CanBodiesCollide((RigidBody) entity1, (RigidBody) entity2);
        }

        public bool CheckBothStaticOrInactive(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            return (entity1.IsStaticOrInactive && entity2.IsStaticOrInactive);
        }

        public bool CheckBoundingBoxes(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            TSBBox boundingBox = entity1.BoundingBox;
            TSBBox box2 = entity2.BoundingBox;
            return ((((boundingBox.max.z >= box2.min.z) && (boundingBox.min.z <= box2.max.z)) && ((boundingBox.max.y >= box2.min.y) && (boundingBox.min.y <= box2.max.y))) && ((boundingBox.max.x >= box2.min.x) && (boundingBox.min.x <= box2.max.x)));
        }

        public abstract void Detect(bool multiThreaded);
        public virtual void Detect(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            RigidBody body = entity1 as RigidBody;
            RigidBody body2 = entity2 as RigidBody;
            if (body > null)
            {
                if (body2 > null)
                {
                    this.DetectRigidRigid(body, body2);
                }
                else
                {
                    SoftBody softBody = entity2 as SoftBody;
                    if (softBody > null)
                    {
                        this.DetectSoftRigid(body, softBody);
                    }
                }
            }
            else
            {
                SoftBody body4 = entity1 as SoftBody;
                if (body2 > null)
                {
                    if (body4 > null)
                    {
                        this.DetectSoftRigid(body2, body4);
                    }
                }
                else
                {
                    SoftBody body5 = entity2 as SoftBody;
                    if ((body4 != null) && (body5 > null))
                    {
                        this.DetectSoftSoft(body4, body5);
                    }
                }
            }
        }

        private void DetectRigidRigid(RigidBody body1, RigidBody body2)
        {
            TSVector vector;
            TSVector vector2;
            FP fp;
            TSVector vector8;
            bool flag = body1.Shape is Multishape;
            bool flag2 = body2.Shape is Multishape;
            bool flag3 = this.speculativeContacts || (body1.EnableSpeculativeContacts || body2.EnableSpeculativeContacts);
            if (!flag && !flag2)
            {
                if (XenoCollide.Detect(body1.Shape, body2.Shape, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out vector, out vector2, out fp))
                {
                    TSVector vector3;
                    TSVector vector4;
                    this.FindSupportPoints(body1, body2, body1.Shape, body2.Shape, ref vector, ref vector2, out vector3, out vector4);
                    this.RaiseCollisionDetected(body1, body2, ref vector3, ref vector4, ref vector2, fp);
                }
                else
                {
                    TSVector vector5;
                    TSVector vector6;
                    if (flag3 && GJKCollide.ClosestPoints(body1.Shape, body2.Shape, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out vector5, out vector6, out vector2))
                    {
                        TSVector vector7 = vector6 - vector5;
                        vector8 = body1.sweptDirection - body2.sweptDirection;
                        if (vector7.sqrMagnitude < vector8.sqrMagnitude)
                        {
                            fp = (FP) (vector7 * vector2);
                            if (fp < FP.Zero)
                            {
                                this.RaiseCollisionDetected(body1, body2, ref vector5, ref vector6, ref vector2, fp);
                            }
                        }
                    }
                }
            }
            else if (flag & flag2)
            {
                Multishape shape = body1.Shape as Multishape;
                Multishape multishape2 = body2.Shape as Multishape;
                shape = shape.RequestWorkingClone();
                multishape2 = multishape2.RequestWorkingClone();
                TSBBox boundingBox = body2.boundingBox;
                boundingBox.InverseTransform(ref body1.position, ref body1.orientation);
                int num = shape.Prepare(ref boundingBox);
                boundingBox = body1.boundingBox;
                boundingBox.InverseTransform(ref body2.position, ref body2.orientation);
                int num2 = multishape2.Prepare(ref boundingBox);
                if ((num == 0) || (num2 == 0))
                {
                    shape.ReturnWorkingClone();
                    multishape2.ReturnWorkingClone();
                }
                else
                {
                    for (int i = 0; i < num; i++)
                    {
                        shape.SetCurrentShape(i);
                        for (int j = 0; j < num2; j++)
                        {
                            multishape2.SetCurrentShape(j);
                            if (XenoCollide.Detect(shape, multishape2, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out vector, out vector2, out fp))
                            {
                                TSVector vector9;
                                TSVector vector10;
                                this.FindSupportPoints(body1, body2, shape, multishape2, ref vector, ref vector2, out vector9, out vector10);
                                this.RaiseCollisionDetected(body1, body2, ref vector9, ref vector10, ref vector2, fp);
                            }
                            else
                            {
                                TSVector vector11;
                                TSVector vector12;
                                if (flag3 && GJKCollide.ClosestPoints(shape, multishape2, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out vector11, out vector12, out vector2))
                                {
                                    TSVector vector13 = vector12 - vector11;
                                    vector8 = body1.sweptDirection - body2.sweptDirection;
                                    if (vector13.sqrMagnitude < vector8.sqrMagnitude)
                                    {
                                        fp = (FP) (vector13 * vector2);
                                        if (fp < FP.Zero)
                                        {
                                            this.RaiseCollisionDetected(body1, body2, ref vector11, ref vector12, ref vector2, fp);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    shape.ReturnWorkingClone();
                    multishape2.ReturnWorkingClone();
                }
            }
            else
            {
                RigidBody body;
                RigidBody body3;
                if (body2.Shape is Multishape)
                {
                    body = body2;
                    body3 = body1;
                }
                else
                {
                    body3 = body2;
                    body = body1;
                }
                Multishape multishape3 = body.Shape as Multishape;
                multishape3 = multishape3.RequestWorkingClone();
                TSBBox box = body3.boundingBox;
                box.InverseTransform(ref body.position, ref body.orientation);
                int num5 = multishape3.Prepare(ref box);
                if (num5 == 0)
                {
                    multishape3.ReturnWorkingClone();
                }
                else
                {
                    for (int k = 0; k < num5; k++)
                    {
                        multishape3.SetCurrentShape(k);
                        if (XenoCollide.Detect(multishape3, body3.Shape, ref body.orientation, ref body3.orientation, ref body.position, ref body3.position, out vector, out vector2, out fp))
                        {
                            TSVector vector14;
                            TSVector vector15;
                            this.FindSupportPoints(body, body3, multishape3, body3.Shape, ref vector, ref vector2, out vector14, out vector15);
                            if (this.useTerrainNormal && (multishape3 is TerrainShape))
                            {
                                (multishape3 as TerrainShape).CollisionNormal(out vector2);
                                TSVector.Transform(ref vector2, ref body.orientation, out vector2);
                            }
                            else if (this.useTriangleMeshNormal && (multishape3 is TriangleMeshShape))
                            {
                                (multishape3 as TriangleMeshShape).CollisionNormal(out vector2);
                                TSVector.Transform(ref vector2, ref body.orientation, out vector2);
                            }
                            this.RaiseCollisionDetected(body, body3, ref vector14, ref vector15, ref vector2, fp);
                        }
                        else
                        {
                            TSVector vector16;
                            TSVector vector17;
                            if (flag3 && GJKCollide.ClosestPoints(multishape3, body3.Shape, ref body.orientation, ref body3.orientation, ref body.position, ref body3.position, out vector16, out vector17, out vector2))
                            {
                                TSVector vector18 = vector17 - vector16;
                                vector8 = body1.sweptDirection - body2.sweptDirection;
                                if (vector18.sqrMagnitude < vector8.sqrMagnitude)
                                {
                                    fp = (FP) (vector18 * vector2);
                                    if (fp < FP.Zero)
                                    {
                                        this.RaiseCollisionDetected(body, body3, ref vector16, ref vector17, ref vector2, fp);
                                    }
                                }
                            }
                        }
                    }
                    multishape3.ReturnWorkingClone();
                }
            }
        }

        private void DetectSoftRigid(RigidBody rigidBody, SoftBody softBody)
        {
            if (rigidBody.Shape is Multishape)
            {
                Multishape shape = rigidBody.Shape as Multishape;
                shape = shape.RequestWorkingClone();
                TSBBox boundingBox = softBody.BoundingBox;
                boundingBox.InverseTransform(ref rigidBody.position, ref rigidBody.orientation);
                int num = shape.Prepare(ref boundingBox);
                List<int> my = this.potentialTriangleLists.GetNew();
                softBody.dynamicTree.Query(my, ref rigidBody.boundingBox);
                foreach (int num2 in my)
                {
                    SoftBody.Triangle userData = softBody.dynamicTree.GetUserData(num2);
                    for (int i = 0; i < num; i++)
                    {
                        TSVector vector;
                        TSVector vector2;
                        FP fp;
                        shape.SetCurrentShape(i);
                        if (XenoCollide.Detect(shape, userData, ref rigidBody.orientation, ref TSMatrix.InternalIdentity, ref rigidBody.position, ref TSVector.InternalZero, out vector, out vector2, out fp))
                        {
                            int num4 = FindNearestTrianglePoint(softBody, num2, ref vector);
                            this.RaiseCollisionDetected(rigidBody, softBody.VertexBodies[num4], ref vector, ref vector, ref vector2, fp);
                        }
                    }
                }
                my.Clear();
                this.potentialTriangleLists.GiveBack(my);
                shape.ReturnWorkingClone();
            }
            else
            {
                List<int> list2 = this.potentialTriangleLists.GetNew();
                softBody.dynamicTree.Query(list2, ref rigidBody.boundingBox);
                foreach (int num5 in list2)
                {
                    TSVector vector3;
                    TSVector vector4;
                    FP fp2;
                    SoftBody.Triangle triangle2 = softBody.dynamicTree.GetUserData(num5);
                    if (XenoCollide.Detect(rigidBody.Shape, triangle2, ref rigidBody.orientation, ref TSMatrix.InternalIdentity, ref rigidBody.position, ref TSVector.InternalZero, out vector3, out vector4, out fp2))
                    {
                        int num6 = FindNearestTrianglePoint(softBody, num5, ref vector3);
                        this.RaiseCollisionDetected(rigidBody, softBody.VertexBodies[num6], ref vector3, ref vector3, ref vector4, fp2);
                    }
                }
                list2.Clear();
                this.potentialTriangleLists.GiveBack(list2);
            }
        }

        private void DetectSoftSoft(SoftBody body1, SoftBody body2)
        {
            List<int> my = this.potentialTriangleLists.GetNew();
            List<int> other = this.potentialTriangleLists.GetNew();
            body1.dynamicTree.Query(other, my, body2.dynamicTree);
            for (int i = 0; i < other.Count; i++)
            {
                TSVector vector;
                TSVector vector2;
                FP fp;
                SoftBody.Triangle userData = body1.dynamicTree.GetUserData(my[i]);
                SoftBody.Triangle triangle2 = body2.dynamicTree.GetUserData(other[i]);
                if (XenoCollide.Detect(userData, triangle2, ref TSMatrix.InternalIdentity, ref TSMatrix.InternalIdentity, ref TSVector.InternalZero, ref TSVector.InternalZero, out vector, out vector2, out fp))
                {
                    int num2 = FindNearestTrianglePoint(body1, my[i], ref vector);
                    int num3 = FindNearestTrianglePoint(body2, other[i], ref vector);
                    this.RaiseCollisionDetected(body1.VertexBodies[num2], body2.VertexBodies[num3], ref vector, ref vector, ref vector2, fp);
                }
            }
            my.Clear();
            other.Clear();
            this.potentialTriangleLists.GiveBack(my);
            this.potentialTriangleLists.GiveBack(other);
        }

        public static int FindNearestTrianglePoint(SoftBody sb, int id, ref TSVector point)
        {
            SoftBody.Triangle userData = sb.dynamicTree.GetUserData(id);
            TSVector position = sb.VertexBodies[userData.indices.I0].position;
            TSVector.Subtract(ref position, ref point, out position);
            FP sqrMagnitude = position.sqrMagnitude;
            position = sb.VertexBodies[userData.indices.I1].position;
            TSVector.Subtract(ref position, ref point, out position);
            FP fp2 = position.sqrMagnitude;
            position = sb.VertexBodies[userData.indices.I2].position;
            TSVector.Subtract(ref position, ref point, out position);
            FP fp3 = position.sqrMagnitude;
            if (sqrMagnitude < fp2)
            {
                if (sqrMagnitude < fp3)
                {
                    return userData.indices.I0;
                }
                return userData.indices.I2;
            }
            if (fp2 < fp3)
            {
                return userData.indices.I1;
            }
            return userData.indices.I2;
        }

        private void FindSupportPoints(RigidBody body1, RigidBody body2, Shape shape1, Shape shape2, ref TSVector point, ref TSVector normal, out TSVector point1, out TSVector point2)
        {
            TSVector vector;
            TSVector vector2;
            TSVector vector3;
            TSVector.Negate(ref normal, out vector);
            this.SupportMapping(body1, shape1, ref vector, out vector2);
            this.SupportMapping(body2, shape2, ref normal, out vector3);
            TSVector.Subtract(ref vector2, ref point, out vector2);
            TSVector.Subtract(ref vector3, ref point, out vector3);
            FP scaleFactor = TSVector.Dot(ref vector2, ref normal);
            FP fp2 = TSVector.Dot(ref vector3, ref normal);
            TSVector.Multiply(ref normal, scaleFactor, out vector2);
            TSVector.Multiply(ref normal, fp2, out vector3);
            TSVector.Add(ref point, ref vector2, out point1);
            TSVector.Add(ref point, ref vector3, out point2);
        }

        protected void RaiseCollisionDetected(RigidBody body1, RigidBody body2, ref TSVector point1, ref TSVector point2, ref TSVector normal, FP penetration)
        {
            if (this.collisionDetected > null)
            {
                this.collisionDetected(body1, body2, point1, point2, normal, penetration);
            }
        }

        public bool RaisePassedBroadphase(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            if (this.PassedBroadphase > null)
            {
                return this.PassedBroadphase(entity1, entity2);
            }
            return true;
        }

        public abstract bool Raycast(RigidBody body, TSVector rayOrigin, TSVector rayDirection, out TSVector normal, out FP fraction);
        public abstract bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out RigidBody body, out TSVector normal, out FP fraction);
        public abstract bool RemoveEntity(IBroadphaseEntity body);
        private void SupportMapping(RigidBody body, Shape workingShape, ref TSVector direction, out TSVector result)
        {
            TSVector.Transform(ref direction, ref body.invOrientation, out result);
            workingShape.SupportMapping(ref result, out result);
            TSVector.Transform(ref result, ref body.orientation, out result);
            TSVector.Add(ref result, ref body.position, out result);
        }

        public bool EnableSpeculativeContacts
        {
            get
            {
                return this.speculativeContacts;
            }
            set
            {
                this.speculativeContacts = value;
            }
        }

        public bool UseTerrainNormal
        {
            get
            {
                return this.useTerrainNormal;
            }
            set
            {
                this.useTerrainNormal = value;
            }
        }

        public bool UseTriangleMeshNormal
        {
            get
            {
                return this.useTriangleMeshNormal;
            }
            set
            {
                this.useTriangleMeshNormal = value;
            }
        }

        protected class BroadphasePair
        {
            public IBroadphaseEntity Entity1;
            public IBroadphaseEntity Entity2;
            public static ResourcePool<CollisionSystem.BroadphasePair> Pool = new ResourcePool<CollisionSystem.BroadphasePair>();
        }
    }
}

