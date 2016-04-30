using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LiteDB;
using Shared.Core;
using Shared.Core.Security;
using Shared.Structs.Data;

namespace Shared
{
    public static class Data
    {
        public class Setting
        {
            public Setting(string Id, object data)
            {
                ID = Id;
                Data = data;
            }
            public Setting()
            {

            }

            public string ID { set; get; }
            public object Data { set; get; }
        }

        public static string SilkroadPath;
        public static string MediaPk2Key;
        public static ushort GatewayPort;
        public static int GameVerison;

        private static Pk2Reader _reader;
        public static LiteDatabase LiteDatabase = new LiteDatabase("Data.db");
        public static List<DivisionServer> DivisionServers;

        private static Dictionary<string, string> ItemNames { set; get; }
        private static Dictionary<string, string> SkillNames { set; get; }
        private static Dictionary<string, string> CharacterNames { set; get; }
        private static Dictionary<int, string> RegionNames { set; get; }

        public static LiteCollection<Skill> Skills => LiteDatabase.GetCollection<Skill>("Skills");
        public static LiteCollection<Item> Items => LiteDatabase.GetCollection<Item>("Items");
        public static LiteCollection<Character> Characters => LiteDatabase.GetCollection<Character>("Characters");
        private static LiteCollection<Setting> Settings => LiteDatabase.GetCollection<Setting>("Settings");

        public static T GetSetting<T>(string key, T data, bool update = false)
        {
            var ret = Settings.FindById(key);
            if (ret != null)
            {
                if (!update) return (T)ret.Data;
                ret.Data = data;
                Settings.Update(ret);
                return (T)ret.Data;
            }
            Settings.Insert(new Setting(key, data));
            return data;
        }

        public static bool Load(string path, string key)
        {
            SilkroadPath = GetSetting("SilkroadPath", path);
            MediaPk2Key = GetSetting("MediaPk2Key", key);
            if (string.IsNullOrEmpty(SilkroadPath) || string.IsNullOrEmpty(MediaPk2Key))
            {
                if (!string.IsNullOrEmpty(path) && !string.IsNullOrEmpty(key))
                {
                    SilkroadPath = GetSetting("SilkroadPath", path, true);
                    MediaPk2Key = GetSetting("MediaPk2Key", key, true);
                }
                else return false;
            }
            _reader = new Pk2Reader(SilkroadPath + @"\Media.pk2", MediaPk2Key);

            var isUpdate = GetSetting("GameVersion", 0);
            var needUpdate = isUpdate == 0 || isUpdate < Version();
            GameVerison = GetSetting("GameVersion", Version(), needUpdate);
            GatewayPort = ushort.Parse(GetSetting("GatewayPort", GatePort(), needUpdate).ToString());
            DivisionServers = DivisionInfo().ToList();

            if (!needUpdate) return true;
            LiteDatabase.DropCollection(Skills.Name);
            LiteDatabase.DropCollection(Items.Name);
            LoadTextData();
            Skills.Insert(LoadSkillsData().GroupBy(x => x.Id).Select(x => x.OrderBy(y => y.Id).First()));
            Items.Insert(LoadItemsData().GroupBy(x => x.Id).Select(x => x.OrderBy(y => y.Id).First()));
            Characters.Insert(LoadCharacterData().GroupBy(x => x.Id).Select(x => x.OrderBy(y => y.Id).First()));
            ItemNames.Clear();
            SkillNames.Clear();
            CharacterNames.Clear();
            RegionNames.Clear();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            return true;
        }

        private static IEnumerable<DivisionServer> DivisionInfo()
        {
            var dataReader = new BinaryReader(_reader.GetFileStream("DIVISIONINFO.TXT"));

            var locale = dataReader.ReadByte(); //Server Locale
            var divisionCount = dataReader.ReadByte();
            for (var i = 1; i <= divisionCount; i++)
            {
                var divisonNameLen = dataReader.ReadInt32();
                var divisionNameShit = dataReader.ReadChars(divisonNameLen);

                dataReader.ReadByte(); //UNKNOWN

                var division = new DivisionServer(new string(divisionNameShit), locale);
                var serverCount = dataReader.ReadByte();
                for (var iS = 1; iS <= serverCount; iS++)
                {
                    var serverNameLen = dataReader.ReadInt32();
                    var serverNameShit = dataReader.ReadChars(serverNameLen);

                    division.LoginServers.Add(new string(serverNameShit));
                    dataReader.ReadByte();
                }
                yield return division;
            }
        }

