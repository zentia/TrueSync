namespace TrueSync.Physics2D
{
    using System;

    internal class YNode : Node
    {
        private TrueSync.Physics2D.Edge _edge;

        public YNode(TrueSync.Physics2D.Edge edge, Node lChild, Node rChild) : base(lChild, rChild)
        {
            this._edge = edge;
        }

        public override Sink Locate(TrueSync.Physics2D.Edge edge)
        {
            if (this._edge.IsAbove(edge.P))
            {
                return base.RightChild.Locate(edge);
            }
            if (!this._edge.IsBelow(edge.P) && (edge.Slope < this._edge.Slope))
            {
                return base.RightChild.Locate(edge);
            }
            return base.LeftChild.Locate(edge);
        }
    }
}

