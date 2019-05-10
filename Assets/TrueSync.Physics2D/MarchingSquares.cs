using System;
using System.Collections.Generic;

namespace TrueSync.Physics2D
{
	public static class MarchingSquares
	{
		internal class CxFastList<T>
		{
			private MarchingSquares.CxFastListNode<T> _head;

			private int _count;

			public MarchingSquares.CxFastListNode<T> Begin()
			{
				return this._head;
			}

			public MarchingSquares.CxFastListNode<T> End()
			{
				return null;
			}

			public T Front()
			{
				return this._head.Elem();
			}

			public MarchingSquares.CxFastListNode<T> Add(T value)
			{
				MarchingSquares.CxFastListNode<T> cxFastListNode = new MarchingSquares.CxFastListNode<T>(value);
				bool flag = this._head == null;
				MarchingSquares.CxFastListNode<T> result;
				if (flag)
				{
					cxFastListNode._next = null;
					this._head = cxFastListNode;
					this._count++;
					result = cxFastListNode;
				}
				else
				{
					cxFastListNode._next = this._head;
					this._head = cxFastListNode;
					this._count++;
					result = cxFastListNode;
				}
				return result;
			}

			public bool Remove(T value)
			{
				MarchingSquares.CxFastListNode<T> cxFastListNode = this._head;
				MarchingSquares.CxFastListNode<T> cxFastListNode2 = this._head;
				EqualityComparer<T> @default = EqualityComparer<T>.Default;
				bool flag = cxFastListNode != null;
				bool result;
				if (flag)
				{
					bool flag2 = value != null;
					if (flag2)
					{
						while (true)
						{
							bool flag3 = @default.Equals(cxFastListNode._elt, value);
							if (flag3)
							{
								break;
							}
							cxFastListNode2 = cxFastListNode;
							cxFastListNode = cxFastListNode._next;
							if (cxFastListNode == null)
							{
								goto IL_AA;
							}
						}
						bool flag4 = cxFastListNode == this._head;
						if (flag4)
						{
							this._head = cxFastListNode._next;
							this._count--;
							result = true;
							return result;
						}
						cxFastListNode2._next = cxFastListNode._next;
						this._count--;
						result = true;
						return result;
					}
					IL_AA:;
				}
				result = false;
				return result;
			}

			public MarchingSquares.CxFastListNode<T> Pop()
			{
				return this.Erase(null, this._head);
			}

			public MarchingSquares.CxFastListNode<T> Insert(MarchingSquares.CxFastListNode<T> node, T value)
			{
				bool flag = node == null;
				MarchingSquares.CxFastListNode<T> result;
				if (flag)
				{
					result = this.Add(value);
				}
				else
				{
					MarchingSquares.CxFastListNode<T> cxFastListNode = new MarchingSquares.CxFastListNode<T>(value);
					MarchingSquares.CxFastListNode<T> next = node._next;
					cxFastListNode._next = next;
					node._next = cxFastListNode;
					this._count++;
					result = cxFastListNode;
				}
				return result;
			}

			public MarchingSquares.CxFastListNode<T> Erase(MarchingSquares.CxFastListNode<T> prev, MarchingSquares.CxFastListNode<T> node)
			{
				MarchingSquares.CxFastListNode<T> next = node._next;
				bool flag = prev != null;
				MarchingSquares.CxFastListNode<T> result;
				if (flag)
				{
					prev._next = next;
				}
				else
				{
					bool flag2 = this._head != null;
					if (!flag2)
					{
						result = null;
						return result;
					}
					this._head = this._head._next;
				}
				this._count--;
				result = next;
				return result;
			}

			public bool Empty()
			{
				return this._head == null;
			}

			public int Size()
			{
				MarchingSquares.CxFastListNode<T> cxFastListNode = this.Begin();
				int num = 0;
				do
				{
					num++;
				}
				while (cxFastListNode.Next() != null);
				return num;
			}

