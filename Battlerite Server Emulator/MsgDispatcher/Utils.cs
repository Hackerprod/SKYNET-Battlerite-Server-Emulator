using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
public static class Utils
{
    public static string NormalizeJsString(this string st)
    {
        return st.Replace("'", "\\'");
    }

    public static string Fill(this string str, int len, char fill = ' ')
    {
        if (len <= 0)
        {
            return string.Empty;
        }
        StringBuilder stringBuilder = new StringBuilder();
        string text = str.Substring(0, Math.Min(len, str.Length));
        stringBuilder.Append(text);
        if (len - text.Length > 0)
        {
            stringBuilder.Append(new string(fill, len - text.Length));
        }
        string result = stringBuilder.ToString();
        stringBuilder.Clear();
        return result;
    }

    public static void CopyProperties<T>(T dest, T src)
    {
        foreach (object obj in TypeDescriptor.GetProperties(src))
        {
            PropertyDescriptor propertyDescriptor = (PropertyDescriptor)obj;
            propertyDescriptor.SetValue(dest, propertyDescriptor.GetValue(src));
        }
    }

    public static T Clone<T>(T source)
    {
        if (!typeof(T).IsSerializable)
        {
            throw new ArgumentException("The type must be serializable.", "source");
        }
        T result;
        if (source == null)
        {
            result = default(T);
            return result;
        }
        IFormatter formatter = new BinaryFormatter();
        Stream stream = new MemoryStream();
        using (stream)
        {
            formatter.Serialize(stream, source);
            stream.Seek(0L, SeekOrigin.Begin);
            result = (T)((object)formatter.Deserialize(stream));
        }
        return result;
    }

    public static byte[] RandomBlock(int size)
    {
        byte[] array = new byte[size];
        Utils.Random.NextBytes(array);
        return array;
    }

    public static string RandomChars(int size, string chars = null)
    {
        string text = string.IsNullOrEmpty(chars) ? "abcdefghijklmnopqrstuvwxyz0123456789" : chars;
        string text2 = "";
        for (int i = 0; i < size; i++)
        {
            text2 += text[Utils.Random.Next(0, text.Length - 1)].ToString();
        }
        return text2;
    }

    public static long GetRandomLong()
    {
        long num = 1000L;
        ulong num2 = (ulong)(long.MaxValue - num);
        ulong num3;
        do
        {
            byte[] array = new byte[8];
            Utils.Random.NextBytes(array);
            num3 = (ulong)BitConverter.ToInt64(array, 0);
        }
        while (num3 > 18446744073709551615UL - (18446744073709551615UL % num2 + 1UL) % num2);
        return (long)(num3 % num2 + (ulong)num);
    }

    public static string SizeSuffix(long value, string minSuffix = null)
    {
        int num = Utils.SizeSuffixes.Length - 1;
        if (!string.IsNullOrEmpty(minSuffix))
        {
            num = Array.IndexOf<string>(Utils.SizeSuffixes, minSuffix);
        }
        if (num == -1)
        {
            num = Utils.SizeSuffixes.Length - 1;
        }
        if (value < 0L)
        {
            return "-" + Utils.SizeSuffix(-value, null);
        }
        int num2 = 0;
        decimal num3 = value;
        while (Math.Round(num3 / 1024m) >= 1m && num2 != num)
        {
            num3 /= 1024m;
            num2++;
        }
        return string.Format("{0:n1} {1}", num3, Utils.SizeSuffixes[num2]);
    }

    public static bool TryGetEnum(int num, IList<Type> searchEnums, out Enum enumFound)
    {
        foreach (Type enumType in searchEnums)
        {
            if (Enum.IsDefined(enumType, num))
            {
                enumFound = (Enum)Enum.ToObject(enumType, num);
                return true;
            }
        }
        enumFound = null;
        return false;
    }

    public static void RenameKey<TKey, TValue>(this IDictionary<TKey, TValue> dic, TKey fromKey, TKey toKey)
    {
        TValue value = dic[fromKey];
        dic.Remove(fromKey);
        dic[toKey] = value;
    }

    public static string EncodeHexString(byte[] input)
    {
        return input.Aggregate(new StringBuilder(), (StringBuilder sb, byte v) => sb.Append(v.ToString("x2"))).ToString();
    }

    public static byte[] DecodeHexString(string hex)
    {
        if (hex == null)
        {
            return null;
        }
        int length = hex.Length;
        byte[] array = new byte[length / 2];
        for (int i = 0; i < length; i += 2)
        {
            array[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        }
        return array;
    }

    public static bool Compare<T>(T[] a, T[] b)
    {
        if (a == null || b == null)
        {
            return false;
        }
        if (a.Length != b.Length)
        {
            return false;
        }
        for (int i = 0; i < a.Length; i++)
        {
            T t = a[i];
            T t2 = b[i];
            if (!object.Equals(t, t2))
            {
                return false;
            }
        }
        return true;
    }

    public static Random Random = new Random(Environment.TickCount);

    private static readonly string[] SizeSuffixes = new string[]
    {
            "bytes",
            "KB",
            "MB",
            "GB",
            "TB",
            "PB",
            "EB",
            "ZB",
            "YB"
    };
}