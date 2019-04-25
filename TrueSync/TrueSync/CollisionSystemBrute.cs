namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CollisionSystemBrute : CollisionSystem
    {
        private List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();
        private Action<object> detectCallback;
        private bool swapOrder = false;

        public CollisionSystemBrute()
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

        public override void Detect(bool multiThreaded)
        {
            int count = this.bodyList.Count;
            if (multiThreaded)
            {
                for (int i = 0; i < count; i++)
                {
                    for (int j = i + 1; j < count; j++)
                    {
                        if ((!base.CheckBothStaticOrInactive(this.bodyList[i], this.bodyList[j]) && base.CheckBoundingBoxes(this.bodyList[i], this.bodyList[j])) && base.RaisePassedBroadphase(this.bodyList[i], this.bodyList[j]))
                        {
                            CollisionSystem.BroadphasePair param = CollisionSystem.BroadphasePair.Pool.GetNew();
                            if (this.swapOrder)
                            {
                                param.Entity1 = this.bodyList[i];
                                param.Entity2 = this.bodyList[j];
                            }
                            else
                            {
                                param.Entity2 = this.bodyList[j];
                                param.Entity1 = this.bodyList[i];
                            }
                            this.swapOrder = !this.swapOrder;
                            base.threadManager.AddTask(this.detectCallback, param);
                        }
                    }
                }
                base.threadManager.Execute();
            }
            else
            {
                for (int k = 0; k < count; k++)
                {
                    for (int m = k + 1; m < count; m++)
                    {
                        if ((!base.CheckBothStaticOrInactive(this.bodyList[k], this.bodyList[m]) && base.CheckBoundingBoxes(this.bodyList[k], this.bodyList[m])) && base.RaisePassedBroadphase(this.bodyList[k], this.bodyList[m]))
                        {
                            if (this.swapOrder)
                            {
                                this.Detect(this.bodyList[k], this.bodyList[m]);
                            }
                            else
                            {
                                this.Detect(this.bodyList[m], this.bodyList[k]);
                            }
                            this.swapOrder = !this.swapOrder;
                        }
                    }
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
    }
}

