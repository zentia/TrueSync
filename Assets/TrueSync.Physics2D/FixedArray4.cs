using System;

namespace TrueSync.Physics2D
{
	public struct FixedArray4<T>
	{
		private T _value0;

		private T _value1;

		private T _value2;

		private T _value3;

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
				case 3:
					result = this._value3;
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
				case 3:
					this._value3 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}
	}
}
