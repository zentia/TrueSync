namespace TrueSync
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    public class ReadOnlyHashset<T> : IEnumerable, IEnumerable<T> where T: IComparable
    {
        private HashList<T> hashset;

        public ReadOnlyHashset(HashList<T> hashset)
        {
            this.hashset = hashset;
        }

        public bool Contains(T item)
        {
            return this.hashset.Contains(item);
        }

        public IEnumerator GetEnumerator()
        {
            return this.hashset.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return this.hashset.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.hashset.Count;
            }
        }
    }
}

