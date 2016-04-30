namespace Shared
{
    public interface IScript
    {
        string Name { get; }
        int Version { get; }
        string Description { get; }

        void Loaded();
    }
}