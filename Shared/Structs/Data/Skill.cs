using System;

namespace Shared.Structs.Data
{
    public class Skill
    {
        #region Properties
        public byte Service { get;  set; }
        public int Id { get;  set; }
        public int GroupId { get;  set; }

        //Basic
        public string BasicCode { get;  set; }
        public string BasicName { get;  set; }
        public string BasicGroup { get;  set; }
        public int BasicOriginal { get;  set; }
        public byte BasicLevel { get;  set; }
        public byte BasicActivity { get;  set; }
        public int BasicChainCode { get;  set; }
        public int BasicRecycleCost { get;  set; }

        //Action
        public int ActionPreparingTime { get;  set; }
        public int ActionCastingTime { get;  set; }
        public int ActionActionDuration { get;  set; }
        public int ActionReuseDelay { get;  set; }
        public int ActionCoolTime { get;  set; }
        public int ActionFlyingSpeed { get;  set; }
        public byte ActionErruptable { get;  set; }
        public int ActionOverlap { get;  set; }
        public byte ActionAutoAttackType { get;  set; }
        public byte ActionInTown { get;  set; }
        public short ActionRange { get;  set; }

        
        //Target
        public byte TargetRequired { get;  set; }
        public byte TargetTypeAnimal { get;  set; }
        public byte TargetTypeLand { get;  set; }
        public byte TargetTypeBuilding { get;  set; }
        public byte TargetGroupSelf { get;  set; }
        public byte TargetGroupAlly { get;  set; }
        public byte TargetGroupParty { get;  set; }
        public byte TargetGroupEnemyM { get;  set; }
        public byte TargetGroupEnemyP { get;  set; }
        public byte TargetGroupNeutral { get;  set; }
        public byte TargetGroupDontCare { get;  set; }
        public byte TargetEtcSelectDeadBody { get;  set; }

        //Requirements
        /*public int ReqCommonMastery1 { get;  set; }
        public int ReqCommonMastery2 { get;  set; }
        public byte ReqCommonMasteryLevel1 { get;  set; }
        public byte ReqCommonMasteryLevel2 { get;  set; }
        public short ReqCommonStr { get;  set; }
        public short ReqCommon { get;  set; }
        public int ReqLearnSkill1 { get;  set; }
        public int ReqLearnSkill2 { get;  set; }
        public int ReqLearnSkill3 { get;  set; }
        public byte ReqLearnSkillLevel1 { get;  set; }
        public byte ReqLearnSkillLevel2 { get;  set; }
        public byte ReqLearnSkillLevel3 { get;  set; }
        public int ReqLearnSp { get;  set; }
        public byte ReqLearnRace { get;  set; }
        public byte ReqRestriction1 { get;  set; }
        public byte ReqRestriction2 { get;  set; }
        public byte ReqCastWeapon1 { get;  set; }
        public byte ReqCastWeapon2 { get;  set; }

        //Consume
        public short ConsumeHp { get;  set; }
        public short ConsumeMp { get;  set; }
        public short ConsumeHpRatio { get;  set; }
        public short ConsumeMpRatio { get;  set; }

        //UI
        public byte ConsumeWhan { get;  set; }
        public byte UiSkillTab { get;  set; }
        public byte UiSkillPage { get;  set; }
        public byte UiSkillColumn { get;  set; }
        public byte UiSkillRow { get;  set; }
        public string UiIconFile { get;  set; }*/
        public string UiSkillName { get;  set; }
        /*public string UiSkillToolTip { get;  set; }
        public string UiSkillToolTipDesc { get;  set; }
        public string UiSkillStudyDesc { get;  set; }*/

        //AI
        //public short AiAttackChance { get;  set; }
        //public byte AiSkillType { get;  set; }
        #endregion

