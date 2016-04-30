namespace LiteDB.Shell.Commands
{
    internal class CollectionFind : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "find");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var query = ReadQuery(s);
            var skipLimit = ReadSkipLimit(s);
            var docs = engine.Find(col, query, skipLimit.Key, skipLimit.Value);

            return new BsonArray(docs);
        }
    }
}