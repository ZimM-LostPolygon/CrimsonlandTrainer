using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using CrimsonlandTrainer.Native;
using Microsoft.Diagnostics.Runtime;
using Microsoft.Diagnostics.Runtime.Interop;

namespace CrimsonlandTrainer {
    public class Debugger : IDebugOutputCallbacksWide, IDebugEventCallbacksWide, IDisposable {
        private DataTarget _dataTarget;
        private bool _exited, _processing;

        #region Events

        public delegate void ModuleEventHandler(Debugger dbg, ModuleEventArgs args);

        public delegate void CreateThreadEventHandler(Debugger dbg, CreateThreadArgs args);

        public delegate void ExitThreadEventHandler(Debugger dbg, int exitCode);

        public delegate void CreateProcessEventHandler(Debugger dbg, CreateProcessArgs args);

        public delegate void ExitProcessEventHandler(Debugger dbg, int exitCode);

        public delegate void ExceptionEventHandler(Debugger dbg, EXCEPTION_RECORD64 ex);

        public delegate void OutputEventHandler(Debugger dbg, string text);

        public event ModuleEventHandler ModuleLoaded;
        public event ModuleEventHandler ModuleUnloaded;

        public event CreateThreadEventHandler ThreadCreated;
        public event ExitThreadEventHandler ThreadExited;

        public event ExceptionEventHandler FirstChanceExceptionCaught;
        public event ExceptionEventHandler SecondChanceExceptionCaught;
        public event CreateProcessEventHandler ProcessCreated;
        public event ExitProcessEventHandler ProcessExited;

        public event OutputEventHandler OutputReceived;

        #endregion

        public IDebugClient5Fixed Client { get; set; }
        public IDebugControl6 Control { get; set; }
        public IDebugDataSpaces4 DataSpaces { get; set; }
        public IDebugRegisters2Fixed Registers { get; set; }

        public bool StateChanged { get; protected set; }
        public bool BreakpointHit { get; protected set; }

        public DataTarget DataTarget => _dataTarget ?? (_dataTarget = DataTarget.CreateFromDebuggerInterface(Client));

        public Debugger() : this(CreateDebugClient()) {
        }

        public Debugger(IDebugClient5Fixed client) {
            Client = client;
            Control = (IDebugControl6) Client;
            DataSpaces = (IDebugDataSpaces4) Client;
            Registers = (IDebugRegisters2Fixed) Client;

            Client.SetOutputCallbacksWide(this);
            Client.SetEventCallbacksWide(this);
        }

        public DEBUG_STATUS ProcessEvents(uint timeout) {
            if (_processing) {
                throw new InvalidOperationException("Cannot call ProcessEvents reentrantly.");
            }

            if (_exited) {
                return DEBUG_STATUS.NO_DEBUGGEE;
            }

            _processing = true;
            int hr = Control.WaitForEvent(0, timeout);
            _processing = false;

            if (hr < 0 && (uint) hr != 0x8000000A) {
                throw new Exception(GetExceptionString("IDebugControl::WaitForEvent", hr));
            }

            return GetDebugStatus();
        }

        public void DetachFromProcess() {
            _exited = true;
            CheckHr(Client.DetachCurrentProcess());
        }

        public void TerminateProcess() {
            _exited = true;
            CheckHr(Client.EndSession(DEBUG_END.ACTIVE_TERMINATE));
        }

        #region Helpers

        private static IDebugClient5Fixed CreateDebugClient() {
            Guid guid = typeof(IDebugClient).GUID;
            IntPtr pObj = IntPtr.Zero;
            CheckHr(Native.NativeMethods.DbgEng.DebugCreate(ref guid, ref pObj));
            return (IDebugClient5Fixed) Marshal.GetTypedObjectForIUnknown(pObj, typeof(IDebugClient5Fixed));
        }

        private void SetDebugStatus(DEBUG_STATUS status) {
            int hr = Control.SetExecutionStatus(status);
            if (hr < 0) {
                throw new Exception(GetExceptionString("IDebugControl::SetExecutionStatus", hr));
            }
        }

        private DEBUG_STATUS GetDebugStatus() {
            int hr = Control.GetExecutionStatus(out DEBUG_STATUS status);

            if (hr < 0) {
                throw new Exception(GetExceptionString("IDebugControl::GetExecutionStatus", hr));
            }

            return status;
        }

        private static string GetExceptionString(string name, int hr) {
            return string.Format("{0} failed with hresult={1:X8}", name, hr);
        }

        protected static void CheckHr(int hr) {
            if (hr < 0)
                throw new Exception("hr < 0, " + new Win32Exception());
        }

        #endregion

        #region IDebugOutputCallbacks

