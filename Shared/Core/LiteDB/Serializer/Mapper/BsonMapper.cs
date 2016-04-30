using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text.RegularExpressions;

namespace LiteDB
{
    /// <summary>
    ///     Class that converts your entity class to/from BsonDocument
    ///     If you prefer use a new instance of BsonMapper (not Global), be sure cache this instance for better performance
    ///     Serialization rules:
    ///     - Classes must be "public" with a public constructor (without parameters)
    ///     - Properties must have public getter (can be read-only)
    ///     - Entity class must have Id property, [ClassName]Id property or [BsonId] attribute
    ///     - No circular references
    ///     - Fields are not valid
    ///     - IList, Array supports
    ///     - IDictionary supports (Key must be a simple datatype - converted by ChangeType)
    /// </summary>
    public partial class BsonMapper
    {
        private const int MAX_DEPTH = 20;

        private readonly Dictionary<Type, Func<BsonValue, object>> _customDeserializer =
            new Dictionary<Type, Func<BsonValue, object>>();

        /// <summary>
        ///     Map serializer/deserialize for custom types
        /// </summary>
        private readonly Dictionary<Type, Func<object, BsonValue>> _customSerializer =
            new Dictionary<Type, Func<object, BsonValue>>();

        /// <summary>
        ///     Mapping cache between Class/BsonDocument
        /// </summary>
        private readonly Dictionary<Type, Dictionary<string, PropertyMapper>> _mapper =
            new Dictionary<Type, Dictionary<string, PropertyMapper>>();

        /// <summary>
        ///     A resolver name property
        /// </summary>
        public Func<string, string> ResolvePropertyName;

        public BsonMapper()
        {
            SerializeNullValues = false;
            TrimWhitespace = true;
            EmptyStringToNull = true;
            ResolvePropertyName = s => s;

            #region Register CustomTypes

            // register custom types
            RegisterType
                (uri => uri.AbsoluteUri, bson => new Uri(bson.AsString)
                );

            RegisterType
                (nv =>
                {
                    var doc = new BsonDocument();

                    foreach (var key in nv.AllKeys)
                    {
                        doc[key] = nv[key];
                    }

                    return doc;
                }, bson =>
                {
                    var nv = new NameValueCollection();
                    var doc = bson.AsDocument;

                    foreach (var key in doc.Keys)
                    {
                        nv[key] = doc[key].AsString;
                    }

                    return nv;
                }
                );

            #endregion Register CustomTypes
        }

        /// <summary>
        ///     Indicate that mapper do not serialize null values
        /// </summary>
        public bool SerializeNullValues { get; set; }

        /// <summary>
        ///     Apply .Trim() in strings
        /// </summary>
        public bool TrimWhitespace { get; set; }

        /// <summary>
        ///     Convert EmptyString to Null
        /// </summary>
        public bool EmptyStringToNull { get; set; }

        /// <summary>
        ///     Register a custom type serializer/deserialize function
        /// </summary>
        public void RegisterType<T>(Func<T, BsonValue> serialize, Func<BsonValue, T> deserialize)
        {
            _customSerializer[typeof (T)] = o => serialize((T) o);
            _customDeserializer[typeof (T)] = b => deserialize(b);
        }

        /// <summary>
        ///     Map your entity class to BsonDocument using fluent API
        /// </summary>
        public EntityBuilder<T> Entity<T>()
        {
            return new EntityBuilder<T>(this);
        }

        /// <summary>
        ///     Get property mapper between typed .NET class and BsonDocument - Cache results
        /// </summary>
        internal Dictionary<string, PropertyMapper> GetPropertyMapper(Type type)
        {
            Dictionary<string, PropertyMapper> props;

            if (!_mapper.TryGetValue(type, out props))
            {
                lock (_mapper)
                {
                    return _mapper[type] = Reflection.GetProperties(type, ResolvePropertyName);
                }
            }

            return props;
        }

        /// <summary>
        ///     Search for [BsonIndex]/Entity.Index() in PropertyMapper. If not found, returns null
        /// </summary>
        internal IndexOptions GetIndexFromMapper<T>(string field)
        {
            var props = GetPropertyMapper(typeof (T));

            // get index options if type has
            return props.Values
                .Where(x => x.FieldName == field && x.IndexOptions != null)
                .Select(x => x.IndexOptions)
                .FirstOrDefault();
        }

        #region Predefinded Property Resolvers

        public void UseCamelCase()
        {
            ResolvePropertyName = s => char.ToLower(s[0]) + s.Substring(1);
        }

        private readonly Regex _lowerCaseDelimiter = new Regex("(?!(^[A-Z]))([A-Z])");

        public void UseLowerCaseDelimiter(char delimiter = '_')
        {
            ResolvePropertyName = s => _lowerCaseDelimiter.Replace(s, delimiter + "$2").ToLower();
        }

        #endregion Predefinded Property Resolvers
    }
}