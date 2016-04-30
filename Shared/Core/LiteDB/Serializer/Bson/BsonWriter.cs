using System;
using System.Collections.Generic;
using System.Text;

namespace LiteDB
{
    /// <summary>
    ///     Internal class to serialize a BsonDocument to BSON data format (byte[])
    /// </summary>
    internal class BsonWriter
    {
        /// <summary>
        ///     Main method - serialize document. Uses ByteWriter
        /// </summary>
        public byte[] Serialize(BsonDocument doc)
        {
            var count = doc.GetBytesCount(true);
            var writer = new ByteWriter(count);

            WriteDocument(writer, doc);

            return writer.Buffer;
        }

        /// <summary>
        ///     Write a bson document
        /// </summary>
        public void WriteDocument(ByteWriter writer, BsonDocument doc)
        {
            writer.Write(doc.GetBytesCount(false));

            foreach (var key in doc.Keys)
            {
                WriteElement(writer, key, doc[key] ?? BsonValue.Null);
            }

            writer.Write((byte) 0x00);
        }

        public void WriteArray(ByteWriter writer, BsonArray array)
        {
            writer.Write(array.GetBytesCount(false));

            for (var i = 0; i < array.Count; i++)
            {
                WriteElement(writer, i.ToString(), array[i] ?? BsonValue.Null);
            }

            writer.Write((byte) 0x00);
        }

        private void WriteElement(ByteWriter writer, string key, BsonValue value)
        {
            // cast RawValue to avoid one if on As<Type>
            switch (value.Type)
            {
                case BsonType.Double:
                    writer.Write((byte) 0x01);
                    WriteCString(writer, key);
                    writer.Write((double) value.RawValue);
                    break;

                case BsonType.String:
                    writer.Write((byte) 0x02);
                    WriteCString(writer, key);
                    WriteString(writer, (string) value.RawValue);
                    break;

                case BsonType.Document:
                    writer.Write((byte) 0x03);
                    WriteCString(writer, key);
                    WriteDocument(writer, new BsonDocument((Dictionary<string, BsonValue>) value.RawValue));
                    break;

                case BsonType.Array:
                    writer.Write((byte) 0x04);
                    WriteCString(writer, key);
                    WriteArray(writer, new BsonArray((List<BsonValue>) value.RawValue));
                    break;

                case BsonType.Binary:
                    writer.Write((byte) 0x05);
                    WriteCString(writer, key);
                    var bytes = (byte[]) value.RawValue;
                    writer.Write(bytes.Length);
                    writer.Write((byte) 0x00); // subtype 00 - Generic binary subtype
                    writer.Write(bytes);
                    break;

                case BsonType.Guid:
                    writer.Write((byte) 0x05);
                    WriteCString(writer, key);
                    var guid = ((Guid) value.RawValue).ToByteArray();
                    writer.Write(guid.Length);
                    writer.Write((byte) 0x04); // UUID
                    writer.Write(guid);
                    break;

                case BsonType.ObjectId:
                    writer.Write((byte) 0x07);
                    WriteCString(writer, key);
                    writer.Write(((ObjectId) value.RawValue).ToByteArray());
                    break;

                case BsonType.Boolean:
                    writer.Write((byte) 0x08);
                    WriteCString(writer, key);
                    writer.Write((byte) ((bool) value.RawValue ? 0x01 : 0x00));
                    break;

                case BsonType.DateTime:
                    writer.Write((byte) 0x09);
                    WriteCString(writer, key);
                    var date = (DateTime) value.RawValue;
                    // do not convert to UTC min/max date values - #19
                    var utc = date == DateTime.MinValue || date == DateTime.MaxValue ? date : date.ToUniversalTime();
                    var ts = utc - BsonValue.UnixEpoch;
                    writer.Write(Convert.ToInt64(ts.TotalMilliseconds));
                    break;

                case BsonType.Null:
                    writer.Write((byte) 0x0A);
                    WriteCString(writer, key);
                    break;

                case BsonType.Int32:
                    writer.Write((byte) 0x10);
                    WriteCString(writer, key);
                    writer.Write((int) value.RawValue);
                    break;

                case BsonType.Int64:
                    writer.Write((byte) 0x12);
                    WriteCString(writer, key);
                    writer.Write((long) value.RawValue);
                    break;

                case BsonType.MinValue:
                    writer.Write((byte) 0xFF);
                    WriteCString(writer, key);
                    break;

                case BsonType.MaxValue:
                    writer.Write((byte) 0x7F);
                    WriteCString(writer, key);
                    break;
            }
        }

        private void WriteString(ByteWriter writer, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            writer.Write(bytes.Length + 1);
            writer.Write(bytes);
            writer.Write((byte) 0x00);
        }

        private void WriteCString(ByteWriter writer, string s)
        {
            var bytes = Encoding.UTF8.GetBytes(s);
            writer.Write(bytes);
            writer.Write((byte) 0x00);
        }
    }
}