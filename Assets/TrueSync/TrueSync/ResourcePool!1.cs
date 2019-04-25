namespace TrueSync
{
    using System;
    using System.Collections.Generic;

    public class ResourcePool<T>
    {
        protected Stack<T> stack;

        public ResourcePool()
        {
            stack = new Stack<T>(10);
        }

        public T GetNew()
        {
            if (stack.Count == 0)
            {
                this.stack.Push(NewInstance());
            }
            T local = this.stack.Pop();
            if (local is ResourcePoolItem)
            {
                ((ResourcePoolItem) local).CleanUp();
            }
            return local;
        }

        public void GiveBack(T obj)
        {
            this.stack.Push(obj);
        }

        protected virtual T NewInstance()
        {
            return Activator.CreateInstance<T>();
        }

        public void ResetResourcePool()
        {
            this.stack.Clear();
        }

        public int Count
        {
            get
            {
                return stack.Count;
            }
        }
    }
}

