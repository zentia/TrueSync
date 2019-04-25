// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.FixedArray3Enum`1
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
    internal struct FixedArray3Enum<T> : IEnumerable<T>, IEnumerable where T : class
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
                    default:
                        throw new IndexOutOfRangeException();
                }
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
            return (IEnumerator)this.GetEnumerator();
        }

        public bool Contains(T value)
        {
            for (int index = 0; index < 3; ++index)
            {
                if ((object)this[index] == (object)value)
                    return true;
            }
            return false;
        }

        public int IndexOf(T value)
        {
            for (int index = 0; index < 3; ++index)
            {
                if ((object)this[index] == (object)value)
                    return index;
            }
            return -1;
        }

        public void Clear()
        {
            this._0 = this._1 = this._2 = default(T);
        }

        public void Clear(T value)
        {
            for (int index = 0; index < 3; ++index)
            {
                if ((object)this[index] == (object)value)
                    this[index] = default(T);
            }
        }

        private IEnumerable<T> Enumerate()
        {
            for (int i = 0; i < 3; ++i)
                yield return this[i];
        }
    }
}
