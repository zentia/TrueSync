using System;

namespace TrueSync.Physics2D
{
	public struct FixedArray3<T>
	{
		private T _value0;

		private T _value1;

		private T _value2;

		public T this[int index]
		{
			get
			{
				T result;
				switch (index)
				{
				case 0:
					result = this._value0;
					break;
				case 1:
					result = this._value1;
					break;
				case 2:
					result = this._value2;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
				return result;
			}
			set
			{
				switch (index)
				{
				case 0:
					this._value0 = value;
					break;
				case 1:
					this._value1 = value;
					break;
				case 2:
					this._value2 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}
	}
}
