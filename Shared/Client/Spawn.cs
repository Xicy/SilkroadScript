using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Shared.Structs.Agent.Spawns;

namespace Shared
{
    public partial class Client
    {
        public static SynchronizedList<Player> Players = new List<Player>();
        public static SynchronizedList<Pet> Pets = new List<Pet>();
        public static SynchronizedList<NPC> NPCs = new List<NPC>();
        public static SynchronizedList<Item> Items = new List<Item>();
        public static SynchronizedList<Monster> Monsters = new List<Monster>();


        public static void DecodeSingleSpawn(Packet p)
        {
            try
            {
                var model = p.ReadUInt();
                var character = Data.Characters.FindById((int)model);
                if (character == null)
                {
                    #region Is Item

                    var item = Data.Items.FindById((int)model);
                    if (item != null) //YES!
                    {

                        var CurrentItem = new Structs.Agent.Spawns.Item();
                        CurrentItem.AssocItem = item;
                        if (item.CodeName.StartsWith("ITEM_ETC_GOLD"))
                        {
                            CurrentItem.Amount = p.ReadUInt();
                        }
                        else if (item.CodeName.StartsWith("ITEM_QSP"))
                        {
                            p.ReadAscii(); //Owner name
                        }
                        else if (item.CodeName.StartsWith("ITEM_CH") || item.CodeName.StartsWith("ITEM_EU"))
                        {
                            CurrentItem.Plus = p.ReadByte();
                        }
                        CurrentItem.ObjectId = p.ReadUInt();
                        CurrentItem.XSector = p.ReadByte();
                        CurrentItem.YSector = p.ReadByte();
                        CurrentItem.XOffset = p.ReadFloat();
                        CurrentItem.ZOffset = p.ReadFloat();
                        CurrentItem.YOffset = p.ReadFloat();
                        p.ReadShort(); //ANGLE
                        if (p.ReadByte() == 1) //Has owner?
                        {
                            CurrentItem.OwnerId = p.ReadUInt();
                        }
                        if (p.ReadByte() == 1)
                        {
                            CurrentItem.IsBlue = true;
                        }
                        p.ReadByte(); //??
                        p.ReadUInt(); //Dropper UID
                        Items.Add(CurrentItem);
                    }

                    #endregion
                }
                else
                {
                    if (character != null)
                    {
                        #region Monster //CHECKED!

                        if (character.CodeName.StartsWith("MOB"))
                        {
                            var CurrentMonster = new Structs.Agent.Spawns.Monster();

                            uint ObjectId = p.ReadUInt();
                            byte xSec = p.ReadByte();
                            byte ySec = p.ReadByte();
                            float xCoordinate = p.ReadFloat();
                            float zCoordinate = p.ReadFloat();
                            float yCoordinate = p.ReadFloat();
                            p.ReadShort(); //Angle
                            byte HasDestination = p.ReadByte(); //Has Destination
                            p.ReadByte(); //Walking flag (00: walking 01: running)
                            if (HasDestination == 1)
                            {
                                xSec = p.ReadByte();
                                ySec = p.ReadByte();

                                if (ySec == 0x80)
                                {
                                    xCoordinate = p.ReadFloat() - p.ReadFloat();
                                    p.ReadShort();
                                    p.ReadShort();
                                    yCoordinate = p.ReadFloat() - p.ReadFloat();
                                }
                                else //Destination X,Y,Z
                                {
                                    xCoordinate = p.ReadFloat();
                                    zCoordinate = p.ReadFloat();
                                    yCoordinate = p.ReadFloat();
                                }

                            }
                            else
                            {
                                p.ReadByte(); //No destination
                                p.ReadShort(); //direction Angle
                            }

                            if (p.ReadByte() == 2) //death flag
                                CurrentMonster.IsAlive = false;

                            //p.ReadByte();
                            //byte move = p.ReadByte(); //Move flag
                            //byte zerk = p.ReadByte(); //Berzerk flag
                            //float walkS = p.ReadFloat(); //walk speed
                            //float runS = p.ReadFloat(); //run speed;
                            //float zerkS = p.ReadFloat(); //zerk speed
                            //byte buffCount = p.ReadByte();

                            //for (int i = 0; i < buffCount; i++)
                            //{
                            //    p.ReadUInt(); //Buff Id
                            //    p.ReadUInt(); //Duration (in ms)
                            //}
                            //p.ReadByte(); // Name type
                            //byte Type = p.ReadByte(); // Monster type (general, champ...)

                            CurrentMonster.ObjectId = ObjectId;
                            CurrentMonster.AssocMonster = character;
                            CurrentMonster.XSector = xSec;
                            CurrentMonster.YSector = ySec;
                            CurrentMonster.XOffset = xCoordinate;
                            CurrentMonster.YOffset = yCoordinate;
                            CurrentMonster.ZOffset = zCoordinate;
                            Monsters.Add(CurrentMonster);
                        }
                        #endregion
                        #region Pet

                        else if (character.CodeName.StartsWith("COS"))
                        {
                           var CurrentPet = new Pet();

                            uint UniqueId = p.ReadUInt();
                            byte xSec = p.ReadByte();
                            byte ySec = p.ReadByte();
                            float xCoordinate = p.ReadFloat();
                            float zCoordinate = p.ReadFloat();
                            float yCoordinate = p.ReadFloat();
                            p.ReadShort();
                            byte HasDestination = p.ReadByte(); //Has Destination
                            p.ReadByte(); //Walking flag (00: walking 01: running)
                            if (HasDestination == 1)
                            {
                                xSec = p.ReadByte();
                                ySec = p.ReadByte();

                                //if (ySec == 0x80)
                                //{
                                //    xCoordinate = p.ReadFloat() - p.ReadFloat();
                                //    p.ReadShort();
                                //    p.ReadShort();
                                //    yCoordinate = p.ReadFloat() - p.ReadFloat();
                                //}
                                //else
                                //{
                                xCoordinate = p.ReadFloat();
                                zCoordinate = p.ReadFloat();
                                yCoordinate = p.ReadFloat();
                                //}

                            }
                            else
                            {
                                p.ReadByte(); //No destination
                                p.ReadShort(); //direction Angle
                            }

                            if (p.ReadByte() == 2) //death flag
                                CurrentPet.IsAlive = false;

                            p.ReadByte();
                            p.ReadByte(); //Move flag
                            p.ReadByte(); //Berzerk flag
                            p.ReadFloat(); //walk speed
                            p.ReadFloat(); //run speed;
                            p.ReadFloat(); //zerk speed
                            byte buffCount = p.ReadByte();
                            for (int i = 0; i < buffCount; i++)
                            {
                                p.ReadUInt(); //Buff Id
                                p.ReadUInt(); //Duration (in ms)
                            }
                            p.ReadByte(); // Name type
                            CurrentPet.Name = p.ReadAscii();
                            CurrentPet.ObjectId = UniqueId;
                            if (CurrentPet.Name == string.Empty)
                                CurrentPet.Name = "No Name";
                            CurrentPet.OwnerName = p.ReadAscii();

                            CurrentPet.AssocPet = character;
                            CurrentPet.XOffset = xCoordinate;
                            CurrentPet.YOffset = yCoordinate;
                            CurrentPet.ZOffset = zCoordinate;
                            CurrentPet.XSector = xSec;
                            CurrentPet.YSector = ySec;
                           Pets.Add(CurrentPet);
                        }
                        #endregion
                        #region NPC

                        else if (character.CodeName.StartsWith("NPC"))
                        {
                            var CurrentNPC = new NPC();
                            uint ObjectId = p.ReadUInt();
                            byte xSec = p.ReadByte();
                            byte ySec = p.ReadByte();
                            float xCoordinate = p.ReadFloat();
                            float zCoordinate = p.ReadFloat();
                            float yCoordinate = p.ReadFloat();

                            CurrentNPC.AssocNPC = character;
                            CurrentNPC.ObjectId = ObjectId;
                            CurrentNPC.XOffset = xCoordinate;
                            CurrentNPC.YOffset = yCoordinate;
                            CurrentNPC.ZOffset = zCoordinate;
                            CurrentNPC.XSector = xSec;
                            CurrentNPC.YSector = ySec;

                            NPCs.Add(CurrentNPC);

                        }
                        #endregion
                        #region Player

                        else if (character.CodeName.StartsWith("CHAR"))
                        {
                            var CurrentPlayer = new Structs.Agent.Spawns.Player();
                            p.ReadByte(); //VOLUME
                            p.ReadShort(); //Noob icon/Knight?
                            p.ReadByte(); //Max item slot
                            p.ReadByte();
                            byte itemCount = p.ReadByte();
                            for (int i = 0; i < itemCount; i++)
                            {
                                short itemId = p.ReadShort();
                                var item = Data.Items.FindById((int)itemId);
                                if (item != null)
                                {
                                    if (item.CodeName.StartsWith("ITEM_EU") || item.CodeName.StartsWith("ITEM_CH"))
                                    {
                                        byte plus = p.ReadByte();
                                        p.ReadShort();
                                    }
                                }
                            }

                            p.ReadByte(); //Max avatars
                            byte avatarCount = p.ReadByte();
                            for (int i = 0; i < avatarCount; i++)
                            {
                                uint avatarId = p.ReadUInt();
                                byte Plus = p.ReadByte();
                            }

                            byte Mask = p.ReadByte();
                            if (Mask == 1)
                            {
                                uint Id = p.ReadUInt();
                                var item = Data.Items.FindById((int)Id);
                                if (item != null)
                                {
                                    if (item.CodeName.StartsWith("CHAR"))
                                    {
                                        p.ReadByte();
                                        byte itemsCount = p.ReadByte();
                                        for (int i = 0; i < itemsCount; i++)
                                        {
                                            p.ReadUInt(); //Item Id
                                        }
                                    }
                                }
                            }
                            CurrentPlayer.ObjectId = p.ReadUInt();

                            byte xSec = p.ReadByte();
                            byte ySec = p.ReadByte();
                            float xCoordinate = p.ReadFloat();
                            float zCoordinate = p.ReadFloat();
                            float yCoordinate = p.ReadFloat();

                            p.ReadShort(); //Angle;
                            byte HasDestination = p.ReadByte();
                            p.ReadByte(); //Walking flag

                            if (HasDestination == 1)
                            {
                                xSec = p.ReadByte();
                                ySec = p.ReadByte();

                                if (ySec == 0x80)
                                {
                                    xCoordinate = p.ReadShort() - p.ReadShort();
                                    p.ReadShort();
                                    p.ReadShort();
                                    yCoordinate = p.ReadShort() - p.ReadShort();
                                }
                                else
                                {
                                    xCoordinate = p.ReadShort();
                                    p.ReadShort();
                                    yCoordinate = p.ReadShort();
                                }
                            }
                            else
                            {
                                p.ReadByte(); //NO DESTINATION
                                p.ReadShort(); //Angle??
                            }
                            byte Alive = p.ReadByte();
                            if (Alive == 2)
                                CurrentPlayer.IsAlive = false;

                            p.ReadByte();
                            p.ReadByte(); //Move flag
                            p.ReadByte(); //zerk flag

                            p.ReadFloat();
                            p.ReadFloat();
                            p.ReadFloat();

                            byte ActiveSkills = p.ReadByte();
                            for (byte i = 0; i < ActiveSkills; i++)
                            {
                                uint SkillId = p.ReadUInt();
                                var skill = Data.Skills.FindById((int)SkillId);
                                uint Duration = p.ReadUInt(); //Duration in ms
                                if (skill != null)
                                {

                                    if (skill.UiSkillName.StartsWith("SKILL_EU_CLERIC_RECOVERYA_GROUP") ||
                                        skill.UiSkillName.StartsWith("SKILL_EU_BARD_BATTLAA_GUARD") ||
                                        skill.UiSkillName.StartsWith("SKILL_EU_BARD_DANCEA") ||
                                        skill.UiSkillName.StartsWith("SKILL_EU_BARD_SPEEDUPA_HITRATE"))
                                    {
                                        p.ReadByte();
                                    }
                                }
                            }
                            string Name = p.ReadAscii();

                            CurrentPlayer.Name = Name;
                            CurrentPlayer.Level = 0;
                            CurrentPlayer.ModelId = model;
                            CurrentPlayer.XOffset = xCoordinate;
                            CurrentPlayer.YOffset = yCoordinate;
                            CurrentPlayer.ZOffset = zCoordinate;
                            CurrentPlayer.XSector = xSec;
                            CurrentPlayer.YSector = ySec;
                            Players.Add(CurrentPlayer);
                        }

                        #endregion


                        //Structures / Teleports
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Print("Spawn Parsing Error:" + ex.Message);
            }

        }

        private static ushort _groupSpawnAmount;
        private static byte _groupSpawnTypeB;
        private static void DecodeGroupSpawnStart(Packet p)
        {
            _groupSpawnTypeB = p.ReadByte();
            _groupSpawnAmount = p.ReadUShort();
        }
        private static void DecodeGroupSpawnEnd(Packet p)
        {
        }
        private static void DecodeGroupSpawn(Packet p)
        {
            for (int i = 0; i < _groupSpawnAmount; i++)
            {
                switch (_groupSpawnTypeB)
                {
                    case 1:
                        DecodeSingleSpawn(p);
                        break;
                    case 2:
                        //DecodeSingleDespawn(p);
                        break;
                }
            }
        }
    }
}