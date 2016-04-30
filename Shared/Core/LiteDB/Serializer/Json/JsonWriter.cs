using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace LiteDB
{
    internal class JsonWriter
    {
        private const int INDENT_SIZE = 4;

        private readonly TextWriter _writer;
        private int _indent;
        private string _spacer = "";

        public JsonWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public bool Pretty { get; set; }
        public bool WriteBinary { get; set; }

        public void Serialize(BsonValue value)
        {
            _indent = 0;
            _spacer = Pretty ? " " : "";

            WriteValue(value ?? BsonValue.Null);
        }

        private void WriteValue(BsonValue value)
        {
            // use direct cast to better performance
            switch (value.Type)
            {
                case BsonType.Null:
                    _writer.Write("null");
                    break;

                case BsonType.Array:
                    WriteArray(new BsonArray((List<BsonValue>) value.RawValue));
                    break;

                case BsonType.Document:
                    WriteObject(new BsonDocument((Dictionary<string, BsonValue>) value.RawValue));
                    break;

                case BsonType.Boolean:
                    _writer.Write(((bool) value.RawValue).ToString().ToLower());
                    break;

                case BsonType.String:
                    WriteString((string) value.RawValue);
                    break;

                case BsonType.Int32:
                    _writer.Write((int) value.RawValue);
                    break;

                case BsonType.Double:
                    _writer.Write(((double) value.RawValue).ToString("0.0########", NumberFormatInfo.InvariantInfo));
                    break;

                case BsonType.Binary:
                    var bytes = (byte[]) value.RawValue;
                    WriteExtendDataType("$binary",
                        WriteBinary
                            ? Convert.ToBase64String(bytes, 0, bytes.Length, Base64FormattingOptions.None)
                            : "-- " + bytes.Length + " bytes --");
                    break;

                case BsonType.ObjectId:
                    WriteExtendDataType("$oid", ((ObjectId) value.RawValue).ToString());
                    break;

                case BsonType.Guid:
                    WriteExtendDataType("$guid", ((Guid) value.RawValue).ToString());
                    break;

                case BsonType.DateTime:
                    WriteExtendDataType("$date", ((DateTime) value.RawValue).ToUniversalTime().ToString("o"));
                    break;

                case BsonType.Int64:
                    WriteExtendDataType("$numberLong", ((long) value.RawValue).ToString());
                    break;

                case BsonType.MinValue:
                    WriteExtendDataType("$minValue", "1");
                    break;

                case BsonType.MaxValue:
                    WriteExtendDataType("$maxValue", "1");
                    break;
            }
        }

        private void WriteObject(BsonDocument obj)
        {
            var length = obj.Keys.Count();
            var hasData = length > 0;

            WriteStartBlock("{", hasData);

            var index = 0;

            foreach (var key in obj.Keys)
            {
                WriteKeyValue(key, obj[key], index++ < length - 1);
            }

            WriteEndBlock("}", hasData);
        }

        private void WriteArray(BsonArray arr)
        {
            var hasData = arr.Count > 0;

            WriteStartBlock("[", hasData);

            for (var i = 0; i < arr.Count; i++)
            {
                var item = arr[i];

                // do not do this tests if is not pretty format - to better performance
                if (Pretty)
                {
                    if (!((item.IsDocument && item.AsDocument.Keys.Any()) || (item.IsArray && item.AsArray.Count > 0)))
                    {
                        WriteIndent();
                    }
                }

                WriteValue(item ?? BsonValue.Null);

                if (i < arr.Count - 1)
                {
                    _writer.Write(',');
                }
                WriteNewLine();
            }

            WriteEndBlock("]", hasData);
        }

        private void WriteString(string s)
        {
            _writer.Write('\"');
            var l = s.Length;
            for (var index = 0; index < l; index++)
            {
                var c = s[index];
                switch (c)
                {
                    case '\"':
                        _writer.Write("\\\"");
                        break;

                    case '\\':
                        _writer.Write("\\\\");
                        break;

                    case '\b':
                        _writer.Write("\\b");
                        break;

                    case '\f':
                        _writer.Write("\\f");
                        break;

                    case '\n':
                        _writer.Write("\\n");
                        break;

                    case '\r':
                        _writer.Write("\\r");
                        break;

                    case '\t':
                        _writer.Write("\\t");
                        break;

                    default:
                        int i = c;
                        if (i < 32 || i > 127)
                        {
                            _writer.Write("\\u");
                            _writer.Write(i.ToString("x04"));
                        }
                        else
                        {
                            _writer.Write(c);
                        }
                        break;
                }
            }
            _writer.Write('\"');
        }

        private void WriteExtendDataType(string type, string value)
        {
            // format: { "$type": "string-value" }
            // no string.Format to better performance
            _writer.Write("{\"");
            _writer.Write(type);
            _writer.Write("\":");
            _writer.Write(_spacer);
            _writer.Write("\"");
            _writer.Write(value);
            _writer.Write("\"}");
        }

        private void WriteKeyValue(string key, BsonValue value, bool comma)
        {
            WriteIndent();

            _writer.Write('\"');
            _writer.Write(key);
            _writer.Write("\":");

            // do not do this tests if is not pretty format - to better performance
            if (Pretty)
            {
                _writer.Write(' ');

                if ((value.IsDocument && value.AsDocument.Keys.Any()) || (value.IsArray && value.AsArray.Count > 0))
                {
                    WriteNewLine();
                }
            }

            WriteValue(value ?? BsonValue.Null);

            if (comma)
            {
                _writer.Write(',');
            }

            WriteNewLine();
        }

        private void WriteStartBlock(string str, bool hasData)
        {
            if (hasData)
            {
                WriteIndent();
                _writer.Write(str);
                WriteNewLine();
                _indent++;
            }
            else
            {
                _writer.Write(str);
            }
        }

        private void WriteEndBlock(string str, bool hasData)
        {
            if (hasData)
            {
                _indent--;
                WriteIndent();
                _writer.Write(str);
            }
            else
            {
                _writer.Write(str);
            }
        }

        private void WriteNewLine()
        {
            if (Pretty)
            {
                _writer.WriteLine();
            }
        }

        private void WriteIndent()
        {
            if (Pretty)
            {
                _writer.Write("".PadRight(_indent*INDENT_SIZE, ' '));
            }
        }
    }
}