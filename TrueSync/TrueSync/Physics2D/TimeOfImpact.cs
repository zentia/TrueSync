namespace TrueSync.Physics2D
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using TrueSync;

    public static class TimeOfImpact
    {
        [ThreadStatic]
        private static DistanceInput _distanceInput;
        [ThreadStatic]
        public static int TOICalls;
        [ThreadStatic]
        public static int TOIIters;
        [ThreadStatic]
        public static int TOIMaxIters;
        [ThreadStatic]
        public static int TOIMaxRootIters;
        [ThreadStatic]
        public static int TOIRootIters;

        public static void CalculateTimeOfImpact(out TOIOutput output, TOIInput input)
        {
            Transform transform;
            Transform transform2;
            DistanceOutput output2;
            SimplexCache cache;
            output = new TOIOutput();
            output.State = TOIOutputState.Unknown;
            output.T = input.TMax;
            Sweep sweepA = input.SweepA;
            Sweep sweepB = input.SweepB;
            sweepA.Normalize();
            sweepB.Normalize();
            FP tMax = input.TMax;
            FP fp2 = input.ProxyA.Radius + input.ProxyB.Radius;
            FP fp3 = TSMath.Max(Settings.LinearSlop, fp2 - (3f * Settings.LinearSlop));
            FP fp4 = 0.25f * Settings.LinearSlop;
            Debug.Assert(fp3 > fp4);
            FP beta = 0f;
            int num = 0;
            _distanceInput = _distanceInput ?? new DistanceInput();
            _distanceInput.ProxyA = input.ProxyA;
            _distanceInput.ProxyB = input.ProxyB;
            _distanceInput.UseRadii = false;
        Label_00FE:
            sweepA.GetTransform(out transform, beta);
            sweepB.GetTransform(out transform2, beta);
            _distanceInput.TransformA = transform;
            _distanceInput.TransformB = transform2;
            Distance.ComputeDistance(out output2, out cache, _distanceInput);
            if (output2.Distance <= 0f)
            {
                output.State = TOIOutputState.Overlapped;
                output.T = 0f;
            }
            else if (output2.Distance < (fp3 + fp4))
            {
                output.State = TOIOutputState.Touching;
                output.T = beta;
            }
            else
            {
                SeparationFunction.Set(ref cache, input.ProxyA, ref sweepA, input.ProxyB, ref sweepB, beta);
                bool flag2 = false;
                FP t = tMax;
                int num2 = 0;
                do
                {
                    int num3;
                    int num4;
                    FP fp7 = SeparationFunction.FindMinSeparation(out num3, out num4, t);
                    if (fp7 > (fp3 + fp4))
                    {
                        output.State = TOIOutputState.Seperated;
                        output.T = tMax;
                        flag2 = true;
                        break;
                    }
                    if (fp7 > (fp3 - fp4))
                    {
                        beta = t;
                        break;
                    }
                    FP fp8 = SeparationFunction.Evaluate(num3, num4, beta);
                    if (fp8 < (fp3 - fp4))
                    {
                        output.State = TOIOutputState.Failed;
                        output.T = beta;
                        flag2 = true;
                        break;
                    }
                    if (fp8 <= (fp3 + fp4))
                    {
                        output.State = TOIOutputState.Touching;
                        output.T = beta;
                        flag2 = true;
                        break;
                    }
                    int num5 = 0;
                    FP fp9 = beta;
                    FP fp10 = t;
                    do
                    {
                        FP fp11;
                        if ((num5 & 1) > 0)
                        {
                            fp11 = fp9 + (((fp3 - fp8) * (fp10 - fp9)) / (fp7 - fp8));
                        }
                        else
                        {
                            fp11 = 0.5f * (fp9 + fp10);
                        }
                        num5++;
                        FP fp12 = SeparationFunction.Evaluate(num3, num4, fp11);
                        if (FP.Abs(fp12 - fp3) < fp4)
                        {
                            t = fp11;
                            break;
                        }
                        if (fp12 > fp3)
                        {
                            fp9 = fp11;
                            fp8 = fp12;
                        }
                        else
                        {
                            fp10 = fp11;
                            fp7 = fp12;
                        }
                    }
                    while (num5 != 50);
                    num2++;
                }
                while (num2 != Settings.MaxPolygonVertices);
                num++;
                if (!flag2)
                {
                    if (num == 20)
                    {
                        output.State = TOIOutputState.Failed;
                        output.T = beta;
                    }
                    else
                    {
                        goto Label_00FE;
                    }
                }
            }
        }
    }
}

