using System;
using System.Linq;
using System.Text;
using GlitchGame.Gui;
using GlitchGame.Gui.Widgets;
using LoonyVM;

namespace GlitchGame.Debugger.Windows
{
    public class Cpu : DebugWindow
    {
        private Window _window;
        private Label[] _registers;
        private Label[] _flags;
        private Label _disassembly;
        private Scrollbar _scrollbar;

        private bool _needSetup;
        private int _disassembleOffset;

        public Cpu(Container parent)
        {
            #region Widget Creation
            _window = new Window(10, 10, 100, 40, "CPU");

            _disassembly = new Label(1, 1, 68, 36, "");
            _window.Add(_disassembly);

            _registers = new Label[RegisterNames.Length];
            for (var i = 0; i < _registers.Length; i++)
            {
                _registers[i] = new Label(72, 1 + i, 25, 1, "");
                _window.Add(_registers[i]);
            }

            _flags = new Label[FlagNames.Length];
            for (var i = 0; i < _flags.Length; i++)
            {
                _flags[i] = new Label(72, 2 + _registers.Length + i, 25, 1, "");
                _window.Add(_flags[i]);
            }

            _scrollbar = new Scrollbar(69, 1, 36);
            _scrollbar.Maximum = 1;
            _scrollbar.Step = 4;
            _scrollbar.Value = 0;

            _scrollbar.Changed += () =>
            {
                var currentValue = _disassembleOffset;
                var newValue = _scrollbar.Value;

                var diff = (int)Math.Ceiling((newValue - currentValue) / 4f);

                while (diff > 0)
                {
                    ScrollDown();
                    diff--;
                }

                while (diff < 0)
                {
                    ScrollUp();
                    diff++;
                }
            };

            _window.Add(_scrollbar);

            var test = new TextBox(72, 36, 16);
            _window.Add(test);

            var test2 = new Button(90, 36, 7, "Goto");
            _window.Add(test2);

            parent.Add(_window);
            #endregion

            _needSetup = true;
        }

        public override void Reset()
        {
            _needSetup = true;
            _disassembleOffset = 0;

            _scrollbar.Value = 0;
        }

        public override void Update()
        {
            if (Target == null || !_window.Visible)
                return;

            if (_needSetup)
            {
                _scrollbar.Maximum = Target.Vm.Memory.Length;
                _needSetup = false;
            }

            var debugInfo = Target.Code.DebugInfo;

            for (var i = 0; i < _registers.Length; i++)
            {
                _registers[i].Caption = string.Format("{0} = {1:X8} {1,11}", RegisterNames[i], Target.Vm.Registers[i]);
            }

            for (var i = 0; i < _flags.Length; i++)
            {
                var set = (Target.Vm.Flags & FlagValues[i]) != 0;
                _flags[i].Caption = string.Format("{0} = {1}", FlagNames[i], set);
            }

            var originalIp = Target.Vm.IP;
            Target.Vm.IP = _disassembleOffset;//(_scrollbar.Value * Target.Vm.Memory.Length);
            var instruction = new Instruction(Target.Vm);
            var disassembly = new StringBuilder();
            var buffer = new byte[8];

            for (var i = 0; i < 36; i++)
            {
                var instrAddr = Target.Vm.IP;
                if (instrAddr >= Target.Vm.Memory.Length)
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
                        Target.Vm.IP = nextLine.Value.Address;
                    else
                        Target.Vm.IP += 4;

                    var read = 0;
                    while (instrAddr <= Target.Vm.IP)
                    {
                        if (instrAddr >= Target.Vm.Memory.Length)
                            break;

                        buffer[read++] = Target.Vm.Memory[instrAddr++];

                        if (read == buffer.Length || instrAddr == Target.Vm.IP)
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
                    Target.Vm.IP += instruction.Length;
                }
            }

            _disassembly.Caption = disassembly.ToString();
            Target.Vm.IP = originalIp;
        }

        public override void Show()
        {
            _window.Visible = true;
            _window.Focus();
        }

        public override void Hide()
        {
            _window.Visible = false;
        }

        private void ScrollDown()
        {
            if (_disassembleOffset >= Target.Vm.Memory.Length)
                return;

            var originalIp = Target.Vm.IP;
            Target.Vm.IP = _disassembleOffset;

            var instruction = new Instruction(Target.Vm);

            var decodeFailed = false;
            try
            {
                instruction.Decode();
            }
            catch
            {
                decodeFailed = true;
            }

            if (!decodeFailed && instruction.IsValid)
                Target.Vm.IP += instruction.Length;
            else
                Target.Vm.IP += 4;

            _disassembleOffset = Target.Vm.IP;
            Target.Vm.IP = originalIp;
        }

        private void ScrollUp()
        {
            if (_disassembleOffset <= 0)
                return;

            var originalIp = Target.Vm.IP;
            Target.Vm.IP = _disassembleOffset;

            var instr = new Instruction(Target.Vm);
            var foundValid = false;

            for (var i = 12; i <= 4; i--)
            {
                Target.Vm.IP = _disassembleOffset - i;

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
            Target.Vm.IP = originalIp;
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
