using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TrueSync.Physics2D
{
	public sealed class TextureConverter
	{
		private const int ClosepixelsLength = 8;

		private static int[,] _closePixels = new int[,]
		{
			{
				-1,
				-1
			},
			{
				0,
				-1
			},
			{
				1,
				-1
			},
			{
				1,
				0
			},
			{
				1,
				1
			},
			{
				0,
				1
			},
			{
				-1,
				1
			},
			{
				-1,
				0
			}
		};

		private uint[] _data;

		private int _dataLength;

		private int _width;

		private int _height;

		private VerticesDetectionType _polygonDetectionType;

		private uint _alphaTolerance;

		private FP _hullTolerance;

		private bool _holeDetection;

		private bool _multipartDetection;

		private bool _pixelOffsetOptimization;

		private Matrix _transform = Matrix.Identity;

		private int _tempIsSolidX;

		private int _tempIsSolidY;

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

		public byte AlphaTolerance
		{
			get
			{
				return (byte)(this._alphaTolerance >> 24);
			}
			set
			{
				this._alphaTolerance = (uint)((uint)value << 24);
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
				bool flag = value > 4f;
				if (flag)
				{
					this._hullTolerance = 4f;
				}
				else
				{
					bool flag2 = value < 0.9f;
					if (flag2)
					{
						this._hullTolerance = 0.9f;
					}
					else
					{
						this._hullTolerance = value;
					}
				}
			}
		}

		public TextureConverter()
		{
			this.Initialize(null, null, null, null, null, null, null, null);
		}

		public TextureConverter(byte? alphaTolerance, FP? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, Matrix? transform)
		{
			this.Initialize(null, null, alphaTolerance, hullTolerance, holeDetection, multipartDetection, pixelOffsetOptimization, transform);
		}

		public TextureConverter(uint[] data, int width)
		{
			this.Initialize(data, new int?(width), null, null, null, null, null, null);
		}

		public TextureConverter(uint[] data, int width, byte? alphaTolerance, FP? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, Matrix? transform)
		{
			this.Initialize(data, new int?(width), alphaTolerance, hullTolerance, holeDetection, multipartDetection, pixelOffsetOptimization, transform);
		}

		private void Initialize(uint[] data, int? width, byte? alphaTolerance, FP? hullTolerance, bool? holeDetection, bool? multipartDetection, bool? pixelOffsetOptimization, Matrix? transform)
		{
			bool flag = data != null && !width.HasValue;
			if (flag)
			{
				throw new ArgumentNullException("width", "'width' can't be null if 'data' is set.");
			}
			bool flag2 = data == null && width.HasValue;
			if (flag2)
			{
				throw new ArgumentNullException("data", "'data' can't be null if 'width' is set.");
			}
			bool flag3 = data != null && width.HasValue;
			if (flag3)
			{
				this.SetTextureData(data, width.Value);
			}
			bool hasValue = alphaTolerance.HasValue;
			if (hasValue)
			{
				this.AlphaTolerance = alphaTolerance.Value;
			}
			else
			{
				this.AlphaTolerance = 20;
			}
			bool hasValue2 = hullTolerance.HasValue;
			if (hasValue2)
			{
				this.HullTolerance = hullTolerance.Value;
			}
			else
			{
				this.HullTolerance = 1.5f;
			}
			bool hasValue3 = holeDetection.HasValue;
			if (hasValue3)
			{
				this.HoleDetection = holeDetection.Value;
			}
			else
			{
				this.HoleDetection = false;
			}
			bool hasValue4 = multipartDetection.HasValue;
			if (hasValue4)
			{
				this.MultipartDetection = multipartDetection.Value;
			}
			else
			{
				this.MultipartDetection = false;
			}
			bool hasValue5 = pixelOffsetOptimization.HasValue;
			if (hasValue5)
			{
				this.PixelOffsetOptimization = pixelOffsetOptimization.Value;
			}
			else
			{
				this.PixelOffsetOptimization = false;
			}
			bool hasValue6 = transform.HasValue;
			if (hasValue6)
			{
				this.Transform = transform.Value;
			}
			else
			{
				this.Transform = Matrix.Identity;
			}
		}

		private void SetTextureData(uint[] data, int width)
		{
			bool flag = data == null;
			if (flag)
			{
				throw new ArgumentNullException("data", "'data' can't be null.");
			}
			bool flag2 = data.Length < 4;
			if (flag2)
			{
				throw new ArgumentOutOfRangeException("data", "'data' length can't be less then 4. Your texture must be at least 2 x 2 pixels in size.");
			}
			bool flag3 = width < 2;
			if (flag3)
			{
				throw new ArgumentOutOfRangeException("width", "'width' can't be less then 2. Your texture must be at least 2 x 2 pixels in size.");
			}
			bool flag4 = data.Length % width != 0;
			if (flag4)
			{
				throw new ArgumentException("'width' has an invalid value.");
			}
			this._data = data;
			this._dataLength = this._data.Length;
			this._width = width;
			this._height = this._dataLength / width;
		}

		public static Vertices DetectVertices(uint[] data, int width)
		{
			TextureConverter textureConverter = new TextureConverter(data, width);
			List<Vertices> list = textureConverter.DetectVertices();
			return list[0];
		}

		public static Vertices DetectVertices(uint[] data, int width, bool holeDetection)
		{
			TextureConverter textureConverter = new TextureConverter(data, width)
			{
				HoleDetection = holeDetection
			};
			List<Vertices> list = textureConverter.DetectVertices();
			return list[0];
		}

		public static List<Vertices> DetectVertices(uint[] data, int width, FP hullTolerance, byte alphaTolerance, bool multiPartDetection, bool holeDetection)
		{
			TextureConverter textureConverter = new TextureConverter(data, width)
			{
				HullTolerance = hullTolerance,
				AlphaTolerance = alphaTolerance,
				MultipartDetection = multiPartDetection,
				HoleDetection = holeDetection
			};
			List<Vertices> list = textureConverter.DetectVertices();
			List<Vertices> list2 = new List<Vertices>();
			for (int i = 0; i < list.Count; i++)
			{
				list2.Add(list[i]);
			}
			return list2;
		}

		public List<Vertices> DetectVertices()
		{
			bool flag = this._data == null;
			if (flag)
			{
				throw new Exception("'_data' can't be null. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			bool flag2 = this._data.Length < 4;
			if (flag2)
			{
				throw new Exception("'_data' length can't be less then 4. Your texture must be at least 2 x 2 pixels in size. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			bool flag3 = this._width < 2;
			if (flag3)
			{
				throw new Exception("'_width' can't be less then 2. Your texture must be at least 2 x 2 pixels in size. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			bool flag4 = this._data.Length % this._width != 0;
			if (flag4)
			{
				throw new Exception("'_width' has an invalid value. You have to use SetTextureData(uint[] data, int width) before calling this method.");
			}
			List<Vertices> list = new List<Vertices>();
			TSVector2? lastHoleEntrance = null;
			TSVector2? tSVector = null;
			List<TSVector2> list2 = new List<TSVector2>();
			bool flag7;
			do
			{
				bool flag5 = list.Count == 0;
				Vertices vertices;
				if (flag5)
				{
					vertices = new Vertices(this.CreateSimplePolygon(TSVector2.zero, TSVector2.zero));
					bool flag6 = vertices.Count > 2;
					if (flag6)
					{
						tSVector = this.GetTopMostVertex(vertices);
					}
				}
				else
				{
					bool hasValue = tSVector.HasValue;
					if (!hasValue)
					{
						break;
					}
					vertices = new Vertices(this.CreateSimplePolygon(tSVector.Value, new TSVector2(tSVector.Value.x - 1f, tSVector.Value.y)));
				}
				flag7 = false;
				bool flag8 = vertices.Count > 2;
				if (flag8)
				{
					bool holeDetection = this._holeDetection;
					if (holeDetection)
					{
						while (true)
						{
							lastHoleEntrance = this.SearchHoleEntrance(vertices, lastHoleEntrance);
							bool hasValue2 = lastHoleEntrance.HasValue;
							if (!hasValue2)
							{
								break;
							}
							bool flag9 = !list2.Contains(lastHoleEntrance.Value);
							if (!flag9)
							{
								break;
							}
							list2.Add(lastHoleEntrance.Value);
							Vertices vertices2 = this.CreateSimplePolygon(lastHoleEntrance.Value, new TSVector2(lastHoleEntrance.Value.x + 1, lastHoleEntrance.Value.y));
							bool flag10 = vertices2 != null && vertices2.Count > 2;
							if (flag10)
							{
								VerticesDetectionType polygonDetectionType = this._polygonDetectionType;
								if (polygonDetectionType != VerticesDetectionType.Integrated)
								{
									if (polygonDetectionType == VerticesDetectionType.Separated)
									{
										bool flag11 = vertices.Holes == null;
										if (flag11)
										{
											vertices.Holes = new List<Vertices>();
										}
										vertices.Holes.Add(vertices2);
									}
								}
								else
								{
									vertices2.Add(vertices2[0]);
									int num;
									int index;
									bool flag12 = this.SplitPolygonEdge(vertices, lastHoleEntrance.Value, out num, out index);
									if (flag12)
									{
										vertices.InsertRange(index, vertices2);
									}
								}
							}
						}
					}
					list.Add(vertices);
				}
				bool flag13 = this._multipartDetection || vertices.Count <= 2;
				if (flag13)
				{
					bool flag14 = this.SearchNextHullEntrance(list, tSVector.Value, out tSVector);
					if (flag14)
					{
						flag7 = true;
					}
				}
			}
			while (flag7);
			bool flag15 = list == null || (list != null && list.Count == 0);
			if (flag15)
			{
				throw new Exception("Couldn't detect any vertices.");
			}
			bool flag16 = this.PolygonDetectionType == VerticesDetectionType.Separated;
			if (flag16)
			{
				this.ApplyTriangulationCompatibleWinding(ref list);
			}
			bool flag17 = this._transform != Matrix.Identity;
			if (flag17)
			{
				this.ApplyTransform(ref list);
			}
			return list;
		}

		private void ApplyTriangulationCompatibleWinding(ref List<Vertices> detectedPolygons)
		{
			for (int i = 0; i < detectedPolygons.Count; i++)
			{
				detectedPolygons[i].Reverse();
				bool flag = detectedPolygons[i].Holes != null && detectedPolygons[i].Holes.Count > 0;
				if (flag)
				{
					for (int j = 0; j < detectedPolygons[i].Holes.Count; j++)
					{
						detectedPolygons[i].Holes[j].Reverse();
					}
				}
			}
		}

		private void ApplyTransform(ref List<Vertices> detectedPolygons)
		{
			for (int i = 0; i < detectedPolygons.Count; i++)
			{
				detectedPolygons[i].Transform(ref this._transform);
			}
		}

		public bool IsSolid(ref TSVector2 v)
		{
			this._tempIsSolidX = (int)((long)v.x);
			this._tempIsSolidY = (int)((long)v.y);
			bool flag = this._tempIsSolidX >= 0 && this._tempIsSolidX < this._width && this._tempIsSolidY >= 0 && this._tempIsSolidY < this._height;
			return flag && this._data[this._tempIsSolidX + this._tempIsSolidY * this._width] >= this._alphaTolerance;
		}

		public bool IsSolid(ref int x, ref int y)
		{
			bool flag = x >= 0 && x < this._width && y >= 0 && y < this._height;
			return flag && this._data[x + y * this._width] >= this._alphaTolerance;
		}

		public bool IsSolid(ref int index)
		{
			bool flag = index >= 0 && index < this._dataLength;
			return flag && this._data[index] >= this._alphaTolerance;
		}

		public bool InBounds(ref TSVector2 coord)
		{
			return coord.x >= 0f && coord.x < this._width && coord.y >= 0f && coord.y < this._height;
		}

		private TSVector2? SearchHoleEntrance(Vertices polygon, TSVector2? lastHoleEntrance)
		{
			bool flag = polygon == null;
			if (flag)
			{
				throw new ArgumentNullException("'polygon' can't be null.");
			}
			bool flag2 = polygon.Count < 3;
			if (flag2)
			{
				throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
			}
			int value = 0;
			bool hasValue = lastHoleEntrance.HasValue;
			int num;
			if (hasValue)
			{
				num = (int)((long)lastHoleEntrance.Value.y);
			}
			else
			{
				num = (int)((long)this.GetTopMostCoord(polygon));
			}
			int num2 = (int)((long)this.GetBottomMostCoord(polygon));
			bool flag3 = num > 0 && num < this._height && num2 > 0 && num2 < this._height;
			TSVector2? result;
			if (flag3)
			{
				for (int i = num; i <= num2; i++)
				{
					List<FP> list = this.SearchCrossingEdges(polygon, i);
					bool flag4 = list.Count > 1 && list.Count % 2 == 0;
					if (flag4)
					{
						for (int j = 0; j < list.Count; j += 2)
						{
							bool flag5 = false;
							bool flag6 = false;
							for (int k = (int)((long)list[j]); k <= (int)((long)list[j + 1]); k++)
							{
								bool flag7 = this.IsSolid(ref k, ref i);
								if (flag7)
								{
									bool flag8 = !flag6;
									if (flag8)
									{
										flag5 = true;
										value = k;
									}
									bool flag9 = flag5 & flag6;
									if (flag9)
									{
										TSVector2? tSVector = new TSVector2?(new TSVector2(value, i));
										bool flag10 = this.DistanceToHullAcceptable(polygon, tSVector.Value, true);
										if (flag10)
										{
											result = tSVector;
											return result;
										}
										tSVector = null;
										break;
									}
								}
								else
								{
									bool flag11 = flag5;
									if (flag11)
									{
										flag6 = true;
									}
								}
							}
						}
					}
					else
					{
						bool flag12 = list.Count % 2 == 0;
						if (flag12)
						{
							Debug.WriteLine("SearchCrossingEdges() % 2 != 0");
						}
					}
				}
			}
			result = null;
			return result;
		}

		private bool DistanceToHullAcceptableHoles(Vertices polygon, TSVector2 point, bool higherDetail)
		{
			bool flag = polygon == null;
			if (flag)
			{
				throw new ArgumentNullException("polygon", "'polygon' can't be null.");
			}
			bool flag2 = polygon.Count < 3;
			if (flag2)
			{
				throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
			}
			bool flag3 = this.DistanceToHullAcceptable(polygon, point, higherDetail);
			bool result;
			if (flag3)
			{
				bool flag4 = polygon.Holes != null;
				if (flag4)
				{
					for (int i = 0; i < polygon.Holes.Count; i++)
					{
						bool flag5 = !this.DistanceToHullAcceptable(polygon.Holes[i], point, higherDetail);
						if (flag5)
						{
							result = false;
							return result;
						}
					}
				}
				result = true;
			}
			else
			{
				result = false;
			}
			return result;
		}

		private bool DistanceToHullAcceptable(Vertices polygon, TSVector2 point, bool higherDetail)
		{
			bool flag = polygon == null;
			if (flag)
			{
				throw new ArgumentNullException("polygon", "'polygon' can't be null.");
			}
			bool flag2 = polygon.Count < 3;
			if (flag2)
			{
				throw new ArgumentException("'polygon.Count' can't be less then 3.");
			}
			TSVector2 tSVector = polygon[polygon.Count - 1];
			bool result;
			if (higherDetail)
			{
				for (int i = 0; i < polygon.Count; i++)
				{
					TSVector2 value = polygon[i];
					bool flag3 = LineTools.DistanceBetweenPointAndLineSegment(ref point, ref value, ref tSVector) <= this._hullTolerance || TSVector2.Distance(point, value) <= this._hullTolerance;
					if (flag3)
					{
						result = false;
						return result;
					}
					tSVector = polygon[i];
				}
				result = true;
			}
			else
			{
				for (int j = 0; j < polygon.Count; j++)
				{
					TSVector2 value = polygon[j];
					bool flag4 = LineTools.DistanceBetweenPointAndLineSegment(ref point, ref value, ref tSVector) <= this._hullTolerance;
					if (flag4)
					{
						result = false;
						return result;
					}
					tSVector = polygon[j];
				}
				result = true;
			}
			return result;
		}

		private bool InPolygon(Vertices polygon, TSVector2 point)
		{
			bool flag = !this.DistanceToHullAcceptableHoles(polygon, point, true);
			bool flag2 = !flag;
			bool result;
			if (flag2)
			{
				List<FP> list = this.SearchCrossingEdgesHoles(polygon, (int)((long)point.y));
				bool flag3 = list.Count > 0 && list.Count % 2 == 0;
				if (flag3)
				{
					for (int i = 0; i < list.Count; i += 2)
					{
						bool flag4 = list[i] <= point.x && list[i + 1] >= point.x;
						if (flag4)
						{
							result = true;
							return result;
						}
					}
				}
				result = false;
			}
			else
			{
				result = true;
			}
			return result;
		}

		private TSVector2? GetTopMostVertex(Vertices vertices)
		{
			FP x = FP.MaxValue;
			TSVector2? result = null;
			for (int i = 0; i < vertices.Count; i++)
			{
				bool flag = x > vertices[i].y;
				if (flag)
				{
					x = vertices[i].y;
					result = new TSVector2?(vertices[i]);
				}
			}
			return result;
		}

		private FP GetTopMostCoord(Vertices vertices)
		{
			FP fP = FP.MaxValue;
			for (int i = 0; i < vertices.Count; i++)
			{
				bool flag = fP > vertices[i].y;
				if (flag)
				{
					fP = vertices[i].y;
				}
			}
			return fP;
		}

		private FP GetBottomMostCoord(Vertices vertices)
		{
			FP fP = FP.MinValue;
			for (int i = 0; i < vertices.Count; i++)
			{
				bool flag = fP < vertices[i].y;
				if (flag)
				{
					fP = vertices[i].y;
				}
			}
			return fP;
		}

		private List<FP> SearchCrossingEdgesHoles(Vertices polygon, int y)
		{
			bool flag = polygon == null;
			if (flag)
			{
				throw new ArgumentNullException("polygon", "'polygon' can't be null.");
			}
			bool flag2 = polygon.Count < 3;
			if (flag2)
			{
				throw new ArgumentException("'polygon.MainPolygon.Count' can't be less then 3.");
			}
			List<FP> list = this.SearchCrossingEdges(polygon, y);
			bool flag3 = polygon.Holes != null;
			if (flag3)
			{
				for (int i = 0; i < polygon.Holes.Count; i++)
				{
					list.AddRange(this.SearchCrossingEdges(polygon.Holes[i], y));
				}
			}
			list.Sort();
			return list;
		}

		private List<FP> SearchCrossingEdges(Vertices polygon, int y)
		{
			List<FP> list = new List<FP>();
			bool flag = polygon.Count > 2;
			if (flag)
			{
				TSVector2 tSVector = polygon[polygon.Count - 1];
				for (int i = 0; i < polygon.Count; i++)
				{
					TSVector2 tSVector2 = polygon[i];
					bool flag2 = (tSVector2.y >= y && tSVector.y <= y) || (tSVector2.y <= y && tSVector.y >= y);
					if (flag2)
					{
						bool flag3 = tSVector2.y != tSVector.y;
						if (flag3)
						{
							bool flag4 = true;
							TSVector2 tSVector3 = tSVector - tSVector2;
							bool flag5 = tSVector2.y == y;
							if (flag5)
							{
								TSVector2 value = polygon[(i + 1) % polygon.Count];
								TSVector2 tSVector4 = tSVector2 - value;
								bool flag6 = tSVector3.y > 0;
								if (flag6)
								{
									flag4 = (tSVector4.y <= 0);
								}
								else
								{
									flag4 = (tSVector4.y >= 0);
								}
							}
							bool flag7 = flag4;
							if (flag7)
							{
								list.Add((y - tSVector2.y) / tSVector3.y * tSVector3.x + tSVector2.x);
							}
						}
					}
					tSVector = tSVector2;
				}
			}
			list.Sort();
			return list;
		}

		private bool SplitPolygonEdge(Vertices polygon, TSVector2 coordInsideThePolygon, out int vertex1Index, out int vertex2Index)
		{
			int num = 0;
			int index = 0;
			bool flag = false;
			FP y = FP.MaxValue;
			bool flag2 = false;
			TSVector2 zero = TSVector2.zero;
			List<FP> list = this.SearchCrossingEdges(polygon, (int)((long)coordInsideThePolygon.y));
			vertex1Index = 0;
			vertex2Index = 0;
			zero.y = coordInsideThePolygon.y;
			bool flag3 = list != null && list.Count > 1 && list.Count % 2 == 0;
			bool result;
			if (flag3)
			{
				for (int i = 0; i < list.Count; i++)
				{
					bool flag4 = list[i] < coordInsideThePolygon.x;
					if (flag4)
					{
						FP fP = coordInsideThePolygon.x - list[i];
						bool flag5 = fP < y;
						if (flag5)
						{
							y = fP;
							zero.x = list[i];
							flag2 = true;
						}
					}
				}
				bool flag6 = flag2;
				if (flag6)
				{
					y = FP.MaxValue;
					int num2 = polygon.Count - 1;
					for (int j = 0; j < polygon.Count; j++)
					{
						TSVector2 tSVector = polygon[j];
						TSVector2 tSVector2 = polygon[num2];
						FP fP = LineTools.DistanceBetweenPointAndLineSegment(ref zero, ref tSVector, ref tSVector2);
						bool flag7 = fP < y;
						if (flag7)
						{
							y = fP;
							num = j;
							index = num2;
							flag = true;
						}
						num2 = j;
					}
					bool flag8 = flag;
					if (flag8)
					{
						TSVector2 value = polygon[index] - polygon[num];
						value.Normalize();
						TSVector2 value2 = polygon[num];
						FP fP = TSVector2.Distance(value2, zero);
						vertex1Index = num;
						vertex2Index = num + 1;
						polygon.Insert(num, fP * value + polygon[vertex1Index]);
						polygon.Insert(num, fP * value + polygon[vertex2Index]);
						result = true;
						return result;
					}
				}
			}
			result = false;
			return result;
		}

		private Vertices CreateSimplePolygon(TSVector2 entrance, TSVector2 last)
		{
			bool flag = false;
			bool flag2 = false;
			Vertices vertices = new Vertices(32);
			Vertices vertices2 = new Vertices(32);
			Vertices vertices3 = new Vertices(32);
			TSVector2 tSVector = TSVector2.zero;
			bool flag3 = entrance == TSVector2.zero || !this.InBounds(ref entrance);
			if (flag3)
			{
				flag = this.SearchHullEntrance(out entrance);
				bool flag4 = flag;
				if (flag4)
				{
					tSVector = new TSVector2(entrance.x - 1f, entrance.y);
				}
			}
			else
			{
				bool flag5 = this.IsSolid(ref entrance);
				if (flag5)
				{
					bool flag6 = this.IsNearPixel(ref entrance, ref last);
					if (flag6)
					{
						tSVector = last;
						flag = true;
					}
					else
					{
						TSVector2 tSVector2;
						bool flag7 = this.SearchNearPixels(false, ref entrance, out tSVector2);
						if (flag7)
						{
							tSVector = tSVector2;
							flag = true;
						}
						else
						{
							flag = false;
						}
					}
				}
			}
			bool flag8 = flag;
			if (flag8)
			{
				vertices.Add(entrance);
				vertices2.Add(entrance);
				TSVector2 tSVector3 = entrance;
				TSVector2 item;
				while (true)
				{
					bool flag9 = this.SearchForOutstandingVertex(vertices2, out item);
					if (flag9)
					{
						bool flag10 = flag2;
						if (flag10)
						{
							break;
						}
						vertices.Add(item);
						vertices2.RemoveRange(0, vertices2.IndexOf(item));
					}
					last = tSVector;
					tSVector = tSVector3;
					bool nextHullPoint = this.GetNextHullPoint(ref last, ref tSVector, out tSVector3);
					if (!nextHullPoint)
					{
						goto IL_161;
					}
					vertices2.Add(tSVector3);
					bool flag11 = tSVector3 == entrance && !flag2;
					if (flag11)
					{
						flag2 = true;
						vertices3.AddRange(vertices2);
						bool flag12 = vertices3.Contains(entrance);
						if (flag12)
						{
							vertices3.Remove(entrance);
						}
					}
				}
				bool flag13 = vertices3.Contains(item);
				if (flag13)
				{
					vertices.Add(item);
				}
				IL_161:;
			}
			return vertices;
		}

		private bool SearchNearPixels(bool searchingForSolidPixel, ref TSVector2 current, out TSVector2 foundPixel)
		{
			bool result;
			for (int i = 0; i < 8; i++)
			{
				int value = (int)((long)current.x) + TextureConverter._closePixels[i, 0];
				int value2 = (int)((long)current.y) + TextureConverter._closePixels[i, 1];
				bool flag = !searchingForSolidPixel ^ this.IsSolid(ref value, ref value2);
				if (flag)
				{
					foundPixel = new TSVector2(value, value2);
					result = true;
					return result;
				}
			}
			foundPixel = TSVector2.zero;
			result = false;
			return result;
		}

		private bool IsNearPixel(ref TSVector2 current, ref TSVector2 near)
		{
			bool result;
			for (int i = 0; i < 8; i++)
			{
				int num = (int)((long)current.x) + TextureConverter._closePixels[i, 0];
				int num2 = (int)((long)current.y) + TextureConverter._closePixels[i, 1];
				bool flag = num >= 0 && num <= this._width && num2 >= 0 && num2 <= this._height;
				if (flag)
				{
					bool flag2 = num == (int)((long)near.x) && num2 == (int)((long)near.y);
					if (flag2)
					{
						result = true;
						return result;
					}
				}
			}
			result = false;
			return result;
		}

		private bool SearchHullEntrance(out TSVector2 entrance)
		{
			bool result;
			for (int i = 0; i <= this._height; i++)
			{
				for (int j = 0; j <= this._width; j++)
				{
					bool flag = this.IsSolid(ref j, ref i);
					if (flag)
					{
						entrance = new TSVector2(j, i);
						result = true;
						return result;
					}
				}
			}
			entrance = TSVector2.zero;
			result = false;
			return result;
		}

		private bool SearchNextHullEntrance(List<Vertices> detectedPolygons, TSVector2 start, out TSVector2? entrance)
		{
			bool flag = false;
			bool result;
			for (int i = (int)((long)start.x) + (int)((long)start.y) * this._width; i <= this._dataLength; i++)
			{
				bool flag2 = this.IsSolid(ref i);
				if (flag2)
				{
					bool flag3 = flag;
					if (flag3)
					{
						int num = i % this._width;
						entrance = new TSVector2?(new TSVector2(num, (i - num) / this._width));
						bool flag4 = false;
						for (int j = 0; j < detectedPolygons.Count; j++)
						{
							bool flag5 = this.InPolygon(detectedPolygons[j], entrance.Value);
							if (flag5)
							{
								flag4 = true;
								break;
							}
						}
						bool flag6 = flag4;
						if (!flag6)
						{
							result = true;
							return result;
						}
						flag = false;
					}
				}
				else
				{
					flag = true;
				}
			}
			entrance = null;
			result = false;
			return result;
		}

		private bool GetNextHullPoint(ref TSVector2 last, ref TSVector2 current, out TSVector2 next)
		{
			int indexOfFirstPixelToCheck = this.GetIndexOfFirstPixelToCheck(ref last, ref current);
			bool result;
			for (int i = 0; i < 8; i++)
			{
				int num = (indexOfFirstPixelToCheck + i) % 8;
				int num2 = (int)((long)current.x) + TextureConverter._closePixels[num, 0];
				int num3 = (int)((long)current.y) + TextureConverter._closePixels[num, 1];
				bool flag = num2 >= 0 && num2 < this._width && num3 >= 0 && num3 <= this._height;
				if (flag)
				{
					bool flag2 = this.IsSolid(ref num2, ref num3);
					if (flag2)
					{
						next = new TSVector2(num2, num3);
						result = true;
						return result;
					}
				}
			}
			next = TSVector2.zero;
			result = false;
			return result;
		}

		private bool SearchForOutstandingVertex(Vertices hullArea, out TSVector2 outstanding)
		{
			TSVector2 tSVector = TSVector2.zero;
			bool result = false;
			bool flag = hullArea.Count > 2;
			if (flag)
			{
				int num = hullArea.Count - 1;
				TSVector2 tSVector2 = hullArea[0];
				TSVector2 tSVector3 = hullArea[num];
				for (int i = 1; i < num; i++)
				{
					TSVector2 tSVector4 = hullArea[i];
					bool flag2 = LineTools.DistanceBetweenPointAndLineSegment(ref tSVector4, ref tSVector2, ref tSVector3) >= this._hullTolerance;
					if (flag2)
					{
						tSVector = hullArea[i];
						result = true;
						break;
					}
				}
			}
			outstanding = tSVector;
			return result;
		}

		private int GetIndexOfFirstPixelToCheck(ref TSVector2 last, ref TSVector2 current)
		{
			int result;
			switch ((int)((long)(current.x - last.x)))
			{
			case -1:
				switch ((int)((long)(current.y - last.y)))
				{
				case -1:
					result = 5;
					return result;
				case 0:
					result = 4;
					return result;
				case 1:
					result = 3;
					return result;
				}
				break;
			case 0:
			{
				int num = (int)((long)(current.y - last.y));
				if (num == -1)
				{
					result = 6;
					return result;
				}
				if (num == 1)
				{
					result = 2;
					return result;
				}
				break;
			}
			case 1:
				switch ((int)((long)(current.y - last.y)))
				{
				case -1:
					result = 7;
					return result;
				case 0:
					result = 0;
					return result;
				case 1:
					result = 1;
					return result;
				}
				break;
			}
			result = 0;
			return result;
		}
	}
}
