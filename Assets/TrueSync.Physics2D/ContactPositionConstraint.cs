using System;

namespace TrueSync.Physics2D
{
	public sealed class ContactPositionConstraint
	{
		public TSVector2[] localPoints = new TSVector2[2];

		public TSVector2 localNormal;

		public TSVector2 localPoint;

		public int indexA;

		public int indexB;

		public FP invMassA;

		public FP invMassB;

		public TSVector2 localCenterA;

		public TSVector2 localCenterB;

		public FP invIA;

		public FP invIB;

		public ManifoldType type;

		public FP radiusA;

		public FP radiusB;

		public int pointCount;
	}
}
