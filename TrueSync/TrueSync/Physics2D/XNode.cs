namespace TrueSync.Physics2D
{
    using System;

    internal class XNode : Node
    {
        private Point _point;

        public XNode(Point point, Node lChild, Node rChild) : base(lChild, rChild)
        {
            this._point = point;
        }

        public override Sink Locate(TrueSync.Physics2D.Edge edge)
        {
            if (edge.P.X >= this._point.X)
            {
                return base.RightChild.Locate(edge);
            }
            return base.LeftChild.Locate(edge);
        }
    }
}

