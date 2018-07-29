using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CrimsonlandTrainer.Game;
using CrimsonlandTrainer.Native;
using Microsoft.Diagnostics.Runtime.Interop;

namespace CrimsonlandTrainer {
    unsafe class GameTrainerDebugger : Debugger {
        private IDebugBreakpoint2 _pickUpWeaponBreakpoint;
        private IDebugBreakpoint2 _pickUpPerkBreakpoint;
        private IDebugBreakpoint2 _pickUpBonusBreakpoint;
        private IDebugBreakpoint2 _spawnCreatureBreakpoint;

        public delegate void PickingUpPerkCallback(ref Perk perk);
        public delegate void PickingUpWeaponCallback(ref Weapon weapon);
        public delegate void PickingUpBonusCallback(ref Bonus bonus);
        public delegate void SpawningCreatureCallback(ref Creature creatureId);

        public event PickingUpPerkCallback PickingUpPerk;
        public event PickingUpWeaponCallback PickingUpWeapon;
        public event PickingUpBonusCallback PickingUpBonus;
        public event SpawningCreatureCallback SpawningCreature;

        public GameTrainerDebugger(Process gameProcess, ProcessModule gameProgModule)
            : base() {
            GameProcess = gameProcess;
            GameProgModule = gameProgModule;
        }

        public ProcessModule GameProgModule { get; set; }

        public Process GameProcess { get; set; }

        public bool HandleEvents() {
            CheckHr(Control.GetExecutionStatus(out DEBUG_STATUS status));
            if (status == DEBUG_STATUS.NO_DEBUGGEE) {
                Console.WriteLine("No Target");
                return false;
            }

            if (status == DEBUG_STATUS.GO ||
                status == DEBUG_STATUS.STEP_BRANCH ||
                status == DEBUG_STATUS.STEP_INTO ||
                status == DEBUG_STATUS.STEP_OVER) {
                int evt = Control.WaitForEvent(DEBUG_WAIT.DEFAULT, 1000);
                CheckHr(evt);
                // S_FALSE
                if (evt == 1) {
                    CheckHr(Control.SetExecutionStatus(DEBUG_STATUS.GO));
                }

                return true;
            }

            if (BreakpointHit) {
                Control.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT, DEBUG_CURRENT.DEFAULT);
                BreakpointHit = false;
            }

            if (StateChanged) {
                StateChanged = false;
            }

            return true;
        }

        private void CreateBreakpoints() {
            byte[] progModuleMemory = ReadProgModuleMemory();

            CreatePickUpWeaponBreakpoint(progModuleMemory);
            CreatePickUpPerkBreakpoint(progModuleMemory);
            CreatePickUpBonusBreakpoint(progModuleMemory);
            //CreateSpawnCreatureBreakpoint(progModuleMemory);
        }

        private void CreatePickUpWeaponBreakpoint(byte[] progModuleMemory) {
            _pickUpWeaponBreakpoint = CreateBreakpointForPattern(
                progModuleMemory,
                "55 8B EC 83 E4 F8 80 3D A9 0F ?? ?? 00 56 57 8B F2 8B F9 75 23 80 3D 9A 0E ?? ?? 00 75 1A 56 E8 9C 94 F9 FF 83 FE 0F 75 0F 83 F8 32 7C 0A B9 D4",
                "PickUpWeapon"
            );
        }

        private void CreatePickUpPerkBreakpoint(byte[] progModuleMemory) {
            _pickUpPerkBreakpoint = CreateBreakpointForPattern(
                progModuleMemory,
                "55 8B EC 83 E4 F8 83 EC 0C 53 8B D9 8B 0D 1C F6 ?? ?? 56 57 89 5C 24 14 E8 03 A1 03 00 FF 04 9D 88 9B ?? ?? 3B 1D 0C BA ?? ?? A1 04 0F ?? ?? 0F",
                "PickUpPerk"
            );
        }

        private void CreatePickUpBonusBreakpoint(byte[] progModuleMemory) {
            _pickUpBonusBreakpoint = CreateBreakpointForPattern(
                progModuleMemory,
                //"55 8B EC 83 EC 4C 53 8B D9 8B 0D 1C F6 59 0F 56 57 8B FA 89 5D F8 E8 15 39 03 00 A1 58 BA 4D 0F F3 0F 10 05 10 74 4C 0F F3 0F 11 45 FC 83 3C 85",
                "55 8B EC 83 EC 4C 53 8B D9 8B 0D 1C F6 ?? ?? 56 57 8B FA 89 5D F8 E8 15 39 03 00 A1 58 BA ?? ?? F3 0F 10 05 10 74 ?? ?? F3 0F 11 45 FC 83 3C 85",
                "PickUpBonus"
            );
        }

