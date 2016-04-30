using System;
using System.IO;
using System.Text;

namespace Shared
{
    public class Packet
    {
        public class PacketException : SystemException
        {
            public PacketException(Packet packet, Exception innerException)
            {
                Packet = packet;
                Message = "PacketError:" + innerException.Message;
            }

            public PacketException(Packet packet, string message)
            {
                Packet = packet;
                Message = "PacketError:" + message;
            }

            public Packet Packet { get; }

            public override string Message { get; }
        }

        internal class PacketReader : BinaryReader
        {
            private byte[] _mInput;

            public PacketReader(byte[] input)
                : base(new MemoryStream(input, false))
            {
                _mInput = input;
            }

            public PacketReader(byte[] input, int index, int count)
                : base(new MemoryStream(input, index, count, false))
            {
                _mInput = input;
            }
        }

        internal class PacketWriter : BinaryWriter
        {
            private readonly MemoryStream _mMs;

            public PacketWriter()
            {
                _mMs = new MemoryStream();
                OutStream = _mMs;
            }

            public byte[] GetBytes()
            {
                return _mMs.ToArray();
            }
        }


        public byte[] GetBytes()
        {
            lock (m_lock)
            {
                return Locked ? m_reader_bytes : m_writer.GetBytes();
            }
        }

        /// <summary>
        ///     Locks the writer and stores everything into the reader.
        /// </summary>
        public void Lock()
        {
            lock (m_lock)
            {
                if (Locked) return;
                m_reader_bytes = m_writer.GetBytes();
                m_reader = new PacketReader(m_reader_bytes);
                m_writer.Close();
                m_writer = null;
                Locked = true;
            }
        }

        /// <summary>
        ///     Opens a the writer and loads the reader bytes into the writer
        /// </summary>
        public void Unlock()
        {
            lock (m_lock)
            {
                if (!Locked) return;
                m_writer = new PacketWriter();
                m_writer.Write(m_reader_bytes);
                m_reader.Close();
                m_reader = null;
                m_reader_bytes = null;
                Locked = false;
            }
        }

        public override string ToString()
        {
            if (Locked)
            {
                return m_reader_bytes != null ? BitConverter.ToString(m_reader_bytes).Replace("-", " ") : "Empty";
            }
            //Get the bytes from the writer
            lock (m_lock)
            {
                var tempBuffer = m_writer.GetBytes();
                return tempBuffer != null ? BitConverter.ToString(tempBuffer).Replace("-", " ") : "Empty";
            }
        }

        #region Fields

        private PacketWriter m_writer;
        private PacketReader m_reader;
        private byte[] m_reader_bytes;
        private readonly object m_lock;

        #endregion

        #region Properties

        public ushort Opcode { get; }

        public bool Encrypted { get; }

        public bool Massive { get; }

        public bool Locked { get; private set; }

        #endregion

        #region Constructor

        public Packet(Packet rhs)
        {
            lock (rhs.m_lock)
            {
                m_lock = new object();

                Opcode = rhs.Opcode;
                Encrypted = rhs.Encrypted;
                Massive = rhs.Massive;

                Locked = rhs.Locked;
                if (!Locked)
                {
                    m_writer = new PacketWriter();
                    m_reader = null;
                    m_reader_bytes = null;
                    m_writer.Write(rhs.m_writer.GetBytes());
                }
                else
                {
                    m_writer = null;
                    m_reader_bytes = rhs.m_reader_bytes;
                    m_reader = new PacketReader(m_reader_bytes);
                }
            }
        }

        public Packet(ushort opcode)
        {
            m_lock = new object();
            Opcode = opcode;
            Encrypted = false;
            Massive = false;
            m_writer = new PacketWriter();
            m_reader = null;
            m_reader_bytes = null;
        }

        public Packet(ushort opcode, bool encrypted)
        {
            m_lock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = false;
            m_writer = new PacketWriter();
            m_reader = null;
            m_reader_bytes = null;
        }

        public Packet(ushort opcode, bool encrypted, bool massive)
        {
            if (encrypted && massive)
            {
                throw new PacketException(this, "Packets cannot both be massive and encrypted!");
            }
            m_lock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = massive;
            m_writer = new PacketWriter();
            m_reader = null;
            m_reader_bytes = null;
        }

        public Packet(ushort opcode, bool encrypted, bool massive, byte[] bytes)
        {
            if (encrypted && massive)
            {
                throw new PacketException(this, "Packets cannot both be massive and encrypted!");
            }
            m_lock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = massive;
            m_writer = new PacketWriter();
            m_writer.Write(bytes);
            m_reader = null;
            m_reader_bytes = null;
        }

