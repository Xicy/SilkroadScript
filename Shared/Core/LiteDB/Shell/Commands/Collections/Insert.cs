using System.Linq;

namespace LiteDB.Shell.Commands
{
    internal class CollectionInsert : BaseCollection, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsCollectionCommand(s, "insert");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var col = ReadCollection(engine, s);
            var value = JsonSerializer.Deserialize(s);

            if (value.IsArray)
            {
                return engine.Insert(col, value.AsArray.RawValue.Select(x => x.AsDocument));
            }
            engine.Insert(col, new[] {value.AsDocument});

            return BsonValue.Null;
        }
    }
}