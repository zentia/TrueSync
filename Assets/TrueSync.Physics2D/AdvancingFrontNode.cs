using System;

namespace TrueSync.Physics2D
{
	internal class AdvancingFrontNode
	{
		public AdvancingFrontNode Next;

		public TriangulationPoint Point;

		public AdvancingFrontNode Prev;

		public DelaunayTriangle Triangle;

		public FP Value;

		public bool HasNext
		{
			get
			{
				return this.Next != null;
			}
		}

		public bool HasPrev
		{
			get
			{
				return this.Prev != null;
			}
		}

		public AdvancingFrontNode(TriangulationPoint point)
		{
			this.Point = point;
			this.Value = point.X;
		}
	}
}
