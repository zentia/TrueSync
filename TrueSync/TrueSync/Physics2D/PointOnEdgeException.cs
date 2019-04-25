namespace TrueSync.Physics2D
{
    using System;

    internal class PointOnEdgeException : NotImplementedException
    {
        public PointOnEdgeException(string message) : base(message)
        {
        }
    }
}

