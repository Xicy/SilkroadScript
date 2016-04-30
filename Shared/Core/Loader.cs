using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace Shared.Core
{
    public static class Loader
    {
        public static void Inject(string sroPath, ushort port, byte locale)
        {
            LoadLibrary("WS2_32.dll");
            LoadLibrary("Kernel32.dll");

            var fileArray = File.ReadAllBytes(sroPath + "\\sro_client.exe");
            #region FIND ADDRESSES
            //AlreadyProgramExeSearch
            _alreadyProgramExe = FindStringPattern(AlreadyProgramExeStringPattern, fileArray, _baseAddress, PUSH, 1) - 2;
            //SeedPatchSearch
            _seedPatchAdress = _baseAddress + FindPattern(SeedPatchPattern, fileArray, 1);
            //ReplaceText
            _startingMsg = FindStringPattern(StartingMsgStringPattern, fileArray, _baseAddress, PUSH, 1) + 24;
            _changeVersion = FindStringPattern(ChangeVersionStringPattern, fileArray, _baseAddress, PUSH, 1);

            //MulticlientSearch
            _multiClientAddress = _baseAddress + FindPattern(MulticlientPattern, fileArray, 1) + 9;
            //CallForwardSearch
            _callForwardAddress = _baseAddress + FindPattern(CallForwardPattern, fileArray, 1);
            //MultiClientErrorSearch
            _multiClientError = FindStringPattern(MultiClientErrorStringPattern, fileArray, _baseAddress, PUSH, 1) - 8;

            //if (checkBox_NudePatch.Checked)
            //{
            //    //NudePatchSearch
            //    NudePatch = BaseAddress + FindPattern(NudePatchPattern, FileArray, 1) + 11;
            //}

            //ZoomhackSearch
            _zoomhack = _baseAddress + FindPattern(ZoomhackPattern, fileArray, 2) + 5;


            //SwearFilterSearch
            _swearFilter1 = FindStringPattern(SwearFilterStringPattern, fileArray, _baseAddress, PUSH, 1) - 2;
            _swearFilter2 = FindStringPattern(SwearFilterStringPattern, fileArray, _baseAddress, PUSH, 2) - 2;
            _swearFilter3 = FindStringPattern(SwearFilterStringPattern, fileArray, _baseAddress, PUSH, 3) - 2;
            _swearFilter4 = FindStringPattern(SwearFilterStringPattern, fileArray, _baseAddress, PUSH, 4) - 2;


            _redirectIpAddress = _baseAddress + FindPattern(RedirectIpAddressPattern, fileArray, 1) - 50;



            #endregion
            StartLoader(sroPath, port, locale);
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllToLoad);
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, int dwProcessId);
        [DllImport("kernel32.dll")]
        static extern uint ReadProcessMemory(IntPtr hProcess, uint lpBaseAddress, uint lpbuffer, uint nSize, uint lpNumberOfBytesRead);
        [DllImport("kernel32.dll")]
        static extern uint WriteProcessMemory(IntPtr hProcess, uint lpBaseAddress, byte[] lpBuffer, int nSize, uint lpNumberOfBytesWritten);
        [DllImport("kernel32.dll")]
        static extern uint VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, uint flAllocationType, uint flProtect);
        [DllImport("kernel32.dll")]
        static extern IntPtr CreateMutex(IntPtr lpMutexAttributes, bool bInitialOwner, string lpName);
        [DllImport("kernel32.dll")]
        static extern IntPtr GetModuleHandle(string lpModuleName);
        [DllImport("kernel32")]
        static extern uint GetProcAddress(IntPtr hModule, string procName);

        static uint _byteArray = 0;

        #region Addresses

        static readonly uint _baseAddress = 0x400000;

        static uint _alreadyProgramExe = 0;
        static uint _redirectIpAddress = 0;
        static uint _multiClientAddress = 0;
        static uint _callForwardAddress = 0;
        static uint _multiClientError = 0;
        static uint NudePatch = 0;
        static uint _swearFilter1 = 0;
        static uint _swearFilter2 = 0;
        static uint _swearFilter3 = 0;
        static uint _swearFilter4 = 0;
        static uint EnglishPatch = 0;
        static uint RussianHack = 0;
        static uint _zoomhack = 0;
        static uint _seedPatchAdress = 0;
        static uint _textDataName = 0;
        static uint _startingMsg = 0;
        static uint _changeVersion = 0;

        #endregion

        #region Patches

        static readonly byte[] HexColorArray = { 0x56, 0xBD, 0xAA, 0xAA }; //BlueGreenRed

        static readonly byte[] Jmp = { 0xEB };
        static readonly byte[] _retn = { 0xC3 };
        static readonly byte[] Nopnop = { 0x90, 0x90 };
        static readonly byte[] LanguageTab = { 0xBF, 0x08, 0x00, 0x00, 0x00, 0x90 };
        static readonly byte[] SeedPatch = { 0xB9, 0x33, 0x00, 0x00, 0x00, 0x90, 0x90, 0x90, 0x90, 0x90 };

        static readonly byte PUSH = 0x68;
        #endregion

        #region BytePattern

        static readonly byte[] RedirectIpAddressPattern = { 0x89, 0x86, 0x2C, 0x01, 0x00, 0x00, 0x8B, 0x17, 0x89, 0x56, 0x50, 0x8B, 0x47, 0x04, 0x89, 0x46, 0x54, 0x8B, 0x4F, 0x08, 0x89, 0x4E, 0x58, 0x8B, 0x57, 0x0C, 0x89, 0x56, 0x5C, 0x5E, 0xB8, 0x01, 0x00, 0x00, 0x00, 0x5D, 0xC3 };
        static readonly byte[] SeedPatchPattern = { 0x8B, 0x4C, 0x24, 0x04, 0x81, 0xE1, 0xFF, 0xFF, 0xFF, 0x7F };
        static readonly byte[] _nudePatchPattern = { 0x8B, 0x84, 0xEE, 0x1C, 0x01, 0x00, 0x00, 0x3B, 0x44, 0x24, 0x14 };
        static readonly byte[] ZoomhackPattern = { 0xDF, 0xE0, 0xF6, 0xC4, 0x41, 0x7A, 0x08, 0xD9, 0x9E };
        static readonly byte[] MulticlientPattern = { 0x6A, 0x06, 0x8D, 0x44, 0x24, 0x48, 0x50, 0x8B, 0xCF };
        static readonly byte[] CallForwardPattern = { 0x56, 0x8B, 0xF1, 0x0F, 0xB7, 0x86, 0x3E, 0x10, 0x00, 0x00, 0x57, 0x66, 0x8B, 0x7C, 0x24, 0x10, 0x0F, 0xB7, 0xCF, 0x8D, 0x14, 0x01, 0x3B, 0x96, 0x4C, 0x10, 0x00, 0x00 };
        static readonly byte[] MultiClientErrorStringPattern = Encoding.Default.GetBytes("½ÇÅ©·Îµå°¡ ÀÌ¹Ì ½ÇÇà Áß ÀÔ´Ï´Ù.");
        static readonly byte[] SwearFilterStringPattern = Encoding.Unicode.GetBytes("UIIT_MSG_CHATWND_MESSAGE_FILTER");
        static readonly byte[] _serverStatusFullStringPattern = Encoding.Unicode.GetBytes("UIO_STT_SERVER_MAX_FULL");
        static readonly byte[] ChangeVersionStringPattern = Encoding.Unicode.GetBytes("Ver %d.%03d");
        static readonly byte[] StartingMsgStringPattern = Encoding.Unicode.GetBytes("UIIT_STT_STARTING_MSG");
        static readonly byte[] AlreadyProgramExeStringPattern = Encoding.ASCII.GetBytes("//////////////////////////////////////////////////////////////////");
        static readonly byte[] _noGameGuardStringPattern = Encoding.ASCII.GetBytes(@"config\n_protect.dat");
        static readonly byte[] _textDataNameStringPattern = Encoding.ASCII.GetBytes(@"%stextdata\textdataname.txt");
        static readonly byte[] _englishStringPattern = Encoding.ASCII.GetBytes("English");
        static readonly byte[] _russiaStringPattern = Encoding.ASCII.GetBytes("Russia");

        #endregion

        private static void StartLoader(string sroPath, ushort port, byte locale, string startText = @"MyBot'a Hoşgeldiniz. Kolay kullanım kodları görmek için.yardım yazın")
        {

            CreateMutex(IntPtr.Zero, false, "Silkroad Online Launcher");
            CreateMutex(IntPtr.Zero, false, "Ready");

            var silkProcess = new Process
            {
                StartInfo =
                {
                    FileName = sroPath + "\\sro_client.exe",
                    Arguments = "0/" + locale + " 0 0"
                }
            };
            silkProcess.Start();

            var sroProcessHandle = OpenProcess((uint)(0x000F0000L | 0x00100000L | 0xFFF), 0, silkProcess.Id);

            Quickpatches(sroProcessHandle);

            RedirectIp(sroProcessHandle, port);

            MultiClient(sroProcessHandle);

            StartingTextMsg(sroProcessHandle, startText, HexColorArray);


        }
        private static void Quickpatches(IntPtr sroProcessHandle)
        {
            //Already Program Exe
            WriteProcessMemory(sroProcessHandle, _alreadyProgramExe, Jmp, Jmp.Length, _byteArray);

            //Multiclient Error MessageBox
            WriteProcessMemory(sroProcessHandle, _multiClientError, Jmp, Jmp.Length, _byteArray);

            //Nude Patch
            WriteProcessMemory(sroProcessHandle, NudePatch, Nopnop, Nopnop.Length, _byteArray);

            //Swear Filter
            WriteProcessMemory(sroProcessHandle, _swearFilter1, Jmp, Jmp.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _swearFilter2, Jmp, Jmp.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _swearFilter3, Jmp, Jmp.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _swearFilter4, Jmp, Jmp.Length, _byteArray);

            //Zoomhack
            WriteProcessMemory(sroProcessHandle, _zoomhack, Jmp, Jmp.Length, _byteArray);

            //English Patch
            WriteProcessMemory(sroProcessHandle, EnglishPatch, Nopnop, Nopnop.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, RussianHack, Jmp, Jmp.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _textDataName, LanguageTab, LanguageTab.Length, _byteArray);

            //Seed Patch
            WriteProcessMemory(sroProcessHandle, _seedPatchAdress, SeedPatch, SeedPatch.Length, _byteArray);

        }
        private static void RedirectIp(IntPtr sroProcessHandle, ushort redirectPort)
        {
            var redirectIpCodeCave = VirtualAllocEx(sroProcessHandle, IntPtr.Zero, 27, 0x1000, 0x4);
            var sockAddrStruct = VirtualAllocEx(sroProcessHandle, IntPtr.Zero, 8, 0x1000, 0x4);
            var ws2Connect = GetProcAddress(GetModuleHandle("WS2_32.dll"), "connect");

            var ws2Array = BitConverter.GetBytes(ws2Connect - redirectIpCodeCave - 26);
            var sockAddr = BitConverter.GetBytes(sockAddrStruct);
            var callRedirectIp = BitConverter.GetBytes(redirectIpCodeCave - _redirectIpAddress - 5);
            var port = BitConverter.GetBytes(Convert.ToUInt32(redirectPort));
            var ip1 = BitConverter.GetBytes(Convert.ToUInt16("127"));
            var ip2 = BitConverter.GetBytes(Convert.ToUInt16("0"));
            var ip3 = BitConverter.GetBytes(Convert.ToUInt16("0"));
            var ip4 = BitConverter.GetBytes(Convert.ToUInt16("1"));

            byte[] connection = { 0x02, 0x00, port[1], port[0], ip1[0], ip2[0], ip3[0], ip4[0] };
            byte[] callAddress = { 0xE8, callRedirectIp[0], callRedirectIp[1], callRedirectIp[2], callRedirectIp[3] };
            byte[] redirectCode = {       0x50, //PUSH EAX
                                          0x66, 0x8B, 0x47, 0x02, //MOV AX,WORD PTR DS:[EDI+2]
                                          0x66, 0x3D, 0x3D, 0xA3, //CMP AX,0A33D
                                          0x75, 0x05, //JNZ SHORT xxxxxxxx
                                          0xBF, sockAddr[0], sockAddr[1], sockAddr[2], sockAddr[3], //MOV EDI,xxxxxxxx
                                          0x58, //POP EAX
                                          0x6A, 0x10, //PUSH 10
                                          0x57, //PUSH EDI
                                          0x51, //PUSH ECX
                                          0xE8, ws2Array[0], ws2Array[1], ws2Array[2], ws2Array[3], //CALL WS2_32.connect
                                          0xC3 //RETN
                                      };

            WriteProcessMemory(sroProcessHandle, redirectIpCodeCave, redirectCode, redirectCode.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, sockAddrStruct, connection, connection.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _redirectIpAddress, callAddress, callAddress.Length, _byteArray);
        }
        private static void MultiClient(IntPtr sroProcessHandle)
        {
            var multiClientCodeCave = VirtualAllocEx(sroProcessHandle, IntPtr.Zero, 45, 0x1000, 0x4);
            var macCodeCave = VirtualAllocEx(sroProcessHandle, IntPtr.Zero, 4, 0x1000, 0x4);
            var gtc = GetProcAddress(GetModuleHandle("kernel32.dll"), "GetTickCount");

            var callBack = BitConverter.GetBytes(multiClientCodeCave + 41);
            var callForward = BitConverter.GetBytes(_callForwardAddress - multiClientCodeCave - 34);
            var macAddress = BitConverter.GetBytes(macCodeCave);
            var gtcAddress = BitConverter.GetBytes(gtc - multiClientCodeCave - 18);

            var multiClientArray = BitConverter.GetBytes(multiClientCodeCave - _multiClientAddress - 5);
            byte[] multiClientCodeArray = { 0xE8, multiClientArray[0], multiClientArray[1], multiClientArray[2], multiClientArray[3] };

            byte[] multiClientCode = {   0x8F, 0x05, callBack[0], callBack[1], callBack[2], callBack[3], //POP DWORD PTR DS:[xxxxxxxx]
                                         0xA3, macAddress[0], macAddress[1], macAddress[2], macAddress[3], //MOV DWORD PTR DS:[xxxxxxxx],EAX
                                         0x60, //PUSHAD
                                         0x9C, //PUSHFD
                                         0xE8, gtcAddress[0], gtcAddress[1], gtcAddress[2], gtcAddress[3], // Call KERNEL32.gettickcount
                                         0x8B, 0x0D, macAddress[0], macAddress[1], macAddress[2], macAddress[3], //MOV ECX,DWORD PTR DS:[xxxxxxxx]
                                         0x89, 0x41, 0x02, // MOV DWORD PTR DS:[ECX+2],EAX
                                         0x9D, //POPFD
                                         0x61, //POPAD
                                         0xE8, callForward[0], callForward[1], callForward[2], callForward[3], //CALL xxxxxxxx
                                         0xFF, 0x35, callBack[0], callBack[1], callBack[2], callBack[3], // PUSH DWORD PTR DS:[xxxxxxxx]
                                         0xC3 //RETN
                                       };

            WriteProcessMemory(sroProcessHandle, multiClientCodeCave, multiClientCode, multiClientCode.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _multiClientAddress, multiClientCodeArray, multiClientCodeArray.Length, _byteArray);
        }
        private static void StartingTextMsg(IntPtr sroProcessHandle, string startingText, byte[] hexColor)
        {
            var changeVersionString = @"MyBot Kullanıyorsun";
            var startingMsgStringCodeCave = VirtualAllocEx(sroProcessHandle, IntPtr.Zero, startingText.Length, 0x1000, 0x4);
            var changeVersionStringCodeCave = VirtualAllocEx(sroProcessHandle, IntPtr.Zero, startingText.Length, 0x1000, 0x4);
            var startingMsgByteArray = Encoding.Unicode.GetBytes(startingText);
            byte[] changeVersionByteArray = Encoding.Unicode.GetBytes(changeVersionString);
            byte[] callStartingMsg = BitConverter.GetBytes(startingMsgStringCodeCave);
            byte[] callChangeVersion = BitConverter.GetBytes(changeVersionStringCodeCave);
            byte[] startingMsgCodeArray = { 0xB8, callStartingMsg[0], callStartingMsg[1], callStartingMsg[2], callStartingMsg[3] };
            byte[] changeVersionCodeArray = { 0x68, callChangeVersion[0], callChangeVersion[1], callChangeVersion[2], callChangeVersion[3] };
            WriteProcessMemory(sroProcessHandle, changeVersionStringCodeCave, changeVersionByteArray, changeVersionByteArray.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _changeVersion, changeVersionCodeArray, changeVersionCodeArray.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _changeVersion - 59, hexColor, hexColor.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, startingMsgStringCodeCave, startingMsgByteArray, startingMsgByteArray.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _startingMsg, startingMsgCodeArray, startingMsgCodeArray.Length, _byteArray);
            WriteProcessMemory(sroProcessHandle, _startingMsg + 9, hexColor, hexColor.Length, _byteArray);
        }
        private static uint FindPattern(byte[] pattern, byte[] fileByteArray, uint result)
        {
            uint myPosition = 0;
            uint resultCounter = 0;
            for (uint positionFileByteArray = 0; positionFileByteArray < fileByteArray.Length - pattern.Length; positionFileByteArray++)
            {
                var found = true;
                for (uint positionPattern = 0; positionPattern < pattern.Length; positionPattern++)
                {
                    if (fileByteArray[positionFileByteArray + positionPattern] == pattern[positionPattern]) continue;
                    found = false;
                    break;
                }
                if (!found) continue;
                resultCounter += 1;
                if (result != resultCounter) continue;
                myPosition = positionFileByteArray;
                break;
            }
            return myPosition;
        }
        private static uint FindStringPattern(byte[] stringByteArray, byte[] fileArray, uint baseAddress, byte stringWorker, uint result)
        {
            byte[] stringWorkerAddress = { stringWorker, 0x00, 0x00, 0x00, 0x00 };
            var stringAddress = BitConverter.GetBytes(baseAddress + FindPattern(stringByteArray, fileArray, 1));
            stringWorkerAddress[1] = stringAddress[0];
            stringWorkerAddress[2] = stringAddress[1];
            stringWorkerAddress[3] = stringAddress[2];
            stringWorkerAddress[4] = stringAddress[3];

            var myPosition = baseAddress + FindPattern(stringWorkerAddress, fileArray, result);
            return myPosition;
        }
    }
}