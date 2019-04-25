namespace TrueSync.Physics2D
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;
    using System.Threading;

    [StructLayout(LayoutKind.Sequential)]
    internal struct FixedBitArray3 : IEnumerable<bool>, IEnumerable
    {
        public bool _0;
        public bool _1;
        public bool _2;
        public bool this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0:
                        return this._0;

                    case 1:
                        return this._1;

                    case 2:
                        return this._2;
                }
                throw new IndexOutOfRangeException();
            }
            set
            {
                switch (index)
                {
                    case 0:
                        this._0 = value;
                        break;

                    case 1:
                        this._1 = value;
                        break;

                    case 2:
                        this._2 = value;
                        break;

                    default:
                        throw new IndexOutOfRangeException();
                }
            }
        }
        public IEnumerator<bool> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool Contains(bool value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        public int IndexOf(bool value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this[i] == value)
                {
                    return i;
                }
            }
            return -1;
        }

        public void Clear()
        {
            this._0 = this._1 = this._2 = false;
        }

        public void Clear(bool value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this[i] == value)
                {
                    this[i] = false;
                }
            }
        }

        private IEnumerable<bool> Enumerate()
        {
            this.<i>5__1 = 0;
            while (this.<i>5__1 < 3)
            {
                yield return this[this.<i>5__1];
                int num2 = this.<i>5__1 + 1;
                this.<i>5__1 = num2;
            }
        }
    }
}

