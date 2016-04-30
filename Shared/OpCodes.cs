namespace Shared
{
    public static class OpCodes
    {
        public enum Machine : ushort
        {
            Handshake = 0x5000,
            AgentServer = 0x2001,
            ServerList = 0x6101,
            UpdateServer = 0x6102,
            Ping = 0x2002,

            UserOnlineCheck = 0x8103,
            UserTempidCheck = 0x8104
        }

        public enum AgentServer : ushort
        {
            Handshake = 0x5000,
            AgentServer = 0x2001,
            PatchInfo = 0x600D,
            GameLogin = 0xA103,
            Login = 0xA102,
            CharacterSelect = 0xB001,

            SingleDespawn = 0x3016,

            ChardataBegin = 0x34A5,
            Chardata = 0x3013,
            ChardataEnd = 0x34A6,

            ItemWear = 0x3038,
            ItemUnwear = 0x3039,

            MasteryUp = 0xB0A2,
            SkillUp = 0xB0A1,
            StrUp = 0xB050,
            IntUp = 0xB051,

            SpeedUpdate = 0x30D0,
            StuffUpdate = 0x304E,
            ExpSpUpdate = 0x3056,
            CharacterInfo = 0x303D,

            ObjectSelect = 0xB045,
            NpcSelect = 0xB046,
            NpcDeselect = 0xB04B,

            ObjectAction = 0xB074,

            Disconnect = 0xB005,

            Move = 0xB021,
            Stuck = 0xB023,

            GroupSpawnBegin = 0x3017,
            GroupSpawnEnd = 0x3018,
            SingleSpawn = 0x3015,
            GroupeSpawn = 0x3019,
            CharacterListing = 0xB007,
            FortressInfo = 0x385F,

            VisualAngle = 0xB024,
            VisualPickup = 0x3036,
            VisualEmotions = 0x3091,
            VisualEffect = 0x30BF,

            BuffInfo = 0xB0BD,
            BuffDell = 0xB072,
            InventoryMovement = 0xB034,
            HpMpUpdate = 0x3057,
            Chat = 0x3026,
            ChatCount = 0xB025,
            PartyInvitation = 0x3080,
            PartyMatching = 0xB06C,

            TeleportResponse = 0xB05A
        }

        public enum AgentClient : ushort
        {
            HandshakeOk = 0x9000,
            Handshake = 0x5000,
            AgentServer = 0x2001,
            PatchRequest = 0x6100,
            Login = 0x6103,

            MasteryUp = 0x70A2,
            SkillUp = 0x70A1,
            StrUp = 0x7050,
            IntUp = 0x7051,

            ObjectSelect = 0x7045,
            NpcSelect = 0x7046,
            NpcDeselect = 0x704B,

            Disconnect = 0x7005,
            CharacterListing = 0x7007,
            SelectCharacter = 0x7001,
            ObjectAction = 0x7074,

            InventoryMovement = 0x7034,
            ConfirmSpawn = 0x3012,
            Repair = 0x703E,
            KillHorse = 0x70C6,
            HorseMove = 0x70C5,

            Chat = 0x7025,
            DropGold = 0x7034,
            Party = 0x3080,
            PartyLeave = 0x7061,

            Movement = 0x7021,
            VisualAngle = 0x7024,
            VisualEmotions = 0x3091,
            Sit = 0x704F,
            Stuck = 0x7023,

            JoinParty = 0x706D,
            Ping = 0x2002,

            TeleportRequest = 0x705A,
            TeleportAccept = 0x34B6
        }

        public enum GatewayServer : ushort
        {
            Handshake = 0x5000,
            Blowfish = 0x8000,
            AgentServer = 0x2001,
            ServerList = 0xA101,
            PatchInfo = 0x600D,
            GameLoginReply = 0xA103,
            LoginReply = 0xA102,
            LocationReply = 0xA107,
            Captcha = 0x2322
        }

        public enum GatewayClient : ushort
        {
            Handshake = 0x5000,
            HandshakeOk = 0x9000,
            AgentServer = 0x2001,
            PatchRequest = 0x6100,
            Ping = 0x2002,
            GameLogin = 0x6103,
            LauncherNews = 0x6104,
            RequestServerList = 0x6101,
            Login = 0x6102,
            LocationRequest = 0x6107,
            Captcha = 0x6323
        }
    }
}