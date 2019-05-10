using System;

namespace TrueSync.Physics2D
{
	internal class PointOnEdgeException : NotImplementedException
	{
		public PointOnEdgeException(string message) : base(message)
		{
		}
	}
}
