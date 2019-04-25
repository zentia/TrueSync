// Decompiled with JetBrains decompiler
// Type: TrueSync.Physics2D.Path
// Assembly: TrueSync, Version=0.1.0.6, Culture=neutral, PublicKeyToken=null
// MVID: 11931B28-7678-4A75-941C-C3C4D965272F
// Assembly location: D:\Photon-TrueSync-Experiments\Assets\Plugins\TrueSync.dll

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

        public Path()
        {
            this.ControlPoints = new List<TSVector2>();
        }

        public Path(TSVector2[] vertices)
        {
            this.ControlPoints = new List<TSVector2>(vertices.Length);
            for (int index = 0; index < vertices.Length; ++index)
                this.Add(vertices[index]);
        }

        public Path(IList<TSVector2> vertices)
        {
            this.ControlPoints = new List<TSVector2>(vertices.Count);
            for (int index = 0; index < vertices.Count; ++index)
                this.Add(vertices[index]);
        }

        public bool Closed { get; set; }

        public int NextIndex(int index)
        {
            if (index == this.ControlPoints.Count - 1)
                return 0;
            return index + 1;
        }

        public int PreviousIndex(int index)
        {
            if (index == 0)
                return this.ControlPoints.Count - 1;
            return index - 1;
        }

        public void Translate(ref TSVector2 vector)
        {
            for (int index = 0; index < this.ControlPoints.Count; ++index)
                this.ControlPoints[index] = TSVector2.Add(this.ControlPoints[index], vector);
        }

        public void Scale(ref TSVector2 value)
        {
            for (int index = 0; index < this.ControlPoints.Count; ++index)
                this.ControlPoints[index] = TSVector2.Multiply(this.ControlPoints[index], value);
        }

        public void Rotate(FP value)
        {
            Matrix result;
            Matrix.CreateRotationZ(value, out result);
            for (int index = 0; index < this.ControlPoints.Count; ++index)
                this.ControlPoints[index] = TSVector2.Transform(this.ControlPoints[index], result);
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            for (int index = 0; index < this.ControlPoints.Count; ++index)
            {
                stringBuilder.Append(this.ControlPoints[index].ToString());
                if (index < this.ControlPoints.Count - 1)
                    stringBuilder.Append(" ");
            }
            return stringBuilder.ToString();
        }

        public Vertices GetVertices(int divisions)
        {
            Vertices vertices = new Vertices();
            FP fp = (FP)(1f / (float)divisions);
            FP time = (FP)0;
            while (time < (FP)1f)
            {
                vertices.Add(this.GetPosition(time));
                time += fp;
            }
            return vertices;
        }

        public TSVector2 GetPosition(FP time)
        {
            if (this.ControlPoints.Count < 2)
                throw new Exception("You need at least 2 control points to calculate a position.");
            TSVector2 tsVector2;
            if (this.Closed)
            {
                this.Add(this.ControlPoints[0]);
                this._deltaT = (FP)(1f / (float)(this.ControlPoints.Count - 1));
                int num = (int)(long)(time / this._deltaT);
                int index1 = num - 1;
                if (index1 < 0)
                    index1 += this.ControlPoints.Count - 1;
                else if (index1 >= this.ControlPoints.Count - 1)
                    index1 -= this.ControlPoints.Count - 1;
                int index2 = num;
                if (index2 < 0)
                    index2 += this.ControlPoints.Count - 1;
                else if (index2 >= this.ControlPoints.Count - 1)
                    index2 -= this.ControlPoints.Count - 1;
                int index3 = num + 1;
                if (index3 < 0)
                    index3 += this.ControlPoints.Count - 1;
                else if (index3 >= this.ControlPoints.Count - 1)
                    index3 -= this.ControlPoints.Count - 1;
                int index4 = num + 2;
                if (index4 < 0)
                    index4 += this.ControlPoints.Count - 1;
                else if (index4 >= this.ControlPoints.Count - 1)
                    index4 -= this.ControlPoints.Count - 1;
                FP amount = (time - this._deltaT * (FP)num) / this._deltaT;
                tsVector2 = TSVector2.CatmullRom(this.ControlPoints[index1], this.ControlPoints[index2], this.ControlPoints[index3], this.ControlPoints[index4], amount);
                this.RemoveAt(this.ControlPoints.Count - 1);
            }
            else
            {
                int num = (int)(long)(time / this._deltaT);
                int index1 = num - 1;
                if (index1 < 0)
                    index1 = 0;
                else if (index1 >= this.ControlPoints.Count - 1)
                    index1 = this.ControlPoints.Count - 1;
                int index2 = num;
                if (index2 < 0)
                    index2 = 0;
                else if (index2 >= this.ControlPoints.Count - 1)
                    index2 = this.ControlPoints.Count - 1;
                int index3 = num + 1;
                if (index3 < 0)
                    index3 = 0;
                else if (index3 >= this.ControlPoints.Count - 1)
                    index3 = this.ControlPoints.Count - 1;
                int index4 = num + 2;
                if (index4 < 0)
                    index4 = 0;
                else if (index4 >= this.ControlPoints.Count - 1)
                    index4 = this.ControlPoints.Count - 1;
                FP amount = (time - this._deltaT * (FP)num) / this._deltaT;
                tsVector2 = TSVector2.CatmullRom(this.ControlPoints[index1], this.ControlPoints[index2], this.ControlPoints[index3], this.ControlPoints[index4], amount);
            }
            return tsVector2;
        }

        public TSVector2 GetPositionNormal(FP time)
        {
            FP time1 = time + (FP)0.0001f;
            TSVector2 position1 = this.GetPosition(time);
            TSVector2 position2 = this.GetPosition(time1);
            TSVector2 result1;
            TSVector2.Subtract(ref position1, ref position2, out result1);
            TSVector2 result2;
            result2.x = -result1.y;
            result2.y = result1.x;
            TSVector2.Normalize(ref result2, out result2);
            return result2;
        }

        public void Add(TSVector2 point)
        {
            this.ControlPoints.Add(point);
            this._deltaT = (FP)(1f / (float)(this.ControlPoints.Count - 1));
        }

        public void Remove(TSVector2 point)
        {
            this.ControlPoints.Remove(point);
            this._deltaT = (FP)(1f / (float)(this.ControlPoints.Count - 1));
        }

        public void RemoveAt(int index)
        {
            this.ControlPoints.RemoveAt(index);
            this._deltaT = (FP)(1f / (float)(this.ControlPoints.Count - 1));
        }

        public FP GetLength()
        {
            List<TSVector2> vertices = (List<TSVector2>)this.GetVertices(this.ControlPoints.Count * 25);
            FP fp = (FP)0;
            for (int index = 1; index < vertices.Count; ++index)
                fp += TSVector2.Distance(vertices[index - 1], vertices[index]);
            if (this.Closed)
                fp += TSVector2.Distance(vertices[this.ControlPoints.Count - 1], vertices[0]);
            return fp;
        }

        public List<Vector3> SubdivideEvenly(int divisions)
        {
            List<Vector3> vector3List = new List<Vector3>();
            FP fp = this.GetLength() / (FP)divisions + (FP)(1f / 1000f);
            FP time = (FP)0.0f;
            TSVector2 controlPoint = this.ControlPoints[0];
            TSVector2 position = this.GetPosition(time);
            while (fp * (FP)0.5f >= TSVector2.Distance(controlPoint, position))
            {
                position = this.GetPosition(time);
                time += (FP)0.0001f;
                if (time >= (FP)1f)
                    break;
            }
            TSVector2 tsVector2 = position;
            for (int index = 1; index < divisions; ++index)
            {
                TSVector2 positionNormal = this.GetPositionNormal(time);
                FP z = FP.Atan2(positionNormal.y, positionNormal.x);
                vector3List.Add(new Vector3(position, z));
                while (fp >= TSVector2.Distance(tsVector2, position))
                {
                    position = this.GetPosition(time);
                    time += (FP)1E-05f;
                    if (time >= (FP)1f)
                        break;
                }
                if (!(time >= (FP)1f))
                    tsVector2 = position;
                else
                    break;
            }
            return vector3List;
        }
    }
}
