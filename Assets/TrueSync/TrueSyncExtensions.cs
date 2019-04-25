using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using TrueSync;
using UnityEngine;

public static class TrueSyncExtensions
{
    public static Vector3 Abs(this Vector3 vector)
    {
        return new Vector3(Mathf.Abs(vector.x), Mathf.Abs(vector.y), Mathf.Abs(vector.z));
    }

    public static object GetValue(this MemberInfo memberInfo, object obj)
    {
        if (memberInfo is PropertyInfo)
        {
            return ((PropertyInfo) memberInfo).GetValue(obj, null);
        }
        if (memberInfo is FieldInfo)
        {
            return ((FieldInfo) memberInfo).GetValue(obj);
        }
        return null;
    }

    public static void Set(this TSVector jVector, TSVector otherVector)
    {
        jVector.Set(otherVector.x, otherVector.y, otherVector.z);
    }

    public static void SetValue(this MemberInfo memberInfo, object obj, object value)
    {
        if (memberInfo is PropertyInfo)
        {
            ((PropertyInfo) memberInfo).SetValue(obj, value, null);
        }
        else if (memberInfo is FieldInfo)
        {
            ((FieldInfo) memberInfo).SetValue(obj, value);
        }
    }

    public static Quaternion ToQuaternion(this TSMatrix jMatrix)
    {
        return TSQuaternion.CreateFromMatrix(jMatrix).ToQuaternion();
    }

    public static Quaternion ToQuaternion(this TSQuaternion rot)
    {
        return new Quaternion((float) rot.x, (float) rot.y, (float) rot.z, (float) rot.w);
    }

    public static TSMatrix ToTSMatrix(this Quaternion rot)
    {
        return TSMatrix.CreateFromQuaternion(rot.ToTSQuaternion());
    }

    public static TSQuaternion ToTSQuaternion(this Quaternion rot)
    {
        return new TSQuaternion(rot.x, rot.y, rot.z, rot.w);
    }

    public static TSVector ToTSVector(this Vector3 vector)
    {
        return new TSVector(vector.x, vector.y, vector.z);
    }

    public static TSVector2 ToTSVector2(this Vector3 vector)
    {
        return new TSVector2(vector.x, vector.y);
    }

    public static Vector3 ToVector(this TSVector jVector)
    {
        return new Vector3((float) jVector.x, (float) jVector.y, (float) jVector.z);
    }

    public static Vector3 ToVector(this TSVector2 jVector)
    {
        return new Vector3((float) jVector.x, (float) jVector.y, 0f);
    }
}

