using System;
using System.Text;

namespace LiteDB
{
    internal unsafe class ByteWriter
    {
        public ByteWriter(int length)
        {
            Buffer = new byte[length];
            Position = 0;
        }

        public ByteWriter(byte[] buffer)
        {
            Buffer = buffer;
            Position = 0;
        }

        public byte[] Buffer { get; }

        public int Position { get; private set; }

        public void Skip(int length)
        {
            Position += length;
        }

        #region Native data types

        public void Write(byte value)
        {
            Buffer[Position] = value;

            Position++;
        }

        public void Write(bool value)
        {
            Buffer[Position] = value ? (byte) 1 : (byte) 0;

            Position++;
        }

        public void Write(ushort value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];

            Position += 2;
        }

        public void Write(uint value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];
            Buffer[Position + 2] = pi[2];
            Buffer[Position + 3] = pi[3];

            Position += 4;
        }

        public void Write(ulong value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];
            Buffer[Position + 2] = pi[2];
            Buffer[Position + 3] = pi[3];
            Buffer[Position + 4] = pi[4];
            Buffer[Position + 5] = pi[5];
            Buffer[Position + 6] = pi[6];
            Buffer[Position + 7] = pi[7];

            Position += 8;
        }

        public void Write(short value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];

            Position += 2;
        }

        public void Write(int value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];
            Buffer[Position + 2] = pi[2];
            Buffer[Position + 3] = pi[3];

            Position += 4;
        }

        public void Write(long value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];
            Buffer[Position + 2] = pi[2];
            Buffer[Position + 3] = pi[3];
            Buffer[Position + 4] = pi[4];
            Buffer[Position + 5] = pi[5];
            Buffer[Position + 6] = pi[6];
            Buffer[Position + 7] = pi[7];

            Position += 8;
        }

        public void Write(float value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];
            Buffer[Position + 2] = pi[2];
            Buffer[Position + 3] = pi[3];

            Position += 4;
        }

        public void Write(double value)
        {
            var pi = (byte*) &value;

            Buffer[Position + 0] = pi[0];
            Buffer[Position + 1] = pi[1];
            Buffer[Position + 2] = pi[2];
            Buffer[Position + 3] = pi[3];
            Buffer[Position + 4] = pi[4];
            Buffer[Position + 5] = pi[5];
            Buffer[Position + 6] = pi[6];
            Buffer[Position + 7] = pi[7];

            Position += 8;
        }

        public void Write(byte[] value)
        {
            System.Buffer.BlockCopy(value, 0, Buffer, Position, value.Length);

            Position += value.Length;
        }

        #endregion Native data types

        #region Extended types

        public void Write(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            Write(bytes.Length);
            Write(bytes);
        }

        public void Write(string value, int length)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            if (bytes.Length != length) throw new ArgumentException("Invalid string length");
            Write(bytes);
        }

        public void Write(DateTime value)
        {
            Write(value.Ticks);
        }

        public void Write(Guid value)
        {
            Write(value.ToByteArray());
        }

        public void Write(ObjectId value)
        {
            Write(value.ToByteArray());
        }

        public void Write(PageAddress value)
        {
            Write(value.PageID);
            Write(value.Index);
        }

        public void WriteBsonValue(BsonValue value, ushort length)
        {
            Write((byte) value.Type);

            switch (value.Type)
            {
                case BsonType.Null:
                case BsonType.MinValue:
                case BsonType.MaxValue:
                    break;

                case BsonType.Int32:
                    Write((int) value.RawValue);
                    break;
                case BsonType.Int64:
                    Write((long) value.RawValue);
                    break;
                case BsonType.Double:
                    Write((double) value.RawValue);
                    break;

                case BsonType.String:
                    Write((string) value.RawValue, length);
                    break;

                case BsonType.Document:
                    new BsonWriter().WriteDocument(this, value.AsDocument);
                    break;
                case BsonType.Array:
                    new BsonWriter().WriteArray(this, value.AsArray);
                    break;

                case BsonType.Binary:
                    Write((byte[]) value.RawValue);
                    break;
                case BsonType.ObjectId:
                    Write((ObjectId) value.RawValue);
                    break;
                case BsonType.Guid:
                    Write((Guid) value.RawValue);
                    break;

                case BsonType.Boolean:
                    Write((bool) value.RawValue);
                    break;
                case BsonType.DateTime:
                    Write((DateTime) value.RawValue);
                    break;

                default:
                    throw new NotImplementedException();
            }
        }

        #endregion Extended types
    }
}