using System;

namespace TrueSync.Physics2D
{
	internal class XNode : Node
	{
		private Point _point;

		public XNode(Point point, Node lChild, Node rChild) : base(lChild, rChild)
		{
			this._point = point;
		}

		public override Sink Locate(Edge edge)
		{
			bool flag = edge.P.X >= this._point.X;
			Sink result;
			if (flag)
			{
				result = this.RightChild.Locate(edge);
			}
			else
			{
				result = this.LeftChild.Locate(edge);
			}
			return result;
		}
	}
}
