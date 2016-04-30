namespace Shared
{
    public static class Types
    {
        public enum Skill
        {
            Partybuff,
            Selfbuff,
            Playerbuff,
            Attack,
            Passive,
            Other
        }
        public enum Monster : byte
        {
            Normal = 0x00,
            Champion = 0x01,
            Unique = 0x03,
            Giant = 0x04,
            Titan = 0x05,
            Elite1 = 0x06,
            Elite2 = 0x07,
            Party = 0x10,
            PartyChampion = 0x11,
            PartyGiant = 0x14
        }
        public enum Weather
        {
            Sun = 1,
            Rain = 2,
            Snow = 3
        }
        public enum Party : byte
        {
            //Item share, EXP share, Allow invitation
            FREE_FREE_NO = 0x00,
            FREE_SHARE_NO = 0x01,
            SHARE_FREE_NO = 0x02,
            SHARE_SHARE_NO = 0x03,
            FREE_FREE_YES = 0x04,
            FREE_SHARE_YES = 0x05,
            SHARE_FREE_YES = 0x06,
            SHARE_SHARE_YES = 0x07
        }
        public enum PartyPurpose : byte
        {
            HUNT = 0x01,
            QUEST = 0x02,
            TRADE = 0x03,
            THIEF = 0x04
        }
    }
}