using System;
using System.Collections.Generic;
using TrueSync.Physics2D;

namespace TrueSync
{
	internal class DynamicTreeClone2D
	{
		public Stack<int> _raycastStack = new Stack<int>(256);

		public Stack<int> _queryStack = new Stack<int>(256);

		public int _freeList;

		public int _nodeCapacity;

		public int _nodeCount;

		public List<TreeNode<FixtureProxy>> _nodes;

		public int _root;

		private int index;

		private int length;

		public void Clone(TrueSync.Physics2D.DynamicTree<FixtureProxy> dynamicTree)
		{
			this._raycastStack.Clear();
			int[] array = dynamicTree._raycastStack.ToArray();
			this.index = 0;
			this.length = array.Length;
			while (this.index < this.length)
			{
				int item = array[this.index];
				this._raycastStack.Push(item);
				this.index++;
			}
			this._queryStack.Clear();
			int[] array2 = dynamicTree._queryStack.ToArray();
			this.index = 0;
			this.length = array2.Length;
			while (this.index < this.length)
			{
				int item2 = array2[this.index];
				this._queryStack.Push(item2);
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
				TreeNode<FixtureProxy> treeNode = dynamicTree._nodes[this.index];
				TreeNode<FixtureProxy> @new = WorldClone2D.poolTreeFixtureProxy.GetNew();
				@new.AABB = treeNode.AABB;
				@new.Child1 = treeNode.Child1;
				@new.Child2 = treeNode.Child2;
				@new.Height = treeNode.Height;
				@new.ParentOrNext = treeNode.ParentOrNext;
				@new.UserData = treeNode.UserData;
				this._nodes.Add(@new);
				this.index++;
			}
		}

		public void Restore(TrueSync.Physics2D.DynamicTree<FixtureProxy> dynamicTree)
		{
			dynamicTree._raycastStack.Clear();
			int[] array = this._raycastStack.ToArray();
			this.index = 0;
			this.length = array.Length;
			while (this.index < this.length)
			{
				int item = array[this.index];
				dynamicTree._raycastStack.Push(item);
				this.index++;
			}
			dynamicTree._queryStack.Clear();
			int[] array2 = this._queryStack.ToArray();
			this.index = 0;
			this.length = array2.Length;
			while (this.index < this.length)
			{
				int item2 = array2[this.index];
				dynamicTree._queryStack.Push(item2);
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
				TreeNode<FixtureProxy> treeNode = this._nodes[this.index];
				TreeNode<FixtureProxy> @new = WorldClone2D.poolTreeFixtureProxy.GetNew();
				@new.AABB = treeNode.AABB;
				@new.Child1 = treeNode.Child1;
				@new.Child2 = treeNode.Child2;
				@new.Height = treeNode.Height;
				@new.ParentOrNext = treeNode.ParentOrNext;
				@new.UserData = treeNode.UserData;
				dynamicTree._nodes[this.index] = @new;
				this.index++;
			}
		}
	}
}
