using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Cj.Chip8.Cpu;
using Microsoft.Win32;
using System.Linq;

namespace Emulator
{
    public class DelegateCommand : ICommand
    {
        private Action _delegateAction;

        public DelegateCommand(Action delegateAction)
        {
            _delegateAction = delegateAction;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            _delegateAction();
        }

        public event EventHandler CanExecuteChanged;
    }

    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel()
        {
            Load = new DelegateCommand(LoadRom);
        }

        public DelegateCommand Load { get; set; }

        private void LoadRom()
        {
            var openFileDialog = new OpenFileDialog();
            var showDialog = openFileDialog.ShowDialog(View);

            if (!showDialog.HasValue || !showDialog.Value)
                return;

            var fileStream = openFileDialog.OpenFile();

            var buffer = new byte[fileStream.Length];
            fileStream.Read(buffer, 0, (int)fileStream.Length);

            CreateCpu(buffer);
        }

        private void CreateCpu(byte[] buffer)
        {
            var display = new Display();
            var randomizer = new Randomizer();
            var wpfKeyboard = new WpfKeyboard(View);
            var bcdConverter = new BcdConverter();
            var chip8Cpu = new Chip8Cpu(display, randomizer, wpfKeyboard, bcdConverter);

            Array.Copy(buffer, 0, chip8Cpu.State.Memory, 0x200, buffer.Length);
            chip8Cpu.State.ProgramCounter = 0x200;

            var displayAdapter = new DisplayAdapter();

            var thread = new Thread(() =>
                {
                    while (true)
                    {
                        chip8Cpu.EmulateCycle();

                        if (display.Dirty)
                        {
                            View.Dispatcher.Invoke(() => BitmapSource = displayAdapter.CreateBitmap(display));    
                            display.Dirty = false;
                        }

                        Thread.Sleep(1);
                    }
                });

            thread.Start();
        }

        private BitmapSource _bitmapSource;
        public BitmapSource BitmapSource
        {
            get { return _bitmapSource; }
            set
            {
                _bitmapSource = value;
                PropertyChanged(this, new PropertyChangedEventArgs("BitmapSource"));
            }
        }

        public MainWindow View { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WpfKeyboard : IKeyboard
    {
        public WpfKeyboard(FrameworkElement framework)
        {
            _keys = new Dictionary<byte, Key>
                {
                    { 0x00, Key.NumPad0},
                    { 0x01, Key.NumPad1},
                    { 0x02, Key.NumPad2},
                    { 0x03, Key.NumPad3},
                    { 0x04, Key.NumPad4},
                    { 0x05, Key.NumPad5},
                    { 0x06, Key.NumPad6},
                    { 0x07, Key.NumPad7},
                    { 0x08, Key.NumPad8},
                    { 0x09, Key.NumPad9},
                    { 0x0A, Key.A},
                    { 0x0B, Key.B},
                    { 0x0C, Key.C},
                    { 0x0D, Key.D},
                    { 0x0E, Key.E},
                    { 0x0F, Key.F},
                };

            _framework = framework;
            _autoResetEvent = new AutoResetEvent(false);

            Keyboard.AddKeyDownHandler(_framework, Handler);
        }

        private void Handler(object sender, KeyEventArgs keyEventArgs)
        {
            lock (this)
            {
                if (_keys.ContainsValue(keyEventArgs.Key))
                {
                    _key = keyEventArgs.Key;
                    _autoResetEvent.Set();    
                }
            }
            
        }

        private readonly Dictionary<byte, Key> _keys;
        private readonly FrameworkElement _framework;
        private readonly AutoResetEvent _autoResetEvent;
        private Key _key;

        public bool IsKeyDown(byte key)
        {
            var keyValue = _keys[key];

            bool isKeyDown = false;
            _framework.Dispatcher.Invoke(() => isKeyDown = Keyboard.IsKeyDown(keyValue));

            return isKeyDown;
        }

        public byte WaitForKeyPress()
        {
            _autoResetEvent.Reset();
            _autoResetEvent.WaitOne();
            lock (this)
            {
                return _keys.Single(x => x.Value == _key).Key;
            }
        }
    }
}