        private static int GatePort()
        {
            return Convert.ToUInt16(_reader.GetFileText("GATEPORT.TXT").Trim());
        }

        private static int Version()
        {
            return int.Parse(Encoding.ASCII.GetString(new Blowfish(Encoding.ASCII.GetBytes("SILKROADVERSION"), 0, 8).Decode(_reader.GetFileBytes("SV.T").Skip(4).Take(8).ToArray()), 0, 4));
        }

        private static void LoadTextData(byte LanguageTab = 8)
        {
            ItemNames = new Dictionary<string, string>();
            SkillNames = new Dictionary<string, string>();
            CharacterNames = new Dictionary<string, string>();
            RegionNames = new Dictionary<int, string>();

            var File_Equip_Skills = "textdata_equip&skill.txt";
            var File_Character_Items = "textdata_object.txt";
            var File_Zone_Names = "textzonename.txt";

            #region Item & Skill
            if (_reader.FileExists(File_Equip_Skills))
            {
                foreach (var entry in _reader.GetFileText(File_Equip_Skills).Split('\n'))
                {
                    var data = entry.Split('\t');
                    if (entry.Contains("ITEM_") && !entry.Contains("//")) //Has to be gear
                    {
                        try
                        {
                            if (Convert.ToByte(data[0]) == 1)
                            {
                                ItemNames.Add(data[1], data[LanguageTab]);
                            }
                        }
                        catch
                        {

                        }

                    }
                    else if (entry.Contains("SKILL_") || entry.Contains("SKILL_") && !entry.Contains("_DESC") && !entry.Contains("_STUDY"))
                    {
                        try
                        {
                            if (Convert.ToByte(data[0]) == 1)
                            {
                                SkillNames.Add(data[1], data[LanguageTab]);
                            }
                        }
                        catch
                        {

                        }

                    }
                }
            }
            #endregion
            #region Character & Item
            if (_reader.FileExists(File_Character_Items))
            {
                foreach (string entry in _reader.GetFileText(File_Character_Items).Split('\n'))
                {
                    var data = entry.Split('\t');
                    if (entry.Contains("_MOB_") && !entry.Contains("SKILL") && !entry.Contains("DESC"))
                    {
                        try
                        {
                            if (Convert.ToByte(data[0]) == 1)
                            {
                                CharacterNames.Add(data[1], data[LanguageTab]);
                            }
                        }
                        catch
                        {

                        }

                    }
                    else if (entry.Contains("ITEM_") && !entry.Contains("DESC"))
                    {
                        try
                        {
                            if (Convert.ToByte(data[0]) == 1)
                            {
                                ItemNames.Add(data[1], data[LanguageTab]);
                            }
                        }
                        catch
                        {

                        }
                    }
                    else if (entry.Contains("_NPC_") && !entry.Contains("DESC"))
                    {
                        try
                        {
                            if (Convert.ToByte(data[0]) == 1)
                            {
                                CharacterNames.Add(data[1], data[LanguageTab]);
                            }
                        }
                        catch
                        {

                        }
                    }
                }
            }
            #endregion
            #region Region
            foreach (string region in _reader.GetFileText(File_Zone_Names).Split('\n'))
            {
                try
                {
                    var data = region.Split('\t');
                    RegionNames.Add(Convert.ToInt32(data[1]), data[LanguageTab]);
                }
                catch
                {
                }
            }
            #endregion
        }

        private static IEnumerable<Skill> LoadSkillsData()
        {
            foreach (var data in _reader.GetFileText("skilldata.txt").Split('\n').Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line) && _reader.FileExists(line)).SelectMany(ft => _reader.GetFileText(ft).Split('\n').Where(line => !string.IsNullOrEmpty(line)).Select(line => line.Trim().Split('\t'))))
            {
                yield return new Skill(data, SkillNames.ContainsKey(data[62]) ? SkillNames[data[62]] : "");
            }

