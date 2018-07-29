using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Diagnostics.Runtime.Interop;

namespace CrimsonlandTrainer.Native {
    public static class NativeMethods {
        public static class DbgEng {
            [DllImport("dbgeng.dll", EntryPoint = "DebugCreate", CallingConvention = CallingConvention.StdCall)]
            public static extern int DebugCreate([In] ref System.Guid InterfaceId, ref System.IntPtr Interface);
        }
    }

    [Flags]
    public enum ProcessAccessFlags : uint {
        All = 0x001F0FFF,
        Terminate = 0x00000001,
        CreateThread = 0x00000002,
        VMOperation = 0x00000008,
        VMRead = 0x00000010,
        VMWrite = 0x00000020,
        DupHandle = 0x00000040,
        SetInformation = 0x00000200,
        QueryInformation = 0x00000400,
        Synchronize = 0x00100000
    }

    [Flags]
    public enum ProcessAccess {
        /// <summary>
        /// Required to create a thread.
        /// </summary>
        CreateThread = 0x0002,

        /// <summary>
        ///
        /// </summary>
        SetSessionId = 0x0004,

        /// <summary>
        /// Required to perform an operation on the address space of a process
        /// </summary>
        VmOperation = 0x0008,

        /// <summary>
        /// Required to read memory in a process using ReadProcessMemory.
        /// </summary>
        VmRead = 0x0010,

        /// <summary>
        /// Required to write to memory in a process using WriteProcessMemory.
        /// </summary>
        VmWrite = 0x0020,

        /// <summary>
        /// Required to duplicate a handle using DuplicateHandle.
        /// </summary>
        DupHandle = 0x0040,

        /// <summary>
        /// Required to create a process.
        /// </summary>
        CreateProcess = 0x0080,

        /// <summary>
        /// Required to set memory limits using SetProcessWorkingSetSize.
        /// </summary>
        SetQuota = 0x0100,

        /// <summary>
        /// Required to set certain information about a process, such as its priority class (see SetPriorityClass).
        /// </summary>
        SetInformation = 0x0200,

        /// <summary>
        /// Required to retrieve certain information about a process, such as its token, exit code, and priority class (see OpenProcessToken).
        /// </summary>
        QueryInformation = 0x0400,

        /// <summary>
        /// Required to suspend or resume a process.
        /// </summary>
        SuspendResume = 0x0800,

        /// <summary>
        /// Required to retrieve certain information about a process (see GetExitCodeProcess, GetPriorityClass, IsProcessInJob, QueryFullProcessImageName).
        /// A handle that has the PROCESS_QUERY_INFORMATION access right is automatically granted PROCESS_QUERY_LIMITED_INFORMATION.
        /// </summary>
        QueryLimitedInformation = 0x1000,

        /// <summary>
        /// Required to wait for the process to terminate using the wait functions.
        /// </summary>
        Synchronize = 0x100000,

        /// <summary>
        /// Required to delete the object.
        /// </summary>
        Delete = 0x00010000,

        /// <summary>
        /// Required to read information in the security descriptor for the object, not including the information in the SACL.
        /// To read or write the SACL, you must request the ACCESS_SYSTEM_SECURITY access right. For more information, see SACL Access Right.
        /// </summary>
        ReadControl = 0x00020000,

        /// <summary>
        /// Required to modify the DACL in the security descriptor for the object.
        /// </summary>
        WriteDac = 0x00040000,

        /// <summary>
        /// Required to change the owner in the security descriptor for the object.
        /// </summary>
        WriteOwner = 0x00080000,

        StandardRightsRequired = 0x000F0000,

