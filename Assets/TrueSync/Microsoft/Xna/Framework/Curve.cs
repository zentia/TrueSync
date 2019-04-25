namespace Microsoft.Xna.Framework
{
    using System;
    using TrueSync;

    public class Curve
    {
        private CurveKeyCollection keys = new CurveKeyCollection();
        private CurveLoopType postLoop;
        private CurveLoopType preLoop;

        public Curve Clone()
        {
            return new Curve { keys = this.keys.Clone(), preLoop = this.preLoop, postLoop = this.postLoop };
        }

        public void ComputeTangent(int keyIndex, CurveTangent tangentType)
        {
            this.ComputeTangent(keyIndex, tangentType, tangentType);
        }

        public void ComputeTangent(int keyIndex, CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            throw new NotImplementedException();
        }

        public void ComputeTangents(CurveTangent tangentType)
        {
            this.ComputeTangents(tangentType, tangentType);
        }

        public void ComputeTangents(CurveTangent tangentInType, CurveTangent tangentOutType)
        {
            throw new NotImplementedException();
        }

        public FP Evaluate(FP position)
        {
            CurveKey key = this.keys[0];
            CurveKey key2 = this.keys[this.keys.Count - 1];
            if (position >= key.Position)
            {
                if (position > key2.Position)
                {
                    int numberOfCycle;
                    FP fp3;
                    switch (this.PostLoop)
                    {
                        case CurveLoopType.Constant:
                            return key2.Value;

                        case CurveLoopType.Cycle:
                            numberOfCycle = this.GetNumberOfCycle(position);
                            fp3 = position - (numberOfCycle * (key2.Position - key.Position));
                            return this.GetCurvePosition(fp3);

                        case CurveLoopType.CycleOffset:
                            numberOfCycle = this.GetNumberOfCycle(position);
                            fp3 = position - (numberOfCycle * (key2.Position - key.Position));
                            return (this.GetCurvePosition(fp3) + (numberOfCycle * (key2.Value - key.Value)));

                        case CurveLoopType.Oscillate:
                            numberOfCycle = this.GetNumberOfCycle(position);
                            fp3 = position - (numberOfCycle * (key2.Position - key.Position));
                            if (!(0f == (((float) numberOfCycle) % 2f)))
                            {
                                fp3 = ((key2.Position - position) + key.Position) + (numberOfCycle * (key2.Position - key.Position));
                            }
                            else
                            {
                                fp3 = position - (numberOfCycle * (key2.Position - key.Position));
                            }
                            return this.GetCurvePosition(fp3);

                        case CurveLoopType.Linear:
                            return (key2.Value + (key.TangentOut * (position - key2.Position)));
                    }
                }
            }
            else
            {
                int num;
                FP fp;
                switch (this.PreLoop)
                {
                    case CurveLoopType.Constant:
                        return key.Value;

                    case CurveLoopType.Cycle:
                        num = this.GetNumberOfCycle(position);
                        fp = position - (num * (key2.Position - key.Position));
                        return this.GetCurvePosition(fp);

                    case CurveLoopType.CycleOffset:
                        num = this.GetNumberOfCycle(position);
                        fp = position - (num * (key2.Position - key.Position));
                        return (this.GetCurvePosition(fp) + (num * (key2.Value - key.Value)));

                    case CurveLoopType.Oscillate:
                        num = this.GetNumberOfCycle(position);
                        if (!(0f == (((float) num) % 2f)))
                        {
                            fp = ((key2.Position - position) + key.Position) + (num * (key2.Position - key.Position));
                            break;
                        }
                        fp = position - (num * (key2.Position - key.Position));
                        break;

                    case CurveLoopType.Linear:
                        return (key.Value - (key.TangentIn * (key.Position - position)));

                    default:
                        goto Label_038C;
                }
                return this.GetCurvePosition(fp);
            }
        Label_038C:
            return this.GetCurvePosition(position);
        }

        private FP GetCurvePosition(FP position)
        {
            CurveKey key = this.keys[0];
            for (int i = 1; i < this.keys.Count; i++)
            {
                CurveKey key2 = this.Keys[i];
                if (key2.Position >= position)
                {
                    if (key.Continuity == CurveContinuity.Step)
                    {
                        if (position >= 1f)
                        {
                            return key2.Value;
                        }
                        return key.Value;
                    }
                    FP fp = (position - key.Position) / (key2.Position - key.Position);
                    FP fp2 = fp * fp;
                    FP fp3 = fp2 * fp;
                    return (((((((2 * fp3) - (3 * fp2)) + 1f) * key.Value) + (((fp3 - (2 * fp2)) + fp) * key.TangentOut)) + (((3 * fp2) - (2 * fp3)) * key2.Value)) + ((fp3 - fp2) * key2.TangentIn));
                }
                key = key2;
            }
            return 0f;
        }

        private int GetNumberOfCycle(FP position)
        {
            FP fp = (position - this.keys[0].Position) / (this.keys[this.keys.Count - 1].Position - this.keys[0].Position);
            if (fp < 0f)
            {
                fp -= 1;
            }
            return (int) ((long) fp);
        }

        public bool IsConstant
        {
            get
            {
                return (this.keys.Count <= 1);
            }
        }

        public CurveKeyCollection Keys
        {
            get
            {
                return this.keys;
            }
        }

        public CurveLoopType PostLoop
        {
            get
            {
                return this.postLoop;
            }
            set
            {
                this.postLoop = value;
            }
        }

        public CurveLoopType PreLoop
        {
            get
            {
                return this.preLoop;
            }
            set
            {
                this.preLoop = value;
            }
        }
    }
}

