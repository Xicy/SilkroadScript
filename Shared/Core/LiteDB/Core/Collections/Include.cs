using System;
using System.Collections.Generic;
using System.Linq;

namespace LiteDB
{
    public partial class LiteCollection<T>
    {
        /// <summary>
        ///     Run an include action in each document returned by Find(), FindById(), FindOne() and All() methods to load DbRef
        ///     documents
        ///     Returns a new Collection with this action included
        /// </summary>
        /*public LiteCollection<T> Include<K>(Expression<Func<T, K>> dbref)
        {
            if (dbref == null) throw new ArgumentNullException("dbref");
            var path = _visitor.GetBsonField(dbref);
            _includes.Add(path);
            return this;
        }*/
        /*private IEnumerable<Action<BsonDocument>> StartInclude()
        {
            foreach (var path in _includes)
            {
                yield return delegate(BsonDocument bson)
                {
                    var value = bson.Get(path);

                    if (value.IsNull) return;

                    // if property value is an array, populate all values
                    if (value.IsArray)
                    {
                        var array = value.AsArray;
                        if (array.Count == 0) return;

                        // all doc refs in an array must be same collection, lets take first only
                        var col = new LiteCollection<BsonDocument>(array[0].AsDocument["$ref"], _engine, _mapper, _log);
                        col._includes.AddRange(_includes);
                        for (var i = 0; i < array.Count; i++)
                        {
                            array[i] = col.FindById(array[i].AsDocument["$id"]);
                        }
                    }
                    else
                    {
                        // for BsonDocument, get property value e update with full object refence
                        var doc = value.AsDocument;
                        var col = new LiteCollection<BsonDocument>(doc["$ref"], _engine, _mapper, _log);
                        col._includes.AddRange(_includes);
                        bson.Set(path, col.FindById(doc["$id"]));
                    }
                };
            }
        }*/
        private IEnumerable<Action<BsonDocument>> StartInclude()
        {
            yield return delegate(BsonDocument bson)
            {
                var keys = bson.Keys.ToArray();
                foreach (var key in keys)
                {
                    var value = bson.Get(key);
                    if (value.IsNull) continue;

                    if (value.IsArray)
                    {
                        var array = value.AsArray;
                        if (array.Count == 0) continue;
                        if (!array[0].IsDocument) continue;
                        if (!array[0].AsDocument.ContainsKey("$ref")) continue;
                        var col = new LiteCollection<BsonDocument>(array[0].AsDocument["$ref"], _engine, _mapper, _log);
                        for (var i = 0; i < array.Count; i++)
                        {
                            array[i] = col.FindById(array[i].AsDocument["$id"]);
                        }
                    }
                    else if (value.IsDocument)
                    {
                        var doc = value.AsDocument;
                        if (!doc.ContainsKey("$ref")) continue;
                        var col = new LiteCollection<BsonDocument>(doc["$ref"], _engine, _mapper, _log);
                        bson.Set(key, col.FindById(doc["$id"]));
                    }
                }
            };
        }
    }
}