namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CollisionSystemSAP : CollisionSystem
    {
        private List<IBroadphaseEntity> active = new List<IBroadphaseEntity>();
        private List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();
        private Action<object> detectCallback;
        private bool swapOrder = false;
        private IBroadphaseEntityXCompare xComparer = new IBroadphaseEntityXCompare();

        public CollisionSystemSAP()
        {
            this.detectCallback = new Action<object>(this.DetectCallback);
        }

        public override void AddEntity(IBroadphaseEntity body)
        {
            if (this.bodyList.Contains(body))
            {
                throw new ArgumentException("The body was already added to the collision system.", "body");
            }
            this.bodyList.Add(body);
        }

        private void AddToActive(IBroadphaseEntity body, bool addToList)
        {
            FP x = body.BoundingBox.min.x;
            int count = this.active.Count;
            bool isStaticOrInactive = body.IsStaticOrInactive;
            int index = 0;
            while (index != count)
            {
                IBroadphaseEntity entity = this.active[index];
                TSBBox boundingBox = entity.BoundingBox;
                if (boundingBox.max.x < x)
                {
                    count--;
                    this.active.RemoveAt(index);
                }
                else
                {
                    TSBBox box2 = body.BoundingBox;
                    if (((!isStaticOrInactive || !entity.IsStaticOrInactive) && (((box2.max.z >= boundingBox.min.z) && (box2.min.z <= boundingBox.max.z)) && ((box2.max.y >= boundingBox.min.y) && (box2.min.y <= boundingBox.max.y)))) && base.RaisePassedBroadphase(entity, body))
                    {
                        if (this.swapOrder)
                        {
                            this.Detect(body, entity);
                        }
                        else
                        {
                            this.Detect(entity, body);
                        }
                        this.swapOrder = !this.swapOrder;
                    }
                    index++;
                }
            }
            this.active.Add(body);
        }

        private void AddToActiveMultithreaded(IBroadphaseEntity body, bool addToList)
        {
            FP x = body.BoundingBox.min.x;
            int count = this.active.Count;
            bool isStaticOrInactive = body.IsStaticOrInactive;
            int index = 0;
            while (index != count)
            {
                IBroadphaseEntity entity = this.active[index];
                TSBBox boundingBox = entity.BoundingBox;
                if (boundingBox.max.x < x)
                {
                    count--;
                    this.active.RemoveAt(index);
                }
                else
                {
                    TSBBox box2 = body.BoundingBox;
                    if (((!isStaticOrInactive || !entity.IsStaticOrInactive) && (((box2.max.z >= boundingBox.min.z) && (box2.min.z <= boundingBox.max.z)) && ((box2.max.y >= boundingBox.min.y) && (box2.min.y <= boundingBox.max.y)))) && base.RaisePassedBroadphase(entity, body))
                    {
                        CollisionSystem.BroadphasePair param = CollisionSystem.BroadphasePair.Pool.GetNew();
                        if (this.swapOrder)
                        {
                            param.Entity1 = body;
                            param.Entity2 = entity;
                        }
                        else
                        {
                            param.Entity2 = body;
                            param.Entity1 = entity;
                        }
                        this.swapOrder = !this.swapOrder;
                        base.threadManager.AddTask(detectCallback, param);
                    }
                    index++;
                }
            }
            this.active.Add(body);
        }

        private int Compare(IBroadphaseEntity body1, IBroadphaseEntity body2)
        {
            FP fp = body1.BoundingBox.min.x - body2.BoundingBox.min.x;
            return ((fp < 0) ? -1 : ((fp > 0) ? 1 : 0));
        }

        public override void Detect(bool multiThreaded)
        {
            this.bodyList.Sort(this.xComparer);
            this.active.Clear();
            if (multiThreaded)
            {
                for (int i = 0; i < this.bodyList.Count; i++)
                {
                    this.AddToActiveMultithreaded(this.bodyList[i], false);
                }
                base.threadManager.Execute();
            }
            else
            {
                for (int j = 0; j < this.bodyList.Count; j++)
                {
                    this.AddToActive(this.bodyList[j], false);
                }
            }
        }

        private void DetectCallback(object obj)
        {
            CollisionSystem.BroadphasePair pair = obj as CollisionSystem.BroadphasePair;
            base.Detect(pair.Entity1, pair.Entity2);
            CollisionSystem.BroadphasePair.Pool.GiveBack(pair);
        }

        public override bool Raycast(RigidBody body, TSVector rayOrigin, TSVector rayDirection, out TSVector normal, out FP fraction)
        {
            fraction = FP.MaxValue;
            normal = TSVector.zero;
            if (!body.BoundingBox.RayIntersect(ref rayOrigin, ref rayDirection))
            {
                return false;
            }
            if (body.Shape is Multishape)
            {
                TSVector vector2;
                TSVector vector3;
                Multishape support = (body.Shape as Multishape).RequestWorkingClone();
                bool flag4 = false;
                TSVector.Subtract(ref rayOrigin, ref body.position, out vector2);
                TSVector.Transform(ref vector2, ref body.invOrientation, out vector2);
                TSVector.Transform(ref rayDirection, ref body.invOrientation, out vector3);
                int num = support.Prepare(ref vector2, ref vector3);
                for (int i = 0; i < num; i++)
                {
                    TSVector vector;
                    FP fp;
                    support.SetCurrentShape(i);
                    if (GJKCollide.Raycast(support, ref body.orientation, ref body.invOrientation, ref body.position, ref rayOrigin, ref rayDirection, out fp, out vector) && (fp < fraction))
                    {
                        if (base.useTerrainNormal && (support is TerrainShape))
                        {
                            (support as TerrainShape).CollisionNormal(out vector);
                            TSVector.Transform(ref vector, ref body.orientation, out vector);
                            vector.Negate();
                        }
                        else if (base.useTriangleMeshNormal && (support is TriangleMeshShape))
                        {
                            (support as TriangleMeshShape).CollisionNormal(out vector);
                            TSVector.Transform(ref vector, ref body.orientation, out vector);
                            vector.Negate();
                        }
                        normal = vector;
                        fraction = fp;
                        flag4 = true;
                    }
                }
                support.ReturnWorkingClone();
                return flag4;
            }
            return GJKCollide.Raycast(body.Shape, ref body.orientation, ref body.invOrientation, ref body.position, ref rayOrigin, ref rayDirection, out fraction, out normal);
        }

        public override bool Raycast(TSVector rayOrigin, TSVector rayDirection, RaycastCallback raycast, out RigidBody body, out TSVector normal, out FP fraction)
        {
            body = null;
            normal = TSVector.zero;
            fraction = FP.MaxValue;
            bool flag = false;
            foreach (IBroadphaseEntity entity in this.bodyList)
            {
                TSVector vector;
                FP fp;
                if (entity is SoftBody)
                {
                    SoftBody body2 = entity as SoftBody;
                    foreach (RigidBody body3 in body2.VertexBodies)
                    {
                        if (this.Raycast(body3, rayOrigin, rayDirection, out vector, out fp) && ((fp < fraction) && ((raycast == null) || raycast(body3, vector, fp))))
                        {
                            body = body3;
                            normal = vector;
                            fraction = fp;
                            flag = true;
                        }
                    }
                }
                else
                {
                    RigidBody body4 = entity as RigidBody;
                    if (this.Raycast(body4, rayOrigin, rayDirection, out vector, out fp) && ((fp < fraction) && ((raycast == null) || raycast(body4, vector, fp))))
                    {
                        body = body4;
                        normal = vector;
                        fraction = fp;
                        flag = true;
                    }
                }
            }
            return flag;
        }

        public override bool RemoveEntity(IBroadphaseEntity body)
        {
            return this.bodyList.Remove(body);
        }

        private class IBroadphaseEntityXCompare : IComparer<IBroadphaseEntity>
        {
            public int Compare(IBroadphaseEntity body1, IBroadphaseEntity body2)
            {
                FP fp = body1.BoundingBox.min.x - body2.BoundingBox.min.x;
                return ((fp < 0) ? -1 : ((fp > 0) ? 1 : 0));
            }
        }
    }
}

