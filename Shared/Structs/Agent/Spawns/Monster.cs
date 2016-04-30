namespace Shared.Structs.Agent.Spawns
{
    public class Monster : Position
    {

        public Data.Character AssocMonster { get; set; }
        public int Type { get; set; }
        public bool IsAlive { get; set; }
        public uint ObjectId { get; set; }
        public string Name => AssocMonster.ObjName;

        public Monster(Data.Character AssocMonster, uint ObjectId, bool IsAlive)
        {
            this.AssocMonster = AssocMonster;
            this.ObjectId = ObjectId;
            this.IsAlive = IsAlive;
        }

        public Monster() { }
    }
}