namespace Shared.Structs.Agent.Spawns
{
    public class Item : Position
    {
        public Data.Item AssocItem { get; set; }
        public uint ObjectId { get; set; }

        public string Name => AssocItem.ObjName;

        public uint OwnerId { get; set; }

        public byte Plus { get; set; }

        public bool IsBlue { get; set; }

        public uint Amount { get; set; }

        public Item(Data.Item AssocItem, uint OwnerId, byte Plus, bool IsBlue, uint Amount, uint ObjectId)
        {
            this.AssocItem = AssocItem;
            this.OwnerId = OwnerId;
            this.Plus = Plus;
            this.IsBlue = IsBlue;
            this.ObjectId = ObjectId;
            this.Amount = Amount;
        }
        public Item() { }
    }
}