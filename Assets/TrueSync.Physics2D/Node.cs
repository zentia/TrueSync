using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal abstract class Node
	{
		protected Node LeftChild;

		public List<Node> ParentList;

		protected Node RightChild;

		protected Node(Node left, Node right)
		{
			this.ParentList = new List<Node>();
			this.LeftChild = left;
			this.RightChild = right;
			bool flag = left != null;
			if (flag)
			{
				left.ParentList.Add(this);
			}
			bool flag2 = right != null;
			if (flag2)
			{
				right.ParentList.Add(this);
			}
		}

		public abstract Sink Locate(Edge s);

		public void Replace(Node node)
		{
			foreach (Node current in node.ParentList)
			{
				bool flag = current.LeftChild == node;
				if (flag)
				{
					current.LeftChild = this;
				}
				else
				{
					current.RightChild = this;
				}
			}
			this.ParentList.AddRange(node.ParentList);
		}
	}
}
