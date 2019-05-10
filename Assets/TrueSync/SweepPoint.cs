using System;

namespace TrueSync
{
	public class SweepPoint
	{
		public IBroadphaseEntity Body;

		public bool Begin;

		public int Axis;

		public FP Value
		{
			get
			{
				bool begin = this.Begin;
				FP result;
				if (begin)
				{
					bool flag = this.Axis == 0;
					if (flag)
					{
						result = this.Body.BoundingBox.min.x;
					}
					else
					{
						bool flag2 = this.Axis == 1;
						if (flag2)
						{
							result = this.Body.BoundingBox.min.y;
						}
						else
						{
							result = this.Body.BoundingBox.min.z;
						}
					}
				}
				else
				{
					bool flag3 = this.Axis == 0;
					if (flag3)
					{
						result = this.Body.BoundingBox.max.x;
					}
					else
					{
						bool flag4 = this.Axis == 1;
						if (flag4)
						{
							result = this.Body.BoundingBox.max.y;
						}
						else
						{
							result = this.Body.BoundingBox.max.z;
						}
					}
				}
				return result;
			}
		}

		public SweepPoint(IBroadphaseEntity body, bool begin, int axis)
		{
			this.Body = body;
			this.Begin = begin;
			this.Axis = axis;
		}
	}
}
