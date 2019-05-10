using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class ConvexHullShape : Shape
	{
		private List<TSVector> vertices = null;

		private TSVector shifted;

		public TSVector Shift
		{
			get
			{
				return -1 * this.shifted;
			}
		}

		public ConvexHullShape(List<TSVector> vertices)
		{
			this.vertices = vertices;
			this.UpdateShape();
		}

		public override void CalculateMassInertia()
		{
			this.mass = Shape.CalculateMassInertia(this, out this.shifted, out this.inertia);
		}

		public override void SupportMapping(ref TSVector direction, out TSVector result)
		{
			FP y = FP.MinValue;
			int index = 0;
			for (int i = 0; i < this.vertices.Count; i++)
			{
				FP fP = TSVector.Dot(this.vertices[i], direction);
				bool flag = fP > y;
				if (flag)
				{
					y = fP;
					index = i;
				}
			}
			result = this.vertices[index] - this.shifted;
		}
	}
}
