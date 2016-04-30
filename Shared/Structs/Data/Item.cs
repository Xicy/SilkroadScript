using System;
using System.Collections.Generic;

namespace Shared.Structs.Data
{
    public class Item : Common
    {
        #region  Properties

        public int MaxStack { get; set; }

        public byte ReqGender { get; set; }

        public int ReqStr { get; set; }

        public int ReqInt { get; set; }

        public byte ItemClass { get; set; }

        public int SetID { get; set; }

        //Durability

        public float Dur_L { get; set; }

        public float Dur_U { get; set; }

        //Physical Defense

        public float PD_L { get; set; }

        public float PD_U { get; set; }

        public float PDInc { get; set; }

        //Evasion Rate (Parry Rate)

        public float ER_L { get; set; }

        public float ER_U { get; set; }

        public float ERInc { get; set; }

        //Physical Absorb Rate
        public float PAR_L { get; set; }

        public float PAR_U { get; set; }

        public float PARInc { get; set; }

        //Block Rate

        public float BR_L { get; set; }

        public float BR_U { get; set; }

        //Magical Defense

        public float MD_L { get; private set; }

        public float MD_U { get; private set; }

        public float MDInc { get; private set; }

        //Magical Absorb Rate

        public float MAR_L { get; set; }

        public float MAR_U { get; set; }

        public float MARInc { get; set; }

        //Physical Defense

        public float PDStr_L { get; set; }

        public float PDStr_U { get; set; }

        //Magical Defense

        public float MDInt_L { get; set; }

        public float MDInt_U { get; set; }

        //Ammo System

        public byte Quivered { get; set; }

        public byte Ammo1_TID4 { get; set; }

        public byte Ammo2_TID4 { get; set; }

        public byte Ammo3_TID4 { get; set; }

        public byte Ammo4_TID4 { get; set; }

        public byte Ammo5_TID4 { get; set; }

        public byte SpeedClass { get; set; }

        public byte TwoHanded { get; set; }

        public short Range { get; set; }

        //Physical Attack

        public float PAttackMin_L { get; set; }

        public float PAttackMin_U { get; set; }

        public float PAttackMax_L { get; set; }

        public float PAttackMax_U { get; set; }

        public float PAttackInc { get; set; }

        //Magical Attack

        public float MAttackMin_L { get; set; }

        public float MAttackMin_U { get; set; }

        public float MAttackMax_L { get; set; }

        public float MAttackMax_U { get; set; }

        public float MAttackInc { get; set; }

        //Physical Reinforce

        public float PAStrMin_L { get; set; }

        public float PAStrMin_U { get; set; }

        public float PAStrMax_L { get; set; }

        public float PAStrMax_U { get; set; }

        //Magical Reinforce

        public float MAIntMin_L { get; set; }

        public float MAIntMin_U { get; set; }

        public float MAIntMax_L { get; set; }

        public float MAIntMax_U { get; set; }

        //Hit Rate (Attack Rate)

        public float HR_L { get; set; }

        public float HR_U { get; set; }

        public float HRInc { get; set; }



        public float CHR_L { get; set; }

        public float CHR_U { get; set; }

        public List<int> Params { get; private set; }

        public byte MaxMagicOptCount { get; set; }

        public byte ChildItemCount { get; set; }

        #endregion