			public void Clear()
			{
				MarchingSquares.CxFastListNode<T> cxFastListNode = this._head;
				while (cxFastListNode != null)
				{
					MarchingSquares.CxFastListNode<T> cxFastListNode2 = cxFastListNode;
					cxFastListNode = cxFastListNode._next;
					cxFastListNode2._next = null;
				}
				this._head = null;
				this._count = 0;
			}

			public bool Has(T value)
			{
				return this.Find(value) != null;
			}

			public MarchingSquares.CxFastListNode<T> Find(T value)
			{
				MarchingSquares.CxFastListNode<T> cxFastListNode = this._head;
				EqualityComparer<T> @default = EqualityComparer<T>.Default;
				bool flag = cxFastListNode != null;
				MarchingSquares.CxFastListNode<T> result;
				if (flag)
				{
					bool flag2 = value != null;
					if (!flag2)
					{
						while (true)
						{
							bool flag3 = cxFastListNode._elt == null;
							if (flag3)
							{
								break;
							}
							cxFastListNode = cxFastListNode._next;
							if (cxFastListNode == this._head)
							{
								goto IL_93;
							}
						}
						result = cxFastListNode;
						return result;
					}
					while (true)
					{
						bool flag4 = @default.Equals(cxFastListNode._elt, value);
						if (flag4)
						{
							break;
						}
						cxFastListNode = cxFastListNode._next;
						if (cxFastListNode == this._head)
						{
							goto Block_3;
						}
					}
					result = cxFastListNode;
					return result;
					Block_3:
					IL_93:;
				}
				result = null;
				return result;
			}

			public List<T> GetListOfElements()
			{
				List<T> list = new List<T>();
				MarchingSquares.CxFastListNode<T> cxFastListNode = this.Begin();
				bool flag = cxFastListNode != null;
				if (flag)
				{
					do
					{
						list.Add(cxFastListNode._elt);
						cxFastListNode = cxFastListNode._next;
					}
					while (cxFastListNode != null);
				}
				return list;
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
			public int Length;

			public MarchingSquares.CxFastList<TSVector2> Points;

			public GeomPoly()
			{
				this.Points = new MarchingSquares.CxFastList<TSVector2>();
				this.Length = 0;
			}
		}

		private class GeomPolyVal
		{
			public int Key;

			public MarchingSquares.GeomPoly GeomP;

			public GeomPolyVal(MarchingSquares.GeomPoly geomP, int K)
			{
				this.GeomP = geomP;
				this.Key = K;
			}
		}

		private static int[] _lookMarch = new int[]
		{
			0,
			224,
			56,
			216,
			14,
			238,
			54,
			214,
			131,
			99,
			187,
			91,
			141,
			109,
			181,
			85
		};

