namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using TrueSync.Physics2D;

    internal class DynamicTreeClone2D
    {
        public int _freeList;
        public int _nodeCapacity;
        public int _nodeCount;
        public List<TreeNode<FixtureProxy>> _nodes;
        public Stack<int> _queryStack = new Stack<int>(0x100);
        public Stack<int> _raycastStack = new Stack<int>(0x100);
        public int _root;
        private int index;
        private int length;

        public void Clone(TrueSync.Physics2D.DynamicTree<FixtureProxy> dynamicTree)
        {
            this._raycastStack.Clear();
            int[] numArray = dynamicTree._raycastStack.ToArray();
            this.index = 0;
            this.length = numArray.Length;
            while (this.index < this.length)
            {
                int item = numArray[this.index];
                this._raycastStack.Push(item);
                this.index++;
            }
            this._queryStack.Clear();
            int[] numArray2 = dynamicTree._queryStack.ToArray();
            this.index = 0;
            this.length = numArray2.Length;
            while (this.index < this.length)
            {
                int num2 = numArray2[this.index];
                this._queryStack.Push(num2);
                this.index++;
            }
            this._freeList = dynamicTree._freeList;
            this._nodeCapacity = dynamicTree._nodeCapacity;
            this._nodeCount = dynamicTree._nodeCount;
            this._root = dynamicTree._root;
            this._nodes = new List<TreeNode<FixtureProxy>>();
            this.index = 0;
            this.length = dynamicTree._nodes.Length;
            while (this.index < this.length)
            {
                TreeNode<FixtureProxy> node = dynamicTree._nodes[this.index];
                TreeNode<FixtureProxy> node2 = WorldClone2D.poolTreeFixtureProxy.GetNew();
                node2.AABB = node.AABB;
                node2.Child1 = node.Child1;
                node2.Child2 = node.Child2;
                node2.Height = node.Height;
                node2.ParentOrNext = node.ParentOrNext;
                node2.UserData = node.UserData;
                this._nodes.Add(node2);
                this.index++;
            }
        }

        public void Restore(TrueSync.Physics2D.DynamicTree<FixtureProxy> dynamicTree)
        {
            dynamicTree._raycastStack.Clear();
            int[] numArray = this._raycastStack.ToArray();
            this.index = 0;
            this.length = numArray.Length;
            while (this.index < this.length)
            {
                int item = numArray[this.index];
                dynamicTree._raycastStack.Push(item);
                this.index++;
            }
            dynamicTree._queryStack.Clear();
            int[] numArray2 = this._queryStack.ToArray();
            this.index = 0;
            this.length = numArray2.Length;
            while (this.index < this.length)
            {
                int num2 = numArray2[this.index];
                dynamicTree._queryStack.Push(num2);
                this.index++;
            }
            dynamicTree._freeList = this._freeList;
            dynamicTree._nodeCapacity = this._nodeCapacity;
            dynamicTree._nodeCount = this._nodeCount;
            dynamicTree._root = this._root;
            dynamicTree._nodes = new TreeNode<FixtureProxy>[this._nodes.Count];
            this.index = 0;
            this.length = this._nodes.Count;
            while (this.index < this.length)
            {
                TreeNode<FixtureProxy> node = this._nodes[this.index];
                TreeNode<FixtureProxy> node2 = WorldClone2D.poolTreeFixtureProxy.GetNew();
                node2.AABB = node.AABB;
                node2.Child1 = node.Child1;
                node2.Child2 = node.Child2;
                node2.Height = node.Height;
                node2.ParentOrNext = node.ParentOrNext;
                node2.UserData = node.UserData;
                dynamicTree._nodes[this.index] = node2;
                this.index++;
            }
        }
    }
}

