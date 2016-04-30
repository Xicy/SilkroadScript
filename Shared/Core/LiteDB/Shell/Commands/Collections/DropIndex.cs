namespace LiteDB.Shell.Commands
{
    internal class CollectionDropIndex : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "drop[iI]ndex");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var index = s.Scan(FieldPattern).Trim();

            return engine.DropIndex(col, index);
        }
    }
}