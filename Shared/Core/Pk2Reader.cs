using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Shared.Core.Security;

namespace Shared.Core
{
    public class Pk2Reader : IDisposable
    {
        public class File
        {
            public string Name { get; set; }
            public long Position { get; set; }
            public uint Size { get; set; }
            public Folder ParentFolder { get; set; }
        }

        public class Folder
        {
            public string Name { get; set; }
            public long Position { get; set; }
            public List<File> Files { get; set; }
            public List<Folder> SubFolders { get; set; }
        }

        #region Constructor

        public Pk2Reader(string fileName, string key)
        {
            if (!System.IO.File.Exists(fileName))
                throw new Exception("File not found");
            Path = fileName;
            Key = GenerateFinalBlowfishKey(key);
            ASCIIKey = key;

            _mFileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            Size = _mFileStream.Length;

            _mBlowfish.Initialize(Key);
            var reader = new BinaryReader(_mFileStream);
            Header = (SPk2Header)BufferToStruct(reader.ReadBytes(256), typeof(SPk2Header));
            _mCurrentFolder = new Folder
            {
                Name = fileName,
                Files = new List<File>(),
                SubFolders = new List<Folder>()
            };

            _mMainFolder = _mCurrentFolder;
            Read(reader.BaseStream.Position);
        }

        #endregion

        #region Dispose

        public void Dispose()
        {
            _mCurrentFolder = null;
            EntryBlocks = null;
            Files = null;
            _mFileStream = null;
            Folders = null;
            Key = null;
            _mMainFolder = null;
            Path = null;
            Size = 0;
        }

        #endregion

        #region Properties & Member variables

        private readonly Blowfish _mBlowfish = new Blowfish();
        private Folder _mCurrentFolder;
        private Folder _mMainFolder;
        private FileStream _mFileStream;
        private byte[] Key { get; set; }

        public long Size { get; private set; }
        public string ASCIIKey { get; }
        public SPk2Header Header { get; }
        public List<SPk2EntryBlock> EntryBlocks { get; private set; } = new List<SPk2EntryBlock>();
        public List<File> Files { get; private set; } = new List<File>();
        public List<Folder> Folders { get; private set; } = new List<Folder>();
        public string Path { get; private set; }

        #endregion

        #region Blowfish:Key_Generation

        private static byte[] GenerateFinalBlowfishKey(string asciiKey)
        {
            //Using the default Base_Key
            return GenerateFinalBlowfishKey(asciiKey,
                new byte[] { 0x03, 0xF8, 0xE4, 0x44, 0x88, 0x99, 0x3F, 0x64, 0xFE, 0x35 });
        }

        private static byte[] GenerateFinalBlowfishKey(string asciiKey, byte[] baseKey)
        {
            var asciiKeyLenght = (byte)asciiKey.Length;

            //Max count of 56 key bytes
            if (asciiKeyLenght > 56)
            {
                asciiKeyLenght = 56;
            }

            //Get bytes from ascii
            var aKey = Encoding.ASCII.GetBytes(asciiKey);

            //This is the Silkroad bas key used in all versions
            var bKey = new byte[56];

            //Copy key to array to keep the b_key at 56 bytes. b_key has to be bigger than a_key
            //to be able to xor every index of a_key.
            Array.ConstrainedCopy(baseKey, 0, bKey, 0, baseKey.Length);

            // Their key modification algorithm for the final blowfish key
            var bfKey = new byte[asciiKeyLenght];
            for (byte x = 0; x < asciiKeyLenght; ++x)
            {
                bfKey[x] = (byte)(aKey[x] ^ bKey[x]);
            }

            return bfKey;
        }

        #endregion

        #region Functions & Methods

        public void ExtractFile(File file, string outputPath)
        {
            ExtractFile(file.Name, outputPath);
        }

        public void ExtractFile(string name, string outputPath)
        {
            var writer = new BinaryWriter(new FileStream(outputPath, FileMode.OpenOrCreate));
            writer.Write(GetFileBytes(name));
            writer.Close();
        }

        public string GetFileExtension(File file)
        {
            return GetFileExtension(file.Name);
        }

        public string GetFileExtension(string name)
        {
            if (FileExists(name))
            {
                return name.Substring(name.LastIndexOf('.'));
            }
            throw new Exception("The file does not exsist");
        }

        public List<File> GetRootFiles()
        {
            return _mMainFolder.Files;
        }

        public List<Folder> GetRootFolders()
        {
            return _mMainFolder.SubFolders;
        }

        public List<File> GetFiles(string parentFolder)
        {
            return Files.Where(file => file.ParentFolder.Name == parentFolder).ToList();
        }

        public List<Folder> GetSubFolders(string parentFolder)
        {
            var objToReturn = new List<Folder>();
            foreach (var folder in Folders.Where(folder => folder.Name == parentFolder))
            {
                objToReturn.AddRange(folder.SubFolders);
            }
            return objToReturn;
        }