        private void CreateSpawnCreatureBreakpoint(byte[] progModuleMemory) {
            _spawnCreatureBreakpoint = CreateBreakpointForPattern(
                progModuleMemory,
                //"55 8B EC 83 EC 4C 53 8B D9 8B 0D 1C F6 59 0F 56 57 8B FA 89 5D F8 E8 15 39 03 00 A1 58 BA 4D 0F F3 0F 10 05 10 74 4C 0F F3 0F 11 45 FC 83 3C 85",
                "55 8B EC 83 E4 F8 83 EC 08 56 57 FF 75 ?? ?? 28 C2 8B F1 FF 75 ?? ?? 28 C8 ?? ?? 11 44 24 14 E8 5C F0 FF FF 83 C4 08 84 ?? ?? 85 AF 00 00 00 8B",
                "SpawnCreature"
            );
        }

        public void StartTrainer() {
            Control.OutputWide(
                DEBUG_OUTPUT.NORMAL,
                $"Module: {GameProgModule.ModuleName}\r\n" +
                $"  BaseAddress: {GameProgModule.BaseAddress.FormatAsHex()}\r\n" +
                $"  EntryPointAddress: {GameProgModule.EntryPointAddress.FormatAsHex()}\r\n" +
                $"  ModuleMemorySize: 0x{GameProgModule.ModuleMemorySize:X8}\r\n");

            CheckHr(Client.AttachProcess(0, (uint) GameProcess.Id, DEBUG_ATTACH.DEFAULT));
            ProcessEvents(uint.MaxValue);

            CheckHr(Control.SetExecutionStatus(DEBUG_STATUS.GO));
            CreateBreakpoints();
        }

        public override int Breakpoint(IDebugBreakpoint2 breakpoint) {
            if (breakpoint == _pickUpWeaponBreakpoint) {
                CheckHr(Registers.GetIndexByName("edx", out uint edxIndex));
                CheckHr(Registers.GetValue(edxIndex, out DEBUG_VALUE edxValue));

                Control.OutputWide(
                    DEBUG_OUTPUT.NORMAL,
                    "Picking up weapon " + (Weapon) edxValue.I32 + "\r\n"
                    );

                Weapon weapon = (Weapon) edxValue.I32;
                PickingUpWeapon?.Invoke(ref weapon);
                edxValue.I32 = (uint) weapon;
                CheckHr(Registers.SetValue(edxIndex, ref edxValue));

                return (int) DEBUG_STATUS.GO;
            } else if (breakpoint == _pickUpPerkBreakpoint) {
                CheckHr(Registers.GetIndexByName("ecx", out uint ecxIndex));
                CheckHr(Registers.GetValue(ecxIndex, out DEBUG_VALUE ecxValue));

                Control.OutputWide(
                    DEBUG_OUTPUT.NORMAL,
                    "Picking up perk " + (Perk) ecxValue.I32 + "\r\n"
                );

                Perk perk = (Perk) ecxValue.I32;
                PickingUpPerk?.Invoke(ref perk);
                ecxValue.I32 = (uint) perk;
                CheckHr(Registers.SetValue(ecxIndex, ref ecxValue));

                return (int) DEBUG_STATUS.GO;
            } else if (breakpoint == _pickUpBonusBreakpoint) {
                CheckHr(Registers.GetIndexByName("edx", out uint edxIndex));
                CheckHr(Registers.GetValue(edxIndex, out DEBUG_VALUE edxValue));

                byte[] bonusIdBytes = new byte[4];
                CheckHr(DataSpaces.ReadVirtualUncached(edxValue.I64, bonusIdBytes, (uint) bonusIdBytes.Length, out uint bytesRead));
                Bonus bonus = (Bonus) BitConverter.ToInt32(bonusIdBytes, 0);

                Control.OutputWide(
                    DEBUG_OUTPUT.NORMAL,
                    "Picking up bonus " + (Bonus) bonus + "\r\n"
                );

                if (bonus != Bonus.Weapon) {
                    PickingUpBonus?.Invoke(ref bonus);
                    bonusIdBytes[0] = (byte) bonus;
                    DataSpaces.WriteVirtualUncached(edxValue.I64, bonusIdBytes, (uint) bonusIdBytes.Length, out uint bytesWritten);
                }

                return (int) DEBUG_STATUS.GO;
            } else if (breakpoint == _spawnCreatureBreakpoint) {
                CheckHr(Registers.GetIndexByName("ecx", out uint ecxIndex));
                CheckHr(Registers.GetValue(ecxIndex, out DEBUG_VALUE ecxValue));

                Control.OutputWide(
                    DEBUG_OUTPUT.NORMAL,
                    "Spawning creature " + (Creature) ecxValue.I32 + "\r\n"
                );

                Creature creature = (Creature) ecxValue.I32;
                SpawningCreature?.Invoke(ref creature);
                ecxValue.I32 = (uint) creature;
                CheckHr(Registers.SetValue(ecxIndex, ref ecxValue));

                return (int) DEBUG_STATUS.GO;
            }

            return base.Breakpoint(breakpoint);
        }