        public Item(string[] data,string name) : base(data)
        {
            ObjName = name;
            MaxStack = Convert.ToInt32(data[57]);
            ReqGender = Convert.ToByte(data[58]);
            ReqStr = Convert.ToInt32(data[59]);
            ReqInt = Convert.ToInt32(data[60]);
            ItemClass = Convert.ToByte(data[61]);
            SetID = Convert.ToInt32(data[62]);

            Dur_L = Convert.ToSingle(data[63]);
            Dur_U = Convert.ToSingle(data[64]);

            PD_L = Convert.ToSingle(data[65]);
            PD_U = Convert.ToSingle(data[66]);
            PDInc = Convert.ToSingle(data[67]);

            ER_L = Convert.ToSingle(data[68]);
            ER_U = Convert.ToSingle(data[69]);
            ERInc = Convert.ToSingle(data[70]);

            PAR_L = Convert.ToSingle(data[71]);
            PAR_U = Convert.ToSingle(data[72]);
            PARInc = Convert.ToSingle(data[73]);

            BR_L = Convert.ToSingle(data[74]);
            BR_U = Convert.ToSingle(data[75]);

            MD_L = Convert.ToSingle(data[76]);
            MD_U = Convert.ToSingle(data[77]);
            MDInc = Convert.ToSingle(data[78]);

            MAR_L = Convert.ToSingle(data[79]);
            MAR_U = Convert.ToSingle(data[80]);
            MARInc = Convert.ToSingle(data[81]);

            PDStr_L = Convert.ToSingle(data[82]);
            PDStr_U = Convert.ToSingle(data[83]);

            MDInt_L = Convert.ToSingle(data[84]);
            MDInt_U = Convert.ToSingle(data[85]);

            Quivered = Convert.ToByte(data[86]);
            Ammo1_TID4 = Convert.ToByte(data[87]);
            Ammo2_TID4 = Convert.ToByte(data[88]);
            Ammo3_TID4 = Convert.ToByte(data[89]);
            Ammo4_TID4 = Convert.ToByte(data[90]);
            Ammo5_TID4 = Convert.ToByte(data[91]);

            SpeedClass = Convert.ToByte(data[92]);
            TwoHanded = Convert.ToByte(data[93]);
            Range = Convert.ToInt16(data[94]);

            PAttackMin_L = Convert.ToSingle(data[95]);
            PAttackMin_U = Convert.ToSingle(data[96]);
            PAttackMax_L = Convert.ToSingle(data[97]);
            PAttackMax_U = Convert.ToSingle(data[98]);
            PAttackInc = Convert.ToSingle(data[99]);

            MAttackMin_L = Convert.ToSingle(data[100]);
            MAttackMin_U = Convert.ToSingle(data[101]);
            MAttackMax_L = Convert.ToSingle(data[102]);
            MAttackMax_U = Convert.ToSingle(data[103]);
            MAttackInc = Convert.ToSingle(data[104]);

            PAStrMin_L = Convert.ToSingle(data[105]);
            PAStrMin_U = Convert.ToSingle(data[106]);
            PAStrMax_L = Convert.ToSingle(data[107]);
            PAStrMax_U = Convert.ToSingle(data[108]);

            MAIntMin_L = Convert.ToSingle(data[109]);
            MAIntMin_U = Convert.ToSingle(data[110]);
            MAIntMax_L = Convert.ToSingle(data[111]);
            MAIntMax_U = Convert.ToSingle(data[112]);

            HR_L = Convert.ToSingle(data[113]);
            HR_U = Convert.ToSingle(data[114]);
            HRInc = Convert.ToSingle(data[115]);

            CHR_L = Convert.ToSingle(data[116]);
            CHR_U = Convert.ToSingle(data[117]);

            /*Params = new List<int>();
            for (int i = 118; i < 157; i += 2)
            {
                Params.Add(Convert.ToInt32(data[i]));
            }*/

            //MaxMagicOptCount = Convert.ToByte(data[158]);
            //ChildItemCount = Convert.ToByte(data[159]);
        }

        public Item() 
        {
        }
    }
}

