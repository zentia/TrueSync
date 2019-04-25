using System.Collections.Generic;

namespace TrueSync
{
    public abstract class CollisionSystem
    {
        protected ThreadManager threadManager = ThreadManager.Instance;
        private bool speculativeContacts = false;
        internal bool useTerrainNormal = true;
        internal bool useTriangleMeshNormal = true;
        private ResourcePool<List<int>> potentialTriangleLists = new ResourcePool<List<int>>();
        public World world;
        private CollisionDetectedHandler collisionDetected;

        public abstract bool RemoveEntity(IBroadphaseEntity body);

        public abstract void AddEntity(IBroadphaseEntity body);

        public event PassedBroadphaseHandler PassedBroadphase;

        public event CollisionDetectedHandler CollisionDetected
        {
            add
            {
                this.collisionDetected = this.collisionDetected + value;
            }
            remove
            {
                this.collisionDetected = this.collisionDetected - value;
            }
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

        public virtual void Detect(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            RigidBody rigidBody1 = entity1 as RigidBody;
            RigidBody rigidBody2 = entity2 as RigidBody;
            if (rigidBody1 != null)
            {
                if (rigidBody2 != null)
                {
                    this.DetectRigidRigid(rigidBody1, rigidBody2);
                }
                else
                {
                    SoftBody softBody = entity2 as SoftBody;
                    if (softBody != null)
                        this.DetectSoftRigid(rigidBody1, softBody);
                }
            }
            else
            {
                SoftBody softBody = entity1 as SoftBody;
                if (rigidBody2 != null)
                {
                    if (softBody != null)
                        this.DetectSoftRigid(rigidBody2, softBody);
                }
                else
                {
                    SoftBody body2 = entity2 as SoftBody;
                    if (softBody != null && body2 != null)
                        this.DetectSoftSoft(softBody, body2);
                }
            }
        }

        private void DetectSoftSoft(SoftBody body1, SoftBody body2)
        {
            List<int> my = this.potentialTriangleLists.GetNew();
            List<int> other = this.potentialTriangleLists.GetNew();
            body1.dynamicTree.Query(other, my, body2.dynamicTree);
            for (int index = 0; index < other.Count; ++index)
            {
                TSVector point;
                TSVector normal;
                FP penetration;
                if (XenoCollide.Detect((ISupportMappable)body1.dynamicTree.GetUserData(my[index]), (ISupportMappable)body2.dynamicTree.GetUserData(other[index]), ref TSMatrix.InternalIdentity, ref TSMatrix.InternalIdentity, ref TSVector.InternalZero, ref TSVector.InternalZero, out point, out normal, out penetration))
                {
                    int nearestTrianglePoint1 = CollisionSystem.FindNearestTrianglePoint(body1, my[index], ref point);
                    int nearestTrianglePoint2 = CollisionSystem.FindNearestTrianglePoint(body2, other[index], ref point);
                    this.RaiseCollisionDetected((RigidBody)body1.VertexBodies[nearestTrianglePoint1], (RigidBody)body2.VertexBodies[nearestTrianglePoint2], ref point, ref point, ref normal, penetration);
                }
            }
            my.Clear();
            other.Clear();
            this.potentialTriangleLists.GiveBack(my);
            this.potentialTriangleLists.GiveBack(other);
        }

        private void DetectRigidRigid(RigidBody body1, RigidBody body2)
        {
            bool flag1 = body1.Shape is Multishape;
            bool flag2 = body2.Shape is Multishape;
            bool flag3 = this.speculativeContacts || (body1.EnableSpeculativeContacts || body2.EnableSpeculativeContacts);
            if (!flag1 && !flag2)
            {
                TSVector point;
                TSVector normal;
                FP penetration;
                if (XenoCollide.Detect((ISupportMappable)body1.Shape, (ISupportMappable)body2.Shape, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out point, out normal, out penetration))
                {
                    TSVector point1;
                    TSVector point2;
                    this.FindSupportPoints(body1, body2, body1.Shape, body2.Shape, ref point, ref normal, out point1, out point2);
                    this.RaiseCollisionDetected(body1, body2, ref point1, ref point2, ref normal, penetration);
                }
                else
                {
                    TSVector p1;
                    TSVector p2;
                    if (!flag3 || !GJKCollide.ClosestPoints((ISupportMappable)body1.Shape, (ISupportMappable)body2.Shape, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out p1, out p2, out normal))
                        return;
                    TSVector tsVector = p2 - p1;
                    if (tsVector.sqrMagnitude < (body1.sweptDirection - body2.sweptDirection).sqrMagnitude)
                    {
                        penetration = tsVector * normal;
                        if (penetration < FP.Zero)
                            this.RaiseCollisionDetected(body1, body2, ref p1, ref p2, ref normal, penetration);
                    }
                }
            }
            else if (flag1 & flag2)
            {
                Multishape shape1 = body1.Shape as Multishape;
                Multishape shape2 = body2.Shape as Multishape;
                Multishape multishape1 = shape1.RequestWorkingClone();
                Multishape multishape2 = shape2.RequestWorkingClone();
                TSBBox boundingBox = body2.boundingBox;
                boundingBox.InverseTransform(ref body1.position, ref body1.orientation);
                int num1 = multishape1.Prepare(ref boundingBox);
                boundingBox = body1.boundingBox;
                boundingBox.InverseTransform(ref body2.position, ref body2.orientation);
                int num2 = multishape2.Prepare(ref boundingBox);
                if (num1 == 0 || num2 == 0)
                {
                    multishape1.ReturnWorkingClone();
                    multishape2.ReturnWorkingClone();
                }
                else
                {
                    for (int index1 = 0; index1 < num1; ++index1)
                    {
                        multishape1.SetCurrentShape(index1);
                        for (int index2 = 0; index2 < num2; ++index2)
                        {
                            multishape2.SetCurrentShape(index2);
                            TSVector point;
                            TSVector normal;
                            FP penetration;
                            if (XenoCollide.Detect((ISupportMappable)multishape1, (ISupportMappable)multishape2, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out point, out normal, out penetration))
                            {
                                TSVector point1;
                                TSVector point2;
                                this.FindSupportPoints(body1, body2, (Shape)multishape1, (Shape)multishape2, ref point, ref normal, out point1, out point2);
                                this.RaiseCollisionDetected(body1, body2, ref point1, ref point2, ref normal, penetration);
                            }
                            else
                            {
                                TSVector p1;
                                TSVector p2;
                                if (flag3 && GJKCollide.ClosestPoints((ISupportMappable)multishape1, (ISupportMappable)multishape2, ref body1.orientation, ref body2.orientation, ref body1.position, ref body2.position, out p1, out p2, out normal))
                                {
                                    TSVector tsVector = p2 - p1;
                                    if (tsVector.sqrMagnitude < (body1.sweptDirection - body2.sweptDirection).sqrMagnitude)
                                    {
                                        penetration = tsVector * normal;
                                        if (penetration < FP.Zero)
                                            this.RaiseCollisionDetected(body1, body2, ref p1, ref p2, ref normal, penetration);
                                    }
                                }
                            }
                        }
                    }
                    multishape1.ReturnWorkingClone();
                    multishape2.ReturnWorkingClone();
                }
            }
            else
            {
                RigidBody body1_1;
                RigidBody body2_1;
                if (body2.Shape is Multishape)
                {
                    body1_1 = body2;
                    body2_1 = body1;
                }
                else
                {
                    body2_1 = body2;
                    body1_1 = body1;
                }
                Multishape multishape = (body1_1.Shape as Multishape).RequestWorkingClone();
                TSBBox boundingBox = body2_1.boundingBox;
                boundingBox.InverseTransform(ref body1_1.position, ref body1_1.orientation);
                int num = multishape.Prepare(ref boundingBox);
                if (num == 0)
                {
                    multishape.ReturnWorkingClone();
                }
                else
                {
                    for (int index = 0; index < num; ++index)
                    {
                        multishape.SetCurrentShape(index);
                        TSVector point;
                        TSVector tsVector1;
                        FP penetration;
                        if (XenoCollide.Detect((ISupportMappable)multishape, (ISupportMappable)body2_1.Shape, ref body1_1.orientation, ref body2_1.orientation, ref body1_1.position, ref body2_1.position, out point, out tsVector1, out penetration))
                        {
                            TSVector point1;
                            TSVector point2;
                            this.FindSupportPoints(body1_1, body2_1, (Shape)multishape, body2_1.Shape, ref point, ref tsVector1, out point1, out point2);
                            if (this.useTerrainNormal && multishape is TerrainShape)
                            {
                                (multishape as TerrainShape).CollisionNormal(out tsVector1);
                                TSVector.Transform(ref tsVector1, ref body1_1.orientation, out tsVector1);
                            }
                            else if (this.useTriangleMeshNormal && multishape is TriangleMeshShape)
                            {
                                (multishape as TriangleMeshShape).CollisionNormal(out tsVector1);
                                TSVector.Transform(ref tsVector1, ref body1_1.orientation, out tsVector1);
                            }
                            this.RaiseCollisionDetected(body1_1, body2_1, ref point1, ref point2, ref tsVector1, penetration);
                        }
                        else
                        {
                            TSVector p1;
                            TSVector p2;
                            if (flag3 && GJKCollide.ClosestPoints((ISupportMappable)multishape, (ISupportMappable)body2_1.Shape, ref body1_1.orientation, ref body2_1.orientation, ref body1_1.position, ref body2_1.position, out p1, out p2, out tsVector1))
                            {
                                TSVector tsVector2 = p2 - p1;
                                if (tsVector2.sqrMagnitude < (body1.sweptDirection - body2.sweptDirection).sqrMagnitude)
                                {
                                    penetration = tsVector2 * tsVector1;
                                    if (penetration < FP.Zero)
                                        this.RaiseCollisionDetected(body1_1, body2_1, ref p1, ref p2, ref tsVector1, penetration);
                                }
                            }
                        }
                    }
                    multishape.ReturnWorkingClone();
                }
            }
        }

        private void DetectSoftRigid(RigidBody rigidBody, SoftBody softBody)
        {
            if (rigidBody.Shape is Multishape)
            {
                Multishape multishape = (rigidBody.Shape as Multishape).RequestWorkingClone();
                TSBBox boundingBox = softBody.BoundingBox;
                boundingBox.InverseTransform(ref rigidBody.position, ref rigidBody.orientation);
                int num1 = multishape.Prepare(ref boundingBox);
                List<int> my = this.potentialTriangleLists.GetNew();
                softBody.dynamicTree.Query(my, ref rigidBody.boundingBox);
                foreach (int num2 in my)
                {
                    SoftBody.Triangle userData = softBody.dynamicTree.GetUserData(num2);
                    for (int index = 0; index < num1; ++index)
                    {
                        multishape.SetCurrentShape(index);
                        TSVector point;
                        TSVector normal;
                        FP penetration;
                        if (XenoCollide.Detect((ISupportMappable)multishape, (ISupportMappable)userData, ref rigidBody.orientation, ref TSMatrix.InternalIdentity, ref rigidBody.position, ref TSVector.InternalZero, out point, out normal, out penetration))
                        {
                            int nearestTrianglePoint = CollisionSystem.FindNearestTrianglePoint(softBody, num2, ref point);
                            this.RaiseCollisionDetected(rigidBody, (RigidBody)softBody.VertexBodies[nearestTrianglePoint], ref point, ref point, ref normal, penetration);
                        }
                    }
                }
                my.Clear();
                this.potentialTriangleLists.GiveBack(my);
                multishape.ReturnWorkingClone();
            }
            else
            {
                List<int> my = this.potentialTriangleLists.GetNew();
                softBody.dynamicTree.Query(my, ref rigidBody.boundingBox);
                foreach (int num in my)
                {
                    SoftBody.Triangle userData = softBody.dynamicTree.GetUserData(num);
                    TSVector point;
                    TSVector normal;
                    FP penetration;
                    if (XenoCollide.Detect((ISupportMappable)rigidBody.Shape, (ISupportMappable)userData, ref rigidBody.orientation, ref TSMatrix.InternalIdentity, ref rigidBody.position, ref TSVector.InternalZero, out point, out normal, out penetration))
                    {
                        int nearestTrianglePoint = CollisionSystem.FindNearestTrianglePoint(softBody, num, ref point);
                        this.RaiseCollisionDetected(rigidBody, (RigidBody)softBody.VertexBodies[nearestTrianglePoint], ref point, ref point, ref normal, penetration);
                    }
                }
                my.Clear();
                this.potentialTriangleLists.GiveBack(my);
            }
        }

        public static int FindNearestTrianglePoint(SoftBody sb, int id, ref TSVector point)
        {
            SoftBody.Triangle userData = sb.dynamicTree.GetUserData(id);
            TSVector result1 = sb.VertexBodies[userData.indices.I0].position;
            TSVector.Subtract(ref result1, ref point, out result1);
            FP sqrMagnitude1 = result1.sqrMagnitude;
            TSVector result2 = sb.VertexBodies[userData.indices.I1].position;
            TSVector.Subtract(ref result2, ref point, out result2);
            FP sqrMagnitude2 = result2.sqrMagnitude;
            TSVector result3 = sb.VertexBodies[userData.indices.I2].position;
            TSVector.Subtract(ref result3, ref point, out result3);
            FP sqrMagnitude3 = result3.sqrMagnitude;
            if (sqrMagnitude1 < sqrMagnitude2)
            {
                if (sqrMagnitude1 < sqrMagnitude3)
                    return userData.indices.I0;
                return userData.indices.I2;
            }
            if (sqrMagnitude2 < sqrMagnitude3)
                return userData.indices.I1;
            return userData.indices.I2;
        }

        private void FindSupportPoints(RigidBody body1, RigidBody body2, Shape shape1, Shape shape2, ref TSVector point, ref TSVector normal, out TSVector point1, out TSVector point2)
        {
            TSVector result1;
            TSVector.Negate(ref normal, out result1);
            TSVector result2;
            this.SupportMapping(body1, shape1, ref result1, out result2);
            TSVector result3;
            this.SupportMapping(body2, shape2, ref normal, out result3);
            TSVector.Subtract(ref result2, ref point, out result2);
            TSVector.Subtract(ref result3, ref point, out result3);
            FP scaleFactor1 = TSVector.Dot(ref result2, ref normal);
            FP scaleFactor2 = TSVector.Dot(ref result3, ref normal);
            TSVector.Multiply(ref normal, scaleFactor1, out result2);
            TSVector.Multiply(ref normal, scaleFactor2, out result3);
            TSVector.Add(ref point, ref result2, out point1);
            TSVector.Add(ref point, ref result3, out point2);
        }

        private void SupportMapping(RigidBody body, Shape workingShape, ref TSVector direction, out TSVector result)
        {
            TSVector.Transform(ref direction, ref body.invOrientation, out result);
            workingShape.SupportMapping(ref result, out result);
            TSVector.Transform(ref result, ref body.orientation, out result);
            TSVector.Add(ref result, ref body.position, out result);
        }

        public abstract bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out RigidBody body, out TSVector normal, out FP fraction);