        /// <summary>
        /// All possible access rights for a process object.
        /// </summary>
        AllAccess = StandardRightsRequired | Synchronize | 0xFFFF
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("e3acb9d7-7ec2-4f0c-a0da-e81e0cbbe628")]
    [ComImport]
    public interface IDebugClient5Fixed : IDebugClient {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int AttachKernel([In] DEBUG_ATTACH Flags, [MarshalAs(UnmanagedType.LPStr), In] string ConnectOptions);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetKernelConnectionOptions([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint OptionsSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetKernelConnectionOptions([MarshalAs(UnmanagedType.LPStr), In] string Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int StartProcessServer([In] DEBUG_CLASS Flags, [MarshalAs(UnmanagedType.LPStr), In] string Options, [In] IntPtr Reserved);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int ConnectProcessServer([MarshalAs(UnmanagedType.LPStr), In] string RemoteOptions, out ulong Server);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int DisconnectProcessServer([In] ulong Server);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetRunningProcessSystemIds(
            [In] ulong Server, [MarshalAs(UnmanagedType.LPArray), Out]
            uint[] Ids, [In] uint Count, out uint ActualCount);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetRunningProcessSystemIdByExecutableName([In] ulong Server, [MarshalAs(UnmanagedType.LPStr), In] string ExeName, [In] DEBUG_GET_PROC Flags, out uint Id);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetRunningProcessDescription([In] ulong Server, [In] uint SystemId, [In] DEBUG_PROC_DESC Flags, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder ExeName, [In] int ExeNameSize, out uint ActualExeNameSize, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder Description, [In] int DescriptionSize, out uint ActualDescriptionSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int AttachProcess([In] ulong Server, [In] uint ProcessID, [In] DEBUG_ATTACH AttachFlags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int CreateProcess([In] ulong Server, [MarshalAs(UnmanagedType.LPStr), In] string CommandLine, [In] DEBUG_CREATE_PROCESS Flags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int CreateProcessAndAttach([In] ulong Server, [MarshalAs(UnmanagedType.LPStr), In] string CommandLine, [In] DEBUG_CREATE_PROCESS Flags, [In] uint ProcessId, [In] DEBUG_ATTACH AttachFlags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetProcessOptions(out DEBUG_PROCESS Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int AddProcessOptions([In] DEBUG_PROCESS Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int RemoveProcessOptions([In] DEBUG_PROCESS Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetProcessOptions([In] DEBUG_PROCESS Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int OpenDumpFile([MarshalAs(UnmanagedType.LPStr), In] string DumpFile);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int WriteDumpFile([MarshalAs(UnmanagedType.LPStr), In] string DumpFile, [In] DEBUG_DUMP Qualifier);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int ConnectSession([In] DEBUG_CONNECT_SESSION Flags, [In] uint HistoryLimit);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int StartServer([MarshalAs(UnmanagedType.LPStr), In] string Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int OutputServer([In] DEBUG_OUTCTL OutputControl, [MarshalAs(UnmanagedType.LPStr), In] string Machine, [In] DEBUG_SERVERS Flags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int TerminateProcesses();

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int DetachProcesses();

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EndSession([In] DEBUG_END Flags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetExitCode(out uint Code);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int DispatchCallbacks([In] uint Timeout);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int ExitDispatch(
            [MarshalAs(UnmanagedType.Interface), In]
            IDebugClient Client);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int CreateClient([MarshalAs(UnmanagedType.Interface)] out IDebugClient Client);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetInputCallbacks([MarshalAs(UnmanagedType.Interface)] out IDebugInputCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetInputCallbacks(
            [MarshalAs(UnmanagedType.Interface), In]
            IDebugInputCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetOutputCallbacks(out IDebugOutputCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetOutputCallbacks([In] IDebugOutputCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetOutputMask(out DEBUG_OUTPUT Mask);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetOutputMask([In] DEBUG_OUTPUT Mask);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetOtherOutputMask(
            [MarshalAs(UnmanagedType.Interface), In]
            IDebugClient Client, out DEBUG_OUTPUT Mask);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetOtherOutputMask(
            [MarshalAs(UnmanagedType.Interface), In]
            IDebugClient Client, [In] DEBUG_OUTPUT Mask);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetOutputWidth(out uint Columns);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetOutputWidth([In] uint Columns);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetOutputLinePrefix([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint PrefixSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetOutputLinePrefix([MarshalAs(UnmanagedType.LPStr), In] string Prefix);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetIdentity([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint IdentitySize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int OutputIdentity([In] DEBUG_OUTCTL OutputControl, [In] uint Flags, [MarshalAs(UnmanagedType.LPStr), In] string Format);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetEventCallbacks(out IDebugEventCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetEventCallbacks([In] IDebugEventCallbacks Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int FlushCallbacks();

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int WriteDumpFile2([MarshalAs(UnmanagedType.LPStr), In] string DumpFile, [In] DEBUG_DUMP Qualifier, [In] DEBUG_FORMAT FormatFlags, [MarshalAs(UnmanagedType.LPStr), In] string Comment);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int AddDumpInformationFile([MarshalAs(UnmanagedType.LPStr), In] string InfoFile, [In] DEBUG_DUMP_FILE Type);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int EndProcessServer([In] ulong Server);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int WaitForProcessServerEnd([In] uint Timeout);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int IsKernelDebuggerEnabled();

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int TerminateCurrentProcess();

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int DetachCurrentProcess();

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int AbandonCurrentProcess();

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetRunningProcessSystemIdByExecutableNameWide([In] ulong Server, [MarshalAs(UnmanagedType.LPWStr), In] string ExeName, [In] DEBUG_GET_PROC Flags, out uint Id);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetRunningProcessDescriptionWide([In] ulong Server, [In] uint SystemId, [In] DEBUG_PROC_DESC Flags, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder ExeName, [In] int ExeNameSize, out uint ActualExeNameSize, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder Description, [In] int DescriptionSize, out uint ActualDescriptionSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int CreateProcessWide([In] ulong Server, [MarshalAs(UnmanagedType.LPWStr), In] string CommandLine, [In] DEBUG_CREATE_PROCESS CreateFlags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int CreateProcessAndAttachWide([In] ulong Server, [MarshalAs(UnmanagedType.LPWStr), In] string CommandLine, [In] DEBUG_CREATE_PROCESS CreateFlags, [In] uint ProcessId, [In] DEBUG_ATTACH AttachFlags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int OpenDumpFileWide([MarshalAs(UnmanagedType.LPWStr), In] string FileName, [In] ulong FileHandle);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int WriteDumpFileWide([MarshalAs(UnmanagedType.LPWStr), In] string DumpFile, [In] ulong FileHandle, [In] DEBUG_DUMP Qualifier, [In] DEBUG_FORMAT FormatFlags, [MarshalAs(UnmanagedType.LPWStr), In] string Comment);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int AddDumpInformationFileWide([MarshalAs(UnmanagedType.LPWStr), In] string FileName, [In] ulong FileHandle, [In] DEBUG_DUMP_FILE Type);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetNumberDumpFiles(out uint Number);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetDumpFile([In] uint Index, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint NameSize, out ulong Handle, out uint Type);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetDumpFileWide([In] uint Index, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint NameSize, out ulong Handle, out uint Type);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int AttachKernelWide([In] DEBUG_ATTACH Flags, [MarshalAs(UnmanagedType.LPWStr), In] string ConnectOptions);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetKernelConnectionOptionsWide([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint OptionsSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetKernelConnectionOptionsWide([MarshalAs(UnmanagedType.LPWStr), In] string Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int StartProcessServerWide([In] DEBUG_CLASS Flags, [MarshalAs(UnmanagedType.LPWStr), In] string Options, [In] IntPtr Reserved);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int ConnectProcessServerWide([MarshalAs(UnmanagedType.LPWStr), In] string RemoteOptions, out ulong Server);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int StartServerWide([MarshalAs(UnmanagedType.LPWStr), In] string Options);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int OutputServersWide([In] DEBUG_OUTCTL OutputControl, [MarshalAs(UnmanagedType.LPWStr), In] string Machine, [In] DEBUG_SERVERS Flags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetOutputCallbacksWide(out IDebugOutputCallbacksWide Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetOutputCallbacksWide([In] IDebugOutputCallbacksWide Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetOutputLinePrefixWide([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint PrefixSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetOutputLinePrefixWide([MarshalAs(UnmanagedType.LPWStr), In] string Prefix);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetIdentityWide([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint IdentitySize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int OutputIdentityWide([In] DEBUG_OUTCTL OutputControl, [In] uint Flags, [MarshalAs(UnmanagedType.LPWStr), In] string Machine);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetEventCallbacksWide(out IDebugEventCallbacksWide Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetEventCallbacksWide([In] IDebugEventCallbacksWide Callbacks);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int CreateProcess2([In] ulong Server, [MarshalAs(UnmanagedType.LPStr), In] string CommandLine, [In] ref DEBUG_CREATE_PROCESS_OPTIONS OptionsBuffer, [In] uint OptionsBufferSize, [MarshalAs(UnmanagedType.LPStr), In] string InitialDirectory, [MarshalAs(UnmanagedType.LPStr), In] string Environment);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int CreateProcess2Wide([In] ulong Server, [MarshalAs(UnmanagedType.LPWStr), In] string CommandLine, [In] ref DEBUG_CREATE_PROCESS_OPTIONS OptionsBuffer, [In] uint OptionsBufferSize, [MarshalAs(UnmanagedType.LPWStr), In] string InitialDirectory, [MarshalAs(UnmanagedType.LPWStr), In] string Environment);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int CreateProcessAndAttach2([In] ulong Server, [MarshalAs(UnmanagedType.LPStr), In] string CommandLine, [In] ref DEBUG_CREATE_PROCESS_OPTIONS OptionsBuffer, [In] uint OptionsBufferSize, [MarshalAs(UnmanagedType.LPStr), In] string InitialDirectory, [MarshalAs(UnmanagedType.LPStr), In] string Environment, [In] uint ProcessId, [In] DEBUG_ATTACH AttachFlags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int CreateProcessAndAttach2Wide([In] ulong Server, [MarshalAs(UnmanagedType.LPWStr), In] string CommandLine, [In] ref DEBUG_CREATE_PROCESS_OPTIONS OptionsBuffer, [In] uint OptionsBufferSize, [MarshalAs(UnmanagedType.LPWStr), In] string InitialDirectory, [MarshalAs(UnmanagedType.LPWStr), In] string Environment, [In] uint ProcessId, [In] DEBUG_ATTACH AttachFlags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int PushOutputLinePrefix([MarshalAs(UnmanagedType.LPStr), In] string NewPrefix, out ulong Handle);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int PushOutputLinePrefixWide([MarshalAs(UnmanagedType.LPWStr), In] string NewPrefix, out ulong Handle);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int PopOutputLinePrefix([In] ulong Handle);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNumberInputCallbacks(out uint Count);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNumberOutputCallbacks(out uint Count);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNumberEventCallbacks([In] DEBUG_EVENT Flags, out uint Count);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetQuitLockString([MarshalAs(UnmanagedType.LPStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint StringSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetQuitLockString([MarshalAs(UnmanagedType.LPStr), In] string LockString);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetQuitLockStringWide([MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder Buffer, [In] int BufferSize, out uint StringSize);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetQuitLockStringWide([MarshalAs(UnmanagedType.LPWStr), In] string LockString);
    }

    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [Guid("1656afa9-19c6-4e3a-97e7-5dc9160cf9c4")]
    [ComImport]
    public interface IDebugRegisters2Fixed {
        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetNumberRegisters(out uint Number);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetDescription([In] uint Register, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder NameBuffer, [In] int NameBufferSize, out uint NameSize, out DEBUG_REGISTER_DESCRIPTION Desc);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetIndexByName([MarshalAs(UnmanagedType.LPStr), In] string Name, out uint Index);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetValue([In] uint Register, out DEBUG_VALUE Value);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetValue([In] uint Register, ref DEBUG_VALUE Value);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetValues(
            [In] uint Count, [MarshalAs(UnmanagedType.LPArray), In] uint[] Indices, [In] uint Start, [MarshalAs(UnmanagedType.LPArray), Out]
            DEBUG_VALUE[] Values);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int SetValues([In] uint Count, [MarshalAs(UnmanagedType.LPArray), In] uint[] Indices, [In] uint Start, [MarshalAs(UnmanagedType.LPArray), In] DEBUG_VALUE[] Values);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int OutputRegisters([In] DEBUG_OUTCTL OutputControl, [In] DEBUG_REGISTERS Flags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetInstructionOffset(out ulong Offset);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetStackOffset(out ulong Offset);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        new int GetFrameOffset(out ulong Offset);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetDescriptionWide([In] uint Register, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder NameBuffer, [In] int NameBufferSize, out uint NameSize, out DEBUG_REGISTER_DESCRIPTION Desc);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetIndexByNameWide([MarshalAs(UnmanagedType.LPWStr), In] string Name, out uint Index);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetNumberPseudoRegisters(out uint Number);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPseudoDescription([In] uint Register, [MarshalAs(UnmanagedType.LPStr), Out] StringBuilder NameBuffer, [In] int NameBufferSize, out uint NameSize, out ulong TypeModule, out uint TypeId);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPseudoDescriptionWide([In] uint Register, [MarshalAs(UnmanagedType.LPWStr), Out] StringBuilder NameBuffer, [In] int NameBufferSize, out uint NameSize, out ulong TypeModule, out uint TypeId);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPseudoIndexByName([MarshalAs(UnmanagedType.LPStr), In] string Name, out uint Index);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPseudoIndexByNameWide([MarshalAs(UnmanagedType.LPWStr), In] string Name, out uint Index);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetPseudoValues(
            [In] uint Source, [In] uint Count, [MarshalAs(UnmanagedType.LPArray), In] uint[] Indices, [In] uint Start, [MarshalAs(UnmanagedType.LPArray), Out]
            DEBUG_VALUE[] Values);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetPseudoValues([In] uint Source, [In] uint Count, [MarshalAs(UnmanagedType.LPArray), In] uint[] Indices, [In] uint Start, [MarshalAs(UnmanagedType.LPArray), In] DEBUG_VALUE[] Values);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetValues2(
            [In] DEBUG_REGSRC Source, [In] uint Count, [MarshalAs(UnmanagedType.LPArray), In] uint[] Indices, [In] uint Start, [MarshalAs(UnmanagedType.LPArray), Out]
            DEBUG_VALUE[] Values);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int SetValues2([In] uint Source, [In] uint Count, [MarshalAs(UnmanagedType.LPArray), In] uint[] Indices, [In] uint Start, [MarshalAs(UnmanagedType.LPArray), In] DEBUG_VALUE[] Values);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int OutputRegisters2([In] uint OutputControl, [In] uint Source, [In] uint Flags);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetInstructionOffset2([In] uint Source, out ulong Offset);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetStackOffset2([In] uint Source, out ulong Offset);

        [MethodImpl(MethodImplOptions.PreserveSig)]
        int GetFrameOffset2([In] uint Source, out ulong Offset);
    }
}
