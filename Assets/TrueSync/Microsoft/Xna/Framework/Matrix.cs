namespace Microsoft.Xna.Framework
{
    using System;
    using System.Runtime.InteropServices;
    using TrueSync;

    [StructLayout(LayoutKind.Sequential)]
    public struct Matrix : IEquatable<Matrix>
    {
        public FP M11;
        public FP M12;
        public FP M13;
        public FP M14;
        public FP M21;
        public FP M22;
        public FP M23;
        public FP M24;
        public FP M31;
        public FP M32;
        public FP M33;
        public FP M34;
        public FP M41;
        public FP M42;
        public FP M43;
        public FP M44;
        private static Matrix identity;
        public static Matrix Identity
        {
            get
            {
                return identity;
            }
        }
        public Vector3 Backward
        {
            get
            {
                return new Vector3(this.M31, this.M32, this.M33);
            }
            set
            {
                this.M31 = value.X;
                this.M32 = value.Y;
                this.M33 = value.Z;
            }
        }
        public Vector3 Down
        {
            get
            {
                return new Vector3(-this.M21, -this.M22, -this.M23);
            }
            set
            {
                this.M21 = -value.X;
                this.M22 = -value.Y;
                this.M23 = -value.Z;
            }
        }
        public Vector3 Forward
        {
            get
            {
                return new Vector3(-this.M31, -this.M32, -this.M33);
            }
            set
            {
                this.M31 = -value.X;
                this.M32 = -value.Y;
                this.M33 = -value.Z;
            }
        }
        public Vector3 Left
        {
            get
            {
                return new Vector3(-this.M11, -this.M12, -this.M13);
            }
            set
            {
                this.M11 = -value.X;
                this.M12 = -value.Y;
                this.M13 = -value.Z;
            }
        }
        public Vector3 Right
        {
            get
            {
                return new Vector3(this.M11, this.M12, this.M13);
            }
            set
            {
                this.M11 = value.X;
                this.M12 = value.Y;
                this.M13 = value.Z;
            }
        }
        public Vector3 Translation
        {
            get
            {
                return new Vector3(this.M41, this.M42, this.M43);
            }
            set
            {
                this.M41 = value.X;
                this.M42 = value.Y;
                this.M43 = value.Z;
            }
        }
        public Vector3 Up
        {
            get
            {
                return new Vector3(this.M21, this.M22, this.M23);
            }
            set
            {
                this.M21 = value.X;
                this.M22 = value.Y;
                this.M23 = value.Z;
            }
        }
        public Matrix(FP m11, FP m12, FP m13, FP m14, FP m21, FP m22, FP m23, FP m24, FP m31, FP m32, FP m33, FP m34, FP m41, FP m42, FP m43, FP m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        public static Matrix CreateWorld(Vector3 position, Vector3 forward, Vector3 up)
        {
            Matrix matrix;
            CreateWorld(ref position, ref forward, ref up, out matrix);
            return matrix;
        }

        public static void CreateWorld(ref Vector3 position, ref Vector3 forward, ref Vector3 up, out Matrix result)
        {
            Vector3 vector;
            Vector3 vector2;
            Vector3 vector3;
            Vector3.Normalize(ref forward, out vector3);
            Vector3.Cross(ref forward, ref up, out vector);
            Vector3.Cross(ref vector, ref forward, out vector2);
            vector.Normalize();
            vector2.Normalize();
            result = new Matrix();
            result.Right = vector;
            result.Up = vector2;
            result.Forward = vector3;
            result.Translation = position;
            result.M44 = 1f;
        }

        public static Matrix Add(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 += matrix2.M11;
            matrix1.M12 += matrix2.M12;
            matrix1.M13 += matrix2.M13;
            matrix1.M14 += matrix2.M14;
            matrix1.M21 += matrix2.M21;
            matrix1.M22 += matrix2.M22;
            matrix1.M23 += matrix2.M23;
            matrix1.M24 += matrix2.M24;
            matrix1.M31 += matrix2.M31;
            matrix1.M32 += matrix2.M32;
            matrix1.M33 += matrix2.M33;
            matrix1.M34 += matrix2.M34;
            matrix1.M41 += matrix2.M41;
            matrix1.M42 += matrix2.M42;
            matrix1.M43 += matrix2.M43;
            matrix1.M44 += matrix2.M44;
            return matrix1;
        }

        public static void Add(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 + matrix2.M11;
            result.M12 = matrix1.M12 + matrix2.M12;
            result.M13 = matrix1.M13 + matrix2.M13;
            result.M14 = matrix1.M14 + matrix2.M14;
            result.M21 = matrix1.M21 + matrix2.M21;
            result.M22 = matrix1.M22 + matrix2.M22;
            result.M23 = matrix1.M23 + matrix2.M23;
            result.M24 = matrix1.M24 + matrix2.M24;
            result.M31 = matrix1.M31 + matrix2.M31;
            result.M32 = matrix1.M32 + matrix2.M32;
            result.M33 = matrix1.M33 + matrix2.M33;
            result.M34 = matrix1.M34 + matrix2.M34;
            result.M41 = matrix1.M41 + matrix2.M41;
            result.M42 = matrix1.M42 + matrix2.M42;
            result.M43 = matrix1.M43 + matrix2.M43;
            result.M44 = matrix1.M44 + matrix2.M44;
        }

        public static Matrix CreateBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 cameraUpVector, Vector3? cameraForwardVector)
        {
            Matrix matrix;
            CreateBillboard(ref objectPosition, ref cameraPosition, ref cameraUpVector, cameraForwardVector, out matrix);
            return matrix;
        }

        public static void CreateBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 cameraUpVector, Vector3? cameraForwardVector, out Matrix result)
        {
            Vector3 vector2;
            Vector3 vector3;
            Vector3 vector4;
            Vector3 vector = objectPosition - cameraPosition;
            Vector3.Normalize(ref vector, out vector2);
            Vector3.Normalize(ref cameraUpVector, out vector4);
            Vector3.Cross(ref vector2, ref vector4, out vector3);
            Vector3.Cross(ref vector2, ref vector3, out vector4);
            result = Identity;
            result.Backward = vector2;
            result.Right = vector3;
            result.Up = vector4;
            result.Translation = vector;
        }

        public static Matrix CreateConstrainedBillboard(Vector3 objectPosition, Vector3 cameraPosition, Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector)
        {
            throw new NotImplementedException();
        }

        public static void CreateConstrainedBillboard(ref Vector3 objectPosition, ref Vector3 cameraPosition, ref Vector3 rotateAxis, Vector3? cameraForwardVector, Vector3? objectForwardVector, out Matrix result)
        {
            throw new NotImplementedException();
        }

        public static Matrix CreateFromAxisAngle(Vector3 axis, FP angle)
        {
            throw new NotImplementedException();
        }

        public static void CreateFromAxisAngle(ref Vector3 axis, FP angle, out Matrix result)
        {
            throw new NotImplementedException();
        }

        public static Matrix CreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            Matrix matrix;
            CreateLookAt(ref cameraPosition, ref cameraTarget, ref cameraUpVector, out matrix);
            return matrix;
        }

        public static void CreateLookAt(ref Vector3 cameraPosition, ref Vector3 cameraTarget, ref Vector3 cameraUpVector, out Matrix result)
        {
            Vector3 vector = Vector3.Normalize(cameraPosition - cameraTarget);
            Vector3 vector2 = Vector3.Normalize(Vector3.Cross(cameraUpVector, vector));
            Vector3 vector3 = Vector3.Cross(vector, vector2);
            result = Identity;
            result.M11 = vector2.X;
            result.M12 = vector3.X;
            result.M13 = vector.X;
            result.M21 = vector2.Y;
            result.M22 = vector3.Y;
            result.M23 = vector.Y;
            result.M31 = vector2.Z;
            result.M32 = vector3.Z;
            result.M33 = vector.Z;
            result.M41 = -Vector3.Dot(vector2, cameraPosition);
            result.M42 = -Vector3.Dot(vector3, cameraPosition);
            result.M43 = -Vector3.Dot(vector, cameraPosition);
        }

        public static Matrix CreateOrthographic(FP width, FP height, FP zNearPlane, FP zFarPlane)
        {
            Matrix matrix;
            CreateOrthographic(width, height, zNearPlane, zFarPlane, out matrix);
            return matrix;
        }

        public static void CreateOrthographic(FP width, FP height, FP zNearPlane, FP zFarPlane, out Matrix result)
        {
            result.M11 = 2 / width;
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 2 / height;
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1 / (zNearPlane - zFarPlane);
            result.M34 = 0;
            result.M41 = 0;
            result.M42 = 0;
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1;
        }

        public static Matrix CreateOrthographicOffCenter(FP left, FP right, FP bottom, FP top, FP zNearPlane, FP zFarPlane)
        {
            Matrix matrix;
            CreateOrthographicOffCenter(left, right, bottom, top, zNearPlane, zFarPlane, out matrix);
            return matrix;
        }

        public static void CreateOrthographicOffCenter(FP left, FP right, FP bottom, FP top, FP zNearPlane, FP zFarPlane, out Matrix result)
        {
            result.M11 = 2 / (right - left);
            result.M12 = 0;
            result.M13 = 0;
            result.M14 = 0;
            result.M21 = 0;
            result.M22 = 2 / (top - bottom);
            result.M23 = 0;
            result.M24 = 0;
            result.M31 = 0;
            result.M32 = 0;
            result.M33 = 1 / (zNearPlane - zFarPlane);
            result.M34 = 0;
            result.M41 = (left + right) / (left - right);
            result.M42 = (bottom + top) / (bottom - top);
            result.M43 = zNearPlane / (zNearPlane - zFarPlane);
            result.M44 = 1;
        }

        public static Matrix CreatePerspective(FP width, FP height, FP zNearPlane, FP zFarPlane)
        {
            throw new NotImplementedException();
        }

        public static void CreatePerspective(FP width, FP height, FP zNearPlane, FP zFarPlane, out Matrix result)
        {
            throw new NotImplementedException();
        }

        public static Matrix CreatePerspectiveFieldOfView(FP fieldOfView, FP aspectRatio, FP nearPlaneDistance, FP farPlaneDistance)
        {
            Matrix matrix;
            CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearPlaneDistance, farPlaneDistance, out matrix);
            return matrix;
        }

        public static void CreatePerspectiveFieldOfView(FP fieldOfView, FP aspectRatio, FP nearPlaneDistance, FP farPlaneDistance, out Matrix result)
        {
            result = new Matrix(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
            if ((fieldOfView < 0) || (fieldOfView > 3.141593f))
            {
                throw new ArgumentOutOfRangeException("fieldOfView", "fieldOfView takes a value between 0 and Pi (180 degrees) in radians.");
            }
            if (nearPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance", "You should specify positive value for nearPlaneDistance.");
            }
            if (farPlaneDistance <= 0f)
            {
                throw new ArgumentOutOfRangeException("farPlaneDistance", "You should specify positive value for farPlaneDistance.");
            }
            if (farPlaneDistance <= nearPlaneDistance)
            {
                throw new ArgumentOutOfRangeException("nearPlaneDistance", "Near plane distance is larger than Far plane distance. Near plane distance must be smaller than Far plane distance.");
            }
            FP fp = 1 / FP.Tan(fieldOfView / 2);
            FP fp2 = fp / aspectRatio;
            result.M11 = fp2;
            result.M22 = fp;
            result.M33 = farPlaneDistance / (nearPlaneDistance - farPlaneDistance);
            result.M34 = -1;
            result.M43 = (nearPlaneDistance * farPlaneDistance) / (nearPlaneDistance - farPlaneDistance);
        }

        public static Matrix CreatePerspectiveOffCenter(FP left, FP right, FP bottom, FP top, FP zNearPlane, FP zFarPlane)
        {
            throw new NotImplementedException();
        }

        public static void CreatePerspectiveOffCenter(FP left, FP right, FP bottom, FP top, FP nearPlaneDistance, FP farPlaneDistance, out Matrix result)
        {
            throw new NotImplementedException();
        }

        public static Matrix CreateRotationX(FP radians)
        {
            Matrix identity = Identity;
            identity.M22 = FP.Cos(radians);
            identity.M23 = FP.Sin(radians);
            identity.M32 = -identity.M23;
            identity.M33 = identity.M22;
            return identity;
        }

        public static void CreateRotationX(FP radians, out Matrix result)
        {
            result = Identity;
            result.M22 = FP.Cos(radians);
            result.M23 = FP.Sin(radians);
            result.M32 = -result.M23;
            result.M33 = result.M22;
        }

        public static Matrix CreateRotationY(FP radians)
        {
            Matrix identity = Identity;
            identity.M11 = FP.Cos(radians);
            identity.M13 = FP.Sin(radians);
            identity.M31 = -identity.M13;
            identity.M33 = identity.M11;
            return identity;
        }

        public static void CreateRotationY(FP radians, out Matrix result)
        {
            result = Identity;
            result.M11 = FP.Cos(radians);
            result.M13 = FP.Sin(radians);
            result.M31 = -result.M13;
            result.M33 = result.M11;
        }

        public static Matrix CreateRotationZ(FP radians)
        {
            Matrix identity = Identity;
            identity.M11 = FP.Cos(radians);
            identity.M12 = FP.Sin(radians);
            identity.M21 = -identity.M12;
            identity.M22 = identity.M11;
            return identity;
        }

        public static void CreateRotationZ(FP radians, out Matrix result)
        {
            result = Identity;
            result.M11 = FP.Cos(radians);
            result.M12 = FP.Sin(radians);
            result.M21 = -result.M12;
            result.M22 = result.M11;
        }

        public static Matrix CreateScale(FP scale)
        {
            Matrix identity = Identity;
            identity.M11 = scale;
            identity.M22 = scale;
            identity.M33 = scale;
            return identity;
        }

        public static void CreateScale(FP scale, out Matrix result)
        {
            result = Identity;
            result.M11 = scale;
            result.M22 = scale;
            result.M33 = scale;
        }

        public static Matrix CreateScale(FP xScale, FP yScale, FP zScale)
        {
            Matrix identity = Identity;
            identity.M11 = xScale;
            identity.M22 = yScale;
            identity.M33 = zScale;
            return identity;
        }

        public static void CreateScale(FP xScale, FP yScale, FP zScale, out Matrix result)
        {
            result = Identity;
            result.M11 = xScale;
            result.M22 = yScale;
            result.M33 = zScale;
        }

        public static Matrix CreateScale(Vector3 scales)
        {
            Matrix identity = Identity;
            identity.M11 = scales.X;
            identity.M22 = scales.Y;
            identity.M33 = scales.Z;
            return identity;
        }

        public static void CreateScale(ref Vector3 scales, out Matrix result)
        {
            result = Identity;
            result.M11 = scales.X;
            result.M22 = scales.Y;
            result.M33 = scales.Z;
        }

        public static Matrix CreateTranslation(FP xPosition, FP yPosition, FP zPosition)
        {
            Matrix identity = Identity;
            identity.M41 = xPosition;
            identity.M42 = yPosition;
            identity.M43 = zPosition;
            return identity;
        }

        public static void CreateTranslation(FP xPosition, FP yPosition, FP zPosition, out Matrix result)
        {
            result = Identity;
            result.M41 = xPosition;
            result.M42 = yPosition;
            result.M43 = zPosition;
        }

        public static Matrix CreateTranslation(Vector3 position)
        {
            Matrix identity = Identity;
            identity.M41 = position.X;
            identity.M42 = position.Y;
            identity.M43 = position.Z;
            return identity;
        }

        public static void CreateTranslation(ref Vector3 position, out Matrix result)
        {
            result = Identity;
            result.M41 = position.X;
            result.M42 = position.Y;
            result.M43 = position.Z;
        }

        public static Matrix Divide(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            Divide(ref matrix1, ref matrix2, out matrix);
            return matrix;
        }

        public static void Divide(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            Matrix matrix = Invert(matrix2);
            Multiply(ref matrix1, ref matrix, out result);
        }

        public static Matrix Divide(Matrix matrix1, FP divider)
        {
            Matrix matrix;
            Divide(ref matrix1, divider, out matrix);
            return matrix;
        }

        public static void Divide(ref Matrix matrix1, FP divider, out Matrix result)
        {
            FP factor = 1f / divider;
            Multiply(ref matrix1, factor, out result);
        }

        public static Matrix Invert(Matrix matrix)
        {
            Invert(ref matrix, out matrix);
            return matrix;
        }

        public static void Invert(ref Matrix matrix, out Matrix result)
        {
            Matrix matrix2;
            FP fp = (matrix.M11 * matrix.M22) - (matrix.M12 * matrix.M21);
            FP fp2 = (matrix.M11 * matrix.M23) - (matrix.M13 * matrix.M21);
            FP fp3 = (matrix.M11 * matrix.M24) - (matrix.M14 * matrix.M21);
            FP fp4 = (matrix.M12 * matrix.M23) - (matrix.M13 * matrix.M22);
            FP fp5 = (matrix.M12 * matrix.M24) - (matrix.M14 * matrix.M22);
            FP fp6 = (matrix.M13 * matrix.M24) - (matrix.M14 * matrix.M23);
            FP fp7 = (matrix.M31 * matrix.M42) - (matrix.M32 * matrix.M41);
            FP fp8 = (matrix.M31 * matrix.M43) - (matrix.M33 * matrix.M41);
            FP fp9 = (matrix.M31 * matrix.M44) - (matrix.M34 * matrix.M41);
            FP fp10 = (matrix.M32 * matrix.M43) - (matrix.M33 * matrix.M42);
            FP fp11 = (matrix.M32 * matrix.M44) - (matrix.M34 * matrix.M42);
            FP fp12 = (matrix.M33 * matrix.M44) - (matrix.M34 * matrix.M43);
            FP fp13 = (((((fp * fp12) - (fp2 * fp11)) + (fp3 * fp10)) + (fp4 * fp9)) - (fp5 * fp8)) + (fp6 * fp7);
            FP fp14 = 1f / fp13;
            matrix2.M11 = (((matrix.M22 * fp12) - (matrix.M23 * fp11)) + (matrix.M24 * fp10)) * fp14;
            matrix2.M12 = (((-matrix.M12 * fp12) + (matrix.M13 * fp11)) - (matrix.M14 * fp10)) * fp14;
            matrix2.M13 = (((matrix.M42 * fp6) - (matrix.M43 * fp5)) + (matrix.M44 * fp4)) * fp14;
            matrix2.M14 = (((-matrix.M32 * fp6) + (matrix.M33 * fp5)) - (matrix.M34 * fp4)) * fp14;
            matrix2.M21 = (((-matrix.M21 * fp12) + (matrix.M23 * fp9)) - (matrix.M24 * fp8)) * fp14;
            matrix2.M22 = (((matrix.M11 * fp12) - (matrix.M13 * fp9)) + (matrix.M14 * fp8)) * fp14;
            matrix2.M23 = (((-matrix.M41 * fp6) + (matrix.M43 * fp3)) - (matrix.M44 * fp2)) * fp14;
            matrix2.M24 = (((matrix.M31 * fp6) - (matrix.M33 * fp3)) + (matrix.M34 * fp2)) * fp14;
            matrix2.M31 = (((matrix.M21 * fp11) - (matrix.M22 * fp9)) + (matrix.M24 * fp7)) * fp14;
            matrix2.M32 = (((-matrix.M11 * fp11) + (matrix.M12 * fp9)) - (matrix.M14 * fp7)) * fp14;
            matrix2.M33 = (((matrix.M41 * fp5) - (matrix.M42 * fp3)) + (matrix.M44 * fp)) * fp14;
            matrix2.M34 = (((-matrix.M31 * fp5) + (matrix.M32 * fp3)) - (matrix.M34 * fp)) * fp14;
            matrix2.M41 = (((-matrix.M21 * fp10) + (matrix.M22 * fp8)) - (matrix.M23 * fp7)) * fp14;
            matrix2.M42 = (((matrix.M11 * fp10) - (matrix.M12 * fp8)) + (matrix.M13 * fp7)) * fp14;
            matrix2.M43 = (((-matrix.M41 * fp4) + (matrix.M42 * fp2)) - (matrix.M43 * fp)) * fp14;
            matrix2.M44 = (((matrix.M31 * fp4) - (matrix.M32 * fp2)) + (matrix.M33 * fp)) * fp14;
            result = matrix2;
        }

        public static Matrix Lerp(Matrix matrix1, Matrix matrix2, FP amount)
        {
            throw new NotImplementedException();
        }

        public static void Lerp(ref Matrix matrix1, ref Matrix matrix2, FP amount, out Matrix result)
        {
            throw new NotImplementedException();
        }

        public static Matrix Multiply(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            Multiply(ref matrix1, ref matrix2, out matrix);
            return matrix;
        }

        public static void Multiply(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = (((matrix1.M11 * matrix2.M11) + (matrix1.M12 * matrix2.M21)) + (matrix1.M13 * matrix2.M31)) + (matrix1.M14 * matrix2.M41);
            result.M12 = (((matrix1.M11 * matrix2.M12) + (matrix1.M12 * matrix2.M22)) + (matrix1.M13 * matrix2.M32)) + (matrix1.M14 * matrix2.M42);
            result.M13 = (((matrix1.M11 * matrix2.M13) + (matrix1.M12 * matrix2.M23)) + (matrix1.M13 * matrix2.M33)) + (matrix1.M14 * matrix2.M43);
            result.M14 = (((matrix1.M11 * matrix2.M14) + (matrix1.M12 * matrix2.M24)) + (matrix1.M13 * matrix2.M34)) + (matrix1.M14 * matrix2.M44);
            result.M21 = (((matrix1.M21 * matrix2.M11) + (matrix1.M22 * matrix2.M21)) + (matrix1.M23 * matrix2.M31)) + (matrix1.M24 * matrix2.M41);
            result.M22 = (((matrix1.M21 * matrix2.M12) + (matrix1.M22 * matrix2.M22)) + (matrix1.M23 * matrix2.M32)) + (matrix1.M24 * matrix2.M42);
            result.M23 = (((matrix1.M21 * matrix2.M13) + (matrix1.M22 * matrix2.M23)) + (matrix1.M23 * matrix2.M33)) + (matrix1.M24 * matrix2.M43);
            result.M24 = (((matrix1.M21 * matrix2.M14) + (matrix1.M22 * matrix2.M24)) + (matrix1.M23 * matrix2.M34)) + (matrix1.M24 * matrix2.M44);
            result.M31 = (((matrix1.M31 * matrix2.M11) + (matrix1.M32 * matrix2.M21)) + (matrix1.M33 * matrix2.M31)) + (matrix1.M34 * matrix2.M41);
            result.M32 = (((matrix1.M31 * matrix2.M12) + (matrix1.M32 * matrix2.M22)) + (matrix1.M33 * matrix2.M32)) + (matrix1.M34 * matrix2.M42);
            result.M33 = (((matrix1.M31 * matrix2.M13) + (matrix1.M32 * matrix2.M23)) + (matrix1.M33 * matrix2.M33)) + (matrix1.M34 * matrix2.M43);
            result.M34 = (((matrix1.M31 * matrix2.M14) + (matrix1.M32 * matrix2.M24)) + (matrix1.M33 * matrix2.M34)) + (matrix1.M34 * matrix2.M44);
            result.M41 = (((matrix1.M41 * matrix2.M11) + (matrix1.M42 * matrix2.M21)) + (matrix1.M43 * matrix2.M31)) + (matrix1.M44 * matrix2.M41);
            result.M42 = (((matrix1.M41 * matrix2.M12) + (matrix1.M42 * matrix2.M22)) + (matrix1.M43 * matrix2.M32)) + (matrix1.M44 * matrix2.M42);
            result.M43 = (((matrix1.M41 * matrix2.M13) + (matrix1.M42 * matrix2.M23)) + (matrix1.M43 * matrix2.M33)) + (matrix1.M44 * matrix2.M43);
            result.M44 = (((matrix1.M41 * matrix2.M14) + (matrix1.M42 * matrix2.M24)) + (matrix1.M43 * matrix2.M34)) + (matrix1.M44 * matrix2.M44);
        }

        public static Matrix Multiply(Matrix matrix1, FP factor)
        {
            matrix1.M11 *= factor;
            matrix1.M12 *= factor;
            matrix1.M13 *= factor;
            matrix1.M14 *= factor;
            matrix1.M21 *= factor;
            matrix1.M22 *= factor;
            matrix1.M23 *= factor;
            matrix1.M24 *= factor;
            matrix1.M31 *= factor;
            matrix1.M32 *= factor;
            matrix1.M33 *= factor;
            matrix1.M34 *= factor;
            matrix1.M41 *= factor;
            matrix1.M42 *= factor;
            matrix1.M43 *= factor;
            matrix1.M44 *= factor;
            return matrix1;
        }

        public static void Multiply(ref Matrix matrix1, FP factor, out Matrix result)
        {
            result.M11 = matrix1.M11 * factor;
            result.M12 = matrix1.M12 * factor;
            result.M13 = matrix1.M13 * factor;
            result.M14 = matrix1.M14 * factor;
            result.M21 = matrix1.M21 * factor;
            result.M22 = matrix1.M22 * factor;
            result.M23 = matrix1.M23 * factor;
            result.M24 = matrix1.M24 * factor;
            result.M31 = matrix1.M31 * factor;
            result.M32 = matrix1.M32 * factor;
            result.M33 = matrix1.M33 * factor;
            result.M34 = matrix1.M34 * factor;
            result.M41 = matrix1.M41 * factor;
            result.M42 = matrix1.M42 * factor;
            result.M43 = matrix1.M43 * factor;
            result.M44 = matrix1.M44 * factor;
        }

        public static Matrix Negate(Matrix matrix)
        {
            Multiply(ref matrix, -1f, out matrix);
            return matrix;
        }

        public static void Negate(ref Matrix matrix, out Matrix result)
        {
            Multiply(ref matrix, -1f, out result);
        }

        public static Matrix Subtract(Matrix matrix1, Matrix matrix2)
        {
            matrix1.M11 -= matrix2.M11;
            matrix1.M12 -= matrix2.M12;
            matrix1.M13 -= matrix2.M13;
            matrix1.M14 -= matrix2.M14;
            matrix1.M21 -= matrix2.M21;
            matrix1.M22 -= matrix2.M22;
            matrix1.M23 -= matrix2.M23;
            matrix1.M24 -= matrix2.M24;
            matrix1.M31 -= matrix2.M31;
            matrix1.M32 -= matrix2.M32;
            matrix1.M33 -= matrix2.M33;
            matrix1.M34 -= matrix2.M34;
            matrix1.M41 -= matrix2.M41;
            matrix1.M42 -= matrix2.M42;
            matrix1.M43 -= matrix2.M43;
            matrix1.M44 -= matrix2.M44;
            return matrix1;
        }

        public static void Subtract(ref Matrix matrix1, ref Matrix matrix2, out Matrix result)
        {
            result.M11 = matrix1.M11 - matrix2.M11;
            result.M12 = matrix1.M12 - matrix2.M12;
            result.M13 = matrix1.M13 - matrix2.M13;
            result.M14 = matrix1.M14 - matrix2.M14;
            result.M21 = matrix1.M21 - matrix2.M21;
            result.M22 = matrix1.M22 - matrix2.M22;
            result.M23 = matrix1.M23 - matrix2.M23;
            result.M24 = matrix1.M24 - matrix2.M24;
            result.M31 = matrix1.M31 - matrix2.M31;
            result.M32 = matrix1.M32 - matrix2.M32;
            result.M33 = matrix1.M33 - matrix2.M33;
            result.M34 = matrix1.M34 - matrix2.M34;
            result.M41 = matrix1.M41 - matrix2.M41;
            result.M42 = matrix1.M42 - matrix2.M42;
            result.M43 = matrix1.M43 - matrix2.M43;
            result.M44 = matrix1.M44 - matrix2.M44;
        }

        public static Matrix Transpose(Matrix matrix)
        {
            Matrix matrix2;
            Transpose(ref matrix, out matrix2);
            return matrix2;
        }

        public static void Transpose(ref Matrix matrix, out Matrix result)
        {
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;
            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;
        }

        public FP Determinant()
        {
            FP fp = (this.M31 * this.M42) - (this.M32 * this.M41);
            FP fp2 = (this.M31 * this.M43) - (this.M33 * this.M41);
            FP fp3 = (this.M31 * this.M44) - (this.M34 * this.M41);
            FP fp4 = (this.M32 * this.M43) - (this.M33 * this.M42);
            FP fp5 = (this.M32 * this.M44) - (this.M34 * this.M42);
            FP fp6 = (this.M33 * this.M44) - (this.M34 * this.M43);
            return ((((this.M11 * (((this.M22 * fp6) - (this.M23 * fp5)) + (this.M24 * fp4))) - (this.M12 * (((this.M21 * fp6) - (this.M23 * fp3)) + (this.M24 * fp2)))) + (this.M13 * (((this.M21 * fp5) - (this.M22 * fp3)) + (this.M24 * fp)))) - (this.M14 * (((this.M21 * fp4) - (this.M22 * fp2)) + (this.M23 * fp))));
        }

        public bool Equals(Matrix other)
        {
            return (this == other);
        }

        public static Matrix operator +(Matrix matrix1, Matrix matrix2)
        {
            Add(ref matrix1, ref matrix2, out matrix1);
            return matrix1;
        }

        public static Matrix operator /(Matrix matrix1, Matrix matrix2)
        {
            Matrix matrix;
            Divide(ref matrix1, ref matrix2, out matrix);
            return matrix;
        }

        public static Matrix operator /(Matrix matrix1, FP divider)
        {
            Matrix matrix;
            Divide(ref matrix1, divider, out matrix);
            return matrix;
        }

        public static bool operator ==(Matrix matrix1, Matrix matrix2)
        {
            return ((((((matrix1.M11 == matrix2.M11) && (matrix1.M12 == matrix2.M12)) && ((matrix1.M13 == matrix2.M13) && (matrix1.M14 == matrix2.M14))) && (((matrix1.M21 == matrix2.M21) && (matrix1.M22 == matrix2.M22)) && ((matrix1.M23 == matrix2.M23) && (matrix1.M24 == matrix2.M24)))) && ((((matrix1.M31 == matrix2.M31) && (matrix1.M32 == matrix2.M32)) && ((matrix1.M33 == matrix2.M33) && (matrix1.M34 == matrix2.M34))) && (((matrix1.M41 == matrix2.M41) && (matrix1.M42 == matrix2.M42)) && (matrix1.M43 == matrix2.M43)))) && (matrix1.M44 == matrix2.M44));
        }

        public static bool operator !=(Matrix matrix1, Matrix matrix2)
        {
            return !(matrix1 == matrix2);
        }

        public static Matrix operator *(Matrix matrix1, Matrix matrix2)
        {
            Matrix result = new Matrix();
            Multiply(ref matrix1, ref matrix2, out result);
            return result;
        }

        public static Matrix operator *(Matrix matrix, FP scaleFactor)
        {
            Multiply(ref matrix, scaleFactor, out matrix);
            return matrix;
        }

        public static Matrix operator *(FP scaleFactor, Matrix matrix)
        {
            Matrix matrix2;
            matrix2.M11 = matrix.M11 * scaleFactor;
            matrix2.M12 = matrix.M12 * scaleFactor;
            matrix2.M13 = matrix.M13 * scaleFactor;
            matrix2.M14 = matrix.M14 * scaleFactor;
            matrix2.M21 = matrix.M21 * scaleFactor;
            matrix2.M22 = matrix.M22 * scaleFactor;
            matrix2.M23 = matrix.M23 * scaleFactor;
            matrix2.M24 = matrix.M24 * scaleFactor;
            matrix2.M31 = matrix.M31 * scaleFactor;
            matrix2.M32 = matrix.M32 * scaleFactor;
            matrix2.M33 = matrix.M33 * scaleFactor;
            matrix2.M34 = matrix.M34 * scaleFactor;
            matrix2.M41 = matrix.M41 * scaleFactor;
            matrix2.M42 = matrix.M42 * scaleFactor;
            matrix2.M43 = matrix.M43 * scaleFactor;
            matrix2.M44 = matrix.M44 * scaleFactor;
            return matrix2;
        }

        public static Matrix operator -(Matrix matrix1, Matrix matrix2)
        {
            Matrix result = new Matrix();
            Subtract(ref matrix1, ref matrix2, out result);
            return result;
        }

        public static Matrix operator -(Matrix matrix1)
        {
            Negate(ref matrix1, out matrix1);
            return matrix1;
        }

        public override bool Equals(object obj)
        {
            return (this == ((Matrix) obj));
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            object[] objArray1 = new object[] { 
                "{ {M11:", this.M11, " M12:", this.M12, " M13:", this.M13, " M14:", this.M14, "} {M21:", this.M21, " M22:", this.M22, " M23:", this.M23, " M24:", this.M24, 
                "} {M31:", this.M31, " M32:", this.M32, " M33:", this.M33, " M34:", this.M34, "} {M41:", this.M41, " M42:", this.M42, " M43:", this.M43, " M44:", this.M44, 
                "} }"
             };
            return string.Concat(objArray1);
        }

        static Matrix()
        {
            identity = new Matrix(1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f);
        }
    }
}

