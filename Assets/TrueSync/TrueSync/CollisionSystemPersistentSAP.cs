namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    public class CollisionSystemPersistentSAP : CollisionSystem
    {
        public List<IBroadphaseEntity> activeList = new List<IBroadphaseEntity>();
        public int addCounter = 0;
        private const int AddedObjectsBruteForceIsUsed = 250;
        public List<SweepPoint> axis1 = new List<SweepPoint>();
        public List<SweepPoint> axis2 = new List<SweepPoint>();
        public List<SweepPoint> axis3 = new List<SweepPoint>();
        public List<IBroadphaseEntity> bodyList = new List<IBroadphaseEntity>();
        public Stack<OverlapPair> depricated = new Stack<OverlapPair>();
        private Action<object> detectCallback;
        public HashList<OverlapPair> fullOverlaps = new HashList<OverlapPair>();
        private Action<object> sortCallback;
        public bool swapOrder = false;

        public CollisionSystemPersistentSAP()
        {
            this.detectCallback = new Action<object>(this.DetectCallback);
            this.sortCallback = new Action<object>(this.SortCallback);
        }

        public override void AddEntity(IBroadphaseEntity body)
        {
            this.bodyList.Add(body);
            this.axis1.Add(new SweepPoint(body, true, 0));
            this.axis1.Add(new SweepPoint(body, false, 0));
            this.axis2.Add(new SweepPoint(body, true, 1));
            this.axis2.Add(new SweepPoint(body, false, 1));
            this.axis3.Add(new SweepPoint(body, true, 2));
            this.axis3.Add(new SweepPoint(body, false, 2));
            this.addCounter++;
        }

        public override void Detect(bool multiThreaded)
        {
            if (this.addCounter > 250)
            {
                this.fullOverlaps.Clear();
                this.DirtySortAxis(this.axis1);
                this.DirtySortAxis(this.axis2);
                this.DirtySortAxis(this.axis3);
            }
            else if (multiThreaded)
            {
                base.threadManager.AddTask(this.sortCallback, this.axis1);
                base.threadManager.AddTask(this.sortCallback, this.axis2);
                base.threadManager.AddTask(this.sortCallback, this.axis3);
                base.threadManager.Execute();
            }
            else
            {
                this.sortCallback(this.axis1);
                this.sortCallback(this.axis2);
                this.sortCallback(this.axis3);
            }
            this.addCounter = 0;
            foreach (OverlapPair pair in this.fullOverlaps)
            {
                if (!base.CheckBothStaticNonKinematic(pair.Entity1, pair.Entity2) && base.RaisePassedBroadphase(pair.Entity1, pair.Entity2))
                {
                    if (multiThreaded)
                    {
                        CollisionSystem.BroadphasePair param = CollisionSystem.BroadphasePair.Pool.GetNew();
                        if (this.swapOrder)
                        {
                            param.Entity1 = pair.Entity1;
                            param.Entity2 = pair.Entity2;
                        }
                        else
                        {
                            param.Entity2 = pair.Entity2;
                            param.Entity1 = pair.Entity1;
                        }
                        base.threadManager.AddTask(this.detectCallback, param);
                    }
                    else if (this.swapOrder)
                    {
                        this.Detect(pair.Entity1, pair.Entity2);
                    }
                    else
                    {
                        this.Detect(pair.Entity2, pair.Entity1);
                    }
                    this.swapOrder = !this.swapOrder;
                }
            }
            if (multiThreaded)
            {
                base.threadManager.Execute();
            }
        }

        private void DetectCallback(object obj)
        {
            CollisionSystem.BroadphasePair pair = obj as CollisionSystem.BroadphasePair;
            base.Detect(pair.Entity1, pair.Entity2);
            CollisionSystem.BroadphasePair.Pool.GiveBack(pair);
        }

        private void DirtySortAxis(List<SweepPoint> axis)
        {
            axis.Sort(new Comparison<SweepPoint>(this.QuickSort));
            this.activeList.Clear();
            for (int i = 0; i < axis.Count; i++)
            {
                SweepPoint point = axis[i];
                if (point.Begin)
                {
                    foreach (IBroadphaseEntity entity in this.activeList)
                    {
                        if (base.CheckBoundingBoxes(entity, point.Body))
                        {
                            this.fullOverlaps.Add(new OverlapPair(entity, point.Body));
                        }
                    }
                    this.activeList.Add(point.Body);
                }
                else
                {
                    this.activeList.Remove(point.Body);
                }
            }
        }

        private int QuickSort(SweepPoint sweepPoint1, SweepPoint sweepPoint2)
        {
            FP fp = sweepPoint1.Value;
            FP fp2 = sweepPoint2.Value;
            if (fp > fp2)
            {
                return 1;
            }
            if (fp2 > fp)
            {
                return -1;
            }
            return 0;
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
            int num = 0;
            for (int i = 0; i < this.axis1.Count; i++)
            {
                if (this.axis1[i].Body == body)
                {
                    num++;
                    this.axis1.RemoveAt(i);
                    if (num == 2)
                    {
                        break;
                    }
                    i--;
                }
            }
            num = 0;
            for (int j = 0; j < this.axis2.Count; j++)
            {
                if (this.axis2[j].Body == body)
                {
                    num++;
                    this.axis2.RemoveAt(j);
                    if (num == 2)
                    {
                        break;
                    }
                    j--;
                }
            }
            num = 0;
            for (int k = 0; k < this.axis3.Count; k++)
            {
                if (this.axis3[k].Body == body)
                {
                    num++;
                    this.axis3.RemoveAt(k);
                    if (num == 2)
                    {
                        break;
                    }
                    k--;
                }
            }
            foreach (OverlapPair pair in this.fullOverlaps)
            {
                if ((pair.Entity1 == body) || (pair.Entity2 == body))
                {
                    this.depricated.Push(pair);
                }
            }
            while (this.depricated.Count > 0)
            {
                this.fullOverlaps.Remove(this.depricated.Pop());
            }
            this.bodyList.Remove(body);
            return true;
        }

        private void SortAxis(List<SweepPoint> axis)
        {
            for (int i = 1; i < axis.Count; i++)
            {
                SweepPoint point = axis[i];
                FP fp = point.Value;
                int num2 = i - 1;
                while ((num2 >= 0) && (axis[num2].Value > fp))
                {
                    SweepPoint point2 = axis[num2];
                    if ((point.Begin && !point2.Begin) && base.CheckBoundingBoxes(point2.Body, point.Body))
                    {
                        HashList<OverlapPair> fullOverlaps = this.fullOverlaps;
                        lock (fullOverlaps)
                        {
                            this.fullOverlaps.Add(new OverlapPair(point2.Body, point.Body));
                        }
                    }
                    if (!point.Begin && point2.Begin)
                    {
                        HashList<OverlapPair> list2 = this.fullOverlaps;
                        lock (list2)
                        {
                            this.fullOverlaps.Remove(new OverlapPair(point2.Body, point.Body));
                        }
                    }
                    axis[num2 + 1] = point2;
                    num2--;
                }
                axis[num2 + 1] = point;
            }
        }

        private void SortCallback(object obj)
        {
            this.SortAxis(obj as List<SweepPoint>);
        }
    }
}

