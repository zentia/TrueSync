namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public sealed class TextureConverter
    {
        private uint _alphaTolerance;
        private static int[,] _closePixels = new int[,] { { -1, -1 }, { 0, -1 }, { 1, -1 }, { 1, 0 }, { 1, 1 }, { 0, 1 }, { -1, 1 }, { -1, 0 } };
        private uint[] _data;
        private int _dataLength;
        private int _height;
        private bool _holeDetection;
        private FP _hullTolerance;
        private bool _multipartDetection;
        private bool _pixelOffsetOptimization;
        private VerticesDetectionType _polygonDetectionType;
        private int _tempIsSolidX;
        private int _tempIsSolidY;
        private Matrix _transform;
        private int _width;
        private const int ClosepixelsLength = 8;

        public TextureConverter()
        {
            this._transform = Matrix.Identity;
            bool? holeDetection = null;
            holeDetection = null;
            this.Initialize(null, null, null, null, holeDetection, holeDetection, null, null);
        }

        public TextureConverter(uint[] data, int width)
        {
            this._transform = Matrix.Identity;
            bool? holeDetection = null;
            holeDetection = null;
            this.Initialize(data, new int?(width), null, null, holeDetection, holeDetection, null, null);
        }

        public TextureConverter(byte? alphaTolerance, FP? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, Matrix? transform)
        {
            this._transform = Matrix.Identity;
            this.Initialize(null, null, alphaTolerance, hullTolerance, holeDetection, multipartDetection, pixelOffsetOptimization, transform);
        }

        public TextureConverter(uint[] data, int width, byte? alphaTolerance, FP? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, Matrix? transform)
        {
            this._transform = Matrix.Identity;
            this.Initialize(data, new int?(width), alphaTolerance, hullTolerance, holeDetection, multipartDetection, pixelOffsetOptimization, transform);
        }

        private void ApplyTransform(ref List<Vertices> detectedPolygons)
        {
            for (int i = 0; i < detectedPolygons.Count; i++)
            {
                detectedPolygons[i].Transform(ref this._transform);
            }
        }

        private void ApplyTriangulationCompatibleWinding(ref List<Vertices> detectedPolygons)
        {
            for (int i = 0; i < detectedPolygons.Count; i++)
            {
                detectedPolygons[i].Reverse();
                if ((detectedPolygons[i].Holes != null) && (detectedPolygons[i].Holes.Count > 0))
                {
                    for (int j = 0; j < detectedPolygons[i].Holes.Count; j++)
                    {
                        detectedPolygons[i].Holes[j].Reverse();
                    }
                }
            }
        }

        private Vertices CreateSimplePolygon(TSVector2 entrance, TSVector2 last)
        {
            bool flag = false;
            bool flag2 = false;
            Vertices vertices = new Vertices(0x20);
            Vertices hullArea = new Vertices(0x20);
            Vertices vertices3 = new Vertices(0x20);
            TSVector2 zero = TSVector2.zero;
            if ((entrance == TSVector2.zero) || !this.InBounds(ref entrance))
            {
                flag = this.SearchHullEntrance(out entrance);
                if (flag)
                {
                    zero = new TSVector2(entrance.x - 1f, entrance.y);
                }
            }
            else if (this.IsSolid(ref entrance))
            {
                if (this.IsNearPixel(ref entrance, ref last))
                {
                    zero = last;
                    flag = true;
                }
                else
                {
                    TSVector2 vector2;
                    if (this.SearchNearPixels(false, ref entrance, out vector2))
                    {
                        zero = vector2;
                        flag = true;
                    }
                    else
                    {
                        flag = false;
                    }
                }
            }
            if (!flag)
            {
                return vertices;
            }
            vertices.Add(entrance);
            hullArea.Add(entrance);
            TSVector2 next = entrance;
            while (true)
            {
                TSVector2 vector4;
                if (this.SearchForOutstandingVertex(hullArea, out vector4))
                {
                    if (flag2)
                    {
                        if (vertices3.Contains(vector4))
                        {
                            vertices.Add(vector4);
                        }
                        return vertices;
                    }
                    vertices.Add(vector4);
                    hullArea.RemoveRange(0, hullArea.IndexOf(vector4));
                }
                last = zero;
                zero = next;
                if (this.GetNextHullPoint(ref last, ref zero, out next))
                {
                    hullArea.Add(next);
                }
                else
                {
                    return vertices;
                }
                if ((next == entrance) && !flag2)
                {
                    flag2 = true;
                    vertices3.AddRange(hullArea);
                    if (vertices3.Contains(entrance))
                    {
                        vertices3.Remove(entrance);
                    }
                }
            }
        }

        public List<Vertices> DetectVertices()
        {
            Vertices vertices;
            bool flag16;
            if (this._data == null)
            {
                throw new Exception("'_data' can't be null. You have to use SetTextureData(uint[] data, int width) before calling this method.");
            }
            if (this._data.Length < 4)
            {
                throw new Exception("'_data' length can't be less then 4. Your texture must be at least 2 x 2 pixels in size. You have to use SetTextureData(uint[] data, int width) before calling this method.");
            }
            if (this._width < 2)
            {
                throw new Exception("'_width' can't be less then 2. Your texture must be at least 2 x 2 pixels in size. You have to use SetTextureData(uint[] data, int width) before calling this method.");
            }
            if ((this._data.Length % this._width) > 0)
            {
                throw new Exception("'_width' has an invalid value. You have to use SetTextureData(uint[] data, int width) before calling this method.");
            }
            List<Vertices> detectedPolygons = new List<Vertices>();
            TSVector2? lastHoleEntrance = null;
            TSVector2? entrance = null;
            List<TSVector2> list2 = new List<TSVector2>();
        Label_0090:
            if (detectedPolygons.Count == 0)
            {
                vertices = new Vertices(this.CreateSimplePolygon(TSVector2.zero, TSVector2.zero));
                if (vertices.Count > 2)
                {
                    entrance = this.GetTopMostVertex(vertices);
                }
            }
            else
            {
                if (!entrance.HasValue)
                {
                    goto Label_02C2;
                }
                vertices = new Vertices(this.CreateSimplePolygon(entrance.Value, new TSVector2(entrance.Value.x - 1f, entrance.Value.y)));
            }
            bool flag = false;
            if (vertices.Count <= 2)
            {
                goto Label_027D;
            }
            if (!this._holeDetection)
            {
                goto Label_0273;
            }
        Label_0151:
            lastHoleEntrance = this.SearchHoleEntrance(vertices, lastHoleEntrance);
            if (!lastHoleEntrance.HasValue || list2.Contains(lastHoleEntrance.Value))
            {
                goto Label_0273;
            }
            list2.Add(lastHoleEntrance.Value);
            Vertices collection = this.CreateSimplePolygon(lastHoleEntrance.Value, new TSVector2(lastHoleEntrance.Value.x + 1, lastHoleEntrance.Value.y));
            if ((collection != null) && (collection.Count > 2))
            {
                switch (this._polygonDetectionType)
                {
                    case VerticesDetectionType.Integrated:
                        int num;
                        int num2;
                        collection.Add(collection[0]);
                        if (this.SplitPolygonEdge(vertices, lastHoleEntrance.Value, out num, out num2))
                        {
                            vertices.InsertRange(num2, collection);
                        }
                        goto Label_0230;

                    case VerticesDetectionType.Separated:
                        goto Label_0230;
                }
            }
            goto Label_0269;
        Label_0230:
            if (vertices.Holes == null)
            {
                vertices.Holes = new List<Vertices>();
            }
            vertices.Holes.Add(collection);
        Label_0269:
            flag16 = true;
            goto Label_0151;
        Label_0273:
            detectedPolygons.Add(vertices);
        Label_027D:
            if ((this._multipartDetection || (vertices.Count <= 2)) && this.SearchNextHullEntrance(detectedPolygons, entrance.Value, out entrance))
            {
                flag = true;
            }
            if (flag)
            {
                goto Label_0090;
            }
        Label_02C2:
            if ((detectedPolygons == null) || ((detectedPolygons != null) && (detectedPolygons.Count == 0)))
            {
                throw new Exception("Couldn't detect any vertices.");
            }
            if (this.PolygonDetectionType == VerticesDetectionType.Separated)
            {
                this.ApplyTriangulationCompatibleWinding(ref detectedPolygons);
            }
            if (this._transform != Matrix.Identity)
            {
                this.ApplyTransform(ref detectedPolygons);
            }
            return detectedPolygons;
        }

        public static Vertices DetectVertices(uint[] data, int width)
        {
            TextureConverter converter = new TextureConverter(data, width);
            return converter.DetectVertices()[0];
        }

        public static Vertices DetectVertices(uint[] data, int width, bool holeDetection)
        {
            TextureConverter converter = new TextureConverter(data, width) {
                HoleDetection = holeDetection
            };
            return converter.DetectVertices()[0];
        }

        public static List<Vertices> DetectVertices(uint[] data, int width, FP hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection)
        {
            TextureConverter converter1 = new TextureConverter(data, width) {
                HullTolerance = hullTolerance,
                AlphaTolerance = alphaTolerance,
                MultipartDetection = multiPartDetection,
                HoleDetection = holeDetection
            };
            List<Vertices> list = converter1.DetectVertices();
            List<Vertices> list2 = new List<Vertices>();
            for (int i = 0; i < list.Count; i++)
            {
                list2.Add(list[i]);
            }
            return list2;
        }

        private bool DistanceToHullAcceptable(Vertices polygon, TSVector2 point, bool higherDetail)
        {
            TSVector2 vector2;
            if (polygon == null)
            {
                throw new ArgumentNullException("polygon", "'polygon' can't be null.");
            }
            if (polygon.Count < 3)
            {
                throw new ArgumentException("'polygon.Count' can't be less then 3.");
            }
            TSVector2 end = polygon[polygon.Count - 1];
            if (higherDetail)
            {
                for (int j = 0; j < polygon.Count; j++)
                {
                    vector2 = polygon[j];
                    if ((LineTools.DistanceBetweenPointAndLineSegment(ref point, ref vector2, ref end) <= this._hullTolerance) || (TSVector2.Distance(point, vector2) <= this._hullTolerance))
                    {
                        return false;
                    }
                    end = polygon[j];
                }
                return true;
            }
            for (int i = 0; i < polygon.Count; i++)
            {
                vector2 = polygon[i];
                if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref vector2, ref end) <= this._hullTolerance)
                {
                    return false;
                }
                end = polygon[i];
            }
            return true;
        }

        private bool DistanceToHullAcceptableHoles(Vertices polygon, TSVector2 point, bool higherDetail)
        {
            if (polygon == null)
            {
                throw new ArgumentNullException("polygon", "'polygon' can't be null.");
            }
            if (polygon.Count < 3)
            {
                throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
            }
            if (this.DistanceToHullAcceptable(polygon, point, higherDetail))
            {
                if (polygon.Holes > null)
                {
                    for (int i = 0; i < polygon.Holes.Count; i++)
                    {
                        if (!this.DistanceToHullAcceptable(polygon.Holes[i], point, higherDetail))
                        {
                            return false;
                        }
                    }
                }
                return true;
            }
            return false;
        }

        private FP GetBottomMostCoord(Vertices vertices)
        {
            FP minValue = FP.MinValue;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (minValue < vertices[i].y)
                {
                    minValue = vertices[i].y;
                }
            }
            return minValue;
        }

        private int GetIndexOfFirstPixelToCheck(ref TSVector2 last, ref TSVector2 current)
        {
            switch (((int) ((long) (current.x - last.x))))
            {
                case -1:
                    switch (((int) ((long) (current.y - last.y))))
                    {
                        case -1:
                            return 5;

                        case 0:
                            return 4;

                        case 1:
                            return 3;
                    }
                    break;

                case 0:
                {
                    int num4 = (int) ((long) (current.y - last.y));
                    if (num4 == -1)
                    {
                        return 6;
                    }
                    if (num4 != 1)
                    {
                        break;
                    }
                    return 2;
                }
                case 1:
                    switch (((int) ((long) (current.y - last.y))))
                    {
                        case -1:
                            return 7;

                        case 0:
                            return 0;

                        case 1:
                            return 1;
                    }
                    break;
            }
            return 0;
        }

        private bool GetNextHullPoint(ref TSVector2 last, ref TSVector2 current, out TSVector2 next)
        {
            int indexOfFirstPixelToCheck = this.GetIndexOfFirstPixelToCheck(ref last, ref current);
            for (int i = 0; i < 8; i++)
            {
                int num4 = (indexOfFirstPixelToCheck + i) % 8;
                int x = ((int) ((long) current.x)) + _closePixels[num4, 0];
                int y = ((int) ((long) current.y)) + _closePixels[num4, 1];
                if (((((x >= 0) && (x < this._width)) && (y >= 0)) && (y <= this._height)) && this.IsSolid(ref x, ref y))
                {
                    next = new TSVector2(x, y);
                    return true;
                }
            }
            next = TSVector2.zero;
            return false;
        }

        private FP GetTopMostCoord(Vertices vertices)
        {
            FP maxValue = FP.MaxValue;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (maxValue > vertices[i].y)
                {
                    maxValue = vertices[i].y;
                }
            }
            return maxValue;
        }

        private TSVector2? GetTopMostVertex(Vertices vertices)
        {
            FP maxValue = FP.MaxValue;
            TSVector2? nullable = null;
            for (int i = 0; i < vertices.Count; i++)
            {
                if (maxValue > vertices[i].y)
                {
                    maxValue = vertices[i].y;
                    nullable = new TSVector2?(vertices[i]);
                }
            }
            return nullable;
        }

        public bool InBounds(ref TSVector2 coord)
        {
            return ((((coord.x >= 0f) && (coord.x < this._width)) && (coord.y >= 0f)) && (coord.y < this._height));
        }

        private void Initialize(uint[] data, int? width, byte? alphaTolerance, FP? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, Matrix? transform)
        {
            if ((data != null) && !width.HasValue)
            {
                throw new ArgumentNullException("width", "'width' can't be null if 'data' is set.");
            }
            if ((data == null) && width.HasValue)
            {
                throw new ArgumentNullException("data", "'data' can't be null if 'width' is set.");
            }
            if ((data != null) && width.HasValue)
            {
                this.SetTextureData(data, width.Value);
            }
            if (alphaTolerance.HasValue)
            {
                this.AlphaTolerance = alphaTolerance.Value;
            }
            else
            {
                this.AlphaTolerance = 20;
            }
            if (hullTolerance.HasValue)
            {
                this.HullTolerance = hullTolerance.Value;
            }
            else
            {
                this.HullTolerance = 1.5f;
            }
            if (holeDetection.HasValue)
            {
                this.HoleDetection = holeDetection.Value;
            }
            else
            {
                this.HoleDetection = false;
            }
            if (multipartDetection.HasValue)
            {
                this.MultipartDetection = multipartDetection.Value;
            }
            else
            {
                this.MultipartDetection = false;
            }
            if (pixelOffsetOptimization.HasValue)
            {
                this.PixelOffsetOptimization = pixelOffsetOptimization.Value;
            }
            else
            {
                this.PixelOffsetOptimization = false;
            }
            if (transform.HasValue)
            {
                this.Transform = transform.Value;
            }
            else
            {
                this.Transform = Matrix.Identity;
            }
        }

        private bool InPolygon(Vertices polygon, TSVector2 point)
        {
            if (this.DistanceToHullAcceptableHoles(polygon, point, true))
            {
                List<FP> list = this.SearchCrossingEdgesHoles(polygon, (int) ((long) point.y));
                if ((list.Count > 0) && ((list.Count % 2) == 0))
                {
                    for (int i = 0; i < list.Count; i += 2)
                    {
                        if ((list[i] <= point.x) && (list[i + 1] >= point.x))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
            return true;
        }

        private bool IsNearPixel(ref TSVector2 current, ref TSVector2 near)
        {
            for (int i = 0; i < 8; i++)
            {
                int num2 = ((int) ((long) current.x)) + _closePixels[i, 0];
                int num3 = ((int) ((long) current.y)) + _closePixels[i, 1];
                if (((((num2 >= 0) && (num2 <= this._width)) && (num3 >= 0)) && (num3 <= this._height)) && ((num2 == ((int) ((long) near.x))) && (num3 == ((int) ((long) near.y)))))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsSolid(ref int index)
        {
            return (((index >= 0) && (index < this._dataLength)) && (this._data[index] >= this._alphaTolerance));
        }

        public bool IsSolid(ref TSVector2 v)
        {
            this._tempIsSolidX = (int) ((long) v.x);
            this._tempIsSolidY = (int) ((long) v.y);
            return (((((this._tempIsSolidX >= 0) && (this._tempIsSolidX < this._width)) && (this._tempIsSolidY >= 0)) && (this._tempIsSolidY < this._height)) && (this._data[this._tempIsSolidX + (this._tempIsSolidY * this._width)] >= this._alphaTolerance));
        }

        public bool IsSolid(ref int x, ref int y)
        {
            return (((((x >= 0) && (x < this._width)) && (y >= 0)) && (y < this._height)) && (this._data[x + (y * this._width)] >= this._alphaTolerance));
        }

        private List<FP> SearchCrossingEdges(Vertices polygon, int y)
        {
            List<FP> list = new List<FP>();
            if (polygon.Count > 2)
            {
                TSVector2 vector3 = polygon[polygon.Count - 1];
                for (int i = 0; i < polygon.Count; i++)
                {
                    TSVector2 vector2 = polygon[i];
                    if ((((vector2.y >= y) && (vector3.y <= y)) || ((vector2.y <= y) && (vector3.y >= y))) && (vector2.y != vector3.y))
                    {
                        bool flag = true;
                        TSVector2 vector = vector3 - vector2;
                        if (vector2.y == y)
                        {
                            TSVector2 vector5 = polygon[(i + 1) % polygon.Count];
                            TSVector2 vector4 = vector2 - vector5;
                            if (vector.y > 0)
                            {
                                flag = vector4.y <= 0;
                            }
                            else
                            {
                                flag = vector4.y >= 0;
                            }
                        }
                        if (flag)
                        {
                            list.Add((((y - vector2.y) / vector.y) * vector.x) + vector2.x);
                        }
                    }
                    vector3 = vector2;
                }
            }
            list.Sort();
            return list;
        }

        private List<FP> SearchCrossingEdgesHoles(Vertices polygon, int y)
        {
            if (polygon == null)
            {
                throw new ArgumentNullException("polygon", "'polygon' can't be null.");
            }
            if (polygon.Count < 3)
            {
                throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
            }
            List<FP> list = this.SearchCrossingEdges(polygon, y);
            if (polygon.Holes > null)
            {
                for (int i = 0; i < polygon.Holes.Count; i++)
                {
                    list.AddRange(this.SearchCrossingEdges(polygon.Holes[i], y));
                }
            }
            list.Sort();
            return list;
        }

        private bool SearchForOutstandingVertex(Vertices hullArea, out TSVector2 outstanding)
        {
            TSVector2 zero = TSVector2.zero;
            bool flag = false;
            if (hullArea.Count > 2)
            {
                int num = hullArea.Count - 1;
                TSVector2 start = hullArea[0];
                TSVector2 end = hullArea[num];
                for (int i = 1; i < num; i++)
                {
                    TSVector2 point = hullArea[i];
                    if (LineTools.DistanceBetweenPointAndLineSegment(ref point, ref start, ref end) >= this._hullTolerance)
                    {
                        zero = hullArea[i];
                        flag = true;
                        break;
                    }
                }
            }
            outstanding = zero;
            return flag;
        }

        private TSVector2? SearchHoleEntrance(Vertices polygon, TSVector2? lastHoleEntrance)
        {
            int y;
            if (polygon == null)
            {
                throw new ArgumentNullException("'polygon' can't be null.");
            }
            if (polygon.Count < 3)
            {
                throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
            }
            int x = 0;
            if (lastHoleEntrance.HasValue)
            {
                y = (int) ((long) lastHoleEntrance.Value.y);
            }
            else
            {
                y = (int) ((long) this.GetTopMostCoord(polygon));
            }
            int bottomMostCoord = (int) ((long) this.GetBottomMostCoord(polygon));
            if ((((y > 0) && (y < this._height)) && (bottomMostCoord > 0)) && (bottomMostCoord < this._height))
            {
                for (int i = y; i <= bottomMostCoord; i++)
                {
                    List<FP> list = this.SearchCrossingEdges(polygon, i);
                    if ((list.Count > 1) && ((list.Count % 2) == 0))
                    {
                        for (int j = 0; j < list.Count; j += 2)
                        {
                            bool flag = false;
                            bool flag2 = false;
                            for (int k = (int) ((long) list[j]); k <= ((int) ((long) list[j + 1])); k++)
                            {
                                if (this.IsSolid(ref k, ref i))
                                {
                                    if (!flag2)
                                    {
                                        flag = true;
                                        x = k;
                                    }
                                    if (!(flag & flag2))
                                    {
                                        continue;
                                    }
                                    TSVector2? nullable = new TSVector2(x, i);
                                    if (this.DistanceToHullAcceptable(polygon, nullable.Value, true))
                                    {
                                        return nullable;
                                    }
                                    nullable = null;
                                    break;
                                }
                                if (flag)
                                {
                                    flag2 = true;
                                }
                            }
                        }
                    }
                    else if ((list.Count % 2) == 0)
                    {
                        Debug.WriteLine("SearchCrossingEdges() % 2 != 0");
                    }
                }
            }
            return null;
        }

        private bool SearchHullEntrance(out TSVector2 entrance)
        {
            for (int i = 0; i <= this._height; i++)
            {
                for (int j = 0; j <= this._width; j++)
                {
                    if (this.IsSolid(ref j, ref i))
                    {
                        entrance = new TSVector2(j, i);
                        return true;
                    }
                }
            }
            entrance = TSVector2.zero;
            return false;
        }

        private bool SearchNearPixels(bool searchingForSolidPixel, ref TSVector2 current, out TSVector2 foundPixel)
        {
            for (int i = 0; i < 8; i++)
            {
                int x = ((int) ((long) current.x)) + _closePixels[i, 0];
                int y = ((int) ((long) current.y)) + _closePixels[i, 1];
                if (!searchingForSolidPixel ^ this.IsSolid(ref x, ref y))
                {
                    foundPixel = new TSVector2(x, y);
                    return true;
                }
            }
            foundPixel = TSVector2.zero;
            return false;
        }

        private bool SearchNextHullEntrance(List<Vertices> detectedPolygons, TSVector2 start, out TSVector2? entrance)
        {
            bool flag = false;
            bool flag2 = false;
            for (int i = ((int) ((long) start.x)) + (((int) ((long) start.y)) * this._width); i <= this._dataLength; i++)
            {
                if (this.IsSolid(ref i))
                {
                    if (!flag)
                    {
                        continue;
                    }
                    int x = i % this._width;
                    entrance = new TSVector2(x, (i - x) / this._width);
                    flag2 = false;
                    for (int j = 0; j < detectedPolygons.Count; j++)
                    {
                        if (this.InPolygon(detectedPolygons[j], entrance.Value))
                        {
                            flag2 = true;
                            break;
                        }
                    }
                    if (flag2)
                    {
                        flag = false;
                        continue;
                    }
                    return true;
                }
                flag = true;
            }
            entrance = 0;
            return false;
        }

        private void SetTextureData(uint[] data, int width)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data", "'data' can't be null.");
            }
            if (data.Length < 4)
            {
                throw new ArgumentOutOfRangeException("data", "'data' length can't be less then 4. Your texture must be at least 2 x 2 pixels in size.");
            }
            if (width < 2)
            {
                throw new ArgumentOutOfRangeException("width", "'width' can't be less then 2. Your texture must be at least 2 x 2 pixels in size.");
            }
            if ((data.Length % width) > 0)
            {
                throw new ArgumentException("'width' has an invalid value.");
            }
            this._data = data;
            this._dataLength = this._data.Length;
            this._width = width;
            this._height = this._dataLength / width;
        }

        private bool SplitPolygonEdge(Vertices polygon, TSVector2 coordInsideThePolygon, out int vertex1Index, out int vertex2Index)
        {
            int index = 0;
            int num2 = 0;
            bool flag = false;
            FP maxValue = FP.MaxValue;
            bool flag2 = false;
            TSVector2 zero = TSVector2.zero;
            List<FP> list = this.SearchCrossingEdges(polygon, (int) ((long) coordInsideThePolygon.y));
            vertex1Index = 0;
            vertex2Index = 0;
            zero.y = coordInsideThePolygon.y;
            if (((list != null) && (list.Count > 1)) && ((list.Count % 2) == 0))
            {
                FP fp2;
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] < coordInsideThePolygon.x)
                    {
                        fp2 = coordInsideThePolygon.x - list[i];
                        if (fp2 < maxValue)
                        {
                            maxValue = fp2;
                            zero.x = list[i];
                            flag2 = true;
                        }
                    }
                }
                if (flag2)
                {
                    maxValue = FP.MaxValue;
                    int num4 = polygon.Count - 1;
                    for (int j = 0; j < polygon.Count; j++)
                    {
                        TSVector2 start = polygon[j];
                        TSVector2 end = polygon[num4];
                        fp2 = LineTools.DistanceBetweenPointAndLineSegment(ref zero, ref start, ref end);
                        if (fp2 < maxValue)
                        {
                            maxValue = fp2;
                            index = j;
                            num2 = num4;
                            flag = true;
                        }
                        num4 = j;
                    }
                    if (flag)
                    {
                        TSVector2 vector = polygon[num2] - polygon[index];
                        vector.Normalize();
                        TSVector2 vector5 = polygon[index];
                        fp2 = TSVector2.Distance(vector5, zero);
                        vertex1Index = index;
                        vertex2Index = index + 1;
                        polygon.Insert(index, ((TSVector2) (fp2 * vector)) + polygon[vertex1Index]);
                        polygon.Insert(index, ((TSVector2) (fp2 * vector)) + polygon[vertex2Index]);
                        return true;
                    }
                }
            }
            return false;
        }

        public byte AlphaTolerance
        {
            get
            {
                return (byte) (this._alphaTolerance >> 0x18);
            }
            set
            {
                this._alphaTolerance = (uint) (value << 0x18);
            }
        }

        public bool HoleDetection
        {
            get
            {
                return this._holeDetection;
            }
            set
            {
                this._holeDetection = value;
            }
        }

        public FP HullTolerance
        {
            get
            {
                return this._hullTolerance;
            }
            set
            {
                if (value > 4f)
                {
                    this._hullTolerance = 4f;
                }
                else if (value < 0.9f)
                {
                    this._hullTolerance = 0.9f;
                }
                else
                {
                    this._hullTolerance = value;
                }
            }
        }

        public bool MultipartDetection
        {
            get
            {
                return this._multipartDetection;
            }
            set
            {
                this._multipartDetection = value;
            }
        }

        public bool PixelOffsetOptimization
        {
            get
            {
                return this._pixelOffsetOptimization;
            }
            set
            {
                this._pixelOffsetOptimization = value;
            }
        }

        public VerticesDetectionType PolygonDetectionType
        {
            get
            {
                return this._polygonDetectionType;
            }
            set
            {
                this._polygonDetectionType = value;
            }
        }

        public Matrix Transform
        {
            get
            {
                return this._transform;
            }
            set
            {
                this._transform = value;
            }
        }
    }
}

