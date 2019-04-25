namespace TrueSync
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Reflection;

    public class HashList<T> : ICollection<T>, IEnumerable<T>, IEnumerable
    {
        private readonly List<T> collection;
        private readonly IComparer<T> comparer;

        public HashList()
        {
            this.collection = new List<T>();
            this.comparer = Comparer<T>.Default;
        }

        public void Add(T item)
        {
            if (this.Count == 0)
            {
                this.collection.Add(item);
            }
            else
            {
                int index = 0;
                int num2 = this.collection.Count - 1;
                while (index <= num2)
                {
                    int num3 = (index + num2) / 2;
                    int num4 = this.comparer.Compare(this.collection[num3], item);
                    if (num4 == 0)
                    {
                        return;
                    }
                    if (num4 < 0)
                    {
                        index = num3 + 1;
                    }
                    else
                    {
                        num2 = num3 - 1;
                    }
                }
                this.collection.Insert(index, item);
            }
        }

        public void AddRange(IEnumerable<T> iCollection)
        {
            this.collection.AddRange(iCollection);
        }

        public void Clear()
        {
            this.collection.Clear();
        }

        public bool Contains(T item)
        {
            return (this.collection.BinarySearch(item) != -1);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            this.collection.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return this.collection.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return this.collection.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public int Count
        {
            get
            {
                return this.collection.Count;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public T this[int index]
        {
            get
            {
                return this.collection[index];
            }
        }
    }
}

