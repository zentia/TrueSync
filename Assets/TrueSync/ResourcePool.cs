using System;
using System.Collections.Generic;

namespace TrueSync
{
	public class ResourcePool<T>
	{
		protected Stack<T> stack = new Stack<T>(10);

		public int Count
		{
			get
			{
				return this.stack.Count;
			}
		}

		public void ResetResourcePool()
		{
			this.stack.Clear();
		}

		public void GiveBack(T obj)
		{
			this.stack.Push(obj);
		}

		public T GetNew()
		{
			bool flag = this.stack.Count == 0;
			if (flag)
			{
				this.stack.Push(this.NewInstance());
			}
			T t = this.stack.Pop();
			bool flag2 = t is ResourcePoolItem;
			if (flag2)
			{
				((ResourcePoolItem)((object)t)).CleanUp();
			}
			return t;
		}

		protected virtual T NewInstance()
		{
			return Activator.CreateInstance<T>();
		}
	}
}
