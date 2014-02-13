﻿using System.Collections.Generic;
using System.Linq;
using GlitchGame.Entities;
using GlitchGame.Gui;
using GlitchGame.Gui.Widgets;
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
            public readonly byte Color;

            public Line(int address, string instruction, byte color)
            {
                Address = address;
                Instruction = instruction;
                Color = color;
            }
        }

        private readonly Scrollbar _scrollbar;
        private readonly List<Line> _lines;
        private bool _needSetup;

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
            renderer.DrawBox(0, 0, Width, Height, GuiSettings.SolidBox, new Character(0, 0, 7));

            for (var i = 0; i < _lines.Count; i++)
            {
                var line = _lines[i];
                renderer.DrawText(0, i, line.Address.ToString("X8"), new Character(0, 0, 7));
                renderer.DrawText(9, i, line.Instruction, new Character(0, line.Color, 7));
            }

            _scrollbar.Draw(renderer.Region(_scrollbar.Left, _scrollbar.Top, _scrollbar.Width, _scrollbar.Height));
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            _scrollbar.MousePressed(x - _scrollbar.Left, y - _scrollbar.Top, button, pressed);
            return true;
        }

        public override void MouseMoved(int x, int y)
        {
            _scrollbar.MouseMoved(x - _scrollbar.Left, y - _scrollbar.Top);
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
            _lines.AddRange(Disassemble(target).SkipWhile(l => l.Address < offset).Take((int)Height));

            target.Vm.IP = originalIp;
        }

        private static IEnumerable<Line> Disassemble(Computer target)
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

                ShipDebugInfo.Line? nextLine = null;

                if (debugInfo != null)
                {
                    var symbol = debugInfo.FindSymbol(instrAddr);
                    if (symbol.HasValue && symbol.Value.Address == instrAddr)
                        yield return new Line(instrAddr, string.Format("< {0} >", symbol.Value.Name), 1);

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
                                yield return new Line(instrAddr - read, string.Format(" db {0}", bufferStr), 8);
                            }

                            read = 0;
                        }
                    }
                }
                else
                {
                    yield return new Line(instrAddr, string.Format(" {0}", instruction), 0);
                    machine.IP += instruction.Length;
                }
            }
        }
    }
}