		public static List<Vertices> DetectSquares(AABB domain, FP cellWidth, FP cellHeight, sbyte[,] f, int lerpCount, bool combine)
		{
			MarchingSquares.CxFastList<MarchingSquares.GeomPoly> cxFastList = new MarchingSquares.CxFastList<MarchingSquares.GeomPoly>();
			List<Vertices> list = new List<Vertices>();
			int num = (int)((long)(domain.Extents.x * 2 / cellWidth));
			bool flag = num == domain.Extents.x * 2 / cellWidth;
			int num2 = (int)((long)(domain.Extents.y * 2 / cellHeight));
			bool flag2 = num2 == domain.Extents.y * 2 / cellHeight;
			bool flag3 = !flag;
			if (flag3)
			{
				num++;
			}
			bool flag4 = !flag2;
			if (flag4)
			{
				num2++;
			}
			sbyte[,] array = new sbyte[num + 1, num2 + 1];
			MarchingSquares.GeomPolyVal[,] array2 = new MarchingSquares.GeomPolyVal[num + 1, num2 + 1];
			for (int i = 0; i < num + 1; i++)
			{
				bool flag5 = i == num;
				int num3;
				if (flag5)
				{
					num3 = (int)((long)domain.UpperBound.x);
				}
				else
				{
					num3 = (int)((long)(i * cellWidth + domain.LowerBound.x));
				}
				for (int j = 0; j < num2 + 1; j++)
				{
					bool flag6 = j == num2;
					int num4;
					if (flag6)
					{
						num4 = (int)((long)domain.UpperBound.y);
					}
					else
					{
						num4 = (int)((long)(j * cellHeight + domain.LowerBound.y));
					}
					array[i, j] = f[num3, num4];
				}
			}
			for (int k = 0; k < num2; k++)
			{
				FP fP = k * cellHeight + domain.LowerBound.y;
				bool flag7 = k == num2 - 1;
				FP y;
				if (flag7)
				{
					y = domain.UpperBound.y;
				}
				else
				{
					y = fP + cellHeight;
				}
				MarchingSquares.GeomPoly geomPoly = null;
				for (int l = 0; l < num; l++)
				{
					FP fP2 = l * cellWidth + domain.LowerBound.x;
					bool flag8 = l == num - 1;
					FP x;
					if (flag8)
					{
						x = domain.UpperBound.x;
					}
					else
					{
						x = fP2 + cellWidth;
					}
					MarchingSquares.GeomPoly geomPoly2 = new MarchingSquares.GeomPoly();
					int num5 = MarchingSquares.MarchSquare(f, array, ref geomPoly2, l, k, fP2, fP, x, y, lerpCount);
					bool flag9 = geomPoly2.Length != 0;
					if (flag9)
					{
						bool flag10 = combine && geomPoly != null && (num5 & 9) != 0;
						if (flag10)
						{
							MarchingSquares.combLeft(ref geomPoly, ref geomPoly2);
							geomPoly2 = geomPoly;
						}
						else
						{
							cxFastList.Add(geomPoly2);
						}
						array2[l, k] = new MarchingSquares.GeomPolyVal(geomPoly2, num5);
					}
					else
					{
						geomPoly2 = null;
					}
					geomPoly = geomPoly2;
				}
			}
			bool flag11 = !combine;
			List<Vertices> result;
			if (flag11)
			{
				List<MarchingSquares.GeomPoly> listOfElements = cxFastList.GetListOfElements();
				foreach (MarchingSquares.GeomPoly current in listOfElements)
				{
					list.Add(new Vertices(current.Points.GetListOfElements()));
				}
				result = list;
			}
			else
			{
				for (int m = 1; m < num2; m++)
				{
					int n = 0;
					while (n < num)
					{
						MarchingSquares.GeomPolyVal geomPolyVal = array2[n, m];
						bool flag12 = geomPolyVal == null;
						if (flag12)
						{
							n++;
						}
						else
						{
							bool flag13 = (geomPolyVal.Key & 12) == 0;
							if (flag13)
							{
								n++;
							}
							else
							{
								MarchingSquares.GeomPolyVal geomPolyVal2 = array2[n, m - 1];
								bool flag14 = geomPolyVal2 == null;
								if (flag14)
								{
									n++;
								}
								else
								{
									bool flag15 = (geomPolyVal2.Key & 3) == 0;
									if (flag15)
									{
										n++;
									}
									else
									{
										FP fP3 = n * cellWidth + domain.LowerBound.x;
										FP y2 = m * cellHeight + domain.LowerBound.y;
										MarchingSquares.CxFastList<TSVector2> points = geomPolyVal.GeomP.Points;
										MarchingSquares.CxFastList<TSVector2> points2 = geomPolyVal2.GeomP.Points;
										bool flag16 = geomPolyVal2.GeomP == geomPolyVal.GeomP;
										if (flag16)
										{
											n++;
										}
										else
										{
											MarchingSquares.CxFastListNode<TSVector2> cxFastListNode = points.Begin();
											while (MarchingSquares.Square(cxFastListNode.Elem().y - y2) > Settings.Epsilon || cxFastListNode.Elem().x < fP3)
											{
												cxFastListNode = cxFastListNode.Next();
											}
											TSVector2 tSVector = cxFastListNode.Next().Elem();
											bool flag17 = MarchingSquares.Square(tSVector.y - y2) > Settings.Epsilon;
											if (flag17)
											{
												n++;
											}
											else
											{
												bool flag18 = true;
												MarchingSquares.CxFastListNode<TSVector2> cxFastListNode2;
												for (cxFastListNode2 = points2.Begin(); cxFastListNode2 != points2.End(); cxFastListNode2 = cxFastListNode2.Next())
												{
													bool flag19 = MarchingSquares.VecDsq(cxFastListNode2.Elem(), tSVector) < Settings.Epsilon;
													if (flag19)
													{
														flag18 = false;
														break;
													}
												}
												bool flag20 = flag18;
												if (flag20)
												{
													n++;
												}
												else
												{
													MarchingSquares.CxFastListNode<TSVector2> cxFastListNode3 = cxFastListNode.Next().Next();
													bool flag21 = cxFastListNode3 == points.End();
													if (flag21)
													{
														cxFastListNode3 = points.Begin();
													}
													while (cxFastListNode3 != cxFastListNode)
													{
														cxFastListNode2 = points2.Insert(cxFastListNode2, cxFastListNode3.Elem());
														cxFastListNode3 = cxFastListNode3.Next();
														bool flag22 = cxFastListNode3 == points.End();
														if (flag22)
														{
															cxFastListNode3 = points.Begin();
														}
														geomPolyVal2.GeomP.Length++;
													}
													fP3 = n + 1;
													while (fP3 < num)
													{
														MarchingSquares.GeomPolyVal geomPolyVal3 = array2[(int)((long)fP3), m];
														bool flag23 = geomPolyVal3 == null || geomPolyVal3.GeomP != geomPolyVal.GeomP;
														if (flag23)
														{
															fP3 += 1;
														}
														else
														{
															geomPolyVal3.GeomP = geomPolyVal2.GeomP;
															fP3 += 1;
														}
													}
													fP3 = n - 1;
													while (fP3 >= 0)
													{
														MarchingSquares.GeomPolyVal geomPolyVal4 = array2[(int)((long)fP3), m];
														bool flag24 = geomPolyVal4 == null || geomPolyVal4.GeomP != geomPolyVal.GeomP;
														if (flag24)
														{
															fP3 -= 1;
														}
														else
														{
															geomPolyVal4.GeomP = geomPolyVal2.GeomP;
															fP3 -= 1;
														}
													}
													cxFastList.Remove(geomPolyVal.GeomP);
													geomPolyVal.GeomP = geomPolyVal2.GeomP;
													n = (int)((long)((cxFastListNode.Next().Elem().x - domain.LowerBound.x) / cellWidth)) + 1;
												}
											}
										}
									}
								}
							}
						}
					}
				}
				List<MarchingSquares.GeomPoly> listOfElements = cxFastList.GetListOfElements();
				foreach (MarchingSquares.GeomPoly current2 in listOfElements)
				{
					list.Add(new Vertices(current2.Points.GetListOfElements()));
				}
				result = list;
			}
			return result;
		}

