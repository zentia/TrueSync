namespace TrueSync.Physics2D
{
    using System;
    using System.Collections.Generic;
    using TrueSync;

    public static class MarchingSquares
    {
        private static int[] _lookMarch = new int[] { 0, 0xe0, 0x38, 0xd8, 14, 0xee, 0x36, 0xd6, 0x83, 0x63, 0xbb, 0x5b, 0x8d, 0x6d, 0xb5, 0x55 };

        private static void combLeft(ref GeomPoly polya, ref GeomPoly polyb)
        {
            CxFastList<TSVector2> points = polya.Points;
            CxFastList<TSVector2> list2 = polyb.Points;
            CxFastListNode<TSVector2> node = points.Begin();
            CxFastListNode<TSVector2> node2 = list2.Begin();
            TSVector2 b = node2.Elem();
            CxFastListNode<TSVector2> prev = null;
            while (node != points.End())
            {
                TSVector2 a = node.Elem();
                if (VecDsq(a, b) < Settings.Epsilon)
                {
                    if (prev > null)
                    {
                        TSVector2 vector8 = prev.Elem();
                        b = node2.Next().Elem();
                        TSVector2 vector9 = a - vector8;
                        TSVector2 vector10 = b - a;
                        FP fp2 = VecCross(vector9, vector10);
                        if ((fp2 * fp2) < Settings.Epsilon)
                        {
                            points.Erase(prev, node);
                            polya.Length--;
                            node = prev;
                        }
                    }
                    bool flag2 = true;
                    CxFastListNode<TSVector2> node4 = null;
                    while (!list2.Empty())
                    {
                        TSVector2 vector11 = list2.Front();
                        list2.Pop();
                        if (!flag2 && !list2.Empty())
                        {
                            node = points.Insert(node, vector11);
                            polya.Length++;
                            node4 = node;
                        }
                        flag2 = false;
                    }
                    node = node.Next();
                    TSVector2 vector3 = node.Elem();
                    node = node.Next();
                    if (node == points.End())
                    {
                        node = points.Begin();
                    }
                    TSVector2 vector4 = node.Elem();
                    TSVector2 vector5 = node4.Elem();
                    TSVector2 vector6 = vector3 - vector5;
                    TSVector2 vector7 = vector4 - vector3;
                    FP fp = VecCross(vector6, vector7);
                    if ((fp * fp) < Settings.Epsilon)
                    {
                        points.Erase(node4, node4.Next());
                        polya.Length--;
                    }
                    break;
                }
                prev = node;
                node = node.Next();
            }
        }

        public static List<Vertices> DetectSquares(AABB domain, FP cellWidth, FP cellHeight, sbyte[,] f, int lerpCount, bool combine)
        {
            List<GeomPoly> listOfElements;
            CxFastList<GeomPoly> list = new CxFastList<GeomPoly>();
            List<Vertices> list2 = new List<Vertices>();
            int num = (int) ((long) ((domain.Extents.x * 2) / cellWidth));
            bool flag = num == ((domain.Extents.x * 2) / cellWidth);
            int num2 = (int) ((long) ((domain.Extents.y * 2) / cellHeight));
            bool flag2 = num2 == ((domain.Extents.y * 2) / cellHeight);
            if (!flag)
            {
                num++;
            }
            if (!flag2)
            {
                num2++;
            }
            sbyte[,] fs = new sbyte[num + 1, num2 + 1];
            GeomPolyVal[,] valArray = new GeomPolyVal[num + 1, num2 + 1];
            for (int i = 0; i < (num + 1); i++)
            {
                int x;
                if (i == num)
                {
                    x = (int) ((long) domain.UpperBound.x);
                }
                else
                {
                    x = (i * cellWidth) + domain.LowerBound.x;
                }
                for (int m = 0; m < (num2 + 1); m++)
                {
                    int y;
                    if (m == num2)
                    {
                        y = (int) ((long) domain.UpperBound.y);
                    }
                    else
                    {
                        y = (m * cellHeight) + domain.LowerBound.y;
                    }
                    fs[i, m] = f[x, y];
                }
            }
            for (int j = 0; j < num2; j++)
            {
                FP fp2;
                FP fp = (j * cellHeight) + domain.LowerBound.y;
                if (j == (num2 - 1))
                {
                    fp2 = domain.UpperBound.y;
                }
                else
                {
                    fp2 = fp + cellHeight;
                }
                GeomPoly polya = null;
                for (int n = 0; n < num; n++)
                {
                    FP fp4;
                    FP fp3 = (n * cellWidth) + domain.LowerBound.x;
                    if (n == (num - 1))
                    {
                        fp4 = domain.UpperBound.x;
                    }
                    else
                    {
                        fp4 = fp3 + cellWidth;
                    }
                    GeomPoly poly = new GeomPoly();
                    int num9 = MarchSquare(f, fs, ref poly, n, j, fp3, fp, fp4, fp2, lerpCount);
                    if (poly.Length > 0)
                    {
                        if ((combine && (polya != null)) && ((num9 & 9) > 0))
                        {
                            combLeft(ref polya, ref poly);
                            poly = polya;
                        }
                        else
                        {
                            list.Add(poly);
                        }
                        valArray[n, j] = new GeomPolyVal(poly, num9);
                    }
                    else
                    {
                        poly = null;
                    }
                    polya = poly;
                }
            }
            if (!combine)
            {
                listOfElements = list.GetListOfElements();
                foreach (GeomPoly poly3 in listOfElements)
                {
                    list2.Add(new Vertices(poly3.Points.GetListOfElements()));
                }
                return list2;
            }
            for (int k = 1; k < num2; k++)
            {
                int num11 = 0;
                while (num11 < num)
                {
                    GeomPolyVal val = valArray[num11, k];
                    if (val == null)
                    {
                        num11++;
                        continue;
                    }
                    if ((val.Key & 12) == 0)
                    {
                        num11++;
                        continue;
                    }
                    GeomPolyVal val2 = valArray[num11, k - 1];
                    if (val2 == null)
                    {
                        num11++;
                        continue;
                    }
                    if ((val2.Key & 3) == 0)
                    {
                        num11++;
                        continue;
                    }
                    FP fp5 = (num11 * cellWidth) + domain.LowerBound.x;
                    FP fp6 = (k * cellHeight) + domain.LowerBound.y;
                    CxFastList<TSVector2> points = val.GeomP.Points;
                    CxFastList<TSVector2> list6 = val2.GeomP.Points;
                    if (val2.GeomP == val.GeomP)
                    {
                        num11++;
                        continue;
                    }
                    CxFastListNode<TSVector2> node = points.Begin();
                    while ((Square(node.Elem().y - fp6) > Settings.Epsilon) || (node.Elem().x < fp5))
                    {
                        node = node.Next();
                    }
                    TSVector2 b = node.Next().Elem();
                    if (Square(b.y - fp6) > Settings.Epsilon)
                    {
                        num11++;
                        continue;
                    }
                    bool flag16 = true;
                    CxFastListNode<TSVector2> node2 = list6.Begin();
                    while (node2 != list6.End())
                    {
                        if (VecDsq(node2.Elem(), b) < Settings.Epsilon)
                        {
                            flag16 = false;
                            break;
                        }
                        node2 = node2.Next();
                    }
                    if (flag16)
                    {
                        num11++;
                    }
                    else
                    {
                        CxFastListNode<TSVector2> node3 = node.Next().Next();
                        if (node3 == points.End())
                        {
                            node3 = points.Begin();
                        }
                        while (node3 != node)
                        {
                            node2 = list6.Insert(node2, node3.Elem());
                            node3 = node3.Next();
                            if (node3 == points.End())
                            {
                                node3 = points.Begin();
                            }
                            val2.GeomP.Length++;
                        }
                        fp5 = num11 + 1;
                        while (fp5 < num)
                        {
                            GeomPolyVal val3 = valArray[(int) ((long) fp5), k];
                            if ((val3 == null) || (val3.GeomP != val.GeomP))
                            {
                                fp5 += 1;
                            }
                            else
                            {
                                val3.GeomP = val2.GeomP;
                                fp5 += 1;
                            }
                        }
                        fp5 = num11 - 1;
                        while (fp5 >= 0)
                        {
                            GeomPolyVal val4 = valArray[(int) ((long) fp5), k];
                            if ((val4 == null) || (val4.GeomP != val.GeomP))
                            {
                                fp5 -= 1;
                            }
                            else
                            {
                                val4.GeomP = val2.GeomP;
                                fp5 -= 1;
                            }
                        }
                        list.Remove(val.GeomP);
                        val.GeomP = val2.GeomP;
                        num11 = ((int) ((long) ((node.Next().Elem().x - domain.LowerBound.x) / cellWidth))) + 1;
                    }
                }
            }
            listOfElements = list.GetListOfElements();
            foreach (GeomPoly poly4 in listOfElements)
            {
                list2.Add(new Vertices(poly4.Points.GetListOfElements()));
            }
            return list2;
        }

        private static FP Lerp(FP x0, FP x1, FP v0, FP v1)
        {
            FP fp2;
            FP fp = v0 - v1;
            if ((fp * fp) < Settings.Epsilon)
            {
                fp2 = 0.5f;
            }
            else
            {
                fp2 = v0 / fp;
            }
            return (x0 + (fp2 * (x1 - x0)));
        }

        private static int MarchSquare(sbyte[,] f, sbyte[,] fs, ref GeomPoly poly, int ax, int ay, FP x0, FP y0, FP x1, FP y1, int bin)
        {
            int index = 0;
            sbyte num2 = fs[ax, ay];
            if (num2 < 0)
            {
                index |= 8;
            }
            sbyte num3 = fs[ax + 1, ay];
            if (num3 < 0)
            {
                index |= 4;
            }
            sbyte num4 = fs[ax + 1, ay + 1];
            if (num4 < 0)
            {
                index |= 2;
            }
            sbyte num5 = fs[ax, ay + 1];
            if (num5 < 0)
            {
                index |= 1;
            }
            int num6 = _lookMarch[index];
            if (num6 > 0)
            {
                CxFastListNode<TSVector2> node = null;
                for (int i = 0; i < 8; i++)
                {
                    if ((num6 & (((int) 1) << i)) > 0)
                    {
                        TSVector2 vector;
                        if ((i == 7) && ((num6 & 1) == 0))
                        {
                            vector = new TSVector2(x0, Ylerp(y0, y1, x0, (FP) num2, (FP) num5, f, bin));
                            poly.Points.Add(vector);
                        }
                        else
                        {
                            if (i == 0)
                            {
                                vector = new TSVector2(x0, y0);
                            }
                            else if (i == 2)
                            {
                                vector = new TSVector2(x1, y0);
                            }
                            else if (i == 4)
                            {
                                vector = new TSVector2(x1, y1);
                            }
                            else if (i == 6)
                            {
                                vector = new TSVector2(x0, y1);
                            }
                            else if (i == 1)
                            {
                                vector = new TSVector2(Xlerp(x0, x1, y0, (FP) num2, (FP) num3, f, bin), y0);
                            }
                            else if (i == 5)
                            {
                                vector = new TSVector2(Xlerp(x0, x1, y1, (FP) num5, (FP) num4, f, bin), y1);
                            }
                            else if (i == 3)
                            {
                                vector = new TSVector2(x1, Ylerp(y0, y1, x1, (FP) num3, (FP) num4, f, bin));
                            }
                            else
                            {
                                vector = new TSVector2(x0, Ylerp(y0, y1, x0, (FP) num2, (FP) num5, f, bin));
                            }
                            node = poly.Points.Insert(node, vector);
                        }
                        poly.Length++;
                    }
                }
            }
            return index;
        }

        private static FP Square(FP x)
        {
            return (x * x);
        }

        private static FP VecCross(TSVector2 a, TSVector2 b)
        {
            return ((a.x * b.y) - (a.y * b.x));
        }

        private static FP VecDsq(TSVector2 a, TSVector2 b)
        {
            TSVector2 vector = a - b;
            return ((vector.x * vector.x) + (vector.y * vector.y));
        }

        private static FP Xlerp(FP x0, FP x1, FP y, FP v0, FP v1, sbyte[,] f, int c)
        {
            FP fp = Lerp(x0, x1, v0, v1);
            if (c == 0)
            {
                return fp;
            }
            sbyte num = f[(int) ((long) fp), (int) ((long) y)];
            if ((v0 * num) < 0)
            {
                return Xlerp(x0, fp, y, v0, (FP) num, f, c - 1);
            }
            return Xlerp(fp, x1, y, (FP) num, v1, f, c - 1);
        }

        private static FP Ylerp(FP y0, FP y1, FP x, FP v0, FP v1, sbyte[,] f, int c)
        {
            FP fp = Lerp(y0, y1, v0, v1);
            if (c == 0)
            {
                return fp;
            }
            sbyte num = f[(int) ((long) x), (int) ((long) fp)];
            if ((v0 * num) < 0)
            {
                return Ylerp(y0, fp, x, v0, (FP) num, f, c - 1);
            }
            return Ylerp(fp, y1, x, (FP) num, v1, f, c - 1);
        }

        internal class CxFastList<T>
        {
            private int _count;
            private MarchingSquares.CxFastListNode<T> _head;

            public MarchingSquares.CxFastListNode<T> Add(T value)
            {
                MarchingSquares.CxFastListNode<T> node = new MarchingSquares.CxFastListNode<T>(value);
                if (this._head == null)
                {
                    node._next = null;
                    this._head = node;
                    this._count++;
                    return node;
                }
                node._next = this._head;
                this._head = node;
                this._count++;
                return node;
            }

            public MarchingSquares.CxFastListNode<T> Begin()
            {
                return this._head;
            }

            public void Clear()
            {
                MarchingSquares.CxFastListNode<T> node = this._head;
                while (node > null)
                {
                    MarchingSquares.CxFastListNode<T> node2 = node;
                    node = node._next;
                    node2._next = null;
                }
                this._head = null;
                this._count = 0;
            }

            public bool Empty()
            {
                return (this._head == null);
            }

            public MarchingSquares.CxFastListNode<T> End()
            {
                return null;
            }

            public MarchingSquares.CxFastListNode<T> Erase(MarchingSquares.CxFastListNode<T> prev, MarchingSquares.CxFastListNode<T> node)
            {
                MarchingSquares.CxFastListNode<T> node2 = node._next;
                if (prev > null)
                {
                    prev._next = node2;
                }
                else if (this._head > null)
                {
                    this._head = this._head._next;
                }
                else
                {
                    return null;
                }
                this._count--;
                return node2;
            }

            public MarchingSquares.CxFastListNode<T> Find(T value)
            {
                MarchingSquares.CxFastListNode<T> node = this._head;
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                if (node > null)
                {
                    if (value > null)
                    {
                        do
                        {
                            if (comparer.Equals(node._elt, value))
                            {
                                return node;
                            }
                            node = node._next;
                        }
                        while (node != this._head);
                    }
                    else
                    {
                        do
                        {
                            if (node._elt == null)
                            {
                                return node;
                            }
                            node = node._next;
                        }
                        while (node != this._head);
                    }
                }
                return null;
            }

            public T Front()
            {
                return this._head.Elem();
            }

            public List<T> GetListOfElements()
            {
                List<T> list = new List<T>();
                for (MarchingSquares.CxFastListNode<T> node = this.Begin(); node > null; node = node._next)
                {
                    list.Add(node._elt);
                }
                return list;
            }

            public bool Has(T value)
            {
                return (this.Find(value) > null);
            }

            public MarchingSquares.CxFastListNode<T> Insert(MarchingSquares.CxFastListNode<T> node, T value)
            {
                if (node == null)
                {
                    return this.Add(value);
                }
                MarchingSquares.CxFastListNode<T> node2 = new MarchingSquares.CxFastListNode<T>(value);
                node2._next = node._next;
                node._next = node2;
                this._count++;
                return node2;
            }

            public MarchingSquares.CxFastListNode<T> Pop()
            {
                return this.Erase(null, this._head);
            }

            public bool Remove(T value)
            {
                MarchingSquares.CxFastListNode<T> node = this._head;
                MarchingSquares.CxFastListNode<T> node2 = this._head;
                EqualityComparer<T> comparer = EqualityComparer<T>.Default;
                if ((node > null) && (value > null))
                {
                    do
                    {
                        if (comparer.Equals(node._elt, value))
                        {
                            if (node == this._head)
                            {
                                this._head = node._next;
                                this._count--;
                                return true;
                            }
                            node2._next = node._next;
                            this._count--;
                            return true;
                        }
                        node2 = node;
                        node = node._next;
                    }
                    while (node > null);
                }
                return false;
            }

            public int Size()
            {
                MarchingSquares.CxFastListNode<T> node = this.Begin();
                int num = 0;
                do
                {
                    num++;
                }
                while (node.Next() > null);
                return num;
            }
        }

        internal class CxFastListNode<T>
        {
            internal T _elt;
            internal MarchingSquares.CxFastListNode<T> _next;

            public CxFastListNode(T obj)
            {
                this._elt = obj;
            }

            public T Elem()
            {
                return this._elt;
            }

            public MarchingSquares.CxFastListNode<T> Next()
            {
                return this._next;
            }
        }

        internal class GeomPoly
        {
            public int Length = 0;
            public MarchingSquares.CxFastList<TSVector2> Points = new MarchingSquares.CxFastList<TSVector2>();
        }

        private class GeomPolyVal
        {
            public MarchingSquares.GeomPoly GeomP;
            public int Key;

            public GeomPolyVal(MarchingSquares.GeomPoly geomP, int K)
            {
                this.GeomP = geomP;
                this.Key = K;
            }
        }
    }
}

