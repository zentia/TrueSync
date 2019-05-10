using System;

namespace TrueSync.Physics2D
{
	internal class YNode : Node
	{
		private Edge _edge;

		public YNode(Edge edge, Node lChild, Node rChild) : base(lChild, rChild)
		{
			this._edge = edge;
		}

		public override Sink Locate(Edge edge)
		{
			bool flag = this._edge.IsAbove(edge.P);
			Sink result;
			if (flag)
			{
				result = this.RightChild.Locate(edge);
			}
			else
			{
				bool flag2 = this._edge.IsBelow(edge.P);
				if (flag2)
				{
					result = this.LeftChild.Locate(edge);
				}
				else
				{
					bool flag3 = edge.Slope < this._edge.Slope;
					if (flag3)
					{
						result = this.RightChild.Locate(edge);
					}
					else
					{
						result = this.LeftChild.Locate(edge);
					}
				}
			}
			return result;
		}
	}
}