        public Skill(string[] data, string name)
        {
            Service = Convert.ToByte(data[0]);
            Id = Convert.ToInt32(data[1]);
            GroupId = Convert.ToInt32(data[2]);
            BasicCode = data[3];
            BasicName = name;//data[4];
            BasicGroup = data[5];
            BasicOriginal = Convert.ToInt32(data[6]);
            BasicLevel = Convert.ToByte(data[7]);
            BasicActivity = Convert.ToByte(data[8]);
            BasicChainCode = Convert.ToInt32(data[9]);
            BasicRecycleCost = Convert.ToInt32(data[10]);
            ActionPreparingTime = Convert.ToInt32(data[11]);
            ActionCastingTime = Convert.ToInt32(data[12]);
            ActionActionDuration = Convert.ToInt32(data[13]);
            ActionReuseDelay = Convert.ToInt32(data[14]);
            ActionCoolTime = Convert.ToInt32(data[15]);
            ActionFlyingSpeed = Convert.ToInt32(data[16]);
            ActionErruptable = Convert.ToByte(data[17]);
            ActionOverlap = Convert.ToInt32(data[18]);
            ActionAutoAttackType = Convert.ToByte(data[19]);
            ActionInTown = Convert.ToByte(data[20]);
            ActionRange = Convert.ToInt16(data[21]);
            TargetRequired = Convert.ToByte(data[22]);
            TargetTypeAnimal = Convert.ToByte(data[23]);
            TargetTypeLand = Convert.ToByte(data[24]);
            TargetTypeBuilding = Convert.ToByte(data[25]);
            TargetGroupSelf = Convert.ToByte(data[26]);
            TargetGroupAlly = Convert.ToByte(data[27]);
            TargetGroupParty = Convert.ToByte(data[28]);
            TargetGroupEnemyM = Convert.ToByte(data[29]);
            TargetGroupEnemyP = Convert.ToByte(data[30]);
            TargetGroupNeutral = Convert.ToByte(data[31]);
            TargetGroupDontCare = Convert.ToByte(data[32]);
            TargetEtcSelectDeadBody = Convert.ToByte(data[33]);
            /*ReqCommonMastery1 = Convert.ToInt32(data[34]);
            ReqCommonMastery2 = Convert.ToInt32(data[35]);
            ReqCommonMasteryLevel1 = Convert.ToByte(data[36]);
            ReqCommonMasteryLevel2 = Convert.ToByte(data[37]);
            ReqCommonStr = Convert.ToInt16(data[38]);
            ReqCommon = Convert.ToInt16(data[39]);
            ReqLearnSkill1 = Convert.ToInt32(data[40]);
            ReqLearnSkill2 = Convert.ToInt32(data[41]);
            ReqLearnSkill3 = Convert.ToInt32(data[42]);
            ReqLearnSkillLevel1 = Convert.ToByte(data[43]);
            ReqLearnSkillLevel2 = Convert.ToByte(data[44]);
            ReqLearnSkillLevel3 = Convert.ToByte(data[45]);
            ReqLearnSp = Convert.ToInt32(data[46]);
            ReqLearnRace = Convert.ToByte(data[47]);
            ReqRestriction1 = Convert.ToByte(data[48]);
            ReqRestriction2 = Convert.ToByte(data[49]);
            ReqCastWeapon1 = Convert.ToByte(data[50]);
            ReqCastWeapon2 = Convert.ToByte(data[51]);
            ConsumeHp = Convert.ToInt16(data[52]);
            ConsumeMp = Convert.ToInt16(data[53]);
            ConsumeHpRatio = Convert.ToInt16(data[54]);
            ConsumeMpRatio = Convert.ToInt16(data[55]);
            ConsumeWhan = Convert.ToByte(data[56]);
            UiSkillTab = Convert.ToByte(data[57]);
            UiSkillColumn = Convert.ToByte(data[58]);
            UiSkillRow = Convert.ToByte(data[59]);
            UiIconFile = data[61];*/
            UiSkillName = data[62];
            //UiSkillToolTip = data[63];
            //UiSkillToolTipDesc = data[64];
        }
        
        public Skill()
        { }
    }
}