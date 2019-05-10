using System;

namespace TrueSync.Physics2D
{
	public sealed class ContactVelocityConstraint
	{
		public VelocityConstraintPoint[] points = new VelocityConstraintPoint[2];

		public TSVector2 normal;

		public Mat22 normalMass;

		public Mat22 K;

		public int indexA;

		public int indexB;

		public FP invMassA;

		public FP invMassB;

		public FP invIA;

		public FP invIB;

		public FP friction;

		public FP restitution;

		public FP tangentSpeed;

		public int pointCount;

		public int contactIndex;

		public ContactVelocityConstraint()
		{
			for (int i = 0; i < 2; i++)
			{
				this.points[i] = new VelocityConstraintPoint();
			}
		}
	}
}
