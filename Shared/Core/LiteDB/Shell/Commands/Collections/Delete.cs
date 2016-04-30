namespace LiteDB.Shell.Commands
{
    internal class CollectionDelete : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "delete");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var query = ReadQuery(s);

            return engine.Delete(col, query);
        }
    }
}