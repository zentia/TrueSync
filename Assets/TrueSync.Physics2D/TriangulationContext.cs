using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace TrueSync.Physics2D
{
	internal abstract class TriangulationContext
	{
		public readonly List<TriangulationPoint> Points = new List<TriangulationPoint>(200);

		public readonly List<DelaunayTriangle> Triangles = new List<DelaunayTriangle>();

		public TriangulationMode TriangulationMode
		{
			get;
			protected set;
		}

		public Triangulatable Triangulatable
		{
			get;
			private set;
		}

		public bool WaitUntilNotified
		{
			get;
			private set;
		}

		public bool Terminated
		{
			get;
			set;
		}

		public int StepCount
		{
			get;
			private set;
		}

		public virtual bool IsDebugEnabled
		{
			get;
			protected set;
		}

		public TriangulationContext()
		{
			this.Terminated = false;
		}

		public void Done()
		{
			int stepCount = this.StepCount;
			this.StepCount = stepCount + 1;
		}

		public virtual void PrepareTriangulation(Triangulatable t)
		{
			this.Triangulatable = t;
			this.TriangulationMode = t.TriangulationMode;
			t.PrepareTriangulation(this);
		}

		public abstract TriangulationConstraint NewConstraint(TriangulationPoint a, TriangulationPoint b);

		[MethodImpl(MethodImplOptions.Synchronized)]
		public void Update(string message)
		{
		}

		public virtual void Clear()
		{
			this.Points.Clear();
			this.Terminated = false;
			this.StepCount = 0;
		}
	}
}
