using System;
using System.Collections;
using System.Collections.Generic;

namespace TrueSync
{
	public class ReadOnlyHashset<T> : IEnumerable, IEnumerable<T> where T : IComparable
	{
		private HashList<T> hashset;

		public int Count
		{
			get
			{
				return this.hashset.Count;
			}
		}

		public ReadOnlyHashset(HashList<T> hashset)
		{
			this.hashset = hashset;
		}

		public IEnumerator GetEnumerator()
		{
			return this.hashset.GetEnumerator();
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return this.hashset.GetEnumerator();
		}

		public bool Contains(T item)
		{
			return this.hashset.Contains(item);
		}
	}
}