		private static FP Lerp(FP x0, FP x1, FP v0, FP v1)
		{
			FP fP = v0 - v1;
			bool flag = fP * fP < Settings.Epsilon;
			FP x2;
			if (flag)
			{
				x2 = 0.5f;
			}
			else
			{
				x2 = v0 / fP;
			}
			return x0 + x2 * (x1 - x0);
		}

		private static FP Xlerp(FP x0, FP x1, FP y, FP v0, FP v1, sbyte[,] f, int c)
		{
			FP fP = MarchingSquares.Lerp(x0, x1, v0, v1);
			bool flag = c == 0;
			FP result;
			if (flag)
			{
				result = fP;
			}
			else
			{
				sbyte value = f[(int)((long)fP), (int)((long)y)];
				bool flag2 = v0 * (int)value < 0;
				if (flag2)
				{
					result = MarchingSquares.Xlerp(x0, fP, y, v0, (int)value, f, c - 1);
				}
				else
				{
					result = MarchingSquares.Xlerp(fP, x1, y, (int)value, v1, f, c - 1);
				}
			}
			return result;
		}

		private static FP Ylerp(FP y0, FP y1, FP x, FP v0, FP v1, sbyte[,] f, int c)
		{
			FP fP = MarchingSquares.Lerp(y0, y1, v0, v1);
			bool flag = c == 0;
			FP result;
			if (flag)
			{
				result = fP;
			}
			else
			{
				sbyte value = f[(int)((long)x), (int)((long)fP)];
				bool flag2 = v0 * (int)value < 0;
				if (flag2)
				{
					result = MarchingSquares.Ylerp(y0, fP, x, v0, (int)value, f, c - 1);
				}
				else
				{
					result = MarchingSquares.Ylerp(fP, y1, x, (int)value, v1, f, c - 1);
				}
			}
			return result;
		}

