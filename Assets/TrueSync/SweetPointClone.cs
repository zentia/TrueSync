using System;

namespace TrueSync
{
	public class SweetPointClone
	{
		public IBroadphaseEntity body;

		public bool begin;

		public int axis;

		public void Clone(SweepPoint sp)
		{
			this.body = sp.Body;
			this.begin = sp.Begin;
			this.axis = sp.Axis;
		}

		public void Restore(SweepPoint sp)
		{
			sp.Body = this.body;
			sp.Begin = this.begin;
			sp.Axis = this.axis;
		}
	}
}
