using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace LiteDB
{
    /// <summary>
    ///     Represent a Bson Value used in BsonDocument
    /// </summary>
    public class BsonValue : IComparable<BsonValue>, IEquatable<BsonValue>
    {
        public static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        /// <summary>
        ///     Represent a Null bson type
        /// </summary>
        public static readonly BsonValue Null = new BsonValue();

        /// <summary>
        ///     Represent a MinValue bson type
        /// </summary>
        public static readonly BsonValue MinValue = new BsonValue {Type = BsonType.MinValue, RawValue = "-oo"};

        /// <summary>
        ///     Represent a MaxValue bson type
        /// </summary>
        public static readonly BsonValue MaxValue = new BsonValue {Type = BsonType.MaxValue, RawValue = "+oo"};

        /// <summary>
        ///     Indicate BsonType of this BsonValue
        /// </summary>
        public BsonType Type { get; private set; }

        /// <summary>
        ///     Get internal .NET value object
        /// </summary>
        public virtual object RawValue { get; private set; }

        #region Constructor

        public BsonValue()
        {
            Type = BsonType.Null;
            RawValue = null;
        }

        public BsonValue(int value)
        {
            Type = BsonType.Int32;
            RawValue = value;
        }

        public BsonValue(long value)
        {
            Type = BsonType.Int64;
            RawValue = value;
        }

        public BsonValue(double value)
        {
            Type = BsonType.Double;
            RawValue = value;
        }

        public BsonValue(string value)
        {
            Type = value == null ? BsonType.Null : BsonType.String;
            RawValue = value;
        }

        public BsonValue(Dictionary<string, BsonValue> value)
        {
            Type = BsonType.Document;
            RawValue = value;
        }

        public BsonValue(List<BsonValue> value)
        {
            Type = BsonType.Array;
            RawValue = value;
        }

        public BsonValue(byte[] value)
        {
            Type = BsonType.Binary;
            RawValue = value;
        }

        public BsonValue(ObjectId value)
        {
            Type = BsonType.ObjectId;
            RawValue = value;
        }

        public BsonValue(Guid value)
        {
            Type = BsonType.Guid;
            RawValue = value;
        }

        public BsonValue(bool value)
        {
            Type = BsonType.Boolean;
            RawValue = value;
        }

        public BsonValue(DateTime value)
        {
            Type = BsonType.DateTime;
            RawValue = value;
        }

        public BsonValue(BsonValue value)
        {
            Type = value.Type;
            RawValue = value.RawValue;
        }

        public BsonValue(object value)
        {
            RawValue = value;

            if (value == null) Type = BsonType.Null;
            else if (value is int) Type = BsonType.Int32;
            else if (value is long) Type = BsonType.Int64;
            else if (value is double) Type = BsonType.Double;
            else if (value is string) Type = BsonType.String;
            else if (value is Dictionary<string, BsonValue>) Type = BsonType.Document;
            else if (value is List<BsonValue>) Type = BsonType.Array;
            else if (value is byte[]) Type = BsonType.Binary;
            else if (value is ObjectId) Type = BsonType.ObjectId;
            else if (value is Guid) Type = BsonType.Guid;
            else if (value is bool) Type = BsonType.Boolean;
            else if (value is DateTime) Type = BsonType.DateTime;
            else if (value is BsonValue)
            {
                var v = (BsonValue) value;
                Type = v.Type;
                RawValue = v.RawValue;
            }
            else
                throw new InvalidCastException(
                    "Value is not a valid BSON data type - Use Mapper.ToDocument for more complex types converts");
        }

        #endregion Constructor

        #region Convert types

        public BsonArray AsArray
        {
            get
            {
                if (IsArray)
                {
                    var array = new BsonArray((List<BsonValue>) RawValue);
                    array.Length = Length;

                    return array;
                }
                return default(BsonArray);
            }
        }

        public BsonDocument AsDocument
        {
            get
            {
                if (IsDocument)
                {
                    var doc = new BsonDocument((Dictionary<string, BsonValue>) RawValue);
                    doc.Length = Length;

                    return doc;
                }
                return default(BsonDocument);
            }
        }

        public byte[] AsBinary
        {
            get { return Type == BsonType.Binary ? (byte[]) RawValue : default(byte[]); }
        }

        public bool AsBoolean
        {
            get { return Type == BsonType.Boolean ? (bool) RawValue : default(bool); }
        }

        public string AsString
        {
            get { return Type != BsonType.Null ? RawValue.ToString() : default(string); }
        }

        public int AsInt32
        {
            get { return IsNumber ? Convert.ToInt32(RawValue) : default(int); }
        }

        public long AsInt64
        {
            get { return IsNumber ? Convert.ToInt64(RawValue) : default(long); }
        }

        public double AsDouble
        {
            get { return IsNumber ? Convert.ToDouble(RawValue) : default(double); }
        }

        public DateTime AsDateTime
        {
            get { return Type == BsonType.DateTime ? (DateTime) RawValue : default(DateTime); }
        }

        public ObjectId AsObjectId
        {
            get { return Type == BsonType.ObjectId ? (ObjectId) RawValue : default(ObjectId); }
        }

        public Guid AsGuid
        {
            get { return Type == BsonType.Guid ? (Guid) RawValue : default(Guid); }
        }

        #endregion Convert types

        #region IsTypes

        public bool IsNull
        {
            get { return Type == BsonType.Null; }
        }

        public bool IsArray
        {
            get { return Type == BsonType.Array; }
        }

        public bool IsDocument
        {
            get { return Type == BsonType.Document; }
        }

        public bool IsInt32
        {
            get { return Type == BsonType.Int32; }
        }

        public bool IsInt64
        {
            get { return Type == BsonType.Int64; }
        }

        public bool IsDouble
        {
            get { return Type == BsonType.Double; }
        }

        public bool IsNumber
        {
            get { return IsInt32 || IsInt64 || IsDouble; }
        }

        public bool IsBinary
        {
            get { return Type == BsonType.Binary; }
        }

        public bool IsBoolean
        {
            get { return Type == BsonType.Boolean; }
        }

        public bool IsString
        {
            get { return Type == BsonType.String; }
        }

        public bool IsObjectId
        {
            get { return Type == BsonType.ObjectId; }
        }

        public bool IsGuid
        {
            get { return Type == BsonType.Guid; }
        }

        public bool IsDateTime
        {
            get { return Type == BsonType.DateTime; }
        }

        public bool IsMinValue
        {
            get { return Type == BsonType.MinValue; }
        }

        public bool IsMaxValue
        {
            get { return Type == BsonType.MaxValue; }
        }

        #endregion IsTypes

        #region Implicit Ctor

        // Int32
        public static implicit operator int(BsonValue value)
        {
            return (int) value.RawValue;
        }

        // Int32
        public static implicit operator BsonValue(int value)
        {
            return new BsonValue {Type = BsonType.Int32, RawValue = value};
        }

        // Int64
        public static implicit operator long(BsonValue value)
        {
            return (long) value.RawValue;
        }

        // Int64
        public static implicit operator BsonValue(long value)
        {
            return new BsonValue {Type = BsonType.Int64, RawValue = value};
        }

        // Double
        public static implicit operator double(BsonValue value)
        {
            return (double) value.RawValue;
        }

        // Double
        public static implicit operator BsonValue(double value)
        {
            return new BsonValue {Type = BsonType.Double, RawValue = value};
        }

        // String
        public static implicit operator string(BsonValue value)
        {
            return (string) value.RawValue;
        }

        // String
        public static implicit operator BsonValue(string value)
        {
            return new BsonValue {Type = BsonType.String, RawValue = value};
        }

        // Document
        public static implicit operator Dictionary<string, BsonValue>(BsonValue value)
        {
            return (Dictionary<string, BsonValue>) value.RawValue;
        }

        // Document
        public static implicit operator BsonValue(Dictionary<string, BsonValue> value)
        {
            return new BsonValue {Type = BsonType.Document, RawValue = value};
        }

        // Array
        public static implicit operator List<BsonValue>(BsonValue value)
        {
            return (List<BsonValue>) value.RawValue;
        }

        // Array
        public static implicit operator BsonValue(List<BsonValue> value)
        {
            return new BsonValue {Type = BsonType.Array, RawValue = value};
        }

        // Binary
        public static implicit operator byte[](BsonValue value)
        {
            return (byte[]) value.RawValue;
        }

        // Binary
        public static implicit operator BsonValue(byte[] value)
        {
            return new BsonValue {Type = BsonType.Binary, RawValue = value};
        }

        // ObjectId
        public static implicit operator ObjectId(BsonValue value)
        {
            return (ObjectId) value.RawValue;
        }

        // ObjectId
        public static implicit operator BsonValue(ObjectId value)
        {
            return new BsonValue {Type = BsonType.ObjectId, RawValue = value};
        }

        // Guid
        public static implicit operator Guid(BsonValue value)
        {
            return (Guid) value.RawValue;
        }

        // Guid
        public static implicit operator BsonValue(Guid value)
        {
            return new BsonValue {Type = BsonType.Guid, RawValue = value};
        }

        // Boolean
        public static implicit operator bool(BsonValue value)
        {
            return (bool) value.RawValue;
        }

        // Boolean
        public static implicit operator BsonValue(bool value)
        {
            return new BsonValue {Type = BsonType.Boolean, RawValue = value};
        }

        // DateTime
        public static implicit operator DateTime(BsonValue value)
        {
            return (DateTime) value.RawValue;
        }

        // DateTime
        public static implicit operator BsonValue(DateTime value)
        {
            return new BsonValue {Type = BsonType.DateTime, RawValue = value};
        }

        public override string ToString()
        {
            return IsNull ? "(null)" : RawValue.ToString();
        }

        #endregion Implicit Ctor

        #region IComparable<BsonValue>, IEquatable<BsonValue>

        public virtual int CompareTo(BsonValue other)
        {
            // first, test if types are diferentes
            if (Type != other.Type)
            {
                // if both values are number, convert them to Double to compare
                if (IsNumber && other.IsNumber)
                {
                    return Convert.ToDouble(RawValue).CompareTo(Convert.ToDouble(RawValue));
                }
                // if not, order by sort type order
                return Type.CompareTo(other.Type);
            }

            // for both values with same datatype just compare
            switch (Type)
            {
                case BsonType.Null:
                case BsonType.MinValue:
                case BsonType.MaxValue:
                    return 0;

                case BsonType.Int32:
                    return ((int) RawValue).CompareTo((int) other.RawValue);
                case BsonType.Int64:
                    return ((long) RawValue).CompareTo((long) other.RawValue);
                case BsonType.Double:
                    return ((double) RawValue).CompareTo((double) other.RawValue);

                case BsonType.String:
                    return string.Compare((string) RawValue, (string) other.RawValue);

                case BsonType.Document:
                    return AsDocument.CompareTo(other);
                case BsonType.Array:
                    return AsArray.CompareTo(other);

                case BsonType.Binary:
                    return ((byte[]) RawValue).BinaryCompareTo((byte[]) other.RawValue);
                case BsonType.ObjectId:
                    return ((ObjectId) RawValue).CompareTo((ObjectId) other.RawValue);
                case BsonType.Guid:
                    return ((Guid) RawValue).CompareTo((Guid) other.RawValue);

                case BsonType.Boolean:
                    return ((bool) RawValue).CompareTo((bool) other.RawValue);
                case BsonType.DateTime:
                    return ((DateTime) RawValue).CompareTo((DateTime) other.RawValue);

                default:
                    throw new NotImplementedException();
            }
        }

        public bool Equals(BsonValue other)
        {
            return CompareTo(other) == 0;
        }

        #endregion IComparable<BsonValue>, IEquatable<BsonValue>

        #region Operators

        public static bool operator ==(BsonValue lhs, BsonValue rhs)
        {
            if (ReferenceEquals(lhs, null)) return ReferenceEquals(rhs, null);
            if (ReferenceEquals(rhs, null))
                return false; // don't check type because sometimes different types can be ==

            return lhs.Equals(rhs);
        }

        public static bool operator !=(BsonValue lhs, BsonValue rhs)
        {
            return !(lhs == rhs);
        }

        public static bool operator >=(BsonValue lhs, BsonValue rhs)
        {
            return lhs.CompareTo(rhs) >= 0;
        }

        public static bool operator >(BsonValue lhs, BsonValue rhs)
        {
            return lhs.CompareTo(rhs) > 0;
        }

        public static bool operator <(BsonValue lhs, BsonValue rhs)
        {
            return lhs.CompareTo(rhs) < 0;
        }

        public static bool operator <=(BsonValue lhs, BsonValue rhs)
        {
            return lhs.CompareTo(rhs) <= 0;
        }

        public override bool Equals(object obj)
        {
            return Equals(new BsonValue(obj));
        }

        public override int GetHashCode()
        {
            var hash = 17;
            hash = 37*hash + Type.GetHashCode();
            hash = 37*hash + RawValue.GetHashCode();
            return hash;
        }

        #endregion Operators

        #region GetBytesCount, Normalize

        internal int? Length;

        /// <summary>
        ///     Returns how many bytes this BsonValue will use to persist in index writes
        /// </summary>
        public int GetBytesCount(bool recalc)
        {
            if (recalc == false && Length.HasValue) return Length.Value;

            switch (Type)
            {
                case BsonType.Null:
                case BsonType.MinValue:
                case BsonType.MaxValue:
                    Length = 0;
                    break;

                case BsonType.Int32:
                    Length = 4;
                    break;
                case BsonType.Int64:
                    Length = 8;
                    break;
                case BsonType.Double:
                    Length = 8;
                    break;

                case BsonType.String:
                    Length = Encoding.UTF8.GetByteCount((string) RawValue);
                    break;

                case BsonType.Binary:
                    Length = ((byte[]) RawValue).Length;
                    break;
                case BsonType.ObjectId:
                    Length = 12;
                    break;
                case BsonType.Guid:
                    Length = 16;
                    break;

                case BsonType.Boolean:
                    Length = 1;
                    break;
                case BsonType.DateTime:
                    Length = 8;
                    break;

                // for Array/Document calculate from elements
                case BsonType.Array:
                    var array = (List<BsonValue>) RawValue;
                    Length = 5; // header + footer
                    for (var i = 0; i < array.Count; i++)
                    {
                        Length += GetBytesCountElement(i.ToString(), array[i] ?? Null, recalc);
                    }
                    break;

                case BsonType.Document:
                    var doc = (Dictionary<string, BsonValue>) RawValue;
                    Length = 5; // header + footer
                    foreach (var key in doc.Keys)
                    {
                        Length += GetBytesCountElement(key, doc[key] ?? Null, recalc);
                    }
                    break;
            }

            return Length.Value;
        }

        private int GetBytesCountElement(string key, BsonValue value, bool recalc)
        {
            return
                1 + // element type
                Encoding.UTF8.GetByteCount(key) + // CString
                1 + // CString 0x00
                value.GetBytesCount(recalc) +
                (value.Type == BsonType.String || value.Type == BsonType.Binary || value.Type == BsonType.Guid ? 5 : 0);
            // bytes.Length + 0x??
        }

        /// <summary>
        ///     Normalize a string value using IndexOptions and returns a new BsonValue - if is not a string, returns some
        ///     BsonValue instance
        /// </summary>
        internal BsonValue Normalize(IndexOptions options)
        {
            // if not string, do nothing
            if (Type != BsonType.String) return this;

            // removing whitespaces
            var text = (string) RawValue;

            if (options.TrimWhitespace) text = text.Trim();
            if (options.IgnoreCase) text = text.ToLower(CultureInfo.InvariantCulture);

            // convert emptystring to null
            if (text.Length == 0 && options.EmptyStringToNull)
            {
                return Null;
            }

            if (!options.RemoveAccents)
            {
                return text;
            }

            // removing accents
            var normalized = text.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (var i = 0; i < normalized.Length; i++)
            {
                var c = normalized[i];

                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString();
        }

        #endregion GetBytesCount, Normalize
    }
}