        public abstract bool Raycast(RigidBody body, TSVector rayOrigin, TSVector rayDirection, out TSVector normal, out FP fraction);

        public bool CheckBothStaticOrInactive(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            return entity1.IsStaticOrInactive && entity2.IsStaticOrInactive;
        }

        public bool CheckBothStaticNonKinematic(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            return !this.world.CanBodiesCollide((RigidBody)entity1, (RigidBody)entity2);
        }

        public bool CheckBoundingBoxes(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            TSBBox boundingBox1 = entity1.BoundingBox;
            TSBBox boundingBox2 = entity2.BoundingBox;
            return boundingBox1.max.z >= boundingBox2.min.z && boundingBox1.min.z <= boundingBox2.max.z && (boundingBox1.max.y >= boundingBox2.min.y && boundingBox1.min.y <= boundingBox2.max.y) && (boundingBox1.max.x >= boundingBox2.min.x && boundingBox1.min.x <= boundingBox2.max.x);
        }

        public bool RaisePassedBroadphase(IBroadphaseEntity entity1, IBroadphaseEntity entity2)
        {
            // ISSUE: reference to a compiler-generated field
            if (this.PassedBroadphase != null)
            {
                // ISSUE: reference to a compiler-generated field
                return this.PassedBroadphase(entity1, entity2);
            }
            return true;
        }

        protected void RaiseCollisionDetected(RigidBody body1, RigidBody body2, ref TSVector point1, ref TSVector point2, ref TSVector normal, FP penetration)
        {
            if (collisionDetected == null)
                return;
            this.collisionDetected(body1, body2, point1, point2, normal, penetration);
        }

        public abstract void Detect(bool multiThreaded);

        protected class BroadphasePair
        {
            public static ResourcePool<CollisionSystem.BroadphasePair> Pool = new ResourcePool<CollisionSystem.BroadphasePair>();
            public IBroadphaseEntity Entity1;
            public IBroadphaseEntity Entity2;
        }
    }
}
