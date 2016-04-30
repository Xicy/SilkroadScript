namespace LiteDB
{
    public partial class LiteCollection<T>
    {
        /// <summary>
        ///     Get collection stats
        /// </summary>
        public BsonDocument Stats()
        {
            return _engine.Stats(Name).AsDocument;
        }
    }
}