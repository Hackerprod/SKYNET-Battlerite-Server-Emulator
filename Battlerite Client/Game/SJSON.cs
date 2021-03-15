using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace SKYNET
{
    public static class SJSON
    {
        [ThreadStatic]
        private static byte[] ByteBuffer;

        [ThreadStatic]
        private static int BufferIndex;

        public static string Encode(Dictionary<string, object> t)
        {
            StringBuilder stringBuilder = new StringBuilder();
            WriteRootObject(t, stringBuilder);
            return stringBuilder.ToString();
        }

        public static string EncodeObject(object o)
        {
            StringBuilder stringBuilder = new StringBuilder();
            Write(o, stringBuilder, 0);
            return stringBuilder.ToString();
        }

        public static Dictionary<string, object> Decode(byte[] sjson)
        {
            int index = 0;
            return ParseRootObject(sjson, ref index);
        }

        public static Dictionary<string, object> SafeLoad(string path, float seconds = 5f)
        {
            while (seconds > 0f)
            {
                try
                {
                    FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
                    byte[] array = new byte[fileStream.Length];
                    fileStream.Read(array, 0, array.Length);
                    return Decode(array);
                }
                catch (IOException ex)
                {
                    seconds -= 0.1f;
                    Thread.Sleep(100);
                    if (seconds <= 0f)
                    {
                        //Log.Error("SJSON.SafeLoad exception (failed to retry...) for path '" + path + "'  Exception: " + ex.ToString(), Responsible.Scarb, LogFilter.SJSON);
                    }
                }
            }
            return null;
        }

        public static Dictionary<string, object> Load(string path)
        {
            FileStream fileStream = File.Open(path, FileMode.Open, FileAccess.Read);
            byte[] array = new byte[fileStream.Length];
            fileStream.Read(array, 0, array.Length);
            return Decode(array);
        }

        public static Dictionary<string, object> LoadFromText(string text)
        {
            return Decode(Encoding.UTF8.GetBytes(text));
        }

        public static void Save(Dictionary<string, object> h, string path)
        {
            string s = Encode(h);
            FileStream fileStream = File.Open(path, FileMode.Create);
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            fileStream.Write(bytes, 0, bytes.Count());
        }

        public static void WriteRootObject(Dictionary<string, object> t, StringBuilder builder)
        {
            WriteObjectFields(t, builder, 0);
        }

        public static void WriteObjectField(Dictionary<string, object> t, string key, StringBuilder builder, int indentation)
        {
            object value = t[key];
            WriteObjectField(key, value, builder, indentation);
        }

        public static void WriteObjectField(string key, object value, StringBuilder builder, int indentation)
        {
            if (builder.Length != 0)
            {
                if (indentation == 0)
                {
                    WriteNewLine(builder, indentation);
                }
                else
                {
                    builder.Append(' ');
                }
            }
            builder.Append(key);
            builder.Append(" = ");
            Write(value, builder, indentation);
        }

        public static void WriteObjectFields(Dictionary<string, object> t, StringBuilder builder, int indentation)
        {
            List<string> list = t.Keys.Cast<string>().ToList();
            list.Sort();
            foreach (string item in list)
            {
                WriteObjectField(t, item, builder, indentation);
            }
        }

        public static void WriteNewLine(StringBuilder builder, int indentation)
        {
            builder.Append('\n');
            for (int i = 0; i < indentation; i++)
            {
                builder.Append("    ");
            }
        }

        public static void Write(object o, StringBuilder builder, int indentation)
        {
            if (o == null)
            {
                builder.Append("null");
                return;
            }
            if (o is bool && !(bool)o)
            {
                builder.Append("false");
                return;
            }
            if (o is bool)
            {
                builder.Append("true");
                return;
            }
            if (o is byte)
            {
                builder.Append((byte)o);
                return;
            }
            if (o is int)
            {
                builder.Append((int)o);
                return;
            }
            if (o is float)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", (float)o);
                return;
            }
            if (o is double)
            {
                builder.AppendFormat(CultureInfo.InvariantCulture, "{0}", (double)o);
                return;
            }
            if (o is string)
            {
                WriteString((string)o, builder);
                return;
            }
            if (o is List<object>)
            {
                WriteArray((List<object>)o, builder, indentation);
                return;
            }
            if (o is Dictionary<string, object>)
            {
                WriteObject((Dictionary<string, object>)o, builder, indentation);
                return;
            }
            if (o is Guid)
            {
                WriteString(((Guid)o).ToString(), builder);
                return;
            }
            throw new ArgumentException("Unknown object: " + o.GetType());
        }

        public static void WriteString(string s, StringBuilder builder)
        {
            builder.Append('"');
            foreach (char c in s)
            {
                if (c == '"' || c == '\\')
                {
                    builder.Append('\\');
                }
                builder.Append(c);
            }
            builder.Append('"');
        }

        public static void WriteArray(List<object> a, StringBuilder builder, int indentation)
        {
            WriteNewLine(builder, indentation);
            builder.Append('[');
            foreach (object item in a)
            {
                Write(item, builder, indentation + 1);
            }
            WriteNewLine(builder, indentation);
            builder.Append(']');
        }

        public static void WriteObject(Dictionary<string, object> t, StringBuilder builder, int indentation)
        {
            WriteNewLine(builder, indentation);
            builder.Append('{');
            WriteObjectFields(t, builder, indentation + 1);
            builder.Append('}');
        }

        private static Dictionary<string, object> ParseRootObject(byte[] json, ref int index)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(16);
            while (!AtEnd(json, ref index))
            {
                string key = ParseIdentifier(json, ref index);
                Consume(json, ref index, "=");
                object obj2 = (dictionary[key] = ParseValue(json, ref index));
            }
            return dictionary;
        }

        private static bool AtEnd(byte[] json, ref int index)
        {
            SkipWhitespace(json, ref index);
            return index >= json.Length;
        }

        private static void SkipWhitespace(byte[] json, ref int index)
        {
            while (index < json.Length)
            {
                switch (json[index])
                {
                    case 47:
                        SkipComment(json, ref index);
                        break;
                    case 9:
                    case 10:
                    case 13:
                    case 32:
                    case 44:
                        index++;
                        break;
                    default:
                        return;
                }
            }
        }

        private static void SkipComment(byte[] json, ref int index)
        {
            byte b = json[index + 1];
            if (b == 47)
            {
                while (index + 1 < json.Length && json[index] != 10)
                {
                    index++;
                }
                index++;
                return;
            }
            if (b == 42)
            {
                while (index + 2 < json.Length && (json[index] != 42 || json[index + 1] != 47))
                {
                    index++;
                }
                index += 2;
                return;
            }
            throw new FormatException();
        }

        private static string ParseIdentifier(byte[] json, ref int index)
        {
            SkipWhitespace(json, ref index);
            if (json[index] == 34)
            {
                return ParseString(json, ref index);
            }
            PrepareBuffer();
            while (true)
            {
                byte b = json[index];
                if (b == 32 || b == 9 || b == 10 || b == 61)
                {
                    break;
                }
                AddToBuffer(b);
                index++;
            }
            return Encoding.UTF8.GetString(ByteBuffer, 0, BufferIndex);
        }

        private static void PrepareBuffer()
        {
            BufferIndex = 0;
            if (ByteBuffer == null)
            {
                ByteBuffer = new byte[1024];
            }
        }

        private static void AddToBuffer(byte character)
        {
            if (BufferIndex >= ByteBuffer.Length)
            {
                Array.Resize(ref ByteBuffer, BufferIndex * 2);
            }
            ByteBuffer[BufferIndex++] = character;
        }

        private static void Consume(byte[] json, ref int index, string consume)
        {
            SkipWhitespace(json, ref index);
            for (int i = 0; i < consume.Length; i++)
            {
                if (json[index] != consume[i])
                {
                    throw new FormatException();
                }
                index++;
            }
        }

        private static object ParseValue(byte[] json, ref int index)
        {
            byte b = Next(json, ref index);
            switch (b)
            {
                case 123:
                    return ParseObject(json, ref index);
                case 91:
                    return ParseArray(json, ref index);
                case 34:
                    return ParseString(json, ref index);
                case 45:
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                    return ParseNumber(json, ref index);
                default:
                    switch (b)
                    {
                        case 116:
                            Consume(json, ref index, "true");
                            return true;
                        case 102:
                            Consume(json, ref index, "false");
                            return false;
                        case 110:
                            Consume(json, ref index, "null");
                            return null;
                        default:
                            throw new FormatException();
                    }
            }
        }

        private static byte Next(byte[] json, ref int index)
        {
            SkipWhitespace(json, ref index);
            return json[index];
        }

        private static Dictionary<string, object> ParseObject(byte[] json, ref int index)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>(16);
            Consume(json, ref index, "{");
            SkipWhitespace(json, ref index);
            while (Next(json, ref index) != 125)
            {
                string key = ParseIdentifier(json, ref index);
                Consume(json, ref index, "=");
                object obj2 = (dictionary[key] = ParseValue(json, ref index));
            }
            Consume(json, ref index, "}");
            return dictionary;
        }

        private static List<object> ParseArray(byte[] json, ref int index)
        {
            List<object> list = new List<object>();
            Consume(json, ref index, "[");
            while (Next(json, ref index) != 93)
            {
                object item = ParseValue(json, ref index);
                list.Add(item);
            }
            Consume(json, ref index, "]");
            return list;
        }

        private static string ParseString(byte[] json, ref int index)
        {
            PrepareBuffer();
            Consume(json, ref index, "\"");
            while (true)
            {
                byte b = json[index];
                index++;
                switch (b)
                {
                    default:
                        AddToBuffer(b);
                        break;
                    case 92:
                        {
                            byte b2 = json[index];
                            index++;
                            switch (b2)
                            {
                                case 34:
                                case 47:
                                case 92:
                                    AddToBuffer(b2);
                                    break;
                                case 98:
                                    AddToBuffer(8);
                                    break;
                                case 102:
                                    AddToBuffer(12);
                                    break;
                                case 110:
                                    AddToBuffer(10);
                                    break;
                                case 114:
                                    AddToBuffer(13);
                                    break;
                                case 116:
                                    AddToBuffer(9);
                                    break;
                                default:
                                    _ = 117;
                                    throw new FormatException();
                            }
                            break;
                        }
                    case 34:
                        return Encoding.UTF8.GetString(ByteBuffer, 0, BufferIndex);
                }
            }
        }

        private static double ParseNumber(byte[] json, ref int index)
        {
            int i;
            for (i = index; i < json.Length && "0123456789+-.eE".IndexOf((char)json[i]) != -1; i++)
            {
            }
            byte[] array = new byte[i - index];
            Array.Copy(json, index, array, 0, array.Length);
            index = i;
            return double.Parse(Encoding.UTF8.GetString(array), CultureInfo.InvariantCulture);
        }

        public static void WriteArray<T>(string key, StringBuilder builder, int indentation, List<T> list, Action<T, StringBuilder, int> WriteCallback, bool compact = false)
        {
            WriteBeginArray(key, builder, indentation, list.Count > 0 && !compact);
            indentation++;
            foreach (T item in list)
            {
                WriteCallback(item, builder, indentation);
            }
            indentation--;
            WriteEndArray(builder, indentation, list.Count > 0 && !compact);
        }

        public static void WriteBeginArray<T>(string key, StringBuilder builder, int indentation, List<T> list)
        {
            WriteBeginArray(key, builder, indentation, list.Count > 0);
        }

        public static void WriteBeginArray(string key, StringBuilder builder, int indentation, bool compact = false)
        {
            if (builder.Length != 0)
            {
                if (indentation == 0)
                {
                    WriteNewLine(builder, indentation);
                }
                else
                {
                    builder.Append(' ');
                }
            }
            builder.Append(key);
            builder.Append(" = ");
            if (!compact)
            {
                WriteNewLine(builder, indentation);
            }
            builder.Append('[');
        }

        public static void WriteEndArray<T>(StringBuilder builder, int indentation, List<T> list)
        {
            WriteEndArray(builder, indentation, list.Count > 0);
        }

        public static void WriteEndArray(StringBuilder builder, int indentation, bool compact = false)
        {
            if (!compact)
            {
                WriteNewLine(builder, indentation);
            }
            builder.Append(']');
        }

        public static void WriteBeginObject(StringBuilder builder, int indentation, bool compact = false)
        {
            if (!compact)
            {
                WriteNewLine(builder, indentation);
            }
            builder.Append('{');
        }

        public static void WriteEndObject(StringBuilder builder)
        {
            builder.Append('}');
        }

        public static void SaveBuilder(string file, StringBuilder builder)
        {
            SaveText(file, builder.ToString());
        }

        public static void SaveText(string file, string text)
        {
            FileStream fileStream = File.Open(file, FileMode.Create);
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            fileStream.Write(bytes, 0, bytes.Count());
        }
    }
}