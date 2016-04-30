using System;

namespace Shared.Structs.Data
{
    public class Character : Common
    {
        public byte Level { get; set; }

        public byte CharGender { get; set; }

        public int MaxHP { get; set; }

        public int MaxMP { get; set; }

        public byte InventorySize { get; set; }

        public byte CanStore_TID1 { get; set; }

        public byte CanStore_TID2 { get; set; }

        public byte CanStore_TID3 { get; set; }

        public byte CanStore_TID4 { get; set; }

        public byte CanBeVehicle { get; set; }

        public byte CanControl { get; set; }

        public byte DamagePortion { get; set; }

        public short MaxPassenger { get; set; }

        public int AssocTactics { get; set; }

        public int PD { get; set; }

        public int MD { get; set; }

        public int PAR { get; set; }

        public int MAR { get; set; }

        public int ER { get; set; }

        public int BR { get; set; }

        public int HR { get; set; }

        public int CHR { get; set; }

        public int ExpToGive { get; set; }

        public int CreepType { get; set; }

        public byte Knockdown { get; set; }

        public int KO_RecoverTime { get; set; }


        public Character(string[] data, string name) : base(data)
        {
            ObjName = name;
            Level = Convert.ToByte(data[57]);
            CharGender = Convert.ToByte(data[58]);
            MaxHP = Convert.ToInt32(data[59]);
            MaxMP = Convert.ToInt32(data[60]);
            InventorySize = Convert.ToByte(data[61]);
            CanStore_TID1 = Convert.ToByte(data[62]);
            CanStore_TID2 = Convert.ToByte(data[63]);
            CanStore_TID3 = Convert.ToByte(data[64]);
            CanStore_TID4 = Convert.ToByte(data[65]);
            CanBeVehicle = Convert.ToByte(data[66]);
            CanControl = Convert.ToByte(data[67]);
            DamagePortion = Convert.ToByte(data[68]);
            MaxPassenger = Convert.ToInt16(data[69]);
            AssocTactics = Convert.ToInt32(data[70]);
            PD = Convert.ToInt32(data[71]);
            MD = Convert.ToInt32(data[72]);
            PAR = Convert.ToInt32(data[73]);
            MAR = Convert.ToInt32(data[74]);
            ER = Convert.ToInt32(data[75]);
            BR = Convert.ToInt32(data[76]);
            HR = Convert.ToInt32(data[77]);
            CHR = Convert.ToInt32(data[78]);
            ExpToGive = Convert.ToInt32(data[79]);
            CreepType = Convert.ToInt32(data[80]);
            Knockdown = Convert.ToByte(data[81]);
            KO_RecoverTime = Convert.ToInt32(data[82]);

        }

        public Character()
        { }
    }
}

//data[57] = 1  Lvl	tinyint	
//data[58] = 1  CharGender	tinyint	
//data[59] = 0  MaxHP	int	
//data[60] = 0  MaxMP	int	
//data[61] = 45  InventorySize	tinyint	
//data[62] = 0  CanStore_TID1	tinyint	
//data[63] = 0  CanStore_TID2	tinyint	
//data[64] = 0  CanStore_TID3	tinyint	
//data[65] = 0  CanStore_TID4	tinyint	
//data[66] = 0  CanBeVehicle	tinyint	
//data[67] = 0  CanControl	tinyint	
//data[68] = 0  DamagePortion	tinyint	
//data[69] = 0  MaxPassenger	smallint	
//data[70] = 0  AssocTactics	int	
//data[71] = 0  PD	int	
//data[72] = 0  MD	int	
//data[73] = 0  PAR	int	
//data[74] = 0  MAR	int	
//data[75] = 0  ER	int	
//data[76] = 0  BR	int	
//data[77] = 0  HR	int	
//data[78] = 0  CHR	int	
//data[79] = 0  ExpToGive	int	
//data[80] = 336860180  CreepType	int	
//data[81] = 3  Knockdown	tinyint	
//data[82] = 3000  KO_RecoverTime	int	
//data[83] = 1  DefaultSkill_1	int	Checked
//data[84] = 2  DefaultSkill_2	int	Checked
//data[85] = 40  DefaultSkill_3	int	Checked
//data[86] = 70  DefaultSkill_4	int	Checked
//data[87] = 0  DefaultSkill_5	int	Checked
//data[88] = 0  DefaultSkill_6	int	Checked
//data[89] = 0  DefaultSkill_7	int	Checked
//data[90] = 0  DefaultSkill_8	int	Checked
//data[91] = 0  DefaultSkill_9	int	Checked
//data[92] = 0  DefaultSkill_10	int	Checked
//data[93] = 0  TextureType	tinyint	Checked
//data[94] = 0  Except_1	int	Checked
//data[95] = 0  Except_2	int	Checked
//data[96] = 0  Except_3	int	Checked
//data[97] = 0  Except_4	int	Checked
//data[98] = 0  Except_5	int	Checked
//data[99] = 0  Except_6	int	Checked
//data[100] = 0  Except_7	int	Checked
//data[101] = 0  Except_8	int	Checked
//data[102] = 0  Except_9	int	Checked
//data[103] = 0  Except_10	int	Checked
