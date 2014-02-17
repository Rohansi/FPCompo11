using System;
using System.Collections.Generic;
using System.Linq;
using GlitchGame.Entities;
using LoonyVM;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger.Widgets
{
    public class Disassembly : Widget
    {
        private struct Line
        {
            public readonly int Address;
            public readonly string Instruction;
            public readonly bool Clickable;
            public readonly byte Foreground;
            public readonly byte Background;

            public Line(int address, string instruction, bool clickable, byte fore, byte back = 7)
            {
                Address = address;
                Instruction = instruction;
                Clickable = clickable;
                Foreground = fore;
                Background = back;
            }
        }

        private readonly Scrollbar _scrollbar;
        private readonly List<Line> _lines;
        private bool _needSetup;

        public event Action<int, Mouse.Button> Clicked;

        public int Offset
        {
            get { return (int)Math.Round(_scrollbar.Value); }
            set { _scrollbar.Value = Util.Clamp(value, _scrollbar.Minimum, _scrollbar.Maximum); }
        }

        public Disassembly(int x, int y, uint w, uint h)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;

            _scrollbar = new Scrollbar((int)Width - 1, 0, Height);
            _scrollbar.Minimum = 0;
            _scrollbar.Maximum = 100;
            _scrollbar.Step = 6;

            _lines = new List<Line>();
            _needSetup = true;
        }

        public override void Draw(ITextRenderer renderer)
        {
            renderer.Clear(new Character(0, 0, 7), true);

            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                renderer.DrawBox(0, i, Width - 1, 1, GuiSettings.SolidBox, new Character(0, 0, line.Background));
                renderer.DrawText(0, i, line.Address.ToString("X8"), new Character(0, 8));
                renderer.DrawText(9, i, line.Instruction, new Character(0, line.Foreground));
            }

            _scrollbar.Draw(renderer.Region(_scrollbar.Left, _scrollbar.Top, _scrollbar.Width, _scrollbar.Height));
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (pressed && x >= 0 && x <= Width - 2 && y >= 0 && y < _lines.Count)
            {
                var line = _lines[y];
                if (line.Clickable && Clicked != null)
                    Clicked(line.Address, button);
            }

            _scrollbar.MousePressed(x - _scrollbar.Left, y - _scrollbar.Top, button, pressed);
            return true;
        }

        public override void MouseMoved(int x, int y)
        {
            _scrollbar.MouseMoved(x - _scrollbar.Left, y - _scrollbar.Top);
        }

        public override bool MouseWheelMoved(int x, int y, int delta)
        {
            _scrollbar.MouseWheelMoved(x, y, delta);
            return true;
        }

        public void Reset()
        {
            _needSetup = true;
            _scrollbar.Value = _scrollbar.Minimum;
            _lines.Clear();
        }

        public void Update(Computer target)
        {
            if (_needSetup)
            {
                _scrollbar.Maximum = target.Vm.Memory.Length;
                _needSetup = false;
            }

            var debugInfo = target.Code.DebugInfo;
            var offset = (int)_scrollbar.Value;
            var originalIp = target.Vm.IP;
            target.Vm.IP = offset;

            if (debugInfo != null)
            {
                var prevLine = debugInfo.FindLine(offset, -1);
                if (prevLine.HasValue)
                    target.Vm.IP = prevLine.Value.Address; // TODO: limit how far back this can go
            }

            _lines.Clear();
            _lines.AddRange(Disassemble(target, originalIp).SkipWhile(l => l.Address < offset).Take((int)Height));

            target.Vm.IP = originalIp;
        }

        private IEnumerable<Line> Disassemble(Computer target, int ip)
        {
            var machine = target.Vm;
            var debugInfo = target.Code.DebugInfo;
            var instruction = new Instruction(machine);
            var buffer = new byte[8];

            while (true)
            {
                var instrAddr = machine.IP;
                if (instrAddr >= machine.Memory.Length)
                    break;

                ShipDebug.Line? nextLine = null;

                if (debugInfo != null)
                {
                    var symbol = debugInfo.FindSymbol(instrAddr);
                    if (symbol.HasValue && symbol.Value.Address == instrAddr)
                        yield return new Line(instrAddr, string.Format("< {0} >", symbol.Value.Name), false, 1);

                    nextLine = debugInfo.FindLine(instrAddr, 1);
                    if (nextLine.HasValue && nextLine.Value.Address == instrAddr)
                        nextLine = null;
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
                        machine.IP = nextLine.Value.Address;
                    else
                        machine.IP += 4;

                    var read = 0;
                    while (instrAddr <= machine.IP)
                    {
                        if (instrAddr >= machine.Memory.Length)
                            break;

                        buffer[read++] = machine.Memory[instrAddr++];

                        if (read == buffer.Length || instrAddr == machine.IP)
                        {
                            if (read > 0)
                            {
                                var bufferStr = string.Join(", ", buffer.Take(read).Select(v => string.Format("0x{0:X2}", v)));
                                yield return new Line(instrAddr - read, string.Format(" db {0}", bufferStr), false, 8);
                            }

                            read = 0;
                        }
                    }
                }
                else
                {
                    var fg = instrAddr == ip ? 4 : 0;
                    var bg = target.HasBreakpoint(instrAddr) ? 15 : 7;
                    yield return new Line(instrAddr, string.Format(" {0}", instruction), true, (byte)fg, (byte)bg);
                    machine.IP += instruction.Length;
                }
            }
        }
    }
}
