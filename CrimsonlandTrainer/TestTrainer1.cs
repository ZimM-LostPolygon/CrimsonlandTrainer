// using System;
// using System.Collections.Generic;
// using System.ComponentModel;
// using System.Diagnostics;
// using System.Linq;
// using System.Text;
// using System.Threading.Tasks;
// using Microsoft.Samples.Debugging.Native;
//
// namespace CrimsonlandTrainer {
//     unsafe class TestTrainer1 {
//         public static void Run(Process clProcess) {
//             ProcessModuleCollection clProcessModules = clProcess.Modules;
//
//             ProcessModule progModule = null;
//             foreach (ProcessModule module in clProcessModules) {
//                 if (module.ModuleName == "prog.dll") {
//                     progModule = module;
//                     break;
//                 }
//             }
//
//             Console.WriteLine(
//                 $"Module: {progModule.ModuleName}\r\n" +
//                 $"  BaseAddress: {progModule.BaseAddress.FormatAsHex()}\r\n" +
//                 $"  EntryPointAddress: {progModule.EntryPointAddress.FormatAsHex()}\r\n" +
//                 $"  ModuleMemorySize: 0x{progModule.ModuleMemorySize:X8}\r\n" +
//                 $"");
//
//             SafeWin32Handle openProcessHandle =
//                 NativeMethods.OpenProcess((int) Native.ProcessAccessFlags.All, false, clProcess.Id);
//
//             if (openProcessHandle.IsInvalid)
//                 throw new Exception("Failed to open process");
//
//             //callbackProvider = new UnmanagedCodeSupplier((PickUpWeaponDelegate)ttExecutionCallback, IntPtr.Size == sizeof(int) ? x86CodeForFastcallWrapperForExecutionDelegate : x64CodeForFastcallWrapperForExecutionDelegate, IntPtr.Size == sizeof(int) ? new UIntPtr(0xF0F0F0F0) : new UIntPtr(0xF0F0F0F0F0F0F0F0));
//
//             byte[] progMemory = new byte[progModule.ModuleMemorySize];
//             int readMemoryBytes;
//             NativeMethods.ReadProcessMemory(openProcessHandle.DangerousGetHandle(), progModule.BaseAddress, progMemory, (UIntPtr) progMemory.Length, out readMemoryBytes);
//             Console.WriteLine($"Read {readMemoryBytes} bytes");
//
//             IntPtr pickUpWeaponAddress;
//             byte[] pickUpWeaponSignature = HexUtility.HexStringToBytes("55 8B EC 83 E4 F8 80 3D A9".Replace(" ", ""));
//             fixed (void* progMemoryPtr = progMemory, pickUpWeaponSignaturePtr = pickUpWeaponSignature) {
//                 int pickUpWeaponAddressOffset =
//                     IndexOfUnchecked(progMemoryPtr, progMemory.Length, pickUpWeaponSignaturePtr, pickUpWeaponSignature.Length);
//                 Console.WriteLine($"PickUpWeapon offset: 0x{pickUpWeaponAddressOffset:X8}");
//                 if (pickUpWeaponAddressOffset == -1)
//                     throw new Exception("Pattern not found!");
//
//                 pickUpWeaponAddress = new IntPtr(pickUpWeaponAddressOffset + (int) progModule.BaseAddress);
//                 //pickUpWeaponAddress = (IntPtr) 0x10054AB0;
//                 Console.WriteLine($"PickUpWeapon address: {pickUpWeaponAddress.FormatAsHex()}");
//
//                 //NativeMethods.ReadProcessMemory()
//                 //new CliHelper()
//                 //OriginalPickUpWeaponFunction = Marshal.GetDelegateForFunctionPointer<PickUpWeaponDelegate>(pickUpWeaponAddressOffset);
//                 /*PickUpWeaponHookMethod = PickUpWeaponDelegateHook;
//                 _pickUpWeaponHook = EasyHook.LocalHook.Create(
//                     pickUpWeaponAddress,
//                     PickUpWeaponHookMethod,
//                     null);*/
//             }
//
//             ///////////
//             NativeMethods.DebugActiveProcess((uint) clProcess.Id);
//             Console.WriteLine(new Win32Exception());
//             NativeMethods.DebugSetProcessKillOnExit(false);
//
//             int mainThreadId = -1;
//             IntPtr mainThreadHandle = new IntPtr();
//
//             byte[] pickUpWeaponStartOriginalByte = new byte[1];
//             NativeMethods.ReadProcessMemory(
//                 openProcessHandle.DangerousGetHandle(),
//                 pickUpWeaponAddress,
//                 pickUpWeaponStartOriginalByte,
//                 (UIntPtr) pickUpWeaponStartOriginalByte.Length,
//                 out readMemoryBytes);
//
//             byte[] int3Byte = { 0xCC };
//             Native.NativeMethods.Kernel32.WriteProcessMemory(
//                 openProcessHandle.DangerousGetHandle(),
//                 pickUpWeaponAddress,
//                 int3Byte,
//                 (UIntPtr) int3Byte.Length,
//                 out readMemoryBytes);
//
//             Native.NativeMethods.Kernel32.FlushInstructionCache(
//                 openProcessHandle.DangerousGetHandle(),
//                 pickUpWeaponAddress,
//                 (UIntPtr) pickUpWeaponStartOriginalByte.Length);
//
//             X86Context context = new X86Context(ContextFlags.X86ContextAll);
//             IContextDirectAccessor contextAccess = context.OpenForDirectAccess();
//             while (true) {
//                 NativeMethods.ContinueStatus continueStatus = NativeMethods.ContinueStatus.DBG_CONTINUE;
//
//                 //
//                 // NativeMethods.DebugBreakProcess(openProcessHandle.DangerousGetHandle());
//                 // Console.WriteLine(new Win32Exception());
//                 //
//                 // NativeMethods.GetThreadContext(mainThreadHandl, contextAccess.RawBuffer);
//                 DebugEvent32 debugEvent32 = new DebugEvent32();
//                 NativeMethods.WaitForDebugEvent32(ref debugEvent32, unchecked((int) 0xFFFFFFFF));
//                 Console.WriteLine(debugEvent32.header.dwDebugEventCode);
//                 switch (debugEvent32.header.dwDebugEventCode) {
//                     case NativeDebugEventCode.None:
//                         break;
//                     case NativeDebugEventCode.EXCEPTION_DEBUG_EVENT:
//                         continueStatus = NativeMethods.ContinueStatus.DBG_EXCEPTION_NOT_HANDLED;
//                         Console.WriteLine("Exception code " + debugEvent32.union.Exception.ExceptionRecord.ExceptionCode + ", address: " + debugEvent32.union.Exception.ExceptionRecord.ExceptionAddress.FormatAsHex());
//                         switch (debugEvent32.union.Exception.ExceptionRecord.ExceptionCode) {
//                             case ExceptionCode.None:
//                                 break;
//                             case ExceptionCode.STATUS_BREAKPOINT:
//                                 if (debugEvent32.union.Exception.ExceptionRecord.ExceptionAddress == pickUpWeaponAddress) {
//                                     continueStatus = NativeMethods.ContinueStatus.DBG_CONTINUE;
//                                     if (mainThreadId == -1) {
//                                         mainThreadId = (int) debugEvent32.header.dwThreadId;
//                                         mainThreadHandle = NativeMethods.OpenThread(ThreadAccess.THREAD_ALL_ACCESS, false, (uint) mainThreadId);
//                                     }
//
//                                     Console.WriteLine("WEAPON PICKUP");
//
//                                     NativeMethods.GetThreadContext(mainThreadHandle, contextAccess.RawBuffer);
//                                     context.InstructionPointer -= 1;
//                                     context.SetSingleStepFlag(true);
//                                     context.SetRegisterByName("EDX", 17);
//                                     NativeMethods.SetThreadContext(mainThreadHandle, contextAccess.RawBuffer);
//
//                                     Native.NativeMethods.Kernel32.WriteProcessMemory(
//                                         openProcessHandle.DangerousGetHandle(),
//                                         pickUpWeaponAddress,
//                                         pickUpWeaponStartOriginalByte,
//                                         (UIntPtr) pickUpWeaponStartOriginalByte.Length,
//                                         out readMemoryBytes);
//
//                                     Native.NativeMethods.Kernel32.FlushInstructionCache(
//                                         openProcessHandle.DangerousGetHandle(),
//                                         pickUpWeaponAddress,
//                                         (UIntPtr) pickUpWeaponStartOriginalByte.Length);
//
//                                     //NativeMethods.
//                                     //NativeMethods.
//                                 }
//
//                                 break;
//                             case ExceptionCode.STATUS_SINGLESTEP:
//                                 continueStatus = NativeMethods.ContinueStatus.DBG_CONTINUE;
//
//                                 Native.NativeMethods.Kernel32.WriteProcessMemory(
//                                     openProcessHandle.DangerousGetHandle(),
//                                     pickUpWeaponAddress,
//                                     int3Byte,
//                                     (UIntPtr) int3Byte.Length,
//                                     out readMemoryBytes);
//
//                                 Native.NativeMethods.Kernel32.FlushInstructionCache(
//                                     openProcessHandle.DangerousGetHandle(),
//                                     pickUpWeaponAddress,
//                                     (UIntPtr) pickUpWeaponStartOriginalByte.Length);
//                                 break;
//                             case ExceptionCode.EXCEPTION_INT_DIVIDE_BY_ZERO:
//                                 break;
//                             case ExceptionCode.DBG_CONTROL_C:
//                                 break;
//                             case ExceptionCode.EXCEPTION_STACK_OVERFLOW:
//                                 break;
//                             case ExceptionCode.EXCEPTION_NONCONTINUABLE_EXCEPTION:
//                                 break;
//                             case ExceptionCode.EXCEPTION_ACCESS_VIOLATION:
//                                 break;
//                             default:
//                                 throw new ArgumentOutOfRangeException();
//                         }
//
//                         break;
//                     case NativeDebugEventCode.CREATE_THREAD_DEBUG_EVENT:
//                         break;
//                     case NativeDebugEventCode.CREATE_PROCESS_DEBUG_EVENT:
//                         break;
//                     case NativeDebugEventCode.EXIT_THREAD_DEBUG_EVENT:
//                         break;
//                     case NativeDebugEventCode.EXIT_PROCESS_DEBUG_EVENT:
//                         break;
//                     case NativeDebugEventCode.LOAD_DLL_DEBUG_EVENT:
//                         break;
//                     case NativeDebugEventCode.UNLOAD_DLL_DEBUG_EVENT:
//                         break;
//                     case NativeDebugEventCode.OUTPUT_DEBUG_STRING_EVENT:
//                         break;
//                     case NativeDebugEventCode.RIP_EVENT:
//                         break;
//                     default:
//                         throw new ArgumentOutOfRangeException();
//                 }
//
//                 NativeMethods.ContinueDebugEvent(debugEvent32.header.dwProcessId, debugEvent32.header.dwThreadId, continueStatus);
//             }
//         }
//
//         internal static unsafe int IndexOfUnchecked(void* haystack, int haystackSize, void* needle, int needleSize) {
//             if (haystackSize < needleSize)
//                 return -1;
//
//             if (needleSize == 0)
//                 return 0;
//
//             byte* thisptr = (byte*) haystack;
//             byte* valueptr = (byte*) needle;
//             byte* ap = thisptr;
//             byte* thisEnd = ap + haystackSize - needleSize + 1;
//             while (ap != thisEnd) {
//                 if (*ap == *valueptr) {
//                     for (int i = 1; i < needleSize; i++) {
//                         if (ap[i] != valueptr[i])
//                             goto NextVal;
//                     }
//
//                     return (int) (ap - thisptr);
//                 }
//
//                 NextVal:
//                 ap++;
//             }
//
//             return -1;
//         }
//     }
// }
