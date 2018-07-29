using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CrimsonlandTrainer
{
    class GameTrainerDebuggerThread
    {
        private readonly ConcurrentQueue<Func<bool>> _actionsQueue = new ConcurrentQueue<Func<bool>>();
        public Thread Thread { get; }

        public event Action PreLoop;
        public event Func<ConcurrentQueue<Func<bool>>, bool> Loop;
        public event Action PostLoop;

        public GameTrainerDebuggerThread() {
            Thread = new Thread(ThreadLoop);

            Thread.Name = "TrainerDebuggerThread";
            Thread.IsBackground = false;
            Thread.SetApartmentState(ApartmentState.STA);
        }

        private void ThreadLoop() {
            PreLoop();
            while (Loop(_actionsQueue)) {

            }
            PostLoop?.Invoke();
        }

        public void Post(Func<bool> action) {
            _actionsQueue.Enqueue(action);
        }
    }
}