        public override int Exception(ref EXCEPTION_RECORD64 Exception, uint FirstChance) {
            return (int) DEBUG_STATUS.GO_NOT_HANDLED;
        }

        public override int CreateProcess(
            ulong ImageFileHandle,
            ulong Handle,
            ulong BaseOffset,
            uint ModuleSize,
            string ModuleName,
            string ImageName,
            uint CheckSum,
            uint TimeDateStamp,
            ulong InitialThreadHandle,
            ulong ThreadDataOffset,
            ulong StartOffset
        ) {
            base.CreateProcess(
                ImageFileHandle,
                Handle,
                BaseOffset,
                ModuleSize,
                ModuleName,
                ImageName,
                CheckSum,
                TimeDateStamp,
                InitialThreadHandle,
                ThreadDataOffset,
                StartOffset
            );

            return (int) DEBUG_STATUS.BREAK;
        }

        private byte[] ReadProgModuleMemory() {
            byte[] progModuleMemory = new byte[GameProgModule.ModuleMemorySize];
            CheckHr(DataSpaces.ReadVirtualUncached(
                (ulong) GameProgModule.BaseAddress,
                progModuleMemory,
                (uint) progModuleMemory.Length,
                out uint progModuleReadMemoryBytes)
            );
            return progModuleMemory;
        }

        private IDebugBreakpoint2 CreateBreakpointForPattern(byte[] progModuleMemory, string searchPattern, string name) {
            (byte[] searchPatternBytes, byte[] searchPatternWildcardBytes) = ParseSearchPattern(searchPattern);

            IntPtr codeAddress;
            fixed (
                void* progModuleMemoryPtr = progModuleMemory,
                searchPatternPtr = searchPatternBytes,
                wildcardPtr = searchPatternWildcardBytes
                ) {
                ulong codeOffset =
                    IndexOfUnchecked(
                        progModuleMemoryPtr,
                        (ulong) progModuleMemory.LongLength,
                        searchPatternPtr,
                        (ulong) searchPatternBytes.LongLength,
                        wildcardPtr
                        );
                Control.OutputWide(
                    DEBUG_OUTPUT.NORMAL,
                    $"{name} offset: 0x{codeOffset:X8}\r\n"
                );
                if (codeOffset == ulong.MaxValue) {
                    Control.OutputWide(DEBUG_OUTPUT.ERROR, "Dumping prog.dll memory to prog.dll.memory");
                    File.WriteAllBytes("prog.dll.memory", progModuleMemory);
                    throw new Exception($"Pattern '{searchPattern}' not found for function {name}!");
                }

                codeAddress = new IntPtr((long) (codeOffset + (ulong) GameProgModule.BaseAddress));
            }

            Control.OutputWide(
                DEBUG_OUTPUT.NORMAL,
                $"{name} address: {codeAddress.FormatAsHex()}\n"
            );

            Control.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.CODE, uint.MaxValue, out IDebugBreakpoint2 breakpoint);
            breakpoint.SetOffset((ulong) codeAddress);
            breakpoint.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);
            //breakpoint.SetCommandWide(".echo Entered " + name);

            return breakpoint;
        }

        private static (byte[] pattern, byte[] wildcard) ParseSearchPattern(string hexString) {
            if (hexString == null)
                throw new ArgumentNullException(nameof(hexString));

            hexString = hexString.Replace(" ", "");

            if (hexString.Length % 2 != 0)
                throw new FormatException("The hex string is invalid because it has an odd length");

            (byte[] pattern, byte[] wildcard) result;

            result.wildcard = new byte[hexString.Length / 2];
            for (int index = 0; index < result.wildcard.Length; ++index) {
                if (hexString.Substring(index * 2, 2) == "??") {
                    result.wildcard[index] = 1;
                }
            }

            hexString = hexString.Replace('?', '0');
            result.pattern = HexUtility.HexStringToBytes(hexString);

            return result;
        }

        internal static unsafe ulong IndexOfUnchecked(void* haystack, ulong haystackSize, void* needle, ulong needleSize, void* wildcard = null) {
            if (haystackSize < needleSize)
                return ulong.MaxValue;

            if (needleSize == 0)
                return 0;

            byte* thisptr = (byte*) haystack;
            byte* valueptr = (byte*) needle;
            byte* wildcardptr = (byte*) wildcard;
            byte* ap = thisptr;
            byte* thisEnd = ap + haystackSize - needleSize + 1;
            while (ap != thisEnd) {
                if (*ap == *valueptr) {
                    for (ulong i = 1; i < needleSize; i++) {
                        bool isMatch = (wildcardptr != null && wildcardptr[i] != 0) || ap[i] == valueptr[i];
                        if (!isMatch)
                            goto NextVal;
                    }

                    return (ulong) (ap - thisptr);
                }

                NextVal:
                ap++;
            }

            return ulong.MaxValue;
        }
    }
}
