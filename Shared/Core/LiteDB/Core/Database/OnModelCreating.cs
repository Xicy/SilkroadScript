using System;
using System.Collections.Generic;

namespace LiteDB
{
    public partial class LiteDatabase : IDisposable
    {
        /// <summary>
        ///     Use mapper cache
        /// </summary>
        private static readonly Dictionary<Type, BsonMapper> _mapperCache = new Dictionary<Type, BsonMapper>();

        private void InitializeMapper()
        {
            var type = GetType();

            if (!_mapperCache.TryGetValue(type, out _mapper))
            {
                lock (_mapperCache)
                {
                    if (!_mapperCache.TryGetValue(type, out _mapper))
                    {
                        _mapper = new BsonMapper();
                        OnModelCreating(_mapper);

                        _mapperCache.Add(type, _mapper);
                    }
                }
            }
        }

        /// <summary>
        ///     Use this method to override and apply rules to map your entities to BsonDocument
        /// </summary>
        protected virtual void OnModelCreating(BsonMapper mapper)
        {
        }
    }
}