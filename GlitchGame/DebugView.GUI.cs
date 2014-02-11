using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlitchGame.GUI;
using GlitchGame.GUI.Widgets;
using LoonyVM;

namespace GlitchGame
{
    public partial class DebugView
    {
        private Window _cpuWindow;
        private Label[] _cpuRegisters;
        private Label[] _cpuFlags;
        private Label _cpuDisassembly;
        private Scrollbar _cpuScrollbar;
        private int _disassembleOffset;

        private void Initialize()
        {
            _gui = new GuiSystem(1000, 250);

            var desktop = new Container(1000, 250);
            desktop.Top = 1;
            _gui.Add(desktop);

            #region Menu
            var menu = new MenuBar();

            var title = new MenuItem("[Debugger]");
            menu.Items.Add(title);

            var view = new MenuItem("View");

            var cpu = new MenuItem("CPU");

            cpu.Clicked += () =>
            {
                _cpuWindow.Focus();
                _cpuWindow.Visible = true;
            };

            view.Items.Add(cpu);

            var stack = new MenuItem("Stack");
            view.Items.Add(stack);

            var mem = new MenuItem("Memory");
            view.Items.Add(mem);

            var sym = new MenuItem("Symbols");
            view.Items.Add(sym);

            menu.Items.Add(view);

            _gui.Add(menu);
            #endregion

            #region CPU Window
            _cpuWindow = new Window(10, 10, 100, 40, "CPU");

            _cpuDisassembly = new Label(1, 1, 68, 36, "");
            _cpuWindow.Add(_cpuDisassembly);

            _cpuRegisters = new Label[RegisterNames.Length];
            for (var i = 0; i < _cpuRegisters.Length; i++)
            {
                _cpuRegisters[i] = new Label(72, 1 + i, 25, 1, "");
                _cpuWindow.Add(_cpuRegisters[i]);
            }

            _cpuFlags = new Label[FlagNames.Length];
            for (var i = 0; i < _cpuFlags.Length; i++)
            {
                _cpuFlags[i] = new Label(72, 2 + _cpuRegisters.Length + i, 25, 1, "");
                _cpuWindow.Add(_cpuFlags[i]);
            }

            _cpuScrollbar = new Scrollbar(69, 1, 36);
            _cpuScrollbar.Minimum = 0;
            _cpuScrollbar.Maximum = 1;
            _cpuScrollbar.Step = 0.01f;

            _cpuWindow.Add(_cpuScrollbar);

            desktop.Add(_cpuWindow);
            #endregion

            _disassembleOffset = 0;
        }

        public void Update()
        {
            if (_target == null)
                return;

            var debugInfo = _target.Code.DebugInfo;
            debugInfo = null;

            for (var i = 0; i < _cpuRegisters.Length; i++)
            {
                _cpuRegisters[i].Caption = string.Format("{0} = {1:X8} {1,11}", RegisterNames[i], _target.Vm.Registers[i]);
            }

            for (var i = 0; i < _cpuFlags.Length; i++)
            {
                var set = (_target.Vm.Flags & FlagValues[i]) != 0;
                _cpuFlags[i].Caption = string.Format("{0} = {1}", FlagNames[i], set);
            }

            var originalIp = _target.Vm.IP;
            _target.Vm.IP = (int)(_cpuScrollbar.Value * _target.Vm.Memory.Length);
            var instruction = new Instruction(_target.Vm);
            var disassembly = new StringBuilder();
            var buffer = new byte[8];

            for (var i = 0; i < 36; i++)
            {
                var instrAddr = _target.Vm.IP;
                if (instrAddr >= _target.Vm.Memory.Length)
                    break;

                ShipDebugInfo.Line? nextLine = null;

                if (debugInfo != null)
                {
                    var symbols = debugInfo.Symbols;
                    var lines = debugInfo.Lines;

                    for (var j = 0; j < symbols.Count; j++)
                    {
                        if (symbols[j].Address == instrAddr)
                        {
                            disassembly.AppendFormat("{0:X8} < {1} >\n", instrAddr, symbols[j].Name);
                            break;
                        }
                    }

                    for (var j = 0; j < lines.Count; j++)
                    {
                        if (lines[j].Address > instrAddr)
                        {
                            nextLine = lines[j];
                            break;
                        }
                    }
                }

                var decodeFailed = false;
                try
                {
                    instruction.Decode();
                }
                catch
                {
                    decodeFailed = true;
                }

                if (decodeFailed || !instruction.IsValid || (nextLine.HasValue && instrAddr + instruction.Length > nextLine.Value.Address))
                {
                    if (nextLine.HasValue)
                        _target.Vm.IP = nextLine.Value.Address;
                    else
                        _target.Vm.IP += 4;

                    var read = 0;
                    while (instrAddr <= _target.Vm.IP)
                    {
                        if (instrAddr >= _target.Vm.Memory.Length)
                            break;

                        buffer[read++] = _target.Vm.Memory[instrAddr++];

                        if (read == buffer.Length || instrAddr == _target.Vm.IP)
                        {
                            if (read > 0)
                                disassembly.AppendFormat("{0:X8}  db {1}\n", instrAddr - read, string.Join(", ", buffer.Take(read).Select(v => string.Format("0x{0:X2}", v))));
                            read = 0;
                        }
                    }
                }
                else
                {
                    disassembly.AppendFormat("{0:X8}  {1}\n", instrAddr, instruction);
                    _target.Vm.IP += instruction.Length;
                }
            }

            _cpuDisassembly.Caption = disassembly.ToString();
            _target.Vm.IP = originalIp;
        }

        private void ScrollDown()
        {
            if (_disassembleOffset >= _target.Vm.Memory.Length)
                return;

            var originalIp = _target.Vm.IP;
            _target.Vm.IP = _disassembleOffset;

            var instruction = new Instruction(_target.Vm);
            instruction.Decode();

            if (instruction.IsValid)
                _target.Vm.IP += instruction.Length;
            else
                _target.Vm.IP += 4;

            _disassembleOffset = _target.Vm.IP;
            _target.Vm.IP = originalIp;
        }

        private void ScrollUp()
        {
            if (_disassembleOffset <= 0)
                return;

            var originalIp = _target.Vm.IP;
            _target.Vm.IP = _disassembleOffset;

            var instr = new Instruction(_target.Vm);
            var foundValid = false;

            for (var i = 4; i <= 12; i++)
            {
                _target.Vm.IP = _disassembleOffset - i;

                var decodeFailed = false;
                try
                {
                    instr.Decode();
                }
                catch
                {
                    decodeFailed = true;
                }

                if (!decodeFailed && instr.IsValid && instr.Length == i)
                {
                    foundValid = true;
                    _disassembleOffset -= i;
                    break;
                }
            }

            if (!foundValid)
                _disassembleOffset -= 4;

            _disassembleOffset = Math.Max(_disassembleOffset, 0);
            _target.Vm.IP = originalIp;
        }

        private static readonly string[] RegisterNames =
        {
            "R0", "R1", "R2", "R3", "R4", "R5", "R6", "R7", "R8", "R9",
            "BP", "SP", "IP"
        };

        private static readonly string[] FlagNames =
        {
            "Z ", "E ", "A ", "B "
        };

        private static readonly VmFlags[] FlagValues =
        {
            VmFlags.Zero, VmFlags.Equal, VmFlags.Above, VmFlags.Below
        };
    }
}
