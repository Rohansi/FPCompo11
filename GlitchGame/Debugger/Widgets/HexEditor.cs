using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GlitchGame.Gui;
using GlitchGame.Gui.Widgets;
using SFML.Window;
using Texter;

namespace GlitchGame.Debugger.Widgets
{
    public class HexEditor : Widget
    {
        private readonly Scrollbar _scrollbar;
        private readonly int _rowSize;
        private bool _needSetup;

        private byte[] _buffer;
        private int _offset;

        public HexEditor(int x, int y, uint w, uint h, int rowSize)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;

            _scrollbar = new Scrollbar((int)Width - 1, 0, Height);
            _scrollbar.Minimum = 0;
            _scrollbar.Maximum = 1;
            _scrollbar.Step = _rowSize;

            _rowSize = rowSize;
            _needSetup = true;
        }

        public override void Draw(ITextRenderer renderer)
        {
            renderer.DrawBox(0, 0, Width, Height, GuiSettings.SolidBox, new Character(0, 0, 7));

            if (!_needSetup)
            {
                var offset = _offset;

                for (var i = 0; i < Height; i++)
                {
                    renderer.DrawText(0, i, offset.ToString("X8"), new Character(0, 8, 7));

                    // TODO: draw hex stuff

                    offset += _rowSize;
                }
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
            _buffer = null;
            _offset = 0;

            _scrollbar.Value = 0;
        }

        public void Update(byte[] buffer)
        {
            if (_needSetup)
            {
                _buffer = buffer;
                _scrollbar.Maximum = (int)(buffer.Length / _rowSize - Height);
                _needSetup = false;
            }

            _offset = (int)(_scrollbar.Value * _rowSize);
        }
    }
}
