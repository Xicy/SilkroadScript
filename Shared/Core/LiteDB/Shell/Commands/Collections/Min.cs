namespace LiteDB.Shell.Commands
{
    internal class CollectionMin : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "min");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var index = s.Scan(FieldPattern).Trim();

            return engine.Min(col, index.Length == 0 ? "_id" : index);
        }
    }
}