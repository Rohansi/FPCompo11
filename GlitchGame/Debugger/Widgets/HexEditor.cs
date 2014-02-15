using System;
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

        private byte[] _buffer;
        private int _offset; // in bytes
        private int _selectedOffset; // in nibbles

        private int _frames;
        private bool _caretVisible;

        public byte[] Buffer
        {
            get { return _buffer; }
            set
            {
                _buffer = value;

                if (_buffer != null)
                    _scrollbar.Maximum = (int)(_buffer.Length / _rowSize - Height);
                else
                    _scrollbar.Maximum = 1;
            }
        }

        public int SelectedOffset
        {
            get { return _selectedOffset; }
            set
            {
                if (_buffer == null)
                {
                    _selectedOffset = 0;
                    return;
                }
                
                _selectedOffset = Math.Max(Math.Min(value, _buffer.Length * 2 - 1), 0);

                var offsetRowMin = _offset / _rowSize;
                var offsetRowMax = offsetRowMin + (int)Height;
                var selectedRow = _selectedOffset / 2 / _rowSize;

                if (selectedRow > offsetRowMin && selectedRow < offsetRowMax)
                    selectedRow = offsetRowMin;
                else if (selectedRow >= offsetRowMax)
                    selectedRow -= (int)Height - 1;

                _scrollbar.Value = selectedRow;

                if (SelectionChanged != null)
                    SelectionChanged();
            }
        }

        public event Action SelectionChanged;

        public HexEditor(int x, int y, uint w, uint h, int rowSize)
        {
            Left = x;
            Top = y;
            Width = w;
            Height = h;

            _scrollbar = new Scrollbar((int)Width - 1, 0, Height);
            _scrollbar.Minimum = 0;
            _scrollbar.Maximum = 1;
            _scrollbar.Step = 1;

            _rowSize = rowSize;
        }

        public override void Draw(ITextRenderer renderer)
        {
            _offset = (int)(Math.Round(_scrollbar.Value) * _rowSize);

            renderer.Clear(new Character(0, 0, 7), true);

            if (_buffer != null)
            {
                var offset = _offset;

                for (var i = 0; i < Height; i++)
                {
                    renderer.DrawText(0, i, offset.ToString("X8"), new Character(0, 8, 7));
                    var x1 = 10;
                    var x2 = x1 + (_rowSize * 3) + 1;
                    for (var j = 0; j < _rowSize; j++, offset++, x1 += 3, x2++)
                    {
                        if (offset < 0 || offset > _buffer.Length)
                            continue;

                        var b = _buffer[offset];
                        renderer.DrawText(x1, i, b.ToString("X2"), new Character(0, 0, 7));
                        renderer.Set(x2, i, new Character(b, 0, 7));
                    }
                }
            }

            _frames++;
            if (_frames >= 30)
            {
                _caretVisible = !_caretVisible;
                _frames = 0;
            }

            if (Focussed && _caretVisible)
            {
                var offset = _selectedOffset - (_offset * 2);
                var y = (offset / 2) / _rowSize;
                var xN = offset % (_rowSize * 2);
                var x = 10 + xN + (xN / 2);
                renderer.Set(x, y, new Character(-1, 7, 0));
            }

            _scrollbar.Draw(renderer.Region(_scrollbar.Left, _scrollbar.Top, _scrollbar.Width, _scrollbar.Height));
        }

        public override bool MousePressed(int x, int y, Mouse.Button button, bool pressed)
        {
            var xOverHex = x >= 10 && x <= 10 + (_rowSize * 3) - 2;

            if (pressed && xOverHex)
            {
                x -= 10;

                if (x % 3 == 2)
                    return true;

                var yB = _offset + y * _rowSize;
                var yN = yB * 2;
                var xB = x / 3;
                var xN = xB * 2 + (x % 3);

                SelectedOffset = yN + xN;
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

        public override bool KeyPressed(Keyboard.Key key, string text)
        {
            var originalCaret = _caretVisible;

            _caretVisible = true;
            _frames = 0;

            byte? value = null;

            switch (key)
            {
                case Keyboard.Key.Left:
                    SelectedOffset--;
                    break;
                case Keyboard.Key.Right:
                    SelectedOffset++;
                    break;
                case Keyboard.Key.Up:
                    SelectedOffset -= _rowSize * 2;
                    break;
                case Keyboard.Key.Down:
                    SelectedOffset += _rowSize * 2;
                    break;
                case Keyboard.Key.Back:
                    SelectedOffset -= 2;
                    break;

                #region Hexadecimal Keys
                case Keyboard.Key.Numpad0:
                case Keyboard.Key.Num0:
                    value = 0x0;
                    break;
                case Keyboard.Key.Numpad1:
                case Keyboard.Key.Num1:
                    value = 0x1;
                    break;
                case Keyboard.Key.Numpad2:
                case Keyboard.Key.Num2:
                    value = 0x2;
                    break;
                case Keyboard.Key.Numpad3:
                case Keyboard.Key.Num3:
                    value = 0x3;
                    break;
                case Keyboard.Key.Numpad4:
                case Keyboard.Key.Num4:
                    value = 0x4;
                    break;
                case Keyboard.Key.Numpad5:
                case Keyboard.Key.Num5:
                    value = 0x5;
                    break;
                case Keyboard.Key.Numpad6:
                case Keyboard.Key.Num6:
                    value = 0x6;
                    break;
                case Keyboard.Key.Numpad7:
                case Keyboard.Key.Num7:
                    value = 0x7;
                    break;
                case Keyboard.Key.Numpad8:
                case Keyboard.Key.Num8:
                    value = 0x8;
                    break;
                case Keyboard.Key.Numpad9:
                case Keyboard.Key.Num9:
                    value = 0x9;
                    break;
                case Keyboard.Key.A:
                    value = 0xA;
                    break;
                case Keyboard.Key.B:
                    value = 0xB;
                    break;
                case Keyboard.Key.C:
                    value = 0xC;
                    break;
                case Keyboard.Key.D:
                    value = 0xD;
                    break;
                case Keyboard.Key.E:
                    value = 0xE;
                    break;
                case Keyboard.Key.F:
                    value = 0xF;
                    break;
                #endregion

                default:
                    _caretVisible = originalCaret;
                    break;
            }

            if (value.HasValue)
            {
                var selectedByte = _selectedOffset / 2;
                var selectedNibble = _selectedOffset % 2;
                var mask = selectedNibble == 0 ? 0x0F : 0xF0;
                var shift = selectedNibble == 0 ? 4 : 0;

                _buffer[selectedByte] = (byte)((_buffer[selectedByte] & mask) | (value.Value << shift));

                _selectedOffset = Math.Min(_selectedOffset + 1, _buffer.Length * 2);
            }

            return true;
        }
    }
}
