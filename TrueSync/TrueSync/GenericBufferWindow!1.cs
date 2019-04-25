namespace TrueSync
{
    using System;

    public class GenericBufferWindow<T>
    {
        public T[] buffer;
        public int currentIndex;
        public int size;

        public GenericBufferWindow(int size)
        {
            this.size = size;
            currentIndex = 0;
            buffer = new T[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = Activator.CreateInstance<T>();
            }
        }

        public GenericBufferWindow(int size, NewInstance NewInstance)
        {
            this.size = size;
            currentIndex = 0;
            buffer = new T[size];
            for (int i = 0; i < size; i++)
            {
                buffer[i] = NewInstance();
            }
        }

        public T Current()
        {
            return buffer[currentIndex];
        }

        public void MoveNext()
        {
            currentIndex = (currentIndex + 1) % size;
        }

        public T Previous()
        {
            int index = currentIndex - 1;
            if (index < 0)
            {
                index = size - 1;
            }
            return buffer[index];
        }

        public void Resize(int newSize)
        {
            if (newSize != size)
            {
                T[] localArray = new T[newSize];
                int num = newSize - size;
                if (newSize > size)
                {
                    for (int i = 0; i < size; i++)
                    {
                        if (i < this.currentIndex)
                        {
                            localArray[i] = this.buffer[i];
                        }
                        else
                        {
                            localArray[i + num] = this.buffer[i];
                        }
                    }
                    for (int j = 0; j < num; j++)
                    {
                        localArray[currentIndex + j] = Activator.CreateInstance<T>();
                    }
                }
                else
                {
                    for (int k = 0; k < newSize; k++)
                    {
                        if (k < this.currentIndex)
                        {
                            localArray[k] = this.buffer[k];
                        }
                        else
                        {
                            localArray[k] = this.buffer[k - num];
                        }
                    }
                    this.currentIndex = this.currentIndex % newSize;
                }
                this.buffer = localArray;
                this.size = newSize;
            }
        }

        public void Set(T instance)
        {
            this.buffer[this.currentIndex] = instance;
        }

        public delegate T NewInstance();
    }
}