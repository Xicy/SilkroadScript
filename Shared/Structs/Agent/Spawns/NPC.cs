namespace Shared.Structs.Agent.Spawns
{
    public class NPC : Position
    {
        public Data.Character AssocNPC { get; set; }
        public uint ObjectId { get; set; }
        public string Name => AssocNPC.ObjName;

        public NPC(Data.Character AssocNPC, uint ObjectId, string Name)
        {
            this.AssocNPC = AssocNPC;
            this.ObjectId = ObjectId;
        }

        public NPC() { }

    }
}