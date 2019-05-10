using System;
using System.Collections;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	internal struct FixedBitArray3 : IEnumerable<bool>, IEnumerable
	{
		public bool _0;

		public bool _1;

		public bool _2;

		public bool this[int index]
		{
			get
			{
				bool result;
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

		public IEnumerator<bool> GetEnumerator()
		{
			return this.Enumerate().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		public bool Contains(bool value)
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

		public int IndexOf(bool value)
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
			this._0 = (this._1 = (this._2 = false));
		}

		public void Clear(bool value)
		{
			for (int i = 0; i < 3; i++)
			{
				bool flag = this[i] == value;
				if (flag)
				{
					this[i] = false;
				}
			}
		}

	    private IEnumerable<bool> Enumerate()
	    {
	        for (int i = 0; i < 3; ++i)
	            yield return this[i];
	    }
    }
}
