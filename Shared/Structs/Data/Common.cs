using System;

namespace Shared.Structs.Data
{
    public class Common
    {
        #region Properties

        public byte Service { get; set; }

        public int Id { get; set; }

        public string CodeName { get; set; }

        public string ObjName { get; set; }

        public string OrgObjCodeName { get; set; }

        public string NameStrId { get; set; }

        public string DescStrId { get; set; }

        public byte CashItem { get; set; }

        public byte Bionic { get; set; }

        public byte TypeID1 { get; set; }

        public byte TypeID2 { get; set; }

        public byte TypeID3 { get; set; }

        public byte TypeID4 { get; set; }

        public int DecayTime { get; set; }

        public byte Country { get; set; }

        public byte Rarity { get; set; }

        public byte CanTrade { get; set; }

        public byte CanSell { get; set; }

        public byte CanBuy { get; set; }

        public byte CanBorrow { get; set; }

        public byte CanDrop { get; set; }

        public byte CanPick { get; set; }

        public byte CanRepair { get; set; }

        public byte CanRevive { get; set; }

        public byte CanUse { get; set; }

        public byte CanThrow { get; set; }

        public int Price { get; set; }

        public int CostRepair { get; set; }

        public int CostRevive { get; set; }

        public int CostBorrow { get; set; }

        public int KeepingFee { get; set; }

        public int SellPrice { get; set; }

        public int ReqLevelType1 { get; set; }

        public byte ReqLevel1 { get; set; }

        public int ReqLevelType2 { get; set; }
        public byte ReqLevel2 { get; set; }
        public int ReqLevelType3 { get; set; }
        public byte ReqLevel3 { get; set; }
        public int ReqLevelType4 { get; set; }
        public byte ReqLevel4 { get; set; }
        public int MaxContain { get; set; }

        public short RegionID { get; set; }

        public short Dir { get; set; }

        public short OffsetX { get; set; }

        public short OffsetY { get; set; }

        public short OffsetZ { get; set; }

        public short Speed1 { get; set; }

        public short Speed2 { get; set; }

        public int Scale { get; set; }

        public short BCHeight { get; set; }

        public short BCRadius { get; set; }

        public int EventID { get; set; }

        public string AssocFileObj128 { get; set; }

        public string AssocFileDrop128 { get; set; }

        public string AssocFileIcon128 { get; set; }

        public string AssocFile1_128 { get; set; }

        public string AssocFile2_128 { get; set; }

        #endregion

        internal Common(string[] data)
        {
            if (data.Length < 56) return;
            Service = Convert.ToByte(data[0]);
            Id = Convert.ToInt32(data[1]);

            CodeName = data[2];
            ObjName = data[3];
            OrgObjCodeName = data[4];
            NameStrId = data[5];
            DescStrId = data[6];

            CashItem = Convert.ToByte(data[7]);
            Bionic = Convert.ToByte(data[8]);

            TypeID1 = Convert.ToByte(data[9]);
            TypeID2 = Convert.ToByte(data[10]);
            TypeID3 = Convert.ToByte(data[11]);
            TypeID4 = Convert.ToByte(data[12]);

            DecayTime = Convert.ToInt32(data[13]);

            Country = Convert.ToByte(data[14]);
            Rarity = Convert.ToByte(data[15]);
            CanTrade = Convert.ToByte(data[16]);
            CanSell = Convert.ToByte(data[17]);
            CanBuy = Convert.ToByte(data[18]);
            CanBorrow = Convert.ToByte(data[19]);
            CanDrop = Convert.ToByte(data[20]);
            CanPick = Convert.ToByte(data[21]);
            CanRepair = Convert.ToByte(data[22]);
            CanRevive = Convert.ToByte(data[23]);
            CanUse = Convert.ToByte(data[24]);
            CanThrow = Convert.ToByte(data[25]);

            Price = Convert.ToInt32(data[26]);
            CostRepair = Convert.ToInt32(data[27]);
            CostRevive = Convert.ToInt32(data[28]);
            CostBorrow = Convert.ToInt32(data[29]);
            KeepingFee = Convert.ToInt32(data[30]);
            SellPrice = Convert.ToInt32(data[31]);

            ReqLevelType1 = Convert.ToInt32(data[32]);
            ReqLevel1 = Convert.ToByte(data[33]);
            ReqLevelType2 = Convert.ToInt32(data[34]);
            ReqLevel2 = Convert.ToByte(data[35]);
            ReqLevelType3 = Convert.ToInt32(data[36]);
            ReqLevel3 = Convert.ToByte(data[37]);
            ReqLevelType4 = Convert.ToInt32(data[38]);
            ReqLevel4 = Convert.ToByte(data[39]);

            MaxContain = Convert.ToInt32(data[40]);

            RegionID = Convert.ToInt16(data[41]);
            Dir = Convert.ToInt16(data[42]);
            OffsetX = Convert.ToInt16(data[43]);
            OffsetY = Convert.ToInt16(data[44]);
            OffsetZ = Convert.ToInt16(data[45]);
            Speed1 = Convert.ToInt16(data[46]);
            Speed2 = Convert.ToInt16(data[47]);
            Scale = Convert.ToInt32(data[48]);

            BCHeight = Convert.ToInt16(data[49]);
            BCRadius = Convert.ToInt16(data[50]);

            EventID = Convert.ToInt32(data[51]);

            AssocFileObj128 = data[52];
            AssocFileDrop128 = data[53];
            AssocFileIcon128 = data[54];
            AssocFile1_128 = data[55];
            AssocFile2_128 = data[56];
        }

        protected Common()
        {
        }
    }
}