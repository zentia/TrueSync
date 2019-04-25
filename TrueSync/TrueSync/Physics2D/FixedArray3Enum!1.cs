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
    internal struct FixedArray3Enum<T> : IEnumerable<T>, IEnumerable where T: class
    {
        public T _0;
        public T _1;
        public T _2;
        public T this[int index]
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
        public IEnumerator<T> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public bool Contains(T value)
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

        public int IndexOf(T value)
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
            this._0 = this._1 = this._2 = default(T);
        }

        public void Clear(T value)
        {
            for (int i = 0; i < 3; i++)
            {
                if (this[i] == value)
                {
                    T local = default(T);
                    this[i] = local;
                }
            }
        }

        private IEnumerable<T> Enumerate()
        {
            this.<i>5__1 = 0;
            while (this.<i>5__1 < 3)
            {
                yield return this[this.<i>5__1];
                int num2 = this.<i>5__1 + 1;
                this.<i>5__1 = num2;
            }
        }
        [CompilerGenerated]
        private sealed class <Enumerate>d__12 : IEnumerable<T>, IEnumerable, IEnumerator<T>, IDisposable, IEnumerator
        {
            private int <>1__state;
            private T <>2__current;
            public FixedArray3Enum<T> <>3__<>4__this;
            public FixedArray3Enum<T> <>4__this;
            private int <>l__initialThreadId;
            private int <i>5__1;

            [DebuggerHidden]
            public <Enumerate>d__12(int <>1__state)
            {
                this.<>1__state = <>1__state;
                this.<>l__initialThreadId = Thread.CurrentThread.ManagedThreadId;
            }

            private bool MoveNext()
            {
                switch (this.<>1__state)
                {
                    case 0:
                        this.<>1__state = -1;
                        this.<i>5__1 = 0;
                        while (this.<i>5__1 < 3)
                        {
                            this.<>2__current = this.<>4__this[this.<i>5__1];
                            this.<>1__state = 1;
                            return true;
                        Label_0049:
                            this.<>1__state = -1;
                            int num2 = this.<i>5__1 + 1;
                            this.<i>5__1 = num2;
                        }
                        return false;

                    case 1:
                        goto Label_0049;
                }
                return false;
            }

            [DebuggerHidden]
            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                FixedArray3Enum<T>.<Enumerate>d__12 d__;
                if ((this.<>1__state == -2) && (this.<>l__initialThreadId == Thread.CurrentThread.ManagedThreadId))
                {
                    this.<>1__state = 0;
                    d__ = (FixedArray3Enum<T>.<Enumerate>d__12) this;
                }
                else
                {
                    d__ = new FixedArray3Enum<T>.<Enumerate>d__12(0);
                }
                d__.<>4__this = this.<>3__<>4__this;
                return d__;
            }

            [DebuggerHidden]
            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.System.Collections.Generic.IEnumerable<T>.GetEnumerator();
            }

            [DebuggerHidden]
            void IEnumerator.Reset()
            {
                throw new NotSupportedException();
            }

            [DebuggerHidden]
            void IDisposable.Dispose()
            {
            }

            T IEnumerator<T>.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }

            object IEnumerator.Current
            {
                [DebuggerHidden]
                get
                {
                    return this.<>2__current;
                }
            }
        }
    }
}

