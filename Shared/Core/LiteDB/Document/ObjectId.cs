using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using System.Threading;

namespace LiteDB
{
    /// <summary>
    ///     Represent a 12-bytes BSON type used in document Id
    /// </summary>
    public class ObjectId : IComparable<ObjectId>, IEquatable<ObjectId>
    {
        /// <summary>
        ///     A zero 12-bytes ObjectId
        /// </summary>
        public static readonly ObjectId Empty = new ObjectId();

        #region Properties

        /// <summary>
        ///     Get timestamp
        /// </summary>
        public int Timestamp { get; }

        /// <summary>
        ///     Get machine number
        /// </summary>
        public int Machine { get; }

        /// <summary>
        ///     Get pid number
        /// </summary>
        public short Pid { get; }

        /// <summary>
        ///     Get increment
        /// </summary>
        public int Increment { get; }

        /// <summary>
        ///     Get creation time
        /// </summary>
        public DateTime CreationTime
        {
            get { return BsonValue.UnixEpoch.AddSeconds(Timestamp); }
        }

        #endregion Properties

        #region Ctor

        /// <summary>
        ///     Initializes a new empty instance of the ObjectId class.
        /// </summary>
        public ObjectId()
        {
            Timestamp = 0;
            Machine = 0;
            Pid = 0;
            Increment = 0;
        }

        /// <summary>
        ///     Initializes a new instance of the ObjectId class from ObjectId vars.
        /// </summary>
        public ObjectId(int timestamp, int machine, short pid, int increment)
        {
            Timestamp = timestamp;
            Machine = machine;
            Pid = pid;
            Increment = increment;
        }

        /// <summary>
        ///     Initializes a new instance of ObjectId class from another ObjectId.
        /// </summary>
        public ObjectId(ObjectId from)
        {
            Timestamp = from.Timestamp;
            Machine = from.Machine;
            Pid = from.Pid;
            Increment = from.Increment;
        }

        /// <summary>
        ///     Initializes a new instance of the ObjectId class from hex string.
        /// </summary>
        public ObjectId(string value)
            : this(FromHex(value))
        {
        }

        /// <summary>
        ///     Initializes a new instance of the ObjectId class from byte array.
        /// </summary>
        public ObjectId(byte[] bytes)
        {
            if (bytes == null) throw new ArgumentNullException("bytes");
            if (bytes.Length != 12) throw new ArgumentException("bytes", "Byte array must be 12 bytes long");

            Timestamp = (bytes[0] << 24) + (bytes[1] << 16) + (bytes[2] << 8) + bytes[3];
            Machine = (bytes[4] << 16) + (bytes[5] << 8) + bytes[6];
            Pid = (short) ((bytes[7] << 8) + bytes[8]);
            Increment = (bytes[9] << 16) + (bytes[10] << 8) + bytes[11];
        }

        /// <summary>
        ///     Convert hex value string in byte array
        /// </summary>
        private static byte[] FromHex(string value)
        {
            if (string.IsNullOrEmpty(value)) throw new ArgumentNullException("val");
            if (value.Length != 24)
                throw new ArgumentException(
                    string.Format("ObjectId strings should be 24 hex characters, got {0} : \"{1}\"", value.Length, value));

            var bytes = new byte[12];

            for (var i = 0; i < 24; i += 2)
            {
                bytes[i/2] = Convert.ToByte(value.Substring(i, 2), 16);
            }

            return bytes;
        }

        #endregion Ctor

        #region Equals/CompareTo/ToString

        /// <summary>
        ///     Equalses the specified other.
        /// </summary>
        public bool Equals(ObjectId other)
        {
            return
                Timestamp == other.Timestamp &&
                Machine == other.Machine &&
                Pid == other.Pid &&
                Increment == other.Increment;
        }

        /// <summary>
        ///     Determines whether the specified object is equal to this instance.
        /// </summary>
        public override bool Equals(object other)
        {
            if (other is ObjectId)
            {
                return Equals((ObjectId) other);
            }

            return false;
        }

        /// <summary>
        ///     Returns a hash code for this instance.
        /// </summary>
        public override int GetHashCode()
        {
            var hash = 17;
            hash = 37*hash + Timestamp.GetHashCode();
            hash = 37*hash + Machine.GetHashCode();
            hash = 37*hash + Pid.GetHashCode();
            hash = 37*hash + Increment.GetHashCode();
            return hash;
        }

        /// <summary>
        ///     Compares two instances of ObjectId
        /// </summary>
        public int CompareTo(ObjectId other)
        {
            var r = Timestamp.CompareTo(other.Timestamp);
            if (r != 0) return r;

            r = Machine.CompareTo(other.Machine);
            if (r != 0) return r;

            r = Pid.CompareTo(other.Pid);
            if (r != 0) return r < 0 ? -1 : 1;

            return Increment.CompareTo(other.Increment);
        }

        /// <summary>
        ///     Represent ObjectId as 12 bytes array
        /// </summary>
        public byte[] ToByteArray()
        {
            var bytes = new byte[12];

            bytes[0] = (byte) (Timestamp >> 24);
            bytes[1] = (byte) (Timestamp >> 16);
            bytes[2] = (byte) (Timestamp >> 8);
            bytes[3] = (byte) Timestamp;
            bytes[4] = (byte) (Machine >> 16);
            bytes[5] = (byte) (Machine >> 8);
            bytes[6] = (byte) Machine;
            bytes[7] = (byte) (Pid >> 8);
            bytes[8] = (byte) Pid;
            bytes[9] = (byte) (Increment >> 16);
            bytes[10] = (byte) (Increment >> 8);
            bytes[11] = (byte) Increment;

            return bytes;
        }

        public override string ToString()
        {
            return BitConverter.ToString(ToByteArray()).Replace("-", "").ToLower();
        }

        #endregion Equals/CompareTo/ToString

        #region Operators

        public static bool operator ==(ObjectId lhs, ObjectId rhs)
        {
            if (ReferenceEquals(lhs, null)) return ReferenceEquals(rhs, null);
            if (ReferenceEquals(rhs, null))
                return false; // don't check type because sometimes different types can be ==

            return lhs.Equals(rhs);
        }

        public static bool operator !=(ObjectId lhs, ObjectId rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >=(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        public static bool operator >(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static bool operator <(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator <=(ObjectId lhs, ObjectId rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        #endregion Operators

        #region Static methods

        private static readonly int _machine;
        private static readonly short _pid;
        private static int _increment;

        // static constructor
        static ObjectId()
        {
            _machine = (GetMachineHash() + AppDomain.CurrentDomain.Id) & 0x00ffffff;
            _increment = new Random().Next();

            try
            {
                _pid = (short) GetCurrentProcessId();
            }
            catch (SecurityException)
            {
                _pid = 0;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        private static int GetMachineHash()
        {
            var hostName = Environment.MachineName; // use instead of Dns.HostName so it will work offline
            return 0x00ffffff & hostName.GetHashCode(); // use first 3 bytes of hash
        }

        /// <summary>
        ///     Creates a new ObjectId.
        /// </summary>
        public static ObjectId NewObjectId()
        {
            var timestamp = (long) Math.Floor((DateTime.UtcNow - BsonValue.UnixEpoch).TotalSeconds);
            var inc = Interlocked.Increment(ref _increment) & 0x00ffffff;

            return new ObjectId((int) timestamp, _machine, _pid, inc);
        }

        #endregion Static methods
    }
}