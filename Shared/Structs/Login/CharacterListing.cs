namespace Shared.Structs
{
    public struct CharacterListing
    {
        public CharacterListing(int modelId, string name, byte volume, byte level, long exp, ushort strength, ushort intelligence, ushort statPoints, uint hp, uint mp, bool isInDeletion)
        {
            IsInDeletion = isInDeletion;
            MP = mp;
            HP = hp;
            StatPoints = statPoints;
            Intelligence = intelligence;
            Strength = strength;
            EXP = exp;
            Level = level;
            Volume = volume;
            Name = name;
            ModelId = modelId;
        }

        public int ModelId { get; set; }
        public string Name { get; set; }
        public byte Volume { get; set; }
        public byte Level { get; set; }
        public long EXP { get; set; }
        public ushort Strength { get; set; }
        public ushort Intelligence { get; set; }
        public ushort StatPoints { get; set; }
        public uint HP { get; set; }
        public uint MP { get; set; }
        public bool IsInDeletion { get; set; }
    }
}