namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;

    internal class ConstrainedPointSet : PointSet
    {
        private List<TriangulationPoint> _constrainedPointList;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private int[] <EdgeIndex>k__BackingField;

        public ConstrainedPointSet(List<TriangulationPoint> points, IEnumerable<TriangulationPoint> constraints) : base(points)
        {
            this._constrainedPointList = new List<TriangulationPoint>();
            this._constrainedPointList.AddRange(constraints);
        }

        public ConstrainedPointSet(List<TriangulationPoint> points, int[] index) : base(points)
        {
            this.EdgeIndex = index;
        }

        public bool isValid()
        {
            return true;
        }

        public override void PrepareTriangulation(TriangulationContext tcx)
        {
            base.PrepareTriangulation(tcx);
            if (this._constrainedPointList > null)
            {
                List<TriangulationPoint>.Enumerator enumerator = this._constrainedPointList.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    TriangulationPoint current = enumerator.Current;
                    enumerator.MoveNext();
                    TriangulationPoint b = enumerator.Current;
                    tcx.NewConstraint(current, b);
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

        public int[] EdgeIndex { get; private set; }

        public override TrueSync.Physics2D.TriangulationMode TriangulationMode
        {
            get
            {
                return TrueSync.Physics2D.TriangulationMode.Constrained;
            }
        }
    }
}

