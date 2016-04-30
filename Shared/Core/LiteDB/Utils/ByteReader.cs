using System;
using System.Text;

namespace LiteDB
{
    internal unsafe class ByteReader
    {
        private readonly byte[] _buffer;

        public ByteReader(byte[] buffer)
        {
            _buffer = buffer;
            Position = 0;
        }

        public int Position { get; private set; }

        public void Skip(int length)
        {
            Position += length;
        }

        #region Native data types

        public byte ReadByte()
        {
            var value = _buffer[Position];

            Position++;

            return value;
        }

        public bool ReadBoolean()
        {
            var value = _buffer[Position];

            Position++;

            return value == 0 ? false : true;
        }

        public ushort ReadUInt16()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 2;
                return *(ushort*) numRef;
            }
        }

        public uint ReadUInt32()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 4;
                return *(uint*) numRef;
            }
        }

        public ulong ReadUInt64()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 8;
                return *(ulong*) numRef;
            }
        }

        public short ReadInt16()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 2;
                return *(short*) numRef;
            }
        }

        public int ReadInt32()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 4;
                return *(int*) numRef;
            }
        }

        public long ReadInt64()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 8;
                return *(long*) numRef;
            }
        }

        public float ReadSingle()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 4;
                return *(float*) numRef;
            }
        }

        public double ReadDouble()
        {
            fixed (byte* numRef = &_buffer[Position])
            {
                Position += 8;
                return *(double*) numRef;
            }
        }

        public byte[] ReadBytes(int count)
        {
            var buffer = new byte[count];

            Buffer.BlockCopy(_buffer, Position, buffer, 0, count);

            Position += count;

            return buffer;
        }

        #endregion Native data types

        #region Extended types

        public string ReadString()
        {
            var length = ReadInt32();
            var bytes = ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public string ReadString(int length)
        {
            var bytes = ReadBytes(length);
            return Encoding.UTF8.GetString(bytes);
        }

        public DateTime ReadDateTime()
        {
            return new DateTime(ReadInt64());
        }

        public Guid ReadGuid()
        {
            return new Guid(ReadBytes(16));
        }

        public ObjectId ReadObjectId()
        {
            return new ObjectId(ReadBytes(12));
        }

        public PageAddress ReadPageAddress()
        {
            return new PageAddress(ReadUInt32(), ReadUInt16());
        }

        public BsonValue ReadBsonValue(ushort length)
        {
            var type = (BsonType) ReadByte();

            switch (type)
            {
                case BsonType.Null:
                    return BsonValue.Null;

                case BsonType.Int32:
                    return ReadInt32();
                case BsonType.Int64:
                    return ReadInt64();
                case BsonType.Double:
                    return ReadDouble();

                case BsonType.String:
                    return ReadString(length);

                case BsonType.Document:
                    return new BsonReader().ReadDocument(this);
                case BsonType.Array:
                    return new BsonReader().ReadArray(this);

                case BsonType.Binary:
                    return ReadBytes(length);
                case BsonType.ObjectId:
                    return ReadObjectId();
                case BsonType.Guid:
                    return ReadGuid();

                case BsonType.Boolean:
                    return ReadBoolean();
                case BsonType.DateTime:
                    return ReadDateTime();

                case BsonType.MinValue:
                    return BsonValue.MinValue;
                case BsonType.MaxValue:
                    return BsonValue.MaxValue;
            }

            throw new NotImplementedException();
        }

        #endregion Extended types
    }
}