		private static FP Square(FP x)
		{
			return x * x;
		}

		private static FP VecDsq(TSVector2 a, TSVector2 b)
		{
			TSVector2 tSVector = a - b;
			return tSVector.x * tSVector.x + tSVector.y * tSVector.y;
		}

		private static FP VecCross(TSVector2 a, TSVector2 b)
		{
			return a.x * b.y - a.y * b.x;
		}

		private static int MarchSquare(sbyte[,] f, sbyte[,] fs, ref MarchingSquares.GeomPoly poly, int ax, int ay, FP x0, FP y0, FP x1, FP y1, int bin)
		{
			int num = 0;
			sbyte b = fs[ax, ay];
			bool flag = b < 0;
			if (flag)
			{
				num |= 8;
			}
			sbyte b2 = fs[ax + 1, ay];
			bool flag2 = b2 < 0;
			if (flag2)
			{
				num |= 4;
			}
			sbyte b3 = fs[ax + 1, ay + 1];
			bool flag3 = b3 < 0;
			if (flag3)
			{
				num |= 2;
			}
			sbyte b4 = fs[ax, ay + 1];
			bool flag4 = b4 < 0;
			if (flag4)
			{
				num |= 1;
			}
			int num2 = MarchingSquares._lookMarch[num];
			bool flag5 = num2 != 0;
			if (flag5)
			{
				MarchingSquares.CxFastListNode<TSVector2> node = null;
				for (int i = 0; i < 8; i++)
				{
					bool flag6 = (num2 & 1 << i) != 0;
					if (flag6)
					{
						bool flag7 = i == 7 && (num2 & 1) == 0;
						if (flag7)
						{
							MarchingSquares.CxFastList<TSVector2> arg_EA_0 = poly.Points;
							TSVector2 value = new TSVector2(x0, MarchingSquares.Ylerp(y0, y1, x0, (int)b, (int)b4, f, bin));
							arg_EA_0.Add(value);
						}
						else
						{
							bool flag8 = i == 0;
							TSVector2 value;
							if (flag8)
							{
								value = new TSVector2(x0, y0);
							}
							else
							{
								bool flag9 = i == 2;
								if (flag9)
								{
									value = new TSVector2(x1, y0);
								}
								else
								{
									bool flag10 = i == 4;
									if (flag10)
									{
										value = new TSVector2(x1, y1);
									}
									else
									{
										bool flag11 = i == 6;
										if (flag11)
										{
											value = new TSVector2(x0, y1);
										}
										else
										{
											bool flag12 = i == 1;
											if (flag12)
											{
												value = new TSVector2(MarchingSquares.Xlerp(x0, x1, y0, (int)b, (int)b2, f, bin), y0);
											}
											else
											{
												bool flag13 = i == 5;
												if (flag13)
												{
													value = new TSVector2(MarchingSquares.Xlerp(x0, x1, y1, (int)b4, (int)b3, f, bin), y1);
												}
												else
												{
													bool flag14 = i == 3;
													if (flag14)
													{
														value = new TSVector2(x1, MarchingSquares.Ylerp(y0, y1, x1, (int)b2, (int)b3, f, bin));
													}
													else
													{
														value = new TSVector2(x0, MarchingSquares.Ylerp(y0, y1, x0, (int)b, (int)b4, f, bin));
													}
												}
											}
										}
									}
								}
							}
							node = poly.Points.Insert(node, value);
						}
						poly.Length++;
					}
				}
			}
			return num;
		}

