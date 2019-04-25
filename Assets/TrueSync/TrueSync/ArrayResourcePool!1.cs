namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class ArrayResourcePool<T>
    {
        private int arrayLength;
        private Stack<T[]> stack;

        public ArrayResourcePool(int arrayLength)
        {
            this.stack = new Stack<T[]>();
            this.arrayLength = arrayLength;
        }

        public T[] GetNew()
        {
            Stack<T[]> stack = this.stack;
            lock (stack)
            {
                if (this.stack.Count == 0)
                {
                    T[] item = new T[this.arrayLength];
                    this.stack.Push(item);
                }
                return this.stack.Pop();
            }
        }

        public void GiveBack(T[] obj)
        {
            Stack<T[]> stack = this.stack;
            lock (stack)
            {
                this.stack.Push(obj);
            }
        }

        public void ResetResourcePool()
        {
            Stack<T[]> stack = this.stack;
            lock (stack)
            {
                this.stack.Clear();
            }
        }

        public int Count
        {
            get
            {
                return this.stack.Count;
            }
        }
    }
}

