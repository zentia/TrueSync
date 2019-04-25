namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;

    public class DynamicTree<T>
    {
        private int _freeList;
        private int _insertionCount;
        private int _nodeCapacity;
        private int _nodeCount;
        private DynamicTreeNode<T>[] _nodes;
        private int _root;
        internal const int NullNode = -1;
        private static FP SettingsAABBMultiplier;
        private FP settingsRndExtension;
        private ResourcePool<Stack<int>> stackPool;

        static DynamicTree()
        {
            DynamicTree<T>.SettingsAABBMultiplier = 2 * FP.One;
        }

        public DynamicTree() : this(FP.EN1)
        {
        }

        public DynamicTree(FP rndExtension)
        {
            this.settingsRndExtension = FP.EN1;
            this.stackPool = new ResourcePool<Stack<int>>();
            this.settingsRndExtension = rndExtension;
            this._root = -1;
            this._nodeCapacity = 0x10;
            this._nodes = new DynamicTreeNode<T>[this._nodeCapacity];
            for (int i = 0; i < (this._nodeCapacity - 1); i++)
            {
                this._nodes[i].ParentOrNext = i + 1;
            }
            this._nodes[this._nodeCapacity - 1].ParentOrNext = -1;
        }

        public int AddProxy(ref TSBBox aabb, T userData)
        {
            int index = this.AllocateNode();
            this._nodes[index].MinorRandomExtension = FP.Half * this.settingsRndExtension;
            TSVector vector = new TSVector(this._nodes[index].MinorRandomExtension);
            this._nodes[index].AABB.min = aabb.min - vector;
            this._nodes[index].AABB.max = aabb.max + vector;
            this._nodes[index].UserData = userData;
            this._nodes[index].LeafCount = 1;
            this.InsertLeaf(index);
            return index;
        }

        private int AllocateNode()
        {
            if (this._freeList == -1)
            {
                Debug.Assert(this._nodeCount == this._nodeCapacity);
                DynamicTreeNode<T>[] sourceArray = this._nodes;
                this._nodeCapacity *= 2;
                this._nodes = new DynamicTreeNode<T>[this._nodeCapacity];
                Array.Copy(sourceArray, this._nodes, this._nodeCount);
                for (int i = this._nodeCount; i < (this._nodeCapacity - 1); i++)
                {
                    this._nodes[i].ParentOrNext = i + 1;
                }
                this._nodes[this._nodeCapacity - 1].ParentOrNext = -1;
                this._freeList = this._nodeCount;
            }
            int index = this._freeList;
            this._freeList = this._nodes[index].ParentOrNext;
            this._nodes[index].ParentOrNext = -1;
            this._nodes[index].Child1 = -1;
            this._nodes[index].Child2 = -1;
            this._nodes[index].LeafCount = 0;
            this._nodeCount++;
            return index;
        }

        public int ComputeHeight()
        {
            return this.ComputeHeight(this._root);
        }

        private int ComputeHeight(int nodeId)
        {
            if (nodeId == -1)
            {
                return 0;
            }
            Debug.Assert((0 <= nodeId) && (nodeId < this._nodeCapacity));
            DynamicTreeNode<T> node = this._nodes[nodeId];
            int num = this.ComputeHeight(node.Child1);
            int num2 = this.ComputeHeight(node.Child2);
            return (1 + Math.Max(num, num2));
        }

        private int CountLeaves(int nodeId)
        {
            if (nodeId == -1)
            {
                return 0;
            }
            Debug.Assert((0 <= nodeId) && (nodeId < this._nodeCapacity));
            DynamicTreeNode<T> node = this._nodes[nodeId];
            if (node.IsLeaf())
            {
                Debug.Assert(node.LeafCount == 1);
                return 1;
            }
            int num = this.CountLeaves(node.Child1);
            int num2 = this.CountLeaves(node.Child2);
            int num3 = num + num2;
            Debug.Assert(num3 == node.LeafCount);
            return num3;
        }

        private void FreeNode(int nodeId)
        {
            Debug.Assert((0 <= nodeId) && (nodeId < this._nodeCapacity));
            Debug.Assert(0 < this._nodeCount);
            this._nodes[nodeId].ParentOrNext = this._freeList;
            this._freeList = nodeId;
            this._nodeCount--;
        }

        public void GetFatAABB(int proxyId, out TSBBox fatAABB)
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
            this._insertionCount++;
            if (this._root == -1)
            {
                this._root = leaf;
                this._nodes[this._root].ParentOrNext = -1;
            }
            else
            {
                TSBBox aABB = this._nodes[leaf].AABB;
                int index = this._root;
                while (!this._nodes[index].IsLeaf())
                {
                    FP fp5;
                    FP fp6;
                    int num4 = this._nodes[index].Child1;
                    int num5 = this._nodes[index].Child2;
                    TSBBox.CreateMerged(ref this._nodes[index].AABB, ref aABB, out this._nodes[index].AABB);
                    this._nodes[index].LeafCount++;
                    FP perimeter = this._nodes[index].AABB.Perimeter;
                    TSBBox box2 = new TSBBox();
                    TSBBox.CreateMerged(ref this._nodes[index].AABB, ref aABB, out this._nodes[index].AABB);
                    FP fp2 = box2.Perimeter;
                    FP fp3 = (2 * FP.One) * fp2;
                    FP fp4 = (2 * FP.One) * (fp2 - perimeter);
                    if (this._nodes[num4].IsLeaf())
                    {
                        TSBBox result = new TSBBox();
                        TSBBox.CreateMerged(ref aABB, ref this._nodes[num4].AABB, out result);
                        fp5 = result.Perimeter + fp4;
                    }
                    else
                    {
                        TSBBox box4 = new TSBBox();
                        TSBBox.CreateMerged(ref aABB, ref this._nodes[num4].AABB, out box4);
                        FP fp7 = this._nodes[num4].AABB.Perimeter;
                        fp5 = (box4.Perimeter - fp7) + fp4;
                    }
                    if (this._nodes[num5].IsLeaf())
                    {
                        TSBBox box5 = new TSBBox();
                        TSBBox.CreateMerged(ref aABB, ref this._nodes[num5].AABB, out box5);
                        fp6 = box5.Perimeter + fp4;
                    }
                    else
                    {
                        TSBBox box6 = new TSBBox();
                        TSBBox.CreateMerged(ref aABB, ref this._nodes[num5].AABB, out box6);
                        FP fp9 = this._nodes[num5].AABB.Perimeter;
                        fp6 = (box6.Perimeter - fp9) + fp4;
                    }
                    if ((fp3 < fp5) && (fp3 < fp6))
                    {
                        break;
                    }
                    TSBBox.CreateMerged(ref aABB, ref this._nodes[index].AABB, out this._nodes[index].AABB);
                    if (fp5 < fp6)
                    {
                        index = num4;
                    }
                    else
                    {
                        index = num5;
                    }
                }
                int parentOrNext = this._nodes[index].ParentOrNext;
                int num3 = this.AllocateNode();
                this._nodes[num3].ParentOrNext = parentOrNext;
                this._nodes[num3].UserData = default(T);
                TSBBox.CreateMerged(ref aABB, ref this._nodes[index].AABB, out this._nodes[num3].AABB);
                this._nodes[num3].LeafCount = this._nodes[index].LeafCount + 1;
                if (parentOrNext != -1)
                {
                    if (this._nodes[parentOrNext].Child1 == index)
                    {
                        this._nodes[parentOrNext].Child1 = num3;
                    }
                    else
                    {
                        this._nodes[parentOrNext].Child2 = num3;
                    }
                    this._nodes[num3].Child1 = index;
                    this._nodes[num3].Child2 = leaf;
                    this._nodes[index].ParentOrNext = num3;
                    this._nodes[leaf].ParentOrNext = num3;
                }
                else
                {
                    this._nodes[num3].Child1 = index;
                    this._nodes[num3].Child2 = leaf;
                    this._nodes[index].ParentOrNext = num3;
                    this._nodes[leaf].ParentOrNext = num3;
                    this._root = num3;
                }
            }
        }

        public bool MoveProxy(int proxyId, ref TSBBox aabb, TSVector displacement)
        {
            Debug.Assert((0 <= proxyId) && (proxyId < this._nodeCapacity));
            Debug.Assert(this._nodes[proxyId].IsLeaf());
            if (this._nodes[proxyId].AABB.Contains(ref aabb) > TSBBox.ContainmentType.Disjoint)
            {
                return false;
            }
            this.RemoveLeaf(proxyId);
            TSBBox box = aabb;
            TSVector vector = new TSVector(this._nodes[proxyId].MinorRandomExtension);
            box.min -= vector;
            box.max += vector;
            TSVector vector2 = (TSVector) (DynamicTree<T>.SettingsAABBMultiplier * displacement);
            if (vector2.x < FP.Zero)
            {
                box.min.x += vector2.x;
            }
            else
            {
                box.max.x += vector2.x;
            }
            if (vector2.y < FP.Zero)
            {
                box.min.y += vector2.y;
            }
            else
            {
                box.max.y += vector2.y;
            }
            if (vector2.z < FP.Zero)
            {
                box.min.z += vector2.z;
            }
            else
            {
                box.max.z += vector2.z;
            }
            this._nodes[proxyId].AABB = box;
            this.InsertLeaf(proxyId);
            return true;
        }

        public void Query(List<int> my, ref TSBBox aabb)
        {
            Stack<int> stack = this.stackPool.GetNew();
            stack.Push(this._root);
            while (stack.Count > 0)
            {
                int index = stack.Pop();
                if (index != -1)
                {
                    DynamicTreeNode<T> node = this._nodes[index];
                    if (aabb.Contains(ref node.AABB) > TSBBox.ContainmentType.Disjoint)
                    {
                        if (node.IsLeaf())
                        {
                            my.Add(index);
                        }
                        else
                        {
                            stack.Push(node.Child1);
                            stack.Push(node.Child2);
                        }
                    }
                }
            }
            this.stackPool.GiveBack(stack);
        }

        public void Query(List<int> other, List<int> my, DynamicTree<T> tree)
        {
            Stack<int> stack = this.stackPool.GetNew();
            Stack<int> stack2 = this.stackPool.GetNew();
            stack.Push(this._root);
            stack2.Push(tree._root);
            while (stack.Count > 0)
            {
                int index = stack.Pop();
                int num2 = stack2.Pop();
                if (((index != -1) && (num2 != -1)) && (tree._nodes[num2].AABB.Contains(ref this._nodes[index].AABB) > TSBBox.ContainmentType.Disjoint))
                {
                    if (this._nodes[index].IsLeaf() && tree._nodes[num2].IsLeaf())
                    {
                        my.Add(index);
                        other.Add(num2);
                    }
                    else if (tree._nodes[num2].IsLeaf())
                    {
                        stack.Push(this._nodes[index].Child1);
                        stack2.Push(num2);
                        stack.Push(this._nodes[index].Child2);
                        stack2.Push(num2);
                    }
                    else if (this._nodes[index].IsLeaf())
                    {
                        stack.Push(index);
                        stack2.Push(tree._nodes[num2].Child1);
                        stack.Push(index);
                        stack2.Push(tree._nodes[num2].Child2);
                    }
                    else
                    {
                        stack.Push(this._nodes[index].Child1);
                        stack2.Push(tree._nodes[num2].Child1);
                        stack.Push(this._nodes[index].Child1);
                        stack2.Push(tree._nodes[num2].Child2);
                        stack.Push(this._nodes[index].Child2);
                        stack2.Push(tree._nodes[num2].Child1);
                        stack.Push(this._nodes[index].Child2);
                        stack2.Push(tree._nodes[num2].Child2);
                    }
                }
            }
            this.stackPool.GiveBack(stack);
            this.stackPool.GiveBack(stack2);
        }

        public void Query(TSVector origin, TSVector direction, List<int> collisions)
        {
            Stack<int> stack = this.stackPool.GetNew();
            stack.Push(this._root);
            while (stack.Count > 0)
            {
                int index = stack.Pop();
                DynamicTreeNode<T> node = this._nodes[index];
                if (node.AABB.RayIntersect(ref origin, ref direction))
                {
                    if (node.IsLeaf())
                    {
                        collisions.Add(index);
                    }
                    else
                    {
                        if (this._nodes[node.Child1].AABB.RayIntersect(ref origin, ref direction))
                        {
                            stack.Push(node.Child1);
                        }
                        if (this._nodes[node.Child2].AABB.RayIntersect(ref origin, ref direction))
                        {
                            stack.Push(node.Child2);
                        }
                    }
                }
            }
            this.stackPool.GiveBack(stack);
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
                    parentOrNext = index;
                    while (parentOrNext != -1)
                    {
                        TSBBox.CreateMerged(ref this._nodes[this._nodes[parentOrNext].Child1].AABB, ref this._nodes[this._nodes[parentOrNext].Child2].AABB, out this._nodes[parentOrNext].AABB);
                        Debug.Assert(this._nodes[parentOrNext].LeafCount > 0);
                        this._nodes[parentOrNext].LeafCount--;
                        parentOrNext = this._nodes[parentOrNext].ParentOrNext;
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

        private void Validate()
        {
            this.CountLeaves(this._root);
        }

        public DynamicTreeNode<T>[] Nodes
        {
            get
            {
                return this._nodes;
            }
        }

        public int Root
        {
            get
            {
                return this._root;
            }
        }
    }
}

