using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync
{
	public class DynamicTree<T>
	{
		internal const int NullNode = -1;

		private int _freeList;

		private int _insertionCount;

		private int _nodeCapacity;

		private int _nodeCount;

		private DynamicTreeNode<T>[] _nodes;

		private static FP SettingsAABBMultiplier = 2 * FP.One;

		private FP settingsRndExtension = FP.EN1;

		private int _root;

		private ResourcePool<Stack<int>> stackPool = new ResourcePool<Stack<int>>();

		public int Root
		{
			get
			{
				return this._root;
			}
		}

		public DynamicTreeNode<T>[] Nodes
		{
			get
			{
				return this._nodes;
			}
		}

		public DynamicTree() : this(FP.EN1)
		{
		}

		public DynamicTree(FP rndExtension)
		{
			this.settingsRndExtension = rndExtension;
			this._root = -1;
			this._nodeCapacity = 16;
			this._nodes = new DynamicTreeNode<T>[this._nodeCapacity];
			for (int i = 0; i < this._nodeCapacity - 1; i++)
			{
				this._nodes[i].ParentOrNext = i + 1;
			}
			this._nodes[this._nodeCapacity - 1].ParentOrNext = -1;
		}

		public int AddProxy(ref TSBBox aabb, T userData)
		{
			int num = this.AllocateNode();
			this._nodes[num].MinorRandomExtension = FP.Half * this.settingsRndExtension;
			TSVector value = new TSVector(this._nodes[num].MinorRandomExtension);
			this._nodes[num].AABB.min = aabb.min - value;
			this._nodes[num].AABB.max = aabb.max + value;
			this._nodes[num].UserData = userData;
			this._nodes[num].LeafCount = 1;
			this.InsertLeaf(num);
			return num;
		}

		public void RemoveProxy(int proxyId)
		{
			Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
			Debug.Assert(this._nodes[proxyId].IsLeaf());
			this.RemoveLeaf(proxyId);
			this.FreeNode(proxyId);
		}

		public bool MoveProxy(int proxyId, ref TSBBox aabb, TSVector displacement)
		{
			Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
			Debug.Assert(this._nodes[proxyId].IsLeaf());
			bool flag = this._nodes[proxyId].AABB.Contains(ref aabb) > TSBBox.ContainmentType.Disjoint;
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				this.RemoveLeaf(proxyId);
				TSBBox tSBBox = aabb;
				TSVector value = new TSVector(this._nodes[proxyId].MinorRandomExtension);
				tSBBox.min -= value;
				tSBBox.max += value;
				TSVector tSVector = DynamicTree<T>.SettingsAABBMultiplier * displacement;
				bool flag2 = tSVector.x < FP.Zero;
				if (flag2)
				{
					tSBBox.min.x = tSBBox.min.x + tSVector.x;
				}
				else
				{
					tSBBox.max.x = tSBBox.max.x + tSVector.x;
				}
				bool flag3 = tSVector.y < FP.Zero;
				if (flag3)
				{
					tSBBox.min.y = tSBBox.min.y + tSVector.y;
				}
				else
				{
					tSBBox.max.y = tSBBox.max.y + tSVector.y;
				}
				bool flag4 = tSVector.z < FP.Zero;
				if (flag4)
				{
					tSBBox.min.z = tSBBox.min.z + tSVector.z;
				}
				else
				{
					tSBBox.max.z = tSBBox.max.z + tSVector.z;
				}
				this._nodes[proxyId].AABB = tSBBox;
				this.InsertLeaf(proxyId);
				result = true;
			}
			return result;
		}

		public T GetUserData(int proxyId)
		{
			Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
			return this._nodes[proxyId].UserData;
		}

		public void GetFatAABB(int proxyId, out TSBBox fatAABB)
		{
			Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
			fatAABB = this._nodes[proxyId].AABB;
		}

		public int ComputeHeight()
		{
			return this.ComputeHeight(this._root);
		}

		public void Query(TSVector origin, TSVector direction, List<int> collisions)
		{
			Stack<int> @new = this.stackPool.GetNew();
			@new.Push(this._root);
			while (@new.Count > 0)
			{
				int num = @new.Pop();
				DynamicTreeNode<T> dynamicTreeNode = this._nodes[num];
				bool flag = dynamicTreeNode.AABB.RayIntersect(ref origin, ref direction);
				if (flag)
				{
					bool flag2 = dynamicTreeNode.IsLeaf();
					if (flag2)
					{
						collisions.Add(num);
					}
					else
					{
						bool flag3 = this._nodes[dynamicTreeNode.Child1].AABB.RayIntersect(ref origin, ref direction);
						if (flag3)
						{
							@new.Push(dynamicTreeNode.Child1);
						}
						bool flag4 = this._nodes[dynamicTreeNode.Child2].AABB.RayIntersect(ref origin, ref direction);
						if (flag4)
						{
							@new.Push(dynamicTreeNode.Child2);
						}
					}
				}
			}
			this.stackPool.GiveBack(@new);
		}

		public void Query(List<int> other, List<int> my, DynamicTree<T> tree)
		{
			Stack<int> @new = this.stackPool.GetNew();
			Stack<int> new2 = this.stackPool.GetNew();
			@new.Push(this._root);
			new2.Push(tree._root);
			while (@new.Count > 0)
			{
				int num = @new.Pop();
				int num2 = new2.Pop();
				bool flag = num == -1;
				if (!flag)
				{
					bool flag2 = num2 == -1;
					if (!flag2)
					{
						bool flag3 = tree._nodes[num2].AABB.Contains(ref this._nodes[num].AABB) > TSBBox.ContainmentType.Disjoint;
						if (flag3)
						{
							bool flag4 = this._nodes[num].IsLeaf() && tree._nodes[num2].IsLeaf();
							if (flag4)
							{
								my.Add(num);
								other.Add(num2);
							}
							else
							{
								bool flag5 = tree._nodes[num2].IsLeaf();
								if (flag5)
								{
									@new.Push(this._nodes[num].Child1);
									new2.Push(num2);
									@new.Push(this._nodes[num].Child2);
									new2.Push(num2);
								}
								else
								{
									bool flag6 = this._nodes[num].IsLeaf();
									if (flag6)
									{
										@new.Push(num);
										new2.Push(tree._nodes[num2].Child1);
										@new.Push(num);
										new2.Push(tree._nodes[num2].Child2);
									}
									else
									{
										@new.Push(this._nodes[num].Child1);
										new2.Push(tree._nodes[num2].Child1);
										@new.Push(this._nodes[num].Child1);
										new2.Push(tree._nodes[num2].Child2);
										@new.Push(this._nodes[num].Child2);
										new2.Push(tree._nodes[num2].Child1);
										@new.Push(this._nodes[num].Child2);
										new2.Push(tree._nodes[num2].Child2);
									}
								}
							}
						}
					}
				}
			}
			this.stackPool.GiveBack(@new);
			this.stackPool.GiveBack(new2);
		}

		public void Query(List<int> my, ref TSBBox aabb)
		{
			Stack<int> @new = this.stackPool.GetNew();
			@new.Push(this._root);
			while (@new.Count > 0)
			{
				int num = @new.Pop();
				bool flag = num == -1;
				if (!flag)
				{
					DynamicTreeNode<T> dynamicTreeNode = this._nodes[num];
					bool flag2 = aabb.Contains(ref dynamicTreeNode.AABB) > TSBBox.ContainmentType.Disjoint;
					if (flag2)
					{
						bool flag3 = dynamicTreeNode.IsLeaf();
						if (flag3)
						{
							my.Add(num);
						}
						else
						{
							@new.Push(dynamicTreeNode.Child1);
							@new.Push(dynamicTreeNode.Child2);
						}
					}
				}
			}
			this.stackPool.GiveBack(@new);
		}

		private int CountLeaves(int nodeId)
		{
			bool flag = nodeId == -1;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				Debug.Assert(0 <= nodeId && nodeId < this._nodeCapacity);
				DynamicTreeNode<T> dynamicTreeNode = this._nodes[nodeId];
				bool flag2 = dynamicTreeNode.IsLeaf();
				if (flag2)
				{
					Debug.Assert(dynamicTreeNode.LeafCount == 1);
					result = 1;
				}
				else
				{
					int num = this.CountLeaves(dynamicTreeNode.Child1);
					int num2 = this.CountLeaves(dynamicTreeNode.Child2);
					int num3 = num + num2;
					Debug.Assert(num3 == dynamicTreeNode.LeafCount);
					result = num3;
				}
			}
			return result;
		}

		private void Validate()
		{
			this.CountLeaves(this._root);
		}

		private int AllocateNode()
		{
			bool flag = this._freeList == -1;
			if (flag)
			{
				Debug.Assert(this._nodeCount == this._nodeCapacity);
				DynamicTreeNode<T>[] nodes = this._nodes;
				this._nodeCapacity *= 2;
				this._nodes = new DynamicTreeNode<T>[this._nodeCapacity];
				Array.Copy(nodes, this._nodes, this._nodeCount);
				for (int i = this._nodeCount; i < this._nodeCapacity - 1; i++)
				{
					this._nodes[i].ParentOrNext = i + 1;
				}
				this._nodes[this._nodeCapacity - 1].ParentOrNext = -1;
				this._freeList = this._nodeCount;
			}
			int freeList = this._freeList;
			this._freeList = this._nodes[freeList].ParentOrNext;
			this._nodes[freeList].ParentOrNext = -1;
			this._nodes[freeList].Child1 = -1;
			this._nodes[freeList].Child2 = -1;
			this._nodes[freeList].LeafCount = 0;
			this._nodeCount++;
			return freeList;
		}

		private void FreeNode(int nodeId)
		{
			Debug.Assert(0 <= nodeId && nodeId < this._nodeCapacity);
			Debug.Assert(0 < this._nodeCount);
			this._nodes[nodeId].ParentOrNext = this._freeList;
			this._freeList = nodeId;
			this._nodeCount--;
		}

		private void InsertLeaf(int leaf)
		{
			this._insertionCount++;
			bool flag = this._root == -1;
			if (flag)
			{
				this._root = leaf;
				this._nodes[this._root].ParentOrNext = -1;
			}
			else
			{
				TSBBox aABB = this._nodes[leaf].AABB;
				int num = this._root;
				while (!this._nodes[num].IsLeaf())
				{
					int child = this._nodes[num].Child1;
					int child2 = this._nodes[num].Child2;
					TSBBox.CreateMerged(ref this._nodes[num].AABB, ref aABB, out this._nodes[num].AABB);
					DynamicTreeNode<T>[] expr_C2_cp_0_cp_0 = this._nodes;
					int expr_C2_cp_0_cp_1 = num;
					expr_C2_cp_0_cp_0[expr_C2_cp_0_cp_1].LeafCount = expr_C2_cp_0_cp_0[expr_C2_cp_0_cp_1].LeafCount + 1;
					FP perimeter = this._nodes[num].AABB.Perimeter;
					TSBBox tSBBox = default(TSBBox);
					TSBBox.CreateMerged(ref this._nodes[num].AABB, ref aABB, out this._nodes[num].AABB);
					FP perimeter2 = tSBBox.Perimeter;
					FP x = 2 * FP.One * perimeter2;
					FP y = 2 * FP.One * (perimeter2 - perimeter);
					bool flag2 = this._nodes[child].IsLeaf();
					FP fP;
					if (flag2)
					{
						TSBBox tSBBox2 = default(TSBBox);
						TSBBox.CreateMerged(ref aABB, ref this._nodes[child].AABB, out tSBBox2);
						fP = tSBBox2.Perimeter + y;
					}
					else
					{
						TSBBox tSBBox3 = default(TSBBox);
						TSBBox.CreateMerged(ref aABB, ref this._nodes[child].AABB, out tSBBox3);
						FP perimeter3 = this._nodes[child].AABB.Perimeter;
						FP perimeter4 = tSBBox3.Perimeter;
						fP = perimeter4 - perimeter3 + y;
					}
					bool flag3 = this._nodes[child2].IsLeaf();
					FP y2;
					if (flag3)
					{
						TSBBox tSBBox4 = default(TSBBox);
						TSBBox.CreateMerged(ref aABB, ref this._nodes[child2].AABB, out tSBBox4);
						y2 = tSBBox4.Perimeter + y;
					}
					else
					{
						TSBBox tSBBox5 = default(TSBBox);
						TSBBox.CreateMerged(ref aABB, ref this._nodes[child2].AABB, out tSBBox5);
						FP perimeter5 = this._nodes[child2].AABB.Perimeter;
						FP perimeter6 = tSBBox5.Perimeter;
						y2 = perimeter6 - perimeter5 + y;
					}
					bool flag4 = x < fP && x < y2;
					if (flag4)
					{
						break;
					}
					TSBBox.CreateMerged(ref aABB, ref this._nodes[num].AABB, out this._nodes[num].AABB);
					bool flag5 = fP < y2;
					if (flag5)
					{
						num = child;
					}
					else
					{
						num = child2;
					}
				}
				int parentOrNext = this._nodes[num].ParentOrNext;
				int num2 = this.AllocateNode();
				this._nodes[num2].ParentOrNext = parentOrNext;
				this._nodes[num2].UserData = default(T);
				TSBBox.CreateMerged(ref aABB, ref this._nodes[num].AABB, out this._nodes[num2].AABB);
				this._nodes[num2].LeafCount = this._nodes[num].LeafCount + 1;
				bool flag6 = parentOrNext != -1;
				if (flag6)
				{
					bool flag7 = this._nodes[parentOrNext].Child1 == num;
					if (flag7)
					{
						this._nodes[parentOrNext].Child1 = num2;
					}
					else
					{
						this._nodes[parentOrNext].Child2 = num2;
					}
					this._nodes[num2].Child1 = num;
					this._nodes[num2].Child2 = leaf;
					this._nodes[num].ParentOrNext = num2;
					this._nodes[leaf].ParentOrNext = num2;
				}
				else
				{
					this._nodes[num2].Child1 = num;
					this._nodes[num2].Child2 = leaf;
					this._nodes[num].ParentOrNext = num2;
					this._nodes[leaf].ParentOrNext = num2;
					this._root = num2;
				}
			}
		}

		private void RemoveLeaf(int leaf)
		{
			bool flag = leaf == this._root;
			if (flag)
			{
				this._root = -1;
			}
			else
			{
				int num = this._nodes[leaf].ParentOrNext;
				int parentOrNext = this._nodes[num].ParentOrNext;
				bool flag2 = this._nodes[num].Child1 == leaf;
				int num2;
				if (flag2)
				{
					num2 = this._nodes[num].Child2;
				}
				else
				{
					num2 = this._nodes[num].Child1;
				}
				bool flag3 = parentOrNext != -1;
				if (flag3)
				{
					bool flag4 = this._nodes[parentOrNext].Child1 == num;
					if (flag4)
					{
						this._nodes[parentOrNext].Child1 = num2;
					}
					else
					{
						this._nodes[parentOrNext].Child2 = num2;
					}
					this._nodes[num2].ParentOrNext = parentOrNext;
					this.FreeNode(num);
					for (num = parentOrNext; num != -1; num = this._nodes[num].ParentOrNext)
					{
						TSBBox.CreateMerged(ref this._nodes[this._nodes[num].Child1].AABB, ref this._nodes[this._nodes[num].Child2].AABB, out this._nodes[num].AABB);
						Debug.Assert(this._nodes[num].LeafCount > 0);
						DynamicTreeNode<T>[] expr_17E_cp_0_cp_0 = this._nodes;
						int expr_17E_cp_0_cp_1 = num;
						expr_17E_cp_0_cp_0[expr_17E_cp_0_cp_1].LeafCount = expr_17E_cp_0_cp_0[expr_17E_cp_0_cp_1].LeafCount - 1;
					}
				}
				else
				{
					this._root = num2;
					this._nodes[num2].ParentOrNext = -1;
					this.FreeNode(num);
				}
			}
		}

		private int ComputeHeight(int nodeId)
		{
			bool flag = nodeId == -1;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				Debug.Assert(0 <= nodeId && nodeId < this._nodeCapacity);
				DynamicTreeNode<T> dynamicTreeNode = this._nodes[nodeId];
				int val = this.ComputeHeight(dynamicTreeNode.Child1);
				int val2 = this.ComputeHeight(dynamicTreeNode.Child2);
				result = 1 + Math.Max(val, val2);
			}
			return result;
		}
	}
}
