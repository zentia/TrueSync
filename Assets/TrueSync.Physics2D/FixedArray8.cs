using System;

namespace TrueSync.Physics2D
{
	public struct FixedArray8<T>
	{
		private T _value0;

		private T _value1;

		private T _value2;

		private T _value3;

		private T _value4;

		private T _value5;

		private T _value6;

		private T _value7;

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
				case 4:
					result = this._value4;
					break;
				case 5:
					result = this._value5;
					break;
				case 6:
					result = this._value6;
					break;
				case 7:
					result = this._value7;
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
				case 4:
					this._value4 = value;
					break;
				case 5:
					this._value5 = value;
					break;
				case 6:
					this._value6 = value;
					break;
				case 7:
					this._value7 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}
	}
}
