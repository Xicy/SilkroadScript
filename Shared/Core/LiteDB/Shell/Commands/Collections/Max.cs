namespace LiteDB.Shell.Commands
{
    internal class CollectionMax : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "max");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var index = s.Scan(FieldPattern).Trim();

            return engine.Max(col, index.Length == 0 ? "_id" : index);
        }
    }
}