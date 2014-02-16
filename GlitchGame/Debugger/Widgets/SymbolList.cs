using System;
using System.Collections.Generic;
using GlitchGame.Gui;
using GlitchGame.Gui.Widgets;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger.Widgets
{
    public class SymbolList : Widget
    {
        public readonly List<ShipDebugInfo.Symbol> Symbols;
        public event Action<ShipDebugInfo.Symbol, Mouse.Button> Clicked;

        private Scrollbar _scrollbar;
        private int _selected;

        public SymbolList(int x, int y, uint w, uint h)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;

            Symbols = new List<ShipDebugInfo.Symbol>();

            _scrollbar = new Scrollbar((int)Width - 1, 0, Height);
            _scrollbar.Minimum = 0;

            _selected = -1;
        }

        public override void Draw(ITextRenderer renderer)
        {
            renderer.Clear(GuiSettings.ListBox, true);

            _scrollbar.Maximum = Math.Max(Symbols.Count - Height, 0);
            var startIndex = (int)_scrollbar.Value;

            for (var i = 0; i < Height; i++)
            {
                var index = startIndex + i;
                if (index >= Symbols.Count)
                    break;

                var s = Symbols[index];
                var reg = renderer.Region(0, i, Width - 1, 1);
                var col = (_selected == index) ? GuiSettings.ListBoxItemSelected : GuiSettings.ListBoxItem;

                reg.Clear(col, true);
                reg.DrawText(0, 0, s.Name, col);
                reg.DrawText((int)Width - 11, 0, string.Format(" {0:X8} ", s.Address), col);
            }

            _scrollbar.Draw(renderer.Region(_scrollbar.Left, _scrollbar.Top, _scrollbar.Width, _scrollbar.Height));
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            if (pressed && (x >= 0 && y >= 0 && x < Width - 1 && y < Height))
            {
                var index = (int)_scrollbar.Value + y;

                if (index < Symbols.Count)
                {
                    _selected = index;

                    if (Clicked != null)
                        Clicked(Symbols[index], button);
                }

                return true;
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
    }
}
