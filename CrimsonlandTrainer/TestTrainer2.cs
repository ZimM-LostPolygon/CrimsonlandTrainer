using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using CrimsonlandTrainer.Native;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;

namespace CrimsonlandTrainer {
    unsafe static class TestTrainer2 {
        private static IntPtr pickUpWeaponAddress;

        private static IDebugBreakpoint2 pickUpWeaponBreakpoint;

        public static void Run(Process clProcess) {
                        ProcessModuleCollection clProcessModules = clProcess.Modules;




            /////////////////////////////////
            Guid guid = typeof(IDebugClient).GUID;

            // create debug client object
            //object obj;
            //CheckHr(Native.NativeMethods.DbgEng.DebugCreate(ref guid, out obj));
            IntPtr pObj = IntPtr.Zero;
            CheckHr(Native.NativeMethods.DbgEng.DebugCreate(ref guid, ref pObj));
            IDebugClient5Fixed client = (IDebugClient5Fixed) Marshal.GetTypedObjectForIUnknown(pObj, typeof(IDebugClient5Fixed));
            IDebugDataSpaces4 dataSpaces = client as IDebugDataSpaces4;
            IDebugRegisters2Fixed registers = client as IDebugRegisters2Fixed;

            //IDebugClient5Fixed client = obj as IDebugClient5Fixed;
            IDebugControl6 control = client as IDebugControl6;
            var events = new EventCallbacks(control);
            client.SetEventCallbacksWide(events);
            client.SetOutputCallbacksWide(new OutputCallbacks());
            Console.CancelKeyPress += (s, e) => {
                e.Cancel = true;

                //control.SetInterrupt(DEBUG_INTERRUPT.ACTIVE);
            };
            CheckHr(client.AttachProcess(0, (uint) clProcess.Id, DEBUG_ATTACH.DEFAULT));

            ///////////
            ProcessModule progModule = null;
            foreach (ProcessModule module in clProcessModules) {
                if (module.ModuleName == "prog.dll") {
                    progModule = module;
                    break;
                }
            }

            Console.WriteLine(
                $"Module: {progModule.ModuleName}\r\n" +
                $"  BaseAddress: {progModule.BaseAddress.FormatAsHex()}\r\n" +
                $"  EntryPointAddress: {progModule.EntryPointAddress.FormatAsHex()}\r\n" +
                $"  ModuleMemorySize: 0x{progModule.ModuleMemorySize:X8}\r\n" +
                $"");


            //callbackProvider = new UnmanagedCodeSupplier((PickUpWeaponDelegate)ttExecutionCallback, IntPtr.Size == sizeof(int) ? x86CodeForFastcallWrapperForExecutionDelegate : x64CodeForFastcallWrapperForExecutionDelegate, IntPtr.Size == sizeof(int) ? new UIntPtr(0xF0F0F0F0) : new UIntPtr(0xF0F0F0F0F0F0F0F0));

            //byte[] progMemory = new byte[progModule.ModuleMemorySize];

            //return;


            ////////

            CheckHr(control.WaitForEvent(DEBUG_WAIT.DEFAULT, 5000));


            byte[] progMemory = new byte[progModule.ModuleMemorySize];
            uint readMemoryBytes;

            ulong validbase;
            uint validsize;
            CheckHr(dataSpaces.GetValidRegionVirtual((ulong) progModule.BaseAddress, (uint) progMemory.Length, out validbase, out validsize));


            //Console.WriteLine($"Read {readMemoryBytes} bytes");


            byte[] pickUpWeaponSignature;
            byte[] wildcard;
            ParseSearchPattern(
                "55 8B EC 83 E4 F8 80 3D A9 0F ?? 0F 00 56 57 8B F2 8B F9 75 23 80 3D 9A 0E ?? 0F 00 75 1A 56 E8 9C 94 F9 FF 83 FE 0F 75 0F 83 F8 32 7C 0A B9 D4",
                out pickUpWeaponSignature,
                out wildcard
                );
            //wildcard[3] = 1;
             ulong matchOffset;

            CheckHr(dataSpaces.ReadVirtualUncached((ulong) progModule.BaseAddress, progMemory, (uint) progMemory.Length, out readMemoryBytes));
            fixed (void* progMemoryPtr = progMemory, pickUpWeaponSignaturePtr = pickUpWeaponSignature, wildcardPtr = wildcard) {
                //pickUpWeaponAddress = (IntPtr) IndexOfUnchecked(progMemoryPtr, progMemory.Length, pickUpWeaponSignaturePtr, pickUpWeaponSignature.Length, wildcardPtr);
                     int pickUpWeaponAddressOffset =
                         IndexOfUnchecked(progMemoryPtr, progMemory.Length, pickUpWeaponSignaturePtr, pickUpWeaponSignature.Length, wildcardPtr);
                     Console.WriteLine($"PickUpWeapon offset: 0x{pickUpWeaponAddressOffset:X8}");
                     if (pickUpWeaponAddressOffset == -1)
                         throw new Exception("Pattern not found!");

                     pickUpWeaponAddress = new IntPtr(pickUpWeaponAddressOffset + (int) progModule.BaseAddress);
            }

            //dataSpaces.SearchVirtual((ulong) progModule.BaseAddress, (ulong) progModule.ModuleMemorySize, pickUpWeaponSignature, (uint) pickUpWeaponSignature.Length, 1, out matchOffset);
            //pickUpWeaponAddress = (IntPtr) matchOffset;
            Console.WriteLine($"PickUpWeapon address: {pickUpWeaponAddress.FormatAsHex()}");

            control.AddBreakpoint2(DEBUG_BREAKPOINT_TYPE.CODE, uint.MaxValue, out pickUpWeaponBreakpoint);
            pickUpWeaponBreakpoint.SetOffset((ulong) pickUpWeaponAddress);
            pickUpWeaponBreakpoint.SetFlags(DEBUG_BREAKPOINT_FLAG.ENABLED);

            pickUpWeaponBreakpoint.SetCommandWide(".echo WEAPON PICKUP!");

            // fixed (void* progMemoryPtr = progMemory, pickUpWeaponSignaturePtr = pickUpWeaponSignature) {
             //     int pickUpWeaponAddressOffset =
             //         IndexOfUnchecked(progMemoryPtr, progMemory.Length, pickUpWeaponSignaturePtr, pickUpWeaponSignature.Length);
             //     Console.WriteLine($"PickUpWeapon offset: 0x{pickUpWeaponAddressOffset:X8}");
             //     if (pickUpWeaponAddressOffset == -1)
             //         throw new Exception("Pattern not found!");
             //
             //     pickUpWeaponAddress = new IntPtr(pickUpWeaponAddressOffset + (int) progModule.BaseAddress);
             //     Console.WriteLine($"PickUpWeapon address: {pickUpWeaponAddress.FormatAsHex()}");
             // }

            control.SetExecutionStatus(DEBUG_STATUS.GO);

            File.WriteAllBytes("dump.bin", progMemory);

            DEBUG_STATUS status;
            int hr;

            while (true) {
                CheckHr(control.GetExecutionStatus(out status));
                if (status == DEBUG_STATUS.NO_DEBUGGEE) {
                    Console.WriteLine("No Target");
                    break;
                }

                if (status == DEBUG_STATUS.GO || status == DEBUG_STATUS.STEP_BRANCH ||
                    status == DEBUG_STATUS.STEP_INTO ||
                    status == DEBUG_STATUS.STEP_OVER) {
                    hr = control.WaitForEvent(DEBUG_WAIT.DEFAULT, uint.MaxValue);
                    continue;
                }

                if (events.StateChanged) {
                    Console.WriteLine();
                    events.StateChanged = false;
                    if (events.BreakpointHit) {
                        control.OutputCurrentState(DEBUG_OUTCTL.THIS_CLIENT,
                            DEBUG_CURRENT.DEFAULT);
                        events.BreakpointHit = false;
                    }
                }


                control.OutputPromptWide(DEBUG_OUTCTL.THIS_CLIENT, null);
                Console.Write(" ");
                Console.ForegroundColor = ConsoleColor.Gray;
                string command = Console.ReadLine();
                if (command == ".detach") {
                    client.DetachCurrentProcess();
                } else {
                    control.ExecuteWide(DEBUG_OUTCTL.THIS_CLIENT, command,
                        DEBUG_EXECUTE.DEFAULT);
                }
            }
        }

