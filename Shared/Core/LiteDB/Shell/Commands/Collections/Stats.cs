namespace LiteDB.Shell.Commands
{
    internal class CollectionStats : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "stats");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);

            return engine.Stats(col);
        }
    }
}