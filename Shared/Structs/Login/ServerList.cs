namespace Shared.Structs
{
    public struct ServerList
    {
        public short Id { get; }
        public string Name { get; }
        public short CurrentUser { get; }
        public short MaxUser { get; }
        public byte Status { get; }

        public ServerList(short id, string name, short currentUser, short maxUser, byte status)
        {
            Id = id;
            Name = name;
            CurrentUser = currentUser;
            MaxUser = maxUser;
            Status = status;

        }
    }
}