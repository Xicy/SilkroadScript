namespace LiteDB.Shell.Commands
{
    internal class CollectionCount : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "count");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var query = ReadQuery(s);

            return engine.Count(col, query);
        }
    }
}