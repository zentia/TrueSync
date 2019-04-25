namespace Microsoft.Xna.Framework
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class CurveKeyCollection : ICollection<CurveKey>, IEnumerable<CurveKey>, IEnumerable
    {
        private List<CurveKey> innerlist = new List<CurveKey>();
        private bool isReadOnly = false;

        public void Add(CurveKey item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("Value cannot be null.", null);
            }
            if (this.innerlist.Count == 0)
            {
                this.innerlist.Add(item);
            }
            else
            {
                for (int i = 0; i < this.innerlist.Count; i++)
                {
                    if (item.Position < this.innerlist[i].Position)
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

        public CurveKeyCollection Clone()
        {
            CurveKeyCollection keys = new CurveKeyCollection();
            foreach (CurveKey key in this.innerlist)
            {
                keys.Add(key);
            }
            return keys;
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

        public int IndexOf(CurveKey item)
        {
            return this.innerlist.IndexOf(item);
        }

        public bool Remove(CurveKey item)
        {
            return this.innerlist.Remove(item);
        }

        public void RemoveAt(int index)
        {
            if ((index == this.Count) || (index <= -1))
            {
                throw new ArgumentOutOfRangeException("Index was out of range. Must be non-negative and less than the size of the collection.\r\nParameter name: index", null);
            }
            this.innerlist.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.innerlist.GetEnumerator();
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

        public CurveKey this[int index]
        {
            get
            {
                return this.innerlist[index];
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }
                if (index >= this.innerlist.Count)
                {
                    throw new IndexOutOfRangeException();
                }
                if (this.innerlist[index].Position == value.Position)
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
    }
}

