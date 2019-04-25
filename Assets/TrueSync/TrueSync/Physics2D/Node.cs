namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;

    internal abstract class Node
    {
        protected Node LeftChild;
        public List<Node> ParentList = new List<Node>();
        protected Node RightChild;

        protected Node(Node left, Node right)
        {
            this.LeftChild = left;
            this.RightChild = right;
            if (left > null)
            {
                left.ParentList.Add(this);
            }
            if (right > null)
            {
                right.ParentList.Add(this);
            }
        }

        public abstract Sink Locate(TrueSync.Physics2D.Edge s);
        public void Replace(Node node)
        {
            foreach (Node node2 in node.ParentList)
            {
                if (node2.LeftChild == node)
                {
                    node2.LeftChild = this;
                }
                else
                {
                    node2.RightChild = this;
                }
            }
            this.ParentList.AddRange(node.ParentList);
        }
    }
}

