using System;

namespace TrueSync.Physics2D
{
	public struct FixedArray2<T>
	{
		private T _value0;

		private T _value1;

		public T this[int index]
		{
			get
			{
				T result;
				if (index != 0)
				{
					if (index != 1)
					{
						throw new IndexOutOfRangeException();
					}
					result = this._value1;
				}
				else
				{
					result = this._value0;
				}
				return result;
			}
			set
			{
				if (index != 0)
				{
					if (index != 1)
					{
						throw new IndexOutOfRangeException();
					}
					this._value1 = value;
				}
				else
				{
					this._value0 = value;
				}
			}
		}
	}
}
