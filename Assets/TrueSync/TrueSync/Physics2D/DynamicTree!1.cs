namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public class DynamicTree<T>
    {
        internal int _freeList;
        internal int _nodeCapacity;
        internal int _nodeCount;
        internal TreeNode<T>[] _nodes;
        internal Stack<int> _queryStack;
        internal Stack<int> _raycastStack;
        internal int _root;
        internal const int NullNode = -1;

        public DynamicTree()
        {
            this._raycastStack = new Stack<int>(0x100);
            this._queryStack = new Stack<int>(0x100);
            this._root = -1;
            this._nodeCapacity = 0x10;
            this._nodeCount = 0;
            this._nodes = new TreeNode<T>[this._nodeCapacity];
            for (int i = 0; i < (this._nodeCapacity - 1); i++)
            {
                this._nodes[i] = new TreeNode<T>();
                this._nodes[i].ParentOrNext = i + 1;
                this._nodes[i].Height = 1;
            }
            this._nodes[this._nodeCapacity - 1] = new TreeNode<T>();
            this._nodes[this._nodeCapacity - 1].ParentOrNext = -1;
            this._nodes[this._nodeCapacity - 1].Height = 1;
            this._freeList = 0;
        }

        public int AddProxy(ref AABB aabb, T userData)
        {
            int index = this.AllocateNode();
            TSVector2 vector = new TSVector2(Settings.AABBExtension, Settings.AABBExtension);
            this._nodes[index].AABB.LowerBound = aabb.LowerBound - vector;
            this._nodes[index].AABB.UpperBound = aabb.UpperBound + vector;
            this._nodes[index].UserData = userData;
            this._nodes[index].Height = 0;
            this.InsertLeaf(index);
            return index;
        }

        private int AllocateNode()
        {
            if (this._freeList == -1)
            {
                Debug.Assert(this._nodeCount == this._nodeCapacity);
                TreeNode<T>[] sourceArray = this._nodes;
                this._nodeCapacity *= 2;
                this._nodes = new TreeNode<T>[this._nodeCapacity];
                Array.Copy(sourceArray, this._nodes, this._nodeCount);
                for (int i = this._nodeCount; i < (this._nodeCapacity - 1); i++)
                {
                    this._nodes[i] = new TreeNode<T>();
                    this._nodes[i].ParentOrNext = i + 1;
                    this._nodes[i].Height = -1;
                }
                this._nodes[this._nodeCapacity - 1] = new TreeNode<T>();
                this._nodes[this._nodeCapacity - 1].ParentOrNext = -1;
                this._nodes[this._nodeCapacity - 1].Height = -1;
                this._freeList = this._nodeCount;
            }
            int index = this._freeList;
            this._freeList = this._nodes[index].ParentOrNext;
            this._nodes[index].ParentOrNext = -1;
            this._nodes[index].Child1 = -1;
            this._nodes[index].Child2 = -1;
            this._nodes[index].Height = 0;
            this._nodes[index].UserData = default(T);
            this._nodeCount++;
            return index;
        }

        private int Balance(int iA)
        {
            Debug.Assert(iA != -1);
            TreeNode<T> node = this._nodes[iA];
            if (node.IsLeaf() || (node.Height < 2))
            {
                return iA;
            }
            int index = node.Child1;
            int num2 = node.Child2;
            Debug.Assert((0 <= index) && (index < this._nodeCapacity));
            Debug.Assert((0 <= num2) && (num2 < this._nodeCapacity));
            TreeNode<T> node2 = this._nodes[index];
            TreeNode<T> node3 = this._nodes[num2];
            int num3 = node3.Height - node2.Height;
            if (num3 > 1)
            {
                int num5 = node3.Child1;
                int num6 = node3.Child2;
                TreeNode<T> node4 = this._nodes[num5];
                TreeNode<T> node5 = this._nodes[num6];
                Debug.Assert((0 <= num5) && (num5 < this._nodeCapacity));
                Debug.Assert((0 <= num6) && (num6 < this._nodeCapacity));
                node3.Child1 = iA;
                node3.ParentOrNext = node.ParentOrNext;
                node.ParentOrNext = num2;
                if (node3.ParentOrNext != -1)
                {
                    if (this._nodes[node3.ParentOrNext].Child1 == iA)
                    {
                        this._nodes[node3.ParentOrNext].Child1 = num2;
                    }
                    else
                    {
                        Debug.Assert(this._nodes[node3.ParentOrNext].Child2 == iA);
                        this._nodes[node3.ParentOrNext].Child2 = num2;
                    }
                }
                else
                {
                    this._root = num2;
                }
                if (node4.Height > node5.Height)
                {
                    node3.Child2 = num5;
                    node.Child2 = num6;
                    node5.ParentOrNext = iA;
                    node.AABB.Combine(ref node2.AABB, ref node5.AABB);
                    node3.AABB.Combine(ref node.AABB, ref node4.AABB);
                    node.Height = 1 + Math.Max(node2.Height, node5.Height);
                    node3.Height = 1 + Math.Max(node.Height, node4.Height);
                    return num2;
                }
                node3.Child2 = num6;
                node.Child2 = num5;
                node4.ParentOrNext = iA;
                node.AABB.Combine(ref node2.AABB, ref node4.AABB);
                node3.AABB.Combine(ref node.AABB, ref node5.AABB);
                node.Height = 1 + Math.Max(node2.Height, node4.Height);
                node3.Height = 1 + Math.Max(node.Height, node5.Height);
                return num2;
            }
            if (num3 >= -1)
            {
                return iA;
            }
            int num7 = node2.Child1;
            int num8 = node2.Child2;
            TreeNode<T> node6 = this._nodes[num7];
            TreeNode<T> node7 = this._nodes[num8];
            Debug.Assert((0 <= num7) && (num7 < this._nodeCapacity));
            Debug.Assert((0 <= num8) && (num8 < this._nodeCapacity));
            node2.Child1 = iA;
            node2.ParentOrNext = node.ParentOrNext;
            node.ParentOrNext = index;
            if (node2.ParentOrNext != -1)
            {
                if (this._nodes[node2.ParentOrNext].Child1 == iA)
                {
                    this._nodes[node2.ParentOrNext].Child1 = index;
                }
                else
                {
                    Debug.Assert(this._nodes[node2.ParentOrNext].Child2 == iA);
                    this._nodes[node2.ParentOrNext].Child2 = index;
                }
            }
            else
            {
                this._root = index;
            }
            if (node6.Height > node7.Height)
            {
                node2.Child2 = num7;
                node.Child1 = num8;
                node7.ParentOrNext = iA;
                node.AABB.Combine(ref node3.AABB, ref node7.AABB);
                node2.AABB.Combine(ref node.AABB, ref node6.AABB);
                node.Height = 1 + Math.Max(node3.Height, node7.Height);
                node2.Height = 1 + Math.Max(node.Height, node6.Height);
                return index;
            }
            node2.Child2 = num8;
            node.Child1 = num7;
            node6.ParentOrNext = iA;
            node.AABB.Combine(ref node3.AABB, ref node6.AABB);
            node2.AABB.Combine(ref node.AABB, ref node7.AABB);
            node.Height = 1 + Math.Max(node3.Height, node6.Height);
            node2.Height = 1 + Math.Max(node.Height, node7.Height);
            return index;
        }

        public int ComputeHeight()
        {
            return this.ComputeHeight(this._root);
        }

        public int ComputeHeight(int nodeId)
        {
            Debug.Assert((0 <= nodeId) && (nodeId < this._nodeCapacity));
            TreeNode<T> node = this._nodes[nodeId];
            if (node.IsLeaf())
            {
                return 0;
            }
            int num = this.ComputeHeight(node.Child1);
            int num2 = this.ComputeHeight(node.Child2);
            return (1 + Math.Max(num, num2));
        }

        private void FreeNode(int nodeId)
        {
            Debug.Assert((0 <= nodeId) && (nodeId < this._nodeCapacity));
            Debug.Assert(0 < this._nodeCount);
            this._nodes[nodeId].ParentOrNext = this._freeList;
            this._nodes[nodeId].Height = -1;
            this._freeList = nodeId;
            this._nodeCount--;
        }

        public void GetFatAABB(int proxyId, out AABB fatAABB)
        {
            Debug.Assert((0 <= proxyId) && (proxyId < this._nodeCapacity));
            fatAABB = this._nodes[proxyId].AABB;
        }

        public T GetUserData(int proxyId)
        {
            Debug.Assert((0 <= proxyId) && (proxyId < this._nodeCapacity));
            return this._nodes[proxyId].UserData;
        }

        private void InsertLeaf(int leaf)
        {
            if (this._root == -1)
            {
                this._root = leaf;
                this._nodes[this._root].ParentOrNext = -1;
            }
            else
            {
                AABB aABB = this._nodes[leaf].AABB;
                int index = this._root;
                while (!this._nodes[index].IsLeaf())
                {
                    FP fp5;
                    FP fp6;
                    int num5 = this._nodes[index].Child1;
                    int num6 = this._nodes[index].Child2;
                    FP perimeter = this._nodes[index].AABB.Perimeter;
                    AABB aabb2 = new AABB();
                    aabb2.Combine(ref this._nodes[index].AABB, ref aABB);
                    FP fp2 = aabb2.Perimeter;
                    FP fp3 = 2f * fp2;
                    FP fp4 = 2f * (fp2 - perimeter);
                    if (this._nodes[num5].IsLeaf())
                    {
                        AABB aabb3 = new AABB();
                        aabb3.Combine(ref aABB, ref this._nodes[num5].AABB);
                        fp5 = aabb3.Perimeter + fp4;
                    }
                    else
                    {
                        AABB aabb4 = new AABB();
                        aabb4.Combine(ref aABB, ref this._nodes[num5].AABB);
                        FP fp7 = this._nodes[num5].AABB.Perimeter;
                        fp5 = (aabb4.Perimeter - fp7) + fp4;
                    }
                    if (this._nodes[num6].IsLeaf())
                    {
                        AABB aabb5 = new AABB();
                        aabb5.Combine(ref aABB, ref this._nodes[num6].AABB);
                        fp6 = aabb5.Perimeter + fp4;
                    }
                    else
                    {
                        AABB aabb6 = new AABB();
                        aabb6.Combine(ref aABB, ref this._nodes[num6].AABB);
                        FP fp9 = this._nodes[num6].AABB.Perimeter;
                        fp6 = (aabb6.Perimeter - fp9) + fp4;
                    }
                    if ((fp3 < fp5) && (fp5 < fp6))
                    {
                        break;
                    }
                    if (fp5 < fp6)
                    {
                        index = num5;
                    }
                    else
                    {
                        index = num6;
                    }
                }
                int num2 = index;
                int parentOrNext = this._nodes[num2].ParentOrNext;
                int num4 = this.AllocateNode();
                this._nodes[num4].ParentOrNext = parentOrNext;
                this._nodes[num4].UserData = default(T);
                this._nodes[num4].AABB.Combine(ref aABB, ref this._nodes[num2].AABB);
                this._nodes[num4].Height = this._nodes[num2].Height + 1;
                if (parentOrNext != -1)
                {
                    if (this._nodes[parentOrNext].Child1 == num2)
                    {
                        this._nodes[parentOrNext].Child1 = num4;
                    }
                    else
                    {
                        this._nodes[parentOrNext].Child2 = num4;
                    }
                    this._nodes[num4].Child1 = num2;
                    this._nodes[num4].Child2 = leaf;
                    this._nodes[num2].ParentOrNext = num4;
                    this._nodes[leaf].ParentOrNext = num4;
                }
                else
                {
                    this._nodes[num4].Child1 = num2;
                    this._nodes[num4].Child2 = leaf;
                    this._nodes[num2].ParentOrNext = num4;
                    this._nodes[leaf].ParentOrNext = num4;
                    this._root = num4;
                }
                for (index = this._nodes[leaf].ParentOrNext; index != -1; index = this._nodes[index].ParentOrNext)
                {
                    index = this.Balance(index);
                    int num7 = this._nodes[index].Child1;
                    int num8 = this._nodes[index].Child2;
                    Debug.Assert(num7 != -1);
                    Debug.Assert(num8 != -1);
                    this._nodes[index].Height = 1 + Math.Max(this._nodes[num7].Height, this._nodes[num8].Height);
                    this._nodes[index].AABB.Combine(ref this._nodes[num7].AABB, ref this._nodes[num8].AABB);
                }
            }
        }

        public bool MoveProxy(int proxyId, ref AABB aabb, TSVector2 displacement)
        {
            Debug.Assert((0 <= proxyId) && (proxyId < this._nodeCapacity));
            Debug.Assert(this._nodes[proxyId].IsLeaf());
            if (this._nodes[proxyId].AABB.Contains(ref aabb))
            {
                return false;
            }
            this.RemoveLeaf(proxyId);
            AABB aabb2 = aabb;
            TSVector2 vector = new TSVector2(Settings.AABBExtension, Settings.AABBExtension);
            aabb2.LowerBound -= vector;
            aabb2.UpperBound += vector;
            TSVector2 vector2 = (TSVector2) (Settings.AABBMultiplier * displacement);
            if (vector2.x < 0f)
            {
                aabb2.LowerBound.x += vector2.x;
            }
            else
            {
                aabb2.UpperBound.x += vector2.x;
            }
            if (vector2.y < 0f)
            {
                aabb2.LowerBound.y += vector2.y;
            }
            else
            {
                aabb2.UpperBound.y += vector2.y;
            }
            this._nodes[proxyId].AABB = aabb2;
            this.InsertLeaf(proxyId);
            return true;
        }

        public void Query(Func<int, bool> callback, ref AABB aabb)
        {
            this._queryStack.Clear();
            this._queryStack.Push(this._root);
            while (this._queryStack.Count > 0)
            {
                int index = this._queryStack.Pop();
                if (index != -1)
                {
                    TreeNode<T> node = this._nodes[index];
                    if (AABB.TestOverlap(ref node.AABB, ref aabb))
                    {
                        if (node.IsLeaf())
                        {
                            if (!callback(index))
                            {
                                break;
                            }
                        }
                        else
                        {
                            this._queryStack.Push(node.Child1);
                            this._queryStack.Push(node.Child2);
                        }
                    }
                }
            }
        }

        public void RayCast(Func<RayCastInput, int, FP> callback, ref RayCastInput input)
        {
            TSVector2 vector = input.Point1;
            TSVector2 vector2 = input.Point2;
            TSVector2 vector3 = vector2 - vector;
            Debug.Assert(vector3.LengthSquared() > 0f);
            vector3.Normalize();
            TSVector2 vector4 = MathUtils.Abs(new TSVector2(-vector3.y, vector3.x));
            FP maxFraction = input.MaxFraction;
            AABB b = new AABB();
            TSVector2 vector5 = vector + (maxFraction * (vector2 - vector));
            TSVector2.Min(ref vector, ref vector5, out b.LowerBound);
            TSVector2.Max(ref vector, ref vector5, out b.UpperBound);
            this._raycastStack.Clear();
            this._raycastStack.Push(this._root);
            while (this._raycastStack.Count > 0)
            {
                int index = this._raycastStack.Pop();
                if (index != -1)
                {
                    TreeNode<T> node = this._nodes[index];
                    if (AABB.TestOverlap(ref node.AABB, ref b))
                    {
                        TSVector2 center = node.AABB.Center;
                        TSVector2 extents = node.AABB.Extents;
                        FP fp2 = FP.Abs(TSVector2.Dot(new TSVector2(-vector3.y, vector3.x), vector - center)) - TSVector2.Dot(vector4, extents);
                        if (fp2 <= 0f)
                        {
                            if (node.IsLeaf())
                            {
                                RayCastInput input2;
                                input2.Point1 = input.Point1;
                                input2.Point2 = input.Point2;
                                input2.MaxFraction = maxFraction;
                                FP fp3 = callback(input2, index);
                                if (fp3 == 0f)
                                {
                                    break;
                                }
                                if (fp3 > 0f)
                                {
                                    maxFraction = fp3;
                                    TSVector2 vector8 = vector + (maxFraction * (vector2 - vector));
                                    b.LowerBound = TSVector2.Min(vector, vector8);
                                    b.UpperBound = TSVector2.Max(vector, vector8);
                                }
                            }
                            else
                            {
                                this._raycastStack.Push(node.Child1);
                                this._raycastStack.Push(node.Child2);
                            }
                        }
                    }
                }
            }
        }

        public void RebuildBottomUp()
        {
            int[] numArray = new int[this._nodeCount];
            int index = 0;
            for (int i = 0; i < this._nodeCapacity; i++)
            {
                if (this._nodes[i].Height >= 0)
                {
                    if (this._nodes[i].IsLeaf())
                    {
                        this._nodes[i].ParentOrNext = -1;
                        numArray[index] = i;
                        index++;
                    }
                    else
                    {
                        this.FreeNode(i);
                    }
                }
            }
            while (index > 1)
            {
                FP maxFP = Settings.MaxFP;
                int num3 = -1;
                int num4 = -1;
                for (int j = 0; j < index; j++)
                {
                    AABB aABB = this._nodes[numArray[j]].AABB;
                    for (int k = j + 1; k < index; k++)
                    {
                        AABB aabb2 = this._nodes[numArray[k]].AABB;
                        AABB aabb3 = new AABB();
                        aabb3.Combine(ref aABB, ref aabb2);
                        FP perimeter = aabb3.Perimeter;
                        if (perimeter < maxFP)
                        {
                            num3 = j;
                            num4 = k;
                            maxFP = perimeter;
                        }
                    }
                }
                int num5 = numArray[num3];
                int num6 = numArray[num4];
                TreeNode<T> node = this._nodes[num5];
                TreeNode<T> node2 = this._nodes[num6];
                int num7 = this.AllocateNode();
                TreeNode<T> node3 = this._nodes[num7];
                node3.Child1 = num5;
                node3.Child2 = num6;
                node3.Height = 1 + Math.Max(node.Height, node2.Height);
                node3.AABB.Combine(ref node.AABB, ref node2.AABB);
                node3.ParentOrNext = -1;
                node.ParentOrNext = num7;
                node2.ParentOrNext = num7;
                numArray[num4] = numArray[index - 1];
                numArray[num3] = num7;
                index--;
            }
            this._root = numArray[0];
            this.Validate();
        }

        private void RemoveLeaf(int leaf)
        {
            if (leaf == this._root)
            {
                this._root = -1;
            }
            else
            {
                int num3;
                int parentOrNext = this._nodes[leaf].ParentOrNext;
                int index = this._nodes[parentOrNext].ParentOrNext;
                if (this._nodes[parentOrNext].Child1 == leaf)
                {
                    num3 = this._nodes[parentOrNext].Child2;
                }
                else
                {
                    num3 = this._nodes[parentOrNext].Child1;
                }
                if (index != -1)
                {
                    if (this._nodes[index].Child1 == parentOrNext)
                    {
                        this._nodes[index].Child1 = num3;
                    }
                    else
                    {
                        this._nodes[index].Child2 = num3;
                    }
                    this._nodes[num3].ParentOrNext = index;
                    this.FreeNode(parentOrNext);
                    for (int i = index; i != -1; i = this._nodes[i].ParentOrNext)
                    {
                        i = this.Balance(i);
                        int num5 = this._nodes[i].Child1;
                        int num6 = this._nodes[i].Child2;
                        this._nodes[i].AABB.Combine(ref this._nodes[num5].AABB, ref this._nodes[num6].AABB);
                        this._nodes[i].Height = 1 + Math.Max(this._nodes[num5].Height, this._nodes[num6].Height);
                    }
                }
                else
                {
                    this._root = num3;
                    this._nodes[num3].ParentOrNext = -1;
                    this.FreeNode(parentOrNext);
                }
            }
        }

        public void RemoveProxy(int proxyId)
        {
            Debug.Assert((0 <= proxyId) && (proxyId < this._nodeCapacity));
            Debug.Assert(this._nodes[proxyId].IsLeaf());
            this.RemoveLeaf(proxyId);
            this.FreeNode(proxyId);
        }

        public void ShiftOrigin(TSVector2 newOrigin)
        {
            for (int i = 0; i < this._nodeCapacity; i++)
            {
                this._nodes[i].AABB.LowerBound -= newOrigin;
                this._nodes[i].AABB.UpperBound -= newOrigin;
            }
        }

        public void Validate()
        {
            this.ValidateStructure(this._root);
            this.ValidateMetrics(this._root);
            int num = 0;
            int index = this._freeList;
            while (index != -1)
            {
                Debug.Assert((0 <= index) && (index < this._nodeCapacity));
                index = this._nodes[index].ParentOrNext;
                num++;
            }
            Debug.Assert(this.Height == this.ComputeHeight());
            Debug.Assert((this._nodeCount + num) == this._nodeCapacity);
        }

        public void ValidateMetrics(int index)
        {
            if (index != -1)
            {
                TreeNode<T> node = this._nodes[index];
                int num = node.Child1;
                int num2 = node.Child2;
                if (node.IsLeaf())
                {
                    Debug.Assert(num == -1);
                    Debug.Assert(num2 == -1);
                    Debug.Assert(node.Height == 0);
                }
                else
                {
                    Debug.Assert((0 <= num) && (num < this._nodeCapacity));
                    Debug.Assert((0 <= num2) && (num2 < this._nodeCapacity));
                    int height = this._nodes[num].Height;
                    int num4 = this._nodes[num2].Height;
                    int num5 = 1 + Math.Max(height, num4);
                    Debug.Assert(node.Height == num5);
                    AABB aabb = new AABB();
                    aabb.Combine(ref this._nodes[num].AABB, ref this._nodes[num2].AABB);
                    Debug.Assert(aabb.LowerBound == node.AABB.LowerBound);
                    Debug.Assert(aabb.UpperBound == node.AABB.UpperBound);
                    this.ValidateMetrics(num);
                    this.ValidateMetrics(num2);
                }
            }
        }

        public void ValidateStructure(int index)
        {
            if (index != -1)
            {
                if (index == this._root)
                {
                    Debug.Assert(this._nodes[index].ParentOrNext == -1);
                }
                TreeNode<T> node = this._nodes[index];
                int num = node.Child1;
                int num2 = node.Child2;
                if (node.IsLeaf())
                {
                    Debug.Assert(num == -1);
                    Debug.Assert(num2 == -1);
                    Debug.Assert(node.Height == 0);
                }
                else
                {
                    Debug.Assert((0 <= num) && (num < this._nodeCapacity));
                    Debug.Assert((0 <= num2) && (num2 < this._nodeCapacity));
                    Debug.Assert(this._nodes[num].ParentOrNext == index);
                    Debug.Assert(this._nodes[num2].ParentOrNext == index);
                    this.ValidateStructure(num);
                    this.ValidateStructure(num2);
                }
            }
        }

        public FP AreaRatio
        {
            get
            {
                if (this._root == -1)
                {
                    return 0f;
                }
                TreeNode<T> node = this._nodes[this._root];
                FP perimeter = node.AABB.Perimeter;
                FP fp2 = 0f;
                for (int i = 0; i < this._nodeCapacity; i++)
                {
                    TreeNode<T> node2 = this._nodes[i];
                    if (node2.Height >= 0)
                    {
                        fp2 += node2.AABB.Perimeter;
                    }
                }
                return (fp2 / perimeter);
            }
        }

        public int Height
        {
            get
            {
                if (this._root == -1)
                {
                    return 0;
                }
                return this._nodes[this._root].Height;
            }
        }

        public int MaxBalance
        {
            get
            {
                int num = 0;
                for (int i = 0; i < this._nodeCapacity; i++)
                {
                    TreeNode<T> node = this._nodes[i];
                    if (node.Height > 1)
                    {
                        Debug.Assert(!node.IsLeaf());
                        int index = node.Child1;
                        int num4 = node.Child2;
                        int num5 = Math.Abs((int) (this._nodes[num4].Height - this._nodes[index].Height));
                        num = Math.Max(num, num5);
                    }
                }
                return num;
            }
        }
    }
}