            foreach (var data in _reader.GetFileText("skilldataenc.txt").Split('\n').Select(line => line.Trim()).Where(line => !string.IsNullOrEmpty(line) && _reader.FileExists(line)).SelectMany(ft => Decrypter.Decrypt(_reader.GetFileStream(ft)).Split('\n').Where(line => !string.IsNullOrEmpty(line)).Select(line => line.Trim().TrimStart('?').Split('\t')).Where(data => data.Length > 10)))
            {
                yield return new Skill(data, SkillNames.ContainsKey(data[62]) ? SkillNames[data[62]] : "");
            }
        }

        private static IEnumerable<Item> LoadItemsData()
        {
            return
                from filename in
                    _reader.GetFileText("itemdata.txt")
                        .Split('\n')
                        .Where(line => !string.IsNullOrEmpty(line))
                        .Select(fn => fn.Trim())
                where _reader.FileExists(filename)
                select _reader.GetFileText(filename)
                    into filetext
                from data in
                    filetext.Split('\n')
                        .Where(line => !string.IsNullOrEmpty(line))
                        .Select(ft => ft.Trim().Split('\t'))
                select new Item(data, ItemNames.ContainsKey(data[5]) ? ItemNames[data[5]] : "");
        }

        private static IEnumerable<Character> LoadCharacterData()
        { 
            return from fileName in _reader.GetFileText("characterdata.txt").Split('\n') where fileName != string.Empty from data in _reader.GetFileText(fileName.Trim()).Split('\n').Where(line => line != string.Empty).Select(line => line.Split('\t')) select new Character(data, CharacterNames.ContainsKey(data[5]) ? CharacterNames[data[5]] : "");
        }

        #region Decrypter
        internal class Decrypter
        {
            #region HashTables

            private static byte[] Hash_Table_1 = new byte[]
{
            0x07, 0x83, 0xBC, 0xEE, 0x4B, 0x79, 0x19, 0xB6, 0x2A, 0x53, 0x4F, 0x3A, 0xCF, 0x71, 0xE5, 0x3C,
            0x2D, 0x18, 0x14, 0xCB, 0xB6, 0xBC, 0xAA, 0x9A, 0x31, 0x42, 0x3A, 0x13, 0x42, 0xC9, 0x63, 0xFC,
            0x54, 0x1D, 0xF2, 0xC1, 0x8A, 0xDD, 0x1C, 0xB3, 0x52, 0xEA, 0x9B, 0xD7, 0xC4, 0xBA, 0xF8, 0x12,
            0x74, 0x92, 0x30, 0xC9, 0xD6, 0x56, 0x15, 0x52, 0x53, 0x60, 0x11, 0x33, 0xC5, 0x9D, 0x30, 0x9A,
            0xE5, 0xD2, 0x93, 0x99, 0xEB, 0xCF, 0xAA, 0x79, 0xE3, 0x78, 0x6A, 0xB9, 0x02, 0xE0, 0xCE, 0x8E,
            0xF3, 0x63, 0x5A, 0x73, 0x74, 0xF3, 0x72, 0xAA, 0x2C, 0x9F, 0xBB, 0x33, 0x91, 0xDE, 0x5F, 0x91,
            0x66, 0x48, 0xD1, 0x7A, 0xFD, 0x3F, 0x91, 0x3E, 0x5D, 0x22, 0xEC, 0xEF, 0x7C, 0xA5, 0x43, 0xC0,
            0x1D, 0x4F, 0x60, 0x7F, 0x0B, 0x4A, 0x4B, 0x2A, 0x43, 0x06, 0x46, 0x14, 0x45, 0xD0, 0xC5, 0x83,
            0x92, 0xE4, 0x16, 0xD0, 0xA3, 0xA1, 0x13, 0xDA, 0xD1, 0x51, 0x07, 0xEB, 0x7D, 0xCE, 0xA5, 0xDB,
            0x78, 0xE0, 0xC1, 0x0B, 0xE5, 0x8E, 0x1C, 0x7C, 0xB4, 0xDF, 0xED, 0xB8, 0x53, 0xBA, 0x2C, 0xB5,
            0xBB, 0x56, 0xFB, 0x68, 0x95, 0x6E, 0x65, 0x00, 0x60, 0xBA, 0xE3, 0x00, 0x01, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x9C, 0xB5, 0xD5, 0x00, 0x00, 0x00, 0x00, 0x00, 0x2E, 0x3F, 0x41, 0x56,
            0x43, 0x45, 0x53, 0x63, 0x72, 0x69, 0x70, 0x74, 0x40, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x64, 0xBB, 0xE3, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
};

            private static byte[] Hash_Table_2 = new byte[]
{
            0x0D, 0x05, 0x90, 0x41, 0xF9, 0xD0, 0x65, 0xBF, 0xF9, 0x0B, 0x15, 0x93, 0x80, 0xFB, 0x01, 0x02,
            0xB6, 0x08, 0xC4, 0x3C, 0xC1, 0x49, 0x94, 0x4D, 0xCE, 0x1D, 0xFD, 0x69, 0xEA, 0x19, 0xC9, 0x57,
            0x9C, 0x4D, 0x84, 0x62, 0xE3, 0x67, 0xF9, 0x87, 0xF4, 0xF9, 0x93, 0xDA, 0xE5, 0x15, 0xF1, 0x4C,
            0xA4, 0xEC, 0xBC, 0xCF, 0xDD, 0xB3, 0x6F, 0x04, 0x3D, 0x70, 0x1C, 0x74, 0x21, 0x6B, 0x00, 0x71,
            0x31, 0x7F, 0x54, 0xB3, 0x72, 0x6C, 0xAA, 0x42, 0xC1, 0x78, 0x61, 0x3E, 0xD5, 0xF2, 0xE1, 0x27,
            0x36, 0x71, 0x3A, 0x25, 0x36, 0x57, 0xD1, 0xF8, 0x70, 0x86, 0xBD, 0x0E, 0x58, 0xB3, 0x76, 0x6D,
            0xC3, 0x50, 0xF6, 0x6C, 0xA0, 0x10, 0x06, 0x64, 0xA2, 0xD6, 0x2C, 0xD4, 0x27, 0x30, 0xA5, 0x36,
            0x1C, 0x1E, 0x3E, 0x58, 0x9D, 0x59, 0x76, 0x9D, 0xA7, 0x42, 0x5A, 0xF0, 0x00, 0xBC, 0x69, 0x31,
            0x40, 0x1E, 0xFA, 0x09, 0x1D, 0xE7, 0xEE, 0xE4, 0x54, 0x89, 0x36, 0x7C, 0x67, 0xC8, 0x65, 0x22,
            0x7E, 0xA3, 0x60, 0x44, 0x1E, 0xBC, 0x68, 0x6F, 0x15, 0x2A, 0xFD, 0x9D, 0x3F, 0x36, 0x6B, 0x28,
            0x06, 0x67, 0xFE, 0xC6, 0x49, 0x6B, 0x9B, 0x3F, 0x80, 0x2A, 0xD2, 0xD4, 0xD3, 0x20, 0x1B, 0x96,
            0xF4, 0xD2, 0xCA, 0x8C, 0x74, 0xEE, 0x0B, 0x6A, 0xE1, 0xE9, 0xC6, 0xD2, 0x6E, 0x33, 0x63, 0xC0,
            0xE9, 0xD0, 0x37, 0xA9, 0x3C, 0xF7, 0x18, 0xF2, 0x4A, 0x74, 0xEC, 0x41, 0x61, 0x7A, 0x19, 0x47,
            0x8F, 0xA0, 0xBB, 0x94, 0x8F, 0x3D, 0x11, 0x11, 0x26, 0xCF, 0x69, 0x18, 0x1B, 0x2C, 0x87, 0x6D,
            0xB3, 0x22, 0x6C, 0x78, 0x41, 0xCC, 0xC2, 0x84, 0xC5, 0xCB, 0x01, 0x6A, 0x37, 0x00, 0x01, 0x65,
            0x4F, 0xA7, 0x85, 0x85, 0x15, 0x59, 0x05, 0x67, 0xF2, 0x4F, 0xAB, 0xB7, 0x88, 0xFA, 0x69, 0x24,
            0x9E, 0xC6, 0x7B, 0x3F, 0xD5, 0x0E, 0x4D, 0x7B, 0xFB, 0xB1, 0x21, 0x3C, 0xB0, 0xC0, 0xCB, 0x2C,
            0xAA, 0x26, 0x8D, 0xCC, 0xDD, 0xDA, 0xC1, 0xF8, 0xCA, 0x7F, 0x6A, 0x3F, 0x2A, 0x61, 0xE7, 0x60,
            0x5C, 0xCE, 0xD3, 0x4C, 0xAC, 0x45, 0x40, 0x62, 0xEA, 0x51, 0xF1, 0x66, 0x5D, 0x2C, 0x45, 0xD6,
            0x8B, 0x7D, 0xCE, 0x9C, 0xF5, 0xBB, 0xF7, 0x52, 0x24, 0x1A, 0x13, 0x02, 0x2B, 0x00, 0xBB, 0xA1,
            0x8F, 0x6E, 0x7A, 0x33, 0xAD, 0x5F, 0xF4, 0x4A, 0x82, 0x76, 0xAB, 0xDE, 0x80, 0x98, 0x8B, 0x26,
            0x4F, 0x33, 0xD8, 0x68, 0x1E, 0xD9, 0xAE, 0x06, 0x6B, 0x7E, 0xA9, 0x95, 0x67, 0x60, 0xEB, 0xE8,
            0xD0, 0x7D, 0x07, 0x4B, 0xF1, 0xAA, 0x9A, 0xC5, 0x29, 0x93, 0x9D, 0x5C, 0x92, 0x3F, 0x15, 0xDE,
            0x48, 0xF1, 0xCA, 0xEA, 0xC9, 0x78, 0x3C, 0x28, 0x7E, 0xB0, 0x46, 0xD3, 0x71, 0x6C, 0xD7, 0xBD,
            0x2C, 0xF7, 0x25, 0x2F, 0xC7, 0xDD, 0xB4, 0x6D, 0x35, 0xBB, 0xA7, 0xDA, 0x3E, 0x3D, 0xA7, 0xCA,
            0xBD, 0x87, 0xDD, 0x9F, 0x22, 0x3D, 0x50, 0xD2, 0x30, 0xD5, 0x14, 0x5B, 0x8F, 0xF4, 0xAF, 0xAA,
            0xA0, 0xFC, 0x17, 0x3D, 0x33, 0x10, 0x99, 0xDC, 0x76, 0xA9, 0x40, 0x1B, 0x64, 0x14, 0xDF, 0x35,
            0x68, 0x66, 0x5B, 0x49, 0x05, 0x33, 0x68, 0x26, 0xC8, 0xBA, 0xD1, 0x8D, 0x39, 0x2B, 0xFB, 0x3E,
            0x24, 0x52, 0x2F, 0x9A, 0x69, 0xBC, 0xF2, 0xB2, 0xAC, 0xB8, 0xEF, 0xA1, 0x17, 0x29, 0x2D, 0xEE,
            0xF5, 0x23, 0x21, 0xEC, 0x81, 0xC7, 0x5B, 0xC0, 0x82, 0xCC, 0xD2, 0x91, 0x9D, 0x29, 0x93, 0x0C,
            0x9D, 0x5D, 0x57, 0xAD, 0xD4, 0xC6, 0x40, 0x93, 0x8D, 0xE9, 0xD3, 0x35, 0x9D, 0xC6, 0xD3, 0x00,
};

            #endregion

            public static string Decrypt(Stream Input)
            {
                int encrypted = 0;
                int key = 0x8c1f;
                byte buff = 0;
                byte[] buffer;
                long fileLen = 0;

                BinaryReader reader = new BinaryReader(Input);

                try
                {
                    fileLen = Input.Length;
                    buffer = new byte[fileLen];

                    //check buffer
                    if (buffer == null)
                    {
                        reader.Close();
                        reader.Dispose();

                        return string.Empty;
                    }


                    reader.BaseStream.Read(buffer, 0, Convert.ToInt32(fileLen));
                    reader.Close();

                    //Check if encrypted
                    if (buffer[0] == 0xE2 && buffer[1] == 0xB0)
                        encrypted = 1;

                    //Decrypt
                    for (long i = 0; i < fileLen; i++)
                    {
                        buff = (byte)(Hash_Table_1[key % 0xA7] - Hash_Table_2[key % 0x1EF]);
                        ++key;
                        if (encrypted == 1)
                            buffer[i] += buff;
                        else
                            buffer[i] -= buff;
                    }
                    reader.Dispose();

                    return ByteArrayToString(buffer);


                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return string.Empty;
                }
            }

            private static string ByteArrayToString(byte[] arr)
            {
                arr = Encoding.Convert(Encoding.GetEncoding(1200), Encoding.ASCII, arr);
                return Encoding.ASCII.GetString(arr);
            }

            private static byte[] StringToByteArray(string str)
            {
                ASCIIEncoding enc = new ASCIIEncoding();
                return enc.GetBytes(str);
            }
        }
        #endregion
    }
}