namespace TrueSync
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;

    public class Utils
    {
        public static string GetMd5Sum(string str)
        {
            System.Text.Encoder encoder = Encoding.Unicode.GetEncoder();
            byte[] bytes = new byte[str.Length * 2];
            encoder.GetBytes(str.ToCharArray(), 0, str.Length, bytes, 0, true);
            byte[] buffer2 = new MD5CryptoServiceProvider().ComputeHash(bytes);
            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < buffer2.Length; i++)
            {
                builder.Append(buffer2[i].ToString("X2"));
            }
            return builder.ToString();
        }

        public static List<MemberInfo> GetMembersInfo(Type type)
        {
            List<MemberInfo> list = new List<MemberInfo>();
            list.AddRange(type.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            list.AddRange(type.GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance));
            return list;
        }
    }
}

