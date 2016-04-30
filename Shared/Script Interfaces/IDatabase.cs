using LiteDB;

namespace Shared
{
    public interface IDatabase<T> where T : new()
    {
        LiteCollection<T> Database { set; get; }
    }
}