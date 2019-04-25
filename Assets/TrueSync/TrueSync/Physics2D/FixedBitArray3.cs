// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.FixedBitArray3
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

using System;
using System.Collections;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
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

        public IEnumerator<bool> GetEnumerator()
        {
            return this.Enumerate().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)this.GetEnumerator();
        }

        public bool Contains(bool value)
        {
            for (int index = 0; index < 3; ++index)
            {
                if (this[index] == value)
                    return true;
            }
            return false;
        }

        public int IndexOf(bool value)
        {
            for (int index = 0; index < 3; ++index)
            {
                if (this[index] == value)
                    return index;
            }
            return -1;
        }

        public void Clear()
        {
            this._0 = this._1 = this._2 = false;
        }

        public void Clear(bool value)
        {
            for (int index = 0; index < 3; ++index)
            {
                if (this[index] == value)
                    this[index] = false;
            }
        }

        private IEnumerable<bool> Enumerate()
        {
            for (int i = 0; i < 3; ++i)
                yield return this[i];
        }
    }
}
