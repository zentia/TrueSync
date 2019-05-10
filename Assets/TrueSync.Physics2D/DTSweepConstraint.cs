using System;

namespace TrueSync.Physics2D
{
	internal class DTSweepConstraint : TriangulationConstraint
	{
		public DTSweepConstraint(TriangulationPoint p1, TriangulationPoint p2)
		{
			this.P = p1;
			this.Q = p2;
			bool flag = p1.Y > p2.Y;
			if (flag)
			{
				this.Q = p1;
				this.P = p2;
			}
			else
			{
				bool flag2 = p1.Y == p2.Y;
				if (flag2)
				{
					bool flag3 = p1.X > p2.X;
					if (flag3)
					{
						this.Q = p1;
						this.P = p2;
					}
					else
					{
						bool flag4 = p1.X == p2.X;
						if (flag4)
						{
						}
					}
				}
			}
			this.Q.AddEdge(this);
		}
	}
}
