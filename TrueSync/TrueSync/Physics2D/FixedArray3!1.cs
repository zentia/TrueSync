namespace TrueSync.Physics2D
{
    using System;
    using System.Reflection;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    public struct FixedArray3<T>
    {
        private T _value0;
        private T _value1;
        private T _value2;
        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this._value0;

                    case 1:
                        return this._value1;

                    case 2:
                        return this._value2;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this._value0 = value;
                        break;

                    case 1:
                        this._value1 = value;
                        break;

                    case 2:
                        this._value2 = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
    }
}