		private static void combLeft(ref MarchingSquares.GeomPoly polya, ref MarchingSquares.GeomPoly polyb)
		{
			MarchingSquares.CxFastList<TSVector2> points = polya.Points;
			MarchingSquares.CxFastList<TSVector2> points2 = polyb.Points;
			MarchingSquares.CxFastListNode<TSVector2> cxFastListNode = points.Begin();
			MarchingSquares.CxFastListNode<TSVector2> cxFastListNode2 = points2.Begin();
			TSVector2 tSVector = cxFastListNode2.Elem();
			MarchingSquares.CxFastListNode<TSVector2> cxFastListNode3 = null;
			while (cxFastListNode != points.End())
			{
				TSVector2 tSVector2 = cxFastListNode.Elem();
				bool flag = MarchingSquares.VecDsq(tSVector2, tSVector) < Settings.Epsilon;
				if (flag)
				{
					bool flag2 = cxFastListNode3 != null;
					if (flag2)
					{
						TSVector2 value = cxFastListNode3.Elem();
						tSVector = cxFastListNode2.Next().Elem();
						TSVector2 a = tSVector2 - value;
						TSVector2 b = tSVector - tSVector2;
						FP fP = MarchingSquares.VecCross(a, b);
						bool flag3 = fP * fP < Settings.Epsilon;
						if (flag3)
						{
							points.Erase(cxFastListNode3, cxFastListNode);
							polya.Length--;
							cxFastListNode = cxFastListNode3;
						}
					}
					bool flag4 = true;
					MarchingSquares.CxFastListNode<TSVector2> cxFastListNode4 = null;
					while (!points2.Empty())
					{
						TSVector2 value2 = points2.Front();
						points2.Pop();
						bool flag5 = !flag4 && !points2.Empty();
						if (flag5)
						{
							cxFastListNode = points.Insert(cxFastListNode, value2);
							polya.Length++;
							cxFastListNode4 = cxFastListNode;
						}
						flag4 = false;
					}
					cxFastListNode = cxFastListNode.Next();
					TSVector2 tSVector3 = cxFastListNode.Elem();
					cxFastListNode = cxFastListNode.Next();
					bool flag6 = cxFastListNode == points.End();
					if (flag6)
					{
						cxFastListNode = points.Begin();
					}
					TSVector2 value3 = cxFastListNode.Elem();
					TSVector2 value4 = cxFastListNode4.Elem();
					TSVector2 a2 = tSVector3 - value4;
					TSVector2 b2 = value3 - tSVector3;
					FP fP2 = MarchingSquares.VecCross(a2, b2);
					bool flag7 = fP2 * fP2 < Settings.Epsilon;
					if (flag7)
					{
						points.Erase(cxFastListNode4, cxFastListNode4.Next());
						polya.Length--;
					}
					break;
				}
				cxFastListNode3 = cxFastListNode;
				cxFastListNode = cxFastListNode.Next();
			}
		}
	}
}
