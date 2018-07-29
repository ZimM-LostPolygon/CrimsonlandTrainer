using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using CrimsonlandTrainer.Game;
using Microsoft.Diagnostics.Runtime.Interop;
using Prism.Mvvm;
using ThreadState = System.Threading.ThreadState;

namespace CrimsonlandTrainer
{
    class TrainerModel : BindableBase
    {
        private readonly DispatcherTimer _checkGameProcessTimer;
        private Process _gameProcess;
        private GameTrainerDebugger _debugger;
        private GameTrainerDebuggerThread _debuggerThread;
        private StringBuilder _debugEngineOutputBuffer = new StringBuilder();

        public int CurrentPerkIndex { get; private set; } = 0;
        public int CurrentWeaponIndex { get; private set; } = 0;

        public BuildModel BuildModel { get; set; } = new BuildModel();

        public TrainerStatus Status { get; private set; } = TrainerStatus.GameProcessNotFound;

        public string DebugEngineOutput {
            get {
                lock (_debugEngineOutputBuffer) {
                    return _debugEngineOutputBuffer.ToString();
                }
            }
        }

        public TrainerModel() {
            _checkGameProcessTimer = new DispatcherTimer();
            _checkGameProcessTimer.Tick += CheckGameProcessTimerOnTick;
            _checkGameProcessTimer.Interval = TimeSpan.FromSeconds(1);
        }

        public void Start() {
            _checkGameProcessTimer.Start();
        }

        public void Stop() {
            KillEverything();
            _checkGameProcessTimer.Stop();
        }

        public void ResetState() {
            CurrentPerkIndex = 0;
            CurrentWeaponIndex = 0;
        }

        private void CheckGameProcessTimerOnTick(object sender, EventArgs e) {
            void HandleWaitingForGameModule() {
                _gameProcess.Refresh();

                ProcessModule progModule = null;
                foreach (ProcessModule module in _gameProcess.Modules) {
                    if (module.ModuleName == "prog.dll") {
                        progModule = module;
                        break;
                    }
                }

                if (progModule == null)
                    return;

                _debugEngineOutputBuffer.Clear();
                _debuggerThread = new GameTrainerDebuggerThread();
                _debuggerThread.PreLoop += () => {
                    _debugger = new GameTrainerDebugger(_gameProcess, progModule);
                    _debugger.OutputReceived += (dbg, text) => {
                        lock (_debugEngineOutputBuffer) {
                            _debugEngineOutputBuffer.Append(text);
                        }

                        RaisePropertyChanged(nameof(DebugEngineOutput));
                    };

                    _debugger.StartTrainer();

                    _debugger.ProcessExited += DebuggerProcessExitedHandler;
                    _debugger.PickingUpPerk += DebuggerOnPickingUpPerk;
                    _debugger.PickingUpWeapon += DebuggerOnPickingUpWeapon;
                    _debugger.PickingUpBonus += DebuggerOnPickingUpBonus;
                };

                _debuggerThread.Loop += DebuggerThreadOnLoop;

                _debuggerThread.Thread.Start();

                Status = TrainerStatus.AttachedToGameProcess;
            }

            switch (Status) {
                case TrainerStatus.GameProcessNotFound:
                    Process gameProcess =
                        Process.GetProcesses()
                            .FirstOrDefault(
                                process => process.ProcessName == "Crimsonland" ||
                                    process.ProcessName == "Crimsonland-D3D11");

                    if (gameProcess != null) {
                        _gameProcess = gameProcess;
                        _gameProcess.EnableRaisingEvents = true;

                        void ExitedHandler(object o, EventArgs args) {
                            _gameProcess.Exited -= ExitedHandler;
                            _gameProcess = null;
                            KillEverything();
                        }

                        _gameProcess.Exited += ExitedHandler;
                        Status = TrainerStatus.WaitingForGameModule;
                        HandleWaitingForGameModule();
                    }
                    break;
                case TrainerStatus.WaitingForGameModule:
                    HandleWaitingForGameModule();

                    break;
                case TrainerStatus.AttachedToGameProcess:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private bool DebuggerThreadOnLoop(ConcurrentQueue<Func<bool>> concurrentQueue) {
            bool result = _debugger.HandleEvents();

            if (!result)
                return false;

            while (concurrentQueue.TryDequeue(out Func<bool> func)) {
                if (!func()) {
                    result = false;
                }
            }

            if (!result)
                return false;

            return true;
        }

        private void KillEverything() {
            RaisePropertyChanged(nameof(DebugEngineOutput));

            if (_debugger != null) {
                _debugger.ProcessExited -= DebuggerProcessExitedHandler;
                _debugger.PickingUpPerk -= DebuggerOnPickingUpPerk;
                _debugger.PickingUpWeapon -= DebuggerOnPickingUpWeapon;
                _debugger.PickingUpBonus -= DebuggerOnPickingUpBonus;
            }

            _debuggerThread?.Post(() => {
                _debugger?.Client.DetachProcesses();
                _debugger?.Dispose();
                return false;
            });

            if (_debuggerThread?.Thread?.IsAlive == true) {
                //_debuggerThread.Thread.Join();
            }

            Status = TrainerStatus.GameProcessNotFound;
        }

        private void DebuggerOnPickingUpBonus(ref Bonus bonus) {
            if ((bonus == Bonus.FireBullets && BuildModel.Settings.ReplaceFireBulletsWith500Points) ||
                (bonus == Bonus.PlasmaOverload && BuildModel.Settings.ReplacePlasmaOverloadWith500Points)) {
                Bonus originalBonus = bonus;
                _debuggerThread?.Post(() => {
                    _debugger.Control.Output(
                        DEBUG_OUTPUT.NORMAL,
                        $"Replacing bonus {originalBonus} with {Bonus.Points500}\r\n"
                    );
                    return true;
                });

                bonus = Bonus.Points500;
            }
        }

        private void DebuggerOnPickingUpWeapon(ref Weapon weapon) {
            if (CurrentWeaponIndex >= BuildModel.Weapons.Count)
                return;

            Weapon originalWeapon = weapon;
            weapon = BuildModel.Weapons[CurrentWeaponIndex];
            Weapon weapon1 = weapon;
            _debuggerThread?.Post(() => {
                _debugger.Control.Output(
                    DEBUG_OUTPUT.NORMAL,
                    $"Replacing weapon {originalWeapon} with {weapon1}\r\n"
                );
                return true;
            });
            CurrentWeaponIndex++;
        }

        private void DebuggerOnPickingUpPerk(ref Perk perk) {
            if (CurrentPerkIndex >= BuildModel.Perks.Count)
                return;

            Perk originalPerk = perk;
            perk = BuildModel.Perks[CurrentPerkIndex];
            Perk perk1 = perk;
            _debuggerThread?.Post(() => {
                _debugger.Control.Output(
                    DEBUG_OUTPUT.NORMAL,
                    $"Replacing perk {originalPerk} with {perk1}\r\n"
                );
                return true;
            });
            CurrentPerkIndex++;
        }

        private void DebuggerProcessExitedHandler(Debugger dbg, int code) {
            KillEverything();
        }
    }
}
