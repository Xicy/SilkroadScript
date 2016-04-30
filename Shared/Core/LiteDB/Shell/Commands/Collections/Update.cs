namespace LiteDB.Shell.Commands
{
    internal class CollectionUpdate : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "update");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var doc = JsonSerializer.Deserialize(s).AsDocument;

            return engine.Update(col, new[] {doc});
        }
    }
}