        public bool FileExists(string name)
        {
            return
                Files.Find(item => string.Equals(item.Name, name, StringComparison.InvariantCultureIgnoreCase)).Position !=
                0;
        }

        public byte[] GetFileBytes(string name)
        {
            if (!FileExists(name)) return null;
            var reader = new BinaryReader(_mFileStream);
            var file = Files.Find(item => string.Equals(item.Name, name, StringComparison.InvariantCultureIgnoreCase));
            reader.BaseStream.Position = file.Position;
            return reader.ReadBytes((int)file.Size);
            //throw new Exception(string.Format("pk2Reader: File not found: {0}", Name));
        }

        public byte[] GetFileBytes(File file)
        {
            return GetFileBytes(file.Name);
        }

        public string GetFileText(string name)
        {
            if (FileExists(name))
            {
                var tempBuffer = GetFileBytes(name);
                if (tempBuffer != null)
                {
                    TextReader txtReader = new StreamReader(new MemoryStream(tempBuffer));
                    return txtReader.ReadToEnd();
                }
                return null;
            }
            throw new Exception("File does not exsist!");
        }

        public string GetFileText(File file)
        {
            return GetFileText(file.Name);
        }

        public Stream GetFileStream(string name)
        {
            return new MemoryStream(GetFileBytes(name));
        }

        public Stream GetFileStream(File file)
        {
            return GetFileStream(file.Name);
        }

        public List<string> GetFileNames()
        {
            return Files.Select(file => file.Name).ToList();
        }

        private void Read(long position)
        {
            var reader = new BinaryReader(_mFileStream);
            reader.BaseStream.Position = position;
            var folders = new List<Folder>();
            var entryBlock =
                (SPk2EntryBlock)
                    BufferToStruct(_mBlowfish.Decode(reader.ReadBytes(Marshal.SizeOf(typeof(SPk2EntryBlock)))),
                        typeof(SPk2EntryBlock));

            for (var i = 0; i < 20; i++)
            {
                var entry = entryBlock.Entries[i]; //.....
                switch (entry.Type)
                {
                    case 0: //Null Entry

                        break;
                    case 1: //Folder 
                        if (entry.Name != "." && entry.Name != "..")
                        {
                            var folder = new Folder
                            {
                                Name = entry.Name,
                                Position = BitConverter.ToInt64(entry.g_Position, 0)
                            };
                            folders.Add(folder);
                            Folders.Add(folder);
                            _mCurrentFolder.SubFolders.Add(folder);
                        }
                        break;
                    case 2: //File
                        var file = new File
                        {
                            Position = entry.Position,
                            Name = entry.Name,
                            Size = entry.Size,
                            ParentFolder = _mCurrentFolder
                        };
                        Files.Add(file);
                        _mCurrentFolder.Files.Add(file);
                        break;
                }
            }
            if (entryBlock.Entries[19].NextChain != 0)
            {
                Read(entryBlock.Entries[19].NextChain);
            }

            foreach (var folder in folders)
            {
                _mCurrentFolder = folder;
                if (folder.Files == null)
                {
                    folder.Files = new List<File>();
                }
                if (folder.SubFolders == null)
                {
                    folder.SubFolders = new List<Folder>();
                }
                Read(folder.Position);
            }
        }

        #endregion

        #region Structures

        private object BufferToStruct(byte[] buffer, Type returnStruct)
        {
            var pointer = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, pointer, buffer.Length);
            return Marshal.PtrToStructure(pointer, returnStruct);
        }

        [StructLayout(LayoutKind.Sequential, Size = 256)]
        public struct SPk2Header
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 30)]
            public readonly string Name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public readonly byte[] Version;
            [MarshalAs(UnmanagedType.I1, SizeConst = 1)]
            public readonly byte Encryption;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public readonly byte[] Verify;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 205)]
            public readonly byte[] Reserved;
        }

        [StructLayout(LayoutKind.Sequential, Size = 128)]
        public struct SPk2Entry
        {
            [MarshalAs(UnmanagedType.I1)]
            public readonly byte Type; //files are 2, folger are 1, null entries re 0
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 81)]
            public readonly string Name;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] AccessTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] CreateTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] ModifyTime;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            public readonly byte[] g_Position;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            private readonly byte[] m_Size;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
            private readonly byte[] m_NextChain;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
            public readonly byte[] Padding;

            public long NextChain => BitConverter.ToInt64(m_NextChain, 0);
            public long Position => BitConverter.ToInt64(g_Position, 0);
            public uint Size => BitConverter.ToUInt32(m_Size, 0);
        }

        [StructLayout(LayoutKind.Sequential, Size = 2560)]
        public struct SPk2EntryBlock
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)]
            public readonly SPk2Entry[] Entries;
        }

        #endregion

    }
}