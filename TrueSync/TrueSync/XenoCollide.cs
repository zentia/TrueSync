namespace TrueSync
{
    using System;
    using System.Runtime.InteropServices;
    using UnityEngine;

    public sealed class XenoCollide
    {
        private static FP CollideEpsilon = FP.EN4;
        private const int MaximumIterations = 0x22;

        public static bool Detect(ISupportMappable support1, ISupportMappable support2, ref TSMatrix orientation1, ref TSMatrix orientation2, ref TSVector position1, ref TSVector position2, out TSVector point, out TSVector normal, out FP penetration)
        {
            TSVector vector;
            TSVector vector2;
            TSVector vector3;
            TSVector vector4;
            TSVector vector5;
            TSVector vector6;
            TSVector vector7;
            TSVector vector8;
            TSVector vector9;
            TSVector vector10;
            TSVector vector11;
            TSVector zero = TSVector.zero;
            TSVector result = TSVector.zero;
            TSVector vector17 = TSVector.zero;
            point = normal = TSVector.zero;
            penetration = FP.Zero;
            support1.SupportCenter(out vector3);
            TSVector.Transform(ref vector3, ref orientation1, out vector3);
            TSVector.Add(ref position1, ref vector3, out vector3);
            support2.SupportCenter(out vector4);
            TSVector.Transform(ref vector4, ref orientation2, out vector4);
            TSVector.Add(ref position2, ref vector4, out vector4);
            TSVector.Subtract(ref vector4, ref vector3, out vector5);
            if (vector5.IsNearlyZero())
            {
                vector5 = new TSVector(FP.EN4, 0, 0);
            }
            TSVector direction = vector5;
            TSVector.Negate(ref vector5, out normal);
            SupportMapTransformed(support1, ref orientation1, ref position1, ref direction, out vector6);
            SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out vector7);
            TSVector.Subtract(ref vector7, ref vector6, out vector8);
            if (TSVector.Dot(ref vector8, ref normal) <= FP.Zero)
            {
                return false;
            }
            TSVector.Cross(ref vector8, ref vector5, out normal);
            if (normal.IsNearlyZero())
            {
                TSVector.Subtract(ref vector8, ref vector5, out normal);
                normal.Normalize();
                point = vector6;
                TSVector.Add(ref point, ref vector7, out point);
                TSVector.Multiply(ref point, FP.Half, out point);
                TSVector.Subtract(ref vector7, ref vector6, out vector);
                penetration = TSVector.Dot(ref vector, ref normal);
                return true;
            }
            TSVector.Negate(ref normal, out direction);
            SupportMapTransformed(support1, ref orientation1, ref position1, ref direction, out vector9);
            SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out vector10);
            TSVector.Subtract(ref vector10, ref vector9, out vector11);
            if (TSVector.Dot(ref vector11, ref normal) <= FP.Zero)
            {
                return false;
            }
            TSVector.Subtract(ref vector8, ref vector5, out vector);
            TSVector.Subtract(ref vector11, ref vector5, out vector2);
            TSVector.Cross(ref vector, ref vector2, out normal);
            if (TSVector.Dot(ref normal, ref vector5) > FP.Zero)
            {
                TSVector.Swap(ref vector8, ref vector11);
                TSVector.Swap(ref vector6, ref vector9);
                TSVector.Swap(ref vector7, ref vector10);
                TSVector.Negate(ref normal, out normal);
                Debug.Log("normal: " + ((TSVector) normal));
            }
            int num = 0;
            int num2 = 0;
            bool flag = false;
            while (true)
            {
                TSVector vector12;
                TSVector vector13;
                TSVector vector14;
                if (num2 > 0x22)
                {
                    return false;
                }
                num2++;
                TSVector.Negate(ref normal, out direction);
                SupportMapTransformed(support1, ref orientation1, ref position1, ref direction, out vector12);
                SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out vector13);
                TSVector.Subtract(ref vector13, ref vector12, out vector14);
                if (TSVector.Dot(ref vector14, ref normal) <= FP.Zero)
                {
                    return false;
                }
                TSVector.Cross(ref vector8, ref vector14, out vector);
                if (TSVector.Dot(ref vector, ref vector5) < FP.Zero)
                {
                    vector11 = vector14;
                    vector9 = vector12;
                    vector10 = vector13;
                    TSVector.Subtract(ref vector8, ref vector5, out vector);
                    TSVector.Subtract(ref vector14, ref vector5, out vector2);
                    TSVector.Cross(ref vector, ref vector2, out normal);
                }
                else
                {
                    TSVector.Cross(ref vector14, ref vector11, out vector);
                    if (TSVector.Dot(ref vector, ref vector5) >= FP.Zero)
                    {
                        while (true)
                        {
                            num++;
                            TSVector.Subtract(ref vector11, ref vector8, out vector);
                            TSVector.Subtract(ref vector14, ref vector8, out vector2);
                            TSVector.Cross(ref vector, ref vector2, out normal);
                            if (normal.IsNearlyZero())
                            {
                                return true;
                            }
                            normal.Normalize();
                            if ((TSVector.Dot(ref normal, ref vector8) >= 0) && !flag)
                            {
                                flag = true;
                            }
                            TSVector.Negate(ref normal, out direction);
                            SupportMapTransformed(support1, ref orientation1, ref position1, ref direction, out zero);
                            SupportMapTransformed(support2, ref orientation2, ref position2, ref normal, out result);
                            TSVector.Subtract(ref result, ref zero, out vector17);
                            TSVector.Subtract(ref vector17, ref vector14, out vector);
                            FP fp3 = TSVector.Dot(ref vector, ref normal);
                            penetration = TSVector.Dot(ref vector17, ref normal);
                            if (((fp3 <= CollideEpsilon) || (penetration <= FP.Zero)) || (num > 0x22))
                            {
                                if (flag)
                                {
                                    TSVector.Cross(ref vector8, ref vector11, out vector);
                                    FP scaleFactor = TSVector.Dot(ref vector, ref vector14);
                                    TSVector.Cross(ref vector14, ref vector11, out vector);
                                    FP fp6 = TSVector.Dot(ref vector, ref vector5);
                                    TSVector.Cross(ref vector5, ref vector8, out vector);
                                    FP fp7 = TSVector.Dot(ref vector, ref vector14);
                                    TSVector.Cross(ref vector11, ref vector8, out vector);
                                    FP fp8 = TSVector.Dot(ref vector, ref vector5);
                                    FP fp9 = ((scaleFactor + fp6) + fp7) + fp8;
                                    if (fp9 <= 0)
                                    {
                                        scaleFactor = 0;
                                        TSVector.Cross(ref vector11, ref vector14, out vector);
                                        fp6 = TSVector.Dot(ref vector, ref normal);
                                        TSVector.Cross(ref vector14, ref vector8, out vector);
                                        fp7 = TSVector.Dot(ref vector, ref normal);
                                        TSVector.Cross(ref vector8, ref vector11, out vector);
                                        fp8 = TSVector.Dot(ref vector, ref normal);
                                        fp9 = (fp6 + fp7) + fp8;
                                    }
                                    FP fp10 = FP.One / fp9;
                                    TSVector.Multiply(ref vector3, scaleFactor, out point);
                                    TSVector.Multiply(ref vector6, fp6, out vector);
                                    TSVector.Add(ref point, ref vector, out point);
                                    TSVector.Multiply(ref vector9, fp7, out vector);
                                    TSVector.Add(ref point, ref vector, out point);
                                    TSVector.Multiply(ref vector12, fp8, out vector);
                                    TSVector.Add(ref point, ref vector, out point);
                                    TSVector.Multiply(ref vector4, scaleFactor, out vector2);
                                    TSVector.Add(ref vector2, ref point, out point);
                                    TSVector.Multiply(ref vector7, fp6, out vector);
                                    TSVector.Add(ref point, ref vector, out point);
                                    TSVector.Multiply(ref vector10, fp7, out vector);
                                    TSVector.Add(ref point, ref vector, out point);
                                    TSVector.Multiply(ref vector13, fp8, out vector);
                                    TSVector.Add(ref point, ref vector, out point);
                                    TSVector.Multiply(ref point, fp10 * FP.Half, out point);
                                }
                                return flag;
                            }
                            TSVector.Cross(ref vector17, ref vector5, out vector);
                            if (TSVector.Dot(ref vector, ref vector8) >= FP.Zero)
                            {
                                if (TSVector.Dot(ref vector, ref vector11) >= FP.Zero)
                                {
                                    vector8 = vector17;
                                    vector6 = zero;
                                    vector7 = result;
                                }
                                else
                                {
                                    vector14 = vector17;
                                    vector12 = zero;
                                    vector13 = result;
                                }
                            }
                            else if (TSVector.Dot(ref vector, ref vector14) >= FP.Zero)
                            {
                                vector11 = vector17;
                                vector9 = zero;
                                vector10 = result;
                            }
                            else
                            {
                                vector8 = vector17;
                                vector6 = zero;
                                vector7 = result;
                            }
                        }
                    }
                    vector8 = vector14;
                    vector6 = vector12;
                    vector7 = vector13;
                    TSVector.Subtract(ref vector14, ref vector5, out vector);
                    TSVector.Subtract(ref vector11, ref vector5, out vector2);
                    TSVector.Cross(ref vector, ref vector2, out normal);
                }
            }
        }

        private static void SupportMapTransformed(ISupportMappable support, ref TSMatrix orientation, ref TSVector position, ref TSVector direction, out TSVector result)
        {
            result.x = ((direction.x * orientation.M11) + (direction.y * orientation.M12)) + (direction.z * orientation.M13);
            result.y = ((direction.x * orientation.M21) + (direction.y * orientation.M22)) + (direction.z * orientation.M23);
            result.z = ((direction.x * orientation.M31) + (direction.y * orientation.M32)) + (direction.z * orientation.M33);
            support.SupportMapping(ref result, out result);
            FP fp = ((result.x * orientation.M11) + (result.y * orientation.M21)) + (result.z * orientation.M31);
            FP fp2 = ((result.x * orientation.M12) + (result.y * orientation.M22)) + (result.z * orientation.M32);
            FP fp3 = ((result.x * orientation.M13) + (result.y * orientation.M23)) + (result.z * orientation.M33);
            result.x = position.x + fp;
            result.y = position.y + fp2;
            result.z = position.z + fp3;
        }
    }
}

