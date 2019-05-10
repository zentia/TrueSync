using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
	public class CurveKeyCollection : ICollection<CurveKey>, IEnumerable<CurveKey>, IEnumerable
	{
		private List<CurveKey> innerlist;

		private bool isReadOnly = false;

		public CurveKey this[int index]
		{
			get
			{
				return this.innerlist[index];
			}
			set
			{
				bool flag = value == null;
				if (flag)
				{
					throw new ArgumentNullException();
				}
				bool flag2 = index >= this.innerlist.Count;
				if (flag2)
				{
					throw new IndexOutOfRangeException();
				}
				bool flag3 = this.innerlist[index].Position == value.Position;
				if (flag3)
				{
					this.innerlist[index] = value;
				}
				else
				{
					this.innerlist.RemoveAt(index);
					this.innerlist.Add(value);
				}
			}
		}

		public int Count
		{
			get
			{
				return this.innerlist.Count;
			}
		}

		public bool IsReadOnly
		{
			get
			{
				return this.isReadOnly;
			}
		}

		public CurveKeyCollection()
		{
			this.innerlist = new List<CurveKey>();
		}

		public void Add(CurveKey item)
		{
			bool flag = item == null;
			if (flag)
			{
				throw new ArgumentNullException("Value cannot be null.", null);
			}
			bool flag2 = this.innerlist.Count == 0;
			if (flag2)
			{
				this.innerlist.Add(item);
			}
			else
			{
				for (int i = 0; i < this.innerlist.Count; i++)
				{
					bool flag3 = item.Position < this.innerlist[i].Position;
					if (flag3)
					{
						this.innerlist.Insert(i, item);
						return;
					}
				}
				this.innerlist.Add(item);
			}
		}

		public void Clear()
		{
			this.innerlist.Clear();
		}

		public bool Contains(CurveKey item)
		{
			return this.innerlist.Contains(item);
		}

		public void CopyTo(CurveKey[] array, int arrayIndex)
		{
			this.innerlist.CopyTo(array, arrayIndex);
		}

		public IEnumerator<CurveKey> GetEnumerator()
		{
			return this.innerlist.GetEnumerator();
		}

		public bool Remove(CurveKey item)
		{
			return this.innerlist.Remove(item);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.innerlist.GetEnumerator();
		}

		public CurveKeyCollection Clone()
		{
			CurveKeyCollection curveKeyCollection = new CurveKeyCollection();
			foreach (CurveKey current in this.innerlist)
			{
				curveKeyCollection.Add(current);
			}
			return curveKeyCollection;
		}

		public int IndexOf(CurveKey item)
		{
			return this.innerlist.IndexOf(item);
		}

		public void RemoveAt(int index)
		{
			bool flag = index != this.Count && index > -1;
			if (flag)
			{
				this.innerlist.RemoveAt(index);
				return;
			}
			throw new ArgumentOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", null);
		}
	}
}
