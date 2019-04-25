namespace TrueSync.Physics2D
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using TrueSync;

    public class Path
    {
        private FP _deltaT;
        [CompilerGenerated, DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private bool <Closed>k__BackingField;
        public List<TSVector2> ControlPoints;

        public Path()
        {
            this.ControlPoints = new List<TSVector2>();
        }

        public Path(IList<TSVector2> vertices)
        {
            this.ControlPoints = new List<TSVector2>(vertices.Count);
            for (int i = 0; i < vertices.Count; i++)
            {
                this.Add(vertices[i]);
            }
        }

        public Path(TSVector2[] vertices)
        {
            this.ControlPoints = new List<TSVector2>(vertices.Length);
            for (int i = 0; i < vertices.Length; i++)
            {
                this.Add(vertices[i]);
            }
        }

        public void Add(TSVector2 point)
        {
            this.ControlPoints.Add(point);
            this._deltaT = 1f / ((float) (this.ControlPoints.Count - 1));
        }

        public FP GetLength()
        {
            List<TSVector2> vertices = this.GetVertices(this.ControlPoints.Count * 0x19);
            FP fp = 0;
            for (int i = 1; i < vertices.Count; i++)
            {
                fp += TSVector2.Distance(vertices[i - 1], vertices[i]);
            }
            if (this.Closed)
            {
                fp += TSVector2.Distance(vertices[this.ControlPoints.Count - 1], vertices[0]);
            }
            return fp;
        }

        public TSVector2 GetPosition(FP time)
        {
            if (this.ControlPoints.Count < 2)
            {
                throw new Exception("You need at least 2 control points to calculate a position.");
            }
            if (this.Closed)
            {
                this.Add(this.ControlPoints[0]);
                this._deltaT = 1f / ((float) (this.ControlPoints.Count - 1));
                int num = (int) ((long) (time / this._deltaT));
                int num2 = num - 1;
                if (num2 < 0)
                {
                    num2 += this.ControlPoints.Count - 1;
                }
                else if (num2 >= (this.ControlPoints.Count - 1))
                {
                    num2 -= this.ControlPoints.Count - 1;
                }
                int num3 = num;
                if (num3 < 0)
                {
                    num3 += this.ControlPoints.Count - 1;
                }
                else if (num3 >= (this.ControlPoints.Count - 1))
                {
                    num3 -= this.ControlPoints.Count - 1;
                }
                int num4 = num + 1;
                if (num4 < 0)
                {
                    num4 += this.ControlPoints.Count - 1;
                }
                else if (num4 >= (this.ControlPoints.Count - 1))
                {
                    num4 -= this.ControlPoints.Count - 1;
                }
                int num5 = num + 2;
                if (num5 < 0)
                {
                    num5 += this.ControlPoints.Count - 1;
                }
                else if (num5 >= (this.ControlPoints.Count - 1))
                {
                    num5 -= this.ControlPoints.Count - 1;
                }
                FP fp = (time - (this._deltaT * num)) / this._deltaT;
                TSVector2 vector = TSVector2.CatmullRom(this.ControlPoints[num2], this.ControlPoints[num3], this.ControlPoints[num4], this.ControlPoints[num5], fp);
                this.RemoveAt(this.ControlPoints.Count - 1);
                return vector;
            }
            int num6 = (int) ((long) (time / this._deltaT));
            int num7 = num6 - 1;
            if (num7 < 0)
            {
                num7 = 0;
            }
            else if (num7 >= (this.ControlPoints.Count - 1))
            {
                num7 = this.ControlPoints.Count - 1;
            }
            int num8 = num6;
            if (num8 < 0)
            {
                num8 = 0;
            }
            else if (num8 >= (this.ControlPoints.Count - 1))
            {
                num8 = this.ControlPoints.Count - 1;
            }
            int num9 = num6 + 1;
            if (num9 < 0)
            {
                num9 = 0;
            }
            else if (num9 >= (this.ControlPoints.Count - 1))
            {
                num9 = this.ControlPoints.Count - 1;
            }
            int num10 = num6 + 2;
            if (num10 < 0)
            {
                num10 = 0;
            }
            else if (num10 >= (this.ControlPoints.Count - 1))
            {
                num10 = this.ControlPoints.Count - 1;
            }
            FP amount = (time - (this._deltaT * num6)) / this._deltaT;
            return TSVector2.CatmullRom(this.ControlPoints[num7], this.ControlPoints[num8], this.ControlPoints[num9], this.ControlPoints[num10], amount);
        }

        public TSVector2 GetPositionNormal(FP time)
        {
            TSVector2 vector3;
            TSVector2 vector4;
            FP fp = time + 0.0001f;
            TSVector2 position = this.GetPosition(time);
            TSVector2 vector2 = this.GetPosition(fp);
            TSVector2.Subtract(ref position, ref vector2, out vector4);
            vector3.x = -vector4.y;
            vector3.y = vector4.x;
            TSVector2.Normalize(ref vector3, out vector3);
            return vector3;
        }

        public Vertices GetVertices(int divisions)
        {
            Vertices vertices = new Vertices();
            FP fp = 1f / ((float) divisions);
            for (FP fp2 = 0; fp2 < 1f; fp2 += fp)
            {
                vertices.Add(this.GetPosition(fp2));
            }
            return vertices;
        }

        public int NextIndex(int index)
        {
            if (index == (this.ControlPoints.Count - 1))
            {
                return 0;
            }
            return (index + 1);
        }

        public int PreviousIndex(int index)
        {
            if (index == 0)
            {
                return (this.ControlPoints.Count - 1);
            }
            return (index - 1);
        }

        public void Remove(TSVector2 point)
        {
            this.ControlPoints.Remove(point);
            this._deltaT = 1f / ((float) (this.ControlPoints.Count - 1));
        }

        public void RemoveAt(int index)
        {
            this.ControlPoints.RemoveAt(index);
            this._deltaT = 1f / ((float) (this.ControlPoints.Count - 1));
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

        public void Scale(ref TSVector2 value)
        {
            for (int i = 0; i < this.ControlPoints.Count; i++)
            {
                this.ControlPoints[i] = TSVector2.Multiply(this.ControlPoints[i], value);
            }
        }

        public List<Vector3> SubdivideEvenly(int divisions)
        {
            List<Vector3> list = new List<Vector3>();
            FP fp2 = (this.GetLength() / divisions) + 0.001f;
            FP time = 0f;
            TSVector2 vector = this.ControlPoints[0];
            TSVector2 position = this.GetPosition(time);
            while ((fp2 * 0.5f) >= TSVector2.Distance(vector, position))
            {
                position = this.GetPosition(time);
                time += 0.0001f;
                if (time >= 1f)
                {
                    break;
                }
            }
            vector = position;
            for (int i = 1; i < divisions; i++)
            {
                TSVector2 positionNormal = this.GetPositionNormal(time);
                FP z = FP.Atan2(positionNormal.y, positionNormal.x);
                list.Add(new Vector3(position, z));
                while (fp2 >= TSVector2.Distance(vector, position))
                {
                    position = this.GetPosition(time);
                    time += 1E-05f;
                    if (time >= 1f)
                    {
                        break;
                    }
                }
                if (time >= 1f)
                {
                    return list;
                }
                vector = position;
            }
            return list;
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < this.ControlPoints.Count; i++)
            {
                builder.Append(this.ControlPoints[i].ToString());
                if (i < (this.ControlPoints.Count - 1))
                {
                    builder.Append(" ");
                }
            }
            return builder.ToString();
        }

        public void Translate(ref TSVector2 vector)
        {
            for (int i = 0; i < this.ControlPoints.Count; i++)
            {
                this.ControlPoints[i] = TSVector2.Add(this.ControlPoints[i], vector);
            }
        }

        public bool Closed { get; set; }
    }
}

