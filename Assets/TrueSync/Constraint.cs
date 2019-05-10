using System;

namespace TrueSync
{
	public abstract class Constraint : IConstraint, IDebugDrawable, IComparable
	{
		internal RigidBody body1;

		internal RigidBody body2;

		internal static int instanceCount = 0;

		private int instance;

		public RigidBody Body1
		{
			get
			{
				return this.body1;
			}
		}

		public RigidBody Body2
		{
			get
			{
				return this.body2;
			}
		}

		public Constraint(RigidBody body1, RigidBody body2)
		{
			this.body1 = body1;
			this.body2 = body2;
			Constraint.instanceCount++;
			this.instance = Constraint.instanceCount;
			bool flag = body1 != null;
			if (flag)
			{
				body1.Update();
			}
			bool flag2 = body2 != null;
			if (flag2)
			{
				body2.Update();
			}
		}

		public virtual void PrepareForIteration(FP timestep)
		{
		}

		public virtual void Iterate()
		{
		}

		public virtual void PostStep()
		{
		}

		public int CompareTo(object otherObj)
		{
			Constraint constraint = (Constraint)otherObj;
			bool flag = constraint.instance < this.instance;
			int result;
			if (flag)
			{
				result = -1;
			}
			else
			{
				bool flag2 = constraint.instance > this.instance;
				if (flag2)
				{
					result = 1;
				}
				else
				{
					result = 0;
				}
			}
			return result;
		}

		public virtual void DebugDraw(IDebugDrawer drawer)
		{
		}
	}
}
