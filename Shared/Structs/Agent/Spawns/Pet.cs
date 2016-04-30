namespace Shared.Structs.Agent.Spawns
{
    public class Pet : Position
    {
        public Data.Character AssocPet { get; set; } = null;
        public bool IsAlive { get; set; }

        public uint ObjectId { get; set; }

        public string Name { get; set; }

        public string OwnerName { get; set; }

        public Pet(Data.Character AssocPet, bool IsAlive, uint ObjectId, string Name, string OwnerName)
        {
            this.AssocPet = AssocPet;
            this.IsAlive = IsAlive;
            this.ObjectId = ObjectId;
            this.Name = Name;
            this.OwnerName = OwnerName;
        }

        public Pet() { }

    }
}