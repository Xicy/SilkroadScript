namespace Shared.Structs.Agent.Spawns
{
    public class Player : Position
    {
        public uint ModelId { get; set; }

        public uint ObjectId { get; set; }

        public bool IsAlive { get; set; }

        public byte Level { get; set; }

        public string Name { get; set; }

        public Player(uint ModelId, uint ObjectId, byte Level, string Name, bool IsAlive)
        {
            this.ModelId = ModelId;
            this.ObjectId = ObjectId;
            this.Level = Level;
            this.Name = Name;
            this.IsAlive = IsAlive;
        }
        public Player() { }
    }
}