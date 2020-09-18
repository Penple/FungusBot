using Discord;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace FungusBot {
    public class MemoryService {
        private const int PROCESS_WM_READ = 0x0010;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess,
        IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);

        Process process;
        ProcessModule module;

        private readonly IServiceProvider _services;

        public MemoryService(IServiceProvider services) {
            _services = services;

            process = Process.GetProcessesByName("Among Us")[0];

            module = process.Modules.Cast<ProcessModule>().SingleOrDefault(m => string.Equals(m.ModuleName, "GameAssembly.dll", StringComparison.OrdinalIgnoreCase));
        }

        public Int32 Read(Int32 address, params Int32[] offsets) {
            Byte[] buffer = new Byte[4];
            Int32 bytesRead = 0;
            Int32 processHandle = (Int32)process.Handle;

            IntPtr baseAddress = module.BaseAddress + address;
            ReadProcessMemory(processHandle, baseAddress, buffer, buffer.Length, ref bytesRead);
            Int32 baseValue = BitConverter.ToInt32(buffer, 0);
            foreach (Int32 offset in offsets) {
                bytesRead = 0;
                baseAddress = (IntPtr)(baseValue + offset);
                ReadProcessMemory(processHandle, baseAddress, buffer, buffer.Length, ref bytesRead);
                baseValue = BitConverter.ToInt32(buffer, 0);
            }
            return baseValue;
        }

        public string ReadString(Int32 address, params Int32[] offsets) {
            Int32 offset = offsets[offsets.Length - 1];
            Array.Resize(ref offsets, offsets.Length - 1);
            IntPtr baseAddress = (IntPtr)Read(address, offsets) + offset;

            Byte[] sizeBuffer = new Byte[4];
            Int32 bytesRead = 0;
            Int32 processHandle = (Int32)process.Handle;

            ReadProcessMemory(processHandle, baseAddress, sizeBuffer, sizeBuffer.Length, ref bytesRead);
            Int32 size = BitConverter.ToInt32(sizeBuffer, 0) * 2;

            bytesRead = 0;
            Byte[] stringBuffer = new Byte[size];
            ReadProcessMemory(processHandle, baseAddress + 0x4, stringBuffer, size, ref bytesRead);
            return Encoding.Unicode.GetString(stringBuffer);
        }

        public bool ReadBool(Int32 address, params Int32[] offsets) {
            Int32 offset = offsets[offsets.Length - 1];
            Array.Resize(ref offsets, offsets.Length - 1);
            IntPtr baseAddress = (IntPtr)Read(address, offsets) + offset;

            Byte[] buffer = new Byte[1];
            Int32 bytesRead = 0;
            Int32 processHandle = (Int32)process.Handle;

            ReadProcessMemory(processHandle, baseAddress, buffer, buffer.Length, ref bytesRead);
            return BitConverter.ToBoolean(buffer, 0);
        }

        public GameState ReadGameState() { // these pointers make me sad, in reality we should only resolve them once
            GameState.VoteState voteState = (GameState.VoteState)Read(0x00D110CC, 0x3C, 0x24, 0x20, 0x5C, 0x0, 0x62C);
            GameState gameState = new GameState(voteState);
            int playerCount = Read(0x00DA5A60, 0x5C, 0x0, 0x24, 0xC);
            for (int i = 0; i < playerCount; i++) {
                GameState.Player.PlayerColor color = (GameState.Player.PlayerColor)Read(0x00DA5A60, 0x5C, 0x0, 0x24, 0x8, 0x10 + 0x4 * i, 0x10);
                string name = ReadString(0x00DA5A60, 0x5C, 0x0, 0x24, 0x8, 0x10 + 0x4 * i, 0xC, 0x8);
                bool dead = ReadBool(0x00DA5A60, 0x5C, 0x0, 0x24, 0x8, 0x10 + 0x4 * i, 0x29);
                gameState.players.Add(new GameState.Player(color, name, dead));
            }
            return gameState;
        }
    }
}