        public Packet(ushort opcode, bool encrypted, bool massive, byte[] bytes, int offset, int length)
        {
            if (encrypted && massive)
            {
                throw new PacketException(this, "Packets cannot both be massive and encrypted!");
            }
            m_lock = new object();
            Opcode = opcode;
            Encrypted = encrypted;
            Massive = massive;
            m_writer = new PacketWriter();
            m_writer.Write(bytes, offset, length);
            m_reader = null;
            m_reader_bytes = null;
        }

        #endregion

        #region Reader

        public int ReaderPosition
        {
            get
            {
                lock (m_lock)
                {
                    if (!Locked)
                    {
                        throw new PacketException(this, "Cannot read position from an unlocked Packet.");
                    }
                    return (int)(m_reader.BaseStream.Length - m_reader.BaseStream.Position);
                }
            }
        }

        public int ReaderLenght
        {
            get
            {
                lock (m_lock)
                {
                    if (!Locked)
                    {
                        throw new PacketException(this, "Cannot read lenght from an unlocked Packet.");
                    }
                    return (int)m_reader.BaseStream.Length;
                }
            }
        }

        public int ReaderRemain
        {
            get
            {
                lock (m_lock)
                {
                    if (!Locked)
                    {
                        throw new PacketException(this, "Cannot read remain from an unlocked Packet.");
                    }
                    return (int)(m_reader.BaseStream.Length - m_reader.BaseStream.Position);
                }
            }
        }

