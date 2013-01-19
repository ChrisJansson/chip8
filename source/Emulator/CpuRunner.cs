using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Cj.Chip8.Cpu;

namespace Gui
{
    public class CpuRunner
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        private readonly Stopwatch _stopwatch;
        private readonly Display _display;
        private readonly DisplayAdapter _displayAdapter;
        private readonly Chip8Cpu _cpu;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private Task _task;

        public CpuRunner(MainWindowViewModel mainWindowViewModel, Chip8Cpu cpu, Display display)
        {
            _display = display;
            _cpu = cpu;
            _mainWindowViewModel = mainWindowViewModel;
            _stopwatch = new Stopwatch();
            _displayAdapter = new DisplayAdapter();

            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Start()
        {
            var cancellationToken = _cancellationTokenSource.Token;
            _task = new Task(() => RunCpu(cancellationToken), cancellationToken);
            _task.Start();
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
        }

        private void RunCpu(CancellationToken cancellationToken)
        {
            _stopwatch.Start();
            var tickSpeed = TimeSpan.FromSeconds(1/10000.0);
            var nextTick = TimeSpan.Zero;

            while (!cancellationToken.IsCancellationRequested)
            {
                var elapsed = _stopwatch.Elapsed;
                if (elapsed > nextTick)
                {
                    nextTick = elapsed + tickSpeed;

                    _cpu.EmulateCycle();
                    DrawDisplay();
                }
            }
        }

        private void DrawDisplay()
        {
            if (!_display.Dirty)
            {
                return;
            }

            _mainWindowViewModel.View.Dispatcher.Invoke(() => _mainWindowViewModel.BitmapSource = _displayAdapter.CreateBitmap(_display));
            _display.Dirty = false;
        }
    }
}