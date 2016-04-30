using System;
using System.Collections;

namespace LiteDB
{
    public partial class BsonMapper
    {
        /// <summary>
        ///     Serialize a entity class to BsonDocument
        /// </summary>
        public BsonDocument ToDocument(Type type, object entity)
        {
            if (entity == null) throw new ArgumentNullException("entity");

            // if object is BsonDocument, just return them
            if (entity is BsonDocument) return (BsonDocument) entity;

            return Serialize(type, entity, 0).AsDocument;
        }

        /// <summary>
        ///     Serialize a entity class to BsonDocument
        /// </summary>
        public BsonDocument ToDocument<T>(T entity)
        {
            return ToDocument(typeof (T), entity).AsDocument;
        }

        internal BsonValue Serialize(Type type, object obj, int depth)
        {
            if (++depth > MAX_DEPTH) throw LiteException.DocumentMaxDepth(MAX_DEPTH);

            if (obj == null) return BsonValue.Null;

            Func<object, BsonValue> custom;

            // if is already a bson value
            if (obj is BsonValue) return new BsonValue((BsonValue) obj);

                // test string - mapper has some special options
            if (obj is string)
            {
                var str = TrimWhitespace ? (obj as string).Trim() : (string) obj;

                if (EmptyStringToNull && str.Length == 0)
                {
                    return BsonValue.Null;
                }
                return new BsonValue(str);
            }
                // basic Bson data types (cast datatype for better performance optimization)
            if (obj is int) return new BsonValue((int) obj);
            if (obj is long) return new BsonValue((long) obj);
            if (obj is double) return new BsonValue((double) obj);
            if (obj is byte[]) return new BsonValue((byte[]) obj);
            if (obj is ObjectId) return new BsonValue((ObjectId) obj);
            if (obj is Guid) return new BsonValue((Guid) obj);
            if (obj is bool) return new BsonValue((bool) obj);
            if (obj is DateTime) return new BsonValue((DateTime) obj);
                // basic .net type to convert to bson
            if (obj is short || obj is ushort || obj is byte)
            {
                return new BsonValue(Convert.ToInt32(obj));
            }
            if (obj is uint || obj is ulong)
            {
                return new BsonValue(Convert.ToInt64(obj));
            }
            if (obj is float || obj is decimal)
            {
                return new BsonValue(Convert.ToDouble(obj));
            }
            if (obj is char || obj is Enum)
            {
                return new BsonValue(obj.ToString());
            }
                // check if is a custom type
            if (_customSerializer.TryGetValue(type, out custom) ||
                _customSerializer.TryGetValue(obj.GetType(), out custom))
            {
                return custom(obj);
            }
                // for dictionary
            if (obj is IDictionary)
            {
                var itemType = type.GetGenericArguments()[1];

                return SerializeDictionary(itemType, obj as IDictionary, depth);
            }
                // check if is a list or array
            if (obj is IEnumerable)
            {
                return SerializeArray(Reflection.GetListItemType(obj), obj as IEnumerable, depth);
            }
                // otherwise serialize as a plain object
            return SerializeObject(type, obj, depth);
        }

        private BsonArray SerializeArray(Type type, IEnumerable array, int depth)
        {
            var arr = new BsonArray();

            foreach (var item in array)
            {
                arr.Add(Serialize(type, item, depth));
            }

            return arr;
        }

        private BsonDocument SerializeDictionary(Type type, IDictionary dict, int depth)
        {
            var o = new BsonDocument();

            foreach (var key in dict.Keys)
            {
                var value = dict[key];

                o.RawValue[key.ToString()] = Serialize(type, value, depth);
            }

            return o;
        }

        private BsonDocument SerializeObject(Type type, object obj, int depth)
        {
            var o = new BsonDocument();
            var t = obj.GetType();
            var mapper = GetPropertyMapper(t);
            var dict = o.RawValue;

            // adding _type only where property Type is not same as object instance type
            if (type != t)
            {
                dict["_type"] = new BsonValue(t.FullName + ", " + t.Assembly.GetName().Name);
            }

            foreach (var prop in mapper.Values)
            {
                // get property value
                var value = prop.Getter(obj);

                if (value == null && SerializeNullValues == false && prop.FieldName != "_id") continue;

                // if prop has a custom serialization, use it
                if (prop.Serialize != null)
                {
                    dict[prop.FieldName] = prop.Serialize(value, this);
                }
                else
                {
                    dict[prop.FieldName] = Serialize(prop.PropertyType, value, depth);
                }
            }

            return o;
        }
    }
}