        public long SeekRead(long offset, SeekOrigin orgin)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot SeekRead on an unlocked Packet.");
                }
                return m_reader.BaseStream.Seek(offset, orgin);
            }
        }

        #region Read

        public bool ReadBool()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadBoolean();
            }
        }

        public byte ReadByte()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadByte();
            }
        }

        public sbyte ReadSByte()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadSByte();
            }
        }

        public ushort ReadUShort()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadUInt16();
            }
        }

        public short ReadShort()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadInt16();
            }
        }

        public uint ReadUInt()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadUInt32();
            }
        }

        public int ReadInt()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadInt32();
            }
        }

        public ulong ReadULong()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadUInt64();
            }
        }

        public long ReadLong()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadInt64();
            }
        }

        public float ReadFloat()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadSingle();
            }
        }

        public double ReadDouble()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                return m_reader.ReadDouble();
            }
        }

        public string ReadAscii()
        {
            return ReadAscii(1252);
        }

        public string ReadAscii(int codepage)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }

                var length = m_reader.ReadUInt16();
                var bytes = m_reader.ReadBytes(length);

                return Encoding.GetEncoding(codepage).GetString(bytes);
            }
        }

        public string ReadUnicode()
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }

                var length = m_reader.ReadUInt16();
                var bytes = m_reader.ReadBytes(length * 2);

                return Encoding.Unicode.GetString(bytes);
            }
        }

        #endregion

        #region ReadArray

        public bool[] ReadBoolArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new bool[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadBoolean();
                }
                return values;
            }
        }

        public byte[] ReadByteArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new byte[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadByte();
                }
                return values;
            }
        }

        public sbyte[] ReadSByteArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new sbyte[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadSByte();
                }
                return values;
            }
        }

        public ushort[] ReadUShortArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new ushort[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadUInt16();
                }
                return values;
            }
        }

        public short[] ReadShortArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new short[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadInt16();
                }
                return values;
            }
        }

        public uint[] ReadUIntArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new uint[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadUInt32();
                }
                return values;
            }
        }

        public int[] ReadIntArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new int[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadInt32();
                }
                return values;
            }
        }

        public ulong[] ReadULongArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new ulong[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadUInt64();
                }
                return values;
            }
        }

        public long[] ReadLongArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new long[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadInt64();
                }
                return values;
            }
        }

        public float[] ReadFloatArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new float[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadSingle();
                }
                return values;
            }
        }

        public double[] ReadDoubleArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new double[count];
                for (var x = 0; x < count; ++x)
                {
                    values[x] = m_reader.ReadDouble();
                }
                return values;
            }
        }

        public string[] ReadAsciiArray(int count)
        {
            return ReadAsciiArray(1252,count);
        }

        public string[] ReadAsciiArray(int codepage, int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new string[count];
                for (var x = 0; x < count; ++x)
                {
                    var length = m_reader.ReadUInt16();
                    var bytes = m_reader.ReadBytes(length);
                    values[x] = Encoding.UTF7.GetString(bytes);
                }
                return values;
            }
        }

        public string[] ReadUnicodeArray(int count)
        {
            lock (m_lock)
            {
                if (!Locked)
                {
                    throw new PacketException(this, "Cannot Read from an unlocked Packet.");
                }
                var values = new string[count];
                for (var x = 0; x < count; ++x)
                {
                    var length = m_reader.ReadUInt16();
                    var bytes = m_reader.ReadBytes(length * 2);
                    values[x] = Encoding.Unicode.GetString(bytes);
                }
                return values;
            }
        }

        #endregion

        #endregion

        #region Writer

        public long SeekWrite(long offset, SeekOrigin orgin)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot SeekWrite on a locked Packet.");
                }
                return m_writer.BaseStream.Seek(offset, orgin);
            }
        }

        #region Write

        public void WriteBool(bool value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteByte(byte value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteSByte(sbyte value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteUShort(ushort value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteShort(short value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteUInt(uint value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteInt(int value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteULong(ulong value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteLong(long value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteFloat(float value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteDouble(double value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(value);
            }
        }

        public void WriteAscii(string value)
        {
            WriteAscii(value, 1252);
        }

        public void WriteAscii(string value, int code_page)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }

                var codepage_bytes = Encoding.GetEncoding(code_page).GetBytes(value);
                var utf7_value = Encoding.UTF7.GetString(codepage_bytes);
                var bytes = Encoding.Default.GetBytes(utf7_value);

                m_writer.Write((ushort)bytes.Length);
                m_writer.Write(bytes);
            }
        }

        public void WriteUnicode(string value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }

                var bytes = Encoding.Unicode.GetBytes(value);

                m_writer.Write((ushort)value.Length);
                m_writer.Write(bytes);
            }
        }

        #endregion

        #region WriteObject

        public void WriteBool(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write((byte)(Convert.ToUInt64(value) & 0xFF));
            }
        }

        public void WriteByte(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write((byte)(Convert.ToUInt64(value) & 0xFF));
            }
        }

        public void WriteSByte(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write((sbyte)(Convert.ToInt64(value) & 0xFF));
            }
        }

        public void WriteUShort(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write((ushort)(Convert.ToUInt64(value) & 0xFFFF));
            }
        }

        public void WriteShort(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write((ushort)(Convert.ToInt64(value) & 0xFFFF));
            }
        }

        public void WriteUInt(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write((uint)(Convert.ToUInt64(value) & 0xFFFFFFFF));
            }
        }

        public void WriteInt(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write((int)(Convert.ToInt64(value) & 0xFFFFFFFF));
            }
        }

        public void WriteULong(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(Convert.ToUInt64(value));
            }
        }

        public void WriteLong(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(Convert.ToInt64(value));
            }
        }

        public void WriteFloat(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(Convert.ToSingle(value));
            }
        }

        public void WriteDouble(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                m_writer.Write(Convert.ToDouble(value));
            }
        }

        public void WriteAscii(object value)
        {
            WriteAscii(value, 1252);
        }

        public void WriteAscii(object value, int code_page)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }

                var codepage_bytes = Encoding.GetEncoding(code_page).GetBytes(value.ToString());
                var utf7_value = Encoding.UTF7.GetString(codepage_bytes);
                var bytes = Encoding.Default.GetBytes(utf7_value);

                m_writer.Write((ushort)bytes.Length);
                m_writer.Write(bytes);
            }
        }

        public void WriteUnicode(object value)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }

                var bytes = Encoding.Unicode.GetBytes(value.ToString());

                m_writer.Write((ushort)value.ToString().Length);
                m_writer.Write(bytes);
            }
        }

        #endregion

        #region WriteArray

        public void WriteBoolArray(bool[] values)
        {
            WriteBoolArray(values, 0, values.Length);
        }

        public void WriteBoolArray(bool[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteByteArray(byte[] values)
        {
            if (Locked)
            {
                throw new PacketException(this, "Cannot Write to a locked Packet.");
            }
            m_writer.Write(values);
        }

        public void WriteByteArray(byte[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteUShortArray(ushort[] values)
        {
            WriteUShortArray(values, 0, values.Length);
        }

        public void WriteUShortArray(ushort[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteShortArray(short[] values)
        {
            WriteShortArray(values, 0, values.Length);
        }

        public void WriteShortArray(short[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteUIntArray(uint[] values)
        {
            WriteUIntArray(values, 0, values.Length);
        }

        public void WriteUIntArray(uint[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteIntArray(int[] values)
        {
            WriteIntArray(values, 0, values.Length);
        }

        public void WriteIntArray(int[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteULongArray(ulong[] values)
        {
            WriteULongArray(values, 0, values.Length);
        }

        public void WriteULongArray(ulong[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteLongArray(long[] values)
        {
            WriteLongArray(values, 0, values.Length);
        }

        public void WriteLongArray(long[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteFloatArray(float[] values)
        {
            WriteFloatArray(values, 0, values.Length);
        }

        public void WriteFloatArray(float[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteDoubleArray(double[] values)
        {
            WriteDoubleArray(values, 0, values.Length);
        }

        public void WriteDoubleArray(double[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    m_writer.Write(values[x]);
                }
            }
        }

        public void WriteAsciiArray(string[] values, int codepage)
        {
            WriteAsciiArray(values, 0, values.Length, codepage);
        }

        public void WriteAsciiArray(string[] values, int index, int count, int codepage)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteAscii(values[x], codepage);
                }
            }
        }

        public void WriteAsciiArray(string[] values)
        {
            WriteAsciiArray(values, 0, values.Length, 1252);
        }

        public void WriteAsciiArray(string[] values, int index, int count)
        {
            WriteAsciiArray(values, index, count, 1252);
        }

        public void WriteUnicodeArray(string[] values)
        {
            WriteUnicodeArray(values, 0, values.Length);
        }

        public void WriteUnicodeArray(string[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteUnicode(values[x]);
                }
            }
        }

        #endregion

        #region WriteObjectArray

        public void WriteBoolArray(object[] values)
        {
            WriteBoolArray(values, 0, values.Length);
        }

        public void WriteBoolArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteBool(values[x]);
                }
            }
        }

        public void WriteByteArray(object[] values)
        {
            WriteByteArray(values, 0, values.Length);
        }

        public void WriteByteArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteByte(values[x]);
                }
            }
        }

        public void WriteSByteArray(object[] values)
        {
            WriteSByteArray(values, 0, values.Length);
        }

        public void WriteSByteArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteSByte(values[x]);
                }
            }
        }

        public void WriteUShortArray(object[] values)
        {
            WriteUShortArray(values, 0, values.Length);
        }

        public void WriteUShortArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteUShort(values[x]);
                }
            }
        }

        public void WriteShortArray(object[] values)
        {
            WriteShortArray(values, 0, values.Length);
        }

        public void WriteShortArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteShort(values[x]);
                }
            }
        }

        public void WriteUIntArray(object[] values)
        {
            WriteUIntArray(values, 0, values.Length);
        }

        public void WriteUIntArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteUInt(values[x]);
                }
            }
        }

        public void WriteIntArray(object[] values)
        {
            WriteIntArray(values, 0, values.Length);
        }

        public void WriteIntArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteInt(values[x]);
                }
            }
        }

        public void WriteULongArray(object[] values)
        {
            WriteULongArray(values, 0, values.Length);
        }

        public void WriteULongArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteULong(values[x]);
                }
            }
        }

        public void WriteLongArray(object[] values)
        {
            WriteLongArray(values, 0, values.Length);
        }

        public void WriteLongArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteLong(values[x]);
                }
            }
        }

        public void WriteFloatArray(object[] values)
        {
            WriteFloatArray(values, 0, values.Length);
        }

        public void WriteFloatArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteFloat(values[x]);
                }
            }
        }

        public void WriteDoubleArray(object[] values)
        {
            WriteDoubleArray(values, 0, values.Length);
        }

        public void WriteDoubleArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteDouble(values[x]);
                }
            }
        }

        public void WriteAsciiArray(object[] values, int codepage)
        {
            WriteAsciiArray(values, 0, values.Length, codepage);
        }

        public void WriteAsciiArray(object[] values, int index, int count, int codepage)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteAscii(values[x].ToString(), codepage);
                }
            }
        }

        public void WriteAsciiArray(object[] values)
        {
            WriteAsciiArray(values, 0, values.Length, 1252);
        }

        public void WriteAsciiArray(object[] values, int index, int count)
        {
            WriteAsciiArray(values, index, count, 1252);
        }

        public void WriteUnicodeArray(object[] values)
        {
            WriteUnicodeArray(values, 0, values.Length);
        }

        public void WriteUnicodeArray(object[] values, int index, int count)
        {
            lock (m_lock)
            {
                if (Locked)
                {
                    throw new PacketException(this, "Cannot Write to a locked Packet.");
                }
                for (var x = index; x < index + count; ++x)
                {
                    WriteUnicode(values[x].ToString());
                }
            }
        }

        #endregion

        #endregion
    }
}