//data[57] = 1  MaxStack	int
//data[58] = 2  ReqGender	tinyint
//data[59] = 0  ReqStr	int
//data[60] = 0  ReqInt	int
//data[61] = 1  ItemClass	tinyint
//data[62] = 0  SetID	int
//data[63] = 62.0  Dur_L	real
//data[64] = 76.0  Dur_U	real
//data[65] = 0.0  PD_L	real
//data[66] = 0.0  PD_U	real
//data[67] = 0.0  PDInc	real
//data[68] = 0.0  ER_L	real
//data[69] = 0.0  ER_U	real
//data[70] = 0.0  ERInc	real
//data[71] = 0.0  PAR_L	real
//data[72] = 0.0  PAR_U	real
//data[73] = 0.0  PARInc	real
//data[74] = 0.0  BR_L	real
//data[75] = 0.0  BR_U	real
//data[76] = 0.0  MD_L	real
//data[77] = 0.0  MD_U	real
//data[78] = 0.0  MDInc	real
//data[79] = 0.0  MAR_L	real
//data[80] = 0.0  MAR_U	real
//data[81] = 0.0  MARInc	real	
//data[82] = 0.0  PDStr_L	real
//data[83] = 0.0  PDStr_U	real
//data[84] = 0.0  MDInt_L	real
//data[85] = 0.0  MDInt_U	real
//data[86] = 0  Quivered	tinyint
//data[87] = 0  Ammo1_TID4	tinyint
//data[88] = 0  Ammo2_TID4	tinyint
//data[89] = 0  Ammo3_TID4	tinyint
//data[90] = 0  Ammo4_TID4	tinyint
//data[91] = 0  Ammo5_TID4	tinyint
//data[92] = 2  SpeedClass	tinyint
//data[93] = 0  TwoHanded	tinyint
//data[94] = 6  Range	smallint
//data[95] = 15.0  PAttackMin_L	real
//data[96] = 16.0  PAttackMin_U	real
//data[97] = 16.0  PAttackMax_L	real
//data[98] = 18.0  PAttackMax_U	real
//data[99] = 2.4  PAttackInc	real
//data[100] = 25.0  MAttackMin_L	real
//data[101] = 26.0  MAttackMin_U	real
//data[102] = 28.0  MAttackMax_L	real
//data[103] = 31.0  MAttackMax_U	real
//data[104] = 4.1  MAttackInc	real
//data[105] = 306.0  PAStrMin_L	real
//data[106] = 322.0  PAStrMin_U	real
//data[107] = 342.0  PAStrMax_L	real
//data[108] = 366.0  PAStrMax_U	real
//data[109] = 519.0  MAInt_Min_L	real
//data[110] = 551.0  MAInt_Min_U	real
//data[111] = 591.0  MAInt_Max_L	real
//data[112] = 639.0  MAInt_Max_U	real
//data[113] = 24.0  HR_L	real
//data[114] = 30.0  HR_U	real
//data[115] = 0.0  HRInc	real
//data[116] = 3.0  CHR_L	real
//data[117] = 15.0  CHR_U	real
//data[118] = -1  Param1	int
//data[119] = xxx  Desc1_128	varchar(129)
//data[120] = -1  Param2	int
//data[121] = xxx  Desc2_128	char(129)
//data[122] = -1  Param3	int
//data[123] = xxx  Desc3_128	varchar(129)
//data[124] = -1  Param4	int
//data[125] = xxx  Desc4_128	varchar(129)
//data[126] = -1  Param5	int
//data[127] = xxx  Desc5_128	varchar(129)
//data[128] = -1  Param6	int
//data[129] = xxx  Desc6_128	varchar(129)
//data[130] = -1  Param7	int
//data[131] = xxx  Desc7_128	varchar(129)
//data[132] = -1  Param8	int	
//data[133] = xxx  Desc8_128	varchar(129)
//data[134] = -1  Param9	int
//data[135] = xxx  Desc9_128	varchar(129)
//data[136] = -1  Param10	int	
//data[137] = xxx  Desc10_128	varchar(129)
//data[138] = -1  Param11	int	
//data[139] = xxx  Desc11_128	varchar(129)
//data[140] = -1  Param12	int	
//data[141] = xxx  Desc12_128	varchar(129)
//data[142] = -1  Param13	int	
//data[143] = xxx  Desc13_128	varchar(129)
//data[144] = -1  Param14	int	
//data[145] = xxx  Desc14_128	varchar(129)
//data[146] = -1  Param15	int	
//data[147] = xxx  Desc15_128	varchar(129)
//data[148] = -1  Param16	int	
//data[149] = xxx  Desc16_128	varchar(129)
//data[150] = -1  Param17	int	
//data[151] = xxx  Desc17_128	varchar(129)
//data[152] = -1  Param18	int	
//data[153] = xxx  Desc18_128	varchar(129)
//data[154] = -1  Param19	int	
//data[155] = xxx  Desc19_128	varchar(129)
//data[156] = 0  Param20	int	
//data[157] = ?? ??? ?? ?? ?  Desc20_128	varchar(129)
//data[158] = 9  MaxMagicOptCount	tinyint
//data[159] = 0  ChildItemCount	tinyint
