namespace TrueSync.Physics2D
{
    using System;

    internal class TreeNode<T>
    {
        internal TrueSync.Physics2D.AABB AABB;
        internal int Child1;
        internal int Child2;
        internal int Height;
        internal int ParentOrNext;
        internal T UserData;

        internal bool IsLeaf()
        {
            return (this.Child1 == -1);
        }
    }
}

