using System;

namespace TrueSync.Physics2D
{
	public sealed class JointEdge
	{
		public Joint Joint;

		public JointEdge Next;

		public Body Other;

		public JointEdge Prev;
	}
}
