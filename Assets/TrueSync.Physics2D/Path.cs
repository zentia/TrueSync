using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace TrueSync.Physics2D
{
	public class Path
	{
		public List<TSVector2> ControlPoints;

		private FP _deltaT;

		public bool Closed
		{
			get;
			set;
		}

		public Path()
		{
			this.ControlPoints = new List<TSVector2>();
		}

		public Path(TSVector2[] vertices)
		{
			this.ControlPoints = new List<TSVector2>(vertices.Length);
			for (int i = 0; i < vertices.Length; i++)
			{
				this.Add(vertices[i]);
			}
		}

		public Path(IList<TSVector2> vertices)
		{
			this.ControlPoints = new List<TSVector2>(vertices.Count);
			for (int i = 0; i < vertices.Count; i++)
			{
				this.Add(vertices[i]);
			}
		}

		public int NextIndex(int index)
		{
			bool flag = index == this.ControlPoints.Count - 1;
			int result;
			if (flag)
			{
				result = 0;
			}
			else
			{
				result = index + 1;
			}
			return result;
		}

		public int PreviousIndex(int index)
		{
			bool flag = index == 0;
			int result;
			if (flag)
			{
				result = this.ControlPoints.Count - 1;
			}
			else
			{
				result = index - 1;
			}
			return result;
		}

		public void Translate(ref TSVector2 vector)
		{
			for (int i = 0; i < this.ControlPoints.Count; i++)
			{
				this.ControlPoints[i] = TSVector2.Add(this.ControlPoints[i], vector);
			}
		}

		public void Scale(ref TSVector2 value)
		{
			for (int i = 0; i < this.ControlPoints.Count; i++)
			{
				this.ControlPoints[i] = TSVector2.Multiply(this.ControlPoints[i], value);
			}
		}

		public void Rotate(FP value)
		{
			Matrix matrix;
			Matrix.CreateRotationZ(value, out matrix);
			for (int i = 0; i < this.ControlPoints.Count; i++)
			{
				this.ControlPoints[i] = TSVector2.Transform(this.ControlPoints[i], matrix);
			}
		}

		public override string ToString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			for (int i = 0; i < this.ControlPoints.Count; i++)
			{
				stringBuilder.Append(this.ControlPoints[i].ToString());
				bool flag = i < this.ControlPoints.Count - 1;
				if (flag)
				{
					stringBuilder.Append(" ");
				}
			}
			return stringBuilder.ToString();
		}

		public Vertices GetVertices(int divisions)
		{
			Vertices vertices = new Vertices();
			FP y = 1f / (float)divisions;
			FP fP = 0;
			while (fP < 1f)
			{
				vertices.Add(this.GetPosition(fP));
				fP += y;
			}
			return vertices;
		}

		public TSVector2 GetPosition(FP time)
		{
			bool flag = this.ControlPoints.Count < 2;
			if (flag)
			{
				throw new Exception("You need at least 2 control points to calculate a position.");
			}
			bool closed = this.Closed;
			TSVector2 result;
			if (closed)
			{
				this.Add(this.ControlPoints[0]);
				this._deltaT = 1f / (float)(this.ControlPoints.Count - 1);
				int num = (int)((long)(time / this._deltaT));
				int num2 = num - 1;
				bool flag2 = num2 < 0;
				if (flag2)
				{
					num2 += this.ControlPoints.Count - 1;
				}
				else
				{
					bool flag3 = num2 >= this.ControlPoints.Count - 1;
					if (flag3)
					{
						num2 -= this.ControlPoints.Count - 1;
					}
				}
				int num3 = num;
				bool flag4 = num3 < 0;
				if (flag4)
				{
					num3 += this.ControlPoints.Count - 1;
				}
				else
				{
					bool flag5 = num3 >= this.ControlPoints.Count - 1;
					if (flag5)
					{
						num3 -= this.ControlPoints.Count - 1;
					}
				}
				int num4 = num + 1;
				bool flag6 = num4 < 0;
				if (flag6)
				{
					num4 += this.ControlPoints.Count - 1;
				}
				else
				{
					bool flag7 = num4 >= this.ControlPoints.Count - 1;
					if (flag7)
					{
						num4 -= this.ControlPoints.Count - 1;
					}
				}
				int num5 = num + 2;
				bool flag8 = num5 < 0;
				if (flag8)
				{
					num5 += this.ControlPoints.Count - 1;
				}
				else
				{
					bool flag9 = num5 >= this.ControlPoints.Count - 1;
					if (flag9)
					{
						num5 -= this.ControlPoints.Count - 1;
					}
				}
				FP amount = (time - this._deltaT * num) / this._deltaT;
				result = TSVector2.CatmullRom(this.ControlPoints[num2], this.ControlPoints[num3], this.ControlPoints[num4], this.ControlPoints[num5], amount);
				this.RemoveAt(this.ControlPoints.Count - 1);
			}
			else
			{
				int num6 = (int)((long)(time / this._deltaT));
				int num7 = num6 - 1;
				bool flag10 = num7 < 0;
				if (flag10)
				{
					num7 = 0;
				}
				else
				{
					bool flag11 = num7 >= this.ControlPoints.Count - 1;
					if (flag11)
					{
						num7 = this.ControlPoints.Count - 1;
					}
				}
				int num8 = num6;
				bool flag12 = num8 < 0;
				if (flag12)
				{
					num8 = 0;
				}
				else
				{
					bool flag13 = num8 >= this.ControlPoints.Count - 1;
					if (flag13)
					{
						num8 = this.ControlPoints.Count - 1;
					}
				}
				int num9 = num6 + 1;
				bool flag14 = num9 < 0;
				if (flag14)
				{
					num9 = 0;
				}
				else
				{
					bool flag15 = num9 >= this.ControlPoints.Count - 1;
					if (flag15)
					{
						num9 = this.ControlPoints.Count - 1;
					}
				}
				int num10 = num6 + 2;
				bool flag16 = num10 < 0;
				if (flag16)
				{
					num10 = 0;
				}
				else
				{
					bool flag17 = num10 >= this.ControlPoints.Count - 1;
					if (flag17)
					{
						num10 = this.ControlPoints.Count - 1;
					}
				}
				FP amount2 = (time - this._deltaT * num6) / this._deltaT;
				result = TSVector2.CatmullRom(this.ControlPoints[num7], this.ControlPoints[num8], this.ControlPoints[num9], this.ControlPoints[num10], amount2);
			}
			return result;
		}

		public TSVector2 GetPositionNormal(FP time)
		{
			FP time2 = time + 0.0001f;
			TSVector2 position = this.GetPosition(time);
			TSVector2 position2 = this.GetPosition(time2);
			TSVector2 tSVector;
			TSVector2.Subtract(ref position, ref position2, out tSVector);
			TSVector2 result;
			result.x = -tSVector.y;
			result.y = tSVector.x;
			TSVector2.Normalize(ref result, out result);
			return result;
		}

		public void Add(TSVector2 point)
		{
			this.ControlPoints.Add(point);
			this._deltaT = 1f / (float)(this.ControlPoints.Count - 1);
		}

		public void Remove(TSVector2 point)
		{
			this.ControlPoints.Remove(point);
			this._deltaT = 1f / (float)(this.ControlPoints.Count - 1);
		}

		public void RemoveAt(int index)
		{
			this.ControlPoints.RemoveAt(index);
			this._deltaT = 1f / (float)(this.ControlPoints.Count - 1);
		}

		public FP GetLength()
		{
			List<TSVector2> vertices = this.GetVertices(this.ControlPoints.Count * 25);
			FP fP = 0;
			for (int i = 1; i < vertices.Count; i++)
			{
				fP += TSVector2.Distance(vertices[i - 1], vertices[i]);
			}
			bool closed = this.Closed;
			if (closed)
			{
				fP += TSVector2.Distance(vertices[this.ControlPoints.Count - 1], vertices[0]);
			}
			return fP;
		}

		public List<Vector3> SubdivideEvenly(int divisions)
		{
			List<Vector3> list = new List<Vector3>();
			FP length = this.GetLength();
			FP x = length / divisions + 0.001f;
			FP fP = 0f;
			TSVector2 value = this.ControlPoints[0];
			TSVector2 position = this.GetPosition(fP);
			while (x * 0.5f >= TSVector2.Distance(value, position))
			{
				position = this.GetPosition(fP);
				fP += 0.0001f;
				bool flag = fP >= 1f;
				if (flag)
				{
					break;
				}
			}
			value = position;
			for (int i = 1; i < divisions; i++)
			{
				TSVector2 positionNormal = this.GetPositionNormal(fP);
				FP z = FP.Atan2(positionNormal.y, positionNormal.x);
				list.Add(new Vector3(position, z));
				while (x >= TSVector2.Distance(value, position))
				{
					position = this.GetPosition(fP);
					fP += 1E-05f;
					bool flag2 = fP >= 1f;
					if (flag2)
					{
						break;
					}
				}
				bool flag3 = fP >= 1f;
				if (flag3)
				{
					break;
				}
				value = position;
			}
			return list;
		}
	}
}