        private static void CheckHr(int hr) {
            if (hr < 0)
                throw new Exception("hr < 0, " + new Win32Exception());
        }



        public class EventCallbacks : IDebugEventCallbacksWide {
            readonly IDebugControl6 _control;
            public bool StateChanged { get; set; }
            public bool BreakpointHit { get; set; }

            public EventCallbacks(IDebugControl6 control) {
                _control = control;
            }

            public int GetInterestMask(out DEBUG_EVENT Mask) {
                Mask = DEBUG_EVENT.BREAKPOINT | DEBUG_EVENT.CHANGE_DEBUGGEE_STATE
                    | DEBUG_EVENT.CHANGE_ENGINE_STATE | DEBUG_EVENT.CHANGE_SYMBOL_STATE |
                    DEBUG_EVENT.CREATE_PROCESS | DEBUG_EVENT.CREATE_THREAD | DEBUG_EVENT.EXCEPTION | DEBUG_EVENT.EXIT_PROCESS |
                    DEBUG_EVENT.EXIT_THREAD | DEBUG_EVENT.LOAD_MODULE |
                    DEBUG_EVENT.SESSION_STATUS | DEBUG_EVENT.SYSTEM_ERROR |
                    DEBUG_EVENT.UNLOAD_MODULE;

                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int Breakpoint(IDebugBreakpoint2 Bp) {
                BreakpointHit = true;
                StateChanged = true;

                if (Bp == pickUpWeaponBreakpoint) {
                    IDebugRegisters2Fixed registers = _control as IDebugRegisters2Fixed;
                    CheckHr(registers.GetIndexByName("edx", out uint edxIndex));
                    DEBUG_VALUE edxValue;
                    CheckHr(registers.GetValue(edxIndex, out edxValue));
                    edxValue.I32 = 17;
                    try {
                        CheckHr(registers.SetValue(edxIndex, ref edxValue));
                    } catch (Exception e) {
                        Console.WriteLine(e);
                        throw;
                    }


                    edxValue = new DEBUG_VALUE();
                    CheckHr(registers.GetValue(edxIndex, out edxValue));
                    edxValue.I32 = 17;
                    CheckHr(registers.SetValue(edxIndex, edxValue));

                    return (int) DEBUG_STATUS.GO;
                }

                //return (int) DEBUG_STATUS.NO_CHANGE;
                return (int) DEBUG_STATUS.BREAK;
            }

            public int Exception(ref EXCEPTION_RECORD64 Exception, uint FirstChance) {
                BreakpointHit = true;
                //return (int) DEBUG_STATUS.NO_CHANGE;
                return (int) DEBUG_STATUS.BREAK;
            }

            public int CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int ExitThread(uint ExitCode) {
                return (int) DEBUG_STATUS.NO_CHANGE;
             }

            public int CreateProcess(ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset) {
                return (int) DEBUG_STATUS.BREAK;
            }

            public int ExitProcess(uint ExitCode) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int UnloadModule(string ImageBaseName, ulong BaseOffset) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int SystemError(uint Error, uint Level) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int SessionStatus(DEBUG_SESSION Status) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int ChangeDebuggeeState(DEBUG_CDS Flags, ulong Argument) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int ChangeEngineState(DEBUG_CES Flags, ulong Argument) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }

