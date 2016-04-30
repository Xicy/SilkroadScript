namespace LiteDB.Shell.Commands
{
    internal class FileUpdate : BaseFileStorage, IShellCommand
    {
        public bool IsCommand(StringScanner s)
        {
            return IsFileCommand(s, "update");
        }

        public BsonValue Execute(DbEngine engine, StringScanner s)
        {
            var fs = new LiteFileStorage(engine);
            var id = ReadId(s);
            var metadata = JsonSerializer.Deserialize(s).AsDocument;

            return fs.SetMetadata(id, metadata);
        }
    }
}