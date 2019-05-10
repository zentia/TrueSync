using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class TriangulationPoint
	{
		public FP X;

		public FP Y;

		public List<DTSweepConstraint> Edges
		{
			get;
			private set;
		}

		public FP Xf
		{
			get
			{
				return this.X;
			}
			set
			{
				this.X = value;
			}
		}

		public FP Yf
		{
			get
			{
				return this.Y;
			}
			set
			{
				this.Y = value;
			}
		}

		public bool HasEdges
		{
			get
			{
				return this.Edges != null;
			}
		}

		public TriangulationPoint(FP x, FP y)
		{
			this.X = x;
			this.Y = y;
		}

		public override string ToString()
		{
			return string.Concat(new object[]
			{
				"[",
				this.X,
				",",
				this.Y,
				"]"
			});
		}

		public void AddEdge(DTSweepConstraint e)
		{
			bool flag = this.Edges == null;
			if (flag)
			{
				this.Edges = new List<DTSweepConstraint>();
			}
			this.Edges.Add(e);
		}
	}
}