            public int ChangeSymbolState(DEBUG_CSS Flags, ulong Argument) {
                return (int) DEBUG_STATUS.NO_CHANGE;
            }
        }

        public class OutputCallbacks : IDebugOutputCallbacksWide {
            public int Output(DEBUG_OUTPUT Mask, string Text) {
                switch (Mask) {
                    case DEBUG_OUTPUT.DEBUGGEE:
                        Console.ForegroundColor = ConsoleColor.Gray;
                        break;

                    case DEBUG_OUTPUT.PROMPT:
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        break;

                    case DEBUG_OUTPUT.ERROR:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;

                    case DEBUG_OUTPUT.EXTENSION_WARNING:
                    case DEBUG_OUTPUT.WARNING:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case DEBUG_OUTPUT.SYMBOLS:
                        Console.ForegroundColor = ConsoleColor.Cyan;
                        break;

                    default:
                        Console.ForegroundColor = ConsoleColor.White;
                        break;
                }

                Console.Write(Text);
                return (int) DEBUG_STATUS.NO_CHANGE;
            }
        }

        private static void ParseSearchPattern(string hexString, out byte[] pattern, out byte[] wildcard) {
            if (hexString == null)
               throw new ArgumentNullException(nameof(hexString));

            hexString = hexString.Replace(" ", "");

            if (hexString.Length % 2 != 0)
                throw new FormatException("The hex string is invalid because it has an odd length");

            wildcard = new byte[hexString.Length / 2];
            for (int index = 0; index < wildcard.Length; ++index) {
                if (hexString.Substring(index * 2, 2) == "??") {
                    wildcard[index] = 1;
                }
            }

            hexString = hexString.Replace('?', '0');
            pattern = HexUtility.HexStringToBytes(hexString);
        }

        internal static unsafe int IndexOfUnchecked(void* haystack, int haystackSize, void* needle, int needleSize, void* wildcard = null) {
            if (haystackSize < needleSize)
                return -1;

            if (needleSize == 0)
                return 0;

            byte* thisptr = (byte*) haystack;
            byte* valueptr = (byte*) needle;
            byte* wildcardptr = (byte*) wildcard;
            byte* ap = thisptr;
            byte* thisEnd = ap + haystackSize - needleSize + 1;
            while (ap != thisEnd) {
                if (*ap == *valueptr) {
                    for (int i = 1; i < needleSize; i++) {
                        bool isMatch = (wildcardptr != null && wildcardptr[i] != 0) || ap[i] == valueptr[i];
                        if (!isMatch)
                            goto NextVal;
                    }

                    return (int) (ap - thisptr);
                }

                NextVal:
                ap++;
            }

            return -1;
        }
    }
}
