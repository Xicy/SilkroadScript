using System.Linq;

namespace LiteDB.Shell.Commands
{
    internal class CollectionIndexes : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "indexes$");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);

            return new BsonArray(engine.GetIndexes(col).Select(x => x.AsDocument));
        }
    }
}