        public virtual int Output(DEBUG_OUTPUT Mask, string Text) {
            OutputReceived?.Invoke(this, Text);

            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        #endregion

        #region IDebugEventCallbacks

        public virtual int GetInterestMask(out DEBUG_EVENT Mask) {
            Mask =
                DEBUG_EVENT.BREAKPOINT |
                DEBUG_EVENT.CHANGE_DEBUGGEE_STATE |
                DEBUG_EVENT.CHANGE_ENGINE_STATE |
                DEBUG_EVENT.CHANGE_SYMBOL_STATE |
                DEBUG_EVENT.CREATE_PROCESS |
                DEBUG_EVENT.CREATE_THREAD |
                DEBUG_EVENT.EXCEPTION |
                DEBUG_EVENT.EXIT_PROCESS |
                DEBUG_EVENT.EXIT_THREAD |
                DEBUG_EVENT.LOAD_MODULE |
                DEBUG_EVENT.SESSION_STATUS |
                DEBUG_EVENT.SYSTEM_ERROR |
                DEBUG_EVENT.UNLOAD_MODULE;

            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int Breakpoint(IDebugBreakpoint2 breakpoint) {
            return (int) DEBUG_STATUS.GO;
        }

        public virtual int CreateProcess(
            ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName,
            uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset) {
            BreakpointHit = true;
            StateChanged = true;

            ProcessCreated?.Invoke(this, new CreateProcessArgs(ImageFileHandle, Handle, BaseOffset, ModuleSize, ModuleName, ImageName, CheckSum, TimeDateStamp, InitialThreadHandle, ThreadDataOffset, StartOffset));

            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ExitProcess(uint ExitCode) {
            ProcessExited?.Invoke(this, (int) ExitCode);

            _exited = true;
            return (int) DEBUG_STATUS.BREAK;
        }

        public virtual int CreateThread(ulong Handle, ulong DataOffset, ulong StartOffset) {
            ThreadCreated?.Invoke(this, new CreateThreadArgs(Handle, DataOffset, StartOffset));

            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ExitThread(uint ExitCode) {
            ThreadExited?.Invoke(this, (int) ExitCode);

            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int Exception(ref EXCEPTION_RECORD64 Exception, uint FirstChance) {
            BreakpointHit = true;

            (FirstChance == 1 ? FirstChanceExceptionCaught : SecondChanceExceptionCaught)?.Invoke(this, Exception);

            return (int) DEBUG_STATUS.BREAK;
        }

        public virtual int LoadModule(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp) {
            ModuleLoaded?.Invoke(this, new ModuleEventArgs(ImageFileHandle, BaseOffset, ModuleSize, ModuleName, ImageName, CheckSum, TimeDateStamp));

            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int UnloadModule(string ImageBaseName, ulong BaseOffset) {
            ModuleUnloaded?.Invoke(this, new ModuleEventArgs(ImageBaseName, BaseOffset));

            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int SessionStatus(DEBUG_SESSION Status) {
            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int SystemError(uint Error, uint Level) {
            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ChangeDebuggeeState(DEBUG_CDS Flags, ulong Argument) {
            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ChangeEngineState(DEBUG_CES Flags, ulong Argument) {
            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public virtual int ChangeSymbolState(DEBUG_CSS Flags, ulong Argument) {
            return (int) DEBUG_STATUS.NO_CHANGE;
        }

        public void Dispose() {
            Client.SetEventCallbacks(null);
            Client.SetOutputCallbacks(null);
        }

        #endregion

        public class ModuleEventArgs {
            public ulong BaseOffset;
            public uint CheckSum;
            public ulong ImageFileHandle;
            public string ImageName;
            public string ModuleName;
            public uint ModuleSize;
            public uint TimeDateStamp;

            public ModuleEventArgs(string imageBaseName, ulong baseOffset) {
                ImageName = imageBaseName;
                BaseOffset = baseOffset;
            }

            public ModuleEventArgs(ulong ImageFileHandle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName, uint CheckSum, uint TimeDateStamp) {
                this.ImageFileHandle = ImageFileHandle;
                this.BaseOffset = BaseOffset;
                this.ModuleSize = ModuleSize;
                this.ModuleName = ModuleName;
                this.ImageName = ImageName;
                this.CheckSum = CheckSum;
                this.TimeDateStamp = TimeDateStamp;
            }
        }

        public class CreateThreadArgs {
            public ulong DataOffset;
            public ulong Handle;
            public ulong StartOffset;

            public CreateThreadArgs(ulong handle, ulong data, ulong start) {
                Handle = handle;
                DataOffset = data;
                StartOffset = start;
            }
        }

        public class CreateProcessArgs {
            public ulong BaseOffset;
            public uint CheckSum;
            public ulong Handle;
            public ulong ImageFileHandle;
            public string ImageName;
            public ulong InitialThreadHandle;
            public string ModuleName;
            public uint ModuleSize;
            public ulong StartOffset;
            public ulong ThreadDataOffset;
            public uint TimeDateStamp;

            public CreateProcessArgs(
                ulong ImageFileHandle, ulong Handle, ulong BaseOffset, uint ModuleSize, string ModuleName, string ImageName,
                uint CheckSum, uint TimeDateStamp, ulong InitialThreadHandle, ulong ThreadDataOffset, ulong StartOffset) {
                this.ImageFileHandle = ImageFileHandle;
                this.Handle = Handle;
                this.BaseOffset = BaseOffset;
                this.ModuleSize = ModuleSize;
                this.ModuleName = ModuleName;
                this.ImageName = ImageName;
                this.CheckSum = CheckSum;
                this.TimeDateStamp = TimeDateStamp;
                this.InitialThreadHandle = InitialThreadHandle;
                this.ThreadDataOffset = ThreadDataOffset;
                this.StartOffset = StartOffset;
            }
        }
    }

}
