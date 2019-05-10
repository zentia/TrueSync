using System;

namespace TrueSync.Physics2D
{
	internal class Sink : Node
	{
		public Trapezoid Trapezoid;

		private Sink(Trapezoid trapezoid) : base(null, null)
		{
			this.Trapezoid = trapezoid;
			trapezoid.Sink = this;
		}

		public static Sink Isink(Trapezoid trapezoid)
		{
			bool flag = trapezoid.Sink == null;
			Sink result;
			if (flag)
			{
				result = new Sink(trapezoid);
			}
			else
			{
				result = trapezoid.Sink;
			}
			return result;
		}

		public override Sink Locate(Edge edge)
		{
			return this;
		}
	}
}
