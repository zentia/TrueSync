using System;
using System.Collections;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal struct FixedArray3Enum<T> : IEnumerable<T>, IEnumerable where T : class
	{
		public T _0;

		public T _1;

		public T _2;

		public T this[int index]
		{
			get
			{
				T result;
				switch (index)
				{
				case 0:
					result = this._0;
					break;
				case 1:
					result = this._1;
					break;
				case 2:
					result = this._2;
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
					this._0 = value;
					break;
				case 1:
					this._1 = value;
					break;
				case 2:
					this._2 = value;
					break;
				default:
					throw new IndexOutOfRangeException();
				}
			}
		}

		public IEnumerator<T> GetEnumerator()
		{
			return this.Enumerate().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool Contains(T value)
		{
			bool result;
			for (int i = 0; i < 3; i++)
			{
				bool flag = this[i] == value;
				if (flag)
				{
					result = true;
					return result;
				}
			}
			result = false;
			return result;
		}

		public int IndexOf(T value)
		{
			int result;
			for (int i = 0; i < 3; i++)
			{
				bool flag = this[i] == value;
				if (flag)
				{
					result = i;
					return result;
				}
			}
			result = -1;
			return result;
		}

		public void Clear()
		{
			this._0 = (this._1 = (this._2 = default(T)));
		}

		public void Clear(T value)
		{
			for (int i = 0; i < 3; i++)
			{
				bool flag = this[i] == value;
				if (flag)
				{
					this[i] = default(T);
				}
			}
		}

	    private IEnumerable<T> Enumerate()
	    {
	        for (int i = 0; i < 3; ++i)
	            yield return this[i];
	    }
    }
}
