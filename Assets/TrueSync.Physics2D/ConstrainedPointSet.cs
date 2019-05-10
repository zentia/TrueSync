using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal class ConstrainedPointSet : PointSet
	{
		private List<TriangulationPoint> _constrainedPointList;

		public int[] EdgeIndex
		{
			get;
			private set;
		}

		public override TriangulationMode TriangulationMode
		{
			get
			{
				return TriangulationMode.Constrained;
			}
		}

		public ConstrainedPointSet(List<TriangulationPoint> points, int[] index) : base(points)
		{
			this.EdgeIndex = index;
		}

		public ConstrainedPointSet(List<TriangulationPoint> points, IEnumerable<TriangulationPoint> constraints) : base(points)
		{
			this._constrainedPointList = new List<TriangulationPoint>();
			this._constrainedPointList.AddRange(constraints);
		}

		public override void PrepareTriangulation(TriangulationContext tcx)
		{
			base.PrepareTriangulation(tcx);
			bool flag = this._constrainedPointList != null;
			if (flag)
			{
				List<TriangulationPoint>.Enumerator enumerator = this._constrainedPointList.GetEnumerator();
				while (enumerator.MoveNext())
				{
					TriangulationPoint current = enumerator.Current;
					enumerator.MoveNext();
					TriangulationPoint current2 = enumerator.Current;
					tcx.NewConstraint(current, current2);
				}
			}
			else
			{
				for (int i = 0; i < this.EdgeIndex.Length; i += 2)
				{
					tcx.NewConstraint(base.Points[this.EdgeIndex[i]], base.Points[this.EdgeIndex[i + 1]]);
				}
			}
		}

		public bool isValid()
		{
			return true;
		}
	}
}
