using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public class DynamicTree<T>
	{
		internal Stack<int> _raycastStack = new Stack<int>(256);

		internal Stack<int> _queryStack = new Stack<int>(256);

		internal int _freeList;

		internal int _nodeCapacity;

		internal int _nodeCount;

		internal TreeNode<T>[] _nodes;

		internal int _root;

		internal const int NullNode = -1;

		public int Height
		{
			get
			{
				bool flag = this._root == -1;
				int result;
				if (flag)
				{
					result = 0;
				}
				else
				{
					result = this._nodes[this._root].Height;
				}
				return result;
			}
		}

		public FP AreaRatio
		{
			get
			{
				bool flag = this._root == -1;
				FP result;
				if (flag)
				{
					result = 0f;
				}
				else
				{
					TreeNode<T> treeNode = this._nodes[this._root];
					FP perimeter = treeNode.AABB.Perimeter;
					FP x = 0f;
					for (int i = 0; i < this._nodeCapacity; i++)
					{
						TreeNode<T> treeNode2 = this._nodes[i];
						bool flag2 = treeNode2.Height < 0;
						if (!flag2)
						{
							x += treeNode2.AABB.Perimeter;
						}
					}
					result = x / perimeter;
				}
				return result;
			}
		}

		public int MaxBalance
		{
			get
			{
				int num = 0;
				for (int i = 0; i < this._nodeCapacity; i++)
				{
					TreeNode<T> treeNode = this._nodes[i];
					bool flag = treeNode.Height <= 1;
					if (!flag)
					{
						Debug.Assert(!treeNode.IsLeaf());
						int child = treeNode.Child1;
						int child2 = treeNode.Child2;
						int val = Math.Abs(this._nodes[child2].Height - this._nodes[child].Height);
						num = Math.Max(num, val);
					}
				}
				return num;
			}
		}

		public DynamicTree()
		{
			this._root = -1;
			this._nodeCapacity = 16;
			this._nodeCount = 0;
			this._nodes = new TreeNode<T>[this._nodeCapacity];
			for (int i = 0; i < this._nodeCapacity - 1; i++)
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
			int num = this.AllocateNode();
			TSVector2 value = new TSVector2(Settings.AABBExtension, Settings.AABBExtension);
			this._nodes[num].AABB.LowerBound = aabb.LowerBound - value;
			this._nodes[num].AABB.UpperBound = aabb.UpperBound + value;
			this._nodes[num].UserData = userData;
			this._nodes[num].Height = 0;
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

		public bool MoveProxy(int proxyId, ref AABB aabb, TSVector2 displacement)
		{
			Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
			Debug.Assert(this._nodes[proxyId].IsLeaf());
			bool flag = this._nodes[proxyId].AABB.Contains(ref aabb);
			bool result;
			if (flag)
			{
				result = false;
			}
			else
			{
				this.RemoveLeaf(proxyId);
				AABB aABB = aabb;
				TSVector2 value = new TSVector2(Settings.AABBExtension, Settings.AABBExtension);
				aABB.LowerBound -= value;
				aABB.UpperBound += value;
				TSVector2 tSVector = Settings.AABBMultiplier * displacement;
				bool flag2 = tSVector.x < 0f;
				if (flag2)
				{
					aABB.LowerBound.x = aABB.LowerBound.x + tSVector.x;
				}
				else
				{
					aABB.UpperBound.x = aABB.UpperBound.x + tSVector.x;
				}
				bool flag3 = tSVector.y < 0f;
				if (flag3)
				{
					aABB.LowerBound.y = aABB.LowerBound.y + tSVector.y;
				}
				else
				{
					aABB.UpperBound.y = aABB.UpperBound.y + tSVector.y;
				}
				this._nodes[proxyId].AABB = aABB;
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

		public void GetFatAABB(int proxyId, out AABB fatAABB)
		{
			Debug.Assert(0 <= proxyId && proxyId < this._nodeCapacity);
			fatAABB = this._nodes[proxyId].AABB;
		}

		public void Query(Func<int, bool> callback, ref AABB aabb)
		{
			this._queryStack.Clear();
			this._queryStack.Push(this._root);
			while (this._queryStack.Count > 0)
			{
				int num = this._queryStack.Pop();
				bool flag = num == -1;
				if (!flag)
				{
					TreeNode<T> treeNode = this._nodes[num];
					bool flag2 = AABB.TestOverlap(ref treeNode.AABB, ref aabb);
					if (flag2)
					{
						bool flag3 = treeNode.IsLeaf();
						if (flag3)
						{
							bool flag4 = callback(num);
							bool flag5 = !flag4;
							if (flag5)
							{
								break;
							}
						}
						else
						{
							this._queryStack.Push(treeNode.Child1);
							this._queryStack.Push(treeNode.Child2);
						}
					}
				}
			}
		}

		public void RayCast(Func<RayCastInput, int, FP> callback, ref RayCastInput input)
		{
			TSVector2 point = input.Point1;
			TSVector2 point2 = input.Point2;
			TSVector2 tSVector = point2 - point;
			Debug.Assert(tSVector.LengthSquared() > 0f);
			tSVector.Normalize();
			TSVector2 value = MathUtils.Abs(new TSVector2(-tSVector.y, tSVector.x));
			FP fP = input.MaxFraction;
			AABB aABB = default(AABB);
			TSVector2 tSVector2 = point + fP * (point2 - point);
			TSVector2.Min(ref point, ref tSVector2, out aABB.LowerBound);
			TSVector2.Max(ref point, ref tSVector2, out aABB.UpperBound);
			this._raycastStack.Clear();
			this._raycastStack.Push(this._root);
			while (this._raycastStack.Count > 0)
			{
				int num = this._raycastStack.Pop();
				bool flag = num == -1;
				if (!flag)
				{
					TreeNode<T> treeNode = this._nodes[num];
					bool flag2 = !AABB.TestOverlap(ref treeNode.AABB, ref aABB);
					if (!flag2)
					{
						TSVector2 center = treeNode.AABB.Center;
						TSVector2 extents = treeNode.AABB.Extents;
						FP x = FP.Abs(TSVector2.Dot(new TSVector2(-tSVector.y, tSVector.x), point - center)) - TSVector2.Dot(value, extents);
						bool flag3 = x > 0f;
						if (!flag3)
						{
							bool flag4 = treeNode.IsLeaf();
							if (flag4)
							{
								RayCastInput arg;
								arg.Point1 = input.Point1;
								arg.Point2 = input.Point2;
								arg.MaxFraction = fP;
								FP fP2 = callback(arg, num);
								bool flag5 = fP2 == 0f;
								if (flag5)
								{
									break;
								}
								bool flag6 = fP2 > 0f;
								if (flag6)
								{
									fP = fP2;
									TSVector2 value2 = point + fP * (point2 - point);
									aABB.LowerBound = TSVector2.Min(point, value2);
									aABB.UpperBound = TSVector2.Max(point, value2);
								}
							}
							else
							{
								this._raycastStack.Push(treeNode.Child1);
								this._raycastStack.Push(treeNode.Child2);
							}
						}
					}
				}
			}
		}

		private int AllocateNode()
		{
			bool flag = this._freeList == -1;
			if (flag)
			{
				Debug.Assert(this._nodeCount == this._nodeCapacity);
				TreeNode<T>[] nodes = this._nodes;
				this._nodeCapacity *= 2;
				this._nodes = new TreeNode<T>[this._nodeCapacity];
				Array.Copy(nodes, this._nodes, this._nodeCount);
				for (int i = this._nodeCount; i < this._nodeCapacity - 1; i++)
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
			int freeList = this._freeList;
			this._freeList = this._nodes[freeList].ParentOrNext;
			this._nodes[freeList].ParentOrNext = -1;
			this._nodes[freeList].Child1 = -1;
			this._nodes[freeList].Child2 = -1;
			this._nodes[freeList].Height = 0;
			this._nodes[freeList].UserData = default(T);
			this._nodeCount++;
			return freeList;
		}

		private void FreeNode(int nodeId)
		{
			Debug.Assert(0 <= nodeId && nodeId < this._nodeCapacity);
			Debug.Assert(0 < this._nodeCount);
			this._nodes[nodeId].ParentOrNext = this._freeList;
			this._nodes[nodeId].Height = -1;
			this._freeList = nodeId;
			this._nodeCount--;
		}

		private void InsertLeaf(int leaf)
		{
			bool flag = this._root == -1;
			if (flag)
			{
				this._root = leaf;
				this._nodes[this._root].ParentOrNext = -1;
			}
			else
			{
				AABB aABB = this._nodes[leaf].AABB;
				int num = this._root;
				while (!this._nodes[num].IsLeaf())
				{
					int child = this._nodes[num].Child1;
					int child2 = this._nodes[num].Child2;
					FP perimeter = this._nodes[num].AABB.Perimeter;
					AABB aABB2 = default(AABB);
					aABB2.Combine(ref this._nodes[num].AABB, ref aABB);
					FP perimeter2 = aABB2.Perimeter;
					FP x = 2f * perimeter2;
					FP y = 2f * (perimeter2 - perimeter);
					bool flag2 = this._nodes[child].IsLeaf();
					FP fP;
					if (flag2)
					{
						AABB aABB3 = default(AABB);
						aABB3.Combine(ref aABB, ref this._nodes[child].AABB);
						fP = aABB3.Perimeter + y;
					}
					else
					{
						AABB aABB4 = default(AABB);
						aABB4.Combine(ref aABB, ref this._nodes[child].AABB);
						FP perimeter3 = this._nodes[child].AABB.Perimeter;
						FP perimeter4 = aABB4.Perimeter;
						fP = perimeter4 - perimeter3 + y;
					}
					bool flag3 = this._nodes[child2].IsLeaf();
					FP y2;
					if (flag3)
					{
						AABB aABB5 = default(AABB);
						aABB5.Combine(ref aABB, ref this._nodes[child2].AABB);
						y2 = aABB5.Perimeter + y;
					}
					else
					{
						AABB aABB6 = default(AABB);
						aABB6.Combine(ref aABB, ref this._nodes[child2].AABB);
						FP perimeter5 = this._nodes[child2].AABB.Perimeter;
						FP perimeter6 = aABB6.Perimeter;
						y2 = perimeter6 - perimeter5 + y;
					}
					bool flag4 = x < fP && fP < y2;
					if (flag4)
					{
						break;
					}
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
				int num2 = num;
				int parentOrNext = this._nodes[num2].ParentOrNext;
				int num3 = this.AllocateNode();
				this._nodes[num3].ParentOrNext = parentOrNext;
				this._nodes[num3].UserData = default(T);
				this._nodes[num3].AABB.Combine(ref aABB, ref this._nodes[num2].AABB);
				this._nodes[num3].Height = this._nodes[num2].Height + 1;
				bool flag6 = parentOrNext != -1;
				if (flag6)
				{
					bool flag7 = this._nodes[parentOrNext].Child1 == num2;
					if (flag7)
					{
						this._nodes[parentOrNext].Child1 = num3;
					}
					else
					{
						this._nodes[parentOrNext].Child2 = num3;
					}
					this._nodes[num3].Child1 = num2;
					this._nodes[num3].Child2 = leaf;
					this._nodes[num2].ParentOrNext = num3;
					this._nodes[leaf].ParentOrNext = num3;
				}
				else
				{
					this._nodes[num3].Child1 = num2;
					this._nodes[num3].Child2 = leaf;
					this._nodes[num2].ParentOrNext = num3;
					this._nodes[leaf].ParentOrNext = num3;
					this._root = num3;
				}
				for (num = this._nodes[leaf].ParentOrNext; num != -1; num = this._nodes[num].ParentOrNext)
				{
					num = this.Balance(num);
					int child3 = this._nodes[num].Child1;
					int child4 = this._nodes[num].Child2;
					Debug.Assert(child3 != -1);
					Debug.Assert(child4 != -1);
					this._nodes[num].Height = 1 + Math.Max(this._nodes[child3].Height, this._nodes[child4].Height);
					this._nodes[num].AABB.Combine(ref this._nodes[child3].AABB, ref this._nodes[child4].AABB);
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
				int parentOrNext = this._nodes[leaf].ParentOrNext;
				int parentOrNext2 = this._nodes[parentOrNext].ParentOrNext;
				bool flag2 = this._nodes[parentOrNext].Child1 == leaf;
				int num;
				if (flag2)
				{
					num = this._nodes[parentOrNext].Child2;
				}
				else
				{
					num = this._nodes[parentOrNext].Child1;
				}
				bool flag3 = parentOrNext2 != -1;
				if (flag3)
				{
					bool flag4 = this._nodes[parentOrNext2].Child1 == parentOrNext;
					if (flag4)
					{
						this._nodes[parentOrNext2].Child1 = num;
					}
					else
					{
						this._nodes[parentOrNext2].Child2 = num;
					}
					this._nodes[num].ParentOrNext = parentOrNext2;
					this.FreeNode(parentOrNext);
					for (int num2 = parentOrNext2; num2 != -1; num2 = this._nodes[num2].ParentOrNext)
					{
						num2 = this.Balance(num2);
						int child = this._nodes[num2].Child1;
						int child2 = this._nodes[num2].Child2;
						this._nodes[num2].AABB.Combine(ref this._nodes[child].AABB, ref this._nodes[child2].AABB);
						this._nodes[num2].Height = 1 + Math.Max(this._nodes[child].Height, this._nodes[child2].Height);
					}
				}
				else
				{
					this._root = num;
					this._nodes[num].ParentOrNext = -1;
					this.FreeNode(parentOrNext);
				}
			}
		}

		private int Balance(int iA)
		{
			Debug.Assert(iA != -1);
			TreeNode<T> treeNode = this._nodes[iA];
			bool flag = treeNode.IsLeaf() || treeNode.Height < 2;
			int result;
			if (flag)
			{
				result = iA;
			}
			else
			{
				int child = treeNode.Child1;
				int child2 = treeNode.Child2;
				Debug.Assert(0 <= child && child < this._nodeCapacity);
				Debug.Assert(0 <= child2 && child2 < this._nodeCapacity);
				TreeNode<T> treeNode2 = this._nodes[child];
				TreeNode<T> treeNode3 = this._nodes[child2];
				int num = treeNode3.Height - treeNode2.Height;
				bool flag2 = num > 1;
				if (flag2)
				{
					int child3 = treeNode3.Child1;
					int child4 = treeNode3.Child2;
					TreeNode<T> treeNode4 = this._nodes[child3];
					TreeNode<T> treeNode5 = this._nodes[child4];
					Debug.Assert(0 <= child3 && child3 < this._nodeCapacity);
					Debug.Assert(0 <= child4 && child4 < this._nodeCapacity);
					treeNode3.Child1 = iA;
					treeNode3.ParentOrNext = treeNode.ParentOrNext;
					treeNode.ParentOrNext = child2;
					bool flag3 = treeNode3.ParentOrNext != -1;
					if (flag3)
					{
						bool flag4 = this._nodes[treeNode3.ParentOrNext].Child1 == iA;
						if (flag4)
						{
							this._nodes[treeNode3.ParentOrNext].Child1 = child2;
						}
						else
						{
							Debug.Assert(this._nodes[treeNode3.ParentOrNext].Child2 == iA);
							this._nodes[treeNode3.ParentOrNext].Child2 = child2;
						}
					}
					else
					{
						this._root = child2;
					}
					bool flag5 = treeNode4.Height > treeNode5.Height;
					if (flag5)
					{
						treeNode3.Child2 = child3;
						treeNode.Child2 = child4;
						treeNode5.ParentOrNext = iA;
						treeNode.AABB.Combine(ref treeNode2.AABB, ref treeNode5.AABB);
						treeNode3.AABB.Combine(ref treeNode.AABB, ref treeNode4.AABB);
						treeNode.Height = 1 + Math.Max(treeNode2.Height, treeNode5.Height);
						treeNode3.Height = 1 + Math.Max(treeNode.Height, treeNode4.Height);
					}
					else
					{
						treeNode3.Child2 = child4;
						treeNode.Child2 = child3;
						treeNode4.ParentOrNext = iA;
						treeNode.AABB.Combine(ref treeNode2.AABB, ref treeNode4.AABB);
						treeNode3.AABB.Combine(ref treeNode.AABB, ref treeNode5.AABB);
						treeNode.Height = 1 + Math.Max(treeNode2.Height, treeNode4.Height);
						treeNode3.Height = 1 + Math.Max(treeNode.Height, treeNode5.Height);
					}
					result = child2;
				}
				else
				{
					bool flag6 = num < -1;
					if (flag6)
					{
						int child5 = treeNode2.Child1;
						int child6 = treeNode2.Child2;
						TreeNode<T> treeNode6 = this._nodes[child5];
						TreeNode<T> treeNode7 = this._nodes[child6];
						Debug.Assert(0 <= child5 && child5 < this._nodeCapacity);
						Debug.Assert(0 <= child6 && child6 < this._nodeCapacity);
						treeNode2.Child1 = iA;
						treeNode2.ParentOrNext = treeNode.ParentOrNext;
						treeNode.ParentOrNext = child;
						bool flag7 = treeNode2.ParentOrNext != -1;
						if (flag7)
						{
							bool flag8 = this._nodes[treeNode2.ParentOrNext].Child1 == iA;
							if (flag8)
							{
								this._nodes[treeNode2.ParentOrNext].Child1 = child;
							}
							else
							{
								Debug.Assert(this._nodes[treeNode2.ParentOrNext].Child2 == iA);
								this._nodes[treeNode2.ParentOrNext].Child2 = child;
							}
						}
						else
						{
							this._root = child;
						}
						bool flag9 = treeNode6.Height > treeNode7.Height;
						if (flag9)
						{
							treeNode2.Child2 = child5;
							treeNode.Child1 = child6;
							treeNode7.ParentOrNext = iA;
							treeNode.AABB.Combine(ref treeNode3.AABB, ref treeNode7.AABB);
							treeNode2.AABB.Combine(ref treeNode.AABB, ref treeNode6.AABB);
							treeNode.Height = 1 + Math.Max(treeNode3.Height, treeNode7.Height);
							treeNode2.Height = 1 + Math.Max(treeNode.Height, treeNode6.Height);
						}
						else
						{
							treeNode2.Child2 = child6;
							treeNode.Child1 = child5;
							treeNode6.ParentOrNext = iA;
							treeNode.AABB.Combine(ref treeNode3.AABB, ref treeNode6.AABB);
							treeNode2.AABB.Combine(ref treeNode.AABB, ref treeNode7.AABB);
							treeNode.Height = 1 + Math.Max(treeNode3.Height, treeNode6.Height);
							treeNode2.Height = 1 + Math.Max(treeNode.Height, treeNode7.Height);
						}
						result = child;
					}
					else
					{
						result = iA;
					}
				}
			}
			return result;
		}

		public int ComputeHeight(int nodeId)
		{
			Debug.Assert(0 <= nodeId && nodeId < this._nodeCapacity);
			TreeNode<T> treeNode = this._nodes[nodeId];
			bool flag = treeNode.IsLeaf();
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				int val = this.ComputeHeight(treeNode.Child1);
				int val2 = this.ComputeHeight(treeNode.Child2);
				result = 1 + Math.Max(val, val2);
			}
			return result;
		}

		public int ComputeHeight()
		{
			return this.ComputeHeight(this._root);
		}

		public void ValidateStructure(int index)
		{
			bool flag = index == -1;
			if (!flag)
			{
				bool flag2 = index == this._root;
				if (flag2)
				{
					Debug.Assert(this._nodes[index].ParentOrNext == -1);
				}
				TreeNode<T> treeNode = this._nodes[index];
				int child = treeNode.Child1;
				int child2 = treeNode.Child2;
				bool flag3 = treeNode.IsLeaf();
				if (flag3)
				{
					Debug.Assert(child == -1);
					Debug.Assert(child2 == -1);
					Debug.Assert(treeNode.Height == 0);
				}
				else
				{
					Debug.Assert(0 <= child && child < this._nodeCapacity);
					Debug.Assert(0 <= child2 && child2 < this._nodeCapacity);
					Debug.Assert(this._nodes[child].ParentOrNext == index);
					Debug.Assert(this._nodes[child2].ParentOrNext == index);
					this.ValidateStructure(child);
					this.ValidateStructure(child2);
				}
			}
		}

		public void ValidateMetrics(int index)
		{
			bool flag = index == -1;
			if (!flag)
			{
				TreeNode<T> treeNode = this._nodes[index];
				int child = treeNode.Child1;
				int child2 = treeNode.Child2;
				bool flag2 = treeNode.IsLeaf();
				if (flag2)
				{
					Debug.Assert(child == -1);
					Debug.Assert(child2 == -1);
					Debug.Assert(treeNode.Height == 0);
				}
				else
				{
					Debug.Assert(0 <= child && child < this._nodeCapacity);
					Debug.Assert(0 <= child2 && child2 < this._nodeCapacity);
					int height = this._nodes[child].Height;
					int height2 = this._nodes[child2].Height;
					int num = 1 + Math.Max(height, height2);
					Debug.Assert(treeNode.Height == num);
					AABB aABB = default(AABB);
					aABB.Combine(ref this._nodes[child].AABB, ref this._nodes[child2].AABB);
					Debug.Assert(aABB.LowerBound == treeNode.AABB.LowerBound);
					Debug.Assert(aABB.UpperBound == treeNode.AABB.UpperBound);
					this.ValidateMetrics(child);
					this.ValidateMetrics(child2);
				}
			}
		}

		public void Validate()
		{
			this.ValidateStructure(this._root);
			this.ValidateMetrics(this._root);
			int num = 0;
			int num2 = this._freeList;
			while (num2 != -1)
			{
				Debug.Assert(0 <= num2 && num2 < this._nodeCapacity);
				num2 = this._nodes[num2].ParentOrNext;
				num++;
			}
			Debug.Assert(this.Height == this.ComputeHeight());
			Debug.Assert(this._nodeCount + num == this._nodeCapacity);
		}

		public void RebuildBottomUp()
		{
			int[] array = new int[this._nodeCount];
			int i = 0;
			for (int j = 0; j < this._nodeCapacity; j++)
			{
				bool flag = this._nodes[j].Height < 0;
				if (!flag)
				{
					bool flag2 = this._nodes[j].IsLeaf();
					if (flag2)
					{
						this._nodes[j].ParentOrNext = -1;
						array[i] = j;
						i++;
					}
					else
					{
						this.FreeNode(j);
					}
				}
			}
			while (i > 1)
			{
				FP y = Settings.MaxFP;
				int num = -1;
				int num2 = -1;
				for (int k = 0; k < i; k++)
				{
					AABB aABB = this._nodes[array[k]].AABB;
					for (int l = k + 1; l < i; l++)
					{
						AABB aABB2 = this._nodes[array[l]].AABB;
						AABB aABB3 = default(AABB);
						aABB3.Combine(ref aABB, ref aABB2);
						FP perimeter = aABB3.Perimeter;
						bool flag3 = perimeter < y;
						if (flag3)
						{
							num = k;
							num2 = l;
							y = perimeter;
						}
					}
				}
				int num3 = array[num];
				int num4 = array[num2];
				TreeNode<T> treeNode = this._nodes[num3];
				TreeNode<T> treeNode2 = this._nodes[num4];
				int num5 = this.AllocateNode();
				TreeNode<T> treeNode3 = this._nodes[num5];
				treeNode3.Child1 = num3;
				treeNode3.Child2 = num4;
				treeNode3.Height = 1 + Math.Max(treeNode.Height, treeNode2.Height);
				treeNode3.AABB.Combine(ref treeNode.AABB, ref treeNode2.AABB);
				treeNode3.ParentOrNext = -1;
				treeNode.ParentOrNext = num5;
				treeNode2.ParentOrNext = num5;
				array[num2] = array[i - 1];
				array[num] = num5;
				i--;
			}
			this._root = array[0];
			this.Validate();
		}

		public void ShiftOrigin(TSVector2 newOrigin)
		{
			for (int i = 0; i < this._nodeCapacity; i++)
			{
				TreeNode<T> expr_18_cp_0_cp_0 = this._nodes[i];
				expr_18_cp_0_cp_0.AABB.LowerBound = expr_18_cp_0_cp_0.AABB.LowerBound - newOrigin;
				TreeNode<T> expr_3B_cp_0_cp_0 = this._nodes[i];
				expr_3B_cp_0_cp_0.AABB.UpperBound = expr_3B_cp_0_cp_0.AABB.UpperBound - newOrigin;
			}
		}
	}
}
