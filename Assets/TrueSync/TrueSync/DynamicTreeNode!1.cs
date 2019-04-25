namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct DynamicTreeNode<T>
    {
        public TSBBox AABB;
        public FP MinorRandomExtension;
        public int Child1;
        public int Child2;
        public int LeafCount;
        public int ParentOrNext;
        public T UserData;
        public bool IsLeaf()
        {
            return (this.Child1 == -1);
        }
    }
}

